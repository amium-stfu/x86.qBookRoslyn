using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoslynDocument = Microsoft.CodeAnalysis.Document;
using RoslynCompletionItem = global::Microsoft.CodeAnalysis.Completion.CompletionItem;

namespace qbook.RoslynServices
{
    public class RoslynAutoCompleteProvider
    {
        private readonly RoslynService _roslyn;

        public RoslynAutoCompleteProvider(RoslynService roslyn)
        {
            _roslyn = roslyn;
        }

        public async Task<IReadOnlyList<string>> GetSuggestionsAsync(RoslynDocument document, string text, int caretPosition, char? triggerChar = null)
        {
            var updatedDoc = document.WithText(Microsoft.CodeAnalysis.Text.SourceText.From(text, Encoding.UTF8));
            var (items, _) = await _roslyn.GetCompletionsAsync(updatedDoc, caretPosition);
            return items.Select(i => i.DisplayText).Distinct().OrderBy(s => s).ToList();
        }
    }
}
