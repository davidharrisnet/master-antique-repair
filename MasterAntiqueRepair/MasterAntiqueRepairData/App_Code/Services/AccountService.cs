using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity;

namespace MasterAntiqueRepair
{
    // Employee/Customer account management for Managers: add, edit, soft-delete.
    // Every failure path throws ArgumentException with the exact message the page
    // already showed, so the Controller's existing catch-and-display pattern needs
    // no changes. Identity-related work (create/password reset) goes through a
    // UserManager built by IdentityConfig - no OWIN/HttpContext needed here since
    // Managers aren't signing the new/edited account in.
    public class AccountService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly UserRepository _users;
        private readonly UserManager<User, int> _userManager;

        public AccountService() : this(new RepairShopContext())
        {
        }

        public AccountService(RepairShopContext db)
        {
            _db = db;
            _users = new UserRepository(_db);
            _userManager = IdentityConfig.CreateUserManager(_db);
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

            var employee = new Employee { UserName = name, CreatedAt = DateTime.Now };
            CreateWithDuplicateHandling(employee, password);
            _userManager.AddToRole(employee.Id, IdentityConfig.EmployeeRole);

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

            RenameWithDuplicateHandling(employee, name);
            if (!string.IsNullOrEmpty(newPassword))
            {
                SetPassword(employee.Id, newPassword);
            }

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

            var customer = new Customer { UserName = name, CreatedAt = DateTime.Now };
            CreateWithDuplicateHandling(customer, password);
            _userManager.AddToRole(customer.Id, IdentityConfig.CustomerRole);

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

            RenameWithDuplicateHandling(customer, name);
            if (!string.IsNullOrEmpty(newPassword))
            {
                SetPassword(customer.Id, newPassword);
            }

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

        // UserManager.Create runs its own uniqueness check (IdentityConfig's
        // ActiveUsernameValidator) and password-policy check up front, but two
        // near-simultaneous requests can still both pass that check before either
        // commits - the database's own filtered unique index is the real backstop, so a
        // DbUpdateException from the race is still turned into the same friendly message.
        private void CreateWithDuplicateHandling(User user, string password)
        {
            IdentityResult result;
            try
            {
                result = _userManager.Create(user, password);
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                throw new ArgumentException("That username is already taken.");
            }

            if (!result.Succeeded)
            {
                throw new ArgumentException(string.Join(" ", result.Errors));
            }
        }

        private void RenameWithDuplicateHandling(User user, string name)
        {
            user.UserName = name;
            IdentityResult result;
            try
            {
                result = _userManager.Update(user);
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                throw new ArgumentException("That username is already taken.");
            }

            if (!result.Succeeded)
            {
                throw new ArgumentException(string.Join(" ", result.Errors));
            }
        }

        // Admin-driven password reset: the Manager doesn't know (and shouldn't need) the
        // old password, so this replaces it outright rather than going through
        // ChangePassword (which requires the current one).
        private void SetPassword(int userId, string newPassword)
        {
            var removeResult = _userManager.RemovePassword(userId);
            if (!removeResult.Succeeded)
            {
                throw new ArgumentException(string.Join(" ", removeResult.Errors));
            }

            var addResult = _userManager.AddPassword(userId, newPassword);
            if (!addResult.Succeeded)
            {
                throw new ArgumentException(string.Join(" ", addResult.Errors));
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
            _userManager.Dispose();
            _db.Dispose();
        }
    }
}
