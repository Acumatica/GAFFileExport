using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR.MassProcess;

namespace TaxesGAFExport.Customized.DAC
{
	public class VendorExt: PXCacheExtension<Vendor>
	{
		#region AcctReferenceNbr
		public abstract class acctReferenceNbr : PX.Data.IBqlField
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), Messages.BusinessRegistrationID)]
		public String AcctReferenceNbr { get; set; }
		#endregion
	}
}
