<%@ Page Title="Metrics" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Metrics.aspx.cs" Inherits="Metrics" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h2><%: Title %>.</h2>

    <asp:Panel runat="server" ID="NoDataPanel" Visible="false" CssClass="text-muted">
        No completed tickets yet - metrics will appear here once employees start closing tickets.
    </asp:Panel>

    <asp:Panel runat="server" ID="ChartsPanel">
        <div class="panel panel-default">
            <div class="panel-heading"><strong>Tickets Closed by Employee (per day)</strong></div>
            <div class="panel-body">
                <canvas id="employeeChart" height="55"></canvas>
            </div>
        </div>

        <div class="panel panel-default">
            <div class="panel-heading"><strong>Total Tickets Closed by Employee (this period)</strong></div>
            <div class="panel-body">
                <canvas id="employeeTotalsChart" height="55"></canvas>
            </div>
        </div>

      <div class="row">
        <div class="col-md-6">
            <div class="panel panel-default">
                <div class="panel-heading"><strong>Totals (this period)</strong></div>
                <div class="panel-body" style="height: 330px; overflow-y: auto;">
                    <table class="table table-condensed">
                        <tbody>
                            <tr>
                                <th>Total Tickets Closed</th>
                                <td><asp:Literal runat="server" ID="PeriodTotalLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Average Closed Per Day</th>
                                <td><asp:Literal runat="server" ID="AveragePerDayLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Average Created Per Day</th>
                                <td><asp:Literal runat="server" ID="AverageCreatedPerDayLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Closed/Created Ratio</th>
                                <td><asp:Literal runat="server" ID="ClosedCreatedRatioLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Most Productive Day</th>
                                <td><asp:Literal runat="server" ID="MostProductiveDayLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Busiest Employee</th>
                                <td><asp:Literal runat="server" ID="BusiestEmployeeLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Quiet Days (no closures)</th>
                                <td><asp:Literal runat="server" ID="QuietDaysLiteral" /></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading"><strong>Comment Activity (this period)</strong></div>
                <div class="panel-body" style="height: 330px; overflow-y: auto;">
                    <table class="table table-condensed">
                        <tbody>
                            <tr>
                                <th>Total Comments</th>
                                <td><asp:Literal runat="server" ID="TotalCommentsLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Average Comments Per Day</th>
                                <td><asp:Literal runat="server" ID="AverageCommentsPerDayLiteral" /></td>
                            </tr>
                            <tr>
                                <th>From Employees</th>
                                <td><asp:Literal runat="server" ID="EmployeeCommentsLiteral" /></td>
                            </tr>
                            <tr>
                                <th>From Customers</th>
                                <td><asp:Literal runat="server" ID="CustomerCommentsLiteral" /></td>
                            </tr>
                            <tr>
                                <th>Most Commented Ticket</th>
                                <td><asp:Literal runat="server" ID="MostCommentedTicketLiteral" /></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="panel panel-default">
                <div class="panel-heading"><strong>Comments by Customer (this period)</strong></div>
                <div class="panel-body text-center" style="height: 330px;">
                    <canvas id="commentsByCustomerChart" width="300" height="300"></canvas>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading"><strong>Comments by Employee (this period)</strong></div>
                <div class="panel-body text-center" style="height: 330px;">
                    <canvas id="commentsByEmployeeChart" width="300" height="300"></canvas>
                </div>
            </div>
        </div>
      </div>


        <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
        <script type="text/javascript">
            var metricsChartData = <asp:Literal runat="server" ID="ChartDataLiteral" />;

            (function () {
                var palette = ['#337ab7', '#5cb85c', '#f0ad4e', '#d9534f', '#5bc0de', '#292b2c'];

                new Chart(document.getElementById('employeeChart'), {
                    type: 'line',
                    data: {
                        labels: metricsChartData.dayLabels,
                        datasets: metricsChartData.employeeSeries.map(function (series, i) {
                            var color = palette[i % palette.length];
                            return {
                                label: series.label,
                                data: series.data,
                                borderColor: color,
                                backgroundColor: color,
                                fill: false,
                                tension: 0.2
                            };
                        })
                    },
                    options: {
                        responsive: true,
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });

                // Both this chart and the single-bar "this period" chart below share
                // the same axis max (the period total) so a bar's length/height means
                // the same thing in both places - an employee's bar and the grand-total
                // bar are drawn on directly comparable scales, not each auto-fit to its
                // own chart's own data.
                var sharedScaleMax = metricsChartData.periodTotal;

                // Same per-employee colors as the line chart above, by matching index,
                // so an employee's bar here is visually tied to their line/dot color there.
                new Chart(document.getElementById('employeeTotalsChart'), {
                    type: 'bar',
                    data: {
                        labels: metricsChartData.employeeSeries.map(function (series) { return series.label; }),
                        datasets: [{
                            label: 'Tickets closed',
                            data: metricsChartData.employeeSeries.map(function (series) { return series.total; }),
                            backgroundColor: metricsChartData.employeeSeries.map(function (series, i) {
                                return palette[i % palette.length];
                            })
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true, max: sharedScaleMax, ticks: { precision: 0 } } }
                    }
                });

                // Fixed pixel size (via the canvas's own width/height attributes) and
                // responsive: false, same as the earlier single-bar total chart - this
                // sidesteps Chart.js's container-width-driven height computation, which
                // is what kept collapsing charts in this page when they lived in a
                // narrower or grid-column-constrained container.
                new Chart(document.getElementById('commentsByCustomerChart'), {
                    type: 'pie',
                    data: {
                        labels: metricsChartData.customerCommentCounts.map(function (c) { return c.label; }),
                        datasets: [{
                            data: metricsChartData.customerCommentCounts.map(function (c) { return c.count; }),
                            backgroundColor: metricsChartData.customerCommentCounts.map(function (c, i) {
                                return palette[i % palette.length];
                            }),
                            borderColor: '#fff'
                        }]
                    },
                    options: {
                        responsive: false
                    }
                });

                new Chart(document.getElementById('commentsByEmployeeChart'), {
                    type: 'pie',
                    data: {
                        labels: metricsChartData.employeeCommentCounts.map(function (c) { return c.label; }),
                        datasets: [{
                            data: metricsChartData.employeeCommentCounts.map(function (c) { return c.count; }),
                            backgroundColor: metricsChartData.employeeCommentCounts.map(function (c, i) {
                                return palette[i % palette.length];
                            }),
                            borderColor: '#fff'
                        }]
                    },
                    options: {
                        responsive: false
                    }
                });
            })();
        </script>
    </asp:Panel>
</asp:Content>
