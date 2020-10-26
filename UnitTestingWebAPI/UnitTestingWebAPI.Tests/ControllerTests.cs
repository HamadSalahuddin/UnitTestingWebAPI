using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using UnitTestingWebAPI.API.Controllers;
using UnitTestingWebAPI.Data;
using UnitTestingWebAPI.Data.Infrastructure;
using UnitTestingWebAPI.Data.Repositories;
using UnitTestingWebAPI.Domain;
using UnitTestingWebAPI.Services;

namespace UnitTestingWebAPI.Tests
{
    [TestFixture]
    class ControllerTests
    {
        #region variables
        IArticleService articleService;
        IArticleRepository articleRepository;
        IUnitOfWork unitOfWork;
        List<Article> randomArticles;
        #endregion

        #region Setup
        [SetUp]
        public void Setup()
        {
            randomArticles = SetupArticles();
            articleRepository = SetupArticleRepository();
            unitOfWork = new Mock<IUnitOfWork>().Object;
            articleService = new ArticleService(articleRepository, unitOfWork);

        }

        public List<Article> SetupArticles()
        {
            int _counter = new int();
            List<Article> _articles = BloggerInitializer.GetAllArticles();

            foreach (Article _article in _articles)
                _article.ID = ++_counter;

            return _articles;
        }

        public IArticleRepository SetupArticleRepository()
        {
            var repo = new Mock<IArticleRepository>();

            // setup mocking behavior.
            repo.Setup(r => r.GetAll()).Returns(randomArticles);

            repo.Setup(r => r.GetById(It.IsAny<int>()))
                .Returns(
                    new Func<int, Article>(
                        id => randomArticles.Find(a => a.ID == id)
                    )
                );

            repo.Setup(r => r.Add(It.IsAny<Article>()))
                .Callback(
                    new Action<Article>(newArticle =>
                    {
                        dynamic maxArticleID = randomArticles.Last().ID;
                        dynamic nextArticleID = maxArticleID + 1;
                        newArticle.ID = nextArticleID;
                        newArticle.DateCreated = DateTime.Now;
                        randomArticles.Add(newArticle);
                    })
                );

            repo.Setup(r => r.Update(It.IsAny<Article>()))
                .Callback(new Action<Article>(x =>
                {
                    var oldArticle = randomArticles.Find(a => a.ID == x.ID);
                    oldArticle.DateEdited = DateTime.Now;
                    oldArticle.URL = x.URL;
                    oldArticle.Title = x.Title;
                    oldArticle.Contents = x.Contents;
                    oldArticle.BlogID = x.BlogID;
                }));

            repo.Setup(r => r.Delete(It.IsAny<Article>()))
                .Callback(new Action<Article>(x =>
                {
                    var _articleToRemove = randomArticles.Find(a => a.ID == x.ID);

                    if (_articleToRemove != null)
                        randomArticles.Remove(_articleToRemove);
                }));

            // Return mock implementation
            return repo.Object;
        }

        [Test]
        public void ControlerShouldReturnAllArticles()
        {
            var _articlesController = new ArticlesController(articleService);

            var result = _articlesController.GetArticles();

            CollectionAssert.AreEqual(result, randomArticles);
        }

        [Test]
        public void ControlerShouldReturnLastArticle()
        {
            var _articlesController = new ArticlesController(articleService);

            var result = _articlesController.GetArticle(3) as OkNegotiatedContentResult<Article>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.Title, randomArticles.Last().Title);
        }

        [Test]
        public void ControlerShouldPutReturnBadRequestResult()
        {
            var articlesController = new ArticlesController(articleService)
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri("http://localhost/api/articles/-1")
                }
            };

            var badResult = articlesController.PutArticle(-1, new Article { Title = "Unknown Article" });
            Assert.That(badResult, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void ControlerShouldPutUpdateFirstArticle()
        {
            var _articlesController = new ArticlesController(articleService)
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri("http://localhost/api/articles/1")
                }
            };

            IHttpActionResult updateResult = _articlesController.PutArticle(1, new Article()
            {
                ID = 1,
                Title = "ASP.NET Web API feat. OData",
                URL = "http://t.co/fuIbNoc7Zh",
                Contents = @"OData is an open standard protocol.."
            }) as IHttpActionResult;

            Assert.That(updateResult, Is.TypeOf<StatusCodeResult>());

            StatusCodeResult statusCodeResult = updateResult as StatusCodeResult;

            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            Assert.That(randomArticles.First().URL, Is.EqualTo("http://t.co/fuIbNoc7Zh"));
        }

        [Test]
        public void ControlerShouldPostNewArticle()
        {
            var article = new Article
            {
                Title = "Web API Unit Testing",
                URL = "https://chsakell.com/web-api-unit-testing",
                Author = "Chris Sakellarios",
                DateCreated = DateTime.Now,
                Contents = "Unit testing Web API.."
            };

            var _articlesController = new ArticlesController(articleService)
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://localhost/api/articles")
                }
            };

            _articlesController.Configuration.MapHttpAttributeRoutes();
            _articlesController.Configuration.EnsureInitialized();
            _articlesController.RequestContext.RouteData = new HttpRouteData(
            new HttpRoute(), new HttpRouteValueDictionary { { "_articlesController", "Articles" } });
            var result = _articlesController.PostArticle(article) as CreatedAtRouteNegotiatedContentResult<Article>;

            Assert.That(result.RouteName, Is.EqualTo("DefaultApi"));
            Assert.That(result.Content.ID, Is.EqualTo(result.RouteValues["id"]));
            Assert.That(result.Content.ID, Is.EqualTo(randomArticles.Max(a => a.ID)));
        }

        [Test]
        public void ControlerShouldNotPostNewArticle()
        {
            var article = new Article
            {
                Title = "Web API Unit Testing",
                URL = "https://chsakell.com/web-api-unit-testing",
                Author = "Chris Sakellarios",
                DateCreated = DateTime.Now,
                Contents = null
            };

            var _articlesController = new ArticlesController(articleService)
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://localhost/api/articles")
                }
            };

            _articlesController.Configuration.MapHttpAttributeRoutes();
            _articlesController.Configuration.EnsureInitialized();
            _articlesController.RequestContext.RouteData = new HttpRouteData(
            new HttpRoute(), new HttpRouteValueDictionary { { "Controller", "Articles" } });
            _articlesController.ModelState.AddModelError("Contents", "Contents is required field");

            var result = _articlesController.PostArticle(article) as InvalidModelStateResult;

            Assert.That(result.ModelState.Count, Is.EqualTo(1));
            Assert.That(result.ModelState.IsValid, Is.EqualTo(false));
        }

        #endregion
    }
}
