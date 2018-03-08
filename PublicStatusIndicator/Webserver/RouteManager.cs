using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PublicStatusIndicator.Controller;

namespace PublicStatusIndicator.Webserver
{
    

    public class RouteManager
    {
        private readonly List<ApiController> _controllers;
        private readonly List<Route> _routes;

        public RouteManager()
        {
            _routes = new List<Route>();
            _controllers = new List<ApiController>();
        }

        /// <summary>
        ///     Invokes a predefined Method with a HttpRequestString 
        ///     (Currently only without Parameters)
        /// </summary>
        /// <param name="reqstring">the raw request string</param>
        public async Task<HttpResponseMessage> InvokeMethod(string reqstring)
        {
            //Todo: get object[] aus dem request und aufruf der Methode mit diesen parametern

            var message = await Task.Factory.StartNew(async () =>
            {
                var authIncluded = reqstring.Contains("Authorization:");
                var methodToInvoke = FindRoute(reqstring);

                if (methodToInvoke.AuthenticationRequired && !authIncluded)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Unauthorized") };
                }

                if (methodToInvoke.AuthenticationRequired && authIncluded)
                {
                    var credentials = RequestParser.GetCredentials(reqstring);
                    if (credentials.Password != "bar" && credentials.UserName != "foo")
                    {
                        return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Unauthorized") }; ;
                    }
                }

                HttpResponseMessage returnMessage;
                try
                {
                    var result =(Task<HttpResponseMessage>) methodToInvoke.Method.Invoke(methodToInvoke.Controller, null);
                    await result;
                    returnMessage = result.Result;
                }
                catch (Exception e)
                {
                    returnMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(e.Message)
                    };
                }
                return returnMessage;
            });
            return message.Result;

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
                if (!myMatch.Success) continue;
                var targetRoute = _routes.FirstOrDefault( route => string.Equals(route.Url, myMatch.Value.Trim(), StringComparison.CurrentCultureIgnoreCase));
                return targetRoute;
            }
            return null;
        }

        /// <summary>
        ///     Initialises all routes for the registered controllers
        /// </summary>
        internal void InitRoutes()
        {
            _routes.Clear();

            foreach (var apiController in _controllers)
            {
                var methodsWithRoutes = apiController.GetInvokeableMethods();
                var controllerNeedsAuth = apiController.NeedsAuth();
                
                foreach (var memberInfo in methodsWithRoutes)
                {
                    var route = memberInfo.GetRoute();
                    if (route == null) continue;

                    var routeNeedsAuth = memberInfo.NeedsAuth();

                    route.Method = memberInfo;
                    route.Controller = apiController;

                    if (controllerNeedsAuth || routeNeedsAuth)
                    {
                        route.AuthenticationRequired = true;
                    }
                    route.Params = memberInfo.GetParameters();
                    _routes.Add(route);
                }
            }
        }

        

        /// <summary>
        /// registers a <see cref="ApiController"/> with the <see cref="RouteManager"/>
        /// </summary>
        /// <param name="controller">instance of <see cref="ApiController"/>to register</param>
        public void Register(ApiController controller)
        {
            _controllers.Add(controller);   
        }
    }
}