using PX.Objects.CM;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply
{
	public abstract class SupplyRecordBuilderByTaxTranCalcedOnDocumentAmtBase<TRegister> :
		GafRecordBuilderByTaxTranCalcedOnDocumentAmt<SupplyRecord, TRegister>
		where TRegister: IRegister
	{
		protected SupplyRecordBuilderByTaxTranCalcedOnDocumentAmtBase(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister)
			: base(gafRepository, gafRecordBuilderByRegister)
		{

		}
	}
}
