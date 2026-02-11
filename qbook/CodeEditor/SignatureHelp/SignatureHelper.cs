using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbook.CodeEditor.SignatureHelp
{
    public class SignatureHelper
    {

        private readonly FormEditor Root;
        private readonly Scintilla Editor;
        private readonly RoslynServices _roslyn;
        ToolStripDropDown dropDown;
        ListBox listBox;

        public SignatureHelper(FormEditor parent, RoslynServices roslyn)
        {
            Root = parent;
            _roslyn = roslyn;
            Editor = Root.Editor;

            listBox = new ListBox
            {
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                IntegralHeight = true,
                SelectionMode = SelectionMode.One,
                Font = new Font("Consolas", 10),
                BackColor = Color.White,
                ForeColor = Color.Black
            };

            var host = new ToolStripControlHost(listBox)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false
            };

            dropDown = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                AutoClose = false

            };

            dropDown.Items.Add(host);

            Editor.KeyDown += Editor_KeyDown;
            Editor.CharAdded += Editor_CharAdded;
            Editor.CharAdded += async (s, e) =>
            {
                char c = (char)e.Char;

                if (Root.GetCurrentDocument() == null)
                    return;

                // Signature Help
                if (c == '(' || c == ',')
                    await TrySignatureHelpAsync();
            };




            listBox.Click += (s, e) => CommitSelection();
            listBox.KeyDown += (s, e) =>
            {


                if (e.KeyCode == Keys.Enter)
                {
                    CommitSelection();
                    e.Handled = true;
                }

                if (e.KeyCode == Keys.Tab)
                {
                    CommitSelection();
                    e.Handled = true;
                }

            };
        }


        public async Task<bool> TrySignatureHelpAsync(bool updateOnly = false)
        {
            try
            {
                if (!Root.NodeIsActive)
                {
                    if (!updateOnly) Editor.CallTipCancel();
                    return false;
                }
                int pos = Math.Max(0, Editor.CurrentPosition);

                var doc = Root.GetCurrentDocument();
                if (doc == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }
                var tree = await doc.GetSyntaxTreeAsync().ConfigureAwait(false);
                if (tree == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }
                var root = await tree.GetRootAsync().ConfigureAwait(false);
                var token = root.FindToken(Math.Max(0, pos - 1));
                var node = token.Parent;
                while (node != null)
                {
                    if (node is InvocationExpressionSyntax inv && inv.ArgumentList != null)
                        return await ShowSignatureForInvocationAsync(inv, doc, pos, updateOnly);
                    if (node is ObjectCreationExpressionSyntax obj && obj.ArgumentList != null)
                        return await ShowSignatureForCreationAsync(obj, doc, pos, updateOnly);
                    node = node.Parent;
                }
                if (!updateOnly) Editor.CallTipCancel();
                return false;
            }
            catch
            {
                return false;
            }
        }
        private int GetArgumentIndex(ArgumentListSyntax list, int caretPos)
        {
            int idx = 0;
            foreach (var a in list.Arguments)
            {
                if (caretPos > a.FullSpan.Start) idx++; else break;
            }
            if (idx > 0 && idx == list.Arguments.Count && caretPos <= list.CloseParenToken.FullSpan.Start)
                idx--; // Cursor direkt hinter letztem Argument
            return Math.Max(0, idx);
        }
        private async Task<bool> ShowSignatureForInvocationAsync(InvocationExpressionSyntax inv, RoslynDocument doc, int caretPos, bool updateOnly)
        {
            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var info = model.GetSymbolInfo(inv);
            var methodGroup = info.Symbol != null ? new[] { info.Symbol } :
                (info.CandidateSymbols.IsDefaultOrEmpty ? Array.Empty<ISymbol>() : info.CandidateSymbols.ToArray());

            var methods = methodGroup.OfType<IMethodSymbol>().ToArray();

            if (methods.Length == 0)
            {
                var exprType = model.GetTypeInfo(inv.Expression).Type as INamedTypeSymbol;
                if (exprType != null)
                {
                    var name = inv.Expression switch
                    {
                        MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
                        IdentifierNameSyntax id => id.Identifier.ValueText,
                        _ => null
                    };
                    if (!string.IsNullOrEmpty(name))
                        methods = exprType.GetMembers(name).OfType<IMethodSymbol>().ToArray();
                }
            }

            if (methods.Length == 0) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            int argIndex = inv.ArgumentList != null ? GetArgumentIndex(inv.ArgumentList, caretPos) : 0;
            var bestMethod = methods.FirstOrDefault(m => m.Parameters.Length > argIndex) ?? methods.First();

            ShowParameterList(bestMethod, inv.ArgumentList!, caretPos);
            return true;
        }
        private async Task<bool> ShowSignatureForCreationAsync(ObjectCreationExpressionSyntax obj, RoslynDocument doc, int caretPos, bool updateOnly)
        {
            var model = await doc.GetSemanticModelAsync().ConfigureAwait(false);
            if (model == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var type = model.GetTypeInfo(obj).Type as INamedTypeSymbol ?? model.GetSymbolInfo(obj.Type).Symbol as INamedTypeSymbol;
            if (type == null) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            var ctors = type.InstanceConstructors.Where(c => !c.IsStatic).OrderBy(c => c.Parameters.Length).ToArray();
            if (ctors.Length == 0) { if (!updateOnly) Editor.CallTipCancel(); return false; }

            int argIndex = GetArgumentIndex(obj.ArgumentList!, caretPos);
            var bestCtor = ctors.FirstOrDefault(c => c.Parameters.Length > argIndex) ?? ctors.First();

            ShowParameterList(bestCtor, obj.ArgumentList!, caretPos);
            return true;
        }
      

        public bool Visible => dropDown.Visible;
        public void Hide() => dropDown.Close();



        private void ShowParameterList(IMethodSymbol constructor, ArgumentListSyntax argumentList, int caretPos)
        {
            listBox.Items.Clear(); // Liste leeren

            var usedParams = argumentList.Arguments
                .Select(arg => arg.NameColon?.Name.Identifier.ValueText)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet();

            int nameLength = 0;
            foreach (var param in constructor.Parameters) 
                if (param.Name.Length > nameLength) nameLength = param.Name.Length;

            nameLength++;

                foreach (var param in constructor.Parameters)
            {
                string name = param.Name;

                // Nur anzeigen, wenn noch nicht verwendet
                if (usedParams.Contains(name))
                    continue;

                string defaultValue = param.HasExplicitDefaultValue ? param.ExplicitDefaultValue?.ToString() ?? "null" : "—";
                string type = param.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                string value = GetArgumentValue(name, argumentList) ?? defaultValue;
                string display = $"{name.PadRight(nameLength)}: ({type})";
                listBox.Items.Add(display);

                if (IsCaretInArgument(name, argumentList, caretPos))
                {
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                    listBox.SelectionMode = SelectionMode.One;
                }
            }

            dropDown.Items.Clear(); // alte Hosts entfernen
            var host = new ToolStripControlHost(listBox);
            dropDown.Items.Add(host);

            listBox.Font = Root.GetFont();
            if (Root.DarkTheme)
            {
                listBox.BackColor = Color.FromArgb(25, 25, 25);
                listBox.ForeColor = Color.FromArgb(200, 200, 200);

            }
            else
            {
                listBox.BackColor = Color.White;
                listBox.ForeColor = Color.Black;
            }



            int pos = Editor.CurrentPosition;
            int x = Editor.PointXFromPosition(pos);
            int y = Editor.PointYFromPosition(pos) + 18;
            Point screenPoint = Editor.PointToScreen(new Point(x, y));
            dropDown.Show(screenPoint);
        }


        private string? GetArgumentValue(string paramName, ArgumentListSyntax args)
        {
            foreach (var arg in args.Arguments)
            {
                if (arg.NameColon?.Name.Identifier.ValueText == paramName)
                    return arg.Expression.ToString();
            }
            return null;
        }

        private bool IsCaretInArgument(string paramName, ArgumentListSyntax args, int caretPos)
        {
            foreach (var arg in args.Arguments)
            {
                if (arg.NameColon?.Name.Identifier.ValueText == paramName && arg.Span.Contains(caretPos))
                    return true;
            }
            return false;
        }


        private void Editor_CharAdded(object sender, CharAddedEventArgs e)
        {
            if ((char)e.Char == ';')
            {
                Hide();
            }
        }




        private void Editor_KeyDown(object sender, KeyEventArgs e)
       {
            if (dropDown == null)
                return;

            if(!dropDown.Visible)
                return;

            //if (e.KeyCode == Keys.Back)
            //{
            //    // wichtig: BeginInvoke auf Editor, nicht auf DropDown
            //    Editor.BeginInvoke(new Action(async () => await HandleBackspaceAsync()));
            //}


            int pos = Editor.CurrentPosition;
            char typedChar = (char)Editor.GetCharAt(pos - 1);

            if (typedChar == ';')
            {
                Hide();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }


            switch (e.KeyCode)
            {

                case Keys.OemSemicolon:
                    // Semikolon erkannt
                    Hide();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;




                case Keys.Escape:
                    Hide();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;


                case Keys.Enter:
                case Keys.Tab:
                    CommitSelection();

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;

              

                case Keys.Up:
                    if (listBox.SelectedIndex > 0)
                        listBox.SelectedIndex--;
                    e.Handled = true;
                    break;

                case Keys.Down:
                    if (listBox.SelectedIndex < listBox.Items.Count - 1)
                        listBox.SelectedIndex++;
                    e.Handled = true;
                    break;
            }
        }

        private void CommitSelection()
        {
            string? selected = listBox.SelectedItem?.ToString().Split(':')[0].Trim() + ":";
            if (string.IsNullOrEmpty(selected))
            {
                Hide();
              
                return;
            }

            string prefix = GetPrefix();

            int pos = Editor.CurrentPosition;
            int start = pos - prefix.Length;

            Editor.DeleteRange(start, prefix.Length);
            Editor.InsertText(start, selected);
            Editor.GotoPosition(start + selected.Length);
            Hide();
        }

        private string GetPrefix()
        {
            int pos = Editor.CurrentPosition;
            int start = Editor.WordStartPosition(pos, true);
            return Editor.GetTextRange(start, pos - start);
        }

    }
}