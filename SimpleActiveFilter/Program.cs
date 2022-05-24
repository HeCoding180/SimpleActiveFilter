namespace SimpleActiveFilter
{
	public class Program
	{
		public static void Main()
		{
			//Example Code
			ActiveFilterEngine filterEngine;

			Console.Write("Enter Filter Type [LP] for LowPass [HP] for HighPass [BP] for BandPass: ");
			string ftypeStr = Console.ReadLine().Replace("\n", "");
			FilterType ftype = FilterType.BP;
			switch (ftypeStr.ToUpper())
            {
				case "HP":
					ftype = FilterType.HP;
					break;
				case "LP":
					ftype = FilterType.LP;
					break;
            }

			Console.WriteLine("Enter Full File Path for the CSV File: ");
			string filePath = Console.ReadLine().Replace("\n", "");

			Console.WriteLine("floating point character: '" + (1.1f).ToString().Replace("1", "") + "'");
			Console.Write("Enter Value for Resistor A (Lowpass characteristic, unit=Ohm): ");
			double Ra = double.Parse(Console.ReadLine().Replace("\n", ""));
			Console.Write("Enter Value for Capacitor A (Lowpass characteristic), unit=Farad: ");
			double Ca = double.Parse(Console.ReadLine().Replace("\n", ""));
			Console.Write("Enter Value for Resistor B (Highpass characteristic), unit=Ohm: ");
			double Rb = double.Parse(Console.ReadLine().Replace("\n", ""));
			Console.Write("Enter Value for Capacitor B (Highpass characteristic), unit=Farad: ");
			double Cb = double.Parse(Console.ReadLine().Replace("\n", ""));

			Console.Write("Use Finite Gain OpAmp [1] [0]: ");
			bool FiniteGain = ParseBool(Console.ReadLine().Replace("\n", ""));

			if (FiniteGain)
            {
				Console.Write("Enter Value for the Gain Bandwidth Product: ");
				double GBP = double.Parse(Console.ReadLine().Replace("\n", ""));
				Console.Write("Enter Value for the Open Loop Gain: ");
				double AOL = double.Parse(Console.ReadLine().Replace("\n", ""));

				filterEngine = new ActiveFilterEngine(ftype, Ra, Rb, Ca, Cb, GBP, AOL);
			}
			else
            {
				filterEngine = new ActiveFilterEngine(ftype, Ra, Rb, Ca, Cb);
			}

			string csvOutputString = "Frequency,Output Gain,Phase Shift\n";

			for (int fDecade = 1; fDecade <= 10000000/*10MHz*/; fDecade *= 10)
			{
				for (double fDecadeOffset = 1; fDecadeOffset <= 9; fDecadeOffset += 1)
				{
					double actualFrequency = (fDecade * fDecadeOffset);
					csvOutputString += actualFrequency.ToString() + "," + filterEngine.GetGain(actualFrequency).ToString() + "," + ActiveFilterEngine.RadToDeg(filterEngine.GetPhase(actualFrequency)).ToString() + "\n";	
				}
			}

			File.WriteAllText(filePath, csvOutputString);
		}

	    public static bool ParseBool(string inputStr)
        {
			return (inputStr == "1") ? true : false;
		}
	}
}