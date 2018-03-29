using PX.Objects.AR;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices
{
	public class SupplyRecordBuilderByARInvoiceTaxTranForTaxCalcedOnDocumentAmt : SupplyRecordBuilderByTaxTranCalcedOnDocumentAmtBase<ARRegister>
	{
		private readonly SupplyRecordBuilderByCustomerData _recordBuilderByCustomerData;
		private readonly SupplyRecordCountryBuilderForARInvoice _recordCountryBuilder;

		public SupplyRecordBuilderByARInvoiceTaxTranForTaxCalcedOnDocumentAmt(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister,
			SupplyRecordBuilderByCustomerData recordBuilderByCustomerData,
			SupplyRecordCountryBuilderForARInvoice recordCountryBuilder) : base(gafRepository, gafRecordBuilderByRegister)
		{
			_recordBuilderByCustomerData = recordBuilderByCustomerData;
			_recordCountryBuilder = recordCountryBuilder;
		}

		public SupplyRecord Build(ARInvoice invoice, TaxTran taxTran, Customer customer, int gafLineNumber)
		{
			var supplyRecord = BuildInternal(invoice, taxTran, gafLineNumber);
			_recordBuilderByCustomerData.Build(supplyRecord, invoice, customer);
			_recordCountryBuilder.Build(supplyRecord, invoice);

			return supplyRecord;
		}
	}
}
