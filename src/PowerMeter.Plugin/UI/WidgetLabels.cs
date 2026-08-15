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
            string satisfaction,
            string planet,
            string star,
            string global,
            string noData)
        {
            Title = title;
            Generation = generation;
            Demand = demand;
            Capacity = capacity;
            Satisfaction = satisfaction;
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
            satisfaction: "充足",
            planet: "惑星",
            star: "星系",
            global: "全星系",
            noData: "—");

        public static readonly WidgetLabels English = new WidgetLabels(
            title: "Power",
            generation: "Gen",
            demand: "Demand",
            capacity: "Cap",
            satisfaction: "Sat",
            planet: "Planet",
            star: "System",
            global: "All",
            noData: "—");

        public string Title { get; }

        public string Generation { get; }

        public string Demand { get; }

        public string Capacity { get; }

        public string Satisfaction { get; }

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
