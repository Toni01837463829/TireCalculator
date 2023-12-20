namespace TireCalculator;

public class AsyncUtility
{
    
    
    public static async Task<Dictionary<string, List<int>>> ReadCsvAsync()
    {
        var lines = await System.IO.File.ReadAllLinesAsync("configs.csv");
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

    public static async Task<List<int[]>> FindPitCombinationsAsync(int laps, int pits, int bottomCap = 1, int topCap = int.MaxValue)
    {
        var combinations = new List<int[]>();
        await GenerateCombinationsAsync(new List<int>(), laps, pits, bottomCap, topCap, combinations);
        return combinations.Distinct().ToList();
    }

    public static async Task GenerateCombinationsAsync(List<int> currentCombination, int remainingLaps, int remainingPits, int bottom, int top, List<int[]> combinations)
    {
        // ... (no changes in this method)
    }

    public static async Task<TireStrategy> GetBestTireOptionAsync(int laps, int[] tireLimits)
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