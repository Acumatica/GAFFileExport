using PX.Objects.AR;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding
{
	public class SupplyRecordCountryBuilderForARInvoice : SupplyRecordCountryBuilderBase<ARInvoice>
	{
		public SupplyRecordCountryBuilderForARInvoice(IGAFRepository gafRepository) : base(gafRepository)
		{
		}

		protected override string GetSaleCountryID(ARInvoice extendedInvoice)
		{
			var address = GafRepository.GetARAddress(extendedInvoice.BillAddressID);

			return address.CountryID;
		}
	}
}
