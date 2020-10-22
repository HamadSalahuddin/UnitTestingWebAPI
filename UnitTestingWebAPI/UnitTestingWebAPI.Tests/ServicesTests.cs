using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestingWebAPI.Data;
using UnitTestingWebAPI.Data.Infrastructure;
using UnitTestingWebAPI.Data.Repositories;
using UnitTestingWebAPI.Domain;
using UnitTestingWebAPI.Services;

namespace UnitTestingWebAPI.Tests
{
    [TestFixture]
    public class ServicesTests
    {
        #region Variables
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
            int counter = new int();
            var articles = BloggerInitializer.GetAllArticles();

            foreach(var article in articles)
            {
                article.ID = ++counter;
            }

            return articles;
        }

        public IArticleRepository SetupArticleRepository()
        {
            // Init Repository.
            var repo = new Mock<IArticleRepository>();

            // setup mocking behavior.
            repo.Setup(r => r.GetAll())
                .Returns(randomArticles);

            repo.Setup(r => r.GetById(It.IsAny<int>()))
                .Returns(
                    new Func<int, Article>(id => randomArticles.Find(a => a.ID.Equals(id)))
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
                    oldArticle = x;
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
        #endregion

        #region Service Tests
        [Test]
        public void ServiceShouldReturnAllArticles()
        {
            var articles = articleService.GetArticles();

            Assert.That(articles, Is.EqualTo(randomArticles));
        }

        [Test]
        public void ServiceShouldAddNewArticle()
        {
            var newArticle = new Article
            {
                Author = "Chris Sakellarios",
                Contents = "If you are an ASP.NET MVC developer, you will certainly..",
                Title = "URL Rooting in ASP.NET (Web Forms)",
                URL = "https://chsakell.com/2013/12/15/url-rooting-in-asp-net-web-forms/"
            };

            int _maxArticleIDBeforeAdd = randomArticles.Max(a => a.ID);
            articleService.CreateArticle(newArticle);

            Assert.That(newArticle, Is.EqualTo(randomArticles.Last()));
            Assert.That(_maxArticleIDBeforeAdd + 1, Is.EqualTo(randomArticles.Last().ID));
        }

        [Test]
        public void ServiceShouldUpdateArticle()
        {
            var _firstArticle = randomArticles.First();

            _firstArticle.Title = "OData feat. ASP.NET Web API"; // reversed <img draggable="false" role="img" class="emoji" alt="🙂" src="https://s0.wp.com/wp-content/mu-plugins/wpcom-smileys/twemoji/2/svg/1f642.svg" scale="0">
            _firstArticle.URL = "http://t.co/fuIbNoc7Zh"; // short link
            articleService.UpdateArticle(_firstArticle);

            Assert.That(_firstArticle.DateEdited, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(_firstArticle.URL, Is.EqualTo("http://t.co/fuIbNoc7Zh"));
            Assert.That(_firstArticle.ID, Is.EqualTo(1)); // hasn't changed
        }

        [Test]
        public void ServiceShouldDeleteArticle()
        {
            int maxID = randomArticles.Max(a => a.ID); // Before removal
            var _lastArticle = randomArticles.Last();

            // Remove last article
            articleService.DeleteArticle(_lastArticle);

            Assert.That(maxID, Is.GreaterThan(randomArticles.Max(a => a.ID))); // Max reduced by 1
        }
        #endregion
    }
}
