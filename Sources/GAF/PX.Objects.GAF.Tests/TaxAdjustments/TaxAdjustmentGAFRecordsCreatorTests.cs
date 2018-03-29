using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.GL;
using PX.Objects.Tests.AP.Builders;
using PX.Objects.Tests.AR.Builders;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.Infrastucture.ApprovalTests;
using PX.Objects.Tests.TX.Builders;
using PX.Objects.Tests.TX.DataContexts;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.Data.Records;
using Xunit;

namespace PX.Objects.GAF.Tests.TaxAdjustments
{
	public class TaxAdjustmentGAFRecordsCreatorTests: GAFTestsBase
	{
		private ARTaxDataBuilder _arTaxDataBuilder;

		private TaxAdjustmentGAFRecordsCreator _taxAdjustmentGAFRecordsCreator;

		private const string TaxPeriodID = "201503";
		private const int BranchID = 1;

		public TaxAdjustmentGAFRecordsCreatorTests()
		{
			_arTaxDataBuilder = GetService<ARTaxDataBuilder>();

			_taxAdjustmentGAFRecordsCreator = new TaxAdjustmentGAFRecordsCreator(GAFRepository,
																					new GafRecordBuilderByTaxAdjustmentTaxTran(GAFRepository));
		}

		public DocumentRecordsAggregate Test_CreateGAFRecordsForDocumentGroup_For_Many_Documents_Taxes_And_TaxTrans(string docType)
		{
			//Arrange
			var vatTaxID = TaxDataContext.VatTax.TaxID;
			var vatTaxID2 = TaxDataContext.VatTax2.TaxID;

			var taxDataItems = _arTaxDataBuilder.CreateTaxDataItems(new[]
			{
				vatTaxID,
				vatTaxID2
			});

			var taxAdjustments = new[]
			{
				new TX.TaxAdjustment()
				{
					DocType = docType,
					RefNbr = DocumentDataContext.RefNbr,
					DocDate = DocumentDataContext.DocDate,
					CuryID = DocumentDataContext.CuryIDGBP,
					DocDesc = "doc1 desc"
				},
				new TX.TaxAdjustment()
				{
					DocType = docType,
					RefNbr = DocumentDataContext.RefNbr2,
					DocDate = DocumentDataContext.DocDate2,
					CuryID = DocumentDataContext.CuryIDEUR,
					DocDesc = "doc2 desc"
				}
			};

			#region TaxTrans

			var taxTranRecordID = 1;

			var taxTrans = new TaxTran[]
			{
				//Tax Adjustment 1
				new TaxTran()
				{
					RefNbr = DocumentDataContext.RefNbr,
					TaxID = vatTaxID,
					TaxableAmt = 100,
					TaxAmt = 7,
					CuryTaxableAmt = 200,
					CuryTaxAmt = 14,
					RecordID = taxTranRecordID++,
					TaxType = taxDataItems[0].TranTaxType
				},
				new TaxTran()
				{
					RefNbr = DocumentDataContext.RefNbr,
					TaxID = TaxDataContext.VatTax.TaxID,
					TaxableAmt = 150,
					TaxAmt = 10.5m,
					CuryTaxableAmt = 300,
					CuryTaxAmt = 21,
					RecordID = taxTranRecordID++,
					TaxType = taxDataItems[0].TranTaxType
				},
				new TaxTran()
				{
					RefNbr = DocumentDataContext.RefNbr,
					TaxID = TaxDataContext.VatTax2.TaxID,
					TaxableAmt = 110,
					TaxAmt = 11,
					CuryTaxableAmt = 220,
					CuryTaxAmt = 22,
					RecordID = taxTranRecordID++,
					TaxType = taxDataItems[1].TranTaxType
				},
				//Tax Adjustment 2
				new TaxTran()
				{
					RefNbr = DocumentDataContext.RefNbr2,
					TaxID = TaxDataContext.VatTax.TaxID,
					TaxableAmt = 400,
					TaxAmt = 28,
					CuryTaxableAmt = 800,
					CuryTaxAmt = 56,
					RecordID = taxTranRecordID++,
					TaxType = taxDataItems[0].TranTaxType
				},
				new TaxTran()
				{
					RefNbr = DocumentDataContext.RefNbr2,
					TaxID = TaxDataContext.VatTax2.TaxID,
					TaxableAmt = 600,
					TaxAmt = 42,
					CuryTaxableAmt = 1200,
					CuryTaxAmt = 84,
					RecordID = taxTranRecordID++,
					TaxType = taxDataItems[1].TranTaxType
				},
				new TaxTran()
				{
					RefNbr = DocumentDataContext.RefNbr2,
					TaxID = TaxDataContext.VatTax2.TaxID,
					TaxableAmt = 330,
					TaxAmt = 33,
					CuryTaxableAmt = 660,
					CuryTaxAmt = 66,
					RecordID = taxTranRecordID++,
					TaxType = taxDataItems[1].TranTaxType
				},
			};

			#endregion

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				DocumentType = docType,
				RefNbrs = taxAdjustments.Select(taxAdj => taxAdj.RefNbr).ToList()
			};

