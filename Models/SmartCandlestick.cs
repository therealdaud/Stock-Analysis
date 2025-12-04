using System;                                            // Math operations
using System.Collections.Generic;                        // For List<T> collection

namespace StockAnalysis.WinForms.Models
{
    /// <summary>
    /// SmartCandlestick: stores OHLCV data and calculates anatomy + pattern flags.
    /// Uses composition (not inheritance) because Candlestick is sealed.
    /// </summary>
    public class SmartCandlestick
    {
        // RAW OHLCV DATA
        public DateTime Date { get; private set; }        // candle's date
        public decimal Open { get; private set; }         // opening price
        public decimal High { get; private set; }         // highest price
        public decimal Low { get; private set; }          // lowest price
        public decimal Close { get; private set; }        // closing price
        public long Volume { get; private set; }          // trading volume

        // CONSTANT THRESHOLDS
        private const decimal DOJI_BODY_RATIO = 0.10m;    // body ≤ 10% of range → Doji
        private const decimal MARUBOZU_SHADOW_RATIO = 0.05m; // shadow ≤ 5% of range → Marubozu
        private const decimal HAMMER_TAIL_TO_BODY_MIN = 2.0m; // tail ≥ 2× body → hammer
        private const decimal HAMMER_SMALL_TAIL_RATIO = 0.20m; // opposite tail ≤ 20% of body

        // COMPUTED ANATOMY
        public decimal range { get; private set; }        // total height (High - Low)
        public decimal bodyRange { get; private set; }    // candle body height (|Close - Open|)
        public decimal upperTailRange { get; private set; } // upper shadow length
        public decimal lowerTailRange { get; private set; } // lower shadow length
        public decimal bodyTop { get; private set; }      // top of real body (max(Open, Close))
        public decimal bodyBottom { get; private set; }   // bottom of real body (min(Open, Close))

        // POLARITY
        public bool isBullish { get; private set; }       // green candle (Close > Open)
        public bool isBearish { get; private set; }       // red candle (Open > Close)

        // PATTERN FLAGS
        public bool isDoji { get; private set; }          // small body vs total range
        public bool isDojiDragonfly { get; private set; } // open≈close≈high, long lower tail
        public bool isDojiGravestone { get; private set; }// open≈close≈low, long upper tail
        public bool isMarubozu { get; private set; }      // full-body candle, no shadows
        public bool isHammer { get; private set; }        // long lower tail, small upper tail
        public bool isInvertedHammer { get; private set; }// long upper tail, small lower tail

        /// <summary>
        /// Builds a SmartCandlestick by copying values from a Candlestick object.
        /// </summary>
        public SmartCandlestick(Candlestick c)
        {
            // Copy basic OHLCV fields from Candlestick
            this.Date = c.Date;       // assign candle date
            this.Open = c.Open;       // assign opening price
            this.High = c.High;       // assign high
            this.Low = c.Low;         // assign low
            this.Close = c.Close;     // assign closing price
            this.Volume = c.Volume;   // assign volume

            // Compute anatomical parts 
            this.bodyTop = (Open > Close ? Open : Close);     // higher of open/close
            this.bodyBottom = (Open > Close ? Close : Open);  // lower of open/close
            this.range = Math.Max(0m, High - Low);            // total range height
            this.bodyRange = Math.Abs(Close - Open);          // body height
            this.upperTailRange = Math.Max(0m, High - bodyTop);   // upper shadow
            this.lowerTailRange = Math.Max(0m, bodyBottom - Low); // lower shadow

            // Determine polarity (bullish/bearish) 
            this.isBullish = Close > Open;                    // green if close > open
            this.isBearish = Open > Close;                    // red if open > close

            // Prevent divide-by-zero for flat candles 
            decimal safeRange = (this.range <= 0m ? 1m : this.range);

            // Doji and its subtypes 
            this.isDoji = (this.bodyRange / safeRange) <= DOJI_BODY_RATIO; // very small body
            this.isDojiDragonfly = this.isDoji &&                          // tiny body near high
                                   (this.upperTailRange / safeRange) <= MARUBOZU_SHADOW_RATIO &&
                                   (this.lowerTailRange / safeRange) >= (DOJI_BODY_RATIO * 2m);
            this.isDojiGravestone = this.isDoji &&                         // tiny body near low
                                    (this.lowerTailRange / safeRange) <= MARUBOZU_SHADOW_RATIO &&
                                    (this.upperTailRange / safeRange) >= (DOJI_BODY_RATIO * 2m);

            // Marubozu: both shadows extremely short 
            this.isMarubozu = (this.upperTailRange / safeRange) <= MARUBOZU_SHADOW_RATIO &&
                              (this.lowerTailRange / safeRange) <= MARUBOZU_SHADOW_RATIO;

            // Hammer: long lower tail, small upper tail 
            this.isHammer = !this.isDoji &&
                            (lowerTailRange >= HAMMER_TAIL_TO_BODY_MIN * bodyRange) &&
                            (upperTailRange <= HAMMER_SMALL_TAIL_RATIO * bodyRange);

            // Inverted Hammer: long upper tail, small lower tail
            this.isInvertedHammer = !this.isDoji &&
                                    (upperTailRange >= HAMMER_TAIL_TO_BODY_MIN * bodyRange) &&
                                    (lowerTailRange <= HAMMER_SMALL_TAIL_RATIO * bodyRange);
        }

        /// <summary>
        /// Converts a list of Candlestick → list of SmartCandlestick (adds analytics).
        /// </summary>
        public static List<SmartCandlestick> ToSmartList(IEnumerable<Candlestick> src)
        {
            var list = new List<SmartCandlestick>();        // prepare output list
            if (src == null) return list;                   // return empty if null input
            foreach (var c in src)                          // loop each candle
                list.Add(new SmartCandlestick(c));          // wrap and compute smart version
            return list;                                    // return computed list
        }
    }
}
