using System; // brings in DateTime and basic types we need.

namespace StockAnalysis.WinForms.Models // keeps our model types organized under the project’s namespace.
{
    /// <summary>
    /// Represents one OHLCV record (Open, High, Low, Close, Volume) for a specific trading date.
    /// </summary>
    public sealed class Candlestick // 'sealed' because we don't need inheritance here; it keeps the class simple and fixed.
    {
        /// <summary>
        /// Trading date of this candlestick.
        /// </summary>
        public DateTime Date { get; set; } // stores the calendar date for this row.

        /// <summary>
        /// Opening price for the day/period.
        /// </summary>
        public decimal Open { get; set; } // decimal keeps money precise (better than double for currency-like values).

        /// <summary>
        /// Highest price in the period.
        /// </summary>
        public decimal High { get; set; } // the intraperiod high value.

        /// <summary>
        /// Lowest price in the period.
        /// </summary>
        public decimal Low { get; set; } // the intraperiod low value.

        /// <summary>
        /// Closing price for the period.
        /// </summary>
        public decimal Close { get; set; } // the price at the end of the period.

        /// <summary>
        /// Trading volume for the period (number of shares/contracts).
        /// </summary>
        public long Volume { get; set; } // long to safely hold large volumes.

        /// <summary>
        /// True when the close is greater than or equal to the open (an "up" candle).
        /// </summary>
        public bool IsUp => Close >= Open; // handy property for coloring: green/lime if true, red if false.

        /// <summary>
        /// Returns a compact string for debugging and logging.
        /// </summary>
        public override string ToString() // overriding ToString helps when we log or inspect items quickly.
            => $"{Date:yyyy-MM-dd} O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}"; // formats key fields in a readable way.
    }
}
