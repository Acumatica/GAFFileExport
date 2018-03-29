using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.Tests.AR.Builders;
using PX.Objects.Tests.AR.DataContexts;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using Xunit;

namespace PX.Objects.GAF.Tests.ARinvoice
{
	public abstract class ARInvoiceGAFRecordsCreatorTestsBase : InvoiceGAFRecordsCreatorTestsBase<ARInvoiceAggregate, AR.ARInvoice, ARTran, ARTax, SupplyRecord, ARTaxDataBuilder>
	{
		protected CustomerDataContext CustomerDataContext;
		protected ARInvoiceAggregateBuilderFactory ArInvoiceAggregateBuilderFactory;
		protected ARAddressDataContext ArAddressDataContext;
		protected CountryDataContext CountryDataContext;

		public ARInvoiceGAFRecordsCreatorTestsBase()
		{
			CustomerDataContext = GetService<CustomerDataContext>();
			ArInvoiceAggregateBuilderFactory = GetService<ARInvoiceAggregateBuilderFactory>();
			ArAddressDataContext = GetService<ARAddressDataContext>();
			CountryDataContext = GetService<CountryDataContext>();
		}		

		public void Test_CreateGAFRecordsForDocumentGroup_For_Documents_With_Two_Tax_And_Custom_LineNumbers(string module, Action additionalSetup = null)
		{
			//Arrange
			var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
			{
				TaxDataContext.VatTax.TaxID,
				TaxDataContext.VatTax2.TaxID
			});

			var taxAgencyID =VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID =TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var invoiceAggr1 = new ARInvoiceAggregateBuilder()
									.CreateDocument(ARDocType.Invoice,
													refNbr: DocumentDataContext.RefNbr,
													docDate: DocumentDataContext.DocDate,
													curyID: DocumentDataContext.CuryIDEUR,
													module: module)
									.DocumentWith(customerID: CustomerDataContext.Customer.BAccountID,
													customerLocationID: LocationDataContext.CustomerLocation.LocationID,
													billingAddressID: ArAddressDataContext.CustomerAddress.AddressID)
									.AddTran(100, 200, "tyre sale 1", lineNbr: 3, taxDataItems: taxDataItems)
									.AddTran(300, 600, "oil sale 1", lineNbr: 7, taxDataItems: taxDataItems)
									.Build();

			var invoiceAggr2 = new ARInvoiceAggregateBuilder()
									.CreateDocument(ARDocType.Invoice,
													refNbr: DocumentDataContext.RefNbr2,
													docDate: DocumentDataContext.DocDate2,
													curyID: DocumentDataContext.CuryIDGBP,
													module: module)
									.DocumentWith(customerID: CustomerDataContext.Customer2.BAccountID,
													customerLocationID: LocationDataContext.Customer2Location.LocationID,
													billingAddressID: ArAddressDataContext.Customer2Address.AddressID)
									.AddTran(200, 400, "tyre sale 2", lineNbr: 4, taxDataItems: taxDataItems)
									.AddTran(500, 700, "oil sale 2", lineNbr: 9, taxDataItems: taxDataItems)
									.Build();

			var invoiceAggregs = new[] { invoiceAggr1, invoiceAggr2 };

			SetupRepositoryMethods(invoiceAggregs, invoiceAggr1.Document.OrigModule, invoiceAggr1.Document.DocType, taxAgencyID, taxPeriodID);

			if (additionalSetup != null)
				additionalSetup();

			var documentGroup = new DocumentGroup<AR.ARInvoice>()
			{
				Module = invoiceAggr1.Document.OrigModule,
				DocumentType = invoiceAggr1.Document.DocType,
				DocumentsByRefNbr = invoiceAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var supplyRecords = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID,
				taxPeriodID);

			//Assert
			Approvals.VerifyAll(supplyRecords, "supplyRecords", record => record.Dump());
		}

		public void Test_CreateGAFRecordsForDocumentGroup_That_Country_Field_Is_Null_When_Sale_Country_Is_Malaysia(string module, Action additionalSetup = null)
		{
			//Arrange
			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var invoiceAggr = ArInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(ARDocType.Invoice, withTax: true,module: module)
																	.Build();

			var invoiceAggregs = invoiceAggr.SingleToArray();

			SetupRepositoryMethods(invoiceAggregs, invoiceAggr.Document.OrigModule, invoiceAggr.Document.DocType, taxAgencyID, taxPeriodID);

			if (additionalSetup != null)
				additionalSetup();

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
			Assert.Null(supplyRecord.Country);
		}

		protected void Test_CreateGAFRecordsForDocumentGroup_When_Tax_Calced_On_Document_Amount(string module, Action additionalSetup = null)
		{
			//Arrange
			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;
			var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
			{
				TaxDataContext.VatTax.TaxID,
			});
			var taxDataItemOfTaxCalcedOnDocAmt = TaxDataBuilder.CreateTaxDataItems(new[]
																					{
																						TaxDataContext.VatTaxCalcedOnDocAmtAmt.TaxID
																					})
																.Single();

