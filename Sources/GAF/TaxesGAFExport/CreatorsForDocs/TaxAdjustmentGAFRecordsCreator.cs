using System.Collections.Generic;
using System.Linq;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs
{
	public class TaxAdjustmentGAFRecordsCreator: CustomGAFRecordsCreatorBase
	{
		private readonly GafRecordBuilderByTaxAdjustmentTaxTran _recordBuilderByTaxAdjustmentTaxTran;

		private IDictionary<string, TaxAdjustment> _taxAdjustmentsByRefNbr;

		public TaxAdjustmentGAFRecordsCreator(IGAFRepository gafRepository, GafRecordBuilderByTaxAdjustmentTaxTran recordBuilderByTaxAdjustmentTaxTran)
			:base(gafRepository)
		{
			_recordBuilderByTaxAdjustmentTaxTran = recordBuilderByTaxAdjustmentTaxTran;
		}

		public DocumentRecordsAggregate CreateGAFRecordsForDocumentGroup(DocumentIDGroup documentIDGroup, int? branchID, string taxPeriodID)
		{
			LoadData(documentIDGroup, branchID, taxPeriodID);

			var resultRecordsAggregate = new DocumentRecordsAggregate();

			foreach (var refNbr in documentIDGroup.RefNbrs)
			{
				var documentId = new DocumentID()
				{
					Module = documentIDGroup.Module,
					DocType = documentIDGroup.DocumentType,
					RefNbr = refNbr
				};

				var documentRecordsAggregate = CreateRecordsByDocument(documentId);

				resultRecordsAggregate.PurchaseRecords.AddRange(documentRecordsAggregate.PurchaseRecords);
				resultRecordsAggregate.SupplyRecords.AddRange(documentRecordsAggregate.SupplyRecords);
			}

			return resultRecordsAggregate;
		}

		protected override void LoadData(DocumentIDGroup documentIDGroup, int? branchID, string taxPeriodID)
		{
			base.LoadData(documentIDGroup, branchID, taxPeriodID);

			var refNbrs = documentIDGroup.RefNbrs.ToArray();

			_taxAdjustmentsByRefNbr = GafRepository.GetTaxAdjustments(documentIDGroup.DocumentType, refNbrs)
												.ToDictionary(taxAdj => taxAdj.RefNbr, taxAdj => taxAdj);

			
		}

		protected override DocumentRecordsAggregate CreateRecordsByDocument(DocumentID documentId)
		{
			var resultRecords = new DocumentRecordsAggregate();

			var taxTrans = TaxTransByRefNbr[documentId.RefNbr].OrderBy(taxTran => taxTran.RecordID);
			var lineNumber = 1;

			foreach (var taxTran in taxTrans)
			{
				if ((documentId.DocType == TaxAdjustmentType.AdjustInput && Taxes[taxTran.TaxID].ReverseTax != true)
				    || (documentId.DocType == TaxAdjustmentType.AdjustOutput && Taxes[taxTran.TaxID].ReverseTax == true)
				    || Taxes[taxTran.TaxID].TaxType == CSTaxType.Withholding)
				{
					var purchaseRecord = new PurchaseRecord();

					_recordBuilderByTaxAdjustmentTaxTran.Build(purchaseRecord, _taxAdjustmentsByRefNbr[documentId.RefNbr], taxTran, lineNumber);

					resultRecords.PurchaseRecords.Add(purchaseRecord);
				}
				else
				{
					var supplyRecord = new SupplyRecord();

					_recordBuilderByTaxAdjustmentTaxTran.Build(supplyRecord, _taxAdjustmentsByRefNbr[documentId.RefNbr], taxTran, lineNumber);

					resultRecords.SupplyRecords.Add(supplyRecord);
				}

				lineNumber++;
			}

			return resultRecords;
		}
	}
}
