using PX.Objects.CA;
using TaxesGAFExport.Data.Records;
using TaxesGAFExport.Repositories;

namespace TaxesGAFExport.CreatorsForDocs.Builders.Purchase
{
	public class PurchaseRecordBuilderByCADocument: GafRecordBuilderByDocumentTranBase<PurchaseRecord, CAAdj, CASplit, CATax>
	{
		public PurchaseRecordBuilderByCADocument(IGAFRepository gafRepository,
			GafRecordBuilderByRegister recordBuilderByRegister) : base(gafRepository, recordBuilderByRegister)
		{
		}

		public PurchaseRecord Build(CAAdj caAdj, CASplit caSplit, CATax caTax, int lineNumber)
		{
			return BuildInternal(caAdj, caSplit, caTax, lineNumber);
		}
	}
}
