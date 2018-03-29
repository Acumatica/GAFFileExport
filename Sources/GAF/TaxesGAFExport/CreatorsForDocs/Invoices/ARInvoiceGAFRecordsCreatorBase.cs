using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common.Documents;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices;
using TaxesGAFExport.Data;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Invoices
{
	public abstract class ARInvoiceGAFRecordsCreatorBase<TSupplyRecordBulder, TExtendedInvoice> : DocumentWithTransGAFRecordsCreatorBase<ARInvoice, ARTran, ARTax, SupplyRecord>
		where TSupplyRecordBulder: SupplyRecordBuilderByInvoiceBase<TExtendedInvoice>
	{
		protected Dictionary<int?, Customer> Customers;

		private readonly TSupplyRecordBulder _supplyRecordBuilder;

		protected override GAFRecordType ExpectedGafRecordType
		{
			get { return GAFRecordType.Supply; }
		}

		protected ARInvoiceGAFRecordsCreatorBase(IGAFRepository gafRepository, TSupplyRecordBulder supplyRecordBuilder)
			: base(gafRepository)
		{
			_supplyRecordBuilder = supplyRecordBuilder;
		}

		protected override void LoadData(DocumentGroup<ARInvoice> documentGroup, int? taxAgencyID, string taxPeriodID)
		{
			base.LoadData(documentGroup, taxAgencyID, taxPeriodID);

			Customers = GetContragentsForDocuments(DocumentsByRefNbr.Values);
		}

		protected override IEnumerable<PXResult<ARTran, ARTax>> GetReportedTransWithTranTaxesForTaxCalcedOnItem(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID)
		{
			return GafRepository.GetReportedARTransWithARTaxesForTaxCalcedOnItem(docType, refNbrs, taxAgencyID, taxPeriodID);
		}

		protected Dictionary<int?, Customer> GetContragentsForDocuments(IEnumerable<ARInvoice> documents)
		{
			var usedCustomerIDs = documents.Select(apReg => apReg.CustomerID)
											.Distinct()
											.ToArray();

			return GafRepository.GetCustomersByIDs(usedCustomerIDs)
									.ToDictionary(customer => customer.BAccountID, customer => customer);
		}

		protected override SupplyRecord BuildGafRecordByDocumentTran(ARInvoice document, ARTran tran, ARTax tranTax, int lineNumberForGaf)
		{
			return _supplyRecordBuilder.Build(GetExtendedInvoice(document), document, tran, tranTax, Customers[document.CustomerID], lineNumberForGaf);
		}

		protected abstract TExtendedInvoice GetExtendedInvoice(ARInvoice invoice);
	}
}