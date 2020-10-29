using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using UnitTestingWebAPI.API.Controllers;
using UnitTestingWebAPI.Domain;
using UnitTestingWebAPI.Tests.Helpers;

namespace UnitTestingWebAPI.Tests
{
    [TestFixture]
    public class RouteTests
    {
        #region Variables
        HttpConfiguration configuration;
        #endregion

        #region Setup
        [SetUp]
        public void Setup()
        {
            configuration = new HttpConfiguration();
            configuration.Routes.MapHttpRoute(name: "DefaultWebAPI", routeTemplate: "api/{controller}/{id}", defaults: new { id = RouteParameter.Optional });
        }
        #endregion

        #region Helper methods
        public static string GetMethodName<T, U>(Expression<Func<T, U>> expression)
        {
            var method = expression.Body as MethodCallExpression;
            if (method != null)
                return method.Method.Name;

            throw new ArgumentException("Expression is wrong");
        }
        #endregion

        [Test]
        public void RouteShouldControllerGetArticleIsInvoked()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.chsakell.com/api/articles/5");

            var _actionSelector = new ControllerActionSelector(configuration, request);

            Assert.That(typeof(ArticlesController), Is.EqualTo(_actionSelector.GetControllerType()));
            Assert.That(
                GetMethodName((ArticlesController c) => c.GetArticle(5)),
                Is.EqualTo(_actionSelector.GetActionName())
            );
        }

        [Test]
        public void RouteShouldPostArticleActionIsInvoked()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://www.chsakell.com/api/articles/");

            var _actionSelector = new ControllerActionSelector(configuration, request);

            Assert.That(
                GetMethodName((ArticlesController c) => c.PostArticle(new Article())),
                Is.EqualTo(_actionSelector.GetActionName())
            );
        }

        [Test]
        public void RouteShouldInvalidRouteThrowException()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://www.chsakell.com/api/InvalidController/");

            var _actionSelector = new ControllerActionSelector(configuration, request);

            Assert.Throws<HttpResponseException>(() => _actionSelector.GetActionName());
        }
    }
}
