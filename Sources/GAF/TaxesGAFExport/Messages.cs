using PX.Common;

namespace TaxesGAFExport
{
	[PXLocalizable]
	public static class Messages
	{
		public const string TheTypeOfGAFRecordPurchaseOrSupplyCannotBeDefined = "The type of GAF Record (purchase or supply) cannot be defined for '{0}'. Please contact support service.";
		public const string TaxDocumentTypeOfTaxTransactionCannotBeIdentified = "Tax document type '{0}' of tax transaction '{1}' cannot be identified . Please contact support service.";
		public const string TheGAFFileCannotBePrepared = "The GAF file cannot be prepared because the start date of the tax year does not match the start date of the financial year.";
		public const string ThereAreUnreleasedDocumentsInModules = "There are unreleased documents in {0} module(s) in the selected period or in previous periods. These documents will not be included in the GAF file.";
		public const string ThereAreUnpostedTransactions = "There are unposted transactions in the selected period or in previous periods. These transactions will not be included in the GAF file.";

		public const string ImportDeclaration = "Import Declaration";
		public const string BusinessRegistrationID = "Business Registration ID";
	}
}
