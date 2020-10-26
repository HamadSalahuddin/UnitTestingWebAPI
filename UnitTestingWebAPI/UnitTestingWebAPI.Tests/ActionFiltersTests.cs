using Microsoft.Owin.Hosting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using UnitTestingWebAPI.API.Filters;
using UnitTestingWebAPI.Data;
using UnitTestingWebAPI.Domain;
using UnitTestingWebAPI.Tests.Hosting;

namespace UnitTestingWebAPI.Tests
{
    public class ActionFiltersTests
    {
        [Test]
        public void ShouldSortArticlesByTitle()
        {
            var filter = new ArticlesReversedFilter();
            var executedContext = new HttpActionExecutedContext(new HttpActionContext
            {
                Response = new HttpResponseMessage(),
            }, null);

            var articles = BloggerInitializer.GetAllArticles();

            executedContext.Response.Content = new ObjectContent<List<Article>>(
                new List<Article>(articles),
                new JsonMediaTypeFormatter()
            );

            filter.OnActionExecuted(executedContext);

            var returnedArticles = executedContext.Response.Content.ReadAsAsync<List<Article>>().Result;

            Assert.That(returnedArticles.First(), Is.EqualTo(articles.Last()));
        }

        // Integration Test
        [Test]
        public void ShouldCallToControllerActionReverseArticles()
        {
            // Arrange
            var address = "http://localhost:9000/";
            using (WebApp.Start<Startup>(address))
            {
                HttpClient _client = new HttpClient();
                var response = _client.GetAsync(address + "api/articles").Result;

                var returnedArticles = response.Content.ReadAsAsync<List<Article>>().Result;

                Assert.That(returnedArticles.First().Title, Is.EqualTo(BloggerInitializer.GetAllArticles().Last().Title));
            }
        }
    }
}
