using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using PublicStatusIndicator.Webserver;

namespace PublicStatusIndicator.Controller
{
    public class ApiController
    {
        /// <summary>
        ///     responds with a <see cref="HttpStatusCode.OK"/> Mesasge
        /// </summary>
        /// <param name="response">object to transfer with the response message</param>
        /// <returns><see cref="HttpResponseMessage"/> with <see cref="HttpStatusCode.OK"/></returns>
        protected async Task<HttpResponseMessage> Ok(object response)
        {
            var responseMessgae = await Task.Factory.StartNew(async () =>
            {
                var message = new HttpResponseMessage();
                try
                {
                    
                    string json;
                    var jsonSerializer = new DataContractJsonSerializer(response.GetType());

                    using (var memoryStream = new MemoryStream())
                    {
                        jsonSerializer.WriteObject(memoryStream, response);
                        json = Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                    message.StatusCode = HttpStatusCode.OK;
                    HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                    message.Content = contentPost;
                }
                catch (Exception ex)
                {
                    return await InternalServerError(ex);
                }
                return message;
            });
            return responseMessgae.Result;
        }

        public async Task <HttpResponseMessage> NotFound()
        {
            var response = await Task.Factory.StartNew(() =>
           {
               var httpResponseMessage = new HttpResponseMessage() { Content = new StringContent("No Route Found") };
               httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
               return httpResponseMessage;
           });
            return response;
        }


        private async Task<HttpResponseMessage> InternalServerError(Exception exception)
        {
            var response = await Task.Factory.StartNew(() =>
            {
                var httpResponseMessage = new HttpResponseMessage() { Content = new StringContent(exception.Message) };
                httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;
                return httpResponseMessage;
            });
            return response;
        }

    }

    public static class ApiControllerExtensions
    {
        public static MethodInfo[] GetInvokeableMethods(this ApiController controller)
        {
            var controllerType = controller.GetType();
            var methodsWithRoutes = controllerType.GetMethods().Where(
                m => m.GetCustomAttributes(typeof(Route)).Any()).ToArray();
            return methodsWithRoutes;
        }

        public static bool NeedsAuth(this ApiController controller)
        {
            return controller.GetType().GetTypeInfo().GetCustomAttribute(typeof(Authentication)) != null;
        }
    }

}