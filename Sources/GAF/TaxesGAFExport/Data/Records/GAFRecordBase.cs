using System;

namespace TaxesGAFExport.Data.Records
{
	public abstract class GAFRecordBase
	{
		/// <summary>
		/// Invoice Date.
		/// </summary>
		public DateTime InvoiceDate { get; set; }

		/// <summary>
		/// Invoice Number.
		/// </summary>
		public string InvoiceNumber { get; set; }

		/// <summary>
		/// Number of invoice line.
		/// </summary>
		public int LineNumber { get; set; }

		/// <summary>
		/// Description for what was sold.
		/// </summary>
		public string ProductDescription { get; set; }

		/// <summary>
		/// Tax Code.
		/// </summary>
		public string TaxCode { get; set; }

		/// <summary>
		/// ISO currency code.
		/// </summary>
		public string ForeignCurrencyCode { get; set; }

		/// <summary>
		/// Value of line excluding GST in Malaysia Ringgit (Base Currency) per invoice line.
		/// Taxable amount.
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Value of GST in Malaysia Ringgit (base currency) per invoice line.
		/// Tax amount.
		/// </summary>
		public decimal GSTAmount { get; set; }

		/// <summary>
		/// Value of line excluding GST in Foreign currency (if applicable) per invoice line.
		/// Taxable amount.
		/// </summary>
		public decimal ForeignCurrencyAmount { get; set; }

		/// <summary>
		/// Value of GST in Foreign currency (if applicable).
		/// Tax amount.
		/// </summary>
		public decimal ForeignCurrencyAmountGST { get; set; }

	}
}
