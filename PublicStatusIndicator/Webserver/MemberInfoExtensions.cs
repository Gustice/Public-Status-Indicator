using System.Linq;
using System.Reflection;

namespace PublicStatusIndicator.Webserver
{
    /// <summary>
    /// Helper methods to obtain routes and attributes 
    /// </summary>
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Gets the Route Attribute from this Memberinfo
        /// </summary>
        /// <param name="memberinfo"><see cref="MemberInfo"/>to search for <see cref="Route"/>Attribute</param>
        /// <returns>instance of <see cref="Route"/>or null</returns>
        public static Route GetRoute(this MemberInfo memberinfo)
        {
            return memberinfo.GetCustomAttributes(typeof(Route)).FirstOrDefault() as Route;
        }

        /// <summary>
        /// indicates whether the method is only accessible with prior authentication
        /// </summary>
        /// <param name="memberinfo"><see cref="MemberInfo"/>to check for <see cref="Authentication"/>attribute</param>
        /// <returns>true, if <see cref="Authentication"/> required</returns>
        public static bool NeedsAuth(this MemberInfo memberinfo)
        {
            return memberinfo.GetCustomAttribute(typeof(Authentication)) != null;
        }
    }
}