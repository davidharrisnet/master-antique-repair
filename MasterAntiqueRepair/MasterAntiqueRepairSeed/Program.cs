using System;
using System.IO;
using System.Linq;
using MasterAntiqueRepair;

namespace MasterAntiqueRepairSeed
{
    // One antique item drives a customer's three tickets (A ends COMPLETED, B ends
    // INPROGRESS, C stays SUBMITTED) plus the completion/follow-up comments on A.
    internal class SeedItem
    {
        public string DescriptionA;
        public string DescriptionB;
        public string DescriptionC;
        public string CompletionComment;
        public string FollowUpComment;
    }

    class Program
    {
        private const string ManagerPassword = "ManagerPass123!";

        private static readonly string[] EmployeePasswords =
        {
            "EmployeePass111!", "EmployeePass222!", "EmployeePass333!"
        };

        private static readonly SeedItem[] Items =
        {
            new SeedItem
            {
                DescriptionA = "Victorian mahogany writing desk — left rear leg has split at the joint and needs re-gluing and reinforcing; missing the brass pull on the top-left drawer.",
                DescriptionB = "Victorian mahogany writing desk — the fold-down writing surface no longer stays latched shut.",
                DescriptionC = "Victorian mahogany writing desk — leather desktop inlay is cracked and lifting at one corner.",
                CompletionComment = "Re-glued and dowelled the rear leg joint, replaced the missing brass drawer pull with a period-correct match. Desk is sturdy and level again.",
                FollowUpComment = "Desk looks wonderful — you can barely tell the leg was ever broken. Thank you!"
            },
            new SeedItem
            {
                DescriptionA = "Set of six oak dining chairs — two chairs have loose seat joints that creak and wobble under weight.",
                DescriptionB = "Set of six oak dining chairs — one chair's back spindle is cracked.",
                DescriptionC = "Set of six oak dining chairs — upholstery on three seats is worn through at the front edge.",
                CompletionComment = "Reinforced and re-glued the loose seat joints on both chairs with hide glue and corner blocks; no more wobble.",
                FollowUpComment = "The chairs feel solid again, no creaking at all. Really appreciate the quick turnaround."
            },
            new SeedItem
            {
                DescriptionA = "Oak grandfather clock — pendulum has stopped swinging, suspect a bent escapement; case veneer also lifting on the right side.",
                DescriptionB = "Oak grandfather clock — chime mechanism is silent, weights drop too fast.",
                DescriptionC = "Oak grandfather clock — glass door hinge is loose and door won't latch.",
                CompletionComment = "Re-glued the escapement pivot and adjusted the beat; pendulum now swings true and keeps time within a minute a week. Reset the chime timing and oiled the movement.",
                FollowUpComment = "Picked it up this afternoon — the chime is right on time again and the case looks great. Thanks for the careful work!"
            },
            new SeedItem
            {
                DescriptionA = "Antique vanity table with tri-fold mirror — center mirror panel is cracked and the hinge on the left panel is broken.",
                DescriptionB = "Antique vanity table with tri-fold mirror — one drawer is stuck and won't slide open.",
                DescriptionC = "Antique vanity table with tri-fold mirror — veneer is peeling along the front edge.",
                CompletionComment = "Replaced the cracked mirror pane with restoration glass and repaired the broken hinge; both side panels swing freely again.",
                FollowUpComment = "The mirror looks brand new and the hinge is smooth. So glad I didn't have to replace the whole piece."
            },
            new SeedItem
            {
                DescriptionA = "Cedar jewelry box with inlay lid — the lid hinge has pulled loose from the wood and the lid won't stay open.",
                DescriptionB = "Cedar jewelry box with inlay lid — the small lock mechanism is jammed.",
                DescriptionC = "Cedar jewelry box with inlay lid — inlay pattern on the lid has a missing piece near the corner.",
                CompletionComment = "Reset the hinge screws into filled, redrilled pilot holes so the lid now stays open on its own; tested through a dozen open/close cycles.",
                FollowUpComment = "The lid stays open perfectly now — such a relief, this box was my grandmother's."
            },
            new SeedItem
            {
                DescriptionA = "Antique oak rocking chair — one rocker rail has a long crack running along the grain and flexes alarmingly when rocking.",
                DescriptionB = "Antique oak rocking chair — armrest on the right side is loose.",
                DescriptionC = "Antique oak rocking chair — finish is worn through on both armrests.",
                CompletionComment = "Splined and clamped the cracked rocker rail with a matching oak insert and reinforced it from underneath. Tested with full weight rocking — completely stable now.",
                FollowUpComment = "It rocks smoothly again with no flex at all. Thank you for taking such care with it."
            },
            new SeedItem
            {
                DescriptionA = "Walnut armoire — one door won't close flush and rubs against the frame; interior shelf has sagged in the middle.",
                DescriptionB = "Walnut armoire — the top cornice molding has separated from the case on one side.",
                DescriptionC = "Walnut armoire — a small burn mark on the interior shelf needs refinishing.",
                CompletionComment = "Rehung the door and adjusted the strike so it closes flush; added a discreet center support under the sagging shelf.",
                FollowUpComment = "Door closes perfectly now and the shelf doesn't sag anymore. Excellent work as always."
            },
            new SeedItem
            {
                DescriptionA = "Mahogany sideboard — the left cabinet door hinge is broken and the door hangs crooked; one leg wobbles on uneven contact.",
                DescriptionB = "Mahogany sideboard — the top surface has several water rings that need refinishing.",
                DescriptionC = "Mahogany sideboard — a decorative brass inlay strip is partially detached along the front edge.",
                CompletionComment = "Replaced the broken hinge with a matching reproduction and shimmed/reset the wobbly leg so all four now sit flush.",
                FollowUpComment = "The door swings true again and it doesn't rock anymore. Really pleased with the fix."
            }
        };

