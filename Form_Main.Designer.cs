namespace StockAnalysis.WinForms
{
    partial class Form_Main
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.panel_controls = new System.Windows.Forms.Panel();
            this.label_status = new System.Windows.Forms.Label();
            this.button_refresh = new System.Windows.Forms.Button();
            this.button_loadData = new System.Windows.Forms.Button();
            this.dateTimePicker_end = new System.Windows.Forms.DateTimePicker();
            this.label_end = new System.Windows.Forms.Label();
            this.dateTimePicker_start = new System.Windows.Forms.DateTimePicker();
            this.label_start = new System.Windows.Forms.Label();
            this.combobox_period = new System.Windows.Forms.ComboBox();
            this.label_period = new System.Windows.Forms.Label();
            this.textbox_symbol = new System.Windows.Forms.TextBox();
            this.label_symbol = new System.Windows.Forms.Label();
            this.chart_ohlc = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel_controls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_ohlc)).BeginInit();
            this.SuspendLayout();
            // 
            // panel_controls
            // 
            this.panel_controls.Controls.Add(this.label_status);
            this.panel_controls.Controls.Add(this.button_refresh);
            this.panel_controls.Controls.Add(this.button_loadData);
            this.panel_controls.Controls.Add(this.dateTimePicker_end);
            this.panel_controls.Controls.Add(this.label_end);
            this.panel_controls.Controls.Add(this.dateTimePicker_start);
            this.panel_controls.Controls.Add(this.label_start);
            this.panel_controls.Controls.Add(this.combobox_period);
            this.panel_controls.Controls.Add(this.label_period);
            this.panel_controls.Controls.Add(this.textbox_symbol);
            this.panel_controls.Controls.Add(this.label_symbol);
            this.panel_controls.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_controls.Location = new System.Drawing.Point(0, 0);
            this.panel_controls.Name = "panel_controls";
            this.panel_controls.Size = new System.Drawing.Size(1182, 90);
            this.panel_controls.TabIndex = 0;
            // 
            // label_status
            // 
            this.label_status.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(921, 13);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(48, 16);
            this.label_status.TabIndex = 10;
            this.label_status.Text = "Ready";
            // 
            // button_refresh
            // 
            this.button_refresh.Location = new System.Drawing.Point(810, 8);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(80, 28);
            this.button_refresh.TabIndex = 9;
            this.button_refresh.Text = "Refresh";
            this.button_refresh.UseVisualStyleBackColor = true;
            this.button_refresh.Click += new System.EventHandler(this.button_refresh_Click);
            // 
            // button_loadData
            // 
            this.button_loadData.Location = new System.Drawing.Point(710, 8);
            this.button_loadData.Name = "button_loadData";
            this.button_loadData.Size = new System.Drawing.Size(95, 28);
            this.button_loadData.TabIndex = 8;
            this.button_loadData.Text = "Open CSV(s)…";
            this.button_loadData.UseVisualStyleBackColor = true;
            this.button_loadData.Click += new System.EventHandler(this.button_loadData_Click);
            // 
            // dateTimePicker_end
            // 
            this.dateTimePicker_end.Location = new System.Drawing.Point(555, 10);
            this.dateTimePicker_end.Name = "dateTimePicker_end";
            this.dateTimePicker_end.Size = new System.Drawing.Size(140, 22);
            this.dateTimePicker_end.TabIndex = 7;
            // 
            // label_end
            // 
            this.label_end.AutoSize = true;
            this.label_end.Location = new System.Drawing.Point(515, 14);
            this.label_end.Name = "label_end";
            this.label_end.Size = new System.Drawing.Size(34, 16);
            this.label_end.TabIndex = 6;
            this.label_end.Text = "End:";
            // 
            // dateTimePicker_start
            // 
            this.dateTimePicker_start.Location = new System.Drawing.Point(365, 10);
            this.dateTimePicker_start.Name = "dateTimePicker_start";
            this.dateTimePicker_start.Size = new System.Drawing.Size(140, 22);
            this.dateTimePicker_start.TabIndex = 5;
            // 
            // label_start
            // 
            this.label_start.AutoSize = true;
            this.label_start.Location = new System.Drawing.Point(320, 14);
            this.label_start.Name = "label_start";
            this.label_start.Size = new System.Drawing.Size(37, 16);
            this.label_start.TabIndex = 4;
            this.label_start.Text = "Start:";
            // 
            // combobox_period
            // 
            this.combobox_period.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_period.FormattingEnabled = true;
            this.combobox_period.Items.AddRange(new object[] {
            "Day",
            "Week",
            "Month"});
            this.combobox_period.Location = new System.Drawing.Point(220, 10);
            this.combobox_period.Name = "combobox_period";
            this.combobox_period.Size = new System.Drawing.Size(90, 24);
            this.combobox_period.TabIndex = 3;
            // 
            // label_period
            // 
            this.label_period.AutoSize = true;
            this.label_period.Location = new System.Drawing.Point(165, 14);
            this.label_period.Name = "label_period";
            this.label_period.Size = new System.Drawing.Size(50, 16);
            this.label_period.TabIndex = 2;
            this.label_period.Text = "Period:";
            // 
            // textbox_symbol
            // 
            this.textbox_symbol.Location = new System.Drawing.Point(75, 10);
            this.textbox_symbol.Name = "textbox_symbol";
            this.textbox_symbol.Size = new System.Drawing.Size(80, 22);
            this.textbox_symbol.TabIndex = 1;
            this.textbox_symbol.Text = "AAPL";
            // 
            // label_symbol
            // 
            this.label_symbol.AutoSize = true;
            this.label_symbol.Location = new System.Drawing.Point(12, 14);
            this.label_symbol.Name = "label_symbol";
            this.label_symbol.Size = new System.Drawing.Size(56, 16);
            this.label_symbol.TabIndex = 0;
            this.label_symbol.Text = "Symbol:";
            // 
            // chart_ohlc
            // 
            chartArea2.Name = "ChartArea1";
            this.chart_ohlc.ChartAreas.Add(chartArea2);
            this.chart_ohlc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_ohlc.Location = new System.Drawing.Point(0, 90);
            this.chart_ohlc.Name = "chart_ohlc";
            series2.ChartArea = "ChartArea1";
            series2.Name = "Series1";
            this.chart_ohlc.Series.Add(series2);
            this.chart_ohlc.Size = new System.Drawing.Size(1182, 663);
            this.chart_ohlc.TabIndex = 2;
            this.chart_ohlc.Text = "chart1";
            title2.Name = "Title1";
            this.chart_ohlc.Titles.Add(title2);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 753);
            this.Controls.Add(this.chart_ohlc);
            this.Controls.Add(this.panel_controls);
            this.Name = "Form_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stock Analysis — Project 1";
            this.Load += new System.EventHandler(this.Form_Main_Load);
            this.panel_controls.ResumeLayout(false);
            this.panel_controls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_ohlc)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_controls;
        private System.Windows.Forms.Label label_symbol;
        private System.Windows.Forms.TextBox textbox_symbol;
        private System.Windows.Forms.Label label_period;
        private System.Windows.Forms.ComboBox combobox_period;
        private System.Windows.Forms.Label label_start;
        private System.Windows.Forms.DateTimePicker dateTimePicker_start;
        private System.Windows.Forms.DateTimePicker dateTimePicker_end;
        private System.Windows.Forms.Label label_end;
        private System.Windows.Forms.Button button_loadData;
        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.Button button_refresh;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_ohlc;
    }
}

