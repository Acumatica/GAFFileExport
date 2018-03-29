using PX.Objects.SO;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding
{
	public class SupplyRecordCountryBuilderForSOInvoice: SupplyRecordCountryBuilderBase<SOInvoice>
	{
		public SupplyRecordCountryBuilderForSOInvoice(IGAFRepository gafRepository) : base(gafRepository)
		{
		}

		protected override string GetSaleCountryID(SOInvoice extendedInvoice)
		{
			var address = GafRepository.GetSOAddress(extendedInvoice.ShipAddressID);

			return address.CountryID;
		}
	}
}