        private class Options
        {
            public string Server;
            public string Database;
            public string AppDataPath;
        }

        static int Main(string[] args)
        {
            var options = ParseArgs(args);

            // |DataDirectory| only auto-resolves inside a web app - point it at the
            // website's real App_Data folder (same trick MasterAntiqueRepairScratch uses)
            // so this connects to the .mdf the given database name actually lives in.
            AppDomain.CurrentDomain.SetData("DataDirectory", options.AppDataPath);

            var connectionString = string.Format(
                "Data Source={0};Initial Catalog={1};AttachDbFilename=|DataDirectory|\\{1}.mdf;Integrated Security=SSPI",
                options.Server, options.Database);

            // Captured once - every timestamp seeded below is an offset from this single
            // moment, so re-seeding on any future day always lands inside MetricsService's
            // rolling 7-day window, while the relative shape stays identical every run.
            var runStart = DateTime.Now;

            try
            {
                using (var db = new RepairShopContext(connectionString))
                {
                    if (db.Users.Any() || db.Tickets.Any())
                    {
                        Console.Error.WriteLine(
                            "Database '" + options.Database + "' is not empty. Reseeding on top of " +
                            "existing data would not produce identical counts. Reset it first:");
                        Console.Error.WriteLine("  Scripts\\Reset-Database.ps1 -Database " + options.Database);
                        return 1;
                    }

                    var manager = CreateManager(db, runStart);
                    var employees = CreateEmployees(db, manager);
                    var customers = CreateCustomers(db, manager);

                    Ticket[] ticketA, ticketB, ticketC;
                    CreateAndProgressTickets(db, customers, employees, out ticketA, out ticketB, out ticketC);

                    var followUpComments = AddFollowUpComments(db, customers, ticketA);

                    PatchTimestamps(db, runStart, employees, customers, ticketA, ticketB, ticketC, followUpComments);
                    db.SaveChanges();

                    PrintSummary(db, employees);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Seeding failed: " + ex);
                return 1;
            }
        }

        private static Options ParseArgs(string[] args)
        {
            var options = new Options
            {
                Server = "(localdb)\\MSSQLLocalDB",
                Database = "aspnet-MasterAntiqueRepair-e93a6129-7f74-4486-97e8-8d4ab1a709b4",
                AppDataPath = Path.GetFullPath(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\MasterAntiqueRepair\App_Data"))
            };

            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "--server":
                        options.Server = args[++i];
                        break;
                    case "--database":
                        options.Database = args[++i];
                        break;
                    case "--appDataPath":
                        options.AppDataPath = args[++i];
                        break;
                }
            }

            // SqlClient's |DataDirectory| substitution rejects a path containing unresolved
            // ".." segments (throws "Invalid value for key 'attachdbfilename'") - the
            // PowerShell wrapper builds AppDataPath with Join-Path, which doesn't collapse
            // "..", so normalize it here regardless of where the value came from.
            options.AppDataPath = Path.GetFullPath(options.AppDataPath);

            return options;
        }

