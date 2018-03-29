using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;

namespace TaxesGAFExport.Customized.DAC
{
	public class CustomerExt : PXCacheExtension<Customer>
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
