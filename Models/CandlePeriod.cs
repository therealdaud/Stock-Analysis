namespace StockAnalysis.WinForms.Models // keeps the enum next to our model types.
{
    /// <summary>
    /// The time aggregation of each candlestick (matches file names Day/Week/Month).
    /// </summary>
    public enum CandlePeriod // enum makes the period choice strongly-typed and less error-prone than plain strings.
    {
        Day,   // daily candles, file suffix "-Day.csv".
        Week,  // weekly candles, file suffix "-Week.csv".
        Month  // monthly candles, file suffix "-Month.csv".
    }
}
