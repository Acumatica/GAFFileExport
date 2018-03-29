using PX.Objects.CA;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Supply.CADocuments
{
	public class SupplyRecordBuilderByCADocument : GafRecordBuilderByDocumentTranBase<SupplyRecord, CAAdj, CASplit, CATax>
	{
		public SupplyRecordBuilderByCADocument(IGAFRepository gafRepository,
			GafRecordBuilderByRegister recordBuilderByRegister) : base(gafRepository, recordBuilderByRegister)
		{
		}

		public SupplyRecord Build(CAAdj caAdj, CASplit caSplit, CATax caTax, int lineNumber)
		{
			return BuildInternal(caAdj, caSplit, caTax, lineNumber);
		}
	}
}
