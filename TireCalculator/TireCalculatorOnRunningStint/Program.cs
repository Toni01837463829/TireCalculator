using TireCalculator;

class Program
{
    static void Main()
    {
        int pitstopTime = 30;
        int lapsOnCurrentTire = 0;
        int laps = 0;
        int maxPits = 0;
        int topCap = 0;
        int bottomCap = 0;
        string currentTire = "";
        List<int[]> options = null;
        bool correctInput = false;
        while (correctInput == false)
        {
            Console.Write("Tire you're currently on: (S)oft, (M)edium, (H)ard: ");
            currentTire = Console.ReadLine();
            currentTire = currentTire.ToLower();
            if (currentTire == "s" || currentTire == "m" || currentTire == "h")
                correctInput = true;
            else
                Console.WriteLine("Please enter a valid tire.");
        }
        correctInput = false;
        while (correctInput == false)
        {
            Console.Write("Number of laps on current tyre: ");
            var lapsstr = Console.ReadLine();
            correctInput = int.TryParse(lapsstr, out lapsOnCurrentTire);
            if (correctInput == false)
                Console.WriteLine("Please enter a valid number.");
        }
        correctInput = false;
        while (correctInput == false)
        {
            Console.Write("Number of laps remaining: ");
            var lapsstr = Console.ReadLine();
            correctInput = int.TryParse(lapsstr, out laps);
            if (correctInput == false)
                Console.WriteLine("Please enter a valid number.");
        }
        correctInput = false;
        while (correctInput == false)
        {
            Console.Write("Maximum number of pitstops: ");
            var pitsstr = Console.ReadLine();
            correctInput = int.TryParse(pitsstr, out maxPits);
            if (correctInput == false)
                Console.WriteLine("Please enter a valid number.");
        }

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
        
        
        
        
        try
        {
            Console.WriteLine("------------------------- nCSV Reading -----------------------");
            Console.WriteLine("trying to read from configs.csv ...");
            var df = Utility.ReadCsv();
            pitstopTime = Convert.ToInt32(df["pitstop_time"][0]);
            Console.WriteLine("read successfully");
        }
        catch (Exception e)
        {

            Console.WriteLine("Error reading configs:");
            //Console.WriteLine(e);
            Console.WriteLine("Using default configs ...");
            
        }
        Console.WriteLine("-------------------- Configurations -------------------------");
        var tireLimit = Utility.CalculateTireLimit();
        Console.WriteLine($"Calculated tire limit: {tireLimit[0]} and {tireLimit[1]}");
        Console.WriteLine($"current tire: {currentTire}");
        Console.WriteLine($"Laps on current tyre: {lapsOnCurrentTire}");
        Console.WriteLine($"Laps remaining: {laps}");
        Console.WriteLine($"Pits: {maxPits}");
        Console.WriteLine($"Bottom cap: {bottomCap}");
        Console.WriteLine($"Top cap: {topCap}");
        Console.WriteLine($"Pitstop time: {pitstopTime}");
        Console.WriteLine("-----------------------------------------------------------\n");
        
        
        
        
        
        
    }
}