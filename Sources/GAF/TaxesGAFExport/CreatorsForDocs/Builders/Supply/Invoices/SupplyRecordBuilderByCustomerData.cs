using PX.Objects.AR;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices
{
	public class SupplyRecordBuilderByCustomerData : IGafRecordBuilderByContragentData<SupplyRecord, ARRegister, Customer>
	{
		private readonly IGAFRepository _gafRepository;

		public SupplyRecordBuilderByCustomerData(IGAFRepository gafRepository)
		{
			_gafRepository = gafRepository;
		}

		public void Build(SupplyRecord gafRecord, ARRegister register, Customer customer)
		{
			gafRecord.CustomerBRN = customer.AcctReferenceNbr;

			var location = _gafRepository.GetLocation(register.CustomerID, register.CustomerLocationID);
			var contact = _gafRepository.GetContactByID(location.DefContactID);

			gafRecord.CustomerName = contact.FullName;
		}
	}
}
