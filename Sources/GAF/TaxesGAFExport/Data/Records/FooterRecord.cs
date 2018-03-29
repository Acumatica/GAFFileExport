
namespace TaxesGAFExport.Data.Records
{
	public class FooterRecord
	{
		#region Purchase

		/// <summary>
		/// Number of ‘P’ records
		/// </summary>
		public int PurchaseRecordsCount;

		/// <summary>
		/// Sum of all purchases
		/// </summary>
		public decimal PurchaseAmountSum;

		/// <summary>
		/// Sum of all GST on purchases
		/// </summary>
		public decimal PurchaseGSTAmountSum;

		#endregion


		#region Supply

		/// <summary>
		/// Number of ‘S’ records
		/// </summary>
		public int SupplyRecordsCount;

		/// <summary>
		/// Sum of all supplies
		/// </summary>
		public decimal SupplyAmountSum;

		/// <summary>
		/// Sum of GST on all supplies
		/// </summary>
		public decimal SupplyGSTAmountSum;

		#endregion


		#region Ledger

		/// <summary>
		/// Number of ‘L’ records
		/// </summary>
		public int LedgerRecordsCount;

		/// <summary>
		/// Sum of all debits on all ‘L’ records
		/// </summary>
		public decimal DebitSum;

		/// <summary>
		/// Sum of all credits on all ‘L’ records
		/// </summary>
		public decimal CreditSum;

		/// <summary>
		/// Sum of all ledger record closing balances
		/// </summary>
		public decimal BalanceSum;

		#endregion

	}
}