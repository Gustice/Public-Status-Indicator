using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PublicStatusIndicator.ApiController
{
    internal class ApiController
    {
        /// <summary>
        ///     ApiMeldung OK mit entsprechend Formatiertem Json Object
        /// </summary>
        /// <param name="response">Das Objekt das ausgegeben werden soll</param>
        /// <returns>HttpResponse OK mit Json Daten</returns>
        public HttpResponseMessage Ok(object response)
        {
            var responseMessgae = new HttpResponseMessage();
            try
            {
                string json;
                var jsonSerializer = new DataContractJsonSerializer(response.GetType());

                using (var memoryStream = new MemoryStream())
                {
                    jsonSerializer.WriteObject(memoryStream, response);
                    json = Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                responseMessgae.StatusCode = HttpStatusCode.OK;
                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                responseMessgae.Content = contentPost;
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return responseMessgae;
        }

        public HttpResponseMessage NotFound()
        {
            var responseMessgae = new HttpResponseMessage();
            responseMessgae.StatusCode = HttpStatusCode.NotFound;
            return responseMessgae;
        }


        public HttpResponseMessage InternalServerError(Exception exception)
        {
            var responseMessgae = new HttpResponseMessage() {Content = new StringContent(exception.Message)};
            responseMessgae.StatusCode = HttpStatusCode.InternalServerError;
            return responseMessgae;
        }

    }
}