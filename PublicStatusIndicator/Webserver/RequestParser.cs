using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace PublicStatusIndicator.Webserver
{
    internal static class RequestParser
    {
        /// <summary>
        /// Tries to get the (Basic) authorisation credentials out of the given request string
        /// </summary>
        /// <param name="requestString">the request string wich contains the basic auth info </param>
        /// <returns><see cref="NetworkCredential"/></returns>
        public static NetworkCredential GetCredentials(string requestString)
        {
            if (string.IsNullOrWhiteSpace(requestString))
                throw new ArgumentException(nameof(requestString));

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