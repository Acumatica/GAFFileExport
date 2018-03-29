using PX.Objects.CM;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public abstract class GafRecordBuilderByDocumentTranBase<TGafRecord, TRegister, TTran, TTranTax> : GafRecordBuilderBase 
		where TGafRecord : GAFRecordBase, new() 
		where TRegister : IRegister
		where TTran : IDocumentTran
		where TTranTax : ITranTax
	{
		private readonly GafRecordBuilderByRegister _recordBuilderByRegister;

		public GafRecordBuilderByDocumentTranBase(IGAFRepository gafRepository,
			GafRecordBuilderByRegister recordBuilderByRegister) : base(gafRepository)
		{
			_recordBuilderByRegister = recordBuilderByRegister;
		}

		protected TGafRecord BuildInternal(TRegister register, TTran tran, TTranTax tranTax, int lineNumber)
		{
			var gafRecord = new TGafRecord();

			_recordBuilderByRegister.Build(gafRecord, register);

			gafRecord.LineNumber = lineNumber;
			gafRecord.ProductDescription = tran.TranDesc;
			gafRecord.Amount = tranTax.TaxableAmt.Value;
			gafRecord.GSTAmount = tranTax.TaxAmt.Value;
			gafRecord.TaxCode = tranTax.TaxID;
			gafRecord.ForeignCurrencyAmount = GetForeignCurrencyAmount(register.CuryID, tranTax.CuryTaxableAmt.Value);
			gafRecord.ForeignCurrencyAmountGST = GetForeignCurrencyAmount(register.CuryID, tranTax.CuryTaxAmt.Value);

			return gafRecord;
		}
	}
}
