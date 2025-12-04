using System;
using System.Collections.Generic;

namespace StockAnalysis.WinForms.Models.Recognizers
{
    /// <summary>
    /// Recognizes Inverted Hammer (single-candle bullish reversal) patterns.
    /// Uses only OHLC so it works with any SmartCandlestick shape fields.
    /// </summary>
    public class Recognizer_InvertedHammer : Recognizer
    {
        public override string Name => "Inverted Hammer";

        public override bool RecognizeAt(IList<SmartCandlestick> smart, int i)
        {
            if (smart == null || i < 0 || i >= smart.Count) return false;

            var c = smart[i];

            // Compute from OHLC directly (avoid relying on SmartCandlestick-specific names)
            decimal body = Math.Abs(c.Close - c.Open);            // real body size
            if (body <= 0m) return false;

            decimal upper = c.High - Math.Max(c.Open, c.Close);   // upper wick
            decimal lower = Math.Min(c.Open, c.Close) - c.Low;    // lower wick
            if (upper < 0m) upper = 0m;
            if (lower < 0m) lower = 0m;

            decimal range = c.High - c.Low;
            if (range <= 0m) return false;

            // Inverted hammer characteristics:
            bool longUpper = upper >= 2m * body;                // long upper shadow
            bool tinyLower = lower <= body * 0.20m;             // tiny/no lower shadow
            bool bodyAtBottom = Math.Min(c.Open, c.Close)         // body near the low
                                <= c.Low + (range * 0.25m);

            return longUpper && tinyLower && bodyAtBottom;
        }

        public override List<int> FindAll(IList<SmartCandlestick> smart)
        {
            var hits = new List<int>();
            for (int i = 0; i < smart.Count; i++)
            {
                if (RecognizeAt(smart, i))
                    hits.Add(i);
            }
            return hits;
        }
    }
}
