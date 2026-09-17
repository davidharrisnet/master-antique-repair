using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace MasterAntiqueRepair
{
    // Employee/Customer account management for Managers: add, edit, soft-delete.
    // Every failure path throws ArgumentException with the exact message the page
    // already showed, so the Controller's existing catch-and-display pattern needs
    // no changes.
    public class AccountService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly UserRepository _users;

        public AccountService() : this(new RepairShopContext())
        {
        }

        public AccountService(RepairShopContext db)
        {
            _db = db;
            _users = new UserRepository(_db);
        }

        public List<Employee> GetActiveEmployees()
        {
            return _users.GetActiveEmployeesOrderedByName();
        }

        public List<Customer> GetActiveCustomers()
        {
            return _users.GetActiveCustomers();
        }

        public Employee GetEmployeeById(int id)
        {
            return _users.GetEmployeeById(id);
        }

        public Customer GetCustomerById(int id)
        {
            return _users.GetCustomerById(id);
        }

        public Employee AddEmployee(int? managerId, string name, string password, string confirmPassword)
        {
            ValidateAccountInput(name, password, confirmPassword);

            if (_users.ExistsActiveByName(name))
            {
                throw new ArgumentException("That username is already taken.");
            }

            var employee = new Employee { Name = name, CreatedAt = DateTime.Now };
            employee.SetPassword(password);

            _users.Add(employee);
            SaveWithDuplicateHandling();

            LogIfManager(managerId, AuditLog.ActionType.CreateUser, employee.Id);
            return employee;
        }

        public Employee EditEmployee(int? managerId, int id, string name, string newPassword, string confirmPassword)
        {
            ValidateAccountInput(name, newPassword, confirmPassword);

            var employee = _users.GetEmployeeById(id);
            if (employee == null)
            {
                throw new EntityNotFoundException("That employee no longer exists.");
            }

            if (_users.ExistsActiveByName(name, excludeId: employee.Id))
            {
                throw new ArgumentException("That username is already taken.");
            }

            employee.Name = name;
            if (!string.IsNullOrEmpty(newPassword))
            {
                employee.SetPassword(newPassword);
            }

            SaveWithDuplicateHandling();

            LogIfManager(managerId, AuditLog.ActionType.EditUser, employee.Id);
            return employee;
        }

        // Soft delete only - their existing Tickets, Comments, and AuditLog entries all
        // keep pointing at this same User row, so history/attribution is unaffected.
        public Employee DeleteEmployee(int? managerId, int id)
        {
            var employee = _users.GetEmployeeById(id);
            if (employee == null)
            {
                throw new EntityNotFoundException("That employee no longer exists.");
            }

            employee.Delete();
            _db.SaveChanges();

            LogIfManager(managerId, AuditLog.ActionType.DeleteUser, employee.Id);
            return employee;
        }

        public Customer AddCustomer(int? managerId, string name, string password, string confirmPassword)
        {
            ValidateAccountInput(name, password, confirmPassword);

            if (_users.ExistsActiveByName(name))
            {
                throw new ArgumentException("That username is already taken.");
            }

            var customer = new Customer { Name = name, CreatedAt = DateTime.Now };
            customer.SetPassword(password);

            _users.Add(customer);
            SaveWithDuplicateHandling();

            LogIfManager(managerId, AuditLog.ActionType.CreateUser, customer.Id);
            return customer;
        }

        public Customer EditCustomer(int? managerId, int id, string name, string newPassword, string confirmPassword)
        {
            ValidateAccountInput(name, newPassword, confirmPassword);

            var customer = _users.GetCustomerById(id);
            if (customer == null)
            {
                throw new EntityNotFoundException("That customer no longer exists.");
            }

            if (_users.ExistsActiveByName(name, excludeId: customer.Id))
            {
                throw new ArgumentException("That username is already taken.");
            }

            customer.Name = name;
            if (!string.IsNullOrEmpty(newPassword))
            {
                customer.SetPassword(newPassword);
            }

            SaveWithDuplicateHandling();

            LogIfManager(managerId, AuditLog.ActionType.EditUser, customer.Id);
            return customer;
        }

        public Customer DeleteCustomer(int? managerId, int id)
        {
            var customer = _users.GetCustomerById(id);
            if (customer == null)
            {
                throw new EntityNotFoundException("That customer no longer exists.");
            }

            customer.Delete();
            _db.SaveChanges();

            LogIfManager(managerId, AuditLog.ActionType.DeleteUser, customer.Id);
            return customer;
        }

        private static void ValidateAccountInput(string name, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The user name field is required.");
            }

            if (password != confirmPassword)
            {
                throw new ArgumentException("The password and confirmation password do not match.");
            }
        }

        // The application-level ExistsActiveByName check above can still lose a race
        // between two near-simultaneous requests - it's a check-then-write, not atomic.
        // The database's own unique index on active usernames is the real backstop; this
        // just turns the resulting DbUpdateException into the same friendly message
        // instead of an unhandled 500.
        private void SaveWithDuplicateHandling()
        {
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                throw new ArgumentException("That username is already taken.");
            }
        }

        private static bool IsDuplicateUsernameViolation(DbUpdateException ex)
        {
            var sqlEx = ex.GetBaseException() as System.Data.SqlClient.SqlException;
            return sqlEx != null && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }

        private void LogIfManager(int? managerId, AuditLog.ActionType action, int entityId)
        {
            if (!managerId.HasValue)
            {
                return;
            }

            var manager = _users.GetManagerById(managerId.Value);
            if (manager == null)
            {
                return;
            }

            AuditLogger.Log(_db, manager, action, AuditLog.EntityKind.User, entityId);
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
