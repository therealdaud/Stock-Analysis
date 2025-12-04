// Models/Recognizers/Recognizer.cs
using System.Collections.Generic;

namespace StockAnalysis.WinForms.Models.Recognizers
{
    /// Base type for all pattern recognizers.
    public abstract class Recognizer
    {
        /// Display name for UI (e.g., "Doji").
        public abstract string Name { get; }

        /// Return true if the pattern exists at index i.
        public abstract bool RecognizeAt(IList<SmartCandlestick> list, int i);

        /// Scan the window and return all hit indices.
        public virtual List<int> FindAll(IList<SmartCandlestick> list)
        {
            var hits = new List<int>();
            if (list == null || list.Count == 0) return hits;
            for (int i = 0; i < list.Count; i++)
                if (RecognizeAt(list, i)) hits.Add(i);
            return hits;
        }
    }
}
