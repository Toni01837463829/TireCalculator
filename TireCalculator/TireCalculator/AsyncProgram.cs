namespace TireCalculator;

public class AsyncProgram
{
    public static async Task AsyncMain()
    {
        int pitstopTime = 30;
        int laps = 0;
        int maxPits = 0;
        int topCap = 0;
        int bottomCap = 0;
        List<int[]> options = null;
        Console.WriteLine("Running AsyncMain ...");
        try
        {
            Console.WriteLine("trying to read from configs.csv ...");
            var df = await AsyncUtility.ReadCsvAsync();
            pitstopTime = Convert.ToInt32(df["pitstop_time"][0]);
            Console.WriteLine("read successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading configs:");
            // Console.WriteLine(e);
            Console.WriteLine("Using default configs ...");
        }
         Console.WriteLine("-----------------------------------------------------------");
        var tireLimit = Utility.CalculateTireLimit();
        Console.WriteLine($"Calculated tire limit: {tireLimit[0]} and {tireLimit[1]}");

        while (true)
        {
            Console.Write("Calculate for one pitstop or all? (o/a) | (q) to quit: ");
            string oneOrAll = Console.ReadLine().ToString().ToLower();

            if (oneOrAll == "o")
            {
                var strategies = new List<TireStrategy>();
                Console.Write("Laps: ");
                laps = Convert.ToInt32(Console.ReadLine());
                Console.Write("Pits: ");
                var pits = Convert.ToInt32(Console.ReadLine()) + 1;
                Console.Write("set Minimum/Maximum Laps with 1 tire? (y/n) ");
                var setCaps = Console.ReadLine().ToLower();

                if (setCaps == "y")
                {
                    Console.Write("Minmum Laps with 1 Tyre: ");
                    bottomCap = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Maximum Laps with 1 Tyre: ");
                    topCap = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    bottomCap = 1;
                    topCap = laps;
                    Console.WriteLine($"Using default caps {bottomCap} and {topCap}");
                }

                Console.WriteLine("--------------------- Your Configurations ---------------------");
                Console.WriteLine($"Laps: {laps}");
                Console.WriteLine($"Pits: {pits}");
                Console.WriteLine($"Bottom cap: {bottomCap}");
                Console.WriteLine($"Top cap: {topCap}");
                Console.WriteLine($"Pitstop time: {pitstopTime}");
                Console.WriteLine("-----------------------------------------------------------\n");
                var startTime = DateTime.Now;

                // -----------------------------------------
                // option calculation
                // -----------------------------------------
                Console.WriteLine("now calculating options...");
                Task<List<int[]>> optionsTask = new Task<List<int[]>>(() => null);
                if (pits == 0 || laps / pits <= bottomCap || laps / pits >= topCap)
                {
                    Console.WriteLine("Laps per stint not capped");
                    optionsTask = AsyncUtility.FindPitCombinationsAsync(laps, pits, 1, laps);
                }
                else
                {
                    Console.WriteLine($"Laps per stint capped at {bottomCap} and {topCap}");
                    optionsTask = AsyncUtility.FindPitCombinationsAsync(laps, pits, bottomCap, topCap);
                }

                options = optionsTask.Result;

                var timeToCalculateOptions = (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine($"calculated {options.Count} options");
                Console.WriteLine($"in {Math.Round(timeToCalculateOptions, 3)} seconds ");
                Console.WriteLine("-----------------------------------------------------------\n");

                // -----------------------------------------
                // calculating Strategies
                // ----------------------------------------- 
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine("now calculating Strategies...");

                foreach (var option in options)
                {
                    TireStrategy strat = new TireStrategy();
                    List<TireStrategy> stints = new List<TireStrategy>();
                    List<Task<TireStrategy>> stintTasks = new List<Task<TireStrategy>>();
                    for (var stint = 0; stint < option.Length; stint++)
                    {
                        stintTasks.Add(AsyncUtility.GetBestTireOptionAsync(option[stint], tireLimit));
                        stints.Add(Utility.GetBestTireOption(option[stint], tireLimit));
                    }
                    await Task.WhenAll(stintTasks);
                    stints = stintTasks.Select(x => x.Result).ToList();

                    foreach (TireStrategy stint in stints)
                    {
                        await Task.Run(() =>
                        {
                            if (stint != stints.First())
                            {
                                strat.Time += pitstopTime;
                                strat.Pitstops++;
                                strat.TimeInPit += pitstopTime;
                            }

                            strat.SLaps += stint.SLaps;
                            strat.MLaps += stint.MLaps;
                            strat.HLaps += stint.HLaps;
                            strat.Time += stint.Time;

                            if (stint.SLaps != 0)
                            {
                                strat.Stint += $"\nStint: {stint.SLaps} laps with Soft";
                            }
                            else if (stint.MLaps != 0)
                            {
                                strat.Stint += $"\nStint: {stint.MLaps} laps with Medium";
                            }
                            else
                            {
                                strat.Stint += $"\nStint: {stint.HLaps} laps with Hard";
                            }
                        });
                    }

                    strategies.Add(strat);
                }

                var timeToCalculateStrategy = (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine(
                    $"Time to calculate all Strategies {Math.Round(timeToCalculateStrategy - timeToCalculateOptions, 3)} seconds ");
                Console.WriteLine("-----------------------------------------------------------\n");

                // -----------------------------------------
                // getting best Strategy
                // ----------------------------------------- 
                Console.WriteLine("***********************************************************");
                Console.WriteLine($"total time: {Math.Round(timeToCalculateStrategy, 3)} seconds ");

                var best = new TireStrategy();
                var bestTime = double.MaxValue;

                foreach (var strategy in strategies)
                {
                    if (strategy.Time < bestTime)
                    {
                        best = strategy;
                        bestTime = strategy.Time;
                    }
                }

                Console.WriteLine("Best Strategy:");
                Console.WriteLine(best);
                Console.WriteLine("time spent in pit: ");
                Console.WriteLine(best.TimeInPit);
            }
            else if (oneOrAll == "a")
            {
                Console.Write("Laps:");
                laps = Convert.ToInt32(Console.ReadLine());
                Console.Write("Max Pitsops:");
                maxPits = Convert.ToInt32(Console.ReadLine()) + 1;
                Console.Write("set Minimum/Maximum Laps with 1 tire? (y/n) ");
                var setCaps = Console.ReadLine().ToLower();

                if (setCaps == "y")
                {
                    Console.Write("Minmum Laps with 1 Tyre:");
                    bottomCap = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Maximum Laps with 1 Tyre:");
                    topCap = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    bottomCap = 1;
                    topCap = laps;
                    Console.WriteLine($"Using default caps {bottomCap} and {topCap}");
                }

                Console.WriteLine("--------------------- Your Configurations ---------------------");
                Console.WriteLine($"Laps: {laps}");
                Console.WriteLine($"Pits: {maxPits - 1}");
                Console.WriteLine($"Bottom cap: {bottomCap}");
                Console.WriteLine($"Top cap: {topCap}");
                Console.WriteLine($"Pitstop time: {pitstopTime}");
                Console.WriteLine("calculating option");
                Console.WriteLine("--------------------- Calculating Options ---------------------\n");

                var startTime = DateTime.Now;

                var bestTimes = new List<TireStrategy>();

                for (var pits = 1; pits <= maxPits; pits++)
                {
                    var strategies = new List<TireStrategy>();

                    Console.Write($"\rcalculating option {pits} out of {maxPits} ...\n");
                    // -----------------------------------------
                    // option calculation
                    // -----------------------------------------
                    if (laps / pits <= bottomCap || laps / pits >= topCap)
                    {
                        options = Utility.FindPitCombinations(laps, pits, 1, laps);
                    }
                    else
                    {
                        options = Utility.FindPitCombinations(laps, pits, bottomCap, topCap);
                    }

                    var timeToCalculateOptions = (DateTime.Now - startTime).TotalSeconds;

                    // -----------------------------------------
                    // calculating Strategies
                    // ----------------------------------------- 
                    List<Task<TireStrategy>> tasks = new List<Task<TireStrategy>>();
                    foreach (var option in options)
                    {
                        var strat = new TireStrategy();
                        var stints = new List<TireStrategy>();

                        for (var stint = 0; stint < option.Length; stint++)
                        {
                            stints.Add(Utility.GetBestTireOption(option[stint], tireLimit));
                        }

                        foreach (var stint in stints)
                        {
                            if (stint != stints.First())
                            {
                                strat.Time += pitstopTime;
                                strat.Pitstops++;
                                strat.TimeInPit += pitstopTime;
                            }

                            strat.SLaps += stint.SLaps;
                            strat.MLaps += stint.MLaps;
                            strat.HLaps += stint.HLaps;
                            strat.Time += stint.Time;

                            if (stint.SLaps != 0)
                            {
                                strat.Stint += $"{stint.SLaps} laps with Soft\n";
                            }
                            else if (stint.MLaps != 0)
                            {
                                strat.Stint += $"{stint.MLaps} laps with Medium\n";
                            }
                            else
                            {
                                strat.Stint += $"{stint.HLaps} laps with Hard\n";
                            }
                        }

                        strategies.Add(strat);
                    }

                    var timeToCalculateStrategy = (DateTime.Now - startTime).TotalSeconds;

                    // -----------------------------------------
                    // getting best Strategy
                    // ----------------------------------------- 
                    var best = new TireStrategy();
                    var bestTime = double.MaxValue;

                    foreach (var strategy in strategies)
                    {
                        if (strategy.Time < bestTime)
                        {
                            best = strategy;
                            bestTime = strategy.Time;
                        }
                    }

                    bestTimes.Add(best);
                }

                Console.WriteLine("--------------------- Best Times ---------------------");


                for (var i = 0; i < bestTimes.Count; i++)
                {
                    Console.WriteLine($"Pits {i}: {Utility.ToPrettyTime(bestTimes[i].Time)}");
                }

                Console.WriteLine("--------------------- Overall Best ---------------------");
                var overallBest = new TireStrategy();
                var overallBestTime = double.MaxValue;

                foreach (var time in bestTimes)
                {
                    if (time.Time < overallBestTime)
                    {
                        overallBestTime = time.Time;
                        overallBest = time;
                    }
                }

                Console.WriteLine(overallBest);
                Console.WriteLine("time spent in pit: ");
                Console.WriteLine(overallBest.TimeInPit);
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine($"time to complete: {Math.Round((DateTime.Now - startTime).TotalSeconds, 3)} seconds ");
            }
            else
            {
                break;
            }
            
            
            
        }
    }
}