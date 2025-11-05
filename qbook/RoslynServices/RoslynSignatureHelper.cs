using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using qbook.ScintillaEditor;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace qbook.CodeEditor
{
    public static class RosylnSignatureHelper
    {

        private static RoslynService _roslyn;
        public static CodeNode ActiveNode;
        private static DocumentEditor Editor => ActiveNode.Editor;

        static FormPopup popup;
        public static Font ListFont;

        public static Font EditorFont => Editor.GetFont();

        public static void Init(RoslynService roslyn)
        {
            _roslyn = roslyn;
            popup = new FormPopup();
            
            popup.EditorFont = new Font("Consolas", 10);

            popup.Config(400, 0, 0, 50, 30, 20);

            if (Theme.IsDark)
                popup.ListView.ApplyDarkTheme();
            else
                popup.ListView.ApplyLightTheme();

            
            popup.ListView.ItemSelected += item => CommitSelection();
        }

        public static bool IsVisible => popup.Visible;

        public static void HidePopup() => popup.Hide();

        public static void Next()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectNext();
            }
        }

        public static void Previous()
        {
            if (popup.Visible)
            {
                popup.ListView.SelectPrevious();
            }
        }

        public static void Commit()
        {
            if (popup.Visible)
            {
                var text = popup.ListView.SelectedText;
                if (!string.IsNullOrEmpty(text))
                {
                    CommitSelection();
                }
                else
                {
                    CommitSelection();
                }
            }
        }

        public static async Task<bool> TrySignatureHelpAsync(bool updateOnly = false)
        {
            try
            {
                if (!ActiveNode.Active)
                {
                    if (!updateOnly) Editor.CallTipCancel();
                    return false;
                }

                int pos = Math.Max(0, Editor.CurrentPosition);
                var doc = ActiveNode.RoslynDoc;
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

        private static int GetArgumentIndex(ArgumentListSyntax list, int caretPos)
        {
            int idx = 0;
            foreach (var a in list.Arguments)
            {
                if (caretPos > a.FullSpan.Start) idx++; else break;
            }
            if (idx > 0 && idx == list.Arguments.Count && caretPos <= list.CloseParenToken.FullSpan.Start)
                idx--;
            return Math.Max(0, idx);
        }

        private static async Task<bool> ShowSignatureForInvocationAsync(InvocationExpressionSyntax inv, RoslynDocument doc, int caretPos, bool updateOnly)
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

        private static async Task<bool> ShowSignatureForCreationAsync(ObjectCreationExpressionSyntax obj, RoslynDocument doc, int caretPos, bool updateOnly)
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

        public static bool Visible => popup?.Visible ?? false;
        public static void Hide() => popup?.Hide();

        private static void ShowParameterList(IMethodSymbol constructor, ArgumentListSyntax argumentList, int caretPos)
        {
            var items = new List<CompletionItem>();
            var usedParams = argumentList.Arguments
                .Select(arg => arg.NameColon?.Name.Identifier.ValueText)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet();

            foreach (var param in constructor.Parameters)
            {
                if (usedParams.Contains(param.Name)) continue;

                string defaultValue = param.HasExplicitDefaultValue ? param.ExplicitDefaultValue?.ToString() ?? "null" : "????";
                string type = param.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                string value = GetArgumentValue(param.Name, argumentList) ?? defaultValue;
                string display = $"{param.Name}: ({type})";

                items.Add(new CompletionItem
                {
                    Type = type,
                    Text = param.Name + ":",
                    Value = value.Replace("-","????"),
                    Description = type
                });
            }

            popup.ListView.SetItems(items);

            popup.EditorFont = EditorFont;
            popup.Height = popup.ListView.GetAutoHeightForItems(maxVisibleItems: 10);

            if (Theme.IsDark)
                popup.ListView.ApplyDarkTheme();
            else
                popup.ListView.ApplyLightTheme();

            int pos = Editor.CurrentPosition;
            int x = Editor.PointXFromPosition(pos);
            int y = Editor.PointYFromPosition(pos) + 18;
            Point screenPoint = Editor.PointToScreen(new Point(x, y));
            popup.Location = screenPoint;
            popup.Show();
            Editor.Focus();
            Editor.GotoPosition(pos);
        }

        private static string? GetArgumentValue(string paramName, ArgumentListSyntax args)
        {
            foreach (var arg in args.Arguments)
            {
                if (arg.NameColon?.Name.Identifier.ValueText == paramName)
                    return arg.Expression.ToString();
            }
            return null;
        }

        private static void CommitSelection()
        {
            string? selected = popup.ListView.SelectedItem?.Text;
            if (string.IsNullOrEmpty(selected)) { Hide(); return; }

            string prefix = GetPrefix();
            int pos = Editor.CurrentPosition;
            int start = pos - prefix.Length;
            Editor.DeleteRange(start, prefix.Length);
            Editor.InsertText(start, selected);
            Editor.GotoPosition(start + selected.Length);
            Hide();
        }

        private static string GetPrefix()
        {
            int pos = Editor.CurrentPosition;
            int start = Editor.WordStartPosition(pos, true);
            return Editor.GetTextRange(start, pos - start);
        }

        public static async void Editor_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (ActiveNode.RoslynDoc == null)
                return;

            char c = (char)e.Char;
            if (c == '(' || c == ',')
                await TrySignatureHelpAsync();

            if (c == ';')
                Hide();
        }

        public static void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (popup == null || !popup.Visible)
                return;

            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Tab:
                    CommitSelection();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Escape:
                case Keys.OemSemicolon:
                    Hide();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Up:
                    popup.ListView.SelectPrevious();
                    e.Handled = true;
                    break;

                case Keys.Down:
                    popup.ListView.SelectNext();
                    e.Handled = true;
                    break;
            }
        }

    }
}