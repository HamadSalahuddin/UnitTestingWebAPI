using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using UnitTestingWebAPI.Domain;

namespace UnitTestingWebAPI.API.Filters
{
    public class ArticlesReversedFilter: ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if(objectContent != null)
            {
                List<Article> articles = objectContent.Value as List<Article>;
                if(articles.Any())
                {
                    articles.Reverse();
                }
            }
        }
    }
}