        // No AddManager service path exists - managers have no self-service creation
        // path in the real app either, so this mirrors production (direct EF insert,
        // no audit row) rather than routing through a Service that doesn't exist.
        private static Manager CreateManager(RepairShopContext db, DateTime runStart)
        {
            var manager = new Manager { Name = "manager", CreatedAt = runStart.AddDays(-7) };
            manager.SetPassword(ManagerPassword);
            db.Users.Add(manager);
            db.SaveChanges();
            return manager;
        }

        // These construct services against the one shared, caller-owned RepairShopContext -
        // deliberately NOT wrapped in a `using`, since AccountService/TicketService/
        // CommentService.Dispose() disposes the context they were given, which would break
        // every other service/query still sharing it. Only Main's own `using (var db = ...)`
        // disposes the context, once, at the very end.
        private static Employee[] CreateEmployees(RepairShopContext db, Manager manager)
        {
            var employees = new Employee[3];
            var accountService = new AccountService(db);
            for (int e = 1; e <= 3; e++)
            {
                employees[e - 1] = accountService.AddEmployee(
                    manager.Id, "employee" + e, EmployeePasswords[e - 1], EmployeePasswords[e - 1]);
            }
            return employees;
        }

        private static Customer[] CreateCustomers(RepairShopContext db, Manager manager)
        {
            var customers = new Customer[8];
            var accountService = new AccountService(db);
            for (int i = 1; i <= 8; i++)
            {
                var password = "CustomerPass" + (i * 111) + "!";
                customers[i - 1] = accountService.AddCustomer(manager.Id, "customer" + i, password, password);
            }
            return customers;
        }

        private static void CreateAndProgressTickets(
            RepairShopContext db, Customer[] customers, Employee[] employees,
            out Ticket[] ticketA, out Ticket[] ticketB, out Ticket[] ticketC)
        {
            var a = new Ticket[8];
            var b = new Ticket[8];
            var c = new Ticket[8];

            var ticketService = new TicketService(db);
            for (int i = 1; i <= 8; i++)
            {
                var item = Items[i - 1];
                a[i - 1] = ticketService.SubmitTicket(customers[i - 1].Id, item.DescriptionA);
                b[i - 1] = ticketService.SubmitTicket(customers[i - 1].Id, item.DescriptionB);
                c[i - 1] = ticketService.SubmitTicket(customers[i - 1].Id, item.DescriptionC);
                // c[i-1] stays SUBMITTED forever - never assigned
            }

            for (int i = 1; i <= 8; i++)
            {
                // employee1<-customers{1,4,7} employee2<-{2,5,8} employee3<-{3,6}
                var employeeId = employees[(i - 1) % 3].Id;
                ticketService.AssignToMe(a[i - 1].Id, employeeId);
                ticketService.AssignToMe(b[i - 1].Id, employeeId); // stays INPROGRESS forever
                ticketService.CompleteTicket(a[i - 1].Id, employeeId, Items[i - 1].CompletionComment);
            }

            ticketA = a;
            ticketB = b;
            ticketC = c;
        }

        private static Comment[] AddFollowUpComments(RepairShopContext db, Customer[] customers, Ticket[] ticketA)
        {
            var followUps = new Comment[8];
            var commentService = new CommentService(db);
            for (int i = 1; i <= 8; i++)
            {
                followUps[i - 1] = commentService.AddCustomerComment(
                    customers[i - 1].Id, ticketA[i - 1].Id, Items[i - 1].FollowUpComment);
            }
            return followUps;
        }

