// Models/Recognizers/Recognizer_Doji.cs
using System;
using System.Collections.Generic;

namespace StockAnalysis.WinForms.Models.Recognizers
{
    public sealed class Recognizer_Doji : Recognizer
    {
        public override string Name => "Doji";

        // Doji: very small body relative to total range.
        public override bool RecognizeAt(IList<SmartCandlestick> list, int i)
        {
            var c = list[i];

            decimal range = c.High - c.Low;
            if (range <= 0m) return false;

            decimal body = Math.Abs(c.Close - c.Open);
            decimal bodyVsRange = body / range;   // |close-open| / (high-low)

            return bodyVsRange <= 0.05m;          // ≤ 10% is a reasonable threshold
        }
    }
}
