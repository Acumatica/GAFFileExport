using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.Common.Documents;
using PX.Objects.GL;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using TaxesGAFExport.Data;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport
{
	/// <summary>
	/// Creates GST File data.
	/// </summary>
	public class GAFDataCreator
	{
		public List<string> _glTaxDocumentTypes = new List<string> { TaxTran.tranType.TranForward, TaxTran.tranType.TranReversed};

		private const string GafVersionTemplate = "GAFv{0}{1}{2}";
		private const string GafVersionDelimiter = ".";
		private const string GafVersionDelimiterForFileName = "_";

		private readonly IGAFRepository _gafRepository;
		private readonly GAFValidator _gafValidator;
		private readonly GLGAFLedgerRecordsCreator _glgafLedgerRecordsCreator;
		private readonly APInvoiceGAFRecordsCreator _apInvoiceGAFRecordsCreator;
		private readonly ARInvoiceGAFRecordsCreator _arInvoiceGAFRecordsCreator;
		private readonly ARInvoiceFromSOGAFRecordsCreator _arInvoiceFromSOGafRecordsCreator;
		private readonly APPaymentGAFRecordsCreator _apPaymentGAFRecordsCreator;
		private readonly CADocumentPurchaseGAFRecordsCreator _caDocumentPurchaseGAFRecordsCreator;
		private readonly CADocumentSupplyGAFRecordsCreator _caDocumentSupplyGAFRecordsCreator;
		private readonly TaxAdjustmentGAFRecordsCreator _taxAdjustmentGAFRecordsCreator;
		private readonly GLDocumentGAFRecordsCreator _glDocumentGAFRecordsCreator;
		private readonly GafRecordWriter _gafRecordWriter;
		private StringBuilder _purchaseGAFDataStringBuilder;
		private StringBuilder _supplyGAFDataStringBuilder;

		public GAFDataCreator(IGAFRepository gafRepository,
			GAFValidator gafValidator,
			GLGAFLedgerRecordsCreator glgafLedgerRecordsCreator,
			APInvoiceGAFRecordsCreator apInvoiceGAFRecordsCreator,
			ARInvoiceGAFRecordsCreator arInvoiceGAFRecordsCreator,
			ARInvoiceFromSOGAFRecordsCreator arInvoiceFromSOGafRecordsCreator,
			APPaymentGAFRecordsCreator apPaymentGAFRecordsCreator,
			CADocumentPurchaseGAFRecordsCreator caDocumentPurchaseGAFRecordsCreator,
			CADocumentSupplyGAFRecordsCreator caDocumentSupplyGAFRecordsCreator,
			TaxAdjustmentGAFRecordsCreator taxAdjustmentGAFRecordsCreator,
			GLDocumentGAFRecordsCreator glDocumentGAFRecordsCreator,
			GafRecordWriter gafRecordWriter)
		{
			if (gafRepository == null)
				throw new ArgumentNullException(nameof(gafRepository));
			if (gafValidator == null)
				throw new ArgumentNullException(nameof(gafValidator));
			if (glgafLedgerRecordsCreator == null)
				throw new ArgumentNullException(nameof(glgafLedgerRecordsCreator));
			if (apInvoiceGAFRecordsCreator == null)
				throw new ArgumentNullException(nameof(apInvoiceGAFRecordsCreator));
			if (arInvoiceGAFRecordsCreator == null)
				throw new ArgumentNullException(nameof(arInvoiceGAFRecordsCreator));
			if (arInvoiceFromSOGafRecordsCreator == null)
				throw new ArgumentNullException(nameof(arInvoiceFromSOGafRecordsCreator));
			if (apPaymentGAFRecordsCreator == null)
				throw new ArgumentNullException(nameof(apPaymentGAFRecordsCreator));
			if (caDocumentPurchaseGAFRecordsCreator == null)
				throw new ArgumentNullException(nameof(caDocumentPurchaseGAFRecordsCreator));
			if (caDocumentSupplyGAFRecordsCreator == null)
				throw new ArgumentNullException(nameof(caDocumentSupplyGAFRecordsCreator));
			if (taxAdjustmentGAFRecordsCreator == null)
				throw new ArgumentNullException(nameof(taxAdjustmentGAFRecordsCreator));
			if (gafRecordWriter == null)
				throw new ArgumentNullException(nameof(gafRecordWriter));

			_gafRepository = gafRepository;
			_gafValidator = gafValidator;
			_glgafLedgerRecordsCreator = glgafLedgerRecordsCreator;
			_apInvoiceGAFRecordsCreator = apInvoiceGAFRecordsCreator;
			_arInvoiceGAFRecordsCreator = arInvoiceGAFRecordsCreator;
			_arInvoiceFromSOGafRecordsCreator = arInvoiceFromSOGafRecordsCreator;
			_apPaymentGAFRecordsCreator = apPaymentGAFRecordsCreator;
			_caDocumentPurchaseGAFRecordsCreator = caDocumentPurchaseGAFRecordsCreator;
			_caDocumentSupplyGAFRecordsCreator = caDocumentSupplyGAFRecordsCreator;
			_taxAdjustmentGAFRecordsCreator = taxAdjustmentGAFRecordsCreator;
			_glDocumentGAFRecordsCreator = glDocumentGAFRecordsCreator;
			_gafRecordWriter = gafRecordWriter;
		}

		/// <summary>
		/// Creates GST File data.
		/// </summary>
        public GSTAuditFile Create(GAFPeriod gafPeriod, DateTime creationDate)
		{
			if (gafPeriod == null)
				throw new ArgumentNullException(nameof(gafPeriod));

			var validationResult =_gafValidator.ValidateTaxYearStructure(_gafRepository.GetTaxPeriodByKey(gafPeriod.BranchID, gafPeriod.TaxAgencyID, gafPeriod.TaxPeriodID));

			validationResult.RaiseIfHasError();

			var documentIds = _gafRepository.GetReportedDocuments(gafPeriod.BranchID, gafPeriod.TaxAgencyID, gafPeriod.TaxPeriodID)
											.ToArray();

			if (!documentIds.Any())
				return null;


			#region Initialization

			var glDocumentIds = documentIds.Where(documentId => _glTaxDocumentTypes.Contains(documentId.DocType))
											.ToArray();

			var documentGroups = SplitToDocumentGroups(documentIds.Except(glDocumentIds));

			if (glDocumentIds.Any())
			{
				var glDocumentGroup = BuildGLDocumentGroup(glDocumentIds);

				documentGroups.Add(glDocumentGroup);
			}

			var documentProcessingQueue = new GAFDocumentProcessingQueue(documentGroups);

			var gafDataStringBuilder = new StringBuilder();

			_purchaseGAFDataStringBuilder = new StringBuilder();
			_supplyGAFDataStringBuilder = new StringBuilder();

			var footerRecord = new FooterRecord();
			var gstAuditFile = new GSTAuditFile();

			#endregion


			#region Processing

			SetGAFVersion(gstAuditFile, gafPeriod);

			var companyRecord = CreateCompanyRecord(gafPeriod, gstAuditFile, creationDate);
			_gafRecordWriter.WriteCompanyRecord(companyRecord, gafDataStringBuilder);

			#region Documents

			var documentGroup = documentProcessingQueue.GetNextDocumentGroup();

			while (documentGroup != null)
			{
				CreateGAFDataByDocumentGroup(documentGroup, gafPeriod, footerRecord);

				_gafRepository.ClearCaches();

				documentGroup = documentProcessingQueue.GetNextDocumentGroup();
			}

			gafDataStringBuilder.Append(_purchaseGAFDataStringBuilder);
			gafDataStringBuilder.Append(_supplyGAFDataStringBuilder);

			#endregion


			#region Ledger

			var ledgerRecords = _glgafLedgerRecordsCreator.CreateLedgerRecords(gafPeriod);

			AddAmountsAndCountToFooterRecord(footerRecord, ledgerRecords);

			foreach (var ledgerRecord in ledgerRecords)
			{
				_gafRecordWriter.WriteLedgerRecord(ledgerRecord, gafDataStringBuilder);
			}

			#endregion


			_gafRecordWriter.WriteFooterRecord(footerRecord, gafDataStringBuilder);

			#endregion


			gstAuditFile.Data = gafDataStringBuilder.ToString();

			var branch = _gafRepository.GetBranchByID(gafPeriod.BranchID);

			gstAuditFile.FileName = string.Concat(branch.BranchCD.Trim(), "_",
				string.Format(GafVersionTemplate, gstAuditFile.MajorVersion, GafVersionDelimiterForFileName,
					gstAuditFile.MinorVersion), ".txt");

			return gstAuditFile;
		}

		private void SetGAFVersion(GSTAuditFile gstAuditFile, GAFPeriod gafPeriod)
		{
			if (gafPeriod.GAFMajorVersion == null)
			{
				var maxGAFMajorVersion = _gafRepository.GetMaxGAFMajorVersion(gafPeriod);

				if (maxGAFMajorVersion != null)
				{
                    gstAuditFile.MajorVersion = maxGAFMajorVersion.Value + 1;
				}
				else
				{
                    gstAuditFile.MajorVersion = 1;
				}

                gstAuditFile.MinorVersion = 0;
			}
			else
			{
				gstAuditFile.MajorVersion = gafPeriod.GAFMajorVersion.Value;
				gstAuditFile.MinorVersion = gafPeriod.GAFMinorLastVersion.Value + 1;
			}
		}

		private CompanyRecord CreateCompanyRecord(GAFPeriod gafPeriod, GSTAuditFile gstAuditFile, DateTime creationDate)
		{
			var companyRecord = new CompanyRecord();

			var branch = _gafRepository.GetBranchByID(gafPeriod.BranchID);
			var baccount = _gafRepository.GetBAccountByID(branch.BAccountID);
			var mainContact = _gafRepository.GetContact(baccount.DefContactID);

			companyRecord.CompanyName = mainContact.FullName;
			companyRecord.CompanyBRN = baccount.AcctReferenceNbr;
			companyRecord.CompanyGSTNumber = baccount.TaxRegistrationID;

			var taxPeriod = _gafRepository.GetTaxPeriodByKey(gafPeriod.BranchID, gafPeriod.TaxAgencyID, gafPeriod.TaxPeriodID);

			companyRecord.PeriodStartDate = taxPeriod.StartDate.Value;
			companyRecord.PeriodEndDate = taxPeriod.EndDateUI.Value;

			companyRecord.FileCreationDate = creationDate;

			var acumaticaVersion = _gafRepository.GetAcumaticaVersion();

			companyRecord.ProductVersion = String.Concat("Acumatica", acumaticaVersion.CurrentVersion);
			companyRecord.GAFVersion = String.Format(GafVersionTemplate, gstAuditFile.MajorVersion, GafVersionDelimiter,
				gstAuditFile.MinorVersion);

			return companyRecord;
		}

		private List<DocumentIDGroup> SplitToDocumentGroups(IEnumerable<DocumentID> documentIds)
		{
			return documentIds.GroupBy(documentId => new
														{
															documentId.DocType,
															documentId.Module
														})
								.OrderBy(docGroup => docGroup.Key.Module)
								.ThenBy(docGroup => docGroup.Key.DocType)
								.Select(docGroup => new DocumentIDGroup()
								{
									Module = docGroup.Key.Module,
									DocumentType = docGroup.Key.DocType,
									RefNbrs = docGroup.Select(documentId => documentId.RefNbr)
														.OrderBy(refNbr => refNbr)
														.ToList()
								})
								.ToList();
		}


		private DocumentIDGroup BuildGLDocumentGroup(IEnumerable<DocumentID> documentIds)
		{
			return new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				DocumentTypes = _glTaxDocumentTypes,
				RefNbrs = documentIds.Select(documentId => documentId.RefNbr)
										.Distinct()
										.ToList()
			};
		}

		#region Creation by document

		private void CreateGAFDataByDocumentGroup(DocumentIDGroup documentIDGroup,
																GAFPeriod gafPeriod,
																FooterRecord footerRecord)
		{
			IList<SupplyRecord> supplyRecords = new List<SupplyRecord>();
			IList<PurchaseRecord> purchaseRecords = new List<PurchaseRecord>();

			if (documentIDGroup.Module == BatchModule.AP)
			{
				if (documentIDGroup.DocumentType == APDocType.Check)
				{
					purchaseRecords = CreatePurchaseRecordsByAPPayments(documentIDGroup, gafPeriod);
				}
				else
				{
					purchaseRecords = CreatePurchaseRecordsByAPInvoices(documentIDGroup, gafPeriod);
				}
			}
			else if (documentIDGroup.Module == BatchModule.AR)
			{
				supplyRecords = CreateSupplyRecordsByARInvoices(documentIDGroup, gafPeriod);
			}
			else if (documentIDGroup.Module == BatchModule.CA)
			{
				CreateGafRecordsByCADocuments(documentIDGroup, gafPeriod, out purchaseRecords, out supplyRecords);
			}
			else if (documentIDGroup.Module == BatchModule.GL
					&& (documentIDGroup.DocumentTypes.Contains(TaxAdjustmentType.AdjustInput)
							|| documentIDGroup.DocumentTypes.Contains(TaxAdjustmentType.AdjustOutput)))
			{
				var documentRecordAggregate = _taxAdjustmentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, gafPeriod.BranchID, gafPeriod.TaxPeriodID);

				supplyRecords = documentRecordAggregate.SupplyRecords;
				purchaseRecords = documentRecordAggregate.PurchaseRecords;
			}
			else if (documentIDGroup.Module == BatchModule.GL
					&& documentIDGroup.DocumentTypes.Intersect(_glTaxDocumentTypes).Any())
			{
				var documentRecordAggregate = _glDocumentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, gafPeriod.BranchID, gafPeriod.TaxPeriodID);

				supplyRecords = documentRecordAggregate.SupplyRecords;
				purchaseRecords = documentRecordAggregate.PurchaseRecords;
			}

			WritePuchaseRecords(purchaseRecords);
			AddAmountsAndCountToFooterRecord(footerRecord, purchaseRecords);

			WriteSupplyRecords(supplyRecords);
			AddAmountsAndCountToFooterRecord(footerRecord, supplyRecords);
		}

		private IList<PurchaseRecord> CreatePurchaseRecordsByAPPayments(DocumentIDGroup documentIDGroup, GAFPeriod gafPeriod)
		{
			var apRegistersByRefNbrs = _gafRepository.GetAPRegistersByIDs(documentIDGroup.DocumentType,
																			documentIDGroup.RefNbrs.ToArray())
														.ToDictionary(apReg => apReg.RefNbr, apReg => apReg);

			var documentGroup = new DocumentGroup<APRegister>()
			{
				Module = documentIDGroup.Module,
				DocumentType = documentIDGroup.DocumentType,
				DocumentsByRefNbr = apRegistersByRefNbrs
			};

			var purchaseRecords = _apPaymentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup,
																								gafPeriod.TaxAgencyID,
																								gafPeriod.TaxPeriodID);
			return purchaseRecords;
		}

		private IList<PurchaseRecord> CreatePurchaseRecordsByAPInvoices(DocumentIDGroup documentIDGroup, GAFPeriod gafPeriod)
		{
			var resultRecords = new List<PurchaseRecord>();

			var apInvoices = _gafRepository.GetAPInvoicesByIDs(documentIDGroup.DocumentType, documentIDGroup.RefNbrs.ToArray());

			//module of taxTran is not honest, it alwayes equals to AP for all Account Payable documents
			var documentGroupsByModule = apInvoices.GroupBy(apReg => apReg.OrigModule);

			foreach (var documentGroupByModule in documentGroupsByModule)
			{
				var documentGroup = new DocumentGroup<APInvoice>()
				{
					Module = documentGroupByModule.Key,
					DocumentType = documentIDGroup.DocumentType,
					DocumentsByRefNbr = documentGroupByModule.ToDictionary(invoice => invoice.RefNbr, invoice => invoice)
				};

				var records = _apInvoiceGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup,
																							gafPeriod.TaxAgencyID,
																							gafPeriod.TaxPeriodID);

				resultRecords.AddRange(records);
			}

			return resultRecords;
		}

		private IList<SupplyRecord> CreateSupplyRecordsByARInvoices(DocumentIDGroup documentIDGroup, GAFPeriod gafPeriod)
		{
			var resultRecords = new List<SupplyRecord>();

			IDocumentGafRecordCreator<ARInvoice, SupplyRecord> documentGafRecordCreator = null;

			var arInvoices = _gafRepository.GetARInvoicesByIDs(documentIDGroup.DocumentType, documentIDGroup.RefNbrs.ToArray());

			//module of taxTran is not honest, it alwayes equals to AR for all Account Receivable documents
			var documentGroupsByModule = arInvoices.GroupBy(apReg => apReg.OrigModule);

			foreach (var documentGroupByModule in documentGroupsByModule)
			{
				var documentGroup = new DocumentGroup<ARInvoice>()
				{
					Module = documentIDGroup.Module,
					DocumentType = documentIDGroup.DocumentType
				};

				if (documentGroupByModule.Key == BatchModule.SO)
				{
					documentGafRecordCreator = _arInvoiceFromSOGafRecordsCreator;
				}
				else
				{
					documentGafRecordCreator = _arInvoiceGAFRecordsCreator;
				}

				documentGroup.DocumentsByRefNbr = documentGroupByModule.ToDictionary(invoice => invoice.RefNbr, invoice => invoice);

				var supplyRecords = documentGafRecordCreator.CreateGAFRecordsForDocumentGroup(documentGroup,
																							gafPeriod.TaxAgencyID,
																							gafPeriod.TaxPeriodID);

				resultRecords.AddRange(supplyRecords);
			}

			return resultRecords;
		}

		private void CreateGafRecordsByCADocuments(DocumentIDGroup documentIDGroup, 
														GAFPeriod gafPeriod,
														out IList<PurchaseRecord> purchaseRecords,
														out IList<SupplyRecord> supplyRecords)
		{
			purchaseRecords = new List<PurchaseRecord>();
			supplyRecords = new List<SupplyRecord>();

			var caAdjs = _gafRepository.GetCAAdjsByIDs(documentIDGroup.DocumentType, documentIDGroup.RefNbrs.ToArray());

			var caAdjGroupsByDrCr = caAdjs.GroupBy(caAdj => caAdj.DrCr);

			foreach (var caAdjGroupByDrCr in caAdjGroupsByDrCr)
			{
				var documentGroup = new DocumentGroup<CAAdj>()
				{
					Module = documentIDGroup.Module,
					DocumentType = documentIDGroup.DocumentType,
					DocumentsByRefNbr = caAdjGroupByDrCr.ToDictionary(caAdj => caAdj.AdjRefNbr, caAdj => caAdj)
				};

				if (caAdjGroupByDrCr.Key == CADrCr.CACredit)
				{
					purchaseRecords = _caDocumentPurchaseGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup,
						gafPeriod.TaxAgencyID, gafPeriod.TaxPeriodID);
				}
				else
				{
					supplyRecords = _caDocumentSupplyGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup,
						gafPeriod.TaxAgencyID, gafPeriod.TaxPeriodID);
				}
			}
		}

		private void WritePuchaseRecords(IEnumerable<PurchaseRecord> purchaseRecords)
		{
			foreach (var purchaseRecord in purchaseRecords)
			{
				_gafRecordWriter.WritePurchaseRecord(purchaseRecord, _purchaseGAFDataStringBuilder);
			}
		}

		private void WriteSupplyRecords(IEnumerable<SupplyRecord> supplyRecords)
		{
			foreach (var supplyRecord in supplyRecords)
			{
				_gafRecordWriter.WriteSupplyRecord(supplyRecord, _supplyGAFDataStringBuilder);
			}
		}

		#endregion


		#region Footer record

		private void AddAmountsAndCountToFooterRecord(FooterRecord footerRecord, IEnumerable<PurchaseRecord> purchaseRecords)
		{
			foreach (var purchaseRecord in purchaseRecords)
			{
				footerRecord.PurchaseRecordsCount++;
				footerRecord.PurchaseAmountSum += purchaseRecord.Amount;
				footerRecord.PurchaseGSTAmountSum += purchaseRecord.GSTAmount;
			}
		}

		private void AddAmountsAndCountToFooterRecord(FooterRecord footerRecord, IEnumerable<SupplyRecord> purchaseRecords)
		{
			foreach (var purchaseRecord in purchaseRecords)
			{
				footerRecord.SupplyRecordsCount++;
				footerRecord.SupplyAmountSum += purchaseRecord.Amount;
				footerRecord.SupplyGSTAmountSum += purchaseRecord.GSTAmount;
			}
		}

		private void AddAmountsAndCountToFooterRecord(FooterRecord footerRecord, IEnumerable<LedgerRecord> ledgerRecords)
		{
			foreach (var ledgerRecord in ledgerRecords)
			{
				footerRecord.LedgerRecordsCount++;
				footerRecord.DebitSum += ledgerRecord.DebitAmount;
				footerRecord.CreditSum += ledgerRecord.CreditAmount;
				footerRecord.BalanceSum += ledgerRecord.BalanceAmount;
			}
		}		

		#endregion
	}
}