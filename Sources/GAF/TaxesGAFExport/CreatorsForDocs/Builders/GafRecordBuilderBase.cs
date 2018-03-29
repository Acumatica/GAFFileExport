using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders
{
	public abstract class GafRecordBuilderBase
	{
		public const string ForeignCurrencyCodeForBaseCuryID = "XXX";

		protected readonly IGAFRepository GafRepository;

		#region BaseCuryID
		private string _baseCuryID;

		protected string BaseCuryID
		{
			get
			{
				if (_baseCuryID == null)
				{
					_baseCuryID = GafRepository.GetCompany().BaseCuryID;
				}

				return _baseCuryID;
			}
		}

		#endregion

		protected GafRecordBuilderBase(IGAFRepository gafRepository)
		{
			GafRepository = gafRepository;
		}

		protected string GetForeignCurrencyCode(string curyID)
		{
			return curyID != BaseCuryID
				? curyID
				: ForeignCurrencyCodeForBaseCuryID;
		}

		protected decimal GetForeignCurrencyAmount(string curyID, decimal amount)
		{
			return curyID != BaseCuryID
				? amount
				: 0;
		}
	}
}
