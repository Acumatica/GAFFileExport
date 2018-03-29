using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Moq;
using PX.Objects.GAF.Tests.Repositories;
using PX.Objects.Tests;
using PX.Objects.Tests.AP.DataContexts;
using PX.Objects.Tests.AR.DataContexts;
using PX.Objects.Tests.Common.DataContexts;
using PX.Objects.Tests.Infrastucture;
using PX.Objects.Tests.SO.DataContexts;
using PX.Objects.Tests.TX.DataContexts;

namespace PX.Objects.GAF.Tests
{
	public class GAFTestsBase : TestsBase
	{
		protected const string ForeignCurrencyCodeForDocumentInBaseCury = "XXX";

		protected Mock<TestGAFRepository> GAFRepositoryMock { get; set; }

		protected TestGAFRepository GAFRepository
		{
			get { return GAFRepositoryMock.Object; }
		}

		protected TaxDataContext TaxDataContext;
		protected CompanyDataContext CompanyDataContext;

		public GAFTestsBase()
		{
			TaxDataContext = GetService<TaxDataContext>();
			CompanyDataContext = GetService<CompanyDataContext>();

			GAFRepositoryMock = new Mock<TestGAFRepository>(GetService<TaxDataContext>(),
															GetService<VendorDataContext>(),
															GetService<LocationDataContext>(),
															GetService<ContactDataContext>(),
															GetService<CompanyDataContext>(),
															GetService<CustomerDataContext>(),
															GetService<ARAddressDataContext>(),
															GetService<CountryDataContext>(),
															GetService<SOAddressDataContext>())
															{CallBase = true};
		}

		protected override void RegisterModules(ContainerBuilder builder)
		{
			base.RegisterModules(builder);

			builder.RegisterModule<GAFTestsBaseModule>();
		}
	}
}
