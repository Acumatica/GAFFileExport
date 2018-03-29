using System.Collections.Generic;
using PX.Objects.CM;
using PX.Objects.Common.Documents;
using TaxesGAFExport.CreatorsForDocs.Invoices;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs
{
	public abstract class DocumentGAFRecordsCreatorBase<TDocument, TGAFRecord> : IDocumentGafRecordCreator<TDocument, TGAFRecord>
		where TDocument : IRegister
		where TGAFRecord: GAFRecordBase
	{
		protected readonly IGAFRepository GafRepository;

		protected IDictionary<string, TDocument> DocumentsByRefNbr;

		protected DocumentGAFRecordsCreatorBase(IGAFRepository gafRepository)
		{
			GafRepository = gafRepository;
		}

		public virtual IList<TGAFRecord> CreateGAFRecordsForDocumentGroup(DocumentGroup<TDocument> documentGroup, int? taxAgencyID, string taxPeriodID)
		{
			DocumentsByRefNbr = documentGroup.DocumentsByRefNbr;

			LoadData(documentGroup, taxAgencyID, taxPeriodID);

			var gafRecords = new List<TGAFRecord>();

			foreach (var refNbr in documentGroup.DocumentsByRefNbr.Keys)
			{
				var documentId = new DocumentID()
				{
					Module = documentGroup.Module,
					DocType = documentGroup.DocumentType,
					RefNbr = refNbr
				};

				var documentRecords = CreateGafRecordsForDocument(documentId);

				gafRecords.AddRange(documentRecords);
			}

			return gafRecords;
		}

		protected abstract void LoadData(DocumentGroup<TDocument> documentGroup, int? taxAgencyID, string taxPeriodID);

		protected abstract IList<TGAFRecord> CreateGafRecordsForDocument(DocumentID documentId);
	}
}
