using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Parsing.LLParser;
using CSScriptLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using QB; //qbookCsScript

namespace qbook.Scripting
{
    public partial class FormCsScript : Form
    {

        string codeHeader = @"using static QbRoot; public class Script { public static void Go() { ";
        string codeFooter = @" } }";

        string CsCodeSourceFileKey = null;
        public FormCsScript()
        {
            InitializeComponent();

            codeEditor.Document.Language = qbook.Core.CsScriptLanguage;
            CsCodeSourceFileKey = qbook.Core.CsScriptAssembly.SourceFiles.Last().Key;
            codeEditor.Document.SetHeaderAndFooterText(codeHeader, codeFooter);

            codeEditor.Text = "QB.Logger.Info(\"hui\");";
        }

        dynamic script;
        class WatchItem
        {
            public string Term { get; set; }
            public object Result { get; set; }
            public string CompileError { get; set; } = null;
        }
        BindingList<WatchItem> watchItems = new BindingList<WatchItem>();
        string lastMethod = "";
        Regex lineColErrorRegex = new Regex(@"^<script>\((?<line>\d+),(?<col>\d+)\)\: (?<err>.*)$", RegexOptions.Compiled);
        bool watchItemsChanged = true;
        private void timerWatch_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            object[] result = null;
            try
            {
                sw.Restart();
                if (watchItemsChanged) //method != lastMethod)
                {
                    watchItemsChanged = false;
                    //foreach (var wi in watchItems)
                    //    wi.CompileError = null;

                    string code = $"object[] r = new object[{watchItems.Count()}];";
                    int i = 0;
                    foreach (WatchItem item in watchItems)
                    {
                        if (!string.IsNullOrEmpty(item.Term) && !item.Term.Trim().StartsWith("//") && (item.CompileError == null))
                            code += "\r\ntry { r[" + i + "] = " + item.Term + "; } catch (Exception ex) { r[" + i + "]=ex.Message; }";
                        else
                            code += "\r\ntry { r[" + i + "] = null; } catch (Exception ex) { r[" + i + "]=ex.Message; }";
                        i++;
                    }
                    code += "\r\nreturn r;";
                    string method = "public object EvalWatchItems() {\r\n" + code + " }";

                    CSScript.Evaluator.ReferenceAssembliesFromCode(method);
                    lastMethod = method;
                    script = CSScript.Evaluator
                        //.ReferenceAssemblyOf(this)
                        .ReferenceAssemblyByName("qbookCsScript")
                        .LoadMethod(method);
                    //.LoadCode(method);
                }
                else
                {
                    sw.Restart();
                }
                //sw.Restart();
                result = script.EvalWatchItems();
                sw.Stop();
                labelInfoWatch.Text = "refresh took " + sw.ElapsedMilliseconds + "ms";
            }
            catch (Exception ex)
            {
                watchItemsChanged = true;
                string[] errLines = ex.Message.Replace("\r", "").Split('\n');
                foreach (string errLine in errLines)
                {
                    Match m = lineColErrorRegex.Match(errLine);
                    if (m.Success)
                    {
                        int lineNr = int.Parse(m.Groups["line"].Value) - 1;
                        int col = int.Parse(m.Groups["col"].Value) - 1;
                        watchItems[lineNr - 6].CompileError = "#ERR: " + m.Groups["err"];
                    }
                }
                OutputWriteLine($"#EX updating watch-items: " + ex.Message);
            }
            sw.Stop();

