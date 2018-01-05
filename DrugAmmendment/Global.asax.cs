using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using DrugAmmendment.Services;
using Autofac.Integration.Web;
using Autofac.Integration.Mvc;
using Autofac.Configuration;

namespace DrugAmmendment
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RegisterContainer();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
        public static IContainerProvider ContainerProvider { get; private set; }
        public void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DrugAmmendment.Services.WebAuthorisationProvider>().As<IAuthorizationProvider>().SingleInstance();
            builder.RegisterModule(new ConfigurationSettingsReader("autofac_config"));

            Autofac.IContainer container = null;
            container = builder.Build();

            ContainerProvider = new ContainerProvider(container);

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
