using PX.Objects.AP;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Purchase
{
	public class PurchaseRecordBuilderByVendorData: IGafRecordBuilderByContragentData<PurchaseRecord, APRegister, Vendor>
	{
		private readonly IGAFRepository _gafRepository;

		public PurchaseRecordBuilderByVendorData(IGAFRepository gafRepository)
		{
			_gafRepository = gafRepository;
		}

		public void Build(PurchaseRecord gafRecord, APRegister register, Vendor vendor)
		{
			var location = _gafRepository.GetLocation(register.VendorID, register.VendorLocationID);
			var contact = _gafRepository.GetContactByID(location.DefContactID);

			gafRecord.SupplierName = contact.FullName;
			gafRecord.SupplierBRN = vendor.AcctReferenceNbr;
		}
	}
}
