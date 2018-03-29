using System.Collections.Generic;
using PX.Objects.Common.Documents;

namespace TaxesGAFExport.CreatorsForDocs.Invoices
{
	public interface IDocumentGafRecordCreator<TDocument, TGAFRecord>
	{
		IList<TGAFRecord> CreateGAFRecordsForDocumentGroup(DocumentGroup<TDocument> documentGroup, int? taxAgencyID, string taxPeriodID);
	}
}
