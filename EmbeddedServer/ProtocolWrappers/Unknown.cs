using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.ProtocolWrappers
{
    public class Unknown : Protocol
    {
        public Unknown(string message) : base(message)
        {
            _message = message;
        }

        public override string BuildAnswer()
        {
            throw new NotImplementedException();
        }

        protected override void TryParse()
        {
            throw new NotImplementedException();
        }
    }
}
