using ria_coding_test_part2.Data;
using ria_coding_test_part2.Model;
using System.Text.Json;

namespace ria_coding_test_part2.Services
{
    public class CustomerService : ICustomerService
    {
        // Database context to access the EF Core storage
        private readonly AppDbContext _context;

        // In-memory list to hold sorted customers
        private readonly List<Customer> _customers = new();

        // Track the highest used ID for validation
        private int _lastId = 0;

        // Lock object for thread safety
        private readonly object _lock = new();

        // Constructor initializes the service and loads customers from the database
        public CustomerService(AppDbContext context)
        {
            _context = context;

            // Load customers from DB into memory
            LoadCustomersFromDb();

            // Set _lastId to highest ID from loaded customers
            _lastId = _customers.Any() ? _customers.Max(c => c.Id) : 0;
        }

        // Return a thread-safe copy of all customers
        public IEnumerable<Customer> GetAllCustomers()
        {
            lock (_lock)
            {
                return new List<Customer>(_customers);
            }
        }

        // Add a collection of new customers with validation and persistence
        public void AddCustomers(IEnumerable<Customer> customers)
        {
            lock (_lock)
            {
                // Validate all customers first
                foreach (var customer in customers)
                {
                    // Reject reused IDs
                    if (customer.Id <= _lastId)
                    {
                        throw new ArgumentException($"ID {customer.Id} has already been used.");
                    }

                    // Enforce minimum age
                    if (customer.Age <= 18)
                    {
                        throw new ArgumentException($"Customer {customer.FirstName} {customer.LastName} must be over 18 years old.");
                    }

                    // Require both first and last name
                    if (string.IsNullOrEmpty(customer.FirstName) || string.IsNullOrEmpty(customer.LastName))
                    {
                        throw new ArgumentException("First name and last name are required.");
                    }
                }

                // If all customers are valid, add them in sorted order
                foreach (var customer in customers)
                {
                    InsertCustomerSorted(customer);
                    _lastId = Math.Max(_lastId, customer.Id);
                }

                // Persist updated list to database
                SaveCustomersToDb();
            }
        }

        // Insert a customer into the in-memory list at a sorted position
        private void InsertCustomerSorted(Customer customer)
        {
            int index = 0;

            // Find the correct position based on last name and then first name
            while (index < _customers.Count)
            {
                var existing = _customers[index];

                // Compare by last name
                int lastNameComparison = string.Compare(existing.LastName, customer.LastName, StringComparison.Ordinal);

                // If last name is greater, or same but first name is greater, insert here
                if (lastNameComparison > 0 ||
                   (lastNameComparison == 0 &&
                    string.Compare(existing.FirstName, customer.FirstName, StringComparison.Ordinal) > 0))
                {
                    break;
                }

                index++;
            }

            // Insert customer at the determined index
            _customers.Insert(index, customer);
        }

        // Load customers from the single-record DB table
        private void LoadCustomersFromDb()
        {
            // Fetch the only CustomerStorage record
            var record = _context.CustomerStorage.FirstOrDefault();

            // If it exists and has data, deserialize and load into memory
            if (record != null && !string.IsNullOrWhiteSpace(record.Data))
            {
                var loaded = JsonSerializer.Deserialize<List<Customer>>(record.Data);

                if (loaded != null)
                {
                    _customers.AddRange(loaded);
                }
            }
        }

        // Save the current in-memory customer list to the database
        private void SaveCustomersToDb()
        {
            // Serialize the list to JSON
            var json = JsonSerializer.Serialize(_customers);

            // Fetch the existing storage row
            var existing = _context.CustomerStorage.FirstOrDefault();

            if (existing != null)
            {
                // Update its JSON data
                existing.Data = json;
                _context.CustomerStorage.Update(existing);
            }
            else
            {
                // Add a new row if none exists
                _context.CustomerStorage.Add(new CustomerStorage { Data = json });
            }

            // Persist changes to the database
            _context.SaveChanges();
        }

        public void ResetCustomerStorage()
        {
            lock (_lock)
            {
                _context.CustomerStorage.RemoveRange(_context.CustomerStorage);

                _context.CustomerStorage.Add(new CustomerStorage { Id = 1, Data = "[]" });

                _customers.Clear();
                _lastId = 0;

                _context.SaveChanges();
            }
        }
    }
}
