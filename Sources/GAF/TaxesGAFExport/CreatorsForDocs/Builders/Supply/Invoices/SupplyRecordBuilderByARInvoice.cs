using PX.Objects.AR;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices
{
	public class SupplyRecordBuilderByARInvoice : SupplyRecordBuilderByInvoiceBase<ARInvoice>
	{
		public SupplyRecordBuilderByARInvoice(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister,
			SupplyRecordBuilderByCustomerData supplyRecordBuilderByCustomerData,
			SupplyRecordCountryBuilderForARInvoice countryBuilder)
			: base(gafRepository, gafRecordBuilderByRegister, supplyRecordBuilderByCustomerData, countryBuilder)
		{
		}
	}
}
