using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestServer
{
    public class ChatException : Exception
    {
        public int HttpErrorCode { get; }

        public ChatException(int errorCode)
        {
            HttpErrorCode = errorCode;
        }
    }

    public class UsernameTakenException : ChatException
    {
        public UsernameTakenException() : base(401)
        {

        }
    }
}
