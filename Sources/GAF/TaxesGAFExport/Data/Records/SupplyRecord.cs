namespace TaxesGAFExport.Data.Records
{
	/// <summary>
	/// Supply Record Element of GST File.
	/// </summary>
	public class SupplyRecord : GAFRecordBase
	{
		/// <summary>
		/// Customer Name.
		/// </summary>
		public string CustomerName { get; set; }

		/// <summary>
		/// Customer’s Business Registration Number.
		/// </summary>
		public string CustomerBRN { get; set; }

		/// <summary>
		/// Destination of goods being exported.
		/// Name of the country.
		/// </summary>
		public string Country { get; set; }
	}
}
