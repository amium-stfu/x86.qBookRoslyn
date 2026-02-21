
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace qbookCode.Roslyn
{
    public interface IDocumentationService
    {
        /// <summary>Gibt die aufbereitete Dokumentation (Summary, Params, Returns, Remarks) für ein Symbol zurück – oder null.</summary>
        MethodDocumentation? Get(ISymbol symbol);

        /// <summary>Optional: Doku an einer Quelltextposition (Fallback über Syntax-Trivia, wenn kein ISymbol vorliegt).</summary>
        Task<MethodDocumentation?> GetAtPositionAsync(Document document, int position, CancellationToken ct = default);
    }

    public sealed class MethodDocumentation
    {
        public string Summary { get; init; } = "";
        public IReadOnlyDictionary<string, string> Parameters { get; init; }
            = new Dictionary<string, string>();
        public string Returns { get; init; } = "";
        public string Remarks { get; init; } = "";
        public string? DocumentationCommentId { get; init; }     // z. B. M:Namespace.Type.Method(System.Int32)
        public string? SymbolDisplay { get; init; }              // z. B. Namespace.Type.Method(int)
        public string? ContainingType { get; init; }             // z. B. Namespace.Type
        public string? ContainingAssembly { get; init; }         // z. B. MyLib
        public string? SourceFilePath { get; set; }             // Pfad, falls verfügbar (eigener Code)
        public int? SourceLineNumber { get; set; }           // Zeile, falls verfügbar
    }


    public sealed class DocumentationService : IDocumentationService
    {
        // Kleiner LRU-Cache nach DocCommentId (z.B. "M:Namespace.Type.Method(System.Int32)")
        private readonly LruCache<string, MethodDocumentation> _cache;
        private readonly CultureInfo? _preferredCulture;

        public DocumentationService(int capacity = 4000, CultureInfo? preferredCulture = null)
        {
            _cache = new LruCache<string, MethodDocumentation>(capacity);
            _preferredCulture = preferredCulture;
        }

        public MethodDocumentation? Get(ISymbol symbol)
        {
            if (symbol is null) return null;

            var id = symbol.GetDocumentationCommentId();

            if (!string.IsNullOrWhiteSpace(id) && _cache.TryGetValue(id, out var cached))
                return cached;

            var xml = symbol.GetDocumentationCommentXml(_preferredCulture, expandIncludes: true, cancellationToken: default);
            var doc = Parse(xml);

            if (IsEmpty(doc))
                doc = ResolveInheritDoc(symbol); // dein InheritDoc-Fallback

            if (!IsEmpty(doc))
            {
                // 🔽 NEU: Referenz-Metadaten anreichern
                doc = EnrichWithSymbolInfo(doc!, symbol);

                if (!string.IsNullOrWhiteSpace(id))
                    _cache.Add(id!, doc!);
            }

            return doc;
        }

        private static MethodDocumentation EnrichWithSymbolInfo(MethodDocumentation doc, ISymbol symbol)
        {
            // 1) Grunddaten
            var enriched = new MethodDocumentation
            {
                Summary = doc.Summary,
                Remarks = doc.Remarks,
                Returns = doc.Returns,
                Parameters = doc.Parameters,
                DocumentationCommentId = symbol.GetDocumentationCommentId(),
                SymbolDisplay = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
                                        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                ContainingType = symbol.ContainingType?.ToDisplayString(),
                ContainingAssembly = symbol.ContainingAssembly?.Name
            };

            // 2) Quelle (nur wenn verfügbar – i. d. R. für eigenen Code)
            //    Achtung: Bei Metadaten-Symbolen (Referenz-DLLs) ist die Liste leer.
            var decl = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (decl != null)
            {
                var syntax = decl.GetSyntax();
                var tree = syntax.SyntaxTree;
                var span = syntax.GetLocation().GetLineSpan();
                enriched.SourceFilePath = tree?.FilePath;
                enriched.SourceLineNumber = span.StartLinePosition.Line + 1; // 1-basiert
            }

            return enriched;
        }

        public async Task<MethodDocumentation?> GetAtPositionAsync(Document document, int position, CancellationToken ct = default)
        {
            if (document is null) return null;

            var tree = await document.GetSyntaxTreeAsync(ct).ConfigureAwait(false);
            if (tree is null) return null;

            var root = await tree.GetRootAsync(ct).ConfigureAwait(false);
            var token = root.FindToken(position);
            var node = token.Parent;
            if (node is null) return null;

            var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
            if (compilation is null) return null;

            var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);

            // 1) Erst versuchen, ein ISymbol zu ermitteln
            var symbol =
                model.GetDeclaredSymbol(node, ct) ??
                model.GetSymbolInfo(node, ct).Symbol ??
                model.GetEnclosingSymbol(position, ct);

            if (symbol != null)
                return Get(symbol);

            // 2) Fallback: XML-Doc via LeadingTrivia direkt an der Deklaration parsen
            // (z.B. wenn Code fehlerhaft ist und kein Symbol gebunden werden kann)
            var trivia = node
                .GetLeadingTrivia()
                .Select(t => t.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (trivia == null) return null;

            var raw = trivia.ToFullString();
            return Parse(raw);
        }

        // --- Parser ---

        private static MethodDocumentation? Parse(string? xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;

            try
            {
                // 1) In ein Root-Element packen, damit XDocument/XElement immer gültig ist.
                var root = XElement.Parse("<root>" + xml + "</root>");

                // 2) Falls es ein <member> Wrapper ist (externe XML-Doku-Dateien):
                //    -> in diesem Fall liegen summary/param/returns als direkte Kindelemente von <member>.
                var member = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "member");
                var container = member ?? root;

                string ExtractFirst(string localName)
                {
                    var el = container
                        .Descendants()                              // immer über Descendants gehen
                        .FirstOrDefault(e => e.Name.LocalName == localName);
                    return el == null ? "" : ToDisplayText(el).Trim();
                }

                // 3) Summary/Remarks/Returns extrahieren
                var summary = ExtractFirst("summary");
                var remarks = ExtractFirst("remarks");
                var returns = ExtractFirst("returns");

                // 4) Parameter (alle)
                var parameters = new Dictionary<string, string>();
                foreach (var p in container.Descendants().Where(e => e.Name.LocalName == "param"))
                {
                    var name = p.Attribute("name")?.Value;
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var text = ToDisplayText(p).Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        parameters[name!] = text;
                }

                if (string.IsNullOrWhiteSpace(summary)
                    && string.IsNullOrWhiteSpace(remarks)
                    && string.IsNullOrWhiteSpace(returns)
                    && parameters.Count == 0)
                    return null;

                return new MethodDocumentation
                {
                    Summary = summary,
                    Remarks = remarks,
                    Returns = returns,
                    Parameters = parameters
                };
            }
            catch
            {
                return null;
            }
        }

        private static string ToDisplayText(XElement element)
        {
            var sb = new System.Text.StringBuilder();
            BuildTextRecursive(element, sb, inCodeBlock: false);

            // Whitespace normalisieren (keine langen Ketten, aber bewahre Absatzumbrüche)
            var text = sb.ToString();
            text = CollapseSpaces(text);
            text = CollapseBlankLines(text);
            return text.Trim();
        }

        private static void BuildTextRecursive(XNode node, System.Text.StringBuilder sb, bool inCodeBlock)
        {
            switch (node)
            {
                case XText t:
                    sb.Append(t.Value);
                    break;

                case XElement el:
                    var name = el.Name.LocalName;

                    if (name.Equals("para", StringComparison.OrdinalIgnoreCase))
                    {
                        // Absatz: davor/nachher Umbruch
                        EnsureNewLine(sb);
                        foreach (var child in el.Nodes())
                            BuildTextRecursive(child, sb, inCodeBlock);
                        EnsureNewLine(sb);
                    }
                    else if (name.Equals("see", StringComparison.OrdinalIgnoreCase))
                    {
                        var cref = el.Attribute("cref")?.Value;
                        if (!string.IsNullOrWhiteSpace(cref))
                            sb.Append(GetShortNameFromCref(cref));
                        else
                            sb.Append(el.Attribute("langword")?.Value ?? ""); // manchmal wird langword verwendet
                    }
                    else if (name.Equals("seealso", StringComparison.OrdinalIgnoreCase))
                    {
                        var cref = el.Attribute("cref")?.Value;
                        if (!string.IsNullOrWhiteSpace(cref))
                            sb.Append(GetShortNameFromCref(cref));
                    }
                    else if (name.Equals("paramref", StringComparison.OrdinalIgnoreCase) ||
                             name.Equals("typeparamref", StringComparison.OrdinalIgnoreCase))
                    {
                        var n = el.Attribute("name")?.Value ?? "";
                        if (!string.IsNullOrWhiteSpace(n))
                            sb.Append('<').Append(n).Append('>');
                    }
                    else if (name.Equals("c", StringComparison.OrdinalIgnoreCase))
                    {
                        // Inline-Code
                        sb.Append('`');
                        foreach (var child in el.Nodes())
                            BuildTextRecursive(child, sb, inCodeBlock: true);
                        sb.Append('`');
                    }
                    else if (name.Equals("code", StringComparison.OrdinalIgnoreCase))
                    {
                        // Codeblock: eigene Zeilen
                        EnsureNewLine(sb);
                        sb.Append("```");
                        EnsureNewLine(sb);
                        foreach (var child in el.Nodes())
                            BuildTextRecursive(child, sb, inCodeBlock: true);
                        EnsureNewLine(sb);
                        sb.Append("```");
                        EnsureNewLine(sb);
                    }
                    else
                    {
                        // Generische Behandlung: Kinder rekursiv verarbeiten
                        foreach (var child in el.Nodes())
                            BuildTextRecursive(child, sb, inCodeBlock);
                    }
                    break;
            }
        }

        private static void EnsureNewLine(System.Text.StringBuilder sb)
        {
            if (sb.Length == 0) return;
            if (sb[^1] != '\n') sb.Append('\n');
        }

        private static string CollapseSpaces(string s)
        {
            // Reduziert Mehrfach-Leerzeichen, aber lässt \n intakt (Absätze).
            var lines = s.Replace("\r", "").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                lines[i] = string.Join(' ', parts);
            }
            return string.Join('\n', lines);
        }

        private static string CollapseBlankLines(string s)
        {
            // Mehrere Leerzeilen -> eine
            var src = s.Replace("\r", "");
            var lines = src.Split('\n');
            var list = new List<string>(lines.Length);
            bool lastEmpty = false;
            foreach (var ln in lines)
            {
                var isEmpty = string.IsNullOrWhiteSpace(ln);
                if (isEmpty && lastEmpty) continue;
                list.Add(ln);
                lastEmpty = isEmpty;
            }
            return string.Join('\n', list);
        }

        private static void ReplaceSeeCrefsInElement(XElement element)
        {
            foreach (var see in element.Descendants().Where(x => x.Name == "see").ToList())
            {
                var cref = see.Attribute("cref")?.Value;
                var text = GetShortNameFromCref(cref ?? "");
                see.ReplaceWith(new XText(text));
            }
        }

        private static string ReplaceSeeCrefs(string text, XElement root)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            // Sehr einfache Heuristik: wir ersetzen im Original-XML die <see/>-Tags
            // und ziehen dann den Plaintext erneut – ausreichend für Tooltips.
            try
            {
                var frag = new XElement("f", root.Nodes());
                foreach (var see in frag.Descendants("see"))
                {
                    var cref = see.Attribute("cref")?.Value;
                    if (!string.IsNullOrWhiteSpace(cref))
                        see.ReplaceWith(new XText(GetShortNameFromCref(cref)));
                }
                var plain = NormalizeWhitespace(frag.Value);
                text = string.IsNullOrWhiteSpace(plain) ? text : plain;
                Debug.WriteLine(text);
                return string.IsNullOrWhiteSpace(plain) ? text : plain;
            }
            catch
            {

                Debug.WriteLine(text);
                return text;
            }
        }

        private static string GetShortNameFromCref(string cref)
        {
            // "M:Namespace.Type.Method(System.Int32)" → "Method(...)"
            // "T:Namespace.Type" → "Type"
            if (string.IsNullOrWhiteSpace(cref)) return "";

            var noPrefix = cref.IndexOf(':') >= 0 ? cref[(cref.IndexOf(':') + 1)..] : cref;
            var lastDot = noPrefix.LastIndexOf('.');
            var tail = lastDot >= 0 ? noPrefix[(lastDot + 1)..] : noPrefix;

            // Parameterliste kürzen
            var paren = tail.IndexOf('(');
            if (paren > 0) tail = tail[..paren] + "(...)";
            return tail;
        }

        private static string NormalizeWhitespace(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var parts = text
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Replace('\t', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts).Trim();
        }

        private static bool IsEmpty(MethodDocumentation? doc)
            => doc == null
               || (string.IsNullOrWhiteSpace(doc.Summary)
                   && string.IsNullOrWhiteSpace(doc.Remarks)
                   && (doc.Parameters?.Count ?? 0) == 0
                   && string.IsNullOrWhiteSpace(doc.Returns));

        // --- <inheritdoc/> Support (einfach) ---

        private MethodDocumentation? ResolveInheritDoc(ISymbol symbol)
        {
            // Sehr pragmatisch: wir schauen bei Overrides und Interface-Implementierungen nach oben
            var baseSymbols = EnumerateBaseSymbols(symbol);
            foreach (var baseSym in baseSymbols)
            {
                var xml = baseSym.GetDocumentationCommentXml(_preferredCulture, expandIncludes: true, cancellationToken: default);
                var parsed = Parse(xml);
                if (!IsEmpty(parsed))
                    return parsed;
            }
            return null;
        }

        private static IEnumerable<ISymbol> EnumerateBaseSymbols(ISymbol symbol)
        {
            switch (symbol)
            {
                case IMethodSymbol m:
                    // Overrides
                    if (m.OverriddenMethod != null) yield return m.OverriddenMethod;
                    // Interface-Methoden
                    foreach (var i in m.ContainingType.AllInterfaces)
                        foreach (var im in i.GetMembers(m.Name).OfType<IMethodSymbol>())
                            if (m.ContainingType.FindImplementationForInterfaceMember(im) is IMethodSymbol impl && SymbolEqualityComparer.IncludeNullability.Equals(impl, m))
                                yield return im;
                    break;

                case IPropertySymbol p:
                    if (p.OverriddenProperty != null) yield return p.OverriddenProperty;
                    foreach (var i in p.ContainingType.AllInterfaces)
                        foreach (var ip in i.GetMembers(p.Name).OfType<IPropertySymbol>())
                            if (p.ContainingType.FindImplementationForInterfaceMember(ip) is IPropertySymbol impl && SymbolEqualityComparer.IncludeNullability.Equals(impl, p))
                                yield return ip;
                    break;

                case IEventSymbol e:
                    if (e.OverriddenEvent != null) yield return e.OverriddenEvent;
                    foreach (var i in e.ContainingType.AllInterfaces)
                        foreach (var ie in i.GetMembers(e.Name).OfType<IEventSymbol>())
                            if (e.ContainingType.FindImplementationForInterfaceMember(ie) is IEventSymbol impl && SymbolEqualityComparer.IncludeNullability.Equals(impl, e))
                                yield return ie;
                    break;

                case INamedTypeSymbol t:
                    // Basistyp
                    var baseType = t.BaseType;
                    while (baseType != null)
                    {
                        yield return baseType;
                        baseType = baseType.BaseType;
                    }
                    break;
            }
        }
    }


    public sealed class LruCache<TKey, TValue>
        where TKey : notnull
    {
        private readonly int _capacity;
        private readonly ConcurrentDictionary<TKey, LinkedListNode<(TKey key, TValue value)>> _map = new();
        private readonly LinkedList<(TKey key, TValue value)> _lru = new();
        private readonly object _lock = new();

        public LruCache(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_map.TryGetValue(key, out var node))
            {
                lock (_lock)
                {
                    _lru.Remove(node);
                    _lru.AddFirst(node);
                }
                value = node.Value.value;
                return true;
            }
            value = default!;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var existing))
                {
                    existing.Value = (key, value);
                    _lru.Remove(existing);
                    _lru.AddFirst(existing);
                    return;
                }

                var node = new LinkedListNode<(TKey key, TValue value)>((key, value));
                _lru.AddFirst(node);
                _map[key] = node;

                if (_map.Count > _capacity)
                {
                    var last = _lru.Last;
                    if (last != null)
                    {
                        _lru.RemoveLast();
                        _map.TryRemove(last.Value.key, out _);
                    }
                }
            }
        }
    }

}
