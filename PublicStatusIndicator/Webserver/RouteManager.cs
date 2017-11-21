using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PublicStatusIndicator.Webserver
{
    internal class RouteManager
    {
        private static RouteManager _instance;
        public List<ApiController.ApiController> Controllers;

        public List<Route> Routes;

        public RouteManager()
        {
            Routes = new List<Route>();
            Controllers = new List<ApiController.ApiController>();
        }

        public static RouteManager CurrentRouteManager
        {
            get
            {
                if (_instance == null)
                    _instance = new RouteManager();
                return _instance;
            }
        }

        /// <summary>
        ///     Invokes a predefined Method with a HttpRequestString 
        ///     (Currently only without Parameters)
        /// </summary>
        /// <param name="reqstring"></param>
        public HttpResponseMessage InvokeMethod(string reqstring)
        {
            //Todo: get object[] aus dem request und aufruf der Methode mit diesen parametern
            var methodToInvoke = FindRoute(reqstring);
            HttpResponseMessage returnMessage;
            try
            {
                returnMessage = methodToInvoke.Method.Invoke(methodToInvoke.Controller, null) as HttpResponseMessage;
            }
            catch (Exception e)
            {
                returnMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content =  new StringContent(e.Message)
                };
            }

            return returnMessage;
        }

        /// <summary>
        ///     looks up the Route to the request
        /// </summary>
        /// <param name="reqstring">HTTP Request</param>
        /// <returns>a <see cref="Route"/></returns>
        private Route FindRoute(string reqstring)
        {
            const string strRegex = @"/\w+/*/.+ ";
            var myRegex = new Regex(strRegex, RegexOptions.IgnoreCase);

            foreach (Match myMatch in myRegex.Matches(reqstring))
            {
                if (myMatch.Success)
                {
                    var targetRoute = Routes.FirstOrDefault( route => string.Equals(route.Url, myMatch.Value.Trim(), StringComparison.CurrentCultureIgnoreCase));
                    return targetRoute;
                }
                
            }
            return null;
        }

        /// <summary>
        ///     Initialises all Routes for the Controllers
        /// </summary>
        internal void InitRoutes()
        {
            foreach (var apiController in Controllers)
            {
                var controllerType = apiController.GetType();
                var methodsWithRoutes = controllerType.GetMethods().Where(
                    m => m.GetCustomAttributes(typeof(Route)).Any()).ToArray();
                foreach (var memberInfo in methodsWithRoutes)
                {
                    var route = memberInfo.GetCustomAttributes(typeof(Route)).FirstOrDefault() as Route;

                    if (route == null) continue;

                    route.Method = memberInfo;
                    route.Controller = apiController;
                    route.Params = memberInfo.GetParameters();
                    Routes.Add(route);
                }
            }
        }
    }
}