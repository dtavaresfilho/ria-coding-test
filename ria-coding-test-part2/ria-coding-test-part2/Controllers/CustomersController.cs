using Microsoft.AspNetCore.Mvc;
using ria_coding_test_part2.Model;
using ria_coding_test_part2.Services;

namespace ria_coding_test_part2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_customerService.GetAllCustomers());
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<Customer> customers)
        {
            try
            {
                _customerService.AddCustomers(customers);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ResetDatabase")]
        public IActionResult ResetDatabase()
        {
            try
            {
                _customerService.ResetCustomerStorage();
                return Ok("Customer storage has been reset.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
