using System;
using System.Reflection;
using PublicStatusIndicator.Controller;

namespace PublicStatusIndicator.Webserver
{
    public class Route : Attribute
    {
        public Route(string route)
        {
            Url = route;
        }

        /// <summary>
        ///     Url of Route
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     Methodinfo to Invoke with given Route
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        ///     Controller which contains Route
        /// </summary>
        public ApiController Controller { get; internal set; }

        /// <summary>
        ///     Parameters for Route
        /// </summary>
        public ParameterInfo[] Params { get; set; }

        /// <summary>
        ///     Indicates whether the controller or the route itself need auth to invoke
        /// </summary>
        public bool AuthenticationRequired { get; set; }

        public override string ToString()
        {
            return Url;
        }
    }

    /// <summary>
    ///     Indicates that the Controller needs Authentication
    /// </summary>
    internal class Authentication : Attribute
    {
    }
}