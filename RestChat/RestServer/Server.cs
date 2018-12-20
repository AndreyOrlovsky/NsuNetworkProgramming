using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestServer.CommandHandlers;

namespace RestServer
{
    internal class Server
    {
        public List<AbstractHandler> CommandHandlers = new List<AbstractHandler>();

        private readonly HttpListener usersListener;

        public Server()
        {
            usersListener = new HttpListener();
            usersListener.Prefixes.Add("http://localhost:17005/");
        }

        public void Start()
        {
            Console.WriteLine("Server is listening. Press q to terminate the server...");

            usersListener.Start();

            Task.Run(async () =>
            {
                while (true)
                {
                    await HandleUserConnection();
                }
            });

            do
            {
            } while (Console.ReadKey().KeyChar != 'q');
        }

        private async Task HandleUserConnection()
        {
            var connection = await usersListener.GetContextAsync().ConfigureAwait(false);
            var url = connection.Request.Url.AbsolutePath;

            var handler = GetCommandHandler(url, connection.Request.HttpMethod);
            if (handler == null)
            {
                connection.Response.StatusCode = 404;
                connection.Response.Headers.Add(HttpResponseHeader.ContentType, "text");

                byte[] buffer = Encoding.UTF8.GetBytes("No appropriate API to handle your request");
                connection.Response.ContentLength64 = buffer.LongLength;
                connection.Response.OutputStream.Write(buffer, 0, buffer.Length);
                return;
            }

            handler.Request = connection.Request;
            handler.Response = connection.Response;
            handler.TryHandle();
        }

        public void Stop()
        {
            usersListener.Stop();
        }

        public void AddCommandHandlers(params AbstractHandler[] handlers)
        {
            CommandHandlers.AddRange(handlers);
        }

        private AbstractHandler GetCommandHandler(string url, string httpMethod)
        {
            return CommandHandlers.FirstOrDefault(handler =>
                handler.RelativeUriPattern.IsMatch(url) && handler.MethodType.Method == httpMethod);
        }
    }
}