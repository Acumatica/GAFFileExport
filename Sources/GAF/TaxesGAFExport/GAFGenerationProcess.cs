using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.CreatorsForDocs.Builders.Purchase;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.CADocuments;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using TaxesGAFExport.Data;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport
{
	public class GAFGenerationProcess : PXGraph<GAFGenerationProcess>
	{
		public PXCancel<GAFPeriod> cancel; 

		public PXAction<GAFPeriod> generate;
		public PXAction<GAFPeriod> viewTaxSummary;
		public PXAction<GAFPeriod> viewTaxDetails;

		public PXSelect<GAFPeriod> GAFPeriodView;

		private GAFRepository _gafRepository;

		private GAFValidator _gafValidator;

		public GAFGenerationProcess()
		{
			_gafRepository = new GAFRepository(this);
			_gafValidator = new GAFValidator(_gafRepository);
		}

		protected virtual void GAFPeriod_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			List<int> branches = PXAccess.GetMasterBranchID(Accessinfo.BranchID);
			e.NewValue = branches != null ? (int?)branches[0] : null;
			e.Cancel = branches != null;
		}

		protected virtual void GAFPeriod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var gafPeriod = (GAFPeriod) e.Row;

			if (gafPeriod == null)
				return;

			if (gafPeriod.TaxPeriodID != null)
			{
				Branch branch = BranchMaint.FindBranchByID(this, gafPeriod.BranchID);

				var taxPeriod = TaxYearMaint.FindTaxPeriodByKey(this, branch.ParentBranchID, gafPeriod.TaxAgencyID,
					gafPeriod.TaxPeriodID);

				if (taxPeriod == null)
					return;

				gafPeriod.StartDate = taxPeriod.StartDate;
				gafPeriod.EndDateUI = taxPeriod.EndDateUI;
			}
			else
			{
				gafPeriod.StartDate = null;
				gafPeriod.EndDateUI = null;
			}
		}

		[PXUIField(DisplayName = "Generate", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable Generate(PXAdapter adapter)
		{
			var gafPeriod = GAFPeriodView.Current;

			Branch branch = BranchMaint.FindBranchByID(this, gafPeriod.BranchID);

			var taxPeriod = TaxYearMaint.FindTaxPeriodByKey(this, branch.ParentBranchID, gafPeriod.TaxAgencyID, gafPeriod.TaxPeriodID);

			if (taxPeriod == null)
				return adapter.Get();

			var chekingResult = _gafValidator.CheckGAFGenerationRequirements(gafPeriod.BranchID, taxPeriod);

			if (!chekingResult.IsSuccess)
			{
				throw new PXException(chekingResult.GetGeneralMessage());
			}

			if (chekingResult.HasWarning)
			{
				if (GAFPeriodView.Ask(chekingResult.GetGeneralMessage(), MessageButtons.OKCancel) != WebDialogResult.OK)
					return adapter.Get();
			}

			PXLongOperation.StartOperation(this, () => GenerateProc(gafPeriod));

			return adapter.Get();
		}

		private static void GenerateProc(GAFPeriod gafPeriod)
		{
			var gafGenerationProcess = PXGraph.CreateInstance<GAFGenerationProcess>();

			IGAFRepository gafRepository = new GAFRepository(gafGenerationProcess);
			var recordBuilderByVendorData = new PurchaseRecordBuilderByVendorData(gafRepository);
			var recordBuilderByCustomerData = new SupplyRecordBuilderByCustomerData(gafRepository);
			var recordBuilderByRegister = new GafRecordBuilderByRegister(gafRepository);
			var recordCountryBuilderForSO = new SupplyRecordCountryBuilderForSOInvoice(gafRepository);
			var recordCountryBuilderForAR = new SupplyRecordCountryBuilderForARInvoice(gafRepository);


			var apInvoiceGAFRecordsCreator = new APInvoiceGAFRecordsCreator(gafRepository,
				new PurchaseRecordBuilderByInvoiceTran(gafRepository, recordBuilderByVendorData, recordBuilderByRegister),
				new PurchaseRecordBuilderByTaxTranFromTaxDocument(gafRepository, recordBuilderByVendorData, recordBuilderByRegister),
				new PurchaseRecordBuilderByAPInvoiceTaxTranForTaxCalcedOnDocumentAmt(gafRepository, recordBuilderByRegister, recordBuilderByVendorData));

			var arInvoiceGAFRecordsCreator = new ARInvoiceGAFRecordsCreator(gafRepository,
																			new SupplyRecordBuilderByARInvoice(gafRepository, 
																												recordBuilderByRegister, 
																												recordBuilderByCustomerData,
																												recordCountryBuilderForAR),
																			new SupplyRecordBuilderByARInvoiceTaxTranForTaxCalcedOnDocumentAmt(gafRepository,
																																	recordBuilderByRegister,
																																	recordBuilderByCustomerData,
																																	recordCountryBuilderForAR));

			var arInvoiceFromSOGAFRecordsCreator = new ARInvoiceFromSOGAFRecordsCreator(gafRepository,
																						new SupplyRecordBuilderBySOInvoice(gafRepository, 
																															recordBuilderByRegister, 
																															recordBuilderByCustomerData, 
																															recordCountryBuilderForSO),
																						new SupplyRecordBuilderBySOInvoiceTaxTranForTaxCalcedOnDocumentAmt(gafRepository,
																																	recordBuilderByRegister,
																																	recordBuilderByCustomerData,
																																	recordCountryBuilderForSO));

			var apPaymentGAFRecordsCreator = new APPaymentGAFRecordsCreator(gafRepository,
				new PurchaseRecordBuilderByTaxTranOfAPPayment(gafRepository, recordBuilderByVendorData, recordBuilderByRegister));

			var caDocumentPurchaseGAFRecordsCreator = new CADocumentPurchaseGAFRecordsCreator(gafRepository,
				new PurchaseRecordBuilderByCADocument(gafRepository, recordBuilderByRegister),
				new PurchaseRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt(gafRepository, recordBuilderByRegister));

			var caDocumentSupplyGAFRecordsCreator = new CADocumentSupplyGAFRecordsCreator(gafRepository,
				new SupplyRecordBuilderByCADocument(gafRepository, recordBuilderByRegister),
				new SupplyRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt(gafRepository, recordBuilderByRegister));

			var taxAdjustmentGafRecordsCreator = new TaxAdjustmentGAFRecordsCreator(gafRepository,
				new GafRecordBuilderByTaxAdjustmentTaxTran(gafRepository));

			var glDocumentGAFRecordsCreator = new GLDocumentGAFRecordsCreator(gafRepository,
				new GafRecordBuilderByGLTranAndTaxTran(gafRepository));

			var gafCreationHelper = new GAFValidator(gafRepository);

			var gafDataCreator = new GAFDataCreator(gafRepository,
				gafCreationHelper,
				new GLGAFLedgerRecordsCreator(gafRepository),
				apInvoiceGAFRecordsCreator,
				arInvoiceGAFRecordsCreator,
				arInvoiceFromSOGAFRecordsCreator,
				apPaymentGAFRecordsCreator,
				caDocumentPurchaseGAFRecordsCreator,
				caDocumentSupplyGAFRecordsCreator,
				taxAdjustmentGafRecordsCreator,
				glDocumentGAFRecordsCreator,
				new GafRecordWriter(gafRepository));



			var gafPeriodFromDB = gafRepository.FindGAFPeriodByKey(gafPeriod.BranchID, gafPeriod.TaxAgencyID,
				gafPeriod.TaxPeriodID);

			gafPeriod = gafPeriodFromDB ?? gafGenerationProcess.GAFPeriodView.Insert(gafPeriod);

			var gstAuditFile = gafDataCreator.Create(gafPeriod, gafGenerationProcess.Accessinfo.BusinessDate.Value);

			if (gstAuditFile == null)
				return;

			gafPeriod.GAFMajorVersion = gstAuditFile.MajorVersion;
			gafPeriod.GAFMinorLastVersion = gstAuditFile.MinorVersion;

			gafGenerationProcess.GAFPeriodView.Update(gafPeriod);

			using (var ts = new PXTransactionScope())
			{
				PX.Objects.Common.Tools.UploadFileHelper.AttachDataAsFile(gstAuditFile.FileName, gstAuditFile.Data, gafPeriod,
					gafGenerationProcess);

				gafGenerationProcess.Actions.PressSave();

				ts.Complete();
			}
		}

		[PXUIField(DisplayName = "View Tax Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTaxSummary(PXAdapter adapter)
		{
			if (GAFPeriodView.Current != null)
			{
				throw new PXReportRequiredException(CreateReportParams(), "TX621000", PX.Objects.TX.Messages.TaxSummary);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "View Tax Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTaxDetails(PXAdapter adapter)
		{
			if (GAFPeriodView.Current != null)
			{
				throw new PXReportRequiredException(CreateReportParams(), "TX620500", PX.Objects.TX.Messages.TaxDetails);
			}
			return adapter.Get();
		}

		private Dictionary<string, string> CreateReportParams()
		{
			var branch = _gafRepository.GetBranchByID(GAFPeriodView.Current.BranchID);
			var taxAgency = _gafRepository.GetVendorsByIDs(GAFPeriodView.Current.TaxAgencyID.SingleToArray()).Single();
			var taxPeriod = _gafRepository.GetTaxPeriodByKey(branch.ParentBranchID, taxAgency.BAccountID, GAFPeriodView.Current.TaxPeriodID);

			return new Dictionary<string, string>
			{
				["MasterBranchID"] = branch.BranchCD,
				["VendorID"] = taxAgency.AcctCD,
				["TaxPeriodID"] = FinPeriodIDAttribute.FormatForDisplay(taxPeriod.TaxPeriodID)
			};
		}
	}
}
