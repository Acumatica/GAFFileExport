using PX.Objects.GL;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public class GafRecordBuilderByGLTranAndTaxTran : GafRecordBuilderByTaxTranBase
	{
		public GafRecordBuilderByGLTranAndTaxTran(IGAFRepository gafRepository) : base(gafRepository)
		{
		}

		public void Build(GAFRecordBase gafRecord, GLTran glTran, string curyID, TaxTran taxTran, int lineNumber)
		{
			BuildInternal(gafRecord, taxTran, curyID, lineNumber);

			gafRecord.InvoiceNumber = string.Concat(glTran.BatchNbr, glTran.RefNbr);
			gafRecord.InvoiceDate = glTran.TranDate.Value;
			gafRecord.ProductDescription = glTran.TranDesc;
			gafRecord.ForeignCurrencyCode = GetForeignCurrencyCode(curyID);			
		}
	}
}
