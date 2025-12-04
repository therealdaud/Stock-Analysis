namespace StockAnalysis.WinForms
{
    partial class Form_Chart
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chart_ohlc = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button_refresh = new System.Windows.Forms.Button();
            this.panel_top = new System.Windows.Forms.Panel();
            this.button_load = new System.Windows.Forms.Button();
            this.button_playPause = new System.Windows.Forms.Button();
            this.comboBox_pattern = new System.Windows.Forms.ComboBox();
            this.trackBar_speed = new System.Windows.Forms.TrackBar();
            this.textBox_ms = new System.Windows.Forms.TextBox();
            this.dateTimePicker_end = new System.Windows.Forms.DateTimePicker();
            this.label_to = new System.Windows.Forms.Label();
            this.dateTimePicker_start = new System.Windows.Forms.DateTimePicker();
            this.label_from = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chart_ohlc)).BeginInit();
            this.panel_top.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_speed)).BeginInit();
            this.SuspendLayout();
            // 
            // chart_ohlc
            // 
            chartArea1.Name = "chartArea_price";
            chartArea2.AlignWithChartArea = "chartArea_price";
            chartArea2.Name = "chartArea_volume";
            this.chart_ohlc.ChartAreas.Add(chartArea1);
            this.chart_ohlc.ChartAreas.Add(chartArea2);
            this.chart_ohlc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_ohlc.Location = new System.Drawing.Point(0, 0);
            this.chart_ohlc.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chart_ohlc.Name = "chart_ohlc";
            series1.ChartArea = "chartArea_price";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series1.CustomProperties = "PriceUpColor=Lime, PriceDownColor=Red";
            series1.Name = "series_candles";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "chartArea_volume";
            series2.Name = "series_volume";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            this.chart_ohlc.Series.Add(series1);
            this.chart_ohlc.Series.Add(series2);
            this.chart_ohlc.Size = new System.Drawing.Size(1200, 689);
            this.chart_ohlc.TabIndex = 0;
            this.chart_ohlc.Text = "chart1";
            // 
            // button_refresh
            // 
            this.button_refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_refresh.AutoSize = true;
            this.button_refresh.Location = new System.Drawing.Point(1037, 7);
            this.button_refresh.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(85, 32);
            this.button_refresh.TabIndex = 1;
            this.button_refresh.Text = "Refresh";
            this.button_refresh.UseVisualStyleBackColor = true;
            this.button_refresh.Click += new System.EventHandler(this.button_refresh_Click);
            // 
            // panel_top
            // 
            this.panel_top.Controls.Add(this.button_refresh);
            this.panel_top.Controls.Add(this.button_load);
            this.panel_top.Controls.Add(this.button_playPause);
            this.panel_top.Controls.Add(this.comboBox_pattern);
            this.panel_top.Controls.Add(this.trackBar_speed);
            this.panel_top.Controls.Add(this.textBox_ms);
            this.panel_top.Controls.Add(this.dateTimePicker_end);
            this.panel_top.Controls.Add(this.label_to);
            this.panel_top.Controls.Add(this.dateTimePicker_start);
            this.panel_top.Controls.Add(this.label_from);
            this.panel_top.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_top.Location = new System.Drawing.Point(0, 0);
            this.panel_top.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel_top.Name = "panel_top";
            this.panel_top.Size = new System.Drawing.Size(1200, 62);
            this.panel_top.TabIndex = 2;
            // 
            // button_load
            // 
            this.button_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_load.AutoSize = true;
            this.button_load.Location = new System.Drawing.Point(950, 7);
            this.button_load.Margin = new System.Windows.Forms.Padding(4);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(80, 32);
            this.button_load.TabIndex = 4;
            this.button_load.Text = "Load…";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // button_playPause
            // 
            this.button_playPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_playPause.AutoSize = true;
            this.button_playPause.Location = new System.Drawing.Point(862, 7);
            this.button_playPause.Margin = new System.Windows.Forms.Padding(4);
            this.button_playPause.Name = "button_playPause";
            this.button_playPause.Size = new System.Drawing.Size(80, 32);
            this.button_playPause.TabIndex = 5;
            this.button_playPause.Text = "Play";
            this.button_playPause.UseVisualStyleBackColor = true;
            this.button_playPause.Click += new System.EventHandler(this.button_playPause_Click);
            // 
            // comboBox_pattern
            // 
            this.comboBox_pattern.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_pattern.FormattingEnabled = true;
            this.comboBox_pattern.Items.AddRange(new object[] {
            "— Pattern —",
            "Doji",
            "Doji (Dragonfly)",
            "Doji (Gravestone)",
            "Marubozu",
            "Hammer",
            "Inverted Hammer",
            "Engulfing",
            "Engulfing (Bullish)",
            "Engulfing (Bearish)",
            "Harami",
            "Harami (Bullish)",
            "Harami (Bearish)"});
            this.comboBox_pattern.Location = new System.Drawing.Point(733, 12);
            this.comboBox_pattern.Name = "comboBox_pattern";
            this.comboBox_pattern.Size = new System.Drawing.Size(110, 24);
            this.comboBox_pattern.TabIndex = 8;
            this.comboBox_pattern.SelectedIndexChanged += new System.EventHandler(this.comboBox_pattern_SelectedIndexChanged);
            // 
            // trackBar_speed
            // 
            this.trackBar_speed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar_speed.LargeChange = 200;
            this.trackBar_speed.Location = new System.Drawing.Point(503, 2);
            this.trackBar_speed.Margin = new System.Windows.Forms.Padding(4);
            this.trackBar_speed.Maximum = 2000;
            this.trackBar_speed.Minimum = 100;
            this.trackBar_speed.Name = "trackBar_speed";
            this.trackBar_speed.Size = new System.Drawing.Size(213, 56);
            this.trackBar_speed.SmallChange = 100;
            this.trackBar_speed.TabIndex = 6;
            this.trackBar_speed.TickFrequency = 100;
            this.trackBar_speed.Value = 1000;
            this.trackBar_speed.Scroll += new System.EventHandler(this.trackBar_speed_Scroll);
            // 
            // textBox_ms
            // 
            this.textBox_ms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_ms.Location = new System.Drawing.Point(428, 10);
            this.textBox_ms.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_ms.Name = "textBox_ms";
            this.textBox_ms.Size = new System.Drawing.Size(57, 22);
            this.textBox_ms.TabIndex = 7;
            this.textBox_ms.Text = "1000";
            this.textBox_ms.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_ms.TextChanged += new System.EventHandler(this.textBox_ms_TextChanged);
            // 
            // dateTimePicker_end
            // 
            this.dateTimePicker_end.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker_end.Location = new System.Drawing.Point(237, 10);
            this.dateTimePicker_end.Margin = new System.Windows.Forms.Padding(4);
            this.dateTimePicker_end.Name = "dateTimePicker_end";
            this.dateTimePicker_end.Size = new System.Drawing.Size(127, 22);
            this.dateTimePicker_end.TabIndex = 3;
            // 
            // label_to
            // 
            this.label_to.AutoSize = true;
            this.label_to.Location = new System.Drawing.Point(200, 15);
            this.label_to.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_to.Name = "label_to";
            this.label_to.Size = new System.Drawing.Size(27, 16);
            this.label_to.TabIndex = 2;
            this.label_to.Text = "To:";
            // 
            // dateTimePicker_start
            // 
            this.dateTimePicker_start.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker_start.Location = new System.Drawing.Point(64, 10);
            this.dateTimePicker_start.Margin = new System.Windows.Forms.Padding(4);
            this.dateTimePicker_start.Name = "dateTimePicker_start";
            this.dateTimePicker_start.Size = new System.Drawing.Size(127, 22);
            this.dateTimePicker_start.TabIndex = 1;
            // 
            // label_from
            // 
            this.label_from.AutoSize = true;
            this.label_from.Location = new System.Drawing.Point(11, 15);
            this.label_from.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_from.Name = "label_from";
            this.label_from.Size = new System.Drawing.Size(41, 16);
            this.label_from.TabIndex = 0;
            this.label_from.Text = "From:";
            // 
            // Form_Chart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 689);
            this.Controls.Add(this.panel_top);
            this.Controls.Add(this.chart_ohlc);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(1194, 678);
            this.Name = "Form_Chart";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_Chart";
            this.Load += new System.EventHandler(this.Form_Chart_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart_ohlc)).EndInit();
            this.panel_top.ResumeLayout(false);
            this.panel_top.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_speed)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart_ohlc;
        private System.Windows.Forms.Button button_refresh;
        private System.Windows.Forms.Panel panel_top;
        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.DateTimePicker dateTimePicker_start;
        private System.Windows.Forms.DateTimePicker dateTimePicker_end;
        private System.Windows.Forms.Label label_from;
        private System.Windows.Forms.Label label_to;
        private System.Windows.Forms.TrackBar trackBar_speed;    // 100–2000 ms
        private System.Windows.Forms.TextBox textBox_ms;         // shows ms value
        private System.Windows.Forms.Button button_playPause;    // Play/Pause
        private System.Windows.Forms.ComboBox comboBox_pattern;   // pattern selector (UI only)


    }
}