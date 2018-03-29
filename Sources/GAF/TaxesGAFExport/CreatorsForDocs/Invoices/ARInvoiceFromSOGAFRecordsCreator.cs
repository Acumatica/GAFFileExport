using System.Collections.Generic;
using System.Linq;
using PX.Objects.AR;
using PX.Objects.Common.Documents;
using PX.Objects.SO;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Invoices
{
	public class ARInvoiceFromSOGAFRecordsCreator : ARInvoiceGAFRecordsCreatorBase<SupplyRecordBuilderBySOInvoice, SOInvoice>
	{
		private readonly SupplyRecordBuilderBySOInvoiceTaxTranForTaxCalcedOnDocumentAmt _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;

		private Dictionary<string, SOInvoice> SOInvoices;

		public ARInvoiceFromSOGAFRecordsCreator(IGAFRepository gafRepository, 
												SupplyRecordBuilderBySOInvoice supplyRecordBuilder,
												SupplyRecordBuilderBySOInvoiceTaxTranForTaxCalcedOnDocumentAmt recordBuilderByTaxTranForTaxCalcedOnDocumentAmt)
			: base(gafRepository, supplyRecordBuilder)
		{
			_recordBuilderByTaxTranForTaxCalcedOnDocumentAmt = recordBuilderByTaxTranForTaxCalcedOnDocumentAmt;
		}

		protected override void LoadData(DocumentGroup<ARInvoice> documentGroup, int? taxAgencyID, string taxPeriodID)
		{
			base.LoadData(documentGroup, taxAgencyID, taxPeriodID);

			SOInvoices = GafRepository.GetSOInvoices(documentGroup.DocumentType, documentGroup.DocumentsByRefNbr.Keys.ToArray())
										.ToDictionary(soInvoice => soInvoice.RefNbr, soInvoice => soInvoice);
		}

		protected override SupplyRecord BuildGafRecordByTaxTranCalcedOnDocumentAmt(ARInvoice document, TaxTran taxTran, int gafLineNumber)
		{
			return _recordBuilderByTaxTranForTaxCalcedOnDocumentAmt.Build(GetExtendedInvoice(document), document, taxTran,
				Customers[document.CustomerID], gafLineNumber);
		}

		protected override SOInvoice GetExtendedInvoice(ARInvoice invoice)
		{
			return SOInvoices[invoice.RefNbr];
		}
	}
}