# 📊 Stock Analysis WinForms Application

## 🧩 Overview
**StockAnalysis.WinForms** is a C# Windows Forms application that visualizes stock market data in both **tabular** and **candlestick chart** formats.  
The project demonstrates concepts of **data binding, file parsing, charting, and OOP pattern recognition**, aligning with the requirements discussed in class.

Users can load **Daily, Weekly, or Monthly** stock `.csv` data files, view their prices and volume, and interactively analyze candlestick patterns such as **Doji**, **Hammer**, **Marubozu**, **Engulfing**, and **Harami**.

---

## 🎯 Project Goals
- Load and display OHLC (Open, High, Low, Close) stock data and Volume.
- Visualize data using **Candlestick** and **Column** charts.
- Support multiple stocks simultaneously (each in its own form).
- Implement data binding, normalization, filtering, and update functionality.
- Recognize and annotate 1- and 2-candlestick patterns.
- Follow naming and commenting conventions for readability and maintainability.

---

## 🧰 Tech Stack

| Component | Description |
|------------|-------------|
| **Language** | C# (.NET Framework / WinForms) |
| **Framework** | Windows Forms Application |
| **IDE** | Visual Studio 2022 |
| **Charting** | `System.Windows.Forms.DataVisualization.Charting` |
| **File Format** | CSV stock data files (`xxx-Day.csv`, `xxx-Week.csv`, `xxx-Month.csv`) |

---

## 📂 Folder Structure
StockAnalysis.WinForms/

│

├── Stock Data/ # Folder containing all .csv files

│ ├── AAPL-Day.csv

│ ├── AAPL-Week.csv

│ └── AAPL-Month.csv

│

├── FormMain.cs # Main form with controls and chart

├── Candlestick.cs # Defines base candlestick structure

├── SmartCandlestick.cs # Derived class with pattern logic

├── Recognizer.cs # Abstract base class for recognizers

├── Recognizer_Doji.cs # Concrete recognizer classes

├── Recognizer_Hammer.cs

├── Recognizer_Marubozu.cs

├── Recognizer_Engulfing.cs

├── Recognizer_Harami.cs

├── ChartHelper.cs # Handles normalization, updating, and display

├── Program.cs # Application entry point

├── README.md

└── StockAnalysis.WinForms.sln



---

## 🖥️ Core Features

### 📁 Load & Display Data
- Loads stock data (`.csv`) from the `Stock Data` folder.
- Supports multiple files: `xxx-Day.csv`, `xxx-Week.csv`, `xxx-Month.csv`.
- Uses **OpenFileDialog** for symbol and period selection.
- Displays OHLC data in the correct chronological order.

### 📈 Candlestick & Volume Chart
- Candlestick chart shows price movements.
- Column chart below displays trade volume.
- Uses **red** for bearish (down) candles and **green/lime** for bullish (up) candles.
- Automatically normalizes chart range with ±2% margins.

### 🔄 Filtering and Updates
- Start and End Date selection using DateTimePickers.
- “**Refresh**” button dynamically reloads filtered data without reopening files.
- Multiple stocks can be displayed in separate chart forms.

### ⏱️ Real-Time Simulation
- Simulates a **live ticker** by progressively displaying candlesticks.
- Controlled via a ScrollBar (100 ms – 2000 ms) and synchronized with a TextBox.
- Optional 2-way binding between ScrollBar and TextBox for live adjustment.

### 🔍 Pattern Recognition
- Supports recognition of the following candlestick patterns:
  - **1-Candlestick Patterns:** Doji (incl. Dragonfly, Gravestone), Marubozu, Hammer, Inverted Hammer
  - **2-Candlestick Patterns:** Engulfing, Harami
- Patterns are detected using specialized recognizer classes derived from the abstract `Recognizer` base class.
- Identified patterns are annotated on the chart using `RectangleAnnotation`, `LineAnnotation`, or `ArrowAnnotation`.
- User can select a pattern to highlight via a **ComboBox**.

