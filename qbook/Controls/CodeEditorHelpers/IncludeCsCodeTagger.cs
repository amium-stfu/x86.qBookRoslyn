using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Utility;
using ActiproSoftware.Text;
using ActiproSoftware.UI.WinForms.Controls.Rendering;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting.Implementation;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Highlighting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbook.CodeEditor
{
    public class IncludeCsCodeTagger : TaggerBase<IClassificationTag>
    {
        private static IClassificationType includeCsCodeClassificationType = new ClassificationType("WordHighlight", "IncludeCsCode");
        /// </summary>
        static IncludeCsCodeTagger()
        {
            IHighlightingStyle style = new HighlightingStyle(null, Color.FromArgb(0xa0, 0xf5, 0xde, 0xb3));
            style.BorderColor = Color.FromArgb(0xFF, 0xf5, 0xde, 0xb3);
            style.BorderCornerKind = HighlightingStyleBorderCornerKind.Square;
            style.BorderKind = LineKind.Solid;
            AmbientHighlightingStyleRegistry.Instance.Register(includeCsCodeClassificationType, style);
        }

        private ITagAggregator<ITokenTag> tokenTagAggregator;

        /// <summary>
        /// Initializes a new instance of the <c>CustomSquiggleTagger</c> class.
        /// </summary>
        /// <param name="document">The document to which this manager is attached.</param>
        public IncludeCsCodeTagger(ICodeDocument document) :
            base("Custom", new Ordering[] { new Ordering(TaggerKeys.Token, OrderPlacement.Before) }, document)
        {
            tokenTagAggregator = document.CreateTagAggregator<ITokenTag>();
        }




        public override IEnumerable<TagSnapshotRange<IClassificationTag>> GetTags(
          NormalizedTextSnapshotRangeCollection snapshotRanges, object parameter)
        {
            foreach (TextSnapshotRange snapshotRange in snapshotRanges)
            {
                // If the snapshot range is not zero-length...
                if (!snapshotRange.IsZeroLength)
                {
                    IEnumerable<TagSnapshotRange<ITokenTag>> tokenTagRanges = tokenTagAggregator.GetTags(snapshotRange);
                    if (tokenTagRanges != null)
                    {
                        foreach (TagSnapshotRange<ITokenTag> tokenTagRange in tokenTagRanges)
                        {
                            if (tokenTagRange.Tag.Token != null)
                            {
                                switch (tokenTagRange.Tag.Token.Key)
                                {
                                    case "CommentSingleLine":
                                        {
                                            if (true) //highlightDocumentationComments)
                                            {
                                                // Get the text of the token
                                                string text = tokenTagRange.SnapshotRange.Text;

                                                // Look for the text "Actipro"
                                                int index = text.IndexOf("+include");
                                                while (index != -1)
                                                {
                                                    // Add a highlighted range
                                                    yield return new TagSnapshotRange<IClassificationTag>(
                                                        new TextSnapshotRange(snapshotRange.Snapshot, TextRange.FromSpan(tokenTagRange.SnapshotRange.StartOffset + index - 2, text.Length)),
                                                        new ClassificationTag(includeCsCodeClassificationType)
                                                        );

                                                    // Look for another match
                                                    index = text.IndexOf("+include", index + 8);
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
