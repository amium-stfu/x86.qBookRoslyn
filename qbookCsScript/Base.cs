using QB.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
//using System.Windows.Forms;

[assembly: InternalsVisibleTo("qbookStudio")]

namespace QB
{
    /*
    public static class Settings
    {
        public static string BookFileName
        {
            get
            {
               return Root.ActiveQbook.BookFileName;
            }
        }
        public static class Directory
        {
            public string Data;
        }
    }
    */

    

    public static class Root
    {
        public static Dictionary<string, object> ClassDict = new Dictionary<string, object>();

        public static Dictionary<string, Item> ObjectDict = new Dictionary<string, Item>();
        public static Dictionary<string, Controls.Control> ControlDict = new Dictionary<string, Controls.Control>();

        public static Dictionary<string, Module> ModuleDict = new Dictionary<string, Module>();
        public static Dictionary<string, Signal> SignalDict = new Dictionary<string, Signal>();

        public static Dictionary<string, Message> MessageDict = new Dictionary<string, Message>();

        public static dynamic ActiveQbook = null;        
              


        static Root()
        {
            //AppDomain currentDomain = AppDomain.CurrentDomain;
            //currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);

            //Assembly assembly1 = Assembly.LoadFrom(@".\libs\ASAP2.dll");
            //Assembly assembly2 = Assembly.LoadFrom(@".\libs\ASAP2If.dll");
            //Type type = assembly.GetType("MyType");
            //object instanceOfMyType = Activator.CreateInstance(type);
        }
        /*
        static private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            string libsDir = ".\\libs\\";
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly, objExecutingAssembly;
            string strTempAssmbPath = "";

            objExecutingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssembly.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                //Check for the assembly names that have raised the "AssemblyResolve" event.
                if (strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == args.Name.Substring(0, args.Name.IndexOf(",")))
                {
                    //Build the path of the assembly from where it has to be loaded.                
                    strTempAssmbPath = libsDir + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
                    break;
                }

            }

            //Load the assembly from the specified path.                    
            MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return MyAssembly;
        }
        */

        //static public string DataDirectory()
        //{
        //    // check @ can dbc imort!!
        //    /*
        //    if ((QB.Root.ActiveQbook) != null && (filename.StartsWith("./") || filename.StartsWith(@".\")))
        //    {
        //        //use the qbook's directory if a relative path is given
        //        filename = Path.GetFullPath(Path.Combine(QB.Root.ActiveQbook.Directory, filename));
        //    }
        //    */
        //    if (QB.Root.ActiveQbook == null) return "";

        //    return Path.GetFullPath(Path.Combine(QB.Root.ActiveQbook.Directory, QB.Root.ActiveQbook.FileName));
        //}



        public static void AddObject(string name, Item o)
        {
            lock (ObjectDict)
            {
                if (!ObjectDict.ContainsKey(name))
                    ObjectDict.Add(name, o);
            }
        }
        public static Item GetObject(string name)
        {
            lock (ObjectDict)
            {
                if (ObjectDict.ContainsKey(name))
                    return ObjectDict[name];
                else
                    return null;
            }
        }

        public static void AddWidget(string name, Controls.Control o)
        {
            lock (ControlDict)
            {
                if (!ControlDict.ContainsKey(name))
                    ControlDict.Add(name, o);
            }
        }
        public static Controls.Control GetWidget(string name)
        {
            lock (ControlDict)
            {
                if (ControlDict.ContainsKey(name))
                    return ControlDict[name];
                else
                    return null;
            }
        }
        public static List<Controls.Control> GetControlsByDirectory(string dir)
        {
            lock (ControlDict)
            {
                //return WidgetDict.Where(i => i.Key.StartsWith(dir))
                return ControlDict.Values.Where(i => i.Directory == dir).ToList();
            }
        }

        public static void ResetObjectDict()
        {
            //do some necessary cleanup (especially timers, callbacks, ...)
            foreach (Item o in ObjectDict.Values.ToList())
            {
                //if (o is Timer)
                //    (o as Timer).Destroy();

                if (o is Item)
                    (o as Item).Destroy();
            }

            if (false) //HALE: test 2025-05-22
            {
                foreach (object o in ClassDict.Values.ToList())
                {
                    if (o.GetType().Name == "oPage")
                    {
                        Type type = o.GetType();
                        Type targetType = Type.GetType("qbook.oPage, qbookStudio");
                        object castedObject = Convert.ChangeType(o, targetType);
                        MethodInfo destroyMethod = type.GetMethod("Destroy");
                        if (destroyMethod != null && destroyMethod.GetParameters().Length == 0)
                        {
                            destroyMethod.Invoke(o, null);
                        }
                        else
                        {
                        }
                    }
                }
            }

            ObjectDict.Clear();

            lock (Root.SignalDict)
                SignalDict.Clear();
            lock (Root.ModuleDict)
                ModuleDict.Clear();
        }
        public static void ResetWidgetDict()
        {
            //do some necessary cleanup (especially timers, callbacks, ...)
            lock (ControlDict)
            {
                foreach (var o in ControlDict.Values)
                {
                    o.Stop();
                }
                ControlDict.Clear();
            }
        }

        public static void ResetModuleDict()
        {
            //do some necessary cleanup (especially timers, callbacks, ...)
            lock (ModuleDict)
            {
                foreach (var o in ModuleDict.Values)
                {
                    o.Destroy();
                }
                ModuleDict.Clear();
            }
        }

        public static void ResetSignalDict()
        {
            //do some necessary cleanup (especially timers, callbacks, ...)
            lock (SignalDict)
            {
                foreach (var o in SignalDict.Values)
                {
                    o.Destroy();
                }
                SignalDict.Clear();
            }
        }

        public static void ResetMessageDict()
        {
            //do some necessary cleanup (especially timers, callbacks, ...)
            lock (MessageDict)
            {
                foreach (var o in MessageDict.Values)
                {
                    o.Destroy();
                }
                MessageDict.Clear();
            }
        }

        public static void ResetAllDicts()
        {
            ResetObjectDict();
            ResetWidgetDict();
            ResetModuleDict();
            ResetSignalDict();
            ResetMessageDict();
        }

        public static void InvalidateBoxBounds()
        {
            lock (ControlDict)
            {
                foreach (var box in ControlDict.Values.OfType<Box>().Where(b => b.Parent == null))
                {
                    //top-level boxes only here...
                    box.InvalidateBounds();
                }
            }
        }


        //public static void UpdateBoxBounds()
        //{
        //    //do some necessary cleanup (especially timers, callbacks, ...)
        //    lock (ControlDict)
        //    {
        //        foreach (var box in ControlDict.Values.OfType<Box>().Where(b => b.ParentBox == null))
        //        {
        //            box.UpdatePageBounds();
        //        }
        //    }
        //}


        public static void RemoveControl(Guid guid)
        {
            string id = guid.ToString();
            RemoveControl(id);
        }
        public static void RemoveControl(string id)
        {
            lock (ControlDict)
            {
                var o = ControlDict.FirstOrDefault(i => i.Value.Id == id).Key;
                if (o != null)
                {
                    ControlDict.Remove(o);
                }
                else
                {

                }
            }
        }
        public static void AddControl(string id, Controls.Control c)
        {
            lock (ControlDict)
            {
                if (ControlDict.ContainsKey(id))
                {
                    //ControlDict[id] = c;
                    ControlDict.Remove(id);
                    ControlDict.Add(id, c);
                }
                else
                    ControlDict.Add(id, c);
            }
        }
    }

    public enum AccessLevel { Online, Offline, User, Service, Admin, Root };
}
