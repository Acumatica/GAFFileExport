using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.GL;
using PX.Objects.Tests.AP.Builders;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.Infrastucture.ApprovalTests;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.CreatorsForDocs.Builders.Purchase;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using TaxesGAFExport.Data.Records;
using Xunit;

namespace PX.Objects.GAF.Tests.APInvoice
{
	public class APInvoiceGAFRecordsCreatorTests : InvoiceGAFRecordsCreatorTestsBase<APInvoiceAggregate, AP.APInvoice, APTran, APTax, PurchaseRecord, APTaxDataBuilder>
	{
		private APInvoiceAggregateBuilderFactory _apInvoiceAggregateBuilderFactory;

		public APInvoiceGAFRecordsCreatorTests()
		{
			var recordBuilderByVendorData = new PurchaseRecordBuilderByVendorData(GAFRepository);
			var recordBuilderByRegister = new GafRecordBuilderByRegister(GAFRepository);
			var recordBuilderByInvoiceTran = new PurchaseRecordBuilderByInvoiceTran(GAFRepository, recordBuilderByVendorData, recordBuilderByRegister);
			var recordBuilderByTaxTranFromTaxDocument = new PurchaseRecordBuilderByTaxTranFromTaxDocument(GAFRepository,
				recordBuilderByVendorData, recordBuilderByRegister);

			InvoiceGafRecordsCreator = new APInvoiceGAFRecordsCreator(GAFRepository, recordBuilderByInvoiceTran,
				recordBuilderByTaxTranFromTaxDocument,
				new PurchaseRecordBuilderByAPInvoiceTaxTranForTaxCalcedOnDocumentAmt(GAFRepository, recordBuilderByRegister,
					recordBuilderByVendorData));

			_apInvoiceAggregateBuilderFactory = GetService<APInvoiceAggregateBuilderFactory>();
		}

		[Theory]
		[InlineData(BatchModule.AP)]
		[InlineData(BatchModule.PO)]
		[UseReporter(typeof(DiffReporter))]
		[UseApprovalFileName("__CreateGAFRecordsForDocumentGroup__Two_Taxes__")]
		public void Test_CreateGAFRecordsForDocumentGroup_For_Documents_With_Two_Taxes_And_Custom_LineNumbers(string module)
		{
			//Arrange
			var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
			{
				TaxDataContext.VatTax.TaxID,
				TaxDataContext.VatTax2.TaxID
			});

			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var billAggr1 = new APInvoiceAggregateBuilder()
									.CreateDocument(
													APDocType.Invoice,
													refNbr: "DOC001",
													docDate: new DateTime(2015, 3, 1),
													curyID: "EUR",
													module: module)
									.DocumentWith(vendorID:VendorDataContext.Vendor.BAccountID,
													vendorLocationID: LocationDataContext.VendorLocation.LocationID)
									.AddTran(100, 200, "tyre purchase 1", lineNbr:3, taxDataItems: taxDataItems)
									.AddTran(300, 600, "oil purchase 1", lineNbr: 7, taxDataItems: taxDataItems)
									.Build();

			var billAggr2 = new APInvoiceAggregateBuilder()
									.CreateDocument(APDocType.Invoice,
													refNbr: "DOC002",
													docDate: new DateTime(2015, 3, 5),
													curyID: "GBP",
													module: module)
									.DocumentWith(vendorID: VendorDataContext.Vendor2.BAccountID,
													vendorLocationID: LocationDataContext.Vendor2Location.LocationID)
									.AddTran(200, 400, "tyre purchase 2", lineNbr: 4, taxDataItems: taxDataItems)
									.AddTran(500, 700, "oil purchase 2", lineNbr: 9, taxDataItems: taxDataItems)
									.Build();

			var billAggregs = new[] {billAggr1, billAggr2};

			SetupRepositoryMethods(billAggregs, billAggr1.Document.OrigModule, billAggr1.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<AP.APInvoice>()
			{
				Module = billAggr1.Document.OrigModule,
				DocumentType = billAggr1.Document.DocType,
				DocumentsByRefNbr = billAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecords = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID,
				taxPeriodID);

			//Assert
			Approvals.VerifyAll(purchaseRecords, "purchaseRecords", record => record.Dump());
		}

