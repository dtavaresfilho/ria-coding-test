using System.Net.Http.Json;

namespace Simulator
{
    internal class Program
    {
        private static readonly HttpClient _httpClient;

        // Random number generator for creating test data
        private static readonly Random _random = new Random();

        // Tracks the next available customer ID
        private static int _nextId = 1;

        // Pool of possible first names for generated customers
        private static readonly string[] _firstNames =
        {
            "Leia", "Sadie", "Jose", "Sara", "Frank",
            "Dewey", "Tomas", "Joel", "Lukas", "Carlos"
        };

        // Pool of possible last names for generated customers
        private static readonly string[] _lastNames =
        {
            "Liberty", "Ray", "Harrison", "Ronan", "Drew",
            "Powell", "Larsen", "Chan", "Anderson", "Lane"
        };

        static Program()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://ria-coding-test.onrender.com/api/"), // API base address
                //BaseAddress = new Uri("https://localhost:44341/api/"), // API base address
                Timeout = TimeSpan.FromSeconds(120) // Request timeout
            };
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Customer Simulator...");

            try
            {
                // Synchronize the next ID with server's current state
                await SyncNextId();
                Console.WriteLine($"Starting with ID: {_nextId}");

                // Run the simulation with 10 pairs of POST/GET requests
                Console.WriteLine("Sending 10 POST and GET requests in parallel...");
                await RunSimulation(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Simulation failed: {ex.Message}");
            }

            Console.WriteLine("Simulation completed. Press any key to exit...");
            Console.ReadKey();
        }

        // Runs the simulation with specified number of request pairs
        private static async Task RunSimulation(int numberOfRequests)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < numberOfRequests; i++)
            {
                // Add POST and GET requests to task list
                tasks.Add(SendPostRequest());
                tasks.Add(SendGetRequest());

                // Small delay between starting requests
                await Task.Delay(200);
            }

            // Wait for all requests to complete
            await Task.WhenAll(tasks);
        }

        // Synchronizes the next ID with server's current maximum ID
        private static async Task SyncNextId()
        {
            try
            {
                // Get current customers from server
                var response = await _httpClient.GetAsync("customers");
                response.EnsureSuccessStatusCode();

                // Parse response and calculate next available ID
                var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
                _nextId = customers?.Count > 0 ? customers.Max(c => c.Id) + 1 : 1;
            }
            catch (Exception ex)
            {
                // Fallback to ID 1 if synchronization fails
                Console.WriteLine($"Warning: Could not sync ID with server. {ex.Message}");
                Console.WriteLine("Starting with default ID: 1");
                _nextId = 1;
            }
        }

        // Sends a POST request with random customer data
        private static async Task SendPostRequest()
        {
            try
            {
                // Generate random customers
                var customers = GenerateRandomCustomers();

                // Send POST request
                var response = await _httpClient.PostAsJsonAsync("customers", customers);

                // Handle failed responses
                if (!response.IsSuccessStatusCode)
                {
                    await HandleFailedRequest(response, customers.Count);
                    return;
                }

                // Log success
                Console.WriteLine($"POST success: Added {customers.Count} customers (IDs: {string.Join(",", customers.Select(c => c.Id))})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"POST failed: {ex.Message}");
            }
        }

        // Generates a list of random customers
        private static List<Customer> GenerateRandomCustomers()
        {
            var customers = new List<Customer>();

            // Generate between 2-5 customers per request
            int count = _random.Next(2, 6);

            for (int i = 0; i < count; i++)
            {
                customers.Add(new Customer
                {
                    Id = _nextId++, // Assign and increment ID
                    FirstName = _firstNames[_random.Next(_firstNames.Length)], // Random first name
                    LastName = _lastNames[_random.Next(_lastNames.Length)], // Random last name
                    Age = _random.Next(10, 91) // Random age (10-90)
                });
            }

            return customers;
        }

        // Handles failed POST requests
        private static async Task HandleFailedRequest(HttpResponseMessage response, int customersCount)
        {
            // Read error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"POST failed ({(int)response.StatusCode}): {errorContent}");

            // Revert unused IDs since the request failed
            _nextId -= customersCount;
        }

        // Sends a GET request to retrieve all customers
        private static async Task SendGetRequest()
        {
            try
            {
                // Send GET request
                var response = await _httpClient.GetAsync("customers");
                response.EnsureSuccessStatusCode();

                // Parse and display results
                var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
                Console.WriteLine($"GET success: Retrieved {customers?.Count ?? 0} customers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GET failed: {ex.Message}");
            }
        }
    }

    // Customer data model
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}