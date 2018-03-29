using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.Tests.AP.Builders;
using PX.Objects.Tests.AP.DataContexts;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.Infrastucture.ApprovalTests;
using PX.Objects.Tests.TX.DataContexts;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.CreatorsForDocs.Builders.Purchase;
using Xunit;

namespace PX.Objects.GAF.Tests.APPayment
{
	public class APPaymentGAFRecordsCreatorTests: GAFTestsBase
	{
		private VendorDataContext _vendorDataContext;
		private TaxPeriodDataContext _taxPeriodDataContext;
		private LocationDataContext _locationDataContext;

		private APPaymentGAFRecordsCreator ApPaymentGAFRecordsCreator;

		public APPaymentGAFRecordsCreatorTests()
		{
			_vendorDataContext = GetService<VendorDataContext>();
			_taxPeriodDataContext = GetService<TaxPeriodDataContext>();
			_locationDataContext = GetService<LocationDataContext>();

			var purchaseRecordBuilder = new PurchaseRecordBuilderByTaxTranOfAPPayment(GAFRepository,
				new PurchaseRecordBuilderByVendorData(GAFRepository),
				new GafRecordBuilderByRegister(GAFRepository));

			ApPaymentGAFRecordsCreator = new APPaymentGAFRecordsCreator(GAFRepository, purchaseRecordBuilder);
		}

