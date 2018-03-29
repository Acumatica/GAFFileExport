using PX.Objects.AP;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Purchase
{
	public class PurchaseRecordBuilderByInvoiceTran : GafRecordBuilderByDocumentTranBase<PurchaseRecord, APRegister, APTran, APTax>
	{
		private readonly PurchaseRecordBuilderByVendorData _recordBuilderByVendorData;

		public PurchaseRecordBuilderByInvoiceTran(IGAFRepository gafRepository,
			PurchaseRecordBuilderByVendorData recordBuilderByVendorData, 
			GafRecordBuilderByRegister recordBuilderByRegister)
			: base(gafRepository, recordBuilderByRegister)
		{
			_recordBuilderByVendorData = recordBuilderByVendorData;
		}

		public PurchaseRecord Build(APRegister register, APTran tran, APTax tranTax, Vendor vendor, int lineNumber)
		{
			var purchaseRecord = BuildInternal(register, tran, tranTax, lineNumber);

			_recordBuilderByVendorData.Build(purchaseRecord, register, vendor);

			return purchaseRecord;
		}
	}
}
