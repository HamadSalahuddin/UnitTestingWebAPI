using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnitTestingWebAPI.API.MediaTypeFormatters;
using UnitTestingWebAPI.Data;
using UnitTestingWebAPI.Domain;

namespace UnitTestingWebAPI.Tests
{
    [TestFixture]
    public class MediaTypeFormatterTests
    {
        #region Variables
        Blog blog;
        Article article;
        ArticleFormatter formatter;
        #endregion

        [SetUp]
        public void Setup()
        {
            blog = BloggerInitializer.GetBlogs().First();
            article = BloggerInitializer.GetChsakellsArticles().First();
            formatter = new ArticleFormatter();
        }

        [Test]
        public void FormatterShouldThrowExceptionWhenUnsupportedType()
        {
            Assert.Throws<InvalidOperationException>(() => new ObjectContent<Blog>(blog, formatter));
        }

        [Test]
        public void FormatterShouldNotThrowExceptionWhenArticle()
        {
            Assert.DoesNotThrow(() => new ObjectContent<Article>(article, formatter));
        }

        [Test]
        public void FormatterShouldHeaderBeSetCorrectly()
        {
            var content = new ObjectContent<Article>(article, new ArticleFormatter());

            Assert.That(content.Headers.ContentType.MediaType, Is.EqualTo("application/article"));
        }

        [Test]
        public async Task FormatterShouldBeAbleToDeserializeArticle()
        {
            var content = new ObjectContent<Article>(article, formatter);

            var deserializedItem = await content.ReadAsAsync<Article>(new[] { formatter });

            Assert.That(article, Is.SameAs(deserializedItem));
        }

        [Test]
        public void FormatterShouldNotBeAbleToWriteUnsupportedType()
        {
            var canWriteBlog = formatter.CanWriteType(typeof(Blog));
            Assert.That(canWriteBlog, Is.False);
        }

        [Test]
        public void FormatterShouldBeAbleToWriteArticle()
        {
            var canWriteArticle = formatter.CanWriteType(typeof(Article));
            Assert.That(canWriteArticle, Is.True);
        }
    }
}
