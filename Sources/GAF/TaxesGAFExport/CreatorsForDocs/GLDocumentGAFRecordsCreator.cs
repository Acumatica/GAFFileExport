using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common.Documents;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.TX;
using TaxesGAFExport.CreatorsForDocs.Builders;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs
{
	public class GLDocumentGAFRecordsCreator : CustomGAFRecordsCreatorBase
	{
		private IDictionary<TaxableGLTranKey, PXResult<GLTran, CurrencyInfo>> _taxableGLTransWithCuryInfo;

		private IDictionary<string, TaxCategoryDet[]> _taxCategoryDetsByTaxID;

		private readonly GafRecordBuilderByGLTranAndTaxTran _recordBuilderByGLTranAndTaxTran;

		public GLDocumentGAFRecordsCreator(IGAFRepository gafRepository, GafRecordBuilderByGLTranAndTaxTran recordBuilderByGLTranAndTaxTran)
			: base(gafRepository)
		{
			_recordBuilderByGLTranAndTaxTran = recordBuilderByGLTranAndTaxTran;
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

			_taxableGLTransWithCuryInfo = GafRepository.GetTaxableGLTransWithCuryInfoGroupedByDocumentAttrAndTaxCategory(branchID, refNbrs)
														.ToDictionary(row =>
														{
															var tran = (GLTran) row;
															return new TaxableGLTranKey()
															{
																BranchID = tran.BranchID,
																RefNbr = tran.RefNbr,
																BatchNbr = tran.BatchNbr,
																TaxableCategoryID = tran.TaxCategoryID
															};
														}
														);

			_taxCategoryDetsByTaxID = GafRepository.GetTaxCategoryDetsForTaxIDs(Taxes.Keys.ToArray())
													.GroupBy(taxCategoryDet => taxCategoryDet.TaxID, taxCategoryDet => taxCategoryDet)
													.ToDictionary(group => group.Key, group => group.ToArray());
		}

		protected override DocumentRecordsAggregate CreateRecordsByDocument(DocumentID documentId)
		{
			var resultRecords = new DocumentRecordsAggregate();

			var taxTranGroupsByLineRefNbr = TaxTransByRefNbr[documentId.RefNbr].OrderBy(taxTran => taxTran.RecordID)
																				.GroupBy(taxTran => taxTran.LineRefNbr);

			foreach (var taxTranGroup in taxTranGroupsByLineRefNbr)
			{
				var lineNumber = 1;

				foreach (var taxTran in taxTranGroup)
				{
					var glTranWithCuryInfo = GetGLTranWithCuryInfo(taxTran);

					var glTran = (GLTran)glTranWithCuryInfo;
					var curyInfo = (CurrencyInfo)glTranWithCuryInfo;

					if ((taxTran.TaxType == CSTaxType.Use && Taxes[taxTran.TaxID].ReverseTax != true)
						|| (taxTran.TaxType == CSTaxType.Sales && Taxes[taxTran.TaxID].ReverseTax == true)
						|| Taxes[taxTran.TaxID].TaxType == CSTaxType.Withholding)
					{
						var purchaseRecord = new PurchaseRecord();

						_recordBuilderByGLTranAndTaxTran.Build(purchaseRecord, glTran, curyInfo.CuryID, taxTran,
							lineNumber);

						ApplySign(purchaseRecord, taxTran.Module, taxTran.TranType);

						resultRecords.PurchaseRecords.Add(purchaseRecord);
					}
					else
					{
						var supplyRecord = new SupplyRecord();

						_recordBuilderByGLTranAndTaxTran.Build(supplyRecord, glTran, curyInfo.CuryID, taxTran,
							lineNumber);

						ApplySign(supplyRecord, taxTran.Module, taxTran.TranType);

						resultRecords.SupplyRecords.Add(supplyRecord);
					}

					lineNumber++;
				}
			}

			return resultRecords;
		}


		private PXResult<GLTran, CurrencyInfo> GetGLTranWithCuryInfo(TaxTran taxTran)
		{
			foreach (var taxCategoryDet in _taxCategoryDetsByTaxID[taxTran.TaxID])
			{
				var taxableGLTranKey = new TaxableGLTranKey()
				{
					BranchID = taxTran.BranchID,
					RefNbr = taxTran.LineRefNbr,
					BatchNbr = taxTran.RefNbr,
					TaxableCategoryID = taxCategoryDet.TaxCategoryID
				};

				if (_taxableGLTransWithCuryInfo.ContainsKey(taxableGLTranKey))
				{
					return _taxableGLTransWithCuryInfo[taxableGLTranKey];
				}
			}

			return null;
		}

		protected void ApplySign(GAFRecordBase gafRecord, string taxTranModule, string docType)
		{
			var sign = ReportTaxProcess.GetMultByTranType(taxTranModule, docType);

			if (sign < 0)
			{
				gafRecord.Amount *= sign;
				gafRecord.GSTAmount *= sign;
				gafRecord.ForeignCurrencyAmount *= sign;
				gafRecord.ForeignCurrencyAmountGST *= sign;
			}
		}

		private struct TaxableGLTranKey
		{
			public int? BranchID { get; set; }
			public string BatchNbr { get; set; }
			public string RefNbr { get; set; }
			public string TaxableCategoryID { get; set; }
		}
	}
}