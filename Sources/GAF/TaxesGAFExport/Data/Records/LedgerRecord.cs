using System;

namespace TaxesGAFExport.Data.Records
{
	/// <summary>
	/// Ledger record of GST File
	/// </summary>
	public class LedgerRecord
	{
		/// <summary>
		/// Transaction date.
		/// </summary>
		public DateTime TransactionDate { get; set; }

		/// <summary>
		/// General ledger account number.
		/// </summary>
		public string AccountID { get; set; }

		/// <summary>
		/// General ledger account name.
		/// </summary>
		public string AccountName { get; set; }

		/// <summary>
		/// Transaction Description.
		/// </summary>
		public string TransactionDescription { get; set; }

		/// <summary>
		/// Name of entity involved. 
		/// Customer or Vendor.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// A unique number that can group related double entries together. 
		/// Batch number.
		/// </summary>
		public string TransactionID { get; set; }

		/// <summary>
		/// Source document number to which the line relates .
		/// Ref. Number of the transaction.
		/// </summary>
		public string SourceDocumentID { get; set; }

		/// <summary>
		/// Refers to the type of transaction.
		/// Module.
		/// </summary>
		public string SourceType { get; set; }

		/// <summary>
		/// Debit Amount in the base currency.
		/// </summary>
		public decimal DebitAmount { get; set; }

		/// <summary>
		/// Credit Amount in the base currency.
		/// </summary>
		public decimal CreditAmount { get; set; }

		/// <summary>
		/// Account balance.
		/// </summary>
		public decimal BalanceAmount { get; set; }
	}
}
