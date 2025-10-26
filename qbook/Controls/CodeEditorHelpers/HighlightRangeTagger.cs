using ActiproSoftware.Text.Tagging.Implementation;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qbook.CodeEditor
{
    public class HighlightRangeTagger : CollectionTagger<IClassificationTag>
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>HighlightRangeTagger</c> class.
        /// </summary>
        /// <param name="document">The document to which this tagger is attached.</param>
        public HighlightRangeTagger(ICodeDocument document)
            : base(key: nameof(HighlightRangeTagger), orderings: null, document: document, isForLanguage: true) { }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates and adds a new <see cref="HighlightRangeTag"/> for the given range.
        /// </summary>
        /// <param name="snapshotRange">The snapshot range.</param>
        public void HighlightRange(TextSnapshotRange snapshotRange)
        {
            // Ignore zero-length ranges since there would be nothing to highlight
            if (snapshotRange.IsZeroLength)
                return;

            // Create a version range for precise control over text range tracking as the document is edited
            ITextVersionRange versionRange = snapshotRange.ToVersionRange(TextRangeTrackingModes.ExpandFirstEdge | TextRangeTrackingModes.DeleteWhenZeroLength);

            // Continue processing in the overload that accepts ITextVersionRange
            HighlightRange(versionRange);
        }

        /// <summary>
        /// Creates and adds a new <see cref="HighlightRangeTag"/> for the given range.
        /// </summary>
        /// <param name="versionRange">The version range.</param>
        public void HighlightRange(ITextVersionRange versionRange)
        {
            if (versionRange is null)
                throw new ArgumentNullException(nameof(versionRange));

            // Create a new IClassificationTag for the given range
            var tag = new HighlightRangeTag();

            // Add the tag to this collection
            this.Add(new TagVersionRange<IClassificationTag>(versionRange, tag));
        }

    }
}