		[Fact]
		[UseReporter(typeof(DiffReporter))]
		[UseApprovalFileName("__CreateGAFRecordsForDocumentGroup_For_Two_Documents__")]
		public void Test_CreateGAFRecordsForDocumentGroup_For_Two_Documents_With_Two_Taxes()
		{
			//Arrange
			var taxAgencyID = _vendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = _taxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var apRegisterAggregateBuilder = new APRegisterAggregateBuilder();

			var paymentAggr1 = apRegisterAggregateBuilder.CreateDocument(APDocType.Check,
																				"CHECK1",
																				DocumentDataContext.DocDate,
																				DocumentDataContext.CuryIDEUR)
																.DocumentWith(vendorID: _vendorDataContext.Vendor.BAccountID,
																				vendorLocationID: _locationDataContext.VendorLocation.LocationID)
																.AddTaxTran(new TaxTran()
																				{
																					TaxID = TaxDataContext.WithholdingTax.TaxID,
																					TaxableAmt = 10,
																					TaxAmt = 1,
																					CuryTaxableAmt = 20,
																					CuryTaxAmt = 3,
																					AdjdDocType = APDocType.Invoice,
																					AdjdRefNbr = DocumentDataContext.RefNbr
																				})
																.AddTaxTran(new TaxTran()
																				{
																					TaxID = TaxDataContext.WithholdingTax2.TaxID,
																					TaxableAmt = 15,
																					TaxAmt = 2,
																					CuryTaxableAmt = 25,
																					CuryTaxAmt = 5,
																					AdjdDocType = APDocType.Invoice,
																					AdjdRefNbr = DocumentDataContext.RefNbr
																})
																.Build();

			var adjdDocument = new APRegister()
			{
				DocType = APDocType.Invoice,
				RefNbr = DocumentDataContext.RefNbr,
				DocDesc = "bill 1 desc"
			};

			var paymentAggr2 = apRegisterAggregateBuilder.CreateDocument(APDocType.Check,
																				"CHECK2",
																				DocumentDataContext.DocDate2,
																				DocumentDataContext.CuryIDGBP)
																.DocumentWith(vendorID: _vendorDataContext.Vendor2.BAccountID,
																				vendorLocationID: _locationDataContext.Vendor2Location.LocationID)
																.AddTaxTran(new TaxTran()
																{
																	TaxID = TaxDataContext.WithholdingTax.TaxID,
																	TaxableAmt = 11,
																	TaxAmt = 1.1m,
																	CuryTaxableAmt = 21,
																	CuryTaxAmt = 3.1m,
																	AdjdDocType = APDocType.Invoice,
																	AdjdRefNbr = DocumentDataContext.RefNbr2
																})
																.AddTaxTran(new TaxTran()
																{
																	TaxID = TaxDataContext.WithholdingTax2.TaxID,
																	TaxableAmt = 16,
																	TaxAmt = 2.1m,
																	CuryTaxableAmt = 26,
																	CuryTaxAmt = 5.1m,
																	AdjdDocType = APDocType.Invoice,
																	AdjdRefNbr = DocumentDataContext.RefNbr2
																})
																.Build();

			var adjdDocument2 = new APRegister()
			{
				DocType = APDocType.Invoice,
				RefNbr = DocumentDataContext.RefNbr2,
				DocDesc = "bill 2 desc"
			};

			var paymentAggregs = new[] { paymentAggr1, paymentAggr2 };
			var adjdRegisters = new[] { adjdDocument, adjdDocument2 };

			SetupRepositoryMethods(paymentAggregs, adjdRegisters, paymentAggr1.Document.OrigModule, paymentAggr1.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<AP.APRegister>()
			{
				Module = paymentAggr1.Document.OrigModule,
				DocumentType = paymentAggr1.Document.DocType,
				DocumentsByRefNbr = paymentAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecords = ApPaymentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID, taxPeriodID);

			//Assert
			Approvals.VerifyAll(purchaseRecords, "purchaseRecords", record => record.Dump());
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Document_Is_In_Base_Cury()
		{
			//Arrange
			var taxAgencyID = _vendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = _taxPeriodDataContext.TaxPeriod.TaxPeriodID;

			const decimal taxableAmt = 100;
			const decimal taxAmt = 15;

			var paymentAggr = new APRegisterAggregateBuilder()
								.CreateDocument(APDocType.Check,
												DocumentDataContext.RefNbr,
												DocumentDataContext.DocDate,
												CompanyDataContext.Company.BaseCuryID)
								.DocumentWith(vendorID: _vendorDataContext.Vendor.BAccountID,
												vendorLocationID: _locationDataContext.VendorLocation.LocationID)
								.AddTaxTran(new TaxTran()
												{
													TaxID = TaxDataContext.WithholdingTax.TaxID,
													TaxableAmt = taxableAmt,
													TaxAmt = taxAmt,
													CuryTaxableAmt = 200, 
													CuryTaxAmt = 30,
													AdjdDocType = APDocType.Invoice,
													AdjdRefNbr = DocumentDataContext.RefNbr2
												})
								.Build();

			var paymentAggregs = paymentAggr.SingleToArray();

			var adjdRegister = new APRegister
			{
				DocType = APDocType.Invoice,
				RefNbr = DocumentDataContext.RefNbr2
			};

			SetupRepositoryMethods(paymentAggregs, adjdRegister.SingleToArray(), paymentAggr.Document.OrigModule, paymentAggr.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<AP.APRegister>()
			{
				Module = paymentAggr.Document.OrigModule,
				DocumentType = paymentAggr.Document.DocType,
				DocumentsByRefNbr = paymentAggregs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecord = ApPaymentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID, taxPeriodID)
																.Single();

			//Assert
			Assert.Equal(taxableAmt, purchaseRecord.Amount);
			Assert.Equal(taxAmt, purchaseRecord.GSTAmount);
			Assert.Equal(ForeignCurrencyCodeForDocumentInBaseCury, purchaseRecord.ForeignCurrencyCode);
			Assert.Equal(0, purchaseRecord.ForeignCurrencyAmount);
			Assert.Equal(0, purchaseRecord.ForeignCurrencyAmountGST);
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Check_Has_Many_Applications_With_Two_Taxes()
		{
			//Arrange
			var taxAgencyID = _vendorDataContext.TaxAgency.BAccountID;
			var taxPeriodID = _taxPeriodDataContext.TaxPeriod.TaxPeriodID;

			var adjdDocumentsRawData = new List<object[]>()
			{
				//			DocType,				RefNbr,		expected gaf line number
				new object[] {APDocType.Invoice,	"BILL1",	2},
				new object[] {APDocType.Invoice,	"BILL2",	3},
				new object[] {APDocType.CreditAdj,	"CRADJ1",	1},
			};

			var paymentAggrBuilder = new APRegisterAggregateBuilder()
								.CreateDocument(APDocType.Check,
												DocumentDataContext.RefNbr,
												DocumentDataContext.DocDate,
												CompanyDataContext.Company.BaseCuryID)
								.DocumentWith(vendorID: _vendorDataContext.Vendor.BAccountID,
												vendorLocationID: _locationDataContext.VendorLocation.LocationID);

			var adjdRegisters = new List<APRegister>();

			var taxIDs = new string[]
			{
				TaxDataContext.WithholdingTax.TaxID,
				TaxDataContext.WithholdingTax2.TaxID
			};

			foreach (var adjdDocumentRawData in adjdDocumentsRawData)
			{
				foreach (var taxID in taxIDs)
				{
					paymentAggrBuilder.AddTaxTran(new TaxTran()
					{
						TaxableAmt = 0,
						TaxAmt = 0,
						CuryTaxableAmt = 0,
						CuryTaxAmt = 0,
						AdjdDocType = (string)adjdDocumentRawData[0],
						AdjdRefNbr = (string)adjdDocumentRawData[1],
						TaxID = taxID
					});
				}

				adjdRegisters.Add(new APRegister()
				{
					DocType = (string)adjdDocumentRawData[0],
					RefNbr = (string)adjdDocumentRawData[1],
					DocDesc = (string)adjdDocumentRawData[1],//to identify gaf record on asserting
				});
			}

			var paymentAggr = paymentAggrBuilder.Build();
			var paymentAggrs = paymentAggr.SingleToArray();

			SetupRepositoryMethods(paymentAggrs, adjdRegisters, paymentAggr.Document.OrigModule, paymentAggr.Document.DocType, taxAgencyID, taxPeriodID);

			var documentGroup = new DocumentGroup<AP.APRegister>()
			{
				Module = paymentAggr.Document.OrigModule,
				DocumentType = paymentAggr.Document.DocType,
				DocumentsByRefNbr = paymentAggrs.ToDictionary(aggr => aggr.Document.RefNbr, aggr => aggr.Document)
			};

			//Action
			var purchaseRecords = ApPaymentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentGroup, taxAgencyID,
				taxPeriodID);

			//Assert
			foreach (var purchaseRecord in purchaseRecords)
			{
				var adjdDocumentRawData = adjdDocumentsRawData.Single(data => data[1] == purchaseRecord.ProductDescription);

				Assert.Equal(adjdDocumentRawData[2], purchaseRecord.LineNumber);
			}
		}

		protected void SetupRepositoryMethods(ICollection<APRegisterAggregate> paymentAggregs, ICollection<APRegister> adjdDocumentRegisters, string module, string docType,
			int? taxAgencyID, string taxPeriodID)
		{
			var taxTrans =
				paymentAggregs.SelectMany(
					apInvoiceAggr =>
						apInvoiceAggr.TaxTransByTax.Values.SelectMany(taxTranList => taxTranList.Select(taxTran => taxTran)));

			GAFRepositoryMock.Setup(repo => repo.GetReportedTaxTransForDocuments(module,
																		docType,
																		It.Is<string[]>(refNbrs => refNbrs.SequenceEqual(paymentAggregs.Select(aggr => aggr.Document.RefNbr))),
																		taxAgencyID,
																		taxPeriodID))
								.Returns(taxTrans);

			var taxTransWithAdjdDocument =
				paymentAggregs.SelectMany(
					aggr => aggr.TaxTransByTax.Values.SelectMany(taxTranList => taxTranList.Select(
						taxTran => new PXResult<TaxTran, APRegister>(taxTran,
							adjdDocumentRegisters.Single(
								register => register.DocType == taxTran.AdjdDocType && register.RefNbr == taxTran.AdjdRefNbr)))));

			GAFRepositoryMock.Setup(repo => repo.GetReportedTaxTransWithAdjdDocumentForAPPayments(docType,
																						It.Is<string[]>(refNbrs => refNbrs.SequenceEqual(paymentAggregs.Select(aggr => aggr.Document.RefNbr))),
																						taxAgencyID,
																						taxPeriodID))
								.Returns(taxTransWithAdjdDocument);
		}
	}
}
