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
            watchItems.Clear();
            dataGridViewWatch.DataSource = watchItems;

            Assembly assembly = Core.ActiveCsAssembly;
            if (assembly == null) return;

            object[] instances = Core.ThisBook?.Main?.Objects?.OfType<oPage>()
                .Select(page => page.DynInstance)
                .Where(instance => instance != null)
                .ToArray() ?? Array.Empty<object>();

            foreach (Type type in assembly.GetTypes().Where(type => type.IsClass))
            {
                foreach (PropertyInfo property in type.GetProperties())
                {
                    MethodInfo getter = property.GetGetMethod(nonPublic: true);
                    if (getter == null || property.GetIndexParameters().Length != 0) continue;

                    object target = null;
                    if (!getter.IsStatic)
                    {
                        target = instances.FirstOrDefault(instance => type.IsInstanceOfType(instance));
                        if (target == null) continue;
                    }

                    object propertyValue;
                    try
                    {
                        propertyValue = property.GetValue(target);
                    }
                    catch (Exception ex)
                    {
                        propertyValue = "#EX: " + ex.Message;
                    }

                    watchItems.Add(new WatchItem
                    {
                        Type = GetAccessModifier(property),
                        Class = type.FullName,
                        Expression = property.Name,
                        Value = propertyValue
                    });
                }
            }
        }
    }
}
