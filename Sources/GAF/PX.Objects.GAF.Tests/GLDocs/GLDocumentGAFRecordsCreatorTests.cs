using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.GL;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.Infrastucture.ApprovalTests;
using PX.Objects.Tests.TX.DataContexts;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.Data.Records;
using Xunit;

namespace PX.Objects.GAF.Tests.GLDocs
{
	public class GLDocumentGAFRecordsCreatorTests : GAFTestsBase
	{
		private GLDocumentGAFRecordsCreator _glDocumentGAFRecordsCreator;

		private const string TaxPeriodID = "201503";
		private const int BranchID = 1;
		private const string BatchNbr = "0004";
		private static DateTime TranDate = new DateTime(2015, 3, 1);
		private static DateTime TranDate2 = new DateTime(2015, 3, 2);
		private const string CuryIDEUR = "EUR";
		private const string TranDesc = "doc desc 1";
		private const string TranDesc2 = "doc desc 1";
		private const string RefNbr = "123";
		private const string RefNbr2 = "124";
		private const string TaxCategoryID = "PAYR";

		private TaxRevDataContext _taxRevDataContext;
		private TaxDataContext _taxDataContext;

		public GLDocumentGAFRecordsCreatorTests()
		{
			_glDocumentGAFRecordsCreator = new GLDocumentGAFRecordsCreator(GAFRepository,
																			new GafRecordBuilderByGLTranAndTaxTran(GAFRepository));

			_taxRevDataContext = GetService<TaxRevDataContext>();
			_taxDataContext = GetService<TaxDataContext>();
		}

		[Theory]
		[InlineData(CSTaxType.Use, TaxDataContext.VatTaxID)]
		[InlineData(CSTaxType.Sales, TaxDataContext.WithholdingTaxID)]
		[InlineData(CSTaxType.Sales, TaxDataContext.ReverseVatTaxID)]
		public void Test_CreateGAFRecordsForDocumentGroup_That_PurchaseRecords_Are_Generated(string tranTaxType, string taxID)
		{
			//Arrange
			var rate = _taxRevDataContext.TaxRevs.Single(taxRev => taxRev.TaxID == taxID).TaxRate.Value;

			var taxableGLTran = new GLTran()
			{
				TranDate = TranDate,
				TranDesc = TranDesc,
				BatchNbr = BatchNbr,
				RefNbr = RefNbr,
				TaxCategoryID = TaxCategoryID,
				BranchID = BranchID
			};

			var curyInfo = new CurrencyInfo() {CuryID = CuryIDEUR};
			var taxCategoryDetail = new TaxCategoryDet() { TaxID = taxID, TaxCategoryID = TaxCategoryID };

			const decimal taxableAmt = 100;
			decimal taxAmt = taxableAmt / rate;
			const decimal curyTaxableAmt = 200;
			decimal curyTaxAmt = curyTaxableAmt / rate;

			var taxTran = new TaxTran()
			{
				Module = BatchModule.GL,
				TranType = TaxTran.tranType.TranForward,
				RefNbr = BatchNbr,
				TaxID = taxID,
				TaxableAmt = taxableAmt,
				TaxAmt = taxAmt,
				CuryTaxableAmt = curyTaxableAmt,
				CuryTaxAmt = curyTaxAmt,
				TaxType = tranTaxType,
				LineRefNbr = RefNbr,
				BranchID = BranchID
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				RefNbrs = taxTran.RefNbr.SingleToList()
			};

			SetupRepositoryMethods(taxableGLTran.SingleToArray(), curyInfo, taxTran.SingleToArray(), documentIDGroup, BranchID,
				TaxPeriodID, taxID.SingleToArray(), taxCategoryDetail);

			//Action
			var recordsAggr = _glDocumentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);
			var purchaseRecord = recordsAggr.PurchaseRecords.Single();

			//Assert
			Assert.Empty(recordsAggr.SupplyRecords);
			Assert.NotEmpty(recordsAggr.PurchaseRecords);

