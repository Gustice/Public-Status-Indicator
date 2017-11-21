using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

                var controllerAuth = controllerType.GetTypeInfo().GetCustomAttribute(typeof(Authentication));
                
                foreach (var memberInfo in methodsWithRoutes)
                {
                    var route = memberInfo.GetCustomAttributes(typeof(Route)).FirstOrDefault() as Route;
                    if (route == null) continue;

                    var routeAuth = memberInfo.GetCustomAttribute(typeof(Authentication));

                    route.Method = memberInfo;
                    route.Controller = apiController;
                    if (controllerAuth != null || routeAuth != null)
                    {
                        route.AuthenticationRequired = true;
                    }
                    route.Params = memberInfo.GetParameters();
                    Routes.Add(route);
                }
            }
        }

        public string GetAuthAttribute<T>()
        {
            //var dnAttribute = typeof(T).GetCustomAttributes(
            //    typeof(DomainNameAttribute), true
            //).FirstOrDefault() as DomainNameAttribute;
            //if (dnAttribute != null)
            //{
            //    return dnAttribute.Name;
            //}
            return null;
        }

    }

    internal class RequestParser
    {
        /// <summary>
        /// Tries to get the (Basic) Authorisation Credentials out of the Request string
        /// </summary>
        /// <param name="requestString"></param>
        /// <returns></returns>
        public static NetworkCredential GetCredentials(string requestString)
        {
            var authRegex = new Regex(@"\b(Basic).[^\s]+");
            var user = authRegex.Match(requestString);
            var encodedUsernamePassword = user.Value.Substring("Basic ".Length).Trim();
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
            if (string.IsNullOrEmpty(usernamePassword))
                throw new Exception("Auth Error");

            if (!usernamePassword.Contains(":"))
                throw new Exception("Auth Error");

            return new NetworkCredential()
            {
                UserName = usernamePassword.Split(':')[0],
                Password= usernamePassword.Split(':')[1]
            };

        }
    }
}