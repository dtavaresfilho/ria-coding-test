using ria_coding_test_part2.Model;

namespace ria_coding_test_part2.Services
{
    public interface ICustomerService
    {
        IEnumerable<Customer> GetAllCustomers();
        void AddCustomers(IEnumerable<Customer> customers);
        void ResetCustomerStorage();
    }
}
