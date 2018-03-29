using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.TX;

namespace TaxesGAFExport.Data
{
	public class GAFPeriod : IBqlTable, IDACWithNote
	{
		#region BranchID

		public abstract class branchID : PX.Data.IBqlField
		{
		}

		protected int? _BranchID;

		[MasterBranch(IsKey = true)]
		public virtual int? BranchID
		{
			get { return this._BranchID; }
			set { this._BranchID = value; }
		}

		#endregion
		#region TaxAgencyID

		public abstract class taxAgencyID : PX.Data.IBqlField
		{
		}

		protected int? _TaxAgencyID;

		[PXDefault]
		[Vendor(typeof(Search<Vendor.bAccountID,
							Where<Vendor.taxAgency, Equal<True>>>), DisplayName = "Tax Agency",
				IsKey = true)]
		public virtual int? TaxAgencyID
		{
			get { return this._TaxAgencyID; }
			set { this._TaxAgencyID = value; }
		}

		#endregion
		#region TaxPeriodID

		public abstract class taxPeriodID : PX.Data.IBqlField
		{
		}

		protected string _TaxPeriodID;

		[FinPeriodID(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Reporting Period")]
		[PXSelector(
			typeof(Search2<TaxPeriod.taxPeriodID,
				InnerJoin<Branch,
					On<TaxPeriod.branchID, Equal<Branch.parentBranchID>>>,
				Where<TaxPeriod.vendorID, Equal<Current<GAFPeriod.taxAgencyID>>,
						And<Branch.branchID, Equal<Current<GAFPeriod.branchID>>,
						And<Where<TaxPeriod.status, Equal<TaxPeriodStatus.prepared>,
									Or<TaxPeriod.status, Equal<TaxPeriodStatus.closed>>>>>>>),
			SelectorMode = PXSelectorMode.NoAutocomplete,
			DirtyRead = true)]
		public virtual string TaxPeriodID
		{
			get { return this._TaxPeriodID; }
			set { this._TaxPeriodID = value; }
		}

		#endregion
		#region GAFMajorVersion

		public abstract class gafMajorVersion : PX.Data.IBqlField
		{
		}

		protected int? _GAFMajorVersion;

		/// <summary>
		/// GAF major version number
		/// </summary>
		[PXDBInt]
		[PXDefault]
		public int? GAFMajorVersion
		{
			get { return this._GAFMajorVersion; }
			set { this._GAFMajorVersion = value; }
		}

		#endregion
		#region GAFMinorLastVersion

		public abstract class gafMinorLastVersion : PX.Data.IBqlField
		{
		}

		protected int? _GAFMinorLastVersion;

		/// <summary>
		/// Last GAF minor version number
		/// </summary>
		[PXDBInt]
		[PXDefault]
		public int? GAFMinorLastVersion
		{
			get { return this._GAFMinorLastVersion; }
			set { this._GAFMinorLastVersion = value; }
		}

		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}

		protected Guid? _NoteID;

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.IBqlField {}
		protected DateTime? _StartDate;
		[PXDate]
		[PXUIField(DisplayName = "From", Enabled = false)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDateUI
		public abstract class endDateUI : PX.Data.IBqlField {}

		protected DateTime? _EndDateUI;

		[PXDate]
		[PXUIField(DisplayName = "To", Enabled = false)]
		public virtual DateTime? EndDateUI
		{
			get
			{
				return this._EndDateUI;
			}
			set
			{
				this._EndDateUI = value;
			}
		}
		#endregion
	}
}