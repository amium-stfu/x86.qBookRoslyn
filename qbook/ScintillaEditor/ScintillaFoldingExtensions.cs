using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ScintillaNET; // Scintilla5.NET 6.01
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static DevExpress.Utils.BindToTypePolicy;

internal static class ScintillaFoldingExtensions
{
    /// <summary>
    /// Visual‑Studio‑ähnliches Folding mit Pfeilen (▶/▼) für C#. Nutzt den eingebauten Lexer.
    /// </summary>
    public static void InitializeScintillaFolding(this Scintilla editor)
    {
        if (editor == null) return;

        // C# wird vom gemeinsamen C/C++/C#/Java‑Lexer abgedeckt
        editor.LexerName = "cpp"; // "cs" ist kein gültiger Lexername

        // Folding aktivieren
        editor.SetProperty("fold", "1");
        editor.SetProperty("fold.compact", "1");
        editor.SetProperty("fold.preprocessor", "1");
        editor.SetProperty("fold.braces", "1");
        editor.SetProperty("fold.cpp.syntax.based", "1");
        editor.SetProperty("fold.cpp.comment.explicit", "1");
        editor.SetProperty("fold.at.else", "1");
        editor.SetProperty("fold.at.open", "1");

        // Folding‑Margin (Pfeile wie in Visual Studio)
        const int FOLD_MARGIN = 2;
        editor.Margins[FOLD_MARGIN].Type = MarginType.Symbol;
        editor.Margins[FOLD_MARGIN].Mask = Marker.MaskFolders;
        editor.Margins[FOLD_MARGIN].Sensitive = true;
        editor.Margins[FOLD_MARGIN].Width = 18;

        // Pfeil‑Symbole statt +/−
        editor.Markers[Marker.Folder].Symbol = MarkerSymbol.Arrow;
        editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.ArrowDown;
        editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
        editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
        editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
        editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.Arrow;
        editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.ArrowDown;

        // Farben der Pfeile
        editor.Markers[Marker.Folder].SetForeColor(Color.Gray);
        editor.Markers[Marker.FolderOpen].SetForeColor(Color.Gray);

        // Automatisches Anzeigen/Klicken/Neuaufbau
        editor.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;

        // Einmal alles neu färben → Folding neu berechnen
        editor.RefreshFolding();
        editor.ApplyRoslynHeaderAndClamp();
    }


    public static void RefreshFolding(this Scintilla editor)
    {
        if (editor == null) return;
        editor.DirectMessage(ScintillaConstants.SCI_COLOURISE, IntPtr.Zero, new IntPtr(-1));
    }

    public static void ToggleFoldAtLine(this Scintilla editor, int line)
    {
        if (editor == null) return;
        if (line < 0 || line >= editor.Lines.Count) return;
        editor.DirectMessage(ScintillaConstants.SCI_TOGGLEFOLD, new IntPtr(line), IntPtr.Zero);
    }

    private const int SCI_GETFOLDLEVEL = 2223;
    private const int SCI_GETLASTCHILD = 2224;
    private const int SCI_TOGGLEFOLD = 2231;
    private const int SC_FOLDLEVELHEADERFLAG = 0x2000;

    public static void PreserveCollapsedRegionsByText(this Scintilla editor, Action foldingUpdate)
    {
        var collapsedHeaders = new HashSet<string>();

        // 1) Vorher: alle zugeklappten Regionen anhand Text + FoldSpan ermitteln
        for (int i = 0; i < editor.Lines.Count; i++)
        {
            int level = (int)editor.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(i), IntPtr.Zero);
            bool isHeader = (level & SC_FOLDLEVELHEADERFLAG) != 0;
            if (!isHeader) continue;

            // Hole letzte Zeile der Fold-Region
            int lastChild = (int)editor.DirectMessage(SCI_GETLASTCHILD, new IntPtr(i), new IntPtr(-1));
            if (lastChild <= i) continue;

            // Wenn alle Zeilen dazwischen verborgen sind → Fold ist collapsed
            bool anyVisible = false;
            for (int j = i + 1; j <= lastChild && j < editor.Lines.Count; j++)
            {
                if (editor.Lines[j].Visible)
                {
                    anyVisible = true;
                    break;
                }
            }

            if (!anyVisible)
            {
                var text = editor.Lines[i].Text.Trim();
                if (!string.IsNullOrEmpty(text))
                    collapsedHeaders.Add(text);
            }
        }

        // 2) Folding neu aufbauen
        foldingUpdate();

