namespace TireCalculator;

public class TireStrategy
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
               $"Time for Race: {Utility.ToPrettyTime(Time)}\n" +
               $"Pitstops: {Pitstops}\n" +
               $"Stints:\n{Stint}";
    }
}