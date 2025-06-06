class Program
{
    // Defines the available coin denominations
    static int[] coins = { 10, 50, 100 };

    static void Main()
    {
        // List of target amounts we want to reach using coin combinations
        int[] targets = { 30, 50, 60, 80, 140, 230, 370, 610, 980 };

        // Loop through each target amount
        foreach (int target in targets)
        {
            // Print the target amount to the console
            Console.WriteLine($"Target amount: {target} EUR");

            // List to store all valid combinations of coins that sum to the target
            List<List<int>> combinations = new();

            // Start the recursive search for coin combinations
            FindCoinCombinations(target, new List<int>(), combinations);

            // Loop through and print each valid combination found
            foreach (var combo in combinations)
            {
                // Format and display the combination
                Console.WriteLine($"{FormatCombination(combo)}");
            }

            // Print a blank line for spacing between results
            Console.WriteLine();
        }
    }

    // Recursive function to find all valid combinations of coins that sum to a given amount
    static void FindCoinCombinations(int remaining, List<int> current, List<List<int>> results)
    {
        // If there's no remaining amount, we have a valid combination
        if (remaining == 0)
        {
            // Add a copy of the current combination to the results
            results.Add(new List<int>(current));
            return; // Exit this recursive call
        }

        // Try adding each coin to the current combination
        foreach (int coin in coins)
        {
            // If there's at least one coin in the current combination
            if (current.Count > 0)
            {
                // Skip coins smaller than the last added one to avoid duplicate combinations in different orders
                if (coin < current[current.Count - 1])
                {
                    continue;
                }
            }

            // Only proceed if the coin does not exceed the remaining amount
            if (remaining >= coin)
            {
                // Add the coin to the current combination
                current.Add(coin);

                // Recursively try to complete the combination with the new remaining value
                FindCoinCombinations(remaining - coin, current, results);

                // Remove the last added coin to try the next option
                current.RemoveAt(current.Count - 1);
            }
        }
    }

    // Formats a combination of coins as a readable string, grouping and counting each coin type
    static string FormatCombination(List<int> combo)
    {
        // Dictionary to count the number of times each coin appears in the combination
        Dictionary<int, int> count = new();

        // Count each coin in the combination
        foreach (int coin in combo)
        {
            if (!count.ContainsKey(coin))
            {
                count[coin] = 0;
            }

            count[coin]++;
        }

        // Convert the counts into a readable string like "2 x 50 EUR + 1 x 100 EUR"
        return string.Join(" + ", count.Select(kvp => $"{kvp.Value} x {kvp.Key} EUR"));
    }
}