		[Theory]
		[InlineData(APDocType.DebitAdj, -1)]
		[InlineData(APDocType.CreditAdj, 1)]
		public void Test_CreateGAFRecordsForDocumentGroup_TaxTran_Amounts_Sign_For_Adjustments(string docType, decimal expectedSign)
		{
			//Arrange
			Func<APInvoiceAggregate> getInvoiceAggregate =
				() => _apInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(docType, withTax: true)
					.Build();

			Test_CreateGAFRecordsForDocumentGroup_TaxTran_Amounts_Sign_For_Adjustments_And_Memos(getInvoiceAggregate, expectedSign);
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		public void Test_CreateGAFRecordsForDocumentGroup_That_Discrepancy_Is_Compensated(decimal discrepancySign)
		{
			//Arrange
			Func<APInvoiceAggregate> getInvoiceAggregate = () =>
			{
				var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
				{
					TaxDataContext.VatTax.TaxID,
					TaxDataContext.VatTax2.TaxID
				});

				return _apInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(APDocType.Invoice, lineCount: 2, taxDataItems: taxDataItems)
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

			var billAggr = new APInvoiceAggregateBuilder()
								.CreateDocument(APDocType.Invoice,
												DocumentDataContext.RefNbr,
												DocumentDataContext.DocDate,
												CompanyDataContext.Company.BaseCuryID)
								.DocumentWith(vendorID: VendorDataContext.Vendor.BAccountID,
												vendorLocationID: LocationDataContext.VendorLocation.LocationID)
								.AddTran(InvoiceTranDataContext.TranAmt, InvoiceTranDataContext.CuryTranAmt, taxDataItems: taxDataItems)
								.Build();

			var billAggregs = billAggr.SingleToArray();

			SetupRepositoryMethods(billAggregs, billAggr.Document.OrigModule, billAggr.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<AP.APInvoice>()
			{
				Module = billAggr.Document.OrigModule,
				DocumentType = billAggr.Document.DocType,
				DocumentsByRefNbr = billAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecord = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID, taxPeriodID)
																.Single();

			//Assert
			Assert.Equal(InvoiceTranDataContext.TranAmt, purchaseRecord.Amount);
			Assert.Equal(InvoiceTranDataContext.VatTaxTaxAmt, purchaseRecord.GSTAmount);
			Assert.Equal(ForeignCurrencyCodeForDocumentInBaseCury, purchaseRecord.ForeignCurrencyCode);
			Assert.Equal(0, purchaseRecord.ForeignCurrencyAmount);
			Assert.Equal(0, purchaseRecord.ForeignCurrencyAmountGST);
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Tax_Calced_On_Document_Amount()
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

			var documentAggr = _apInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(APDocType.Invoice, lineCount: 1, taxDataItems: taxDataItems)
																.AddTaxTran(taxDataItemOfTaxCalcedOnDocAmt.TaxID,
																			taxDataItemOfTaxCalcedOnDocAmt.TranTaxType,
																			taxableAmt, taxAmt, curyTaxableAmt, curyTaxAmt)
																.Build();

			var documentAggregs = documentAggr.SingleToArray();

			SetupRepositoryMethods(documentAggregs, documentAggr.Document.OrigModule, documentAggr.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<AP.APInvoice>()
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
			Assert.Equal(ContactDataContext.VendorLocationContact.FullName, gafRecords.SupplierName);
			Assert.Equal(VendorDataContext.Vendor.AcctReferenceNbr, gafRecords.SupplierBRN);
			Assert.Equal(null, gafRecords.ImportDeclarationNumber);
			Assert.Equal(2, gafRecords.LineNumber);
			Assert.Equal(taxableAmt, gafRecords.Amount);
			Assert.Equal(taxAmt, gafRecords.GSTAmount);
			Assert.Equal(curyTaxableAmt, gafRecords.ForeignCurrencyAmount);
			Assert.Equal(curyTaxAmt, gafRecords.ForeignCurrencyAmountGST);
		}


		#region Tests for tax trans by tax documents

		[Theory]
		[InlineData(BatchModule.AP)]
		[InlineData(BatchModule.PO)]
		[UseReporter(typeof (DiffReporter))]
		[UseApprovalFileName("__CreateGAFRecordsForDocumentGroup__TaxTrans_By_Tax_Document")]
		public void Test_CreateGAFRecordsForDocumentGroup_For_Documents_With_Two_Tax_And_TaxTrans_By_Tax_Document(string module)
		{
			//Arrange
			var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
			{
				TaxDataContext.VatDirect.TaxID,
				TaxDataContext.VatDirect2.TaxID,
				TaxDataContext.VatTax.TaxID,//invalid - not direct tax, should not be exported
			});

			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var billRefNbr1 = "DOC001";

			var billAggr1 = new APInvoiceAggregateBuilder()
									.CreateDocument(APDocType.Invoice,
													refNbr: billRefNbr1,
													docDate: new DateTime(2015, 3, 1),
													curyID: "EUR",
													docDesc: "bill desc 1",
													module: module)
									.DocumentWith(vendorID: VendorDataContext.Vendor.BAccountID,
													vendorLocationID: LocationDataContext.VendorLocation.LocationID)
									.AddTaxTran(taxDataItems[0].TaxID, taxDataItems[0].TranTaxType, 10, 1, 20, 3)
									.AddTaxTran(taxDataItems[1].TaxID, taxDataItems[1].TranTaxType, 15, 2, 25, 5)
									.AddTaxTran(taxDataItems[2].TaxID, taxDataItems[2].TranTaxType)//should not be exported
									.Build();

			var taxBill1 = new APInvoiceAggregateBuilder()
									.CreateDocument(APDocType.Invoice,
													docDesc: "tax bill desc 1")
									.AddTaxTran(new TaxTran() { TaxID = taxDataItems[0].TaxID, OrigRefNbr = billRefNbr1 })
									.AddTaxTran(new TaxTran() { TaxID = taxDataItems[1].TaxID, OrigRefNbr = billRefNbr1 })
									.Build();


			var billRefNbr2 = "DOC002";

			var billAggr2 = new APInvoiceAggregateBuilder()
									.CreateDocument(APDocType.Invoice,
													refNbr: billRefNbr2,
													docDate: new DateTime(2015, 3, 5),
													curyID: "GBP",
													docDesc: "bill desc 2",
													module: module)
									.DocumentWith(vendorID: VendorDataContext.Vendor2.BAccountID,
													vendorLocationID: LocationDataContext.Vendor2Location.LocationID)
									.AddTaxTran(taxDataItems[0].TaxID, taxDataItems[0].TranTaxType, 14, 3, 23, 6)
									.AddTaxTran(taxDataItems[1].TaxID, taxDataItems[1].TranTaxType, 16, 4, 27, 8)
									.AddTaxTran(taxDataItems[2].TaxID, taxDataItems[2].TranTaxType)//should not be exported
									.Build();

			var taxBill2 = new APInvoiceAggregateBuilder()
									.CreateDocument(APDocType.Invoice,
													docDesc: "tax bill desc 2")
									.AddTaxTran(new TaxTran() { TaxID = taxDataItems[0].TaxID, OrigRefNbr = billRefNbr2 })
									.AddTaxTran(new TaxTran() { TaxID = taxDataItems[1].TaxID, OrigRefNbr = billRefNbr2 })
									.Build();

			var billAggregs = new[] { billAggr1, billAggr2 };
			var taxBillAggregs = new[] { taxBill1, taxBill2 };

			SetupRepositoryMethodsForDocumentsWithTaxDocumentTrans(billAggregs, billAggr1.Document.OrigModule, billAggr1.Document.DocType, taxAgencyID,
				taxPeriodID, taxBillAggregs);

			var documentGroup = new DocumentGroup<AP.APInvoice>()
			{
				Module = billAggr1.Document.OrigModule,
				DocumentType = billAggr1.Document.DocType,
				DocumentsByRefNbr = billAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecords = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID,
				taxPeriodID);

			//Assert
			Approvals.VerifyAll(purchaseRecords, "purchaseRecords", record => record.Dump());
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Document_Is_In_Base_Cury_And_TaxTrans_By_Tax_Document()
		{
			//Arrange
			var taxDataItem = TaxDataBuilder.CreateTaxDataItems(new[]
													{
														TaxDataContext.VatDirect.TaxID,
													})
												.Single();

			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			const decimal taxableAmt = 10;
			const decimal taxAmt = 1m;

			var billAggr1 = new APInvoiceAggregateBuilder()
									.CreateDocument(APDocType.Invoice,
													refNbr: DocumentDataContext.RefNbr,
													docDate: DocumentDataContext.DocDate,
													curyID: CompanyDataContext.Company.BaseCuryID)
									.DocumentWith(vendorID: VendorDataContext.Vendor.BAccountID,
													vendorLocationID: LocationDataContext.VendorLocation.LocationID)
									.AddTaxTran(taxDataItem.TaxID, taxDataItem.TranTaxType, taxableAmt, taxAmt, 20, 2)
									.Build();

			var taxBill1 = new APInvoiceAggregateBuilder()
									.CreateDocument(APDocType.Invoice)
									.AddTaxTran(new TaxTran() { TaxID = taxDataItem.TaxID, OrigRefNbr = DocumentDataContext.RefNbr })
									.Build();

			var billAggregs = new[] { billAggr1 };
			var taxBillAggregs = new[] { taxBill1 };

			SetupRepositoryMethodsForDocumentsWithTaxDocumentTrans(billAggregs, billAggr1.Document.OrigModule, billAggr1.Document.DocType, taxAgencyID,
				taxPeriodID, taxBillAggregs);

			var documentGroup = new DocumentGroup<AP.APInvoice>()
			{
				Module = billAggr1.Document.OrigModule,
				DocumentType = billAggr1.Document.DocType,
				DocumentsByRefNbr = billAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecord = InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID, taxPeriodID)
																.Single();

			//Assert
			Assert.Equal(taxableAmt, purchaseRecord.Amount);
			Assert.Equal(taxAmt, purchaseRecord.GSTAmount);
			Assert.Equal(ForeignCurrencyCodeForDocumentInBaseCury, purchaseRecord.ForeignCurrencyCode);
			Assert.Equal(0, purchaseRecord.ForeignCurrencyAmount);
			Assert.Equal(0, purchaseRecord.ForeignCurrencyAmountGST);
		}


		#region Test_CreateGAFRecordsForDocumentGroup_When_Exist_Many_Tax_Documents_For_Same_Document_And_Tax

		public class TaxDocumentDataItem
		{
			public string DocType { get; set; }
			public string RefNbr { get; set; }
			public string DocDesc { get; set; }

			public TaxDocumentDataItem(string docType, string refNbr, string docDesc)
			{
				DocType = docType;
				RefNbr = refNbr;
				DocDesc = docDesc;
			}
		}

		public static IEnumerable<object[]> TaxDocumentDataItems
		{
			get
			{
				return new[]
				{
					new object[]
					{
						new object[]
						{
							new TaxDocumentDataItem(APDocType.DebitAdj, "DOC2", "doc2 desc"),
							new TaxDocumentDataItem(APDocType.Invoice, "DOC1", "doc1 desc"),
							new TaxDocumentDataItem(APDocType.CreditAdj, "DOC3", "doc3 desc"),
						},
						"doc1 desc"
					},
					new object[]
					{
						new object[]
						{
							new TaxDocumentDataItem(APDocType.DebitAdj, "DOC2", "doc2 desc"),
							new TaxDocumentDataItem(APDocType.CreditAdj, "DOC3", "doc3 desc"),
						},
						"doc3 desc"
					},
					new object[]
					{
						new object[]
						{
							new TaxDocumentDataItem(APDocType.Invoice, "DOC4", "doc4 desc"),
							new TaxDocumentDataItem(APDocType.Invoice, "DOC1", "doc1 desc"),
						},
						"doc1 desc"
					},
				};
			}
		}

		[Theory, MemberData("TaxDocumentDataItems")]
		public void Test_CreateGAFRecordsForDocumentGroup_That_IDN_Has_Been_Taked_From_Determined_Document_When_Exist_Many_Tax_Documents_For_Same_Document_And_Tax(
			TaxDocumentDataItem[] taxDocumentDataItems,
			string expectedDocDesc)
		{
			//Arrange
			var taxDataItems = TaxDataBuilder.CreateTaxDataItems(new[]
			{
				TaxDataContext.VatDirect.TaxID,
				TaxDataContext.VatDirect2.TaxID,
			});

			var taxAgencyID = VendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = TaxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var billAggr1 = _apInvoiceAggregateBuilderFactory.CreateInvoiceAggregateBuilder(APDocType.Invoice)
																.AddTaxTran(taxDataItems[0].TaxID, taxDataItems[0].TranTaxType)
																.AddTaxTran(taxDataItems[1].TaxID, taxDataItems[1].TranTaxType)
																.Build();


			var docBuilder = new APInvoiceAggregateBuilder();

			var billAggregs = new[] { billAggr1 };
			var taxBillAggregs = taxDocumentDataItems.Select(taxDocData => 
														docBuilder.CreateDocument(taxDocData.DocType, 
																					refNbr: taxDocData.RefNbr, 
																					docDesc: taxDocData.DocDesc)
																	.AddTaxTran(new TaxTran() { TaxID = taxDataItems[0].TaxID, OrigRefNbr = DocumentDataContext.RefNbr })
																	.AddTaxTran(new TaxTran() { TaxID = taxDataItems[1].TaxID, OrigRefNbr = DocumentDataContext.RefNbr })
																	.Build())
														.ToArray();

			SetupRepositoryMethodsForDocumentsWithTaxDocumentTrans(billAggregs, billAggr1.Document.OrigModule,
				billAggr1.Document.DocType, taxAgencyID, taxPeriodID, taxBillAggregs);

			var documentGroup = new DocumentGroup<AP.APInvoice>()
			{
				Module = billAggr1.Document.OrigModule,
				DocumentType = billAggr1.Document.DocType,
				DocumentsByRefNbr = billAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecords =
				InvoiceGafRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID, taxPeriodID).ToArray();

			//Assert
			Assert.Equal(expectedDocDesc, purchaseRecords[0].ImportDeclarationNumber);
			Assert.Equal(expectedDocDesc, purchaseRecords[1].ImportDeclarationNumber);
		}

		#endregion

		#region Service

		private void SetupRepositoryMethodsForDocumentsWithTaxDocumentTrans(ICollection<APInvoiceAggregate> apInvoiceAggregs, string module,
			string docType, int? taxAgencyID, string taxPeriodID, ICollection<APInvoiceAggregate> taxInvoiceAggregates)
		{
			var refNbrs = apInvoiceAggregs.Select(apInvoiceAggr => apInvoiceAggr.Document.RefNbr)
										.ToArray();

			SetupRepositoryMethodsBase(apInvoiceAggregs, module, docType, taxAgencyID, taxPeriodID, refNbrs);

			GAFRepositoryMock.Setup(
				repo =>
					repo.GetReportedTaxAPRegistersWithTaxTransForDocuments(docType, refNbrs, taxAgencyID))
				.Returns(
					taxInvoiceAggregates.SelectMany(
						taxInvAggr =>
							taxInvAggr.TaxTransByTax.Values.SelectMany(
								taxTrans => taxTrans.Select(taxTran => new PXResult<APRegister, TaxTran>(taxInvAggr.Document, taxTran)))));
		}

		#endregion

		#endregion


		#region Service

		protected override void SetupRepositoryMethods(ICollection<APInvoiceAggregate> apInvoiceAggregs, string module, string docType, int? taxAgencyID, string taxPeriodID)
		{
			var refNbrs = apInvoiceAggregs.Select(apInvoiceAggr => apInvoiceAggr.Document.RefNbr)
										   .ToArray();

			SetupRepositoryMethodsBase(apInvoiceAggregs, module, docType, taxAgencyID, taxPeriodID, refNbrs);

			GAFRepositoryMock.Setup(repo => repo.GetReportedTaxAPRegistersWithTaxTransForDocuments(docType, refNbrs, taxAgencyID))
				.Returns(new PXResult<APRegister, TaxTran>[] { });
		}

		private void SetupRepositoryMethodsBase(ICollection<APInvoiceAggregate> apInvoiceAggregs, string module,
			string docType, int? taxAgencyID, string taxPeriodID, string[] refNbrs)
		{
			var taxTrans =
				apInvoiceAggregs.SelectMany(
					apInvoiceAggr => apInvoiceAggr.TaxTransByTax.Values.SelectMany(list => list.Select(tran => tran)));

			GAFRepositoryMock.Setup(
				repo => repo.GetReportedTaxTransForDocuments(module, docType, refNbrs, taxAgencyID, taxPeriodID))
				.Returns(taxTrans);

			SetupGetAPTranWithAPTaxForDocuments(docType, refNbrs, taxAgencyID, taxPeriodID, apInvoiceAggregs);
		}

		protected void SetupGetAPTranWithAPTaxForDocuments(string docType, string[] refNbrs, int? taxAgencyID, string taxPeriodID, IEnumerable<APInvoiceAggregate> apInvoiceAggregates)
		{
			var mockResult = new List<PXResult<APTran, APTax>>();

			foreach (var apInvoiceAggregate in apInvoiceAggregates)
			{
				foreach (var apTranKvp in apInvoiceAggregate.TransByLineNbr)
				{
					if (!apInvoiceAggregate.TranTaxesByLineNbrAndTax.ContainsKey(apTranKvp.Key))
						continue;

					foreach (var apTaxKvp in apInvoiceAggregate.TranTaxesByLineNbrAndTax[apTranKvp.Key])
					{
						mockResult.Add(new PXResult<APTran, APTax>(apTranKvp.Value, apTaxKvp.Value));
					}
				}
			}

			GAFRepositoryMock.Setup(repo => repo.GetReportedAPTransWithAPTaxesForTaxCalcedOnItem(docType, refNbrs, taxAgencyID, taxPeriodID))
				.Returns(mockResult);
		}

		#endregion
	}
}
