using System.Collections.Generic;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.CADocuments;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using TaxesGAFExport.Data;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs
{
	public class CADocumentSupplyGAFRecordsCreator: DocumentWithTransGAFRecordsCreatorBase<CAAdj, CASplit, CATax, SupplyRecord>
	{
		private readonly SupplyRecordBuilderByCADocument _supplyRecordBuilderByCaDocument;
		private readonly SupplyRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;

		protected override GAFRecordType ExpectedGafRecordType
		{
			get { return GAFRecordType.Supply; }
		}

		public CADocumentSupplyGAFRecordsCreator(IGAFRepository gafRepository, SupplyRecordBuilderByCADocument supplyRecordBuilderByCaDocument,
			SupplyRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt recordBuilderByTaxTranForTaxCalcedOnDocumentAmt ) : base(gafRepository)
		{
			_supplyRecordBuilderByCaDocument = supplyRecordBuilderByCaDocument;
			_recordBuilderByTaxTranForTaxCalcedOnDocumentAmt = recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;
		}

		protected override IEnumerable<PXResult<CASplit, CATax>> GetReportedTransWithTranTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return GafRepository.GetReportedCASplitsWithCATaxesForTaxCalcedOnItem(docType, refNbrs, taxAgencyID, taxPeriodID);
		}

		protected override SupplyRecord BuildGafRecordByTaxTranCalcedOnDocumentAmt(CAAdj document, TaxTran taxTran, int gafLineNumber)
		{
			return _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt.Build(document, taxTran, gafLineNumber);
		}

		protected override SupplyRecord BuildGafRecordByDocumentTran(CAAdj document, CASplit tran, CATax tranTax, int lineNumberForGaf)
		{
			return _supplyRecordBuilderByCaDocument.Build(document, tran, tranTax, lineNumberForGaf);
		}
	}
}