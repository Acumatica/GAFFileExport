using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.TX;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport
{
	public class GAFValidator
	{
		private readonly IGAFRepository _gafRepository;

		public GAFValidator(IGAFRepository gafRepository)
		{
			_gafRepository = gafRepository;
		}

		public ProcessingResult ValidateTaxYearStructure(TaxPeriod taxPeriod)
		{
			var finPeriodWithSameStartDate = _gafRepository.FindFinPeriodWithStartDate(taxPeriod.StartDate);
			var finPeriodWithSameEndDate = _gafRepository.FindFinPeriodWithEndDate(taxPeriod.EndDate);

			if (finPeriodWithSameStartDate == null || finPeriodWithSameEndDate == null)
			{
				var result = new ProcessingResult();
				result.AddErrorMessage(Messages.TheGAFFileCannotBePrepared);
				return result;
			}

			return ProcessingResult.Success;
		}

		public ProcessingResult CheckUnreleasedAndUnpostedDocumentsDoNotExist(int? branchID, TaxPeriod taxPeriod)
		{
			var lastFinPeriod = _gafRepository.FindFinPeriodWithEndDate(taxPeriod.EndDate);

			var result = new ProcessingResult();
			var modulesWithUnreleasedDocuments = new List<string>();

			if (!_gafRepository.CheckDoNotExistUnreleasedAPRegistersWithAPTransWithBranchWithFinPeriodLessOrEqual(branchID, lastFinPeriod.FinPeriodID))
			{
				modulesWithUnreleasedDocuments.Add(BatchModule.AP);
			}
			if (!_gafRepository.CheckDoNotExistUnreleasedARRegistersWithARTransWithBranchWithFinPeriodLessOrEqual(branchID, lastFinPeriod.FinPeriodID))
			{
				modulesWithUnreleasedDocuments.Add(BatchModule.AR);
			}
			if (!_gafRepository.CheckDoNotExistUnreleasedCAAdjsWithCASplitsWithBranchWithFinPeriodLessOrEqual(branchID, lastFinPeriod.FinPeriodID))
			{
				modulesWithUnreleasedDocuments.Add(BatchModule.CA);
			}

			if (modulesWithUnreleasedDocuments.Any())
			{
				result.AddMessage(PXErrorLevel.Warning, Messages.ThereAreUnreleasedDocumentsInModules,
					modulesWithUnreleasedDocuments.JoinIntoStringForMessage(edgingSymbol: null));
			}

			if (!_gafRepository.CheckDoNotExistUnreleasedOrUnpostedGLTransWithBranchWithFinPeriodLessOrEqual(branchID, lastFinPeriod.FinPeriodID))
			{
				result.AddMessage(PXErrorLevel.Warning, Messages.ThereAreUnpostedTransactions);
			}

			return result;
		}

		public ProcessingResult CheckGAFGenerationRequirements(int? branchID, TaxPeriod taxPeriod)
		{
			var result = ValidateTaxYearStructure(taxPeriod);
			var documentCheckingResult = CheckUnreleasedAndUnpostedDocumentsDoNotExist(branchID, taxPeriod);

			result.Aggregate(documentCheckingResult);

			return result;
		}
	}
}
