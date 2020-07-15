using System;

namespace Mautom.Portunus.Shared.Pgp
{
    public class PgpException : Exception
    {
        public PgpException(string message) : base(message) {}
        public PgpException(string message, Exception inner) : base(message,inner) {}
        
    }
}