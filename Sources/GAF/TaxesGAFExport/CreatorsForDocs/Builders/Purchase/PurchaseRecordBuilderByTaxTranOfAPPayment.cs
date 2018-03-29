using PX.Objects.AP;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Purchase
{
	public class PurchaseRecordBuilderByTaxTranOfAPPayment: GafRecordBuilderByRegisterAndTaxTranBase<PurchaseRecord, APRegister>
	{
		private readonly PurchaseRecordBuilderByVendorData _recordBuilderByVendorData;

		public PurchaseRecordBuilderByTaxTranOfAPPayment(IGAFRepository gafRepository,
			PurchaseRecordBuilderByVendorData recordBuilderByVendorData,
			GafRecordBuilderByRegister gafRecordBuilderByRegister)
			: base(gafRepository, gafRecordBuilderByRegister)
		{
			_recordBuilderByVendorData = recordBuilderByVendorData;
		}

		public PurchaseRecord Build(APRegister apRegister, TaxTran taxTran, Vendor vendor, int lineNumber, string adjdDocumentDesc)
		{
			var purchaseRecord = BuildInternal(apRegister, taxTran, lineNumber);

			_recordBuilderByVendorData.Build(purchaseRecord, apRegister, vendor);

			purchaseRecord.ProductDescription = adjdDocumentDesc;

			return purchaseRecord;
		}
	}
}
