using System;

namespace TaxesGAFExport.Data.Records
{
	/// <summary>
	/// C Record Element of GST File.
	/// </summary>
	public class CompanyRecord
	{
		/// <summary>
		/// Company Name.
		/// </summary>
		public string CompanyName { get; set; }

		/// <summary>
		/// Company’s Business Registration Number.
		/// </summary>
		public string CompanyBRN { get; set; }

		/// <summary>
		/// Company’s GST Number.
		/// </summary>
		public string CompanyGSTNumber { get; set; }

		/// <summary>
		/// GST Filing Period Start Date.
		/// </summary>
		public DateTime PeriodStartDate { get; set; }

		/// <summary>
		/// GST Filing Period End Date.
		/// </summary>
		public DateTime PeriodEndDate { get; set; }

		/// <summary>
		/// GAF file creation date.
		/// </summary>
		public DateTime FileCreationDate { get; set; }

		/// <summary>
		/// Accounting software name and version.
		/// </summary>
		public string ProductVersion { get; set; }

		/// <summary>
		/// GAF version number.
		/// </summary>
		public string GAFVersion { get; set; }
	}
}
