using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace TaxesGAFExport.Customized.DAC
{
	public class BranchBAccountExt : PXCacheExtension<BranchMaint.BranchBAccount>
	{
		#region AcctReferenceNbr
		public abstract class acctReferenceNbr : PX.Data.IBqlField
		{
		}

		[PXUIField(DisplayName = Messages.BusinessRegistrationID)]
		[PXDBString(50, IsUnicode = true, BqlField = typeof(BAccount.acctReferenceNbr))]
		public String AcctReferenceNbr { get; set; }
		#endregion
	}
}
