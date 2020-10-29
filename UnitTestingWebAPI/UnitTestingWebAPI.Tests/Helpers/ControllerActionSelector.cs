using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace UnitTestingWebAPI.Tests.Helpers
{
    public class ControllerActionSelector
    {
        #region Variables
        HttpConfiguration configuration;
        HttpRequestMessage requestMessage;
        IHttpRouteData routeData;
        IHttpControllerSelector controllerSelector;
        HttpControllerContext controllerContext;
        #endregion

        #region Constructor
        public ControllerActionSelector(HttpConfiguration conf, HttpRequestMessage req)
        {
            configuration = conf;
            requestMessage = req;
            routeData = configuration.Routes.GetRouteData(requestMessage);
            requestMessage.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
            controllerSelector = new DefaultHttpControllerSelector(configuration);
            controllerContext = new HttpControllerContext(configuration, routeData, requestMessage);
        }
        #endregion

        public string GetActionName()
        {
            if(controllerContext.ControllerDescriptor == null)
            {
                GetControllerType();
            }

            var actionSelector = new ApiControllerActionSelector();
            var descriptor = actionSelector.SelectAction(controllerContext);

            return descriptor.ActionName;
        }

        public Type GetControllerType()
        {
            var descriptor = controllerSelector.SelectController(requestMessage);
            controllerContext.ControllerDescriptor = descriptor;
            return descriptor.ControllerType;
        }
    }
}
