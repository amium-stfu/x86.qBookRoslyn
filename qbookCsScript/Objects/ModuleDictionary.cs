using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QB
{
    public class ModuleDictionary
    {

        Dictionary<string, Module> Dictionary = new Dictionary<string, Module>();
        public List<Module> Values
        {
            get { lock (Dictionary) { return Dictionary.Values.ToList(); } }
        }
        public List<Module> SortedValues
        {
            get
            {
                lock (Dictionary)
                {
                    var list = Dictionary.Keys.ToList();
                    list.Sort();
                    List<Module> modules = new List<Module>();
                    foreach (string key in list)
                        modules.Add(Dictionary[key]);
                    // Dictionary<string, Module> l = (Dictionary<string, Module>)Dictionary.OrderBy(x => x.Key);

                    return modules;// ues.ToList();// as  as List<Module>;//  Dictionary.OrderBy(x => x.Key).ToList(); 
                }
            }
        }

        public bool Has(string id)
        {
            return Dictionary.ContainsKey(id);
        }
        public bool Has(uint id)
        {
            return Dictionary.ContainsKey(id.ToString("X3"));
        }
        public Module this[string id]
        {
            get
            {
                lock (Dictionary)
                {
                    if (Has(id)) { return Dictionary[id]; }
                    else
                    {
                        var newModule = new Module(id);
                     //   newModule.Tag[TAG.Height] = 40;
                        Dictionary.Add(id, newModule);
                        return newModule;
                    }
                }
            }
            set
            {
                lock (Dictionary)
                {
                    if (Dictionary.ContainsKey(id)) { Dictionary[id] = value; }
                    else { Dictionary.Add(id, value); }
                }
            }
        }
        public Module this[uint id]
        {
            get
            {
                return this[id.ToString("X3")];
            }
            set
            {
                this[id.ToString("X3")] = value;
            }

        }
    }
}
