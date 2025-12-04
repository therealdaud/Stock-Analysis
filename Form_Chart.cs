using System;                                            // DateTime, Func<>
using System.Collections.Generic;                        // List<T>
using System.ComponentModel;                             // BindingList<T>
using System.Drawing;                                    // Font, ContentAlignment
using System.Windows.Forms;                              // Form, events
using System.Windows.Forms.DataVisualization.Charting;   // Chart, Series, ChartArea, Title
using StockAnalysis.WinForms.Models.Recognizers;         // ensure this using is at top of file

namespace StockAnalysis.WinForms
{
    public partial class Form_Chart : Form
    {
        // per-window immutable state
        private string _symbol;                   // stock ticker displayed by this child
        private string _period;                   // "Day" | "Week" | "Month" for this child
        private readonly List<Models.Candlestick> _source; // list stays readonly (we mutate contents only)

        // view + external date providers
        private BindingList<Models.Candlestick> _bound =                  // filtered/bound view used by DataBinding
            new BindingList<Models.Candlestick>();                        // start with empty list

        private readonly Func<DateTime> _getStart;                        // delegate to read Start date from main form
        private readonly Func<DateTime> _getEnd;                          // delegate to read End   date from main form                                                                  
        private System.Windows.Forms.Timer timer_ticker;  // drives playback ticks
        private int _tickIndex = 0;                       // last index shown
        private bool _playing = false;                    // play/pause state
                                                          // playback working sets
        private List<Models.Candlestick> _playAll = null;         // all candles in current window
        private BindingList<Models.Candlestick> _playView = null; // growing subset
        private IList<int> _patternHits = null;                 // indices in _playAll that match current pattern
        private string _activePattern = null;                   // current combobox text (normalized)
        private System.Drawing.Color _activePatternColor;       // arrow color to use during this run

        private void ResetPlayback()
        {
            _playing = false;                 // not playing
            _tickIndex = 0;                   // start index
            _playAll = null;                  // forget lists
            _playView = null;
            if (this.timer_ticker != null) this.timer_ticker.Stop();       // stop timer
            if (this.button_playPause != null) this.button_playPause.Text = "Play"; // update text
                                                                                    // re-enable inputs (safety)
            if (this.dateTimePicker_start != null) this.dateTimePicker_start.Enabled = true;
            if (this.dateTimePicker_end != null) this.dateTimePicker_end.Enabled = true;
            if (this.button_load != null) this.button_load.Enabled = true;
            if (this.button_refresh != null) this.button_refresh.Enabled = true;
        }


        /// <summary>
        /// Creates a chart window for one (symbol, period) using in-memory candles and live Start/End accessors.
        /// </summary>
        /// <param name="symbol">Ticker (e.g., "AAPL").</param>
        /// <param name="period">"Day", "Week", or "Month".</param>
        /// <param name="source">All candles (already parsed and sorted oldest→newest).</param>
        /// <param name="getStart">Function returning current Start date from the main form.</param>
        /// <param name="getEnd">Function returning current End date from the main form.</param>
        public Form_Chart(string symbol,
                          string period,
                          List<Models.Candlestick> source,
                          Func<DateTime> getStart,
                          Func<DateTime> getEnd)
        {
            InitializeComponent();                        // build controls added in the Designer
            _symbol = symbol;                           // remember symbol for the title
            _period = period;                           // remember period for gaps/title logic
            _source = source ?? new List<Models.Candlestick>(); // keep a non-null list
            _getStart = getStart;                         // store Start accessor
            _getEnd = getEnd;                           // store End accessor

            // Seed the child pickers to match the main form's current dates
            if (this.dateTimePicker_start != null && _getStart != null)
                this.dateTimePicker_start.Value = _getStart().Date;
            if (this.dateTimePicker_end != null && _getEnd != null)
                this.dateTimePicker_end.Value = _getEnd().Date;

            // Pattern dropdown: populate once
            if (this.comboBox_pattern != null && this.comboBox_pattern.Items.Count == 0)
            {
                this.comboBox_pattern.DropDownStyle = ComboBoxStyle.DropDownList; // fixed set
                this.comboBox_pattern.Items.Add("— Pattern —");                   // placeholder
                this.comboBox_pattern.Items.Add("Doji");                           // already implemented
                this.comboBox_pattern.Items.Add("Hammer (Bullish)");               // new
                this.comboBox_pattern.Items.Add("Hammer (Inverted)");              // new
                this.comboBox_pattern.Items.Add("Engulfing (Bullish)");            // new
                this.comboBox_pattern.Items.Add("Engulfing (Bearish)");            // new
                this.comboBox_pattern.SelectedIndex = 0;                            // default = placeholder
            }


            // create timer and sync its interval to the speed trackbar
            this.timer_ticker = new System.Windows.Forms.Timer();      // WinForms timer
            this.timer_ticker.Interval = this.trackBar_speed.Value;    // ms from slider
            this.timer_ticker.Tick += new EventHandler(this.timer_ticker_Tick); // tick handler



            ConfigureChartOnce();                         // one-time visual configuration
            RefreshFromMainDates();                       // initial paint using current picker dates
        }

