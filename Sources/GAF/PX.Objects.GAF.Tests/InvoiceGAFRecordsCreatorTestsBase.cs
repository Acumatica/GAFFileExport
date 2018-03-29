using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.CM;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.Tests.AP.DataContexts;
using PX.Objects.Tests.Common.Builders;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.TX.Builders;
using PX.Objects.Tests.TX.DataContexts;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using TaxesGAFExport.Data.Records;
using Xunit;

namespace PX.Objects.GAF.Tests
{
	public abstract class InvoiceGAFRecordsCreatorTestsBase<TInvoiveAggregate, TInvoice, TTran, TTranTax, TGafRecord, TTaxDataBuilder>: GAFTestsBase
		where TInvoiveAggregate: IInvoiceAggregate<TInvoice, TTran, TTranTax>
		where TInvoice : IInvoice
		where TGafRecord: GAFRecordBase
		where TTaxDataBuilder: TaxDataBuilderBase
		where TTranTax: ITranTax
	{ 
		protected VendorDataContext VendorDataContext;
		protected TaxPeriodDataContext TaxPeriodDataContext;
		protected LocationDataContext LocationDataContext;
		protected ContactDataContext ContactDataContext;

		protected TTaxDataBuilder TaxDataBuilder;

		protected IDocumentGafRecordCreator<TInvoice, TGafRecord> InvoiceGafRecordsCreator;

		protected InvoiceGAFRecordsCreatorTestsBase()
		{
			VendorDataContext = GetService<VendorDataContext>();
			TaxPeriodDataContext = GetService<TaxPeriodDataContext>();
			TaxDataBuilder = GetService<TTaxDataBuilder>();
			LocationDataContext = GetService<LocationDataContext>();
			ContactDataContext = GetService<ContactDataContext>();
		} 

		protected void Test_CreateGAFRecordsForDocumentGroup_TaxTran_Amounts_Sign_For_Adjustments_And_Memos(Func<TInvoiveAggregate> getInvoiceAggregate,
			decimal expectedSign)
		{
			//Arrange
			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var adjAggr = getInvoiceAggregate();

			var adjAggregs = adjAggr.SingleToArray();

			SetupRepositoryMethods(adjAggregs, adjAggr.Document.OrigModule, adjAggr.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<TInvoice>()
			{
				Module = adjAggr.Document.OrigModule,
				DocumentType = adjAggr.Document.DocType,
				DocumentsByRefNbr = adjAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var gafRecords = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, 
																						taxAgencyID, taxPeriodID)
														.Single();

			//Assert
			Assert.Equal(expectedSign * InvoiceTranDataContext.TranAmt, gafRecords.Amount);
			Assert.Equal(expectedSign * InvoiceTranDataContext.VatTaxTaxAmt, gafRecords.GSTAmount);
			Assert.Equal(expectedSign * InvoiceTranDataContext.CuryTranAmt, gafRecords.ForeignCurrencyAmount);
			Assert.Equal(expectedSign * InvoiceTranDataContext.VatTaxCuryTaxAmt, gafRecords.ForeignCurrencyAmountGST);
		}

		public void Test_CreateGAFRecordsForDocumentGroup_That_Discrepancy_Is_Compensated(Func<TInvoiveAggregate> getInvoiceAggregate, decimal discrepancySign)
		{
			//Arrange
			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var billAggr = getInvoiceAggregate();

			var taxIDForDiscrepancy = TaxDataContext.VatTax.TaxID;

			var taxTran = billAggr.TaxTransByTax[taxIDForDiscrepancy].Single();

			taxTran.TaxableAmt += discrepancySign * 1;
			taxTran.TaxAmt += discrepancySign * 2;
			taxTran.CuryTaxableAmt += discrepancySign * 3;
			taxTran.CuryTaxAmt += discrepancySign * 4;

			var taxableAmtWithDiscrepancy = taxTran.TaxableAmt;
			var taxAmtWithDiscerpancy = taxTran.TaxAmt;
			var curyTaxableAmtWithDiscerpancy = taxTran.CuryTaxableAmt;
			var curyTaxAmtWithDiscerpancy = taxTran.CuryTaxAmt;

			var billAggregs = new[] { billAggr };

			SetupRepositoryMethods(billAggregs, billAggr.Document.OrigModule, billAggr.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<TInvoice>()
			{
				Module = billAggr.Document.OrigModule,
				DocumentType = billAggr.Document.DocType,
				DocumentsByRefNbr = billAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var gafRecords = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID, taxPeriodID);

			//Assert
			var recordWithCompensatedDiscrepancy = gafRecords.Single(record => record.TaxCode == taxIDForDiscrepancy && record.LineNumber == 2);

			Assert.Equal(taxableAmtWithDiscrepancy - InvoiceTranDataContext.TranAmt, recordWithCompensatedDiscrepancy.Amount);
			Assert.Equal(taxAmtWithDiscerpancy - InvoiceTranDataContext.VatTaxTaxAmt, recordWithCompensatedDiscrepancy.GSTAmount);
			Assert.Equal(curyTaxableAmtWithDiscerpancy - InvoiceTranDataContext.CuryTranAmt, recordWithCompensatedDiscrepancy.ForeignCurrencyAmount);
			Assert.Equal(curyTaxAmtWithDiscerpancy - InvoiceTranDataContext.VatTaxCuryTaxAmt, recordWithCompensatedDiscrepancy.ForeignCurrencyAmountGST);


			var recordsWithoutCompensatedDiscrepancy = gafRecords.Except(recordWithCompensatedDiscrepancy.SingleToArray());

			foreach (var record in recordsWithoutCompensatedDiscrepancy)
			{
				var tranTax = billAggr.TranTaxesByLineNbrAndTax[record.LineNumber][record.TaxCode];

				Assert.Equal(tranTax.TaxableAmt, record.Amount);
				Assert.Equal(tranTax.TaxAmt, record.GSTAmount);
				Assert.Equal(tranTax.CuryTaxableAmt, record.ForeignCurrencyAmount);
				Assert.Equal(tranTax.CuryTaxAmt, record.ForeignCurrencyAmountGST);
			}
		}

		protected abstract void SetupRepositoryMethods(ICollection<TInvoiveAggregate> invoiceAggregs, string module, string docType,
			int? taxAgencyID, string taxPeriodID);
	}
}
