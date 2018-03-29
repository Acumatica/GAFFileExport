using PX.Objects.SO;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices
{
	public class SupplyRecordBuilderBySOInvoice : SupplyRecordBuilderByInvoiceBase<SOInvoice>
	{
		public SupplyRecordBuilderBySOInvoice(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister,
			SupplyRecordBuilderByCustomerData supplyRecordBuilderByCustomerData,
			SupplyRecordCountryBuilderForSOInvoice countryBuilder)
			: base(gafRepository, gafRecordBuilderByRegister, supplyRecordBuilderByCustomerData, countryBuilder)
		{
		}
	}
}
