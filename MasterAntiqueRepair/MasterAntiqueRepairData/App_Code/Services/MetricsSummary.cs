using System;
using System.Collections.Generic;

namespace MasterAntiqueRepair
{
    public class MetricsSummary
    {
        public bool HasData { get; set; }
        public List<DateTime> Days { get; set; }
        public List<EmployeeSeriesEntry> EmployeeSeries { get; set; }
        public int PeriodTotal { get; set; }
        public double AveragePerDay { get; set; }
        public double AverageCreatedPerDay { get; set; }
        public int CreatedInPeriod { get; set; }
        // Null when CreatedInPeriod is 0 - a completed ticket can have been submitted
        // before the window started, so "closed this period" doesn't guarantee "created
        // this period" > 0 the way it used to guarantee the old ratio's denominator.
        public double? ClosedCreatedRatio { get; set; }
        public DateTime MostProductiveDay { get; set; }
        public int MostProductiveDayCount { get; set; }
        public int QuietDays { get; set; }
        public string BusiestEmployeeLabel { get; set; }
        public int BusiestEmployeeTotal { get; set; }
        public int TotalComments { get; set; }
        public double AverageCommentsPerDay { get; set; }
        public int EmployeeCommentCount { get; set; }
        public int CustomerCommentCount { get; set; }
        public int? MostCommentedTicketId { get; set; }
        public int MostCommentedTicketCount { get; set; }
        public List<NamedCount> CustomerCommentCounts { get; set; }
        public List<NamedCount> EmployeeCommentCounts { get; set; }
    }

    public class EmployeeSeriesEntry
    {
        public string Label { get; set; }
        public List<int> Data { get; set; }
        public int Total { get; set; }
    }

    public class NamedCount
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }
}