        /// <summary>
        /// One-time chart setup: areas, series, legend removal, "no gaps" for Day, and an initial placeholder title.
        /// </summary>
        private void ConfigureChartOnce()
        {
            var areas = chart_ohlc.ChartAreas;                               // reference areas collection

            // Ensure a price area (top)
            ChartArea caPrice = areas.IndexOf("chartArea_price") >= 0
                ? areas["chartArea_price"]                                    // reuse if exists
                : areas.Add("chartArea_price");                                // else create

            // Ensure a volume area (bottom)
            ChartArea caVol = areas.IndexOf("chartArea_volume") >= 0
                ? areas["chartArea_volume"]                                    // reuse if exists
                : areas.Add("chartArea_volume");                                // else create

            caVol.AlignWithChartArea = caPrice.Name;                           // share X axis with price

            // Simplify gridlines per spec
            caPrice.AxisX.MajorGrid.Enabled = false;                           // no vertical grid on price
            caPrice.AxisY.MajorGrid.Enabled = true;                            // keep horizontal grid on price
            caVol.AxisX.MajorGrid.Enabled = false;                           // clean volume X grid
            caVol.AxisY.MajorGrid.Enabled = false;                           // clean volume Y grid

            chart_ohlc.Series.Clear();                                         // remove any default series ("Series1")

            // Price (candlesticks) series
            Series sC = chart_ohlc.Series.Add("series_candles");               // add candle series
            sC.ChartType = SeriesChartType.Candlestick;                       // render OHLC as candles
            sC.ChartArea = caPrice.Name;                                      // draw on price area
            sC.XValueType = ChartValueType.DateTime;                           // X axis is DateTime
            sC.YValueType = ChartValueType.Double;                             // Y values are numeric
            sC["PriceUpColor"] = "Lime";                                     // up candle color = green
            sC["PriceDownColor"] = "Red";                                      // down candle color = red

            // Volume (columns) series
            Series sV = chart_ohlc.Series.Add("series_volume");                // add volume series
            sV.ChartType = SeriesChartType.Column;                            // column bars
            sV.ChartArea = caVol.Name;                                        // draw on volume area
            sV.XValueType = ChartValueType.DateTime;                           // align bars by date
            sV.YValueType = ChartValueType.Double;                             // numeric volume

            // Remove legend to free right-side space (per spec)
            if (chart_ohlc.Legends.Count > 0) chart_ohlc.Legends[0].Enabled = false;

            // For "Day", compress weekend/holiday gaps (no gaps requirement)
            bool isDay = _period.Equals("Day", StringComparison.OrdinalIgnoreCase); // check period
            sC.IsXValueIndexed = isDay;                                             // index X for price when Day
            sV.IsXValueIndexed = isDay;                                             // index X for volume when Day

            // Seed a placeholder 2-line title; actual date line is set during Refresh
            chart_ohlc.Titles.Clear();                                              // start fresh
            chart_ohlc.Titles.Add($"{_symbol} - {_period}");                        // line 1 placeholder
            chart_ohlc.Titles.Add(string.Empty);                                     // line 2 placeholder
        }

        /// <summary>
        /// Re-filters the in-memory candles by Start/End from the main form, binds to the chart, normalizes axes, and updates the 2-line title.
        /// </summary>
        public void RefreshFromMainDates()
        {
            ResetPlayback();   // any date change cancels ongoing playback
            // wipe all pattern state when dates change
            _patternHits = null;
            _activePattern = null;
            ClearPatternAnnotations();
            ClearPatternMarkers();

            // Prefer this child window's pickers; if absent, fall back to the main form delegates
            DateTime start = (this.dateTimePicker_start != null ? this.dateTimePicker_start.Value.Date
                                                               : (_getStart != null ? _getStart().Date : DateTime.MinValue));
            DateTime end = (this.dateTimePicker_end != null ? this.dateTimePicker_end.Value.Date
                                                               : (_getEnd != null ? _getEnd().Date : DateTime.MaxValue));

            if (start > end) { var t = start; start = end; end = t; }                  // swap if reversed

            // Build filtered view (do not mutate _source)
            var view = new List<Models.Candlestick>();                                 // collect in-range rows
            for (int i = 0; i < _source.Count; i++)                                     // scan all rows
            {
                var c = _source[i];                                                    // row i
                if (c.Date >= start && c.Date <= end) view.Add(c);                     // keep inside [start,end]
            }
            _bound = new BindingList<Models.Candlestick>(view);                        // replace binding list

            // DATA BINDING
            var sC = chart_ohlc.Series["series_candles"];                              // price series handle
            sC.XValueMember = nameof(Models.Candlestick.Date);                        // X = Date
            sC.YValueMembers = string.Join(",",                                        // Y members (High,Low,Open,Close)
                nameof(Models.Candlestick.High),
                nameof(Models.Candlestick.Low),
                nameof(Models.Candlestick.Open),
                nameof(Models.Candlestick.Close));

            var sV = chart_ohlc.Series["series_volume"];                               // volume series handle
            sV.XValueMember = nameof(Models.Candlestick.Date);                        // X = Date
            sV.YValueMembers = nameof(Models.Candlestick.Volume);                      // Y = Volume

            // remove weekend gaps if period = Day
            bool isDayNow = _period.Equals("Day", StringComparison.OrdinalIgnoreCase);
            sC.IsXValueIndexed = isDayNow;  // price
            sV.IsXValueIndexed = isDayNow;  // volume




            chart_ohlc.DataSource = _bound;                                            // set shared data source
            chart_ohlc.DataBind();                                                     // bind both series

            NormalizeAxes(view);                                                       // compute sensible Y ranges
            UpdateTitles(start, end);                                                  // set 2-line title in required format
        }

