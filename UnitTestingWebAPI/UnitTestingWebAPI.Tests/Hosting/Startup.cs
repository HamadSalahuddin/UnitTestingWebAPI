using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Moq;
using Owin;
using UnitTestingWebAPI.API;
using UnitTestingWebAPI.API.Controllers;
using UnitTestingWebAPI.API.Filters;
using UnitTestingWebAPI.API.HeaderAppenderHandler;
using UnitTestingWebAPI.API.MessageHandlers;
using UnitTestingWebAPI.Data;
using UnitTestingWebAPI.Data.Infrastructure;
using UnitTestingWebAPI.Data.Repositories;
using UnitTestingWebAPI.Domain;
using UnitTestingWebAPI.Services;

[assembly: OwinStartup(typeof(UnitTestingWebAPI.Tests.Hosting.Startup))]

namespace UnitTestingWebAPI.Tests.Hosting
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.MessageHandlers.Add(new HeaderAppenderHandler());
            config.MessageHandlers.Add(new EndRequestHandler());
            config.Filters.Add(new ArticlesReversedFilter());
            config.Services.Replace(typeof(IAssembliesResolver), new CustomAssembliesResolver());

            // Routing
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.MapHttpAttributeRoutes();

            // Autofac Configuration

            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(typeof(ArticlesController).Assembly);

            // Unit of work.
            var unitOfWork = new Mock<IUnitOfWork>();
            builder.RegisterInstance(unitOfWork.Object).As<IUnitOfWork>();

            // Repositories.
            var articlesRepository = new Mock<IArticleRepository>();
            articlesRepository.Setup(r => r.GetAll()).Returns(
                BloggerInitializer.GetAllArticles()
            );
            builder.RegisterInstance(articlesRepository.Object).As<IArticleRepository>();

            var _blogsRepository = new Mock<IBlogRepository>();
            _blogsRepository.Setup(x => x.GetAll()).Returns(
                BloggerInitializer.GetBlogs
                );
            builder.RegisterInstance(_blogsRepository.Object).As<IBlogRepository>();

            // Services
            builder.RegisterAssemblyTypes(typeof(ArticleService).Assembly)
               .Where(t => t.Name.EndsWith("Service"))
               .AsImplementedInterfaces().InstancePerRequest();

            builder.RegisterInstance(new ArticleService(articlesRepository.Object, unitOfWork.Object));
            builder.RegisterInstance(new BlogService(_blogsRepository.Object, unitOfWork.Object));

            IContainer container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            appBuilder.UseWebApi(config);

        }
    }
}
