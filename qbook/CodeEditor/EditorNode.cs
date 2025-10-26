using DevExpress.Utils.DPI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static qbook.CodeEditor.FoldingControl;
using RoslynDocument = Microsoft.CodeAnalysis.Document;


namespace qbook.CodeEditor
{
    public class Undo
    {
        public string Text { get; set; }
        public int Position { get; set; }
        public int FirstLine { get; set; }

      

        public Undo(string text, int position, int firstLine)
        {
            Text = text;
            Position = position;
            FirstLine = firstLine;
        }
    }

    public class EditorNode : TreeNode
    {
        public int NodeID { get; set; }
        public oPage Page { get; set; }
        public bool Active { get; set; }
        public string Code { get; set; } = "";
        public int LastCursorPos { get; set; } = 0;
        public int LastFirstLine { get; set; } = 0;

        public List<FoldingStateByContent> Foldings;
        public EditorTab Tab { get; set; }

        private List<Undo> UndoBuffer = new();
        public void AddUndo(string text, int pos, int fistLine)
        {
        //    Debug.WriteLine("Add Undo " + UndoBuffer.Count);
            if (UndoBuffer.Count > 1000) 
                UndoBuffer.Remove(UndoBuffer.First());

            Undo add = new Undo(text, pos, fistLine);
            UndoBuffer.Add(add);
        }

        public bool hasUndo => UndoBuffer.Count > 0;

        public Undo GetUndo()
        {
            try
            {
                if (UndoBuffer.Count == 0) return null;
                int c = UndoBuffer.Count() - 1;
                Undo result = UndoBuffer[c];
                UndoBuffer.Remove(UndoBuffer.Last());
           //     Debug.WriteLine("New Buffer " + UndoBuffer.Count);
                return result;
            }
            catch
            {
                return null;
            }
        }
        public string SubcodeKey { get; set; }
        public string FilePath { get; set; }

        RoslynServices Roslyn;
        public RoslynDocument RoslynDoc { get; set; }
        public NodeType Type { get; set; }

        public EditorNode PageNode;


        public enum NodeType
        {
            Page,
            SubCode,
            Book
        }

        public EditorNode(oPage page, RoslynServices roslyn,string filePath, NodeType type, string subcodeKey = null, EditorNode pageNode = null) : base($"{subcodeKey}_{page.FullName}")
        {
            Page = page;
            FilePath = filePath;
            Roslyn = roslyn;
            Type = type;
            SubcodeKey = subcodeKey;
            if (Type == NodeType.Page)
            {
                this.Text = page.FullName;
                this.Name = page.FullName;
                this.ImageIndex = 2;
            }
            else
            {
                this.ImageIndex = 3;
                this.Text = subcodeKey;
                this.Name = $"{page.FullName}_{subcodeKey}";
                this.PageNode = pageNode;
            }
            this.ToolTipText = FilePath;
            // Initial load of code
           
        }
        public EditorNode(string name) : base(name)
        {
            Type = NodeType.Book;
        }
        public async void Save()
        {
            string code;
            if (Active)
            code = await GetCodeAsync("EditorNode.Save");
            else
            code = Code;

            if (Type == NodeType.SubCode)
            {
                try
                {
                    //Debug.WriteLine("============ Subcode ============");
                    //Debug.WriteLine(code);
                    //Debug.WriteLine("============ SaveCode ============");

                    int namespaceIndex = code.IndexOf("namespace");
                    if (namespaceIndex == -1)
                        Page.CsCodeExtra[SubcodeKey] = code; // Kein Namespace vorhanden

                    int firstBrace = code.IndexOf('{', namespaceIndex);

                    int braceCount = 1;
                    int i = firstBrace + 1;


                    while (i < code.Length && braceCount > 0)
                    {
                        if (code[i] == '{') braceCount++;
                        else if (code[i] == '}') braceCount--;
                        i++;
                    }

                    if (braceCount != 0)
                    {
                        throw new Exception("Klammern nicht ausgeglichen!");
                    }
                    string innerContent = code.Substring(firstBrace + 1, i - firstBrace - 2);
                    Page.CsCodeExtra[SubcodeKey] = innerContent.Trim();
                    //Debug.WriteLine(Page.CsCodeExtra[SubcodeKey]);
                    //Debug.WriteLine("========================");


                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fehler beim Speichern des Codes: " + ex.Message);
                    return;
                }
            }
            if (Type == NodeType.Page)
            {
                Debug.WriteLine("Save Page " + Name);
                try
                {
                    // Schritt 1: Extrahiere die using-Direktiven
                    Match usingMatch = Regex.Match(code, @"^(using\s.+?;\s*)+", RegexOptions.Singleline);
                    string usingDirectives = usingMatch.Success ? usingMatch.Value.TrimEnd('\r', '\n') : "";

                    // Schritt 2: Entferne den namespace-Block
                    string codeWithoutNamespace = Regex.Replace(code, @"namespace\s+\w+\s*", "", RegexOptions.Multiline);

                    // Schritt 3: Entferne die erste öffnende Klammer
                    int firstBrace = codeWithoutNamespace.IndexOf('{');
                    if (firstBrace >= 0)
                        codeWithoutNamespace = codeWithoutNamespace.Substring(firstBrace + 1);

                    // Schritt 4: Entferne die letzte schließende Klammer
                    int lastBrace = codeWithoutNamespace.LastIndexOf('}');
                    if (lastBrace >= 0)
                        codeWithoutNamespace = codeWithoutNamespace.Substring(0, lastBrace);

                    // Schritt 5: Entferne führende Zeilenumbrüche
                    codeWithoutNamespace = codeWithoutNamespace.TrimStart('\r', '\n');

                    // Schritt 6: Kombiniere das Ergebnis mit genau einem Zeilenumbruch
                    Page.CsCode = usingDirectives + Environment.NewLine + codeWithoutNamespace.TrimEnd();

                    foreach(EditorNode subNode in this.Nodes)
                    {
                     //   Debug.WriteLine("update sub using from " + subNode.PageNode.Text);
                        await subNode.UpdateSubCodeUsingsAsync();
                       // Debug.WriteLine("update");

                       List<string> usings = await subNode.GetUsings();
                        foreach (string u in usings) Debug.WriteLine(u);
                    }
                }
                catch
                {
                    Debug.WriteLine("Faild save Pagecode " + Page.FullName);
                }
            }  
        }
        public async Task<List<string>> GetUsings()
        {
            var usings = new List<string>();

            if (RoslynDoc == null) return usings;

            var text = await RoslynDoc.GetTextAsync();
            string code = Code;

            var usingMatch = Regex.Match(code, @"^(using\s.+?;\s*)+", RegexOptions.Singleline);
            if (usingMatch.Success)
            {
                var usingLines = usingMatch.Value
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line));

