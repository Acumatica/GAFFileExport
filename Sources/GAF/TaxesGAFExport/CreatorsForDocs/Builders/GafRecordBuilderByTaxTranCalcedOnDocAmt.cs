using PX.Objects.CM;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public abstract class GafRecordBuilderByTaxTranCalcedOnDocumentAmt<TGafRecord, TRegister> : GafRecordBuilderByRegisterAndTaxTranBase<TGafRecord, TRegister> 
		where TGafRecord : GAFRecordBase, new()
		where TRegister : IRegister
	{
		public GafRecordBuilderByTaxTranCalcedOnDocumentAmt(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister)
			: base(gafRepository, gafRecordBuilderByRegister)
		{
		}

		protected override TGafRecord BuildInternal(TRegister register, TaxTran taxTran, int gafLineNumber)
		{
			var gafRecord = base.BuildInternal(register, taxTran, gafLineNumber);

			gafRecord.ProductDescription = register.DocDesc;

			return gafRecord;
		}
	}
}
