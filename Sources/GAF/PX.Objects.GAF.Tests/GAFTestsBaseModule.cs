using Autofac;
using PX.Objects.GAF.Tests.Repositories;
using PX.Objects.Tests;
using PX.Objects.Tests.AP.Builders;
using PX.Objects.Tests.AR.Builders;
using PX.Objects.Tests.Infrastucture;
using PX.Objects.Tests.TX.DataContexts;

namespace PX.Objects.GAF.Tests
{
	public class GAFTestsBaseModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			ServiceRegistrationHelper.RegisterDataContextFromAssembly(ThisAssembly, builder);
			ServiceRegistrationHelper.RegisterDataContextFromAssembly(typeof(TaxDataContext).Assembly, builder);

			builder.RegisterType<APTaxDataBuilder>();
			builder.RegisterType<ARTaxDataBuilder>();
			builder.RegisterType<APInvoiceAggregateBuilderFactory>();
			builder.RegisterType<ARInvoiceAggregateBuilderFactory>();
		}
	}
}
