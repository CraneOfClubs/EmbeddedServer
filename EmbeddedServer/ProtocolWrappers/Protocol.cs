using EmbeddedServer.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer
{
    public enum MsgType {
        ACK,
        AUTH,
        DUMP,
        ACTION,
        POLL
    }
    abstract public class Protocol
    {
        protected MsgType _messageType;
        protected PacketError _resultInfo; 
        protected Dictionary<String, String> _headers = new Dictionary<string, string>();
        protected bool _isValid = false;
        protected string _message;
        protected string _answer;
        public Protocol(string message)
        {
            _resultInfo = new PacketError();
            _message = message;
            TryParse();
        }

        public void LoadMessage(string message)
        {
            _message = message;
            TryParse();
        }

        public MsgType MessageType
        {
            private set { }
            get
            {
                return _messageType;
            }
        }
        public bool IsMessageValid
        {
            private set { }
            get
            {
                return _isValid;
            }
        }

        public string Answer
        {
            private set { }
            get
            {
                return _answer;
            }
        }

        public PacketError ResultInfo
        {
            private set { }
            get
            {
                return _resultInfo;
            }
        }

        public Dictionary<String, String> Headers
        {
            private set { }
            get
            {
                return _headers;
            }
        }

        protected abstract void TryParse();
        public abstract string BuildAnswer();
    }
}