			Assert.Null(purchaseRecord.SupplierName);
			Assert.Null(purchaseRecord.SupplierBRN);
			Assert.Null(purchaseRecord.ImportDeclarationNumber);
			Assert.Equal(TranDate, purchaseRecord.InvoiceDate);
			Assert.Equal(string.Concat(BatchNbr, RefNbr), purchaseRecord.InvoiceNumber);
			Assert.Equal(1, purchaseRecord.LineNumber);
			Assert.Equal(TranDesc, purchaseRecord.ProductDescription);
			Assert.Equal(taxID, purchaseRecord.TaxCode);
			Assert.Equal(CuryIDEUR, purchaseRecord.ForeignCurrencyCode);
			Assert.Equal(taxableAmt, purchaseRecord.Amount);
			Assert.Equal(taxAmt, purchaseRecord.GSTAmount);
			Assert.Equal(curyTaxableAmt, purchaseRecord.ForeignCurrencyAmount);
			Assert.Equal(curyTaxAmt, purchaseRecord.ForeignCurrencyAmountGST);
		}

		[Theory]
		[InlineData(CSTaxType.Sales, TaxDataContext.VatTaxID)]
		[InlineData(CSTaxType.Use, TaxDataContext.ReverseVatTaxID)]
		public void Test_CreateGAFRecordsForDocumentGroup_That_SupplyRecords_Are_Generated(string tranTaxType, string taxID)
		{
			//Arrange
			var rate = _taxRevDataContext.TaxRevs.Single(taxRev => taxRev.TaxID == taxID).TaxRate.Value;

			var taxableGLTran = new GLTran()
			{
				TranDate = TranDate,
				TranDesc = TranDesc,
				BatchNbr = BatchNbr,
				RefNbr = RefNbr,
				TaxCategoryID = TaxCategoryID,
				BranchID = BranchID
			};

			var curyInfo = new CurrencyInfo() { CuryID = CuryIDEUR };
			var taxCategoryDetail = new TaxCategoryDet() { TaxID = taxID, TaxCategoryID = TaxCategoryID };

			const decimal taxableAmt = 100;
			decimal taxAmt = taxableAmt / rate;
			const decimal curyTaxableAmt = 200;
			decimal curyTaxAmt = curyTaxableAmt / rate;

			var taxTran = new TaxTran()
			{
				Module = BatchModule.GL,
				TranType = TaxTran.tranType.TranForward,
				RefNbr = BatchNbr,
				TaxID = taxID,
				TaxableAmt = taxableAmt,
				TaxAmt = taxAmt,
				CuryTaxableAmt = curyTaxableAmt,
				CuryTaxAmt = curyTaxAmt,
				TaxType = tranTaxType,
				LineRefNbr = RefNbr,
				BranchID = BranchID
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				RefNbrs = taxTran.RefNbr.SingleToList()
			};

			SetupRepositoryMethods(taxableGLTran.SingleToArray(), curyInfo, taxTran.SingleToArray(), documentIDGroup, BranchID,
				TaxPeriodID, taxID.SingleToArray(), taxCategoryDetail);

			//Action
			var recordsAggr = _glDocumentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);
			var supplyRecord = recordsAggr.SupplyRecords.Single();

			//Assert
			Assert.Empty(recordsAggr.PurchaseRecords);
			Assert.NotEmpty(recordsAggr.SupplyRecords);