        /// <summary>
        /// Sets AxisY ranges (+/−2% headroom for price, +10% headroom for volume) for the current filtered view.
        /// </summary>
        private void NormalizeAxes(List<Models.Candlestick> filtered)
        {
            if (filtered == null || filtered.Count == 0) return;                       // nothing to scale

            decimal minLow = filtered[0].Low;                                          // seed min
            decimal maxHigh = filtered[0].High;                                        // seed max
            long maxVol = filtered[0].Volume;                                       // seed volume max

            for (int i = 1; i < filtered.Count; i++)                                   // scan all rows
            {
                var c = filtered[i];                                                   // row i
                if (c.Low < minLow) minLow = c.Low;                               // track min low
                if (c.High > maxHigh) maxHigh = c.High;                              // track max high
                if (c.Volume > maxVol) maxVol = c.Volume;                            // track max vol
            }

            var caPrice = chart_ohlc.ChartAreas["chartArea_price"];                    // price area handle
            var caVol = chart_ohlc.ChartAreas["chartArea_volume"];                   // volume area handle

            double padUp = (double)maxHigh * 0.02;                                     // +2% padding above
            double padDn = (double)minLow * 0.02;                                     // +2% padding below

            caPrice.AxisY.Minimum = Math.Max(0.0, (double)minLow - padDn);             // clamp at >= 0
            caPrice.AxisY.Maximum = (double)maxHigh + padUp;                           // padded top

            caVol.AxisY.Minimum = 0;                                                   // volume starts at zero
            caVol.AxisY.Maximum = Math.Max(1, (int)Math.Ceiling(maxVol * 1.10));       // +10% volume headroom
        }

        /// <summary>
        /// Sets the chart title to two lines exactly as required:
        ///  1) "SYMBOL - Day/Week/Month"
        ///  2) "MM/dd/yyyy – MM/dd/yyyy"
        /// </summary>
        private void UpdateTitles(DateTime start, DateTime end)
        {
            string line1 = $"{_symbol} - {_period}";                                   // top line with symbol/period
            string line2 = $"{start:MM/dd/yyyy} – {end:MM/dd/yyyy}";                   // date line in exact format

            chart_ohlc.Titles.Clear();                                                 // reset titles
            chart_ohlc.Titles.Add(new Title                                           // add a single 2-line Title
            {
                Text = line1 + Environment.NewLine + line2,                            // combine with newline
                Alignment = ContentAlignment.MiddleCenter,                             // centered title
                Docking = Docking.Top,                                                 // dock above plotting area
                IsDockedInsideChartArea = false,                                       // keep outside chart area
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)                     // readable UI font
            });
        }

        /// <summary>
        /// Click handler for the child window's Refresh button: re-filters & redraws using the main form’s current dates.
        /// </summary>
        private void button_refresh_Click(object sender, EventArgs e)
        {
            RefreshFromMainDates();                                                    // re-run full render path
        }

        /// <summary>TrackBar changed: reflect in TextBox and timer interval.</summary>
        private void trackBar_speed_Scroll(object sender, EventArgs e)
        {
            this.textBox_ms.Text = this.trackBar_speed.Value.ToString();   // show ms
            this.timer_ticker.Interval = this.trackBar_speed.Value;        // apply ms
        }

