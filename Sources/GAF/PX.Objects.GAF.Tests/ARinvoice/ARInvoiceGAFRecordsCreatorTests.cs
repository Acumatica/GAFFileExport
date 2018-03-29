using System;
using System.Linq;
using ApprovalTests.Reporters;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.Tests.AR.Builders;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.Infrastucture.ApprovalTests;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using Xunit;

namespace PX.Objects.GAF.Tests.ARinvoice
{
	public class ARInvoiceGAFRecordsCreatorTests : ARInvoiceGAFRecordsCreatorTestsBase
	{
		public ARInvoiceGAFRecordsCreatorTests()
		{
			InvoiceGafRecordsCreator = new ARInvoiceGAFRecordsCreator(GAFRepository,
																		new SupplyRecordBuilderByARInvoice(GAFRepository, 
																											new GafRecordBuilderByRegister(GAFRepository),
																											new SupplyRecordBuilderByCustomerData(GAFRepository),
																											new SupplyRecordCountryBuilderForARInvoice(GAFRepository)),
																		new SupplyRecordBuilderByARInvoiceTaxTranForTaxCalcedOnDocumentAmt(GAFRepository,
																																new GafRecordBuilderByRegister(GAFRepository),
																																new SupplyRecordBuilderByCustomerData(GAFRepository),
																																new SupplyRecordCountryBuilderForARInvoice(GAFRepository)));
		}

		[Fact]
		[UseReporter(typeof(DiffReporter))]
		[UseApprovalFileName("__AR__CreateGAFRecordsForDocumentGroup__Two_Taxes__")]
		public void Test_CreateGAFRecordsForDocumentGroup_For_Documents_With_Two_Taxes_And_Custom_LineNumbers()
		{
			Test_CreateGAFRecordsForDocumentGroup_For_Documents_With_Two_Tax_And_Custom_LineNumbers(BatchModule.AR);
		}

		[Theory]
		[InlineData(ARDocType.CreditMemo, -1)]
		[InlineData(ARDocType.DebitMemo, 1)]
		public void Test_CreateGAFRecordsForDocumentGroup_TaxTran_Amounts_Sign_For_Memo(string docType, decimal expectedSign)
		{
			//Arrange
			Func<ARInvoiceAggregate> getInvoiceAggregate =
				() => ArInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(docType, withTax: true)
					.Build();

			Test_CreateGAFRecordsForDocumentGroup_TaxTran_Amounts_Sign_For_Adjustments_And_Memos(getInvoiceAggregate, expectedSign);
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		public void Test_CreateGAFRecordsForDocumentGroup_That_Discrepancy_Is_Compensated(decimal discrepancySign)
		{
			//Arrange
			Func<ARInvoiceAggregate> getInvoiceAggregate = () =>
			{
				var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
				{
					TaxDataContext.VatTax.TaxID,
					TaxDataContext.VatTax2.TaxID
				});

				return ArInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(ARDocType.Invoice, lineCount: 2, taxDataItems: taxDataItems)
														.Build();
			};

			Test_CreateGAFRecordsForDocumentGroup_That_Discrepancy_Is_Compensated(getInvoiceAggregate, discrepancySign);
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Document_Is_In_Base_Cury()
		{
			//Arrange
			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
			{
				TaxDataContext.VatTax.TaxID,
			});

			var invoiceAggr = new ARInvoiceAggregateBuilder()
								.CreateDocument(APDocType.Invoice,
												DocumentDataContext.RefNbr,
												DocumentDataContext.DocDate,
												CompanyDataContext.Company.BaseCuryID)
								.DocumentWith(customerID: CustomerDataContext.Customer.BAccountID,
												customerLocationID: LocationDataContext.CustomerLocation.LocationID,
												billingAddressID: ArAddressDataContext.CustomerAddress.AddressID)
								.AddTran(InvoiceTranDataContext.TranAmt, InvoiceTranDataContext.CuryTranAmt, taxDataItems: taxDataItems)
								.Build();

			var invoiceAggregs = invoiceAggr.SingleToArray();

			SetupRepositoryMethods(invoiceAggregs, invoiceAggr.Document.OrigModule, invoiceAggr.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<AR.ARInvoice>()
			{
				Module = invoiceAggr.Document.OrigModule,
				DocumentType = invoiceAggr.Document.DocType,
				DocumentsByRefNbr = invoiceAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var supplyRecord = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID, taxPeriodID)
																.Single();

			//Assert
			Assert.Equal(InvoiceTranDataContext.TranAmt, supplyRecord.Amount);
			Assert.Equal(InvoiceTranDataContext.VatTaxTaxAmt, supplyRecord.GSTAmount);
			Assert.Equal(ForeignCurrencyCodeForDocumentInBaseCury, supplyRecord.ForeignCurrencyCode);
			Assert.Equal(0, supplyRecord.ForeignCurrencyAmount);
			Assert.Equal(0, supplyRecord.ForeignCurrencyAmountGST);
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_That_Country_Field_Is_Null_When_Country_Of_Billing_Address_Is_Malaysia()
		{
			Action setup = delegate()
			{
				GAFRepositoryMock.Setup(repo => repo.GetARAddress(ArAddressDataContext.CustomerAddress.AddressID))
					.Returns(new ARAddress() {CountryID = SupplyRecordCountryBuilderForARInvoice.MalaysiaCountryCode});
			};

			Test_CreateGAFRecordsForDocumentGroup_That_Country_Field_Is_Null_When_Sale_Country_Is_Malaysia(BatchModule.AR, setup);
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Tax_Calced_On_Document_Amount()
		{
			Test_CreateGAFRecordsForDocumentGroup_When_Tax_Calced_On_Document_Amount(BatchModule.AR);
		}
	}
}
