using PX.Objects.AP;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Purchase
{
	public class PurchaseRecordBuilderByAPInvoiceTaxTranForTaxCalcedOnDocumentAmt : GafRecordBuilderByTaxTranCalcedOnDocumentAmt<PurchaseRecord,APRegister>
	{
		private readonly PurchaseRecordBuilderByVendorData _recordBuilderByVendorData;

		public PurchaseRecordBuilderByAPInvoiceTaxTranForTaxCalcedOnDocumentAmt(IGAFRepository gafRepository,
			GafRecordBuilderByRegister gafRecordBuilderByRegister,
			PurchaseRecordBuilderByVendorData recordBuilderByVendorData) : base(gafRepository, gafRecordBuilderByRegister)
		{
			_recordBuilderByVendorData = recordBuilderByVendorData;
		}

		public PurchaseRecord Build(APRegister register, TaxTran taxTran, Vendor vendor, int gafLineNumber)
		{
			var purchaseRecord = BuildInternal(register, taxTran, gafLineNumber);
			_recordBuilderByVendorData.Build(purchaseRecord, register, vendor);

			return purchaseRecord;
		}
	}
}
