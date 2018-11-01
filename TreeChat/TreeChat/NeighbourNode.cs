using System;
using System.Collections.Concurrent;

namespace TreeChat
{
    public class AttemptsInfo
    {
        public int AttemptsAlready { get; set; }

        public DateTime LastAttempt { get; set; }


        public AttemptsInfo(int attemptsAlready, DateTime lastAttempt)
        {
            AttemptsAlready = attemptsAlready;
            LastAttempt = lastAttempt;
        }

    }

    public class NeighbourNode
    {
        public ConcurrentDictionary<Message, AttemptsInfo> LastAttempts;
        public AttemptsInfo LastPinged;

        public NeighbourNode()
        {
            LastAttempts = new ConcurrentDictionary<Message, AttemptsInfo>();
            ResetAttempts();
        }

        public void ResetAttempts()
        {
            LastPinged = new AttemptsInfo(0, DateTime.Now);
        }
    }
}
