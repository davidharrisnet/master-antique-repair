using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MasterAntiqueRepair;

namespace MasterAntiqueRepairSeed
{
    // One antique item drives a customer's three tickets (A ends COMPLETED, B ends
    // INPROGRESS, C stays SUBMITTED) plus the completion comment and 1-3 follow-up
    // comments on A (count and wording picked per-customer by the seeded RNG).
    internal class SeedItem
    {
        public string DescriptionA;
        public string DescriptionB;
        public string DescriptionC;
        public string CompletionComment;
        public string[] FollowUpComments;
    }

    class Program
    {
        private const string ManagerPassword = "ManagerPass123!";

        // Fixed, not time-based: this seeds a single System.Random reused for every
        // "random" choice below (timing jitter, follow-up comment counts). A fixed seed
        // means the exact same sequence comes out on every run - looks organic (no
        // formulaic linear/cyclic pattern to spot), but stays exactly reproducible,
        // which is the entire point of this tool.
        private const int RandomSeed = 20250917;

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
                FollowUpComments = new[]
                {
                    "Desk looks wonderful — you can barely tell the leg was ever broken. Thank you!",
                    "Picked it up today, the drawer pull matches perfectly. Great work.",
                    "So relieved you could save the original leather inlay too. Much appreciated!"
                }
            },
            new SeedItem
            {
                DescriptionA = "Set of six oak dining chairs — two chairs have loose seat joints that creak and wobble under weight.",
                DescriptionB = "Set of six oak dining chairs — one chair's back spindle is cracked.",
                DescriptionC = "Set of six oak dining chairs — upholstery on three seats is worn through at the front edge.",
                CompletionComment = "Reinforced and re-glued the loose seat joints on both chairs with hide glue and corner blocks; no more wobble.",
                FollowUpComments = new[]
                {
                    "The chairs feel solid again, no creaking at all. Really appreciate the quick turnaround.",
                    "Used them for a dinner party last night — nobody could tell they'd ever been repaired.",
                    "Thank you, the whole set finally matches again."
                }
            },
            new SeedItem
            {
                DescriptionA = "Oak grandfather clock — pendulum has stopped swinging, suspect a bent escapement; case veneer also lifting on the right side.",
                DescriptionB = "Oak grandfather clock — chime mechanism is silent, weights drop too fast.",
                DescriptionC = "Oak grandfather clock — glass door hinge is loose and door won't latch.",
                CompletionComment = "Re-glued the escapement pivot and adjusted the beat; pendulum now swings true and keeps time within a minute a week. Reset the chime timing and oiled the movement.",
                FollowUpComments = new[]
                {
                    "Picked it up this afternoon — the chime is right on time again and the case looks great. Thanks for the careful work!",
                    "It's been keeping perfect time all week. Couldn't be happier.",
                    "My grandfather would be proud — thank you for treating it so carefully."
                }
            },
            new SeedItem
            {
                DescriptionA = "Antique vanity table with tri-fold mirror — center mirror panel is cracked and the hinge on the left panel is broken.",
                DescriptionB = "Antique vanity table with tri-fold mirror — one drawer is stuck and won't slide open.",
                DescriptionC = "Antique vanity table with tri-fold mirror — veneer is peeling along the front edge.",
                CompletionComment = "Replaced the cracked mirror pane with restoration glass and repaired the broken hinge; both side panels swing freely again.",
                FollowUpComments = new[]
                {
                    "The mirror looks brand new and the hinge is smooth. So glad I didn't have to replace the whole piece.",
                    "It's back in my bedroom and looks stunning. Thank you!",
                    "Didn't expect the mirror to look this clear again — wonderful job."
                }
            },
            new SeedItem
            {
                DescriptionA = "Cedar jewelry box with inlay lid — the lid hinge has pulled loose from the wood and the lid won't stay open.",
                DescriptionB = "Cedar jewelry box with inlay lid — the small lock mechanism is jammed.",
                DescriptionC = "Cedar jewelry box with inlay lid — inlay pattern on the lid has a missing piece near the corner.",
                CompletionComment = "Reset the hinge screws into filled, redrilled pilot holes so the lid now stays open on its own; tested through a dozen open/close cycles.",
                FollowUpComments = new[]
                {
                    "The lid stays open perfectly now — such a relief, this box was my grandmother's.",
                    "Tested it a dozen times since bringing it home, works flawlessly.",
                    "Thank you for treating something so sentimental with such care."
                }
            },
            new SeedItem
            {
                DescriptionA = "Antique oak rocking chair — one rocker rail has a long crack running along the grain and flexes alarmingly when rocking.",
                DescriptionB = "Antique oak rocking chair — armrest on the right side is loose.",
                DescriptionC = "Antique oak rocking chair — finish is worn through on both armrests.",
                CompletionComment = "Splined and clamped the cracked rocker rail with a matching oak insert and reinforced it from underneath. Tested with full weight rocking — completely stable now.",
                FollowUpComments = new[]
                {
                    "It rocks smoothly again with no flex at all. Thank you for taking such care with it.",
                    "Sat in it all evening, feels sturdier than when I bought it.",
                    "You really saved this one — I thought it was beyond repair."
                }
            },
            new SeedItem
            {
                DescriptionA = "Walnut armoire — one door won't close flush and rubs against the frame; interior shelf has sagged in the middle.",
                DescriptionB = "Walnut armoire — the top cornice molding has separated from the case on one side.",
                DescriptionC = "Walnut armoire — a small burn mark on the interior shelf needs refinishing.",
                CompletionComment = "Rehung the door and adjusted the strike so it closes flush; added a discreet center support under the sagging shelf.",
                FollowUpComments = new[]
                {
                    "Door closes perfectly now and the shelf doesn't sag anymore. Excellent work as always.",
                    "Finally able to use the top shelf again without worrying about it collapsing.",
                    "Looks like nothing ever happened to it. Thank you!"
                }
            },
            new SeedItem
            {
                DescriptionA = "Mahogany sideboard — the left cabinet door hinge is broken and the door hangs crooked; one leg wobbles on uneven contact.",
                DescriptionB = "Mahogany sideboard — the top surface has several water rings that need refinishing.",
                DescriptionC = "Mahogany sideboard — a decorative brass inlay strip is partially detached along the front edge.",
                CompletionComment = "Replaced the broken hinge with a matching reproduction and shimmed/reset the wobbly leg so all four now sit flush.",
                FollowUpComments = new[]
                {
                    "The door swings true again and it doesn't rock anymore. Really pleased with the fix.",
                    "Already back in the dining room, works great.",
                    "Appreciate you fitting the reproduction hinge so precisely."
                }
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

            // Fixed seed (see RandomSeed) - every draw from this instance is identical on
            // every run, so "randomized" timing/comment-count variation is still an exact,
            // reproducible regression baseline, not true nondeterminism.
            var random = new Random(RandomSeed);

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

                    var followUpComments = AddFollowUpComments(db, random, customers, ticketA);

                    PatchTimestamps(db, random, runStart, employees, customers, ticketA, ticketB, ticketC, followUpComments);
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

        // Each customer gets 1-3 follow-up comments (not a uniform count) - picked by the
        // seeded RNG, using that item's first N canned lines (N = the drawn count).
        private static List<Comment>[] AddFollowUpComments(
            RepairShopContext db, Random random, Customer[] customers, Ticket[] ticketA)
        {
            var followUps = new List<Comment>[8];
            var commentService = new CommentService(db);
            for (int i = 1; i <= 8; i++)
            {
                var count = random.Next(1, 4); // 1, 2, or 3
                var comments = new List<Comment>();
                for (int n = 0; n < count; n++)
                {
                    comments.Add(commentService.AddCustomerComment(
                        customers[i - 1].Id, ticketA[i - 1].Id, Items[i - 1].FollowUpComments[n]));
                }
                followUps[i - 1] = comments;
            }
            return followUps;
        }

        // Every timestamp above was stamped with the real DateTime.Now by the domain/service
        // methods (no override parameter exists on any of them) - this pass overwrites them
        // using the seeded `random` so re-seeding still produces the exact same values every
        // run, just without an obviously formulaic (linear/cyclic) pattern to them.
        private static void PatchTimestamps(
            RepairShopContext db, Random random, DateTime runStart, Employee[] employees, Customer[] customers,
            Ticket[] ticketA, Ticket[] ticketB, Ticket[] ticketC, List<Comment>[] followUpComments)
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

                // Completion (and its comments) is what MetricsService's rolling 7-day
                // window actually charts per day - pick a random day within that window
                // (0 = today .. -6) and a random time of day, rather than a linear/cyclic
                // formula in i (which is what produced the "sine wave" look).
                var completedDate = RandomTimeOnDay(random, runStart, -random.Next(0, 7));
                if (completedDate > runStart)
                {
                    completedDate = runStart.AddMinutes(-random.Next(1, 60));
                }
                a.CompletedDate = completedDate;

                // Open-to-close duration also varies (1-5 days) instead of a fixed gap -
                // this is what was reading as suspiciously symmetric before.
                a.SubmittedDate = completedDate.AddDays(-random.Next(1, 6)).AddMinutes(-random.Next(0, 1440));
                a.AssignedDate = a.SubmittedDate.Value.AddMinutes(random.Next(15, 480));

                b.SubmittedDate = RandomTimeOnDay(random, runStart, -random.Next(1, 8));
                b.AssignedDate = b.SubmittedDate.Value.AddMinutes(random.Next(15, 480));

                c.SubmittedDate = RandomTimeOnDay(random, runStart, -random.Next(1, 8));

                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, a.Id, AuditLog.ActionType.CreateTicket, a.SubmittedDate.Value);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, b.Id, AuditLog.ActionType.CreateTicket, b.SubmittedDate.Value);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, c.Id, AuditLog.ActionType.CreateTicket, c.SubmittedDate.Value);

                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, a.Id, AuditLog.ActionType.AssignTicket, a.AssignedDate.Value);
                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, b.Id, AuditLog.ActionType.AssignTicket, b.AssignedDate.Value);

                SetAuditTimestamp(db, AuditLog.EntityKind.Ticket, a.Id, AuditLog.ActionType.CompleteTicket, a.CompletedDate.Value);

                // The comment embedded in Employee.CompleteTicket returns no reference of
                // its own - find it by the one thing that disambiguates it from the
                // follow-up comments on the same ticket: which user posted it.
                var embeddedComment = db.Comments.Local.Single(cm => cm.TicketId == a.Id && cm.UserId == employeeId);
                embeddedComment.CreatedAt = a.CompletedDate.Value;

                // Each follow-up comment lands at its own random delay after completion
                // (10 minutes to 2 days), clamped so none of them land in the future.
                foreach (var comment in followUpComments[i - 1])
                {
                    var createdAt = a.CompletedDate.Value.AddMinutes(random.Next(10, 2880));
                    if (createdAt > runStart)
                    {
                        createdAt = runStart.AddMinutes(-random.Next(1, 30));
                    }
                    comment.CreatedAt = createdAt;
                    SetAuditTimestamp(db, AuditLog.EntityKind.Comment, comment.Id, AuditLog.ActionType.AddComment, createdAt);
                }
            }
        }

        // A random time of day, `dayOffset` days from runStart's date (dayOffset <= 0).
        private static DateTime RandomTimeOnDay(Random random, DateTime runStart, int dayOffset)
        {
            return runStart.Date.AddDays(dayOffset).AddSeconds(random.Next(0, 86400));
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
