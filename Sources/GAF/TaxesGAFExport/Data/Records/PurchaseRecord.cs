namespace TaxesGAFExport.Data.Records
{
	/// <summary>
	///  Purchase Record of GST File.
	/// </summary>
	public class PurchaseRecord : GAFRecordBase
	{
		/// <summary>
		/// Supplier Name.
		/// </summary>
		public string SupplierName { get; set; }

		/// <summary>
		/// Supplier’s Business Registration Number.
		/// </summary>
		public string SupplierBRN { get; set; }

		/// <summary>
		/// Import Declaration Number.
		/// </summary>
		public string ImportDeclarationNumber { get; set; }
	}
}
