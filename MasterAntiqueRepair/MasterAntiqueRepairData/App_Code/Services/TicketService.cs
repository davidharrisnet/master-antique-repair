using System;
using System.Collections.Generic;

namespace MasterAntiqueRepair
{
    public class TicketService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly TicketRepository _tickets;
        private readonly UserRepository _users;

        public TicketService() : this(new RepairShopContext())
        {
        }

        public TicketService(RepairShopContext db)
        {
            _db = db;
            _tickets = new TicketRepository(_db);
            _users = new UserRepository(_db);
        }

        // Returns null if customerId is missing or doesn't resolve to a real Customer
        // (matches the page's existing silent-no-op behavior for that case, rather than
        // throwing); throws ArgumentException for a validation failure, same as
        // Ticket.CreateSubmitted. customerId is nullable because RepairAuthHelper.
        // GetCurrentUserId() is (it reads a claim that could theoretically be absent).
        public Ticket SubmitTicket(int? customerId, string description)
        {
            var customer = customerId.HasValue ? _users.GetCustomerById(customerId.Value) : null;
            if (customer == null)
            {
                return null;
            }

            var ticket = Ticket.CreateSubmitted(description, customer);

            _tickets.Add(ticket);
            _db.SaveChanges();

            AuditLogger.Log(_db, customer, AuditLog.ActionType.CreateTicket, AuditLog.EntityKind.Ticket, ticket.Id);
            _db.SaveChanges();

            return ticket;
        }

        public List<Ticket> GetMyTickets(int? customerId)
        {
            return customerId.HasValue ? _tickets.GetByCustomer(customerId.Value) : new List<Ticket>();
        }

        public List<Ticket> GetUnassigned()
        {
            return _tickets.GetUnassigned();
        }

        public List<Ticket> GetUnassignedWithCustomer()
        {
            return _tickets.GetUnassignedWithCustomer();
        }

        public List<Ticket> GetAssignedTo(int? employeeId)
        {
            return employeeId.HasValue ? _tickets.GetByEmployee(employeeId.Value) : new List<Ticket>();
        }

        // Silently no-ops (matches the page's existing behavior) if the employee/ticket
        // don't resolve, or the ticket has already been taken by someone else.
        public void AssignToMe(int ticketId, int? employeeId)
        {
            var employee = employeeId.HasValue ? _users.GetEmployeeById(employeeId.Value) : null;
            var ticket = _tickets.GetById(ticketId);

            if (employee == null || ticket == null || ticket.User != null)
            {
                return;
            }

            employee.TakeTicket(ticket);
            AuditLogger.Log(_db, employee, AuditLog.ActionType.AssignTicket, AuditLog.EntityKind.Ticket, ticket.Id);
            _db.SaveChanges();
        }

        // Silently no-ops if the employee/ticket don't resolve or the ticket isn't
        // assigned to this employee - matches the page's existing behavior.
        public void CompleteTicket(int ticketId, int? employeeId, string comment)
        {
            var employee = employeeId.HasValue ? _users.GetEmployeeById(employeeId.Value) : null;
            var ticket = employee != null ? _tickets.GetByIdForEmployee(ticketId, employee.Id) : null;

            if (employee == null || ticket == null)
            {
                return;
            }

            employee.CompleteTicket(ticket, comment);
            AuditLogger.Log(_db, employee, AuditLog.ActionType.CompleteTicket, AuditLog.EntityKind.Ticket, ticket.Id);
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