			const decimal taxableAmt = 50;
			const decimal taxAmt = 3.5m;
			const decimal curyTaxableAmt = 101;
			const decimal curyTaxAmt = 7.1m;

			var documentAggr = ArInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(ARDocType.Invoice, lineCount: 1, taxDataItems: taxDataItems, module: module)
																.AddTaxTran(taxDataItemOfTaxCalcedOnDocAmt.TaxID,
																			taxDataItemOfTaxCalcedOnDocAmt.TranTaxType,
																			taxableAmt, taxAmt, curyTaxableAmt, curyTaxAmt)
																.Build();

			var documentAggregs = documentAggr.SingleToArray();

			SetupRepositoryMethods(documentAggregs, documentAggr.Document.OrigModule, documentAggr.Document.DocType, taxAgencyID, taxPeriodID);

			if (additionalSetup != null)
				additionalSetup();

			var documentGroup = new DocumentGroup<AR.ARInvoice>()
			{
				Module = documentAggr.Document.OrigModule,
				DocumentType = documentAggr.Document.DocType,
				DocumentsByRefNbr = documentAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var gafRecords = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup,
																						taxAgencyID, taxPeriodID)
														.Last();

			//Assert
			Assert.Equal(taxDataItemOfTaxCalcedOnDocAmt.TaxID, gafRecords.TaxCode);
			Assert.Equal(DocumentDataContext.DocDesc, gafRecords.ProductDescription);
			Assert.Equal(DocumentDataContext.RefNbr, gafRecords.InvoiceNumber);
			Assert.Equal(DocumentDataContext.CuryIDEUR, gafRecords.ForeignCurrencyCode);
			Assert.Equal(ContactDataContext.CustomerLocationContact.FullName, gafRecords.CustomerName);
			Assert.Equal(CustomerDataContext.Customer.AcctReferenceNbr, gafRecords.CustomerBRN);
			Assert.Equal(CountryDataContext.CountryDE.Description, gafRecords.Country);
			Assert.Equal(2, gafRecords.LineNumber);
			Assert.Equal(taxableAmt, gafRecords.Amount);
			Assert.Equal(taxAmt, gafRecords.GSTAmount);
			Assert.Equal(curyTaxableAmt, gafRecords.ForeignCurrencyAmount);
			Assert.Equal(curyTaxAmt, gafRecords.ForeignCurrencyAmountGST);
		}


		#region Service

		protected override void SetupRepositoryMethods(ICollection<ARInvoiceAggregate> arInvoiceAggregs, string module, string docType, int? taxAgencyID, string taxPeriodID)
		{
			var refNbrs = arInvoiceAggregs.Select(apInvoiceAggr => apInvoiceAggr.Document.RefNbr)
										   .ToArray();

			var taxTrans =
				arInvoiceAggregs.SelectMany(
					apInvoiceAggr => apInvoiceAggr.TaxTransByTax.Values.SelectMany(list => list.Select(tran => tran)));

			GAFRepositoryMock.Setup(repo => repo.GetReportedTaxTransForDocuments(module, docType, refNbrs, taxAgencyID, taxPeriodID))
				.Returns(taxTrans);

			SetupGetARTranWithARTaxForDocuments(docType, refNbrs, taxAgencyID, taxPeriodID, arInvoiceAggregs);

			GAFRepositoryMock.Setup(repo => repo.GetReportedTaxAPRegistersWithTaxTransForDocuments(docType, refNbrs, taxAgencyID))
				.Returns(new PXResult<APRegister, TaxTran>[] { });
		}

		protected void SetupGetARTranWithARTaxForDocuments(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID, IEnumerable<ARInvoiceAggregate> arInvoiceAggregates)
		{
			var mockResult = new List<PXResult<ARTran, ARTax>>();

			foreach (var apInvoiceAggregate in arInvoiceAggregates)
			{
				foreach (var apTranKvp in apInvoiceAggregate.TransByLineNbr)
				{
					if (!apInvoiceAggregate.TranTaxesByLineNbrAndTax.ContainsKey(apTranKvp.Key))
						continue;

					foreach (var apTaxKvp in apInvoiceAggregate.TranTaxesByLineNbrAndTax[apTranKvp.Key])
					{
						mockResult.Add(new PXResult<ARTran, ARTax>(apTranKvp.Value, apTaxKvp.Value));
					}
				}
			}

			GAFRepositoryMock.Setup(repo => repo.GetReportedARTransWithARTaxesForTaxCalcedOnItem(docType, refNbrs, taxAgencyID, taxPeriodID))
				.Returns(mockResult);
		}

		#endregion
	}
}
