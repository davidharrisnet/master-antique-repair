using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterAntiqueRepair
{
    // Backs the Search page's four tabs (Ticket/Customer/Employee/Comment). Customer and
    // Employee lookups are deliberately unfiltered (include soft-deleted accounts) - a
    // Manager investigating a ticket needs to find historical records too, not just
    // currently-active ones.
    public class SearchService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly TicketRepository _tickets;
        private readonly UserRepository _users;
        private readonly CommentRepository _comments;

        public SearchService()
        {
            _db = new RepairShopContext();
            _tickets = new TicketRepository(_db);
            _users = new UserRepository(_db);
            _comments = new CommentRepository(_db);
        }

        public List<Ticket> GetTicketDropdownList()
        {
            return _tickets.GetAllOrderedById();
        }

        public Ticket GetTicketDetail(int id)
        {
            return _tickets.GetByIdWithCustomerAndUser(id);
        }

        public List<Customer> GetCustomerDropdownList()
        {
            return _users.GetAllCustomersOrderedById();
        }

        public Customer GetCustomerById(int id)
        {
            return _users.GetCustomerById(id);
        }

        public List<Ticket> GetTicketsForCustomer(int id)
        {
            return _tickets.GetByCustomer(id);
        }

        public List<Employee> GetEmployeeDropdownList()
        {
            return _users.GetAllEmployeesOrderedById();
        }

        public Employee GetEmployeeById(int id)
        {
            return _users.GetEmployeeById(id);
        }

        public List<Ticket> GetTicketsForEmployee(int id)
        {
            return _tickets.GetByEmployee(id);
        }

        public List<CommentSearchResult> SearchComments(string text)
        {
            var commentMatches = _comments.SearchByTextContains(text)
                .Select(c => new CommentSearchResult
                {
                    Type = "Comment",
                    Text = c.Text,
                    AuthorName = c.User.Name,
                    Posted = c.CreatedAt,
                    TicketId = c.TicketId
                });

            var descriptionMatches = _tickets.SearchByDescriptionContains(text)
                .Select(t => new CommentSearchResult
                {
                    Type = "Ticket Description",
                    Text = t.Description,
                    AuthorName = t.Customer.Name,
                    Posted = t.SubmittedDate ?? DateTime.MinValue,
                    TicketId = t.Id
                });

            return commentMatches
                .Concat(descriptionMatches)
                .OrderByDescending(r => r.Posted)
                .ToList();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
