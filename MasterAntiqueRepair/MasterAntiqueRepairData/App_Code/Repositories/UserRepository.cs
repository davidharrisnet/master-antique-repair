using System.Collections.Generic;
using System.Linq;

namespace MasterAntiqueRepair
{
    public class UserRepository
    {
        private readonly RepairShopContext _db;

        public UserRepository(RepairShopContext db)
        {
            _db = db;
        }

        public List<Employee> GetActiveEmployees()
        {
            return _db.Set<Employee>().Where(e => !e.DeletedAt.HasValue).ToList();
        }

        public Customer GetCustomerById(int id)
        {
            return _db.Users.OfType<Customer>().FirstOrDefault(c => c.Id == id);
        }

        public Employee GetEmployeeById(int id)
        {
            return _db.Users.OfType<Employee>().FirstOrDefault(e => e.Id == id);
        }

        public Manager GetManagerById(int id)
        {
            return _db.Users.OfType<Manager>().FirstOrDefault(m => m.Id == id);
        }

        public User GetById(int id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }

        public User GetByName(string name)
        {
            return _db.Users.FirstOrDefault(u => u.UserName == name);
        }

        public List<Customer> GetActiveCustomers()
        {
            return _db.Users.OfType<Customer>().Where(c => !c.DeletedAt.HasValue).OrderBy(c => c.UserName).ToList();
        }

        public List<Employee> GetActiveEmployeesOrderedByName()
        {
            return _db.Users.OfType<Employee>().Where(e => !e.DeletedAt.HasValue).OrderBy(e => e.UserName).ToList();
        }

        // Unfiltered (includes soft-deleted) - a Manager searching by id should still be
        // able to find a historical record, not just currently-active accounts.
        public List<Customer> GetAllCustomersOrderedById()
        {
            return _db.Users.OfType<Customer>().OrderBy(c => c.Id).ToList();
        }

        public List<Employee> GetAllEmployeesOrderedById()
        {
            return _db.Users.OfType<Employee>().OrderBy(e => e.Id).ToList();
        }

        public bool ExistsActiveByName(string name, int? excludeId = null)
        {
            return _db.Users.Any(u => u.UserName == name && !u.DeletedAt.HasValue && (!excludeId.HasValue || u.Id != excludeId.Value));
        }
    }
}
