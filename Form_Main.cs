using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using StockAnalysis.WinForms.Models;

namespace StockAnalysis.WinForms
{
    public partial class Form_Main : Form
    {
        // In-memory store for the currently loaded CSV
        private List<Candlestick> listOfCandlesticks = new List<Candlestick>();  // never null

        // Bindable view used as the chart’s DataSource
        private BindingList<Candlestick> boundCandlesticks = new BindingList<Candlestick>();

        // Resolved absolute path to “…\Stock Data”
        private string folder_stockData = string.Empty;

        // Files last chosen in OpenFileDialog
        private List<string> lastSelectedCsvFiles = new List<string>();

        /// <summary>
        /// Initializes the main form and its designer generated controls.
        /// </summary>
        public Form_Main()
        {
            InitializeComponent(); // create controls and wire designer events
        }

        /// <summary>
        /// Walks up from the EXE folder and returns the first directory that contains a child named exactly "Stock Data".
        /// Returns empty string if not found.
        /// </summary>
        private string ResolveStockDataFolder()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);          // start where the app runs
            for (int i = 0; i < 8 && dir != null; i++)                      // climb a few levels safely
            {
                string candidate = Path.Combine(dir.FullName, "Stock Data");// sibling "Stock Data"?
                if (Directory.Exists(candidate)) return candidate;          // success: return path
                dir = dir.Parent;                                           // otherwise keep walking up
            }
            return string.Empty;                                            // not found
        }

        /// <summary>
        /// One time UI and chart setup, date presets, and robust detection of the "Stock Data" folder.
        /// </summary>
        private void Form_Main_Load(object sender, EventArgs e)
        {
            // Period choices and behavior
            if (combobox_period.Items.Count == 0)
                combobox_period.Items.AddRange(new object[] { "Day", "Week", "Month" }); // exact labels per spec
            combobox_period.DropDownStyle = ComboBoxStyle.DropDownList;                  // lock to defined choices
            combobox_period.SelectedIndex = 0;                                           // default to Day
            chart_ohlc.Visible = false;                                                  // P2: main form shows no chart


            // Chart areas
            var areas = chart_ohlc.ChartAreas;                                          // collection handle
            ChartArea chartArea_price =
                areas.IndexOf("chartArea_price") >= 0 ? areas["chartArea_price"] : areas.Add("chartArea_price");
            ChartArea chartArea_volume =
                areas.IndexOf("chartArea_volume") >= 0 ? areas["chartArea_volume"] : areas.Add("chartArea_volume");
            chartArea_volume.AlignWithChartArea = chartArea_price.Name;                 // align X axes

            // Gridlines & style
            chartArea_price.AxisX.MajorGrid.Enabled = false;                            // no vertical lines on price
            chartArea_price.AxisY.MajorGrid.Enabled = true;                             // keep horizontal lines
            chartArea_volume.AxisX.MajorGrid.Enabled = false;                           // clean volume look
            chartArea_volume.AxisY.MajorGrid.Enabled = false;

            // Series (remove default and add our two)
            chart_ohlc.Series.Clear();
            Series series_candles = chart_ohlc.Series.Add("series_candles");            // OHLC series
            series_candles.ChartType = SeriesChartType.Candlestick;
            series_candles.ChartArea = chartArea_price.Name;
            series_candles.XValueType = ChartValueType.DateTime;                        // X binds to Date
            series_candles.YValueType = ChartValueType.Double;                          // Y binds to OHLC
            series_candles["PriceUpColor"] = "Lime";                                    // green up
            series_candles["PriceDownColor"] = "Red";                                   // red down

            Series series_volume = chart_ohlc.Series.Add("series_volume");              // Volume series
            series_volume.ChartType = SeriesChartType.Column;
            series_volume.ChartArea = chartArea_volume.Name;
            series_volume.XValueType = ChartValueType.DateTime;
            series_volume.YValueType = ChartValueType.Double;

            // remove legend for more room
            chart_ohlc.Legends.Clear();

            // Date presets 
            dateTimePicker_start.Value = DateTime.Today.AddYears(-1);
            dateTimePicker_end.Value = DateTime.Today;

            // Robust “Stock Data” locator
            folder_stockData = ResolveStockDataFolder();
            label_status.Text = string.IsNullOrWhiteSpace(folder_stockData)
                ? "Ready — Data folder not found"
                : $"Ready — Data folder: {folder_stockData}";

            // Make the main input form slim (no chart shown on the main window)
            chart_ohlc.Visible = false;                                  // hide the chart surface entirely
            chart_ohlc.Height = 1;                                       // collapse its height
            this.MinimumSize = new Size(this.Width, 150);                // keep a compact height
            this.ClientSize = new Size(this.ClientSize.Width, 150);      // shrink window height
        }

        /// <summary>
        /// Parses a CSV name like "AAPL-Day.csv" and returns (SYMBOL, PERIOD) normalized to ("AAPL","Day|Week|Month").
        /// Returns ("","") on failure.
        /// </summary>
        private (string symbol, string period) ParseSymbolAndPeriodFromFileName(string filePath)
        {
            string fileName = Path.GetFileName(filePath);                               // "AAPL-Day.csv"
            string baseName = Path.GetFileNameWithoutExtension(fileName);               // "AAPL-Day"
            int dash = baseName.LastIndexOf('-');                                       // split on last '-'
            if (dash <= 0 || dash >= baseName.Length - 1) return ("", "");              // invalid pattern
            string symbol = baseName.Substring(0, dash).Trim().ToUpperInvariant();      // normalize ticker
            string periodRaw = baseName.Substring(dash + 1).Trim();                     // "Day"|"Week"|"Month"

            string period =
                periodRaw.Equals("Day", StringComparison.OrdinalIgnoreCase) ? "Day" :
                periodRaw.Equals("Week", StringComparison.OrdinalIgnoreCase) ? "Week" :
                periodRaw.Equals("Month", StringComparison.OrdinalIgnoreCase) ? "Month" : "";

            if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(period)) return ("", "");
            return (symbol, period);
        }

        /// <summary>
        /// Opens a file picker (multi-select). First CSV populates the main chart; the rest open in child windows.
        /// </summary>
        private void button_loadData_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())                                                      // create a picker
            {
                // Filter for All / Day / Week / Month CSV files
                ofd.Filter =
                    "All CSV (*.csv)|*.csv|" +
                    "Day CSV (*-Day.csv)|*-Day.csv|" +
                    "Week CSV (*-Week.csv)|*-Week.csv|" +
                    "Month CSV (*-Month.csv)|*-Month.csv";
                ofd.FilterIndex = 1;                             // default to All

                ofd.Title = "Select stock CSV file(s)";          // dialog caption
                ofd.Multiselect = true;                          // allow multiple

                // Prefer the app's "Stock Data" folder if it exists
                var stockDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stock Data");
                if (System.IO.Directory.Exists(stockDir))
                    ofd.InitialDirectory = stockDir;
                else if (!string.IsNullOrWhiteSpace(folder_stockData) && Directory.Exists(folder_stockData))
                    ofd.InitialDirectory = folder_stockData;
                // start there

                var dlg = ofd.ShowDialog(this);                                                         // show the dialog
                if (dlg != DialogResult.OK || ofd.FileNames == null || ofd.FileNames.Length == 0)       // user canceled / none picked
                {
                    label_status.Text = "Open canceled.";                                               // status text
                    return;                                                                              // stop
                }

                lastSelectedCsvFiles = new List<string>(ofd.FileNames);                                  // remember selection

                int opened = 0;                                                                          // how many child forms opened
                string firstValidSymbol = null;                                                          // to sync UI (optional)
                string firstValidPeriod = null;                                                          // to sync UI (optional)

                // OPEN EVERY SELECTED CSV IN ITS OWN CHILD WINDOW (INCLUDING THE FIRST)
                for (int i = 0; i < lastSelectedCsvFiles.Count; i++)                                     // iterate all picks
                {
                    string path = lastSelectedCsvFiles[i];                                               // current CSV path

                    var (sym, per) = ParseSymbolAndPeriodFromFileName(path);                             // parse "SYMBOL-Period"
                    if (string.IsNullOrEmpty(sym) || string.IsNullOrEmpty(per))                          // bad filename pattern?
                        continue;                                                                        // skip

                    var data = readCandlesticksFromFile(path);                                           // load and parse rows
                    if (data.Count == 0)                                                                 // empty / malformed?
                        continue;                                                                        // skip

                    if (opened == 0)                                                                     // remember first valid
                    {
                        firstValidSymbol = sym;                                                          // save symbol
                        firstValidPeriod = per;                                                         // save period
                    }

                    var child = new Form_Chart(                                                           // create child window
                        sym,                                                                              // symbol (e.g., "AAPL")
                        per,                                                                              // period ("Day"/"Week"/"Month")
                        data,                                                                             // already-read candles
                        () => dateTimePicker_start.Value.Date,                                            // live Start from main
                        () => dateTimePicker_end.Value.Date                                               // live End from main
                    );

                    child.Text = $"{sym}-{per}";                                                          // window caption
                    child.Show(this);                                                                     // modeless; owner = main
                    opened++;                                                                             // count it
                }

                if (opened == 0)                                                                          // no valid files opened?
                {
                    MessageBox.Show(                                                                      // inform the user
                        "No valid CSV files were selected.\nExpected pattern: SYMBOL-Period.csv\nand columns Date/Open/High/Low/Close/Volume.",
                        "Nothing to open",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    label_status.Text = "No chart windows opened.";                                       // status text
                    return;                                                                               // stop
                }

                // Sync main UI to the first valid file (purely for user feedback on the inputs)
                if (!string.IsNullOrEmpty(firstValidSymbol))                                              // if we have a symbol
                    textbox_symbol.Text = firstValidSymbol;                                               // show it in textbox
                if (!string.IsNullOrEmpty(firstValidPeriod) &&                                            // if we have a period
                    combobox_period.Items.Contains(firstValidPeriod))                                     // ensure it exists in list
                    combobox_period.SelectedItem = firstValidPeriod;                                      // select it

                label_status.Text = $"Opened {opened} chart window(s).";                                  // final status
            }
        }


        /// <summary>
        /// CSV reader: header mapping + parsing; returns rows sorted oldest -> newest.
        /// </summary>
        private List<Candlestick> readCandlesticksFromFile(string filePath)
        {
            var result = new List<Candlestick>();
            if (!File.Exists(filePath)) return result;

            using (var reader = new StreamReader(filePath))
            {
                string header = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(header)) return result;

                char delim = header.Contains(",") ? ',' : (header.Contains(";") ? ';' : '\t');

                string[] headerCols = header.Split(delim);
                var nameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < headerCols.Length; i++) nameToIndex[headerCols[i].Trim().Trim('"')] = i;

                int idx(string[] names) { foreach (var n in names) if (nameToIndex.ContainsKey(n)) return nameToIndex[n]; return -1; }
                int iDate = idx(new[] { "Date", "date", "DATE" });
                int iOpen = idx(new[] { "Open", "open" });
                int iHigh = idx(new[] { "High", "high" });
                int iLow = idx(new[] { "Low", "low" });
                int iClose = idx(new[] { "Close", "close" });
                int iVol = idx(new[] { "Volume", "volume", "Vol" });
                if (iDate < 0 || iOpen < 0 || iHigh < 0 || iLow < 0 || iClose < 0 || iVol < 0) return result;

                var inv = CultureInfo.InvariantCulture;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(delim);
                    if (parts.Length <= Math.Max(iVol, Math.Max(iClose, iOpen))) continue;

                    string clean(int k) => parts[k].Trim().Trim('"', ' ', '\t', '\r', '\n');

                    if (!DateTime.TryParse(clean(iDate), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                        !DateTime.TryParse(clean(iDate), CultureInfo.CurrentCulture, DateTimeStyles.None, out date)) continue;

                    if (!decimal.TryParse(clean(iOpen), NumberStyles.Any, inv, out var open)) continue;
                    if (!decimal.TryParse(clean(iHigh), NumberStyles.Any, inv, out var high)) continue;
                    if (!decimal.TryParse(clean(iLow), NumberStyles.Any, inv, out var low)) continue;
                    if (!decimal.TryParse(clean(iClose), NumberStyles.Any, inv, out var close)) continue;

                    long volume;
                    if (!long.TryParse(clean(iVol), NumberStyles.AllowThousands, inv, out volume))
                    {
                        if (decimal.TryParse(clean(iVol), NumberStyles.Any, inv, out var volDec))
                            volume = (long)Math.Round(volDec);
                        else
                            continue;
                    }

                    result.Add(new Candlestick { Date = date, Open = open, High = high, Low = low, Close = close, Volume = volume });
                }
            }

            result.Sort((a, b) => a.Date.CompareTo(b.Date)); // chronological
            return result;
        }

        /// <summary>
        /// Returns a new list containing candles within the inclusive range [start, end].
        /// </summary>
        private List<Candlestick> filterCandlesticks(List<Candlestick> unfiltered, DateTime start, DateTime end)
        {
            var result = new List<Candlestick>();
            if (unfiltered == null || unfiltered.Count == 0) return result;
            if (start > end) { var t = start; start = end; end = t; }                  // guard reversed

            for (int i = 0; i < unfiltered.Count; i++)
            {
                var c = unfiltered[i];
                if (c.Date >= start && c.Date <= end) result.Add(c);                  // keep in-range rows
            }
            return result;
        }

        /// <summary>
        /// Sets sensible Y ranges (+/−2% headroom for price; +10% for volume) for the current view.
        /// </summary>
        private void normalizeChart(List<Candlestick> filtered)
        {
            if (filtered == null || filtered.Count == 0) return;

            decimal minLow = filtered[0].Low, maxHigh = filtered[0].High;
            long maxVol = filtered[0].Volume;
            for (int i = 1; i < filtered.Count; i++)
            {
                var c = filtered[i];
                if (c.Low < minLow) minLow = c.Low;
                if (c.High > maxHigh) maxHigh = c.High;
                if (c.Volume > maxVol) maxVol = c.Volume;
            }

            var caPrice = chart_ohlc.ChartAreas["chartArea_price"];
            var caVol = chart_ohlc.ChartAreas["chartArea_volume"];
            double padUp = (double)maxHigh * 0.02;
            double padDn = (double)minLow * 0.02;

            caPrice.AxisY.Minimum = Math.Max(0.0, (double)minLow - padDn);
            caPrice.AxisY.Maximum = (double)maxHigh + padUp;
            caVol.AxisY.Minimum = 0;
            caVol.AxisY.Maximum = Math.Max(1, (int)Math.Ceiling(maxVol * 1.10));
        }

        /// <summary>
        /// Two-line title: "SYMBOL-Period" on top; "start – end" (short dates) below.
        /// </summary>
        private void setChartTitle(string symbol, string period, DateTime start, DateTime end)
        {
            chart_ohlc.Titles.Clear();
            string line1 = $"{symbol}-{period}";
            string line2 = $"{start:d} \u2013 {end:d}";
            var t = new Title
            {
                Text = line1 + "\n" + line2,
                Alignment = ContentAlignment.MiddleCenter,
                Docking = Docking.Top,
                IsDockedInsideChartArea = false
            };
            chart_ohlc.Titles.Add(t);
        }

        /// <summary>
        /// Filter -> DATA BIND -> normalize -> set Title. Returns number of rows shown.
        /// </summary>
        private int update(List<Candlestick> source, DateTime start, DateTime end)
        {
            var filtered = filterCandlesticks(source, start, end);                     // make view
            boundCandlesticks = new BindingList<Candlestick>(filtered);                // bindable view

            // Turn on no weekend/holiday gaps ONLY for Day
            var period = (combobox_period.SelectedItem?.ToString() ?? "Day");
            bool isDay = string.Equals(period, "Day", StringComparison.OrdinalIgnoreCase);
            var sC = chart_ohlc.Series["series_candles"];
            var sV = chart_ohlc.Series["series_volume"];
            sC.IsXValueIndexed = isDay;
            sV.IsXValueIndexed = isDay;

            // DATA BINDING
            sC.XValueMember = nameof(Candlestick.Date);
            sC.YValueMembers = string.Join(",", nameof(Candlestick.High), nameof(Candlestick.Low),
                                                 nameof(Candlestick.Open), nameof(Candlestick.Close));
            sV.XValueMember = nameof(Candlestick.Date);
            sV.YValueMembers = nameof(Candlestick.Volume);
            chart_ohlc.DataSource = boundCandlesticks;                                  // set data source
            chart_ohlc.DataBind();                                                      // perform the bind

            normalizeChart(filtered);                                                   // scale axes
            var symbolForTitle = (textbox_symbol.Text ?? "").Trim().ToUpperInvariant(); // current ticker in UI
            setChartTitle(symbolForTitle, period, start, end);                          // 2-line title

            return filtered.Count;
        }

        /// <summary>
        /// Wrapper: update using the current in memory list and the picker dates.
        /// </summary>
        private int update()
        {
            return update(listOfCandlesticks, dateTimePicker_start.Value.Date, dateTimePicker_end.Value.Date);
        }

        /// <summary>
        /// Refresh button: applies the current date range without re reading files.
        /// </summary>
        private void button_refresh_Click(object sender, EventArgs e)
        {
            // Push the new date range to all open child chart windows
            int refreshed = 0;
            foreach (Form owned in this.OwnedForms)
            {
                if (owned is Form_Chart fc)
                {
                    fc.RefreshFromMainDates();    // child re filters and redraws from its in-memory data
                    refreshed++;
                }
            }
            label_status.Text = $"Refreshed {refreshed} chart window(s).";

        }
    }
}
