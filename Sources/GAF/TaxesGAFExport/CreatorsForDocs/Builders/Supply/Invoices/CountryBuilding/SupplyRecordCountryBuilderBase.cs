using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding
{
	public abstract class SupplyRecordCountryBuilderBase<TExtendedInvoice>
	{
		public const string MalaysiaCountryCode = "MY";

		protected readonly IGAFRepository GafRepository;

		protected SupplyRecordCountryBuilderBase(IGAFRepository gafRepository)
		{
			GafRepository = gafRepository;
		}

		public void Build(SupplyRecord supplyRecord, TExtendedInvoice extendedInvoice)
		{
			var saleCountryID = GetSaleCountryID(extendedInvoice);

			if (saleCountryID != MalaysiaCountryCode)
			{
				var country = GafRepository.GetCountry(saleCountryID);

				supplyRecord.Country = country.Description;
			}
		}

		protected abstract string GetSaleCountryID(TExtendedInvoice extendedInvoice);
	}
}
