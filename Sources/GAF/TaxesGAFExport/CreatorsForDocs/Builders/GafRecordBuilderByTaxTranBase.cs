using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public abstract class GafRecordBuilderByTaxTranBase: GafRecordBuilderBase
	{
		protected GafRecordBuilderByTaxTranBase(IGAFRepository gafRepository) : base(gafRepository)
		{

		}

		protected void BuildInternal(GAFRecordBase gafRecord, TaxTran taxTran, string curyID, int lineNumber)
		{
			gafRecord.LineNumber = lineNumber;

			gafRecord.GSTAmount = taxTran.TaxAmt.Value;
			gafRecord.Amount = taxTran.TaxableAmt.Value;

			gafRecord.ForeignCurrencyAmountGST = GetForeignCurrencyAmount(curyID, taxTran.CuryTaxAmt.Value);
			gafRecord.ForeignCurrencyAmount = GetForeignCurrencyAmount(curyID, taxTran.CuryTaxableAmt.Value);

			gafRecord.TaxCode = taxTran.TaxID;
		}
	}
}
