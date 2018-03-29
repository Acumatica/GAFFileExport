using PX.Objects.AR;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Invoices
{
	public class ARInvoiceGAFRecordsCreator : ARInvoiceGAFRecordsCreatorBase<SupplyRecordBuilderByARInvoice, ARInvoice>
	{
		private readonly SupplyRecordBuilderByARInvoiceTaxTranForTaxCalcedOnDocumentAmt _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;

		public ARInvoiceGAFRecordsCreator(IGAFRepository gafRepository, 
											SupplyRecordBuilderByARInvoice supplyRecordBuilder,
											SupplyRecordBuilderByARInvoiceTaxTranForTaxCalcedOnDocumentAmt recordBuilderByTaxTranForTaxCalcedOnDocumentAmt)
			: base(gafRepository, supplyRecordBuilder)
		{
			_recordBuilderByTaxTranForTaxCalcedOnDocumentAmt = recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;
		}

		protected override ARInvoice GetExtendedInvoice(ARInvoice invoice)
		{
			return invoice;
		}

		protected override SupplyRecord BuildGafRecordByTaxTranCalcedOnDocumentAmt(ARInvoice document, TaxTran taxTran, int gafLineNumber)
		{
			return _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt.Build(document, taxTran, Customers[document.CustomerID], gafLineNumber);
		}
	}
}