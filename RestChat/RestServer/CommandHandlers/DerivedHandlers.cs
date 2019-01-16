using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using SharedData;

namespace RestServer.CommandHandlers
{
    public class LoginHandler : GenericHandler<LoginRequestData, LoginResponseData>
    {

        public LoginHandler(UsersController usersController) : base(usersController)
        {
        }

        public override Regex RelativeUriPattern => new Regex("^/login$");
        public override HttpMethod MethodType => HttpMethod.Post;

        public override LoginResponseData Handle(LoginRequestData requestData)
        {
            var user = usersController.SignUp(requestData.Username);

            return (new LoginResponseData
            {
                Id = user.Id,
                Username = user.Username,
                Online = true,
                Token = user.Token
            });
        }
    }

    public class LogoutHandler : GenericHandler<LogoutRequestData, LogoutResponseData>
    {

        public LogoutHandler(UsersController usersController) : base(usersController)
        {
        }

        public override Regex RelativeUriPattern => new Regex("^/logout$");
        public override HttpMethod MethodType => HttpMethod.Get;

        public override LogoutResponseData Handle(LogoutRequestData empty)
        {
            if (CommandInvoker == null)
            {
                throw new ChatException(401);
            }

            usersController.Logout(CommandInvoker);

            return (new LogoutResponseData
            {
                Message = "bye!"
            });
        }
    }

    public class MessagesListHandler : GenericHandler<MessagesListRequestData, MessagesListResponseData>
    {
        private readonly MessagesController messagesController;

        public MessagesListHandler(UsersController usersController, MessagesController messagesController)
            : base(usersController)
        {
            this.messagesController = messagesController;
        }

        public override Regex RelativeUriPattern => new Regex("^/messages$");
        public override HttpMethod MethodType => HttpMethod.Get;

        public override MessagesListResponseData Handle(MessagesListRequestData _)
        {
            if (CommandInvoker == null)
            {
                throw new ChatException(401);
            }

            var offsetUnparsed = Request.QueryString["offset"];
            if (offsetUnparsed == null || !int.TryParse(offsetUnparsed, out int offset))
            {
                offset = 0;
            }
            else if (offset < 0)
            {
                throw new ChatException(400);
            }

            var countUnparsed = Request.QueryString["count"];
            if (countUnparsed == null || !int.TryParse(countUnparsed, out int count))
            {
                count = 10;
            }
            else if (count <= 0 || count > 100)
            {
                throw new ChatException(400);
            }

            var messages = messagesController.GetMessages(offset, count);

            return new MessagesListResponseData
            {
                Messages = messages
                    .Select(m => new SendMessageResponseData
                    {
                        Id = m.Id,
                        Message = m.Text,
                        Author = m.Author
                    })
                    .ToList()
            };
        }
    }

    public class SendMessageHandler : GenericHandler<SendMessageRequestData, SendMessageResponseData>
    {
        private readonly MessagesController messagesController;

        public SendMessageHandler(UsersController usersController, MessagesController messagesController)
            : base(usersController)
        {
            this.messagesController = messagesController;
        }

        public override Regex RelativeUriPattern => new Regex("^/messages$");
        public override HttpMethod MethodType => HttpMethod.Post;

        public override SendMessageResponseData Handle(SendMessageRequestData requestData)
        {
            if (CommandInvoker == null)
            {
                throw new ChatException(401);
            }

            var message = messagesController.PostMessage(requestData.Message, CommandInvoker);

            return (new SendMessageResponseData
            {
                Id = message.Id,
                Message = message.Text,
                Author = message.Author
            });
        }
    }

    public class UserInfoHandler : GenericHandler<UserInfoRequestData, UserInfoResponseData>
    {

        public UserInfoHandler(UsersController usersController) : base(usersController)
        {
        }

        public override Regex RelativeUriPattern => new Regex("^/users/(\\d+)$");
        public override HttpMethod MethodType => HttpMethod.Get;

        public override UserInfoResponseData Handle(UserInfoRequestData _)
        {
            if (CommandInvoker == null)
            {
                throw new ChatException(401);
            }

            var endpointRegexMatch = RelativeUriPattern.Match(Request.Url.AbsolutePath);
            var userId = int.Parse(endpointRegexMatch.Groups[1].Value);
            var user = usersController.GetUserByID(userId);

            return (new UserInfoResponseData
            {
                Id = user.Id,
                Username = user.Username,
                Online = user.Online
            });
        }
    }

    public class UsersListHandler : GenericHandler<UserListRequestData, UserListResponseData>
    {

        public UsersListHandler(UsersController usersController) : base(usersController)
        {
        }

        public override Regex RelativeUriPattern => new Regex("^/users$");
        public override HttpMethod MethodType => HttpMethod.Get;

        public override UserListResponseData Handle(UserListRequestData _)
        {
            if (CommandInvoker == null)
            {
                throw new ChatException(401);
            }

            return (new UserListResponseData
            {
                Users = usersController
                    .GetActiveUsers()
                    .Select(user => new UserInfoResponseData()
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Online = user.Online
                    })
                    .ToList()
            });
        }
    }
}