        // 3) Danach: matching Header-Zeilen wieder zuklappen
        for (int i = 0; i < editor.Lines.Count; i++)
        {
            int level = (int)editor.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(i), IntPtr.Zero);
            bool isHeader = (level & SC_FOLDLEVELHEADERFLAG) != 0;
            if (!isHeader) continue;

            var text = editor.Lines[i].Text.Trim();
            if (!string.IsNullOrEmpty(text) && collapsedHeaders.Contains(text))
            {
                // Toggle nur, wenn aktuell expanded
                int lastChild = (int)editor.DirectMessage(SCI_GETLASTCHILD, new IntPtr(i), new IntPtr(-1));
                bool anyVisible = false;
                for (int j = i + 1; j <= lastChild && j < editor.Lines.Count; j++)
                {
                    if (editor.Lines[j].Visible)
                    {
                        anyVisible = true;
                        break;
                    }
                }

                if (anyVisible)
                    editor.DirectMessage(SCI_TOGGLEFOLD, new IntPtr(i), IntPtr.Zero);
            }
        }
    }



}

internal static class ScintillaConstants
{
    // Ergänzung der fehlenden Konstanten
    public const int SCI_COLOURISE = 4003;     // vollständiges Re‑Colourise
    public const int SCI_TOGGLEFOLD = 2231;    // bereits bekannt, hier nur der Vollständigkeit halber
}


// --- HYBRID-FIX: VS‑Style (nur Signatur bleibt sichtbar), auch mit Cpp‑Lexer ---
// Idee: Der eingebaute Cpp‑Lexer markiert die ZEILE MIT "{" als Header.
// VS markiert stattdessen die Signaturzeile als Header. Wir „heben den Header eine Zeile hoch“.
// → Dadurch wird beim Einklappen NUR die Signaturzeile sichtbar; die "{"‑Zeile verschwindet.

internal static class ScintillaVsStyleFoldFix
{
    // Scintilla FOLD-Konstanten
    private const int SCI_GETFOLDLEVEL = 2223;
    private const int SCI_SETFOLDLEVEL = 2222;
    private const int SC_FOLDLEVELNUMBERMASK = 0x0FFF;
    private const int SC_FOLDLEVELWHITEFLAG = 0x1000;
    private const int SC_FOLDLEVELHEADERFLAG = 0x2000;

    public static void ApplyVsStyleHeaderShift(this Scintilla editor)
    {
        if (editor == null || editor.Lines.Count == 0) return;

        // 1) Erst syntaxfärben/folding vom Lexer berechnen lassen
        editor.DirectMessage(ScintillaConstants.SCI_COLOURISE, IntPtr.Zero, new IntPtr(-1));

        // 2) Alle Header ermitteln, die auf einer reinen "{"‑Zeile liegen,
        //    und den Header auf die vorherige NICHT‑LEERE Zeile verschieben.
        for (int i = 1; i < editor.Lines.Count; i++)
        {
            var text = editor.Lines[i].Text?.Trim();
            if (string.IsNullOrEmpty(text)) continue;

            // Nur Allman‑Style‑Klammern ("{" allein auf der Zeile)
            if (text != "{") continue;

            int fold = (int)editor.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(i), IntPtr.Zero);
            bool isHeader = (fold & SC_FOLDLEVELHEADERFLAG) != 0;
            if (!isHeader) continue; // nur dann verschieben, wenn Lexer die "{"‑Zeile als Header markiert hat

            // Vorherige NICHT‑LEERE Zeile finden (typisch: Signaturzeile "public Foo()")
            int prev = i - 1;
            while (prev >= 0 && string.IsNullOrWhiteSpace(editor.Lines[prev].Text))
                prev--;
            if (prev < 0) continue;

            // Ebene beibehalten, aber Header‑Flag nach oben verlagern
            int levelNum = fold & SC_FOLDLEVELNUMBERMASK;

            // Vorherige Level beibehalten, nur Header‑Flag setzen
            int prevFold = (int)editor.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(prev), IntPtr.Zero);
            int prevLevelNum = prevFold & SC_FOLDLEVELNUMBERMASK;
            if (prevLevelNum != levelNum)
            {
                // Ebene angleichen, damit die Hierarchie stimmt
                prevFold = (prevFold & ~SC_FOLDLEVELNUMBERMASK) | levelNum;
            }
            prevFold = (prevFold | SC_FOLDLEVELHEADERFLAG) & ~SC_FOLDLEVELWHITEFLAG;
            editor.DirectMessage(SCI_SETFOLDLEVEL, new IntPtr(prev), new IntPtr(prevFold));

