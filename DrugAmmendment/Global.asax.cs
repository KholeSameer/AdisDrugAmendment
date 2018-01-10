using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using DrugAmmendment.Services;
using Autofac.Integration.Mvc;

namespace DrugAmmendment
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            RegisterContainer();
        }

        private void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DrugAmmendment.Services.WebAuthorisationProvider>().As<IAuthorizationProvider>().SingleInstance();
            builder.RegisterType<DrugAmendmentConnectionService>().As<IDrugAmendmentConnectionService>().InstancePerDependency();
            builder.RegisterControllers(System.Reflection.Assembly.GetExecutingAssembly()).PropertiesAutowired();

            Autofac.IContainer container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
