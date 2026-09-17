using System;
using System.Linq;
using System.Web.UI;
using MasterAntiqueRepair;
using Newtonsoft.Json;

public partial class Metrics : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RepairAuthHelper.RequireRole(Response, "Manager"))
        {
            return;
        }

        if (IsPostBack)
        {
            return;
        }

        using (var service = new MetricsService())
        {
            var summary = service.GetSummary();

            ChartsPanel.Visible = summary.HasData;
            NoDataPanel.Visible = !summary.HasData;

            if (!summary.HasData)
            {
                return;
            }

            PeriodTotalLiteral.Text = summary.PeriodTotal.ToString();
            AveragePerDayLiteral.Text = summary.AveragePerDay.ToString("0.0");
            AverageCreatedPerDayLiteral.Text = summary.AverageCreatedPerDay.ToString("0.0");
            CreatedClosedRatioLiteral.Text = summary.CreatedClosedRatio.ToString("0.00") + " : 1";
            MostProductiveDayLiteral.Text = summary.MostProductiveDay.ToString("MMM d") + " (" + summary.MostProductiveDayCount + " closed)";
            QuietDaysLiteral.Text = summary.QuietDays + " of " + summary.Days.Count;
            BusiestEmployeeLiteral.Text = summary.BusiestEmployeeLabel != null
                ? summary.BusiestEmployeeLabel + " (" + summary.BusiestEmployeeTotal + " closed)"
                : "(none)";

            TotalCommentsLiteral.Text = summary.TotalComments.ToString();
            AverageCommentsPerDayLiteral.Text = summary.AverageCommentsPerDay.ToString("0.0");
            EmployeeCommentsLiteral.Text = summary.EmployeeCommentCount.ToString();
            CustomerCommentsLiteral.Text = summary.CustomerCommentCount.ToString();
            MostCommentedTicketLiteral.Text = summary.MostCommentedTicketId.HasValue
                ? "#" + summary.MostCommentedTicketId + " (" + summary.MostCommentedTicketCount + " comments)"
                : "(none)";

            var chartData = new
            {
                dayLabels = summary.Days.Select(d => d.ToString("MMM d")).ToList(),
                employeeSeries = summary.EmployeeSeries.Select(s => new { label = s.Label, data = s.Data, total = s.Total }).ToList(),
                periodTotal = summary.PeriodTotal,
                customerCommentCounts = summary.CustomerCommentCounts.Select(c => new { label = c.Label, count = c.Count }).ToList(),
                employeeCommentCounts = summary.EmployeeCommentCounts.Select(c => new { label = c.Label, count = c.Count }).ToList()
            };

            // Embedded directly into a <script> block (see Metrics.aspx) - JsonConvert
            // already escapes quotes/backslashes for a valid JS string, but a literal
            // "</script>" inside a value (e.g. an employee name) would still terminate
            // the block early, so that one extra escape is added on top.
            ChartDataLiteral.Text = JsonConvert.SerializeObject(chartData).Replace("</", "<\\/");
        }
    }
}
