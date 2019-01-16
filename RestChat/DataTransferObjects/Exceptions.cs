using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedData
{
    public class ChatException : Exception
    {
        public int HttpErrorCode { get; }

        public ChatException(int errorCode)
        {
            HttpErrorCode = errorCode;
        }

        public ChatException(HttpStatusCode errorCode) : this((int) errorCode)
        {

        }
    }

    public class UsernameTakenException : ChatException
    {
        public UsernameTakenException() : base(401)
        {

        }
    }
}
