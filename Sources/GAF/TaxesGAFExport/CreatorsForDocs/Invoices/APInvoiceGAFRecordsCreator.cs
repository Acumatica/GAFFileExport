using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Purchase;
using TaxesGAFExport.Data;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Invoices
{
	public class APInvoiceGAFRecordsCreator : DocumentWithTransGAFRecordsCreatorBase<APInvoice, APTran, APTax, PurchaseRecord>
	{
		private readonly PurchaseRecordBuilderByInvoiceTran _purchaseRecordBuilderByInvoiceTran;
		private readonly PurchaseRecordBuilderByTaxTranFromTaxDocument _purchaseRrcordBuilderByTaxTranFromTaxDocument;
		private readonly PurchaseRecordBuilderByAPInvoiceTaxTranForTaxCalcedOnDocumentAmt _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;

		private Dictionary<string, Dictionary<string, APRegister>> _taxApRegistersByOrigRefNbrAndTax;
		protected Dictionary<int?, Vendor> Vendors;

		protected override GAFRecordType ExpectedGafRecordType
		{
			get { return GAFRecordType.Purchase; }
		}

		public APInvoiceGAFRecordsCreator(IGAFRepository gafRepository,
			PurchaseRecordBuilderByInvoiceTran purchaseRecordBuilderByInvoiceTran,
			PurchaseRecordBuilderByTaxTranFromTaxDocument purchaseRrcordBuilderByTaxTranFromTaxDocument,
			PurchaseRecordBuilderByAPInvoiceTaxTranForTaxCalcedOnDocumentAmt recordBuilderByTaxTranForTaxCalcedOnDocumentAmt)
			: base(gafRepository)
		{
			_purchaseRecordBuilderByInvoiceTran = purchaseRecordBuilderByInvoiceTran;
			_purchaseRrcordBuilderByTaxTranFromTaxDocument = purchaseRrcordBuilderByTaxTranFromTaxDocument;
			_recordBuilderByTaxTranForTaxCalcedOnDocumentAmt = recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;
		}

		protected override void LoadData(DocumentGroup<APInvoice> documentGroup, int? taxAgencyID, string taxPeriodID)
		{
			base.LoadData(documentGroup, taxAgencyID, taxPeriodID);

			Vendors = GetContragentsForDocuments(DocumentsByRefNbr.Values);

			LoadValuableTaxApRegistersForDocuments(documentGroup, taxAgencyID);
		}

		protected override PurchaseRecord BuildGafRecordByTaxTranCalcedOnDocumentAmt(APInvoice document, TaxTran taxTran, int gafLineNumber)
		{
			return _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt.Build(document, taxTran, Vendors[document.VendorID], gafLineNumber);
		}

		protected override IEnumerable<PXResult<APTran, APTax>> GetReportedTransWithTranTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return GafRepository.GetReportedAPTransWithAPTaxesForTaxCalcedOnItem(docType, refNbrs, taxAgencyID, taxPeriodID);
		}

		private void LoadValuableTaxApRegistersForDocuments(DocumentGroup<APInvoice> documentGroup, int? taxAgencyID)
		{
			_taxApRegistersByOrigRefNbrAndTax = new Dictionary<string, Dictionary<string, APRegister>>();

			var taxApRegistersWithTaxTransGroupsByOrigRefNbrAndTax = GafRepository.GetReportedTaxAPRegistersWithTaxTransForDocuments(documentGroup.DocumentType,
																																documentGroup.DocumentsByRefNbr.Keys.ToArray(), 
																																taxAgencyID)
																					.GroupBy(row => new {((TaxTran) row).OrigRefNbr, ((TaxTran) row).TaxID});

			foreach (var taxApRegistersWithTaxTransGroup in taxApRegistersWithTaxTransGroupsByOrigRefNbrAndTax)
			{
				var taxApRegistersWithTaxTran = taxApRegistersWithTaxTransGroup.OrderBy(row =>
																					{
																						var apRegister = (APRegister) row;

																						switch (apRegister.DocType)
																						{
																							case APDocType.Invoice:
																								return 0;
																							case APDocType.CreditAdj:
																								return 1;
																							case APDocType.DebitAdj:
																								return 2;
																							default:
																								throw new PXException(Messages.TaxDocumentTypeOfTaxTransactionCannotBeIdentified,
																									apRegister.DocType, ((TaxTran) row).GetKeyImage());
																						}
																					})
																				.ThenBy(row => ((APRegister)row).RefNbr)
																				.First();

				var taxTran = (TaxTran) taxApRegistersWithTaxTran;
				var taxApRegister = (APRegister) taxApRegistersWithTaxTran;

				if (!_taxApRegistersByOrigRefNbrAndTax.ContainsKey(taxTran.OrigRefNbr))
				{
					_taxApRegistersByOrigRefNbrAndTax[taxTran.OrigRefNbr] = new Dictionary<string, APRegister>();
				}

				_taxApRegistersByOrigRefNbrAndTax[taxTran.OrigRefNbr].Add(taxTran.TaxID, taxApRegister);
			}
		}

		protected Dictionary<int?, Vendor> GetContragentsForDocuments(IEnumerable<APRegister> documents)
		{
			var usedVendorIDs = documents.Select(apReg => apReg.VendorID)
											.Distinct()
											.ToArray();

			return GafRepository.GetVendorsByIDs(usedVendorIDs)
									.ToDictionary(vendor => vendor.BAccountID, vendor => vendor);
		}

		protected override IList<PurchaseRecord> CreateGafRecordsForDocument(DocumentID documentId)
		{
			var records = base.CreateGafRecordsForDocument(documentId);

			var recordsByTaxDocuments = CreateDataByTaxDocumentTrans(documentId.RefNbr, records.Count + 1);

			var taxTran = TaxTransByRefNbrAndTaxID[documentId.RefNbr].Values.First();

			ApplySign(recordsByTaxDocuments, taxTran.Module, documentId.DocType);

			records.AddRange(recordsByTaxDocuments);

			return records;
		}

		protected override PurchaseRecord BuildGafRecordByDocumentTran(APInvoice invoice, APTran tran, APTax tranTax, int lineNumberForGaf)
		{
			return _purchaseRecordBuilderByInvoiceTran.Build(invoice, tran, tranTax, Vendors[invoice.VendorID],
				lineNumberForGaf);
		}

		/// <summary>
		/// Create GAF Data by tax transactions which have been created by Tax Bills and Adjustments.
		/// </summary>
		private IList<PurchaseRecord> CreateDataByTaxDocumentTrans(string refNbr, int startGafLineNumber)
		{
			var purchaseRecords = new List<PurchaseRecord>();

			if(!_taxApRegistersByOrigRefNbrAndTax.ContainsKey(refNbr))
				return new List<PurchaseRecord>();

			var apRegister = DocumentsByRefNbr[refNbr];

			var taxTransByTaxDocuments =
				TaxTransByRefNbrAndTaxID[refNbr].Values.Where(taxTran => Taxes[taxTran.TaxID].DirectTax == true)
														.OrderBy(taxTran => taxTran.TaxID);

			foreach (var taxTran in taxTransByTaxDocuments)
			{
				var purchaseRecord = _purchaseRrcordBuilderByTaxTranFromTaxDocument.Build(
											apRegister,
											taxTran,
											Vendors[apRegister.VendorID],
											startGafLineNumber++,
											_taxApRegistersByOrigRefNbrAndTax[refNbr][taxTran.TaxID].DocDesc);

				purchaseRecords.Add(purchaseRecord);
			}

			return purchaseRecords;
		}		
	}
}