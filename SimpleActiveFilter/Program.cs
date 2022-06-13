namespace SimpleActiveFilter
{
	public class Program
	{
		public static void Main()
		{
			ActiveFilterEngine filterEngine;

			//Example Code
			Console.Write("Enter Filter Type [LP] for LowPass [HP] for HighPass [BP] for BandPass: ");
			string ftypeStr = Console.ReadLine().Replace("\n", "").ToUpper();
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

			double Ca;
			double Cb;

			Console.WriteLine("floating point character: '" + (1.1f).ToString().Replace("1", "") + "'");
			Console.Write("Enter Value for Resistor A (Lowpass characteristic, unit=Ohm): ");
			double Ra = double.Parse(Console.ReadLine().Replace("\n", ""));
			if (ftype != FilterType.HP)
			{
				Console.Write("Enter Value for Capacitor A (Lowpass characteristic), unit=Farad: ");
				Ca = double.Parse(Console.ReadLine().Replace("\n", ""));
			}
			else
			{
				Ca = 1.0f;
			}
			Console.Write("Enter Value for Resistor B (Highpass characteristic), unit=Ohm: ");
			double Rb = double.Parse(Console.ReadLine().Replace("\n", ""));
			if (ftype != FilterType.LP)
			{
				Console.Write("Enter Value for Capacitor B (Highpass characteristic), unit=Farad: ");
				Cb = double.Parse(Console.ReadLine().Replace("\n", ""));
			}
			else
			{
				Cb = 1.0f;
			}

			Console.Write("Use Finite Gain OpAmp [1] [0]: ");
			bool FiniteGain = ParseBool(Console.ReadLine().Replace("\n", ""));

			if (FiniteGain)
            {
				Console.Write("Enter Value for the Unity-gain bandwidth: ");
				double GBP = double.Parse(Console.ReadLine().Replace("\n", ""));
				Console.Write("Enter Value for the Open Loop Gain: ");
				double AOL = double.Parse(Console.ReadLine().Replace("\n", ""));

				filterEngine = new ActiveFilterEngine(ftype, Ra, Rb, Ca, Cb, GBP, AOL);
			}
			else
            {
				filterEngine = new ActiveFilterEngine(ftype, Ra, Rb, Ca, Cb);
			}

			string csvOutputString = "Frequency [Hz],Output Gain,Phase Shift[deg]\n";

			double fMax = 100000000; //100MHz
			double fOffsetIncrement = 1;

			double fDecadeFactor = 1;

			//Frequency Iterator
			for (double fDecade = 1; fDecade <= fMax; fDecade *= 10)
			{
				for (fDecadeFactor = 1; (fDecadeFactor <= 9) && ((fDecade * fDecadeFactor) <= fMax); fDecadeFactor += fOffsetIncrement)
				{
					double fActual = (fDecade * fDecadeFactor);
					csvOutputString += fActual.ToString() + "," + filterEngine.GetGain(fActual).ToString() + "," + ActiveFilterEngine.RadToDeg(filterEngine.GetPhase(fActual)).ToString() + "\n";	
				}
			}

			//Write the Graph to the csv File
			File.WriteAllText(filePath, csvOutputString);
		}

	    public static bool ParseBool(string inputStr)
        {
			return (inputStr == "1") ? true : false;
		}
	}
}