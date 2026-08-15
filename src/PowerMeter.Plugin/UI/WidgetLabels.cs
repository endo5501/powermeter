namespace PowerMeter.Plugin.UI
{
    /// <summary>ウィジットに出す固定文字列。</summary>
    public class WidgetLabels
    {
        private WidgetLabels(
            string title,
            string generation,
            string demand,
            string capacity,
            string utilization,
            string satisfaction,
            string charge,
            string discharge,
            string netCharge,
            string stored,
            string planet,
            string star,
            string global,
            string noData)
        {
            Title = title;
            Generation = generation;
            Demand = demand;
            Capacity = capacity;
            Utilization = utilization;
            Satisfaction = satisfaction;
            Charge = charge;
            Discharge = discharge;
            NetCharge = netCharge;
            Stored = stored;
            Planet = planet;
            Star = star;
            Global = global;
            NoData = noData;
        }

        public static readonly WidgetLabels Japanese = new WidgetLabels(
            title: "電力",
            generation: "発電",
            demand: "需要",
            capacity: "容量",
            utilization: "使用率",
            satisfaction: "充足",
            charge: "充電",
            discharge: "放電",
            netCharge: "充放電",
            stored: "蓄電",
            planet: "惑星",
            star: "星系",
            global: "全星系",
            noData: "—");

        public static readonly WidgetLabels English = new WidgetLabels(
            title: "Power",
            generation: "Gen",
            demand: "Demand",
            capacity: "Cap",
            utilization: "Load",
            satisfaction: "Sat",
            charge: "Charge",
            discharge: "Discharge",
            netCharge: "Net",
            stored: "Stored",
            planet: "Planet",
            star: "System",
            global: "All",
            noData: "—");

        public string Title { get; }

        public string Generation { get; }

        public string Demand { get; }

        public string Capacity { get; }

        public string Utilization { get; }

        public string Satisfaction { get; }

        public string Charge { get; }

        public string Discharge { get; }

        public string NetCharge { get; }

        public string Stored { get; }

        public string Planet { get; }

        public string Star { get; }

        public string Global { get; }

        /// <summary>集計対象が無いときに数値の代わりに出す文字列。</summary>
        public string NoData { get; }

        public static WidgetLabels For(bool japanese)
        {
            return japanese ? Japanese : English;
        }
    }
}
