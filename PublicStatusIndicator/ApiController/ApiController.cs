using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace PublicStatusIndicator.ApiController
{
    internal class ApiController
    {
        /// <summary>
        ///     ApiMeldung OK mit entsprechend Formatiertem Json Object
        /// </summary>
        /// <param name="response">Das Objekt das ausgegeben werden soll</param>
        /// <returns>HttpResponse OK mit Json Daten</returns>
        public async Task<HttpResponseMessage> Ok(object response)
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


        public async Task<HttpResponseMessage> InternalServerError(Exception exception)
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
}