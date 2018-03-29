using System.Collections.Generic;

namespace TaxesGAFExport.Data.Records
{
	public class DocumentRecordsAggregate
	{
		public IList<PurchaseRecord> PurchaseRecords { get; set; }

		public IList<SupplyRecord> SupplyRecords { get; set; }

		public DocumentRecordsAggregate()
		{
			PurchaseRecords = new List<PurchaseRecord>();
			SupplyRecords = new List<SupplyRecord>();
		}
	}
}
