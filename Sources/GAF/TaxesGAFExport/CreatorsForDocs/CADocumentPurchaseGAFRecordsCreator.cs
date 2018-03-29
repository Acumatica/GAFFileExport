using System.Collections.Generic;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Purchase;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using TaxesGAFExport.Data;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs
{
	public class CADocumentPurchaseGAFRecordsCreator : DocumentWithTransGAFRecordsCreatorBase<CAAdj, CASplit, CATax, PurchaseRecord>
	{
		private readonly PurchaseRecordBuilderByCADocument _purchaseRecordBuilderByCaDocument;
		private readonly PurchaseRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;

		protected override GAFRecordType ExpectedGafRecordType
		{
			get { return GAFRecordType.Purchase; }
		}

		public CADocumentPurchaseGAFRecordsCreator(IGAFRepository gafRepository,
			PurchaseRecordBuilderByCADocument purchaseRecordBuilderByCaDocument,
			PurchaseRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt recordBuilderByTaxTranForTaxCalcedOnDocumentAmt) : base(gafRepository)
		{
			_purchaseRecordBuilderByCaDocument = purchaseRecordBuilderByCaDocument;
			_recordBuilderByTaxTranForTaxCalcedOnDocumentAmt = recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;
		}

		protected override IEnumerable<PXResult<CASplit, CATax>> GetReportedTransWithTranTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return GafRepository.GetReportedCASplitsWithCATaxesForTaxCalcedOnItem(docType, refNbrs, taxAgencyID, taxPeriodID);
		}

		protected override PurchaseRecord BuildGafRecordByTaxTranCalcedOnDocumentAmt(CAAdj document, TaxTran taxTran, int gafLineNumber)
		{
			return _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt.Build(document, taxTran, gafLineNumber);
		}

		protected override PurchaseRecord BuildGafRecordByDocumentTran(CAAdj document, CASplit tran, CATax tranTax, int lineNumberForGaf)
		{
			return _purchaseRecordBuilderByCaDocument.Build(document, tran, tranTax, lineNumberForGaf);
		}
	}
}