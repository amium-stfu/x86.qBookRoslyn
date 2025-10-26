using CefSharp.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qbook.Controls
{
    public partial class DebugConsole : Form
    {
        public DebugConsole()
        {
            InitializeComponent();
        }


        class WatchItem
        {
            public string Type { get; set; }
            public string Class { get; set; }
            public string Expression { get; set; }
            public object Value { get; set; }
        }



        static string GetAccessModifier(PropertyInfo property)
        {
            if (property.GetGetMethod(true) != null)
            {
                if (property.GetGetMethod(true).IsPublic && property.GetSetMethod(true) != null && property.GetSetMethod(true).IsPublic)
                {
                    return "public";
                }
                else if (property.GetGetMethod(true).IsPrivate && property.GetSetMethod(true).IsPrivate)
                {
                    return "private";
                }
                else if (property.GetGetMethod(true).IsFamily && property.GetSetMethod(true).IsFamily)
                {
                    return "protected";
                }
                else
                {
                    return "internal";
                }
            }
            else return "n/a";
        }


        BindingList<WatchItem> watchItems = new BindingList<WatchItem>();
        private void buttonRefresh_Click(object sender, EventArgs e)
        {

            //Assembly assembly = qbook.Core.ActiveCsAssembly;

            // Get all types in the assembly
            //Type[] types = assembly.GetTypes();
            Type[] types = qbook.Core.csScript.GetType().Assembly.GetTypes();
            Type qbScript = types.FirstOrDefault(t => t.Name == "QbScript");
            //types = qbScript.GetType().Assembly.GetTypes();
            object castedCsScript = Convert.ChangeType(qbook.Core.csScript, qbook.Core.csScript.GetType()); 
            watchItems.Clear();
            // Filter and display only classes
            //Console.WriteLine("Classes in the assembly:");


            foreach (Type type in types)
            {
                if (type.IsClass)
                {
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        string accessModifier = GetAccessModifier(property);
                        object propertyValue = null;
                        try
                        {
                            //Console.WriteLine($"Type of csScript: {qbook.Core.csScript.GetType()}");
                            //Console.WriteLine($"Type of property: {property.DeclaringType}");
                            propertyValue = property.GetValue(castedCsScript);
                        }
                        catch (Exception ex) 
                        {
                            propertyValue = "#EX: " + ex.Message;
                        }
                        watchItems.Add(new WatchItem { Type = accessModifier, Class = type.FullName, Expression = property.Name, Value = propertyValue });
                    }


                    //PropertyInfo[] properties = type.GetProperties(BindingFlags.Public);
                    //foreach (PropertyInfo property in properties)
                    //{
                    //    watchItems.Add(new WatchItem { Type = "public", Class = type.FullName, Expression = property.Name, Value = null/*property.GetValue(null)*/ });
                    //}

                    //properties = type.GetProperties(BindingFlags.NonPublic);
                    //foreach (PropertyInfo property in properties)
                    //{
                    //    watchItems.Add(new WatchItem { Type = "private", Class = type.FullName, Expression = property.Name, Value = null/*property.GetValue(null)*/ });
                    //}

                    //properties = type.GetProperties(BindingFlags.Instance);
                    //foreach (PropertyInfo property in properties)
                    //{
                    //    watchItems.Add(new WatchItem { Type = "instance", Class = type.FullName, Expression = property.Name, Value = null/*property.GetValue(null)*/ });
                    //}

                }
            }
            dataGridViewWatch.DataSource = watchItems;
        }
    }
}
