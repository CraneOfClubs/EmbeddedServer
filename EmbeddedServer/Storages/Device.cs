using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.Storages
{

    public class Device
    {
        public String AuthToken { get; set; }
        public bool Authorized { get; private set; }
        public bool? Registered { get; set; }
        public String Name { get; private set; }
        public String ExternalName { get; set; }
        public Int64 CurrentTime { get; set; }

        public DateTime LastSync { get; set; }
        private List<Module> _modules = new List<Module>();

        private String _enqueuedMessage;

        public String GetQueueMessage()
        {
            string res = "";
            res += "destination=" + Name + ";" + _enqueuedMessage;
            _enqueuedMessage = "";
            return res;
        }
        public List<Module> Modules
        {
            get
            {
                return _modules;
            }
            private set { }
        }

        public bool IsModuleExist(string Name){
            foreach(var x in _modules)
            {
                if (x.Name == Name)
                    return true;
            }
            return false;
        }
        public void AddModule (Module module)
        {
            if (IsModuleExist(module.Name))
            {
                var foundModule = _modules.Find(x => x.Name == module.Name);
                _modules.Remove(foundModule);  //Shooting legs here.
                _modules.Add(module);
            }
            else
            _modules.Add(module);
        }

        //public void ChangeModule(Module module, string value)
        //{
        //    if (IsModuleExist(module.Name))
        //    {
        //        var foundModule = _modules.Find(x => x.Name == module.Name);
        //        foundModule.ChangeStateFromDevice(value);
        //    }
        //}

        public void ChangeModuleFromDevice(Module mutable, Module from)
        {
            if (IsModuleExist(mutable.Name))
            {
                var foundModule = _modules.Find(x => x.Name == mutable.Name);
                foundModule.ChangeStateFromDevice(from);
            }
        } 

        public Module GetModuleByName(string name)
        {
            return _modules.Find(x => x.Name == name);
        }

        public Device(String Name, String ExternalName)
        {
            this.Name = Name;
            this.ExternalName = ExternalName;
        }

        public void EnqueueAuth(String authToken)
        {
            AuthToken = authToken;
            Authorized = true;
            _enqueuedMessage += "auth=" + AuthToken + ";";
        }

        public void EnqueueDump(String authToken)
        {
            //_enqueuedMessage += "auth=" + AuthToken + ";";
        }
        public void EnqueueRegister()
        {
            _enqueuedMessage += "auth=awaiting_register;";
        }

        public bool GetNonValidatedModules(out List<Module> modules)
        {
            List<Module> ret = new List<Module>();
            foreach (var m in _modules)
            {
                if (m.ValidationNeeded())
                {
                    ret.Add(m);
                }
            }
            modules = ret;
            return ret.Count != 0;
        }
    }
}
