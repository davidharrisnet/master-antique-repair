using System;

namespace MasterAntiqueRepair
{
    // Add is the only comment operation - Edit/Delete were removed from the app
    // entirely (Customers and Employees may not edit or delete comments once posted).
    public class CommentService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly TicketRepository _tickets;
        private readonly UserRepository _users;

        public CommentService()
        {
            _db = new RepairShopContext();
            _tickets = new TicketRepository(_db);
            _users = new UserRepository(_db);
        }

        // Returns null if the customer/ticket don't resolve, or the ticket isn't theirs
        // (matches the page's existing silent-no-op behavior); throws
        // InvalidOperationException/ArgumentException for a real validation failure, same
        // as User.AddComment.
        public Comment AddCustomerComment(int? customerId, int ticketId, string text)
        {
            var customer = customerId.HasValue ? _users.GetCustomerById(customerId.Value) : null;
            var ticket = customer != null ? _tickets.GetByIdForCustomer(ticketId, customer.Id) : null;

            if (customer == null || ticket == null)
            {
                return null;
            }

            var comment = customer.AddComment(ticket, text);
            _db.SaveChanges();

            AuditLogger.Log(_db, customer, AuditLog.ActionType.AddComment, AuditLog.EntityKind.Comment, comment.Id);
            _db.SaveChanges();

            return comment;
        }

        public Comment AddEmployeeComment(int? employeeId, int ticketId, string text)
        {
            var employee = employeeId.HasValue ? _users.GetEmployeeById(employeeId.Value) : null;
            var ticket = employee != null ? _tickets.GetByIdForEmployee(ticketId, employee.Id) : null;

            if (employee == null || ticket == null)
            {
                return null;
            }

            var comment = employee.AddComment(ticket, text);
            _db.SaveChanges();

            AuditLogger.Log(_db, employee, AuditLog.ActionType.AddComment, AuditLog.EntityKind.Comment, comment.Id);
            _db.SaveChanges();

            return comment;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
