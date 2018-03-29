using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.TX;

namespace TaxesGAFExport.Customized
{
	public class TXInvoiceEntryExtension: PXGraphExtension<TXInvoiceEntry>
	{
		public override void Initialize()
		{
			PXUIFieldAttribute.SetDisplayName<APInvoice.docDesc>(Base.Document.Cache, Messages.ImportDeclaration);
		}
	}
}
