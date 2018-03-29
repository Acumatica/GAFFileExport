using PX.Objects.CA;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Purchase
{
	public class PurchaseRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt :
		GafRecordBuilderByTaxTranCalcedOnDocumentAmt<PurchaseRecord, CAAdj>
	{
		public PurchaseRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister) : base(gafRepository, gafRecordBuilderByRegister)
		{
		}

		public PurchaseRecord Build(CAAdj caAdj, TaxTran taxTran, int gafLineNumber)
		{
			return BuildInternal(caAdj, taxTran, gafLineNumber);
		}
	}
}