            // Auf der "{"‑Zeile das Header‑Flag entfernen und auf Child‑Level setzen
            int newFoldForBrace = (levelNum + 1) | (fold & SC_FOLDLEVELWHITEFLAG);
            editor.DirectMessage(SCI_SETFOLDLEVEL, new IntPtr(i), new IntPtr(newFoldForBrace));
        }
    }
}

// Verwendung nach dem Laden/Setzen des Textes:
// editor.InitializeScintillaFolding();
// editor.ApplyVsStyleHeaderShift(); // -> jetzt klappt eine Methode zu: nur die Signatur bleibt, "{" verschwindet

// --- HYBRID-FIX (final): Roslyn klemmt Regionen exakt bis zur '}' und hebt Header auf die Signaturlinie ---
internal static class ScintillaRoslynOverlay
{
    private const int SCI_GETFOLDLEVEL = 2223;
    private const int SCI_SETFOLDLEVEL = 2222;
    private const int SC_FOLDLEVELNUMBERMASK = 0x0FFF;
    private const int SC_FOLDLEVELWHITEFLAG = 0x1000;
    private const int SC_FOLDLEVELHEADERFLAG = 0x2000;

    public static void ApplyRoslynHeaderAndClamp(this Scintilla editor)
    {
        if (editor == null || string.IsNullOrWhiteSpace(editor.Text)) return;

        var tree = CSharpSyntaxTree.ParseText(editor.Text);
        var root = tree.GetRoot();

        foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            PromoteAndClamp(editor,
                headerPos: cls.SpanStart,
                openPos: cls.OpenBraceToken.SpanStart,
                closePos: cls.CloseBraceToken.SpanStart);

            foreach (var m in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                if (m.Body == null) continue;
                PromoteAndClamp(editor,
                    headerPos: m.SpanStart,
                    openPos: m.Body.OpenBraceToken.SpanStart,
                    closePos: m.Body.CloseBraceToken.SpanStart);
            }
        }

        foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                 .Where(x => x.Parent is not ClassDeclarationSyntax))
        {
            if (m.Body == null) continue;
            PromoteAndClamp(editor,
                headerPos: m.SpanStart,
                openPos: m.Body.OpenBraceToken.SpanStart,
                closePos: m.Body.CloseBraceToken.SpanStart);
        }
    }

    private static void PromoteAndClamp(Scintilla ed, int headerPos, int openPos, int closePos)
    {
        int headerLine = ed.LineFromPosition(headerPos);
        int openLine = ed.LineFromPosition(openPos);
        int closeLine = ed.LineFromPosition(closePos);
        if (headerLine < 0 || openLine < 0 || closeLine < 0) return;
        if (closeLine <= headerLine) return;

        int openFold = (int)ed.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(openLine), IntPtr.Zero);
        int levelNum = openFold & SC_FOLDLEVELNUMBERMASK;

        int hdrFold = (int)ed.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(headerLine), IntPtr.Zero);
        hdrFold = (hdrFold & ~SC_FOLDLEVELNUMBERMASK) | levelNum;
        hdrFold = (hdrFold | SC_FOLDLEVELHEADERFLAG) & ~SC_FOLDLEVELWHITEFLAG;
        ed.DirectMessage(SCI_SETFOLDLEVEL, new IntPtr(headerLine), new IntPtr(hdrFold));

        int braceFold = (levelNum + 1) | (openFold & SC_FOLDLEVELWHITEFLAG);
        ed.DirectMessage(SCI_SETFOLDLEVEL, new IntPtr(openLine), new IntPtr(braceFold));

        for (int i = headerLine + 1; i <= closeLine && i < ed.Lines.Count; i++)
        {
            int f = (int)ed.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(i), IntPtr.Zero);
            int nf = (f & ~SC_FOLDLEVELNUMBERMASK) | (levelNum + 1);
            nf &= ~SC_FOLDLEVELHEADERFLAG;
            ed.DirectMessage(SCI_SETFOLDLEVEL, new IntPtr(i), new IntPtr(nf));
        }

        int after = closeLine + 1;
        if (after < ed.Lines.Count)
        {
            int af = (int)ed.DirectMessage(SCI_GETFOLDLEVEL, new IntPtr(after), IntPtr.Zero);
            af = (af & ~SC_FOLDLEVELNUMBERMASK) | levelNum;
            af &= ~SC_FOLDLEVELHEADERFLAG;
            ed.DirectMessage(SCI_SETFOLDLEVEL, new IntPtr(after), new IntPtr(af));
        }
    }
}