        // Every timestamp above was stamped with the real DateTime.Now by the domain/service
        // methods (no override parameter exists on any of them) - this pass overwrites them
        // with fixed offsets from the single captured runStart so re-seeding is deterministic.
        private static void PatchTimestamps(
            RepairShopContext db, DateTime runStart, Employee[] employees, Customer[] customers,
            Ticket[] ticketA, Ticket[] ticketB, Ticket[] ticketC, Comment[] followUpComments)
        {
            for (int e = 1; e <= 3; e++)
            {
                employees[e - 1].CreatedAt = runStart.AddDays(-7).AddMinutes(10 * e);
                SetAuditTimestamp(db, AuditLog.EntityKind.User, employees[e - 1].Id,
                    AuditLog.ActionType.CreateUser, employees[e - 1].CreatedAt);
            }

            for (int i = 1; i <= 8; i++)
            {
                customers[i - 1].CreatedAt = runStart.AddDays(-6).AddMinutes(15 * i);
                SetAuditTimestamp(db, AuditLog.EntityKind.User, customers[i - 1].Id,
                    AuditLog.ActionType.CreateUser, customers[i - 1].CreatedAt);

                var a = ticketA[i - 1];
                var b = ticketB[i - 1];
                var c = ticketC[i - 1];
                var employeeId = employees[(i - 1) % 3].Id;

                a.SubmittedDate = runStart.AddDays(-5).AddHours(i);
                b.SubmittedDate = runStart.AddDays(-4).AddHours(i);
                c.SubmittedDate = runStart.AddDays(-3).AddHours(i);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, a.Id, AuditLog.ActionType.CreateTicket, a.SubmittedDate.Value);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, b.Id, AuditLog.ActionType.CreateTicket, b.SubmittedDate.Value);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, c.Id, AuditLog.ActionType.CreateTicket, c.SubmittedDate.Value);

                a.AssignedDate = a.SubmittedDate.Value.AddHours(2);
                b.AssignedDate = b.SubmittedDate.Value.AddHours(2);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, a.Id, AuditLog.ActionType.AssignTicket, a.AssignedDate.Value);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, b.Id, AuditLog.ActionType.AssignTicket, b.AssignedDate.Value);

                a.CompletedDate = runStart.AddDays(-1).AddHours(i);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, a.Id, AuditLog.ActionType.CompleteTicket, a.CompletedDate.Value);

                // The comment embedded in Employee.CompleteTicket returns no reference of
                // its own - find it by the one thing that disambiguates it from the
                // follow-up comment on the same ticket: which user posted it.
                var embeddedComment = db.Comments.Local.Single(cm => cm.TicketId == a.Id && cm.UserId == employeeId);
                embeddedComment.CreatedAt = a.CompletedDate.Value;

                followUpComments[i - 1].CreatedAt = a.CompletedDate.Value.AddHours(3);
                SetAuditTimestamp(db, AuditLog.EntityKind.Comment, followUpComments[i - 1].Id,
                    AuditLog.ActionType.AddComment, followUpComments[i - 1].CreatedAt);
            }
        }

        // Every (EntityType, EntityId, Action) triple is unique in this seeded dataset -
        // no entity is ever the subject of the same action twice - so .Single() is safe.
        // db.AuditLogs.Local avoids a round trip: every row was added to this same context.
        private static void SetAuditTimestamp(
            RepairShopContext db, AuditLog.EntityKind entityType, int entityId, AuditLog.ActionType action, DateTime timestamp)
        {
            var log = db.AuditLogs.Local.Single(a => a.EntityType == entityType && a.EntityId == entityId && a.Action == action);
            log.Timestamp = timestamp;
        }

        private static void PrintSummary(RepairShopContext db, Employee[] employees)
        {
            Console.WriteLine();
            Console.WriteLine("Seed complete. Golden numbers:");
            Console.WriteLine();

            Console.WriteLine("AuditLog by action:");
            foreach (var group in db.AuditLogs.GroupBy(a => a.Action).OrderBy(g => g.Key.ToString()))
            {
                Console.WriteLine("  {0,-18} {1}", group.Key, group.Count());
            }
            Console.WriteLine("  {0,-18} {1}", "TOTAL", db.AuditLogs.Count());
            Console.WriteLine();

            Console.WriteLine("Comments table rows: " + db.Comments.Count());
            Console.WriteLine();

            Console.WriteLine("Tickets by state:");
            foreach (var group in db.Tickets.GroupBy(t => t.State).OrderBy(g => g.Key.ToString()))
            {
                Console.WriteLine("  {0,-12} {1}", group.Key, group.Count());
            }
            Console.WriteLine();

            Console.WriteLine("Assigned tickets per employee:");
            foreach (var employee in employees)
            {
                var count = db.Tickets.Count(t => t.User.Id == employee.Id);
                Console.WriteLine("  {0,-12} {1}", employee.Name, count);
            }

            Console.WriteLine();
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