---

## 🧠 Object-Oriented Design

| Class | Responsibility |
|-------|----------------|
| `Candlestick` | Stores OHLC and Volume data. |
| `SmartCandlestick` | Extends Candlestick; adds computed metrics (range, tails, body, bullish/bearish). |
| `Recognizer` | Abstract class defining structure for pattern recognizers. |
| `Recognizer_xxx` | Concrete recognizers (e.g., `Recognizer_Doji`) each implementing `recognize()` logic. |
| `ChartHelper` | Handles normalization, axis scaling, and dynamic updates. |
| `FormMain` | Manages UI elements, event handling, and interactivity. |

---

## 🧮 Key Functions

| Method | Description |
|---------|-------------|
| `readCandlesticksFromFile()` | Reads CSV files and converts rows into Candlestick objects. |
| `filterCandlesticks()` | Filters data between selected start and end dates. |
| `normalizeChart()` | Adjusts Y-axis to utilize full visual range. |
| `displayCandlesticks()` | Binds filtered list to the chart control. |
| `update()` | Refreshes charts based on user-selected filters. |
| `recognizePatterns()` | Invokes recognizer classes to mark candlestick patterns. |

Each method follows C# XML documentation comments (`///`) with summaries, parameters, and return value descriptions.

---

## 🧾 Naming & Commenting Conventions
- **Controls:** `controlType_action` (e.g., `button_loadData`, `chart_displayCandles`)
- **Methods:** PascalCase (e.g., `ReadCandlesticksFromFile`)
- **Comments:**
  - `///` for method-level XML documentation.
  - `//` or `/* */` for line-level logic explanations.
- Each custom line of code is commented.
- Links to external resources (if used) are cited within comments.

---

## 🧩 Example Chart Title Format
When displaying data for `ABBV-Day` between March 1 – April 1 2023:


ABBV-Day
3/1/2023 – 4/1/2023


---

## 🪜 How to Run
1. Open `StockAnalysis.WinForms.sln` in **Visual Studio 2022**.
2. Ensure the `Stock Data` folder is in the same directory as your `.sln`.
3. Build the project (**Build → Clean Solution → Build Solution**).
4. Run with **F5** or click **Start Debugging**.
5. Use the “Load Data” button to select a `.csv` file and visualize results.
6. Adjust start and end dates, pattern type, or ticker speed as needed.

---

## 🧠 Pattern Recognition Workflow
1. Load stock data and instantiate `SmartCandlestick` objects.
2. Each recognizer (`Recognizer_xxx`) runs its `recognize()` method over the dataset.
3. Detected patterns are visually annotated on the chart.
4. The ComboBox allows selecting and highlighting any recognized pattern interactively.

---

## 📚 References & Learning Resources
- [C# Syntax – W3Schools](https://www.w3schools.com/cs/)
- [Stock Chart & Candlestick Tutorial – FoxLearn (YouTube)](https://www.youtube.com/watch?v=qX5nD2Ahy40)
- [Data Binding with WinForms – Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
- [Detailed Data Binding Tutorial – CodeProject](https://www.codeproject.com/Articles/24656/A-Detailed-Data-Binding-Tutorial)

---

## 🧑‍💻 Author
**Daud Ahmad Nisar**  
Computer Science Student — University of South Florida  
💡 Focus Areas: AI/ML, Cloud Computing, Data Analytics  
📫 [LinkedIn](https://www.linkedin.com/) · [GitHub](https://github.com/)

---

## 📜 License
This project is developed for academic purposes. Redistribution is allowed under the MIT License.

---

## 🌟 Extra Features & Future Enhancements
- Optional **Dark Mode UI**
- Live API integration (Yahoo Finance / Alpha Vantage)
- Export charts as **PNG/PDF**
- Pattern summary statistics and performance dashboard
- Real-time alerting for pattern formations

---

