using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static QB.Item;

namespace QB.UI
{
    public partial class ParametersDialog : Form
    {

        public ParametersDialog(CParameter parameters)
        {
            InitializeComponent();
            int y = 0;

            foreach (object parameter in parameters.Values)
            {
                if (parameter is Signal)
                {
                    ParameterControl pc = new ParameterControl((parameter as Signal).Name, (Signal)parameter);
                    pc.Location = new Point(10, y);
                    y += 70;
                    Controls.Add(pc);
                    // if (!string.IsNullOrEmpty(prop.Editor))
                    //   pc.SetEditor(prop.Editor);
                    pc.Validated += Pc_Validated;

                    //        ParameterControl pc = new ParameterControl("?", parameter);
                    //        pc.Location = new Point(0, y);
                    //        y += 75;
                    //        Controls.Add(pc);
                }
            }
            Show();
            BringToFront();
        }


        class PropertyItem
        {
            public string Name = null;
            public object Class = null;
            public object Source = null;
            public Type SourceType = null;
            public string Category = null;
            public string Description = null;
            public string Editor = null; //TODO
        }


        public ParametersDialog(object myClass)
        {
            InitializeComponent();
            int y = 0;

            string title = "";
            if (myClass is Item)
            {
                var item = (Item)myClass;
                title += $"{item.Name}@{(item.Directory ?? "root")}";
            }
            title += $" :{myClass.GetType().Name}";
            this.Text = title;
            List<PropertyItem> PropertyList = new List<PropertyItem>();
            var fields = myClass.GetType().GetFields();
            var properties = myClass.GetType().GetProperties();
            foreach (var f in fields)
            {
                if (f.GetCustomAttributes(false).Contains(BrowsableAttribute.Yes))
                {
                    string category = null;
                    try
                    {
                        CategoryAttribute categoryA = ((CategoryAttribute)f.GetCustomAttributes(typeof(CategoryAttribute), false)[0]);
                        if (categoryA != null)
                            category = categoryA.Category;
                    }
                    catch (Exception ex)
                    {

                    }

                    string description = null;
                    try
                    {
                        DescriptionAttribute descriptionA = ((DescriptionAttribute)f.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]);
                        if (descriptionA != null)
                            description = descriptionA.Description;
                    }
                    catch (Exception ex)
                    {

                    }

                    string editorType = null;
                    string editorBase = null;
                    try
                    {
                        EditorAttribute editorA = ((EditorAttribute)f.GetCustomAttributes(typeof(EditorAttribute), false)[0]);
                        if (editorA != null)
                        {
                            editorType = editorA.EditorTypeName;
                            editorBase = editorA.EditorBaseTypeName;
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    PropertyItem pi = new PropertyItem();
                    pi.Class = myClass;
                    pi.Source = f.GetValue(myClass);
                    pi.SourceType = f.FieldType;
                    pi.Name = f.Name;
                    pi.Category = category;
                    pi.Description = description;
                    pi.Editor = editorType;
                    PropertyList.Add(pi);
                }
            }

            foreach (var f in properties)
            {
                if (f.GetCustomAttributes(false).Contains(BrowsableAttribute.Yes))
                {
                    CategoryAttribute categoryA = ((CategoryAttribute)f.GetCustomAttributes(typeof(CategoryAttribute), false)[0]);
                    string category = null;
                    if (categoryA != null)
                        category = categoryA.Category;
                    DescriptionAttribute descriptionA = ((DescriptionAttribute)f.GetCustomAttributes(typeof(DescriptionAttribute), false)[0]);
                    string description = null;
                    if (descriptionA != null)
                        description = descriptionA.Description;

                    PropertyItem pi = new PropertyItem();
                    pi.Class = myClass;
                    pi.Source = f;
                    pi.SourceType = f.PropertyType;
                    pi.Name = f.Name;
                    pi.Category = category;
                    pi.Description = description;
                    PropertyList.Add(pi);
                }
            }

            foreach (string category in PropertyList.Select(p => p.Category).Distinct().OrderBy(c => c))
            {
                y += 10;
                Label l = new Label();
                l.Font = new Font(this.Font, FontStyle.Bold);
                l.AutoSize = true;
                l.Text = category;
                l.Location = new Point(6, y);
                Controls.Add(l);
                y += 20;

                var props = PropertyList.Where(c => c.Category == category).OrderBy(p => p.Name);
                foreach (var prop in props)
                {
                    if (prop.SourceType.FullName == typeof(Signal).FullName)
                    //if (prop.Source is Signal)
                    {
                        //Signal signal = prop
                        ParameterControl pc = new ParameterControl(prop.Name, (Signal)prop.Source);
                        pc.Location = new Point(10, y);
                        y += 70;
                        Controls.Add(pc);
                        if (!string.IsNullOrEmpty(prop.Editor))
                            pc.SetEditor(prop.Editor);
                        pc.Validated += Pc_Validated;
                    }
                    else
                    {
                        ParameterControl pc = new ParameterControl(prop.Name, null);
                        pc.Location = new Point(10, y);
                        y += 70;
                        Controls.Add(pc);
                    }
                }
            }

            //foreach (Signal parameter in parameters.Values)
            //{
            //    ParameterControl pc = new ParameterControl(parameter);
            //    pc.Location = new Point(0, y);
            //    y += 30;
            //    Controls.Add(pc);
            //}
            Show();
            BringToFront();
        }

        private void Pc_Validated(object sender, EventArgs e)
        {
            ParameterControl pc = sender as ParameterControl;
            pc.MySignal.Value = pc.Value;
        }
    }
}