        /// <summary>TextBox edited: clamp [100..2000], push to TrackBar and timer.</summary>
        private void textBox_ms_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(this.textBox_ms.Text.Trim(), out int val)) return; // ignore bad input
            if (val < 100) val = 100;                                            // clamp min
            if (val > 2000) val = 2000;                                          // clamp max
            if (this.trackBar_speed.Value != val) this.trackBar_speed.Value = val; // sync slider
            this.timer_ticker.Interval = val;                                    // apply to timer
        }

        /// <summary>
        /// Remove any prior pattern annotations from the chart.
        /// </summary>
        private void ClearPatternAnnotations()
        {
            this.chart_ohlc.Annotations.Clear();  // wipe all chart annotations
        }

        /// <summary>
        /// Build a SmartCandlestick list for the current visible date window (same logic as RefreshFromMainDates).
        /// </summary>
        private List<Models.SmartCandlestick> BuildSmartListForCurrentRange()
        {
            var smart = new List<Models.SmartCandlestick>();              // output list
                                                                          // read date pickers, fall back to delegates
            DateTime start = (this.dateTimePicker_start != null ? this.dateTimePicker_start.Value.Date
                                                                : (_getStart != null ? _getStart().Date : DateTime.MinValue));
            DateTime end = (this.dateTimePicker_end != null ? this.dateTimePicker_end.Value.Date
                                                              : (_getEnd != null ? _getEnd().Date : DateTime.MaxValue));
            // swap if reversed
            if (start > end) { var t = start; start = end; end = t; }     // ensure start <= end

            // filter _source into window
            var window = new List<Models.Candlestick>();                  // raw windowed candles
            for (int i = 0; i < _source.Count; i++)                       // scan all rows
            {
                var c = _source[i];                                       // row i
                if (c.Date >= start && c.Date <= end)                     // inside window?
                    window.Add(c);                                        // keep row
            }

            // convert to smart list
            foreach (var c in window)                                     // wrap each candle
                smart.Add(new Models.SmartCandlestick(c));                // compute anatomy/flags

            return smart;                                                 // done
        }

        // Draw arrows in a specific color, anchored to the candle's datapoint
        // Replace existing AnnotateMatches with this exact method
        private void AnnotateMatches(IList<Models.SmartCandlestick> smart, IList<int> indices, System.Drawing.Color arrowColor)
        {
            // clear previous annotations & markers
            this.chart_ohlc.Annotations.Clear();
            ClearPatternMarkers();

            var priceArea = chart_ohlc.ChartAreas["chartArea_price"];
            var sC = chart_ohlc.Series["series_candles"];
            if (smart == null || indices == null || indices.Count == 0) { chart_ohlc.Invalidate(); return; }
            if (sC == null || sC.Points == null || sC.Points.Count == 0) { chart_ohlc.Invalidate(); return; }

            // For each hit: set star marker and add an anchored text annotation (triangle) so it's always visible
            foreach (var smartIdx in indices)
            {
                if (smartIdx < 0 || smartIdx >= smart.Count) continue;
                var sc = smart[smartIdx];

                // find the corresponding plotted point index for the candle Date
                int pIdx = FindPointIndexByDate(sC, sc.Date);
                if (pIdx < 0) continue;                           // not visible in current window
                var dp = sC.Points[pIdx];                         // the plotted point

                // keep the star marker so user already sees it
                dp.MarkerStyle = MarkerStyle.Star4;
                dp.MarkerSize = 10;
                dp.MarkerColor = System.Drawing.Color.Magenta;

                // Create a TextAnnotation with a triangle symbol anchored to the datapoint
                // (Using a textual triangle is robust across chart configs and respects axes)
                var tri = new TextAnnotation
                {
                    Text = "▲",                                   // triangle glyph
                    AnchorDataPoint = dp,                         // lock to the data point
                    AxisX = priceArea.AxisX,
                    AxisY = priceArea.AxisY,
                    Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
                    ForeColor = arrowColor,                       // color per pattern
                    BackColor = System.Drawing.Color.Transparent,
                    AllowMoving = false,
                    ClipToChartArea = priceArea.Name,
                    // Slight offset so triangle sits above the candle: Use AnchorAlignment to place relative to the point
                    AnchorAlignment = ContentAlignment.BottomCenter
                };

                chart_ohlc.Annotations.Add(tri);
            }

            chart_ohlc.Invalidate(); // force redraw
        }

        // Map dropdown text -> recognizer (only the ones we know are stable)
        private StockAnalysis.WinForms.Models.Recognizers.Recognizer MakeRecognizer(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            switch (name.Trim())
            {
                case "Doji":
                    return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Doji();

                case "Engulfing (Bullish)":
                    return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Engulfing(true);

                case "Engulfing (Bearish)":
                    return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Engulfing(false);

                case "Hammer (Bullish)":
                    return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Hammer(true);

                case "Hammer (Bearish)":
                    return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Hammer(false);

                // The ones we don’t have implemented yet
                case "Inverted Hammer (Bullish)":
                case "Inverted Hammer (Bearish)":
                case "Morning Star":
                case "Evening Star":
                case "Harami (Bullish)":
                case "Harami (Bearish)":
                    MessageBox.Show("This pattern is not implemented yet.", "Info",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;

                default:
                    return null;
            }
        }


        // Assign a distinct arrow color per pattern
        private System.Drawing.Color GetPatternColor(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return System.Drawing.Color.Magenta;
            switch (name.Trim())
            {
                case "Doji": return System.Drawing.Color.Magenta;
                case "Engulfing (Bullish)": return System.Drawing.Color.LimeGreen;
                case "Engulfing (Bearish)": return System.Drawing.Color.Red;
                case "Hammer (Bullish)": return System.Drawing.Color.DodgerBlue;
                case "Hammer (Bearish)": return System.Drawing.Color.MediumPurple;
                default: return System.Drawing.Color.Gray; // for “not implemented yet”
            }
        }


        private void comboBox_pattern_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearPatternAnnotations();
            ClearPatternMarkers();

            var choice = this.comboBox_pattern.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(choice) || choice.StartsWith("—")) return;

            // Always rebind to current window so DataPoints match dates
            RefreshFromMainDates();

            // Build smart list for current window
            var smart = BuildSmartListForCurrentRange();

            // Get a recognizer only for the patterns we know are stable
            var recog = MakeRecognizer(choice);
            if (recog == null)
            {
                // Show “not implemented yet” once; comment this out later if you prefer silent
                MessageBox.Show($"{choice} is not implemented yet.", "Pattern", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Run recognizer with guard so one bug can’t kill all arrows
            List<int> hits;
            try
            {
                hits = recog.FindAll(smart) ?? new List<int>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Pattern error: {ex.Message}", "Pattern", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Draw arrows with a distinct color per pattern
            var color = GetPatternColor(choice);
            AnnotateMatches(smart, hits, color);

            // Keep count in title for sanity
            this.Text = $"{_symbol}-{_period}  ({choice}: {hits.Count})";
        }



        /// <summary>
        /// Remove any prior point markers from the candlestick series.
        /// </summary>
        private void ClearPatternMarkers()
        {
            var sC = chart_ohlc.Series["series_candles"];              // candle series
            if (sC == null || sC.Points == null) return;               // guard
            for (int i = 0; i < sC.Points.Count; i++)                  // loop all points
            {
                var p = sC.Points[i];                                  // point i
                p.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None; // no marker
                p.MarkerSize = 0;                                      // hide size
            }
        }


        /// <summary>
        /// Handles Play/Pause logic. If not currently playing, resumes if paused or starts fresh.
        /// </summary>
        private void button_playPause_Click(object sender, EventArgs e)
        {
            // If currently stopped → try to play
            if (!_playing)
            {
                // RESUME mode: lists already exist and not finished
                if (_playAll != null && _playView != null && _tickIndex > 0 && _tickIndex < _playAll.Count)
                {
                    _playing = true;                                         // mark as playing
                    this.button_playPause.Text = "Pause";                    // toggle text
                    this.timer_ticker.Interval = this.trackBar_speed.Value;  // apply slider speed
                    this.timer_ticker.Start();                               // resume timer
                                                                             // disable controls during playback
                    this.dateTimePicker_start.Enabled = false;
                    this.dateTimePicker_end.Enabled = false;
                    this.button_load.Enabled = false;
                    this.button_refresh.Enabled = false;
                    return;
                }

                // FRESH RUN: build full working lists
                _playing = true;                          // now playing
                this.button_playPause.Text = "Pause";     // toggle text

                // Get date range (from pickers or parent form)
                DateTime start = (this.dateTimePicker_start != null ? this.dateTimePicker_start.Value.Date
                                                                    : (_getStart != null ? _getStart().Date : DateTime.MinValue));
                DateTime end = (this.dateTimePicker_end != null ? this.dateTimePicker_end.Value.Date
                                                                : (_getEnd != null ? _getEnd().Date : DateTime.MaxValue));
                if (start > end) { var t = start; start = end; end = t; }    // swap if reversed

                // Filter candles in date range
                _playAll = new List<Models.Candlestick>();
                for (int i = 0; i < _source.Count; i++)
                {
                    var c = _source[i];
                    if (c.Date >= start && c.Date <= end)
                        _playAll.Add(c);
                }

                // No candles → stop immediately
                if (_playAll.Count == 0)
                {
                    ResetPlayback();
                    return;
                }

                // Initialize playback list and bind
                _playView = new BindingList<Models.Candlestick>();
                _playView.Add(_playAll[0]);               // start with first candle
                _tickIndex = 1;                           // next index to add

                // Configure chart series bindings
                var sC = chart_ohlc.Series["series_candles"];
                var sV = chart_ohlc.Series["series_volume"];
                sC.XValueMember = nameof(Models.Candlestick.Date);
                sC.YValueMembers = string.Join(",", nameof(Models.Candlestick.High),
                                                       nameof(Models.Candlestick.Low),
                                                       nameof(Models.Candlestick.Open),
                                                       nameof(Models.Candlestick.Close));
                sV.XValueMember = nameof(Models.Candlestick.Date);
                sV.YValueMembers = nameof(Models.Candlestick.Volume);

                // Switch indexing based on Day/Week/Month
                bool isDayNow = _period.Equals("Day", StringComparison.OrdinalIgnoreCase);
                sC.IsXValueIndexed = isDayNow;
                sV.IsXValueIndexed = isDayNow;

                // Bind data to chart and normalize axes
                chart_ohlc.DataSource = _playView;
                chart_ohlc.DataBind();
                NormalizeAxes(_playAll);
                UpdateTitles(start, end);

                // Disable editing controls during animation
                this.dateTimePicker_start.Enabled = false;
                this.dateTimePicker_end.Enabled = false;
                this.button_load.Enabled = false;
                this.button_refresh.Enabled = false;

                // Resolve active recognizer for this run (and lock it for the whole playback)
                var recog = GetSelectedRecognizer(out var color);
                _activePatternColor = color;
                _activePattern = (comboBox_pattern.SelectedItem as string ?? "").Trim();

                // Build a Smart list for the full playback range
                var smartAll = new List<Models.SmartCandlestick>(_playAll.Count);
                for (int i = 0; i < _playAll.Count; i++)
                    smartAll.Add(new Models.SmartCandlestick(_playAll[i]));

                // Compute all hits once for this run
                _patternHits = (recog != null) ? recog.FindAll(smartAll) : new List<int>();

                // We will reveal arrows progressively as bars appear
                ClearPatternAnnotations();
                ClearPatternMarkers();

                // UI: disable pattern switching while running
                comboBox_pattern.Enabled = false;


                // Start ticker
                this.timer_ticker.Interval = this.trackBar_speed.Value;
                this.timer_ticker.Start();
            }
            else
            {
                // PAUSE mode
                _playing = false;                         // not playing
                this.button_playPause.Text = "Play";      // toggle text
                this.timer_ticker.Stop();                 // stop timer

                // Allow control editing again
                this.dateTimePicker_start.Enabled = true;
                this.dateTimePicker_end.Enabled = true;
                this.button_load.Enabled = true;
                this.button_refresh.Enabled = true;
                this.comboBox_pattern.Enabled = true;

            }
        }


        /// <summary>On each tick, append one more candle. Stop and restore UI when done.</summary>
        private void timer_ticker_Tick(object sender, EventArgs e)
        {
            if (!_playing || _playAll == null || _playView == null) return;

            // if done, stop and restore normal state
            if (_tickIndex >= _playAll.Count)
            {
                this.timer_ticker.Stop();
                _playing = false;
                this.button_playPause.Text = "Play";

                // re-enable inputs
                this.dateTimePicker_start.Enabled = true;
                this.dateTimePicker_end.Enabled = true;
                this.button_load.Enabled = true;
                this.button_refresh.Enabled = true;

                // leave the final state drawn (or call RefreshFromMainDates to rebind to full view)
                // RefreshFromMainDates();
                return;
            }

            // append next candle and rebind
            _playView.Add(_playAll[_tickIndex]);
            _tickIndex++;

            // Rebind so the new point appears
            chart_ohlc.DataBind();

            // Re-draw all annotations up to the bar we just showed
            RedrawAnnotationsUpToCurrentTick();



            // Reveal pattern annotation for the bar that just appeared (_tickIndex - 1)
            try
            {
                if (_patternHits != null && _tickIndex > 0)
                {
                    int justShown = _tickIndex - 1;               // index into _playAll
                    if (_patternHits.Contains(justShown))
                    {
                        // Add ONE arrow anchored by date of _playAll[justShown]
                        var sC = chart_ohlc.Series["series_candles"];
                        if (sC != null && sC.Points != null)
                        {
                            var sc = new Models.SmartCandlestick(_playAll[justShown]);
                            int pIdx = FindPointIndexByDate(sC, sc.Date);
                            if (pIdx >= 0)
                            {
                                // small marker + arrow using your existing styling
                                var dp = sC.Points[pIdx];
                                dp.MarkerStyle = MarkerStyle.Star4;
                                dp.MarkerSize = 10;
                                dp.MarkerColor = _activePatternColor;

                                // arrow anchored to candle body
                                double seg = (double)Math.Max(sc.range * 0.25m, 1.0m);
                                var priceArea = chart_ohlc.ChartAreas["chartArea_price"];
                                var arrow = new ArrowAnnotation
                                {
                                    AnchorDataPoint = dp,
                                    AxisX = priceArea.AxisX,
                                    AxisY = priceArea.AxisY,
                                    ClipToChartArea = priceArea.Name,
                                    IsSizeAlwaysRelative = false,
                                    LineColor = _activePatternColor,
                                    LineWidth = 3,
                                    ArrowSize = 8,
                                    Width = 0,
                                    X = dp.XValue,
                                    Y = (double)sc.bodyTop + seg * 0.6,
                                    Height = -seg
                                };
                                chart_ohlc.Annotations.Add(arrow);
                                chart_ohlc.Invalidate();
                            }
                        }
                    }
                }
            }
            catch { /* keep playback resilient */ }
        }

        private void RedrawAnnotationsUpToCurrentTick()
        {
            if (_patternHits == null || _tickIndex <= 0) return;

            // Clear and re-add everything up to current index
            chart_ohlc.Annotations.Clear();

            var sC = chart_ohlc.Series["series_candles"];
            var priceArea = chart_ohlc.ChartAreas["chartArea_price"];
            if (sC == null || sC.Points == null) return;

            // For each hit that is now visible, add marker + triangle again
            foreach (var hit in _patternHits)
            {
                if (hit < 0 || hit >= _tickIndex) continue;           // only the bars already revealed
                var sc = new Models.SmartCandlestick(_playAll[hit]);

                int pIdx = FindPointIndexByDate(sC, sc.Date);
                if (pIdx < 0) continue;

                var dp = sC.Points[pIdx];
                dp.MarkerStyle = MarkerStyle.Star4;
                dp.MarkerSize = 10;
                dp.MarkerColor = _activePatternColor;

                var tri = new TextAnnotation
                {
                    Text = "▲",
                    AnchorDataPoint = dp,
                    AxisX = priceArea.AxisX,
                    AxisY = priceArea.AxisY,
                    Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold),
                    ForeColor = _activePatternColor,
                    BackColor = System.Drawing.Color.Transparent,
                    AllowMoving = false,
                    ClipToChartArea = priceArea.Name,
                    AnchorAlignment = ContentAlignment.BottomCenter
                };
                chart_ohlc.Annotations.Add(tri);
            }

            chart_ohlc.Invalidate();
        }



        /// <summary>
        /// Handles the “Load…” button click in the child chart window.
        /// Opens a file dialog, lets the user pick a new stock CSV,
        /// updates this form’s symbol/period/source, and re-renders the chart.
        /// </summary>
        private void button_load_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog instance for selecting a CSV
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select stock CSV (SYMBOL-Day/Week/Month.csv)";   // dialog title
                                                                              // filter: All / Day / Week / Month
                                                                              // Set file filter to allow user to select Day/Week/Month CSVs specifically
                ofd.Filter =
                    "All CSV (*.csv)|*.csv|" +          // show all CSVs
                    "Day CSV (*-Day.csv)|*-Day.csv|" +  // only day files
                    "Week CSV (*-Week.csv)|*-Week.csv|" + // only week files
                    "Month CSV (*-Month.csv)|*-Month.csv"; // only month files

                // Default to showing all CSVs in the dropdown initially
                ofd.FilterIndex = 1;

                // Optional: set initial folder to the Stock Data folder in the app directory
                var stockDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stock Data");
                if (System.IO.Directory.Exists(stockDir))
                    ofd.InitialDirectory = stockDir; // open directly inside "Stock Data"


                ofd.Multiselect = false;                                       // only one file per Load action

                // Show dialog; abort if user clicks Cancel
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                // Parse stock symbol and period (Day/Week/Month) from filename
                var (sym, per) = ParseSymbolAndPeriodFromFileName(ofd.FileName);

                // Validate filename structure; warn if incorrect
                if (string.IsNullOrWhiteSpace(sym) || string.IsNullOrWhiteSpace(per))
                {
                    MessageBox.Show(this,
                        "Filename must look like ABBV-Day.csv / ABBV-Week.csv / ABBV-Month.csv.",
                        "Bad filename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;                                                     // stop if filename invalid
                }

                // Read CSV contents into list of Candlestick objects
                var candles = ReadCandlesticksFromFile(ofd.FileName);

                // Warn if file empty or unreadable
                if (candles.Count == 0)
                {
                    MessageBox.Show(this,
                        "No rows were read from the CSV.",
                        "Empty/Invalid CSV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Replace this form’s state with new data
                _symbol = sym;                                                 // update symbol name
                _period = per;                                                 // update period string
                _source.Clear();                                               // clear old list
                _source.AddRange(candles);                                     // copy new candles

                // Update window title and redraw chart
                this.Text = $"{_symbol}-{_period}";                            // caption reflects new stock
                ResetPlayback();   // new file cancels ongoing playback
                RefreshFromMainDates();                                        // refresh to show changes
            }
        }

        /// <summary>
        /// Extracts "SYMBOL" and "Day|Week|Month" parts from a filename like “ABBV-Day.csv”.
        /// Returns empty strings if parsing fails.
        /// </summary>
        private (string symbol, string period) ParseSymbolAndPeriodFromFileName(string filePath)
        {
            string fileName = System.IO.Path.GetFileName(filePath);            // e.g. ABBV-Day.csv
            string baseName = System.IO.Path.GetFileNameWithoutExtension(fileName); // ABBV-Day

            int dash = baseName.LastIndexOf('-');                              // locate last '-'
            if (dash <= 0 || dash >= baseName.Length - 1) return ("", "");     // validate position

            string symbol = baseName.Substring(0, dash).Trim().ToUpperInvariant(); // ABBV
            string periodRaw = baseName.Substring(dash + 1).Trim();                // Day

            // Normalize period token to expected values
            string period = periodRaw.Equals("Day", StringComparison.OrdinalIgnoreCase) ? "Day" :
                            periodRaw.Equals("Week", StringComparison.OrdinalIgnoreCase) ? "Week" :
                            periodRaw.Equals("Month", StringComparison.OrdinalIgnoreCase) ? "Month" : "";

            // Return empty tuple if either missing
            if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(period))
                return ("", "");

            return (symbol, period);                                           // success
        }

        /// <summary>
        /// Reads a stock CSV file (Daily, Weekly, or Monthly) into a list of Candlestick objects.
        /// Handles different delimiters (comma, semicolon, tab) and flexible headers such as “Adj Close”.
        /// </summary>
        /// <param name="path">Full path to the CSV file being read.</param>
        /// <returns>A List of Candlestick objects, sorted by ascending date.</returns>
        private List<Models.Candlestick> ReadCandlesticksFromFile(string path)
        {
            // Create an empty list that will hold all parsed candlestick objects
            var list = new List<Models.Candlestick>();

            // Open a StreamReader to read the file line by line
            using (var reader = new System.IO.StreamReader(path))
            {
                // Read the first line, which should contain column headers
                string header = reader.ReadLine() ?? "";

                // Split the header using common delimiters (, ; or tab)
                string[] rawCols = header.Split(new[] { ',', ';', '\t' });

                // Helper function to clean up and standardize column names
                string norm(string s) => s.Trim().Trim('"', '\'').ToUpperInvariant();

                // Build a dictionary to map normalized header names to their index positions
                var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < rawCols.Length; i++)
                {
                    string k = norm(rawCols[i]);                        // clean each column header
                    k = k.Replace("  ", " ").Replace("ADJ CLOSE", "ADJCLOSE"); // handle “Adj Close” variations
                    map[k] = i;                                         // store index in map
                }

                // Helper function: checks if a column name exists in the dictionary
                bool has(string key) => map.ContainsKey(key);

                // Helper to find an index, supporting fallback names
                int idxReq(string a, string b = null)
                {
                    if (has(a)) return map[a];                          // primary key found
                    if (!string.IsNullOrEmpty(b) && has(b)) return map[b]; // fallback key found
                    return -1;                                          // not found
                }

                // Identify the required columns (case-insensitive)
                int iDate = idxReq("DATE");                            // date column index
                int iOpen = idxReq("OPEN");                            // open price column index
                int iHigh = idxReq("HIGH");                            // high price column index
                int iLow = idxReq("LOW");                             // low price column index
                int iClose = idxReq("CLOSE");                           // close price column index
                int iVol = idxReq("VOLUME", "VOL");                   // volume column index (handles VOL)

                // If any critical column is missing, return an empty list
                if (iDate < 0 || iOpen < 0 || iHigh < 0 || iLow < 0 || iClose < 0 || iVol < 0)
                    return list;

                // Define an invariant culture for consistent numeric parsing
                var inv = System.Globalization.CultureInfo.InvariantCulture;

                string line; // line variable for reading CSV rows
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;       // skip empty lines

                    // Detect the proper delimiter based on header line
                    var parts = line.Split(header.Contains(";") ? ';' :
                                           header.Contains("\t") ? '\t' : ',');

                    // Helper: safely extract and trim a field
                    string get(int i) => i >= 0 && i < parts.Length
                        ? parts[i].Trim().Trim('"', '\'', ' ', '\t', '\r', '\n')
                        : "";

                    // Helper: remove commas in numbers for safe parsing (e.g., “1,234.56”)
                    string num(string s) => s.Replace(",", "");

                    // Parse each field into its proper type
                    if (!DateTime.TryParse(get(iDate), out var date)) continue;
                    if (!decimal.TryParse(num(get(iOpen)), System.Globalization.NumberStyles.Any, inv, out var open)) continue;
                    if (!decimal.TryParse(num(get(iHigh)), System.Globalization.NumberStyles.Any, inv, out var high)) continue;
                    if (!decimal.TryParse(num(get(iLow)), System.Globalization.NumberStyles.Any, inv, out var low)) continue;
                    if (!decimal.TryParse(num(get(iClose)), System.Globalization.NumberStyles.Any, inv, out var close)) continue;

                    // Try to parse volume; default to 0 if missing
                    long vol = 0;
                    long.TryParse(num(get(iVol)), System.Globalization.NumberStyles.Any, inv, out vol);

                    // Add the candlestick to the list
                    list.Add(new Models.Candlestick
                    {
                        Date = date,
                        Open = open,
                        High = high,
                        Low = low,
                        Close = close,
                        Volume = vol
                    });
                }
            }

            // Sort all candlesticks by date from oldest to newest
            list.Sort((a, b) => a.Date.CompareTo(b.Date));

            // Return the final list of parsed candlesticks
            return list;
        }

        /// Find the DataPoint index whose XValue date == given date (Day-level compare).
        private int FindPointIndexByDate(Series s, DateTime date)
        {
            if (s == null || s.Points == null) return -1;                 // guard
            var target = date.Date;                                       // compare at day resolution
            for (int j = 0; j < s.Points.Count; j++)                      // scan all points
            {
                var pj = s.Points[j];
                var d = DateTime.FromOADate(pj.XValue).Date;             // convert XValue to Date
                if (d == target) return j;                                // found matching candle
            }
            return -1;                                                    // not found
        }

        

        /// <summary>
        /// Load event (Designer-wired). Nothing needed—constructor already configures & paints the chart.
        /// </summary>
        private void Form_Chart_Load(object sender, EventArgs e)
        {
            // Populate pattern list once (safe to do at load time)
            if (comboBox_pattern.Items.Count == 0)
            {
                comboBox_pattern.Items.AddRange(new object[]
                {
                    "Doji",
                    "Engulfing (Bullish)",
                    "Engulfing (Bearish)",
                    "Hammer",
                    "Inverted Hammer"
                });
                comboBox_pattern.SelectedIndex = 0;      // default selection is OK
            }
            comboBox_pattern.Enabled = true;             // will be disabled while playing


        }

        private StockAnalysis.WinForms.Models.Recognizers.Recognizer
            GetSelectedRecognizer(out System.Drawing.Color arrowColor)
        {
            arrowColor = System.Drawing.Color.Magenta; // default
            var c = (comboBox_pattern.SelectedItem as string ?? "").Trim().ToLowerInvariant();

            if (c == "doji")
            {
                arrowColor = System.Drawing.Color.Magenta;
                return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Doji();
            }
            if (c.Contains("engulfing") && c.Contains("bullish"))
            {
                arrowColor = System.Drawing.Color.DeepSkyBlue;
                return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Engulfing(true);
            }
            if (c.Contains("engulfing") && c.Contains("bearish"))
            {
                arrowColor = System.Drawing.Color.OrangeRed;
                return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Engulfing(false);
            }
            if (c == "hammer")
            {
                arrowColor = System.Drawing.Color.LimeGreen;
                return new StockAnalysis.WinForms.Models.Recognizers.Recognizer_Hammer(true);
            }
            if (c == "inverted hammer")
            {
                MessageBox.Show("Inverted Hammer is not implemented yet.", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return null;
        }

    }
}
