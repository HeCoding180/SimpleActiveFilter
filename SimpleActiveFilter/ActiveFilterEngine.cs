using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleActiveFilter
{
    public enum FilterType
    {
        HP, //High-Pass
        LP, //Low-Pass
        BP  //Band-Pass
    }

    public struct ComplexImpedance
    {
        public double Impedance;
        public double Phi;
    }

    public class ActiveFilterEngine
    {
        public FilterType filterType { set; get; }

        //Attributes for 
        public bool UseFiniteOpenLoopGain { set; get; }
        public double UnityGainFrequency { set; get; }
        public double OpenLoopGain { set; get; }

        //Part Attributes
        public double ResistorA { set; get; }   //Resistance, for the lowpass characteristic
        public double CapacitorA { set; get; }  //Capacitance, for the lowpass characteristic
        public double ResistorB { set; get; }   //Resistance, for the highpass characteristic
        public double CapacitorB { set; get; }  //Capacitance, for the highpass characteristic

        //Constructors
        public ActiveFilterEngine(FilterType type)
        {
            UseFiniteOpenLoopGain = false;
            filterType = type;
            ResistorA = 0.0f;
            ResistorB = 0.0f;
            CapacitorA = 0.0f;
            CapacitorB = 0.0f;
        }
        public ActiveFilterEngine(FilterType type, double Ra, double Rb, double Ca, double Cb)
        {
            UseFiniteOpenLoopGain = false;
            filterType = type;
            ResistorA = Ra;
            ResistorB = Rb;
            CapacitorA = Ca;
            CapacitorB = Cb;
        }
        public ActiveFilterEngine(FilterType type, double Ra, double Rb, double Ca, double Cb, double fUnityGain, double AOL)
        {
            UseFiniteOpenLoopGain = true;
            filterType = type;
            ResistorA = Ra;
            ResistorB = Rb;
            CapacitorA = Ca;
            CapacitorB = Cb;
            UnityGainFrequency = fUnityGain;
            OpenLoopGain = AOL;
        }

        //Calculation Methods (private)
        private double GetZa(double frequency)
        {
            double result;
            switch (filterType)
            {
                case FilterType.HP:
                    result = ResistorA;
                    break;
                default:
                    double Ga = 1 / ResistorA;
                    double Ba = 2 * Math.PI * frequency * CapacitorA;
                    result = 1 / PythagoricTheorem(Ga, Ba);
                    break;
            }
            return result;
        }
        
        private double GetZb(double frequency)
        {
            double result;
            switch (filterType)
            {
                case FilterType.LP:
                    result = ResistorB;
                    break;
                default:
                    double Xcb = 1 / (2 * Math.PI * frequency * CapacitorB);
                    result = PythagoricTheorem(ResistorB, Xcb);
                    break;
            }
            return result;
        }

        private double GetGainBandwidthProduct(double frequency)
        {
            return 1 / (1 / OpenLoopGain + frequency / UnityGainFrequency);
        }

        private double GetBareGain(double frequency)
        {
            return 1 + (GetZa(frequency) / GetZb(frequency));
        }

        //Calculation Methods (public)
        public double GetGain(double freqency)
        {
            if (!UseFiniteOpenLoopGain)  return GetBareGain(freqency);                                                   //Return the bare gain of the Filter
            else                         return 1 / (1 / GetBareGain(freqency) + 1 / GetGainBandwidthProduct(freqency)); //Return the gain including OpAmp characteristics
        }

        public double GetPhase(double frequency)
        {
            return GetLogarythmicDerivative(GetGain, frequency);
        }

        //Private Utility Methods
        private double GetLogarythmicDerivative(Func<double, double> referenceFunction, double frequency)
        {
            double fLog = Math.Log10(frequency);                //Logarythmic representation of the derivative frequency
            double fPre = Math.Pow(10, fLog - fLog / 1000);     //Logarythmically 99.9% of the derivative frequency
            double fPost = Math.Pow(10, fLog + fLog / 1000);    //Logarythmically 100.1% of the derivative frequency
            double fDelta = fPost - fPre;

            return (referenceFunction(fPost) - referenceFunction(fPre)) / fDelta;
        }

        public static double PythagoricTheorem(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
        
        //Public Utility Methods
        public static double RadToDeg(double rad)
        {
            return rad / Math.PI * 180;
        }
    }
}
