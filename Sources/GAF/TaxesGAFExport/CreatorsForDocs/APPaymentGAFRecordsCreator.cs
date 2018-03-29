using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common.Documents;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Purchase;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs
{
	/// <summary>
	/// Exports withholding tax trans linked to Check.
	/// </summary>
	public class APPaymentGAFRecordsCreator : DocumentGAFRecordsCreatorBase<APRegister, PurchaseRecord>
	{
		private readonly PurchaseRecordBuilderByTaxTranOfAPPayment _recordBuilderByTaxTranOfApPayment;

		private Dictionary<string, List<PXResult<TaxTran, APRegister>>> _taxTransWithAdjdDocumentByRefNbrAndTaxID;

		protected Dictionary<int?, Vendor> Contragents;

		public APPaymentGAFRecordsCreator(IGAFRepository gafRepository,
			PurchaseRecordBuilderByTaxTranOfAPPayment recordBuilderByTaxTranOfApPayment) : base(gafRepository)
		{
			_recordBuilderByTaxTranOfApPayment = recordBuilderByTaxTranOfApPayment;
		}

		private Dictionary<int?, Vendor> GetContragentsForDocuments(IEnumerable<APRegister> documents)
		{
			var usedVendorIDs = documents.Select(apReg => apReg.VendorID)
											.Distinct()
											.ToArray();

			return GafRepository.GetVendorsByIDs(usedVendorIDs)
									.ToDictionary(vendor => vendor.BAccountID, vendor => vendor);
		}

		protected override void LoadData(DocumentGroup<APRegister> documentGroup, int? taxAgencyID, string taxPeriodID)
		{
			Contragents = GetContragentsForDocuments(DocumentsByRefNbr.Values);

			_taxTransWithAdjdDocumentByRefNbrAndTaxID = GafRepository.GetReportedTaxTransWithAdjdDocumentForAPPayments(documentGroup.DocumentType,
																												documentGroup.DocumentsByRefNbr.Keys.ToArray(),
																												taxAgencyID,
																												taxPeriodID)
																		.GroupBy(row => ((TaxTran)row).RefNbr)
																		.ToDictionary(group => group.Key, group => group.ToList());
		}

		protected override IList<PurchaseRecord> CreateGafRecordsForDocument(DocumentID documentId)
		{
			//we can not match old check to bill -- skip it.
			if (!_taxTransWithAdjdDocumentByRefNbrAndTaxID.ContainsKey(documentId.RefNbr))
				return new List<PurchaseRecord>();

			var taxTransWithAdjdDocument = _taxTransWithAdjdDocumentByRefNbrAndTaxID[documentId.RefNbr].OrderBy(taxTranWithAdjdDocument => ((TaxTran)taxTranWithAdjdDocument).AdjdDocType)
																				.ThenBy(taxTranWithAdjdDocument => ((TaxTran)taxTranWithAdjdDocument).AdjdRefNbr)
																				.ThenBy(taxTranWithAdjdDocument => ((TaxTran)taxTranWithAdjdDocument).AdjNbr)
																				.ThenBy(taxTranWithAdjdDocument => ((TaxTran)taxTranWithAdjdDocument).TaxID);

			var paymentRegister = DocumentsByRefNbr[documentId.RefNbr];

			var lineNumberForGaf = 1;
			TaxTran prevTaxTran = taxTransWithAdjdDocument.First();

			var purchaseRecords = new List<PurchaseRecord>();

			foreach (var taxTranWithAdjdDocument in taxTransWithAdjdDocument)
			{
				var taxTran = (TaxTran)taxTranWithAdjdDocument;
				var adjdDocumentRegister = (APRegister)taxTranWithAdjdDocument;

				if (taxTran.AdjdDocType != prevTaxTran.AdjdDocType
					|| taxTran.AdjdRefNbr != prevTaxTran.AdjdRefNbr
					|| taxTran.AdjNbr != prevTaxTran.AdjNbr)
				{
					prevTaxTran = taxTran;
					lineNumberForGaf++;
				}

				var gafRecord = _recordBuilderByTaxTranOfApPayment.Build(paymentRegister, taxTran,
					Contragents[paymentRegister.VendorID], lineNumberForGaf, adjdDocumentRegister.DocDesc);

				purchaseRecords.Add(gafRecord);
			}

			return purchaseRecords;
		}
	}
}