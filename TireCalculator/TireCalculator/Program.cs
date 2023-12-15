using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        int pitstopTime = 30;
        int laps = 0;
        int maxPits = 0;
        int topCap = 0;
        int bottomCap = 0;
        List<int[]> options = null;

        try
        {
            Console.WriteLine("trying to read from configs.csv ...");
            var df = ReadCsv();
            pitstopTime = Convert.ToInt32(df["pitstop_time"][0]);
            Console.WriteLine("read successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine("Error reading configs:");
            Console.WriteLine(e);
            Console.WriteLine("Using default configs ...");
            Console.WriteLine("-----------------------------------------------------------");
        }

        var tireLimit = CalculateTireLimit();
        Console.WriteLine($"Calculated tire limit: {tireLimit[0]} and {tireLimit[1]}");

        while (true)
        {
            Console.Write("Calculate for one pitstop or all? (o/a) | (q) to quit: ");
            string oneOrAll = Console.ReadLine().ToString().ToLower();

            if (oneOrAll == "o")
            {
                var strategies = new List<TireStrategy>();
                laps = Convert.ToInt32(Console.ReadLine());
                var pits = Convert.ToInt32(Console.ReadLine()) + 1;
                Console.Write("set Minimum/Maximum Laps with 1 tire? (y/n) ");
                var setCaps = Console.ReadLine().ToLower();

                if (setCaps == "y")
                {
                    bottomCap = Convert.ToInt32(Console.ReadLine());
                    topCap = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    bottomCap = 1;
                    topCap = laps;
                    Console.WriteLine($"Using default caps {bottomCap} and {topCap}");
                }

                Console.WriteLine("your configurations:");
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

                if (pits == 0 || laps / pits <= bottomCap || laps / pits >= topCap)
                {
                    Console.WriteLine("Laps per stint not capped");
                    options = FindPitCombinations(laps, pits, 1, laps);
                }
                else
                {
                    Console.WriteLine($"Laps per stint capped at {bottomCap} and {topCap}");
                    options = FindPitCombinations(laps, pits, bottomCap, topCap);
                }

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
                    var strat = new TireStrategy();
                    var stints = new List<TireStrategy>();

                    for (var stint = 0; stint < option.Length; stint++)
                    {
                        stints.Add(GetBestTireOption(option[stint], tireLimit));
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
                    }

                    strategies.Add(strat);
                }

                var timeToCalculateStrategy = (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine($"Time to calculate all Strategies {Math.Round(timeToCalculateStrategy - timeToCalculateOptions, 3)} seconds ");
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

                Console.WriteLine("your configurations:");
                Console.WriteLine($"Laps: {laps}");
                Console.WriteLine($"Pits: {maxPits - 1}");
                Console.WriteLine($"Bottom cap: {bottomCap}");
                Console.WriteLine($"Top cap: {topCap}");
                Console.WriteLine($"Pitstop time: {pitstopTime}");
                Console.WriteLine("-----------------------------------------------------------\n");
                var startTime = DateTime.Now;

                var bestTimes = new List<TireStrategy>();

                for (var pits = 1; pits <= maxPits; pits++)
                {
                    var strategies = new List<TireStrategy>();
                    Console.WriteLine($"Pits {pits - 1}:");
                    Console.Write($"\r{pits} out of {maxPits} completed");


                    // -----------------------------------------
                    // option calculation
                    // -----------------------------------------
                    if (laps / pits <= bottomCap || laps / pits >= topCap)
                    {
                        options = FindPitCombinations(laps, pits, 1, laps);
                    }
                    else
                    {
                        options = FindPitCombinations(laps, pits, bottomCap, topCap);
                    }

                    var timeToCalculateOptions = (DateTime.Now - startTime).TotalSeconds;

                    // -----------------------------------------
                    // calculating Strategies
                    // ----------------------------------------- 
                    foreach (var option in options)
                    {
                        

                        var strat = new TireStrategy();
                        var stints = new List<TireStrategy>();

                        for (var stint = 0; stint < option.Length; stint++)
                        {
                            stints.Add(GetBestTireOption(option[stint], tireLimit));
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

                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine("best times:");

                for (var i = 0; i < bestTimes.Count; i++)
                {
                    Console.WriteLine($"Pits {i}: {ToPrettyTime(bestTimes[i].Time)}");
                }

                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine("overall best strategy:");
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

    class TireStrategy
    {
        public int SLaps { get; set; }
        public int MLaps { get; set; }
        public int HLaps { get; set; }
        public double Time { get; set; }
        public int Pitstops { get; set; }
        public string Stint { get; set; }
        public double TimeInPit { get; set; }

        public override string ToString()
        {
            return $"Soft Laps: {SLaps} | Medium Laps: {MLaps} | Hard Laps: {HLaps}\n" +
                   $"Time for Race: {ToPrettyTime(Time)}\n" +
                   $"Pitstops: {Pitstops}\n" +
                   $"Stints:\n{Stint}";
        }
    }

    static TireStrategy GetBestTireOption(int laps, int[] tireLimits)
    {
        var result = new TireStrategy();

        if (laps <= tireLimits[0])
        {
            result.SLaps = laps;
            result.Time = Enumerable.Range(1, laps).Sum(i => Soft(i));
        }
        else if (laps <= tireLimits[1])
        {
            result.MLaps = laps;
            result.Time = Enumerable.Range(1, laps).Sum(i => Medium(i));
        }
        else
        {
            result.HLaps = laps;
            result.Time = Enumerable.Range(1, laps).Sum(i => Hard(i));
        }

        return result;
    }

    static double Soft(int x)
    {
        return 117.9 + Math.Exp(0.23 * x - 1.3);
    }

    static double Medium(int x)
    {
        return 120.5 + Math.Exp(0.169 * x - 3.2);
    }

    static double Hard(int x)
    {
        return 121.8 + Math.Exp(0.092 * x - 2.3);
    }

    static string ToPrettyTime(double seconds)
    {
        return $"{(int)(seconds / 3600)}h{(int)((seconds % 3600) / 60)}m{(int)(seconds % 60)}s";
    }

    static Dictionary<string, List<int>> ReadCsv()
    {
        var lines = System.IO.File.ReadAllLines("configs.csv");
        var header = lines[0].Split(',');

        var data = new Dictionary<string, List<int>>();
        foreach (var row in lines.Skip(1))
        {
            var values = row.Split(',');
            for (var i = 0; i < header.Length; i++)
            {
                var columnName = header[i];
                var columnValue = Convert.ToInt32(values[i]);

                if (!data.ContainsKey(columnName))
                    data[columnName] = new List<int>();

                data[columnName].Add(columnValue);
            }
        }

        return data;
    }

    static int[] CalculateTireLimit()
    {
        var tireLimit = new int[2];
        // find out when medium is faster than soft
        for (var i = 1; i < 100; i++)
        {
            if (Medium(i) < Soft(i))
            {
                tireLimit[0] = i;
                break;
            }
        }
        // find out when hard is faster than medium
        for (var i = 1; i < 100; i++)
        {
            if (Hard(i) < Medium(i))
            {
                tireLimit[1] = i;
                break;
            }
        }

        return tireLimit;
    }

    static List<int[]> FindPitCombinations(int laps, int pits, int bottomCap = 1, int topCap = int.MaxValue)
    {
        var combinations = new List<int[]>();
        GenerateCombinations(new List<int>(), laps, pits, bottomCap, topCap, combinations);
        return combinations.Distinct().ToList();
    }

    static void GenerateCombinations(List<int> currentCombination, int remainingLaps, int remainingPits, int bottom, int top, List<int[]> combinations)
    {
        if (remainingPits == 0)
        {
            if (remainingLaps == 0)
            {
                combinations.Add(currentCombination.OrderBy(x => x).ToArray());
            }

            return;
        }

        for (var i = bottom; i <= Math.Min(remainingLaps, top); i++)
        {
            currentCombination.Add(i);
            GenerateCombinations(new List<int>(currentCombination), remainingLaps - i, remainingPits - 1, i, top, combinations);
            currentCombination.RemoveAt(currentCombination.Count - 1);
        }
    }
}
