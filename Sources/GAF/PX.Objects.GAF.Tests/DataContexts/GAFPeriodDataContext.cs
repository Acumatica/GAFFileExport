using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.Tests;
using PX.Objects.Tests.Infrastucture;
using TaxesGAFExport.Data;

namespace PX.Objects.GAF.Tests.DataContexts
{
	public class GAFPeriodDataContext : DataContextBase
	{
		private GAFPeriod _gafPeriod;

		public GAFPeriod GAFPeriod
		{
			get
			{
				if (_gafPeriod == null)
				{
					_gafPeriod = new GAFPeriod()
					{
						NoteID = Guid.Parse("189bf53fcfcce511973d10c37b4dfd7d"),
						BranchID = 76,
						TaxAgencyID = 7010,
						TaxPeriodID = "201503",
						GAFMajorVersion = 1,
						GAFMinorLastVersion = 1
					};
				}

				return _gafPeriod;
			}
		}
	}
}
