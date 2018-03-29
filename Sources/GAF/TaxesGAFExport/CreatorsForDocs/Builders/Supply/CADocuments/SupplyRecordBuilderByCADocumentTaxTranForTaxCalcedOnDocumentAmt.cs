using PX.Objects.CA;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.CADocuments
{
	public class SupplyRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt : SupplyRecordBuilderByTaxTranCalcedOnDocumentAmtBase<CAAdj>
	{
		public SupplyRecordBuilderByCADocumentTaxTranForTaxCalcedOnDocumentAmt(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister) : base(gafRepository, gafRecordBuilderByRegister)
		{
		}

		public SupplyRecord Build(CAAdj caAdj, TaxTran taxTran, int gafLineNumber)
		{
			return BuildInternal(caAdj, taxTran, gafLineNumber);
		}
	}
}
