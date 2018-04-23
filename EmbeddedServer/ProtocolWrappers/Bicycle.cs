using EmbeddedServer.ErrorHandling;
using EmbeddedServer.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmbeddedServer.ProtocolWrappers
{
    public class TypeTexts
    {
        public string TypeText { get; set; }
        public string ValueText { get; set; }
    }
    public class Bicycle :  Protocol
    {
        private const string _dev_name = "smserver";
        private Dictionary<string, MsgType> _msgTypes = new Dictionary<string, MsgType>();
        private string _opening = "Bicycle=v0.1;";
        private string _ending = "End_Bicycle";
        private Dictionary<Type, TypeTexts> _typeTexts = new Dictionary<Type, TypeTexts>();

        private void Init()
        {
            _msgTypes.Add("ack", MsgType.ACK);
            _msgTypes.Add("action", MsgType.ACTION);
            _msgTypes.Add("auth", MsgType.AUTH);
            _msgTypes.Add("dump", MsgType.DUMP);
            _msgTypes.Add("poll", MsgType.POLL);

            TypeTexts trigger = new TypeTexts() { TypeText = "bool_trigger", ValueText = "module_state" };
            TypeTexts variadicTrigger = new TypeTexts() { TypeText = "variadic_trigger", ValueText = "module_value" };
            TypeTexts sensor = new TypeTexts() { TypeText = "sensor", ValueText = "module_value" };

            _typeTexts.Add(typeof(Trigger), trigger);
            _typeTexts.Add(typeof(VariadicTrigger), variadicTrigger);
            _typeTexts.Add(typeof(Sensor), sensor);
        }
        public Bicycle(string message) : base (message) 
        {
            
            _message = message;
        }

        protected override void TryParse()
        {
            Init();
            _isValid = true;
            _message = _message.Replace(" ", string.Empty);
            if (!ParseTechnicalBlocks())
            {
                _resultInfo.Code = (int)RetCodes.BADREQUEST;
                _resultInfo.Message = "Technical blocks parsing failed.";
                _isValid = false;
            }
            if (!ParseHeaders())
            {
                _resultInfo.Code = (int)RetCodes.BADREQUEST;
                _resultInfo.Message = "Headers parsing failed.";
                _isValid = false;
            }
            if (!RecognizeHeaders())
            {
                _isValid = false;
            }

        }

        private bool ParseTechnicalBlocks()
        {
            try
            {
                var fBlock = _message.IndexOf(";");
                _message = _message.Substring(fBlock + 1, _message.Length - fBlock - 1);
                var eBlock = _message.IndexOf(_ending);
                _message = _message.Substring(0, eBlock);
                if (_message.EndsWith(';'))
                {
                    _message = _message.Substring(0, _message.Length - 1);
                }

            } catch (Exception e)
            {
                return false;
            }

            return true;
        }

        private bool ParseHeaders()
        {
            try
            {
                var splitted = _message.Split(";");
                foreach (var part in splitted)
                {
                    var partHeader = part.Split("=");
                    Headers.Add(partHeader[0], partHeader[1]);
                }

            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        private bool RecognizeHeaders()
        {
            try
            {
                _messageType = _msgTypes[_headers["type"]];
            }
            catch (Exception e)
            {
                _resultInfo.Code = (int)RetCodes.BADREQUEST;
                _resultInfo.Message = "Message type is not recognised.";
                return false;
            }
            if (!(_headers.ContainsKey("local_time") && _headers.ContainsKey("device_name") && _headers.ContainsKey("destination")))
            {
                _resultInfo.Code = (int)RetCodes.BADREQUEST;
                _resultInfo.Message = "Message does not contain requred headers.";
            }

            return true;
        }

        private List<Module> ParseModulesFromHeaders()
        {
            var ret = new List<Module>();
            foreach (var _h in _headers.Keys)
            {
                if (_h.Contains("module_name"))
                {
                    Module curModule = new Module("empty", "Empty");
                    var index = _h.Substring("module_name".Length);
                    string strType = _headers["module_type" + index];
                    if (strType.Contains("bool_trigger"))
                    {
                        curModule = new Trigger(_headers[_h], "Empty");
                        (curModule as Trigger).State = Utility.WrapStates(_headers["module_state" + index]);
                    }
                    if (strType.Contains("variadic_trigger"))
                    {
                        curModule = new VariadicTrigger(_headers[_h], "Empty");
                        (curModule as VariadicTrigger).State = Int32.Parse(_headers["module_value" + index]);
                    }
                    if (strType.Contains("sensor"))
                    {
                        curModule = new Sensor(_headers[_h], "Empty");
                        (curModule as Sensor).State = Int32.Parse(_headers["module_value" + index]);
                    }
                    ret.Add(curModule);
                }
            }
            return ret;
        } 

        private string HandleAck()
        {
            Device buf = new Device(_headers["device_name"], "Empty");
            buf.LastSync = DateTime.Now;
            if (DeviceStorage.Contains(_headers["device_name"]))
            {
                buf = DeviceStorage.GetDeviceByName(_headers["device_name"]);
            } else
            {
                if (DatabaseWrapper.IsDeviceExist(_headers["device_name"]))
                {
                    buf = DatabaseWrapper.LoadDeviceFromDB(_headers["device_name"]).Device;
                }
            }

            buf.CurrentTime = Int64.Parse(_headers["local_time"]);
            var modules = ParseModulesFromHeaders();
            foreach(var m in modules)
            {
                if (buf.IsModuleExist(m.Name))
                {
                    buf.ChangeModuleFromDevice(buf.GetModuleByName(m.Name), m);
                } else
                    buf.AddModule(m);
            }
            if (!DeviceStorage.DeviceExists(_headers["device_name"]))
            {
                DeviceStorage.AddDevice(buf);
            } 
            
            _resultInfo.Code = (int)RetCodes.OK;
            _resultInfo.Message = "Ok";
            return buf.GetQueueMessage();
        }

        private string HandlePoll()
        {

            throw new NotImplementedException();
        }

        private string HandleDump()
        {
            Device buf = new Device(_headers["device_name"], "Empty");
            buf.CurrentTime = Int64.Parse(_headers["local_time"]);
            if (!DeviceStorage.Contains(_headers["device_name"]))
            {
                _resultInfo.Code = 404;
                _resultInfo.Message = "Device not found";
            } else
            {
                buf = DeviceStorage.GetDeviceByName(_headers["device_name"]);
                if (!_headers.ContainsKey("auth"))
                {
                    _resultInfo.Code = (int)RetCodes.UNAUTHORIZED;
                    _resultInfo.Message = "No auth header";
                } else
                {
                    if (_headers["auth"] == buf.AuthToken)
                    {
                        var modules = ParseModulesFromHeaders();
                        foreach (var m in modules)
                        {
                            if (buf.IsModuleExist(m.Name))
                            {
                                buf.ChangeModuleFromDevice(buf.GetModuleByName(m.Name), m);
                            }
                            else
                                buf.AddModule(m);
                        }
                        _resultInfo.Code = (int)RetCodes.OK;
                        _resultInfo.Message = "Ok";
                    }
                    else
                    {
                        _resultInfo.Code = (int)RetCodes.UNAUTHORIZED;
                        _resultInfo.Message = "Auth token is wrong.";

                    }
                }

            }
            return buf.GetQueueMessage() + BuildValidationString(buf);
        }

        public override string BuildAnswer()
        {
            string buf = _opening;
            if (_resultInfo.Code == (int)RetCodes.PROCESSING)
            {
                if (_messageType == MsgType.ACK)
                {
                    buf += HandleAck();
                    buf += "code=" + _resultInfo.Code + ";";
                    
                }
                if (_messageType == MsgType.DUMP)
                {
                    buf += HandleDump();
                    buf += "code=" + _resultInfo.Code + ";";
                }
            } else
            {
                buf+= buf += "code=" + _resultInfo.Code + ";";
            }
            var reversed = _msgTypes.ToDictionary(x => x.Value, x => x.Key);

            buf += "type=" + reversed[_messageType] + ";" + _ending;
            return buf;
        }

        public string BuildValidationString(Device device)
        {
            string ret = "";
            List<Module> modulesToInvalidate = new List<Module>();
            if (device.GetNonValidatedModules(out modulesToInvalidate))
            {
                var counter = 0;
                foreach (var m in modulesToInvalidate)
                {
                    counter++;
                    ret += "module_name-"+ counter + "=" + m.Name + ";module_type-" + counter + "=" + _typeTexts[m.GetType()].TypeText
                        + ";"+ _typeTexts[m.GetType()].ValueText +"-" + counter +"="+ Utility.UnWrapStates(m) +";";
                    m.Validate();
                }
            }

            return ret;
        }
    }
}