                usings.AddRange(usingLines);
            }

            foreach (var u in usings) Debug.WriteLine(u);
            return usings;
        }
        public async Task<(int startLine, int endLine)> GetUsingBlockLineRange()

        {
              string code;
            if (Active)
                code = await GetCodeAsync("EditorNode.GetUsingBlockLineRange");
            else
            code = Code;

            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int start = -1;
            int end = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("using "))
                {
                    if (start == -1)
                        start = i;

                    end = i;
                }
                else if (start != -1 && !string.IsNullOrWhiteSpace(line))
                {
                    // Sobald eine nicht-leere Zeile kommt, die kein using ist, beenden
                    break;
                }
            }
            return (start, end);
        }

        public async Task<(int startLine, int endLine)> GetIncludeBlockLineRange()
        {
            string code;
            if (Active)
                code = await GetCodeAsync("EditorNode.GetIncludeBlockLineRange");
            else
                code = Code;

            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            int start = -1;
            int end = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line == "//<IncludeStart>")
                {
                    start = i;
                }
                else if (line == "//<IncludeEnd>")
                {
                    end = i;
                    break; // Nach dem Endmarker kann abgebrochen werden
                }
            }

            return (start, end);
        }

        public async Task UpdateSubCodeUsingsAsync()
        {
            if (Page == null || PageNode == null || RoslynDoc == null) return;

            var usings = await PageNode.GetUsings();
            var text = await RoslynDoc.GetTextAsync();
            var code =text.ToString();

            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            // Finde den using-Block
            int start = -1, end = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("using "))
                {
                    if (start == -1) start = i;
                    end = i;
                }
                else if (start != -1 && !string.IsNullOrWhiteSpace(line))
                {
                    break;
                }
            }

            //Debug.WriteLine(code);

            //Debug.WriteLine($"Using {start} / {end} {code.Length}");

            if (start == -1 || end == -1) return;

            // Ersetze die alten using-Zeilen durch die neuen
            var updatedLines = new List<string>();
            updatedLines.AddRange(lines.Take(start));
            updatedLines.AddRange(usings.Select(u => u.Trim()));
            updatedLines.AddRange(lines.Skip(end + 1));

            var updatedCode = string.Join("\r\n", updatedLines);

            // Aktualisiere das Roslyn-Dokument
            var workspace = RoslynDoc.Project.Solution.Workspace;
            var docId = RoslynDoc.Id;
            var doc = workspace.CurrentSolution.GetDocument(docId);

           
            var newText = SourceText.From(updatedCode);

         
                var updatedDoc = doc.WithText(newText);
                if (workspace.TryApplyChanges(updatedDoc.Project.Solution))
                    RoslynDoc = workspace.CurrentSolution.GetDocument(docId);
            

        }
        public async Task<string> GetCodeAsync(string sender)
        {
            if (Type == NodeType.Page)
            {
                var text = await RoslynDoc.GetTextAsync();
                return text.ToString();
            }
            else if (Type == NodeType.SubCode)
            {
                await UpdateSubCodeUsingsAsync();
                if (Active)
                {
                    var text = await RoslynDoc.GetTextAsync();
                    return text.ToString();
                }
                else
                {
                    return Code;
                }     
            }
            return string.Empty;
        }

        //Qbook

        public async Task<string> UpdatedCodeForQbook(string sender, string code)
        {
            Code = code;

            if (Type == NodeType.SubCode && Active)
            {
                Code = await UpdateSubCodeStringAsync(code);
            }
            return Code;
        }
        public async Task<string> UpdateSubCodeStringAsync(string code)
        {
            if (Page == null || PageNode == null || RoslynDoc == null) return string.Empty;

            var usings = await PageNode.GetUsings();

        

            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            // Finde den using-Block
            int start = -1, end = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("using "))
                {
                    if (start == -1) start = i;
                    end = i;
                }
                else if (start != -1 && !string.IsNullOrWhiteSpace(line))
                {
                    break;
                }
            }

            //Debug.WriteLine(code);

            //Debug.WriteLine($"Using {start} / {end} {code.Length}");

            if (start == -1 || end == -1) return string.Empty;

            // Ersetze die alten using-Zeilen durch die neuen
            var updatedLines = new List<string>();
            updatedLines.AddRange(lines.Take(start));
            updatedLines.AddRange(usings.Select(u => u.Trim()));
            updatedLines.AddRange(lines.Skip(end + 1));

            var updatedCode = string.Join("\r\n", updatedLines);

      
            return updatedCode;

        }


    }
}
