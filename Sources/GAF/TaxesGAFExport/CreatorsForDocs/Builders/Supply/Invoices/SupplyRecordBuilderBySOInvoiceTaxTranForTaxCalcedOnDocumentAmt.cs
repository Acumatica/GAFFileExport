using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices
{
	public class SupplyRecordBuilderBySOInvoiceTaxTranForTaxCalcedOnDocumentAmt : SupplyRecordBuilderByTaxTranCalcedOnDocumentAmtBase<ARRegister>
	{
		private readonly SupplyRecordBuilderByCustomerData _recordBuilderByCustomerData;
		private readonly SupplyRecordCountryBuilderForSOInvoice _recordCountryBuilder;

		public SupplyRecordBuilderBySOInvoiceTaxTranForTaxCalcedOnDocumentAmt(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister,
			SupplyRecordBuilderByCustomerData recordBuilderByCustomerData,
			SupplyRecordCountryBuilderForSOInvoice recordCountryBuilder) : base(gafRepository, gafRecordBuilderByRegister)
		{
			_recordBuilderByCustomerData = recordBuilderByCustomerData;
			_recordCountryBuilder = recordCountryBuilder;
		}

		public SupplyRecord Build(SOInvoice soInvoice, ARRegister register, TaxTran taxTran, Customer customer, int gafLineNumber)
		{
			var supplyRecord = BuildInternal(register, taxTran, gafLineNumber);
			_recordBuilderByCustomerData.Build(supplyRecord, register, customer);
			_recordCountryBuilder.Build(supplyRecord, soInvoice);

			return supplyRecord;
		}
	}
}
