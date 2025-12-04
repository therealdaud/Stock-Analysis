// Models/Recognizers/Recognizer_Engulfing.cs
using System.Collections.Generic;

namespace StockAnalysis.WinForms.Models.Recognizers
{
    public sealed class Recognizer_Engulfing : Recognizer
    {
        private readonly bool _bullish;
        public Recognizer_Engulfing(bool bullish) { _bullish = bullish; }

        public override string Name => _bullish ? "Engulfing (Bullish)" : "Engulfing (Bearish)";

        public override bool RecognizeAt(IList<SmartCandlestick> list, int i)
        {
            if (i == 0) return false; // need a previous candle
            var p = list[i - 1];
            var c = list[i];

            // define “direction” by close vs open
            bool prevDown = p.Close < p.Open;
            bool prevUp = p.Close > p.Open;
            bool currUp = c.Close > c.Open;
            bool currDown = c.Close < c.Open;

            // bodies
            decimal pLow = System.Math.Min(p.Open, p.Close);
            decimal pHigh = System.Math.Max(p.Open, p.Close);
            decimal cLow = System.Math.Min(c.Open, c.Close);
            decimal cHigh = System.Math.Max(c.Open, c.Close);

            if (_bullish)
            {
                // prev down, current up, and current body engulfs previous body
                return prevDown && currUp && cLow <= pLow && cHigh >= pHigh;
            }
            else
            {
                // prev up, current down, and current body engulfs previous body
                return prevUp && currDown && cLow <= pLow && cHigh >= pHigh;
            }
        }
    }
}
