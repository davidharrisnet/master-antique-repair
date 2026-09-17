using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class TicketRepository
    {
        private readonly RepairShopContext _db;

        public TicketRepository(RepairShopContext db)
        {
            _db = db;
        }

        public List<Ticket> GetCompleted()
        {
            return _db.Tickets
                .Where(t => t.State == State.RepairState.COMPLETED && t.CompletedDate.HasValue)
                .Include(t => t.User)
                .OrderBy(t => t.CompletedDate)
                .ToList();
        }

        public int CountCreatedBetween(DateTime startInclusive, DateTime endExclusive)
        {
            return _db.Tickets.Count(t =>
                t.SubmittedDate.HasValue && t.SubmittedDate.Value >= startInclusive && t.SubmittedDate.Value < endExclusive);
        }

        public void Add(Ticket ticket)
        {
            _db.Tickets.Add(ticket);
        }

        public Ticket GetById(int id)
        {
            return _db.Tickets.FirstOrDefault(t => t.Id == id);
        }

        public Ticket GetByIdWithCustomerAndUser(int id)
        {
            return _db.Tickets
                .Include(t => t.Customer)
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);
        }

        public Ticket GetByIdForCustomer(int ticketId, int customerId)
        {
            return _db.Tickets.FirstOrDefault(t => t.Id == ticketId && t.Customer != null && t.Customer.Id == customerId);
        }

        public Ticket GetByIdForEmployee(int ticketId, int employeeId)
        {
            return _db.Tickets.FirstOrDefault(t => t.Id == ticketId && t.User != null && t.User.Id == employeeId);
        }

        public List<Ticket> GetAllOrderedById()
        {
            return _db.Tickets.OrderBy(t => t.Id).ToList();
        }

        public List<Ticket> GetUnassigned()
        {
            return _db.Tickets.Where(t => t.User == null).OrderBy(t => t.Id).ToList();
        }

        public List<Ticket> GetUnassignedWithCustomer()
        {
            return _db.Tickets
                .Include(t => t.Customer)
                .Where(t => t.User == null)
                .OrderBy(t => t.Id)
                .ToList();
        }

        public List<Ticket> GetByCustomer(int customerId)
        {
            return _db.Tickets
                .Where(t => t.Customer != null && t.Customer.Id == customerId)
                .OrderByDescending(t => t.Id)
                .ToList();
        }

        public List<Ticket> GetByEmployee(int employeeId)
        {
            return _db.Tickets
                .Where(t => t.User != null && t.User.Id == employeeId)
                .OrderBy(t => t.Id)
                .ToList();
        }

        public List<Ticket> SearchByDescriptionContains(string text)
        {
            return _db.Tickets
                .Include(t => t.Customer)
                .Where(t => t.Description.Contains(text))
                .ToList();
        }
    }
}
