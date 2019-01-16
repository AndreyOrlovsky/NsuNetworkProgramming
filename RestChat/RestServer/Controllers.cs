using System;
using System.Collections.Generic;
using System.Linq;
using SharedData;

namespace RestServer
{
    public class UsersController
    {
        private readonly TimeSpan timeout;

        private readonly List<User> users = new List<User>();

        public UsersController()
        {
            timeout = TimeSpan.FromSeconds(10);
        }

        public User SignUp(string username)
        {
            UpdateOnlineStatuses();

            var user = users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                user = new User
                {
                    Id = users.Count + 1,
                    Username = username,
                    Token = Guid.NewGuid(),
                    LastSeen = DateTime.Now,
                    Online = true
                };
                users.Add(user);
                return user;
            }

            if (user.Online != true)
            {
                user.LastSeen = DateTime.Now;
                user.Online = true;
                return user;
            }

            throw new UsernameTakenException();
        }

        public User SignIn(Guid token)
        {
            UpdateOnlineStatuses();

            var user = users.FirstOrDefault(u => u.Token == token);

            if (user == null)
            {
                throw new ChatException(403);
            }

            user.LastSeen = DateTime.Now;

            return user;
        }

        public void Logout(User user)
        {
            user.Online = false;
        }

        public User GetUserByID(int id)
        {
            UpdateOnlineStatuses();

            var user = users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                throw new ChatException(404);
            }

            return user;
        }

        public IEnumerable<User> GetActiveUsers()
        {
            UpdateOnlineStatuses();

            return users;
        }

        private void UpdateOnlineStatuses()
        {
            foreach (var user in users)
            {
                if (user.Online == true && TimeoutExpired(user))
                {
                    user.Online = null;
                }
            }
        }

        private bool TimeoutExpired(User user)
            => DateTime.Now - user.LastSeen > timeout;
    }

    public class MessagesController
    {
        private readonly List<Message> messages = new List<Message>();

        public Message PostMessage(string text, User author)
        {
            var message = new Message
            {
                Id = messages.Count + 1,
                Text = text,
                Author = author.Id
            };

            Console.WriteLine($"{author.Username} posted message: \"{text}\"");

            messages.Add(message);
            return message;
        }

        public IEnumerable<Message> GetMessages(int offset, int count)
        {
            return messages.Skip(offset).Take(count);
        }
    }
}