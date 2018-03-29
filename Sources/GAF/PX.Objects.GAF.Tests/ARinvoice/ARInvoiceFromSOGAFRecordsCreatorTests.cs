using System;
using System.Linq;
using ApprovalTests.Reporters;
using Moq;
using PX.Objects.AR;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.Infrastucture.ApprovalTests;
using PX.Objects.Tests.SO.DataContexts;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices;
using TaxesGAFExport.CreatorsForDocs.Builders.Supply.Invoices.CountryBuilding;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using Xunit;

namespace PX.Objects.GAF.Tests.ARinvoice
{
	public class ARInvoiceFromSOGAFRecordsCreatorTests : ARInvoiceGAFRecordsCreatorTestsBase
	{
		private SOAddressDataContext _soAddressDataContext;

		public ARInvoiceFromSOGAFRecordsCreatorTests()
		{
			InvoiceGafRecordsCreator = new ARInvoiceFromSOGAFRecordsCreator(GAFRepository,
																			new SupplyRecordBuilderBySOInvoice(GAFRepository, 
																												new GafRecordBuilderByRegister(GAFRepository),
																												new SupplyRecordBuilderByCustomerData(GAFRepository),
																												new SupplyRecordCountryBuilderForSOInvoice(GAFRepository)),
																			new SupplyRecordBuilderBySOInvoiceTaxTranForTaxCalcedOnDocumentAmt(GAFRepository,
																																	new GafRecordBuilderByRegister(GAFRepository),
																																	new SupplyRecordBuilderByCustomerData(GAFRepository),
																																	new SupplyRecordCountryBuilderForSOInvoice(GAFRepository)));

			_soAddressDataContext = GetService<SOAddressDataContext>();
		}

		[Fact]
		[UseReporter(typeof (DiffReporter))]
		[UseApprovalFileName("__SO__CreateGAFRecordsForDocumentGroup__Two_Taxes__")]
		public void Test_CreateGAFRecordsForDocumentGroup_For_Documents_With_Two_Taxes_And_Custom_LineNumbers()
		{
			//Arrange
			Action setup = delegate ()
			{
				var soInvoices = new[]
				{
					new SOInvoice()
					{
						DocType = ARDocType.Invoice,
						RefNbr = DocumentDataContext.RefNbr,
						ShipAddressID = _soAddressDataContext.CustomerAddress.AddressID
					},
					new SOInvoice()
					{
						DocType = ARDocType.Invoice,
						RefNbr = DocumentDataContext.RefNbr2,
						ShipAddressID = _soAddressDataContext.Customer2Address.AddressID
					}
				};

				GAFRepositoryMock.Setup(
					repo => repo.GetSOInvoices(It.IsAny<string>(), It.IsAny<string[]>()))
					.Returns<string, string[]>((docType, refNbrs) => soInvoices.Where(soInvoice => soInvoice.DocType == docType &&
					                                                                               refNbrs.Contains(soInvoice.RefNbr)));
			};

			Test_CreateGAFRecordsForDocumentGroup_For_Documents_With_Two_Tax_And_Custom_LineNumbers(BatchModule.SO, setup);
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_That_Country_Field_Is_Null_When_Country_Of_Ship_Address_Is_Malaysia()
		{
			Action setup = delegate ()
			{
				SetupGetSOInvoices();

				GAFRepositoryMock.Setup(repo => repo.GetSOAddress(_soAddressDataContext.CustomerAddress.AddressID))
					.Returns(new SOAddress() { CountryID = SupplyRecordCountryBuilderForSOInvoice.MalaysiaCountryCode });
			};

			Test_CreateGAFRecordsForDocumentGroup_That_Country_Field_Is_Null_When_Sale_Country_Is_Malaysia(BatchModule.SO, setup);
		}

		[Fact]
		public void Test_CreateGAFRecordsForDocumentGroup_When_Tax_Calced_On_Document_Amount()
		{
			Test_CreateGAFRecordsForDocumentGroup_When_Tax_Calced_On_Document_Amount(BatchModule.SO, SetupGetSOInvoices);
		}

		private void SetupGetSOInvoices()
		{
			GAFRepositoryMock.Setup(repo => repo.GetSOInvoices(ARDocType.Invoice, new[] { DocumentDataContext.RefNbr }))
					.Returns(() => new SOInvoice()
					{
						DocType = ARDocType.Invoice,
						RefNbr = DocumentDataContext.RefNbr,
						ShipAddressID = _soAddressDataContext.CustomerAddress.AddressID
					}.SingleToArray());
		}
	}
}
