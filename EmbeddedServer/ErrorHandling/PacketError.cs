using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.ErrorHandling
{
    public class PacketError
    {
        public int Code { get; set; } = 102;
        public string Message { get; set; } = "Processing";
        
    }
}