			Assert.Null(supplyRecord.CustomerName);
			Assert.Null(supplyRecord.CustomerBRN);
			Assert.Null(supplyRecord.Country);
			Assert.Equal(TranDate, supplyRecord.InvoiceDate);
			Assert.Equal(string.Concat(BatchNbr, RefNbr), supplyRecord.InvoiceNumber);
			Assert.Equal(1, supplyRecord.LineNumber);
			Assert.Equal(TranDesc, supplyRecord.ProductDescription);
			Assert.Equal(taxID, supplyRecord.TaxCode);
			Assert.Equal(CuryIDEUR, supplyRecord.ForeignCurrencyCode);
			Assert.Equal(taxableAmt, supplyRecord.Amount);
			Assert.Equal(taxAmt, supplyRecord.GSTAmount);
			Assert.Equal(curyTaxableAmt, supplyRecord.ForeignCurrencyAmount);
			Assert.Equal(curyTaxAmt, supplyRecord.ForeignCurrencyAmountGST);
		}

		[Theory]
		[InlineData(CSTaxType.Sales)]
		[InlineData(CSTaxType.Use)]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Document_Is_In_Base_Cury(string tranTaxType)
		{
			//Arrange
			var taxID = _taxDataContext.VatTax.TaxID;

			var rate = _taxRevDataContext.TaxRevs.Single(taxRev => taxRev.TaxID == taxID).TaxRate.Value;

			var taxableGLTran = new GLTran()
			{
				TranDate = TranDate,
				TranDesc = TranDesc,
				BatchNbr = BatchNbr,
				RefNbr = RefNbr,
				TaxCategoryID = TaxCategoryID,
				BranchID = BranchID
			};

			var curyInfo = new CurrencyInfo() { CuryID = CompanyDataContext.CompanyBaseCuryID };
			var taxCategoryDetail = new TaxCategoryDet() { TaxID = taxID, TaxCategoryID = TaxCategoryID };

			const decimal taxableAmt = 100;
			decimal taxAmt = taxableAmt / rate;
			const decimal curyTaxableAmt = 200;
			decimal curyTaxAmt = curyTaxableAmt / rate;

			var taxTran = new TaxTran()
			{
				Module = BatchModule.GL,
				TranType = TaxTran.tranType.TranForward,
				RefNbr = BatchNbr,
				TaxID = taxID,
				TaxableAmt = taxableAmt,
				TaxAmt = taxAmt,
				CuryTaxableAmt = curyTaxableAmt,
				CuryTaxAmt = curyTaxAmt,
				TaxType = tranTaxType,
				LineRefNbr = RefNbr,
				BranchID = BranchID
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				RefNbrs = taxTran.RefNbr.SingleToList()
			};

			SetupRepositoryMethods(taxableGLTran.SingleToArray(), curyInfo, taxTran.SingleToArray(), documentIDGroup, BranchID,
				TaxPeriodID, taxID.SingleToArray(), taxCategoryDetail);

			//Action
			var recordsAggr = _glDocumentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);

			var gafRecord = tranTaxType == CSTaxType.Sales
				? (GAFRecordBase)recordsAggr.SupplyRecords.Single()
				: (GAFRecordBase)recordsAggr.PurchaseRecords.Single();

			//Assert
			Assert.Equal(taxableAmt, gafRecord.Amount);
			Assert.Equal(taxAmt, gafRecord.GSTAmount);
			Assert.Equal(ForeignCurrencyCodeForDocumentInBaseCury, gafRecord.ForeignCurrencyCode);
			Assert.Equal(0, gafRecord.ForeignCurrencyAmount);
			Assert.Equal(0, gafRecord.ForeignCurrencyAmountGST);
		}

		[Theory]
		[InlineData(CSTaxType.Sales)]
		[InlineData(CSTaxType.Use)]
		public void Test_CreateGAFRecordsForDocumentGroup_That_Sign_Is_Negative_When_Have_Reversed_Type(string tranTaxType)
		{
			//Arrange
			var taxID = _taxDataContext.VatTax.TaxID;

			var rate = _taxRevDataContext.TaxRevs.Single(taxRev => taxRev.TaxID == taxID).TaxRate.Value;

			var taxableGLTran = new GLTran()
			{
				TranDate = TranDate,
				TranDesc = TranDesc,
				BatchNbr = BatchNbr,
				RefNbr = RefNbr,
				TaxCategoryID = TaxCategoryID,
				BranchID = BranchID
			};

			var curyInfo = new CurrencyInfo() { CuryID = CuryIDEUR };
			var taxCategoryDetail = new TaxCategoryDet() { TaxID = taxID, TaxCategoryID = TaxCategoryID };

			const decimal taxableAmt = 100;
			decimal taxAmt = taxableAmt / rate;
			const decimal curyTaxableAmt = 200;
			decimal curyTaxAmt = curyTaxableAmt / rate;

			var taxTran = new TaxTran()
			{
				RefNbr = BatchNbr,
				TaxID = taxID,
				TaxableAmt = taxableAmt,
				TaxAmt = taxAmt,
				CuryTaxableAmt = curyTaxableAmt,
				CuryTaxAmt = curyTaxAmt,
				TaxType = tranTaxType,
				TranType = TaxTran.tranType.TranReversed,
				LineRefNbr = RefNbr,
				BranchID = BranchID
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				RefNbrs = taxTran.RefNbr.SingleToList()
			};

			SetupRepositoryMethods(taxableGLTran.SingleToArray(), curyInfo, taxTran.SingleToArray(), documentIDGroup, BranchID,
				TaxPeriodID, taxID.SingleToArray(), taxCategoryDetail);

			//Action
			var recordsAggr = _glDocumentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);

			var gafRecord = tranTaxType == CSTaxType.Sales
				? (GAFRecordBase)recordsAggr.SupplyRecords.Single()
				: (GAFRecordBase)recordsAggr.PurchaseRecords.Single();

			//Assert
			Assert.Equal(-taxableAmt, gafRecord.Amount);
			Assert.Equal(-taxAmt, gafRecord.GSTAmount);
			Assert.Equal(-curyTaxableAmt, gafRecord.ForeignCurrencyAmount);
			Assert.Equal(-curyTaxAmt, gafRecord.ForeignCurrencyAmountGST);
		}

		[Fact]
		[UseReporter(typeof(DiffReporter))]
		[UseApprovalFileName("__CreateGAFRecordsForDocumentGroup_Batch_Contains_Two_RefNbrs")]
		public void Test_CreateGAFRecordsForDocumentGroup_Batch_Contains_Two_RefNbrs()
		{
			//Arrange
			var taxID = _taxDataContext.VatTax.TaxID;

			var taxableGLTrans = new[]
			{
				new GLTran()
				{
					TranDate = TranDate,
					TranDesc = TranDesc,
					BatchNbr = BatchNbr,
					RefNbr = RefNbr,
					TaxCategoryID = TaxCategoryID,
					BranchID = BranchID
				},
				new GLTran()
				{
					TranDate = TranDate2,
					TranDesc = TranDesc2,
					BatchNbr = BatchNbr,
					RefNbr = RefNbr2,
					TaxCategoryID = TaxCategoryID,
					BranchID = BranchID
				},
			};

			var curyInfo = new CurrencyInfo() { CuryID = CuryIDEUR };
			var taxCategoryDetail = new TaxCategoryDet() { TaxID = taxID, TaxCategoryID = TaxCategoryID };

			var taxTrans = new[]
			{
				new TaxTran()
				{
					Module = BatchModule.GL,
					TranType = TaxTran.tranType.TranForward,
					RefNbr = BatchNbr,
					TaxID = taxID,
					TaxableAmt = 100,
					TaxAmt = 7,
					CuryTaxableAmt = 200,
					CuryTaxAmt = 14,
					TaxType = CSTaxType.Sales,
					LineRefNbr = RefNbr,
					BranchID = BranchID
				},
				new TaxTran()
				{
					Module = BatchModule.GL,
					TranType = TaxTran.tranType.TranForward,
					RefNbr = BatchNbr,
					TaxID = taxID,
					TaxableAmt = 300,
					TaxAmt = 21,
					CuryTaxableAmt = 600,
					CuryTaxAmt = 42,
					TaxType = CSTaxType.Sales,
					LineRefNbr = RefNbr2,
					BranchID = BranchID
				},
			};

			var documentIDGroup = new DocumentIDGroup()
			{
				Module = BatchModule.GL,
				RefNbrs = BatchNbr.SingleToList()
			};

			SetupRepositoryMethods(taxableGLTrans, curyInfo, taxTrans, documentIDGroup, BranchID,
				TaxPeriodID, taxID.SingleToArray(), taxCategoryDetail);

			//Action
			var recordsAggr = _glDocumentGAFRecordsCreator.CreateGAFRecordsForDocumentGroup(documentIDGroup, BranchID, TaxPeriodID);

			//Assert
			Approvals.VerifyAll(recordsAggr.SupplyRecords, "supplyRecords", record => record.Dump());
		}

		private void SetupRepositoryMethods(ICollection<GLTran> glTrans, 
											CurrencyInfo curyInfo, 
											ICollection<TaxTran> taxTrans, 
											DocumentIDGroup documentIDGroup, 
											int? branchID, 
											string taxPeriodID,
											string[] taxIDs,
											TaxCategoryDet taxCategoryDet)
		{
			GAFRepositoryMock.Setup(repo => repo.GetTaxableGLTransWithCuryInfoGroupedByDocumentAttrAndTaxCategory(branchID,
														It.Is<string[]>(refNbrs => refNbrs.SequenceEqual(documentIDGroup.RefNbrs))))
								.Returns(glTrans.Select(glTran => new PXResult<GLTran, CurrencyInfo>(glTran, curyInfo)));

			GAFRepositoryMock.Setup(repo => repo.GetTaxTransForDocuments(documentIDGroup.Module,
																			It.Is<string[]>(refNbrs => refNbrs.SequenceEqual(documentIDGroup.DocumentTypes)),
																			It.Is<string[]>(refNbrs => refNbrs.SequenceEqual(documentIDGroup.RefNbrs)), taxPeriodID))
								.Returns(taxTrans);

			GAFRepositoryMock.Setup(repo => repo.GetTaxCategoryDetsForTaxIDs(It.Is<string[]>(pTaxIDs => pTaxIDs.SequenceEqual(taxIDs))))
								.Returns(taxCategoryDet.SingleToArray());
		}
	}
}
