using System;

namespace TreeChat
{
    

    [Serializable] public class Message
    {
        public Guid Id { get; }

        public string UserName { get;  }

        public string Text { get;  }

        public DateTime WhenSent { get; }

        public Message(byte[] bytes) 
            => Id = new Guid(bytes);

        public Message(string userName, string text, DateTime whenSent)
        {
            Id = Guid.NewGuid();
            UserName = userName;
            Text = text;
            WhenSent = whenSent;
        }


        public bool Equals(Message other) 
            => Id.Equals(other.Id);

        public override bool Equals(object obj) 
            => (obj is Message message) && Equals(message);

        public override int GetHashCode() 
            => Id.GetHashCode();

        public override string ToString() 
            => $"[{UserName}]: {Text}";
    }
}
