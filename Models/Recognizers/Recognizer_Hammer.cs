// Models/Recognizers/Recognizer_Hammer.cs
using System;
using System.Collections.Generic;

namespace StockAnalysis.WinForms.Models.Recognizers
{
    public sealed class Recognizer_Hammer : Recognizer
    {
        private readonly bool _bullish;                  // true = hammer; false = inverted hammer
        public Recognizer_Hammer(bool bullish) { _bullish = bullish; }

        public override string Name => _bullish ? "Hammer (Bullish)" : "Hammer (Inverted)";

        public override bool RecognizeAt(IList<SmartCandlestick> list, int i)
        {
            var c = list[i];

            // Basic anatomy computed from OHLC
            decimal range = c.High - c.Low;
            if (range <= 0m) return false;

            decimal bodyTop = Math.Max(c.Open, c.Close);
            decimal bodyBot = Math.Min(c.Open, c.Close);
            decimal body = bodyTop - bodyBot;

            decimal upperWick = c.High - bodyTop;
            decimal lowerWick = bodyBot - c.Low;

            // Small body
            bool smallBody = (body / range) <= 0.30m;

            if (_bullish)
            {
                // Hammer: body near the top, long lower wick
                bool bodyNearTop = (c.High - bodyTop) / range <= 0.15m;
                bool longLower = lowerWick >= (body * 2m);
                return smallBody && bodyNearTop && longLower;
            }
            else
            {
                // Inverted hammer: body near the bottom, long upper wick
                bool bodyNearBottom = (bodyBot - c.Low) / range <= 0.15m;
                bool longUpper = upperWick >= (body * 2m);
                return smallBody && bodyNearBottom && longUpper;
            }
        }
    }
}
