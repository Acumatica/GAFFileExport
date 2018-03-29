using PX.Objects.CM;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public class GafRecordBuilderByRegister : GafRecordBuilderBase
	{
		public GafRecordBuilderByRegister(IGAFRepository gafRepository) : base(gafRepository)
		{
		}

		public void Build(GAFRecordBase gafRecord, IRegister register)
		{
			gafRecord.InvoiceDate = register.DocDate.Value;
			gafRecord.InvoiceNumber = register.RefNbr;
			gafRecord.ForeignCurrencyCode = GetForeignCurrencyCode(register.CuryID);
		}
	}
}
