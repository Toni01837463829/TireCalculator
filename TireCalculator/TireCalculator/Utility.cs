namespace TireCalculator;

public class Utility
{
    public static double Soft(int x)
    {
        return 117.9 + Math.Exp(0.23 * x - 1.3);
    }

    public static double Medium(int x)
    {
        return 120.5 + Math.Exp(0.169 * x - 3.2);
    }

    public static double Hard(int x)
    {
        return 121.8 + Math.Exp(0.092 * x - 2.3);
    }

    public static string ToPrettyTime(double seconds)
    {
        return $"{(int)(seconds / 3600)}h{(int)((seconds % 3600) / 60)}m{(int)(seconds % 60)}s";
    }

    public static Dictionary<string, List<int>> ReadCsv()
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

    public static int[] CalculateTireLimit()
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

    public static List<int[]> FindPitCombinations(int laps, int pits, int bottomCap = 1, int topCap = int.MaxValue)
    {
        var combinations = new List<int[]>();
        GenerateCombinations(new List<int>(), laps, pits, bottomCap, topCap, combinations);
        return combinations.Distinct().ToList();
    }

    public static void GenerateCombinations(List<int> currentCombination, int remainingLaps, int remainingPits, int bottom, int top, List<int[]> combinations)
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
    
    public  static TireStrategy GetBestTireOption(int laps, int[] tireLimits)
    {
        var result = new TireStrategy();

        if (laps <= tireLimits[0])
        {
            result.SLaps = laps;
            result.Time = Enumerable.Range(1, laps).Sum(i => Utility.Soft(i));
        }
        else if (laps <= tireLimits[1])
        {
            result.MLaps = laps;
            result.Time = Enumerable.Range(1, laps).Sum(i => Utility.Medium(i));
        }
        else
        {
            result.HLaps = laps;
            result.Time = Enumerable.Range(1, laps).Sum(i => Utility.Hard(i));
        }

        return result;
    }
}