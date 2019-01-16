using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestServer.CommandHandlers;

namespace RestServer
{
    static class Program
    {
        static void Main()
        {
            var usersController = new UsersController();
            var messagesController = new MessagesController();

            var server = new Server();

            server.AddCommandHandlers(new LoginHandler(usersController), 
                new LogoutHandler(usersController),
                new UsersListHandler(usersController), 
                new UserInfoHandler(usersController),
                new MessagesListHandler(usersController, messagesController),
                new SendMessageHandler(usersController, messagesController));

            server.Start();
        }
    }
}
