using System;

namespace RestServer
{
    public class User
    {
        public int Id { get; set; }
        public Guid Token { get; set; }
        public string Username { get; set; }

        public DateTime LastSeen { get; set; }
        public bool? Online { get; set; }
    }

    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Author { get; set; }
    }
}