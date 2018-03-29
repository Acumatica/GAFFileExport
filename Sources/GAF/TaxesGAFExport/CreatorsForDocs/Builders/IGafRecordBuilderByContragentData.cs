using PX.Objects.CM;
using TaxesGAFExport.Data.Records;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public interface IGafRecordBuilderByContragentData<TGAFRecord, TRegister, TContragent>
		where TGAFRecord: GAFRecordBase
		where TRegister: IRegister
	{
		void Build(TGAFRecord gafRecord, TRegister register, TContragent contragent);
	}
}
