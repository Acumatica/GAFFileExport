using PX.Objects.AR;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices
{
	public abstract class SupplyRecordBuilderByInvoiceBase<TExtendedInvoice>: GafRecordBuilderByDocumentTranBase<SupplyRecord, ARRegister, ARTran, ARTax>
	{
		private readonly SupplyRecordBuilderByCustomerData _supplyRecordBuilderByCustomerData;
		private readonly SupplyRecordCountryBuilderBase<TExtendedInvoice> _countryBuilder;

		protected SupplyRecordBuilderByInvoiceBase(IGAFRepository gafRepository,
			GafRecordBuilderByRegister recordBuilderByRegister,
			SupplyRecordBuilderByCustomerData supplyRecordBuilderByCustomerData,
			SupplyRecordCountryBuilderBase<TExtendedInvoice> countryBuilder) : base(gafRepository, recordBuilderByRegister)
		{
			_supplyRecordBuilderByCustomerData = supplyRecordBuilderByCustomerData;
			_countryBuilder = countryBuilder;
		}

		public SupplyRecord Build(TExtendedInvoice extendedInvoice, ARRegister register, ARTran tran, ARTax tranTax, Customer customer, int lineNumber)
		{
			var supplyRecord = BuildInternal(register, tran, tranTax, lineNumber);

			_supplyRecordBuilderByCustomerData.Build(supplyRecord, register, customer);

			_countryBuilder.Build(supplyRecord, extendedInvoice);

			return supplyRecord;
		}
	}
}