            if (result != null)
            {
                //AddResult($"{result?.Length} items in " + sw.ElapsedMilliseconds + "ms");
                int i = 0;
                foreach (WatchItem item in watchItems)
                {
                    //if (item.CompileError == null)
                    if (i < result.Length)
                    {
                        item.Result = result[i];
                        i++;
                    }
                }

                if (!dataGridView1.IsCurrentCellInEditMode)
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = watchItems;
                }
            }
            else
            {
                //AddResult($"#ERR: result is null");
            }
        }

        bool hasPendingParseData = false;
        private void codeEditor_UserInterfaceUpdate(object sender, EventArgs e)
        {
            // If there is a pending parse data change...
            if (hasPendingParseData)
            {
                // Clear flag
                hasPendingParseData = false;

                var parseData = codeEditor.Document.ParseData as ILLParseData;
                if (parseData != null)
                {
                    //if (codeEditor.Document.CurrentSnapshot.Length < 10000)
                    //{
                    //    // Show the AST
                    //    if (parseData.Ast != null)
                    //        astOutputEditor.Text = parseData.Ast.ToTreeString(0).Replace("\t", " ");
                    //    else
                    //        astOutputEditor.Text = null;
                    //}
                    //else
                    //    astOutputEditor.Text = "(Not displaying large AST for performance reasons)";

                    // Output errors
                    this.RefreshErrorList(parseData.Errors);
                }
                else
                {
                    // Clear UI
                    //astOutputEditor.Text = null;
                    this.RefreshErrorList(null);
                }
            }
        }

        private void RefreshErrorList(IEnumerable<IParseError> errors)
        {
            errorListView.Items.Clear();

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    var item = new ListViewItem(new string[] {
                        error.PositionRange.StartPosition.DisplayLine.ToString(), error.PositionRange.StartPosition.DisplayCharacter.ToString(), error.Description
                    });
                    item.Tag = error;
                    errorListView.Items.Add(item);
                }
            }
        }

        private void syntaxEditor1_DocumentParseDataChanged(object sender, EventArgs e)
        {
            hasPendingParseData = true;
        }

        private void FormCsScript_Load(object sender, EventArgs e)
        {
            if (System.IO.File.Exists("CsScript.immediate.txt"))
                codeEditor.Text = System.IO.File.ReadAllText("CsScript.immediate.txt");

            if (System.IO.File.Exists("CsScript.watchitems.txt"))
            {
                foreach (string line in System.IO.File.ReadAllLines("CsScript.watchitems.txt"))
                    watchItems.Add(new WatchItem() { Term = line });
                dataGridView1.DataSource = watchItems;
            }
        }



        private void buttonEval_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            try
            {
                SetOutputColor();
                OutputWriteLine("evaluating code...");

                if (false)
                {
                    //string code = "using static QbRoot; void Go() { " + codeEditor.Text + " }";
                    CSScript.EvaluatorConfig.Access = EvaluatorAccess.Singleton;

                    //string mainEntry = " public void Main() { page1.Go(); }";
                    string programMain = qbook.Core.ProgramMainCode;

                    //1) get all pages' (header-)usings and join together
                    List<string> usingList = new List<string>();
                    foreach (var page in qbook.Core.ThisBook.Main.Objects)
                        usingList.AddRange(QB.ScriptHelpers.GetUsingsFromCode(page.CsCodeHeader));
                    usingList = usingList.Distinct().ToList();
                    string usingsCode = string.Join("\r\n", usingList.Select(i => "using " + i + ";"));

                    //2) join all pages' code
                    string code = "";
                    foreach (var page in qbook.Core.ThisBook.Main.Objects)
                    {
                        code += "\r\n\r\n//=== class '" + page.FullName + "' ===\r\n";
                        code += "public class @class_" + page.FullName + " {";
                        code += QB.ScriptHelpers.StripUsingsFromCode(page.CsCode, out int offset);
                        code += "\r\n}";
                    }

                    //string scriptCode = "class Script { void ScriptGo() { " + codeEditor.Text + " } }";
                    string scriptCode = codeEditor.Text;

                    //3) fullCode = (joinedHeader + programMain + joinedPages + commonFooter)
                    //string fullCode = usingsCode + Main.Qb.ProgramMainCode.Replace("QbRoot.main.Go();", scriptCode) + code; // + "\r\n" + scriptCode; // + "}";
                    string fullCode = usingsCode + "\r\npublic static void Go() { \r\n" + scriptCode + " }"; // + "\r\n" + scriptCode; // + "}";
                    fullCode = fullCode.Replace("using static QbRoot;", "");


                    sw.Restart();

                    CSScript.Evaluator.ReferenceAssembliesFromCode(fullCode);
                    script = CSScript.Evaluator
                                .ReferenceAssemblyOf(this)
                                //.ReferenceAssembly(Main.Qb.CsScript_ass)
                                .ReferenceAssemblyByName("qbookCsScript")
                                .LoadMethod(fullCode)
                            //.LoadCode(fullCode)
                            ;

                    //script.Main(null);
                    script.Go();
                }
                if (true)
                {
                    string scriptCode = codeEditor.Text;
                    object result = RunInteractive(scriptCode);
                }

                sw.Stop();
                OutputWriteLine("evaluating code... OK (took " + sw.ElapsedMilliseconds + "ms)");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                sw.Stop();
                SetOutputColor(Color.LightCoral);
                OutputWriteLine("evaluating code... ERROR (took " + sw.ElapsedMilliseconds + "ms)");
                tabControl3.SelectedTab = tabPageOutput;
                OutputWrite(ex.Message); //.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"));
            }
            finally
            {
            }
        }

        public object RunInteractive(string code)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            try
            {
                this.Cursor = Cursors.WaitCursor;

                sw.Restart();
                //string fullCode = programMain + "\r\n\r\n//---pages---"
                //    + textBoxHeader.Text
                //    + codeEditor.Text
                //    + textBoxFooter.Text;

                string wrappedCode = code;
                try
                {
                    var temp_types = qbook.Core.CsScript_ass.GetExportedTypes();

                    wrappedCode = "object Func() { " + code + " \r\nreturn null;}";
                    //wrappedCode = "using static css_root.QbRoot;\r\n" + wrappedCode;
                    //CSScript.Evaluator.ReferenceAssembliesFromCode(wrappedCode);
                    //CSScript.Evaluator.ReferenceAssemblyOf(Main.Qb.CsScript_ass);
                    script = CSScript.Evaluator
                        .ReferenceAssemblyByName("qbookCsScript")
                        .ReferenceAssemblyOf(qbook.Core.CsScript_ass)
                        .LoadMethod(wrappedCode);

                    return script.Func();
                }
                catch (CSScriptLib.CompilerException ex)
                {
                    if (ex.Message.Contains("CS0029")) //result is void
                    {
                        wrappedCode = "void Func() { " + code + "}";
                        script = CSScript.Evaluator
                            .ReferenceAssemblyByName("qbookCsScript")
                            .LoadMethod(wrappedCode);
                        script.Func();
                        return null;
                    }
                    else
                    {
                        tabControl3.SelectedTab = tabPageOutput;
                        OutputWriteLine("#EX: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {

                    //toolStripLabelInfo.Text = "#EX: " + ex.Message;
                    //toolStripLabelInfo.Text = "#ERR: rebuild took " + sw.ElapsedMilliseconds.ToString() + "ms";
                    tabControl3.SelectedTab = tabPageOutput;
                    OutputWriteLine("#EX: " + ex.Message);
                }
            }
            finally
            {
                sw.Stop();
                this.Cursor = Cursors.Default;
                //toolStripLabelInfo.Text = "Success: execution took " + sw.ElapsedMilliseconds.ToString() + "ms";
            }

            return null;
        }


        void OutputWrite(string text)
        {
            //textBoxOutput.Text += "\r\n" + textBoxOutput.Text.TrimEnd();
            textBoxOutput.Text += "\r\n" + DateTime.Now.ToString("HH:mm:ss.fff") + ": "
                + text.Replace("\r\n", "\r\n    ").TrimEnd();

            textBoxOutput.SelectionStart = textBoxOutput.TextLength;
            textBoxOutput.ScrollToCaret();
        }

        int repeatCounter = 1;
        void OutputWriteLine(string text)
        {
            //HACK -> change to output to List<string>
            string lastLine = textBoxOutput.Lines.Length > 0 ? textBoxOutput.Lines.Last() : "";
            int repeatIndex = lastLine.IndexOf(" [*");
            if ((lastLine.Length > 14 && repeatIndex == -1 && lastLine.Substring(14) == text)
                || (lastLine.Length > 14 && repeatIndex >= 0 && lastLine.Substring(14, repeatIndex - 14) == text))
            {
                repeatCounter++;
                textBoxOutput.Text = string.Join(Environment.NewLine, textBoxOutput.Lines.Take(textBoxOutput.Lines.Count() - 1));
                text += $" [*{repeatCounter}]";
                OutputWrite(text + "\r\n");
            }
            else
            {
                repeatCounter = 1;
                OutputWrite(text + "\r\n");
            }
        }

        void SetOutputColor(Color? backColor = null, Color? foreColor = null)
        {
            textBoxOutput.BackColor = backColor ?? Color.White;
            textBoxOutput.ForeColor = foreColor ?? Color.Black;
        }


        private void FormCsScript_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                buttonEval.PerformClick();
            }
        }

        private void buttonClearOutput_Click(object sender, EventArgs e)
        {
            textBoxOutput.Text = "";
        }

        private void FormCsScript_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CsCodeSourceFileKey != null && qbook.Core.CsScriptAssembly.SourceFiles.Contains(CsCodeSourceFileKey))
                qbook.Core.CsScriptAssembly.SourceFiles.Remove(CsCodeSourceFileKey);

            System.IO.File.WriteAllText("CsScript.immediate.txt", codeEditor.Text);
            System.IO.File.WriteAllText("CsScript.watchitems.txt", string.Join(Environment.NewLine, watchItems.Select(w => w.Term)));
        }

        class ObjectItem
        {
            public bool Selected { get; set; }
            public string Key { get; set; }
            public string Info { get; set; }

            public QB.Item MyObject { get; set; }

            public override string ToString()
            {
                return $"{Key}: {Info}";
            }
        }

        BindingList<ObjectItem> objectItems = new BindingList<ObjectItem>();

        string lastObjectListKeys = "";
        string lastWidgetListKeys = "";
        bool forceObjectListRefresh = false;
        Stopwatch sw = new Stopwatch();
        private void timerObjectList_Tick(object sender, EventArgs e)
        {
            if (false)
            {
                PopulateObjectsFromCsScript(qbook.Core.ActiveCsAssembly);
                return;
            }


            sw.Restart();
            try
            {
                var objectListKeys = string.Join(",", QB.Root.ObjectDict.Keys);
                var widgetListKeys = string.Join(",", QB.Root.ControlDict.Keys);
                if (checkBoxObjects.Checked && (objectListKeys != lastObjectListKeys)
                    || (checkBoxWidgets.Checked && (widgetListKeys != lastWidgetListKeys))
                    || forceObjectListRefresh)
                {
                    forceObjectListRefresh = false;
                    lastObjectListKeys = objectListKeys;
                    lastWidgetListKeys = widgetListKeys;
                    objectItems.Clear();
                    if (checkBoxObjects.Checked)
                    {
                        foreach (var kvp in QB.Root.ObjectDict)
                        {
                            objectItems.Add(new ObjectItem()
                            {
                                MyObject = kvp.Value,
                                Selected = false,
                                Key = kvp.Key,
                                Info = $"{kvp.Value.GetType().Name.PadRightFixed(6)}:{kvp.Value.Directory}.{kvp.Value.Name}('{kvp.Value.Name}')"
                            });
                        }
                    }
                    if (checkBoxWidgets.Checked)
                    {
                        foreach (var kvp in QB.Root.ControlDict)
                        {
                            objectItems.Add(new ObjectItem()
                            {
                                MyObject = kvp.Value,
                                Selected = false,
                                Key = kvp.Key,
                                Info = $"{kvp.Value.GetType().Name.PadRightFixed(6)}:{kvp.Value.Directory}.{kvp.Value.Name}('{kvp.Value.Name}')"
                            });
                        }
                    }
                    //listBoxObjects.Items.Clear();
                    try
                    {
                        var lb = listBoxObjects as ListBox;
                        //lb.DataSource = objectItems;
                        lb.Items.Clear();
                        lb.Items.AddRange(objectItems.ToArray());
                        lb.DisplayMember = "Info";
                        lb.ValueMember = "Key";
                    }
                    catch (Exception ex)
                    {

                    }
                }

                string info = "";
                foreach (ObjectItem item in listBoxObjects.CheckedItems)
                {
                    QB.Item qbo = item.MyObject;
                    info += $"--- {qbo.GetType().FullName}: {qbo.Directory}.{qbo.Name} ('{qbo.Name}') ---\r\n";
                    foreach (var f in qbo.GetType().GetFields())
                    {
                        info += $"  f:{f.Name}={f.GetValue(qbo)}\r\n";
                    }
                    foreach (var p in qbo.GetType().GetProperties())
                    {
                        info += $"  p:{p.Name}={p.GetValue(qbo)}\r\n";
                    }
                    if (checkBoxMethods.Checked)
                    {
                        foreach (var m in qbo.GetType().GetMethods())
                        {
                            if (m.Name.StartsWith("set_") || m.Name.StartsWith("get_")
                                || m.Name.StartsWith("ToString") || m.Name.StartsWith("Equals") || m.Name.StartsWith("GetHashCode") || m.Name.StartsWith("GetType"))
                                continue;
                            string pInfo = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
                            info += $"  m:{m.ReturnParameter.ParameterType.Name} {m.Name}({pInfo})\r\n";
                        }
                    }
                }
                textBoxObjectInfo.Text = info;
                sw.Stop();
                labelInfo.Text = "refresh took " + sw.ElapsedMilliseconds + "ms";
            }
            catch (Exception ex)
            {
                sw.Stop();
                labelInfo.Text = "#ERR: refresh took " + sw.ElapsedMilliseconds + "ms";
            }
        }

        class ScriptObject
        {
            //public int Key { get; set; }
            public string Info { get; set; }

            public Object MyObject { get; set; }

            public override string ToString()
            {
                return $"{Info}";
            }
        }

        void PopulateObjectsFromCsScript(System.Reflection.Assembly assembly)
        {
            if (assembly == null)
            {
                listBoxObjects.Items.Clear();
                listBoxObjects.Items.Add("# no script assembly #");
                return;
            }

            try
            {
                List<ScriptObject> objectList = new List<ScriptObject>();
                //if (forceObjectListRefresh)
                {
                    //foreach (var t in ((System.Reflection.Assembly)script.GetType().Assembly).ExportedTypes)
                    foreach (var t in assembly.ExportedTypes)
                    {
                        if (t.Name.StartsWith("@class_"))
                        {
                            objectList.Add(new ScriptObject() { MyObject = t, Info = $"=== class '{t.Name.Substring("@class_".Length)}' ===" });
                            var properties = ((System.Reflection.TypeInfo)t).DeclaredProperties;
                            foreach (var p1 in properties)
                            {
                                if (p1.PropertyType.BaseType.Name == "qbObject" && !checkBoxObjects.Checked)
                                    continue;
                                if (p1.PropertyType.BaseType.Name == "qbWidget" && !checkBoxWidgets.Checked)
                                    continue;

                                bool canPublicGet = (p1.CanRead && p1.GetGetMethod() != null);
                                bool canPublicSet = (p1.CanWrite && p1.GetSetMethod() != null);
                                if (canPublicGet || canPublicSet)
                                {
                                    string getSetInfo = "";
                                    if (canPublicGet && canPublicSet)
                                        getSetInfo = "(get+set)";
                                    else if (canPublicGet && !canPublicSet)
                                        getSetInfo = "(get)";
                                    else if (!canPublicGet && canPublicSet)
                                        getSetInfo = "(set)";
                                    else if (!canPublicGet && !canPublicSet)
                                        getSetInfo = "(-)";
                                    objectList.Add(new ScriptObject() { MyObject = t, Info = $"p [{p1.PropertyType.Name.PadRightFixed(6)}] {p1.Name} {getSetInfo}" });
                                    foreach (var p in p1.PropertyType.GetProperties())
                                    {
                                    }
                                    //p.PropertyType.GetFields();
                                    //p.PropertyType.GetMethods();
                                }
                            }

                            var fields = ((System.Reflection.TypeInfo)t).DeclaredFields;
                            foreach (var f1 in fields)
                            {
                                if (f1.FieldType.BaseType.Name == "qbObject" && !checkBoxObjects.Checked)
                                    continue;
                                if (f1.FieldType.BaseType.Name == "qbWidget" && !checkBoxWidgets.Checked)
                                    continue;

                                if (f1.Attributes == System.Reflection.FieldAttributes.Public)
                                {
                                    objectList.Add(new ScriptObject() { MyObject = t, Info = $"f [{f1.FieldType.Name.PadRightFixed(6)}] {f1.Name}" });
                                    //var v = f1.GetValue(t);
                                    //f.FieldType.GetProperties();
                                    //f.FieldType.GetFields();
                                    //f.FieldType.GetMethods();
                                }
                            }

                            var fieldsRT = ((System.Reflection.TypeInfo)t).GetRuntimeFields();
                            foreach (var f1 in fieldsRT)
                            {
                                if (f1.FieldType.BaseType.Name == "qbObject" && !checkBoxObjects.Checked)
                                    continue;
                                if (f1.FieldType.BaseType.Name == "qbWidget" && !checkBoxWidgets.Checked)
                                    continue;

                                if (f1.Attributes == System.Reflection.FieldAttributes.Public)
                                {
                                    objectList.Add(new ScriptObject() { MyObject = t, Info = $"RTf [{f1.FieldType.Name.PadRightFixed(6)}] {f1.Name}" });
                                    //var v = f1.GetValue(t);
                                    //f.FieldType.GetProperties();
                                    //f.FieldType.GetFields();
                                    //f.FieldType.GetMethods();
                                }
                            }

                            var methods = ((System.Reflection.TypeInfo)t).DeclaredMethods;
                            foreach (var m in methods)
                            {
                                if (m.Attributes == System.Reflection.MethodAttributes.Public)
                                {
                                    string pInfo = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
                                    string signature = $"{m.ReturnParameter.ParameterType.Name} {m.Name}({pInfo})";

                                    objectList.Add(new ScriptObject() { MyObject = t, Info = $"m {signature}" });

                                    //objectItems.Add(new ObjectItem()
                                    //{
                                    //    MyObject = p,
                                    //    Selected = false,
                                    //    Key = p.Name,
                                    //    Info = $"{p.Name.PadRightFixed(6)} = {p.GetValue(p1)}"
                                    //});
                                }
                            }
                        }
                    }
                }
                listBoxObjects.Items.Clear();
                listBoxObjects.Items.AddRange(objectList.ToArray());
                listBoxObjects.DisplayMember = "Info";
                listBoxObjects.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                listBoxObjects.Items.Clear();
                listBoxObjects.Items.Add("#EX populating items:");
                listBoxObjects.Items.Add(ex.Message);
            }
        }

        private void checkBoxObjects_CheckedChanged(object sender, EventArgs e)
        {
            forceObjectListRefresh = true;
        }

        private void checkBoxWidgets_CheckedChanged(object sender, EventArgs e)
        {
            forceObjectListRefresh = true;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            foreach (var wi in watchItems)
                wi.CompileError = null;

            watchItemsChanged = true;
        }
    }
}
