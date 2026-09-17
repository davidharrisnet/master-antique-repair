using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class MetricsService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly TicketRepository _tickets;
        private readonly UserRepository _users;
        private readonly CommentRepository _comments;

        public MetricsService()
        {
            _db = new RepairShopContext();
            _tickets = new TicketRepository(_db);
            _users = new UserRepository(_db);
            _comments = new CommentRepository(_db);
        }

        public MetricsSummary GetSummary()
        {
            var completedTickets = _tickets.GetCompleted();

            if (completedTickets.Count == 0)
            {
                return new MetricsSummary { HasData = false };
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

            // From the active employee roster, not from completedTickets - an employee
            // with zero completions so far should still show up with a zero-filled
            // series, not disappear from the charts entirely until their first close.
            var employeeNames = _users.GetActiveEmployees()
                .Select(emp => emp.UserName)
                .OrderBy(n => n)
                .ToList();

            var employeeSeries = employeeNames.Select(name =>
            {
                var data = days.Select(d => completedTickets.Count(t =>
                    t.User != null && t.User.UserName == name && t.CompletedDate.Value.Date == d)).ToList();
                return new EmployeeSeriesEntry { Label = name, Data = data, Total = data.Sum() };
            }).ToList();

            var periodTotal = completedTickets.Count(t =>
                t.CompletedDate.Value.Date >= windowStart && t.CompletedDate.Value.Date <= today);

            // .Date isn't supported in LINQ to Entities (unlike completedTickets above,
            // which is already an in-memory list by this point) - the repository compares
            // against the window as a plain DateTime range instead, so this still runs as
            // a real SQL COUNT rather than pulling every ticket into memory first.
            var windowEndExclusive = today.AddDays(1);
            var createdInPeriod = _tickets.CountCreatedBetween(windowStart, windowEndExclusive);

            var closedPerDay = days.Select(d => new
            {
                Day = d,
                Count = completedTickets.Count(t => t.CompletedDate.Value.Date == d)
            }).ToList();
            var mostProductiveDay = closedPerDay.OrderByDescending(x => x.Count).First();
            var quietDays = closedPerDay.Count(x => x.Count == 0);
            var busiestEmployee = employeeSeries.OrderByDescending(s => s.Total).FirstOrDefault();

            var commentsInPeriod = _comments.GetBetween(windowStart, windowEndExclusive);

            var mostCommentedTicket = commentsInPeriod
                .GroupBy(c => c.TicketId)
                .Select(g => new { TicketId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            var customerCommentCounts = commentsInPeriod
                .Where(c => c.User is Customer)
                .GroupBy(c => c.User.UserName)
                .Select(g => new NamedCount { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var employeeCommentCounts = commentsInPeriod
                .Where(c => c.User is Employee)
                .GroupBy(c => c.User.UserName)
                .Select(g => new NamedCount { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new MetricsSummary
            {
                HasData = true,
                Days = days,
                EmployeeSeries = employeeSeries,
                PeriodTotal = periodTotal,
                AveragePerDay = (double)periodTotal / days.Count,
                AverageCreatedPerDay = (double)createdInPeriod / days.Count,
                CreatedInPeriod = createdInPeriod,
                // createdInPeriod can legitimately be 0 (a ticket closed this period may
                // have been submitted before the window started), unlike periodTotal -
                // which the window is clamped to always include at least one of - so this
                // one genuinely needs the null guard the old ratio didn't.
                ClosedCreatedRatio = createdInPeriod > 0 ? (double?)((double)periodTotal / createdInPeriod) : null,
                MostProductiveDay = mostProductiveDay.Day,
                MostProductiveDayCount = mostProductiveDay.Count,
                QuietDays = quietDays,
                BusiestEmployeeLabel = busiestEmployee != null ? busiestEmployee.Label : null,
                BusiestEmployeeTotal = busiestEmployee != null ? busiestEmployee.Total : 0,
                TotalComments = commentsInPeriod.Count,
                AverageCommentsPerDay = (double)commentsInPeriod.Count / days.Count,
                EmployeeCommentCount = commentsInPeriod.Count(c => c.User is Employee),
                CustomerCommentCount = commentsInPeriod.Count(c => c.User is Customer),
                MostCommentedTicketId = mostCommentedTicket != null ? (int?)mostCommentedTicket.TicketId : null,
                MostCommentedTicketCount = mostCommentedTicket != null ? mostCommentedTicket.Count : 0,
                CustomerCommentCounts = customerCommentCounts,
                EmployeeCommentCounts = employeeCommentCounts
            };
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
