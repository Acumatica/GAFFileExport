using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public class GafRecordBuilderByTaxAdjustmentTaxTran: GafRecordBuilderByTaxTranBase
	{
		public GafRecordBuilderByTaxAdjustmentTaxTran(IGAFRepository gafRepository) : base(gafRepository)
		{
		}

		public void Build(GAFRecordBase gafRecord, TaxAdjustment taxAdjustment, TaxTran taxTran, int lineNumber)
		{
			BuildInternal(gafRecord, taxTran, taxAdjustment.CuryID, lineNumber);

			gafRecord.InvoiceNumber = taxAdjustment.RefNbr;
			gafRecord.InvoiceDate = taxAdjustment.DocDate.Value;
			gafRecord.ProductDescription = taxAdjustment.DocDesc;
			gafRecord.ForeignCurrencyCode = GetForeignCurrencyCode(taxAdjustment.CuryID);
		}
	}
}
