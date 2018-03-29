using PX.Objects.CM;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public abstract class GafRecordBuilderByRegisterAndTaxTranBase<TGafRecord, TRegister> : GafRecordBuilderByTaxTranBase
		where TGafRecord: GAFRecordBase, new() 
		where TRegister: IRegister
	{
		private readonly GafRecordBuilderByRegister _gafRecordBuilderByRegister;

		protected GafRecordBuilderByRegisterAndTaxTranBase(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister) : base(gafRepository)
		{
			_gafRecordBuilderByRegister = gafRecordBuilderByRegister;
		}

		protected virtual TGafRecord BuildInternal(TRegister register, TaxTran taxTran, int lineNumber)
		{
			var gafRecord = new TGafRecord();

			BuildInternal(gafRecord, taxTran, register.CuryID, lineNumber);
			_gafRecordBuilderByRegister.Build(gafRecord, register);

			return gafRecord;
		}
	}
}
