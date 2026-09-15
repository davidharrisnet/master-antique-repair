using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        using (var db = new RepairShopContext())
        {
            var manager = new Manager();
            var completedTickets = manager.GetCompletedTickets(db);

            ChartsPanel.Visible = completedTickets.Count > 0;
            NoDataPanel.Visible = !ChartsPanel.Visible;

            if (!ChartsPanel.Visible)
            {
                return;
            }

            // Rolling 7-day window ending today, zero-filled so every day in the window
            // appears even with no completions - clamped so it never reaches back before
            // the earliest completion on record (no point padding empty days that predate
            // any real data).
            var today = DateTime.Now.Date;
            var earliestCompletionDay = completedTickets.Min(t => t.CompletedDate.Value.Date);
            var windowStart = today.AddDays(-6);
            if (windowStart < earliestCompletionDay)
            {
                windowStart = earliestCompletionDay;
            }

            var days = new List<DateTime>();
            for (var d = windowStart; d <= today; d = d.AddDays(1))
            {
                days.Add(d);
            }

            var employeeNames = completedTickets
                .Where(t => t.User != null)
                .Select(t => t.User.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var employeeSeries = employeeNames.Select(name =>
            {
                var data = days.Select(d => completedTickets.Count(t =>
                    t.User != null && t.User.Name == name && t.CompletedDate.Value.Date == d)).ToList();
                return new
                {
                    label = name,
                    data = data,
                    total = data.Sum()
                };
            }).ToList();

            var periodTotal = completedTickets.Count(t =>
                t.CompletedDate.Value.Date >= windowStart && t.CompletedDate.Value.Date <= today);

            PeriodTotalLiteral.Text = periodTotal.ToString();
            AveragePerDayLiteral.Text = ((double)periodTotal / days.Count).ToString("0.0");

            // .Date isn't supported in LINQ to Entities (unlike completedTickets above,
            // which is already an in-memory list by this point) - compare against the
            // window as a plain DateTime range instead, so this still runs as a real SQL
            // COUNT rather than pulling every ticket into memory first.
            var windowEndExclusive = today.AddDays(1);
            var createdInPeriod = db.Tickets.Count(t =>
                t.SubmittedDate.HasValue && t.SubmittedDate.Value >= windowStart && t.SubmittedDate.Value < windowEndExclusive);
            AverageCreatedPerDayLiteral.Text = ((double)createdInPeriod / days.Count).ToString("0.0");

            // periodTotal is always >= 1 here - the window is clamped to always include
            // the earliest completion on record, so there's no divide-by-zero to guard.
            CreatedClosedRatioLiteral.Text = ((double)createdInPeriod / periodTotal).ToString("0.00") + " : 1";

            var closedPerDay = days.Select(d => new
            {
                Day = d,
                Count = completedTickets.Count(t => t.CompletedDate.Value.Date == d)
            }).ToList();
            var mostProductiveDay = closedPerDay.OrderByDescending(x => x.Count).First();
            MostProductiveDayLiteral.Text = mostProductiveDay.Day.ToString("MMM d") + " (" + mostProductiveDay.Count + " closed)";

            var quietDays = closedPerDay.Count(x => x.Count == 0);
            QuietDaysLiteral.Text = quietDays + " of " + days.Count;

            var busiestEmployee = employeeSeries.OrderByDescending(s => s.total).FirstOrDefault();
            BusiestEmployeeLiteral.Text = busiestEmployee != null
                ? busiestEmployee.label + " (" + busiestEmployee.total + " closed)"
                : "(none)";

            var commentsInPeriod = db.Comments
                .Include(c => c.User)
                .Include(c => c.Ticket)
                .Where(c => c.CreatedAt >= windowStart && c.CreatedAt < windowEndExclusive)
                .ToList();

            TotalCommentsLiteral.Text = commentsInPeriod.Count.ToString();
            AverageCommentsPerDayLiteral.Text = ((double)commentsInPeriod.Count / days.Count).ToString("0.0");
            EmployeeCommentsLiteral.Text = commentsInPeriod.Count(c => c.User is Employee).ToString();
            CustomerCommentsLiteral.Text = commentsInPeriod.Count(c => c.User is Customer).ToString();

            var mostCommentedTicket = commentsInPeriod
                .GroupBy(c => c.TicketId)
                .Select(g => new { TicketId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();
            MostCommentedTicketLiteral.Text = mostCommentedTicket != null
                ? "#" + mostCommentedTicket.TicketId + " (" + mostCommentedTicket.Count + " comments)"
                : "(none)";

            var customerCommentCounts = commentsInPeriod
                .Where(c => c.User is Customer)
                .GroupBy(c => c.User.Name)
                .Select(g => new { label = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToList();

            var employeeCommentCounts = commentsInPeriod
                .Where(c => c.User is Employee)
                .GroupBy(c => c.User.Name)
                .Select(g => new { label = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToList();

            var chartData = new
            {
                dayLabels = days.Select(d => d.ToString("MMM d")).ToList(),
                employeeSeries = employeeSeries,
                periodTotal = periodTotal,
                customerCommentCounts = customerCommentCounts,
                employeeCommentCounts = employeeCommentCounts
            };

            // Embedded directly into a <script> block (see Metrics.aspx) - JsonConvert
            // already escapes quotes/backslashes for a valid JS string, but a literal
            // "</script>" inside a value (e.g. an employee name) would still terminate
            // the block early, so that one extra escape is added on top.
            ChartDataLiteral.Text = JsonConvert.SerializeObject(chartData).Replace("</", "<\\/");
        }
    }
}
