using System.Collections.Generic;
using System.Linq;
using PX.Objects.Common.Documents;
using PX.Objects.TX;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs
{
	public abstract class CustomGAFRecordsCreatorBase
	{
		protected readonly IGAFRepository GafRepository;

		protected IDictionary<string, List<TaxTran>> TaxTransByRefNbr;

		protected IDictionary<string, Tax> Taxes;

		protected CustomGAFRecordsCreatorBase(IGAFRepository gafRepository)
		{
			GafRepository = gafRepository;
		}

		protected virtual void LoadData(DocumentIDGroup documentIDGroup, int? branchID, string taxPeriodID)
		{
			var refNbrs = documentIDGroup.RefNbrs.ToArray();

			TaxTransByRefNbr = GafRepository.GetTaxTransForDocuments(documentIDGroup.Module, documentIDGroup.DocumentTypes.ToArray(), refNbrs, taxPeriodID)
											.GroupBy(taxTran => taxTran.RefNbr)
											.ToDictionary(group => group.Key, group => group.ToList());

			var usedTaxIDs = TaxTransByRefNbr.Values.SelectMany(taxTranList => taxTranList.Select(taxTran => taxTran.TaxID))
													.Distinct()
													.ToArray();

			Taxes = GafRepository.GetTaxesByIDs(usedTaxIDs)
									.ToDictionary(tax => tax.TaxID, tax => tax);
		}

		protected abstract DocumentRecordsAggregate CreateRecordsByDocument(DocumentID documentID);
	}
}
