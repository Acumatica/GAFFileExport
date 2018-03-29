using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.TX;
using TaxesGAFExport.Data;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Invoices
{
	public abstract class DocumentWithTransGAFRecordsCreatorBase<TDocument, TTran, TTranTax, TGAFRecord> : DocumentGAFRecordsCreatorBase<TDocument, TGAFRecord>
		where TTranTax : class, ITranTax, IBqlTable, new() 
		where TTran : class, IDocumentTran, IBqlTable, new()
		where TDocument : IRegister
		where TGAFRecord : GAFRecordBase, new()
	{
		protected Dictionary<string, Dictionary<string, TaxTran>> TaxTransByRefNbrAndTaxID;

		/// <summary>
		/// Contains data only for taxes which are calculated on item amount.
		/// </summary>
		protected Dictionary<string, PXResult<TTran, TTranTax>[]> TranWithTranTaxByDocument;

		protected Dictionary<string, Tax> Taxes;

		#region BaseCuryID
		private string _baseCuryID;

		protected string BaseCuryID
		{
			get
			{
				if (_baseCuryID == null)
				{
					_baseCuryID = GafRepository.GetCompany().BaseCuryID;
				}

				return _baseCuryID;
			}
		}

		#endregion

		protected abstract GAFRecordType ExpectedGafRecordType { get; }

		protected DocumentWithTransGAFRecordsCreatorBase(IGAFRepository gafRepository) : base(gafRepository)
		{
		}

		#region Repo Methods

		protected abstract IEnumerable<PXResult<TTran, TTranTax>> GetReportedTransWithTranTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID);

		protected Dictionary<string, Dictionary<string, TaxTran>> LoadTaxTransForDocumentsGroupedByDocumentAndTax(string module, string docType, IEnumerable<string> refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return GafRepository.GetReportedTaxTransForDocuments(module, docType, refNbrs.ToArray(), taxAgencyID, taxPeriodID)
								.GroupBy(taxTran => taxTran.RefNbr)
								.ToDictionary(group => group.Key,
												group => group.ToDictionary(taxTran => taxTran.TaxID, taxTran => taxTran));
		}

		#endregion

		protected override IList<TGAFRecord> CreateGafRecordsForDocument(DocumentID documentId)
		{
			var gafRecords = CreateGAFRecordsByDocumentLineTrans(documentId.RefNbr);

			var gafRecordsByDocAmt = CreateGAFRecordsByTaxTransCalcedOnDocumentAmt(documentId.RefNbr, gafRecords.Count + 1);

			gafRecords.AddRange(gafRecordsByDocAmt);

			var taxTran = GetFirstTaxTranOfDocument(documentId.RefNbr);

			ApplySign(gafRecords, taxTran.Module, documentId.DocType);

			return gafRecords;
		}		

		protected override void LoadData(DocumentGroup<TDocument> documentGroup, int? taxAgencyID, string taxPeriodID)
		{
			var refNbrs = documentGroup.DocumentsByRefNbr.Keys.ToArray();

			TaxTransByRefNbrAndTaxID = LoadTaxTransForDocumentsGroupedByDocumentAndTax(documentGroup.Module, documentGroup.DocumentType, refNbrs, taxAgencyID, taxPeriodID);

			TranWithTranTaxByDocument = GetReportedTransWithTranTaxesForTaxCalcedOnItem(documentGroup.DocumentType, refNbrs, taxAgencyID, taxPeriodID)
														.GroupBy(row => ((TTran)row).RefNbr)
														.ToDictionary(group => group.Key, group => group.ToArray());

			var usedTaxIDs = TaxTransByRefNbrAndTaxID.Values.SelectMany(taxTransByTax => taxTransByTax.Values.Select(taxTran => taxTran.TaxID))
															.Union(TranWithTranTaxByDocument.Values.SelectMany(rows => rows.Select(row => ((TTranTax)row).TaxID)))
															.Distinct()
															.ToArray();

			Taxes = GafRepository.GetTaxesByIDs(usedTaxIDs)
								 .ToDictionary(tax => tax.TaxID, tax => tax);
		}

		protected IList<TGAFRecord> CreateGAFRecordsByTaxTransCalcedOnDocumentAmt(string refNbr, int startGafLineNumber)
		{
			var taxTransCalcedOnDocAmt = TaxTransByRefNbrAndTaxID[refNbr].Where(taxTaxTranKvp => Taxes[taxTaxTranKvp.Key].TaxCalcType == CSTaxCalcType.Doc)
																			.Select(taxTaxTranKvp => taxTaxTranKvp.Value);

			var gafRecords = new List<TGAFRecord>();

			foreach (var taxTran in taxTransCalcedOnDocAmt)
			{
				var gafRecord = BuildGafRecordByTaxTranCalcedOnDocumentAmt(DocumentsByRefNbr[refNbr], taxTran, startGafLineNumber++);

				gafRecords.Add(gafRecord);
			}

			return gafRecords;
		}

		protected abstract TGAFRecord BuildGafRecordByTaxTranCalcedOnDocumentAmt(TDocument document, TaxTran taxTran, int gafLineNumber);

		protected IList<TGAFRecord> CreateGAFRecordsByDocumentLineTrans(string refNbr)
		{
			if (!TranWithTranTaxByDocument.ContainsKey(refNbr))
				return new List<TGAFRecord>();

			var transWithTaxData = TranWithTranTaxByDocument[refNbr].OrderBy(apTranWithTaxData => ((TTranTax)apTranWithTaxData).LineNbr)
																	.ThenBy(apTranWithTaxData => ((TTranTax)apTranWithTaxData).TaxID);
			var register = DocumentsByRefNbr[refNbr];
			var lineNumberForGaf = 0;
			var currentDocumentLineNbr = -1;

			var gafRecordsByTax = new Dictionary<string, List<TGAFRecord>>();

			foreach (var tranWithTaxData in transWithTaxData)
			{
				var tran = (TTran)tranWithTaxData;
				var tranTax = (TTranTax)tranWithTaxData;
				var taxTran = TaxTransByRefNbrAndTaxID[refNbr][tranTax.TaxID];

				ValidateTaxAndRecordTypeAccordance(taxTran);

				if (tran.LineNbr != currentDocumentLineNbr)
				{
					currentDocumentLineNbr = tran.LineNbr.Value;
					lineNumberForGaf++;
				}

				var gafRecord = BuildGafRecordByDocumentTran(register, tran, tranTax, lineNumberForGaf);

				if (!gafRecordsByTax.ContainsKey(tranTax.TaxID))
				{
					gafRecordsByTax[tranTax.TaxID] = new List<TGAFRecord>();
				}

				gafRecordsByTax[tranTax.TaxID].Add(gafRecord);
			}

			CompensateDiscrepancy(TaxTransByRefNbrAndTaxID[refNbr],
									transWithTaxData.Select(row => (TTranTax)row),
									gafRecordsByTax,
									register.CuryID);

			var gafRecords = gafRecordsByTax.Values.SelectMany(records => records.Select(record => record))
															.OrderBy(record => record.LineNumber)
															.ToList();

			return gafRecords;
		}

		protected TaxTran GetFirstTaxTranOfDocument(string refNbr)
		{
			return TaxTransByRefNbrAndTaxID[refNbr].Values.First();
		}

		private void ValidateTaxAndRecordTypeAccordance(TaxTran taxTran)
		{
			var recordType = GetRecordType(taxTran, Taxes[taxTran.TaxID]);

			if (recordType != ExpectedGafRecordType)
				throw new InvalidEnumArgumentException("recordType", (int) recordType, typeof (GAFRecordType));
		}

		protected abstract TGAFRecord BuildGafRecordByDocumentTran(TDocument document, TTran tran, TTranTax tranTax, int lineNumberForGaf);

		protected void CompensateDiscrepancy(Dictionary<string, TaxTran> taxTransByTax, 
											IEnumerable<TTranTax> tranTax, 
											IReadOnlyDictionary<string, List<TGAFRecord>> gafRecordsByTax,
											string documentCuryID)
		{
			var taxDetailsByTaxGroups = tranTax.GroupBy(apTax => apTax.TaxID);

			foreach (var taxDetailsForTax in taxDetailsByTaxGroups)
			{
				var taxID = taxDetailsForTax.Key;

				TGAFRecord cachedRecordWithMaxAmount = null;
				Func<TGAFRecord> getCachedRecordWithMaxAmount = () =>
				{
					if (cachedRecordWithMaxAmount == null)
					{
						cachedRecordWithMaxAmount = gafRecordsByTax[taxID].GetItemWithMax(gafRecord => gafRecord.Amount);
					}

					return cachedRecordWithMaxAmount;
				};

				//taxableAmt
				var taxableAmtSum = taxDetailsForTax.Sum(apTax => apTax.TaxableAmt);
				var taxableAmtDiscrepancy = taxableAmtSum - taxTransByTax[taxID].TaxableAmt;

				if (taxableAmtDiscrepancy != 0.0m)
				{
					var recordWithMaxAmount = getCachedRecordWithMaxAmount();
					recordWithMaxAmount.Amount -= taxableAmtDiscrepancy.Value;
				}

				//taxAmt
				var taxAmtSum = taxDetailsForTax.Sum(apTax => apTax.TaxAmt);
				var taxAmtDiscrepancy = taxAmtSum - taxTransByTax[taxID].TaxAmt;

				if (taxAmtDiscrepancy != 0.0m)
				{
					var recordWithMaxAmount = getCachedRecordWithMaxAmount();
					recordWithMaxAmount.GSTAmount -= taxAmtDiscrepancy.Value;
				}

				if (documentCuryID != BaseCuryID)
				{
					//curyTaxableAmt
					var curyTaxableAmtSum = taxDetailsForTax.Sum(apTax => apTax.CuryTaxableAmt);
					var curyTaxableAmtDiscrepancy = curyTaxableAmtSum - taxTransByTax[taxID].CuryTaxableAmt;

					if (curyTaxableAmtDiscrepancy != 0.0m)
					{
						var recordWithMaxAmount = getCachedRecordWithMaxAmount();
						recordWithMaxAmount.ForeignCurrencyAmount -= curyTaxableAmtDiscrepancy.Value;
					}

					//curyTaxAmt
					var curyTaxAmtSum = taxDetailsForTax.Sum(apTax => apTax.CuryTaxAmt);
					var curyTaxAmtDiscrepancy = curyTaxAmtSum - taxTransByTax[taxID].CuryTaxAmt;

					if (curyTaxAmtDiscrepancy != 0.0m)
					{
						var recordWithMaxAmount = getCachedRecordWithMaxAmount();
						recordWithMaxAmount.ForeignCurrencyAmountGST -= curyTaxAmtDiscrepancy.Value;
					}
				}
			}
		}

		protected GAFRecordType GetRecordType(TaxTran taxTran, Tax tax)
		{
			if (taxTran.TaxType == CSTaxType.Use
				|| taxTran.TaxType == CSTaxType.Sales && tax.ReverseTax == true
				|| tax.TaxType == CSTaxType.Withholding)
			{
				return GAFRecordType.Purchase;
			}
			else if (taxTran.TaxType == CSTaxType.Sales && tax.ReverseTax != true)
			{
				return GAFRecordType.Supply;
			}
			else
			{
				throw new PXException(Messages.TheTypeOfGAFRecordPurchaseOrSupplyCannotBeDefined, taxTran);
			}
		}

		protected void ApplySign(IEnumerable<GAFRecordBase> gafRecords, string taxTranModule, string docType)
		{
			var sign = ReportTaxProcess.GetMultByTranType(taxTranModule, docType);

			if (sign < 0)
			{
				foreach (var gafRecord in gafRecords)
				{
					gafRecord.Amount *= sign;
					gafRecord.GSTAmount *= sign;
					gafRecord.ForeignCurrencyAmount *= sign;
					gafRecord.ForeignCurrencyAmountGST *= sign;
				}
			}
		}
	}
}