			SetupRepositoryMethods(taxAdjustments, taxTrans, documentIDGroup, TaxPeriodID);

			//Action
			return _taxAdjustmentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);
		}


		[Fact]
		[UseReporter(typeof (DiffReporter))]
		[UseApprovalFileName("__CreateGAFRecordsForDocumentGroup__Many_Docs__Purchase")]
		public void Test_CreateGAFRecordsForDocumentGroup_For_Many_Documents_Taxes_And_TaxTrans_For_Purchase()
		{
			var gafRecordsAggr =
				Test_CreateGAFRecordsForDocumentGroup_For_Many_Documents_Taxes_And_TaxTrans(TaxAdjustmentType.AdjustInput);

			//Assert
			Assert.Empty(gafRecordsAggr.SupplyRecords);
			Approvals.VerifyAll(gafRecordsAggr.PurchaseRecords, "records", record => record.Dump());
		}

		[Fact]
		[UseReporter(typeof(DiffReporter))]
		[UseApprovalFileName("__CreateGAFRecordsForDocumentGroup__Many_Docs__Supply")]
		public void Test_CreateGAFRecordsForDocumentGroup_For_Many_Documents_Taxes_And_TaxTrans_For_Supply()
		{
			var gafRecordsAggr =
					Test_CreateGAFRecordsForDocumentGroup_For_Many_Documents_Taxes_And_TaxTrans(TaxAdjustmentType.AdjustOutput);

			//Assert
			Assert.Empty(gafRecordsAggr.PurchaseRecords);
			Approvals.VerifyAll(gafRecordsAggr.SupplyRecords, "records", record => record.Dump());
		}

		[Theory]
		[InlineData(TaxAdjustmentType.AdjustInput, TaxDataContext.VatTaxID)]
		[InlineData(TaxAdjustmentType.AdjustOutput, TaxDataContext.WithholdingTaxID)]
		[InlineData(TaxAdjustmentType.AdjustOutput, TaxDataContext.ReverseVatTaxID)]
		public void Test_CreateGAFRecordsForDocumentGroup_That_PurchaseRecords_Are_Generated(string docType, string taxID)
		{
			//Arrange
			TaxDataBuilderBase taxDataBuilder = null;

			taxDataBuilder = docType == TaxAdjustmentType.AdjustOutput
								? (TaxDataBuilderBase) GetService<ARTaxDataBuilder>()
								: GetService<APTaxDataBuilder>();

			var taxData = taxDataBuilder.CreateTaxDataItems(taxID.SingleToArray())
										.Single();

			const string docDesc = "doc1 desc";

			var taxAdjustment = new TX.TaxAdjustment()
			{
				DocType = docType,
				RefNbr = DocumentDataContext.RefNbr,
				DocDate = DocumentDataContext.DocDate,
				CuryID = DocumentDataContext.CuryIDGBP,
				DocDesc = docDesc
			};

			const decimal taxableAmt = 100;
			decimal taxAmt = taxableAmt / taxData.Rate;
			const decimal curyTaxableAmt = 200;
			decimal curyTaxAmt = curyTaxableAmt / taxData.Rate;

			var taxTran = new TaxTran()
			{
				RefNbr = DocumentDataContext.RefNbr,
				TaxID = taxData.TaxID,
				TaxableAmt = taxableAmt,
				TaxAmt = taxAmt,
				CuryTaxableAmt = curyTaxableAmt,
				CuryTaxAmt = curyTaxAmt,
				TaxType = taxData.TranTaxType
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				DocumentType = docType,
				RefNbrs = taxAdjustment.RefNbr.SingleToList()
			};

			SetupRepositoryMethods(taxAdjustment.SingleToArray(), taxTran.SingleToArray(), documentIDGroup, TaxPeriodID);

			//Action
			var recordsAggr = _taxAdjustmentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);
			var purchaseRecord = recordsAggr.PurchaseRecords.Single();

			//Assert
			Assert.Empty(recordsAggr.SupplyRecords);
			Assert.NotEmpty(recordsAggr.PurchaseRecords);

			Assert.Null(purchaseRecord.SupplierName);
			Assert.Null(purchaseRecord.SupplierBRN);
			Assert.Null(purchaseRecord.ImportDeclarationNumber);
			Assert.Equal(DocumentDataContext.DocDate, purchaseRecord.InvoiceDate);
			Assert.Equal(DocumentDataContext.RefNbr, purchaseRecord.InvoiceNumber);
			Assert.Equal(1, purchaseRecord.LineNumber);
			Assert.Equal(docDesc, purchaseRecord.ProductDescription);
			Assert.Equal(taxID, purchaseRecord.TaxCode);
			Assert.Equal(DocumentDataContext.CuryIDGBP, purchaseRecord.ForeignCurrencyCode);
			Assert.Equal(taxableAmt, purchaseRecord.Amount);
			Assert.Equal(taxAmt, purchaseRecord.GSTAmount);
			Assert.Equal(curyTaxableAmt, purchaseRecord.ForeignCurrencyAmount);
			Assert.Equal(curyTaxAmt, purchaseRecord.ForeignCurrencyAmountGST);
		}

		[Theory]
		[InlineData(TaxAdjustmentType.AdjustOutput, TaxDataContext.VatTaxID)]
		[InlineData(TaxAdjustmentType.AdjustInput, TaxDataContext.ReverseVatTaxID)]
		public void Test_CreateGAFRecordsForDocumentGroup_That_SupplyRecords_Are_Generated(string docType, string taxID)
		{
			//Arrange
			TaxDataBuilderBase taxDataBuilder = null;

			taxDataBuilder = docType == TaxAdjustmentType.AdjustOutput
								? (TaxDataBuilderBase)GetService<ARTaxDataBuilder>()
								: GetService<APTaxDataBuilder>();

			var taxData = taxDataBuilder.CreateTaxDataItems(taxID.SingleToArray())
										.Single();

			const string docDesc = "doc1 desc";

			var taxAdjustment = new TX.TaxAdjustment()
			{
				DocType = docType,
				RefNbr = DocumentDataContext.RefNbr,
				DocDate = DocumentDataContext.DocDate,
				CuryID = DocumentDataContext.CuryIDGBP,
				DocDesc = docDesc
			};

			const decimal taxableAmt = 100;
			decimal taxAmt = taxableAmt / taxData.Rate;
			const decimal curyTaxableAmt = 200;
			decimal curyTaxAmt = curyTaxableAmt / taxData.Rate;

			var taxTran = new TaxTran()
			{
				RefNbr = DocumentDataContext.RefNbr,
				TaxID = taxData.TaxID,
				TaxableAmt = taxableAmt,
				TaxAmt = taxAmt,
				CuryTaxableAmt = curyTaxableAmt,
				CuryTaxAmt = curyTaxAmt,
				TaxType = taxData.TranTaxType
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				DocumentType = docType,
				RefNbrs = taxAdjustment.RefNbr.SingleToList()
			};

			SetupRepositoryMethods(taxAdjustment.SingleToArray(), taxTran.SingleToArray(), documentIDGroup, TaxPeriodID);

			//Action
			var recordsAggr = _taxAdjustmentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);
			var supplyRecord = recordsAggr.SupplyRecords.Single();

			//Assert
			Assert.Empty(recordsAggr.PurchaseRecords);
			Assert.NotEmpty(recordsAggr.SupplyRecords);

			Assert.Null(supplyRecord.CustomerName);
			Assert.Null(supplyRecord.CustomerBRN);
			Assert.Null(supplyRecord.Country);
			Assert.Equal(DocumentDataContext.DocDate, supplyRecord.InvoiceDate);
			Assert.Equal(DocumentDataContext.RefNbr, supplyRecord.InvoiceNumber);
			Assert.Equal(1, supplyRecord.LineNumber);
			Assert.Equal(docDesc, supplyRecord.ProductDescription);
			Assert.Equal(taxID, supplyRecord.TaxCode);
			Assert.Equal(DocumentDataContext.CuryIDGBP, supplyRecord.ForeignCurrencyCode);
			Assert.Equal(taxableAmt, supplyRecord.Amount);
			Assert.Equal(taxAmt, supplyRecord.GSTAmount);
			Assert.Equal(curyTaxableAmt, supplyRecord.ForeignCurrencyAmount);
			Assert.Equal(curyTaxAmt, supplyRecord.ForeignCurrencyAmountGST);
		}

		[Theory]
		[InlineData(TaxAdjustmentType.AdjustOutput)]
		[InlineData(TaxAdjustmentType.AdjustInput)]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Document_Is_In_Base_Cury(string docType)
		{
			//Arrange
			TaxDataBuilderBase taxDataBuilder = null;

			taxDataBuilder = docType == TaxAdjustmentType.AdjustOutput
								? (TaxDataBuilderBase)GetService<ARTaxDataBuilder>()
								: GetService<APTaxDataBuilder>();

			var taxData = taxDataBuilder.CreateTaxDataItems(TaxDataContext.VatTax.TaxID.SingleToArray())
										.Single();

			const string docDesc = "doc1 desc";

			var taxAdjustment = new TX.TaxAdjustment()
			{
				DocType = docType,
				RefNbr = DocumentDataContext.RefNbr,
				DocDate = DocumentDataContext.DocDate,
				CuryID = CompanyDataContext.Company.BaseCuryID,
				DocDesc = docDesc
			};

			const decimal taxableAmt = 100;
			const decimal taxAmt = 7;
			const decimal curyTaxableAmt = 200;
			const decimal curyTaxAmt = 14;

			var taxTran = new TaxTran()
			{
				RefNbr = DocumentDataContext.RefNbr,
				TaxID = taxData.TaxID,
				TaxableAmt = taxableAmt,
				TaxAmt = taxAmt,
				CuryTaxableAmt = curyTaxableAmt,
				CuryTaxAmt = curyTaxAmt,
				TaxType = taxData.TranTaxType
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				DocumentType = docType,
				RefNbrs = taxAdjustment.RefNbr.SingleToList()
			};

			SetupRepositoryMethods(taxAdjustment.SingleToArray(), taxTran.SingleToArray(), documentIDGroup, TaxPeriodID);

			//Action
			var recordsAggr = _taxAdjustmentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);

			var gafRecord = taxAdjustment.DocType == TaxAdjustmentType.AdjustOutput
				? (GAFRecordBase) recordsAggr.SupplyRecords.Single()
				: (GAFRecordBase) recordsAggr.PurchaseRecords.Single();

			//Assert
			Assert.Equal(taxableAmt, gafRecord.Amount);
			Assert.Equal(taxAmt, gafRecord.GSTAmount);
			Assert.Equal(ForeignCurrencyCodeForDocumentInBaseCury, gafRecord.ForeignCurrencyCode);
			Assert.Equal(0, gafRecord.ForeignCurrencyAmount);
			Assert.Equal(0, gafRecord.ForeignCurrencyAmountGST);
		}

		private void SetupRepositoryMethods(ICollection<TX.TaxAdjustment> taxAdjustments, ICollection<TaxTran> taxTrans, DocumentIDGroup documentIDGroup, string taxPeriodID)
		{
			GAFRepositoryMock.Setup<IEnumerable<TX.TaxAdjustment>>(repo => repo.GetTaxAdjustments(documentIDGroup.DocumentType,
																	It.Is<string[]>(refNbrs => refNbrs.SequenceEqual(documentIDGroup.RefNbrs))))
								.Returns(taxAdjustments);

			GAFRepositoryMock.Setup(repo=>repo.GetTaxTransForDocuments(documentIDGroup.Module,
																			It.Is<string[]>(docTypes => docTypes.SequenceEqual(documentIDGroup.DocumentTypes)), 
																			It.Is<string[]>(refNbrs => refNbrs.SequenceEqual(documentIDGroup.RefNbrs)), taxPeriodID))
								.Returns(taxTrans);
		}
	}
}
