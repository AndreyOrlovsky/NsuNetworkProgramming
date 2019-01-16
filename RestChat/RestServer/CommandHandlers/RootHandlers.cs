using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedData;

namespace RestServer.CommandHandlers
{
    public abstract class AbstractHandler
    {
        public abstract Regex RelativeUriPattern { get; }
        public abstract HttpMethod MethodType { get; }

        public HttpListenerRequest Request { get; set; }
        public HttpListenerResponse Response { get; set; }
        public User CommandInvoker { get; protected set; }


        protected readonly UsersController usersController;

        public AbstractHandler(UsersController usersController)
        {
            this.usersController = usersController;
        }

        public abstract Task HandleAsync();

        public void TryHandle()
        {
            Task.Run(async () =>
            {
                try
                {
                    await this.HandleAsync().ConfigureAwait(false);
                }
                catch (ChatException e)
                {
                    Response.StatusCode = e.HttpErrorCode;
                    if (e is UsernameTakenException)
                        Response.Headers.Add(HttpResponseHeader.WwwAuthenticate, "Token realm='Username is already in use'");

                }
                catch (Exception e)
                {
                    Response.StatusCode = 500;
                    Response.Headers.Add(HttpResponseHeader.ContentType, "text");

                    byte[] buffer = Encoding.UTF8.GetBytes(e.Message);
                    Response.ContentLength64 = buffer.Length;
                    Response.OutputStream.Write(buffer, 0, buffer.Length);

                    Console.WriteLine("Exception occured. Information was sent to client.");
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    Response.OutputStream.Close();
                }
            });

        }
    }

    public abstract class GenericHandler<TRequestData, TResponseData> : AbstractHandler
    {
        public GenericHandler(UsersController usersController) : base(usersController)
        {
        }


        public abstract TResponseData Handle(TRequestData requestData);

        public override async Task HandleAsync()
        {
            if (Request.Headers["Authorization"] != null)
            {
                var token = Guid.Parse(Request.Headers["Authorization"]);
                CommandInvoker = usersController.SignIn(token);
            }

            string requestJson;
            using (var textReader = new StreamReader(Request.InputStream, Request.ContentEncoding))
            {
                requestJson = await textReader.ReadToEndAsync().ConfigureAwait(false);
            }

            TRequestData requestData = JsonConvert.DeserializeObject<TRequestData>(requestJson);

            TResponseData responseData = Handle(requestData);

            var responseJson = JsonConvert.SerializeObject(responseData);

            Response.StatusCode = 200;
            Response.Headers.Add(HttpResponseHeader.ContentType, "application/json");

            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
            Response.ContentLength64 = buffer.Length;
            Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}