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

        //Attributes for the OpAmp Characteristics
        public bool UseFiniteOpenLoopGain { set; get; }
        private double _fu { set; get; }
        private double _A { set; get; }
        public double UnityGainBandwidth
        {
            set
            {
                if ((value > 0) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _fu = value;
                else
                    throw new ArgumentOutOfRangeException(value.ToString() + "Hz is an invalid frequency");
            }
            get
            {
                return _fu;
            }
        }
        public double OpenLoopGain
        {
            set
            {
                if ((value >= 1.0f) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _A = value;
                else
                    throw new ArgumentOutOfRangeException(value.ToString() + "V/V is an invalid gain");
            }
            get
            {
                return _A;
            }
        }

        //Private Part Attributes
        private double _ra { set; get; }        //Resistance, for the lowpass characteristic
        private double _ca { set; get; }        //Capacitance, for the lowpass characteristic
        private double _rb { set; get; }        //Resistance, for the highpass characteristic
        private double _cb { set; get; }        //Capacitance, for the highpass characteristic

        //Public Part Attributes
        public double ResistorA                 //Resistance, for the lowpass characteristic
        {
            set
            {
                if ((value > 0) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _ra = value;
                else
                    throw new ArgumentOutOfRangeException(value.ToString() + "Ω is an invalid resistance");
            }
            get
            {
                return _ra;
            }
        }
        public double CapacitorA                //Capacitance, for the lowpass characteristic
        {
            set
            {
                if ((value > 0) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _ca = value;
                else
                    throw new ArgumentOutOfRangeException(value.ToString() + "F is an invalid capacitance");
            }
            get
            {
                return _ca;
            }
        }
        public double ResistorB                 //Resistance, for the highpass characteristic
        {
            set
            {
                if ((value > 0) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _rb = value;
                else
                    throw new ArgumentOutOfRangeException(value.ToString() + "Ω is an invalid resistance");
            }
            get
            {
                return _rb;
            }
        }
        public double CapacitorB                //Capacitance, for the highpass characteristic
        {
            set
            {
                if ((value > 0) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _cb = value;
                else
                    throw new ArgumentOutOfRangeException(value.ToString() + "F is an invalid capacitance");
            }
            get
            {
                return _cb;
            }
        }

        //Constructors
        public ActiveFilterEngine(FilterType type)
        {
            UseFiniteOpenLoopGain = false;
            filterType = type;
            ResistorA = 1.0f;
            ResistorB = 1.0f;
            CapacitorA = 1.0f;
            CapacitorB = 1.0f;
            UnityGainBandwidth = 100000000;     //100MHz
            OpenLoopGain = 100000;              //100kV/V
        }
        public ActiveFilterEngine(FilterType type, double Ra, double Rb, double Ca, double Cb)
        {
            UseFiniteOpenLoopGain = false;
            filterType = type;
            ResistorA = Ra;
            ResistorB = Rb;
            CapacitorA = Ca;
            CapacitorB = Cb;
            UnityGainBandwidth = 100000000;     //100MHz
            OpenLoopGain = 100000;              //100kV/V
        }
        public ActiveFilterEngine(FilterType type, double Ra, double Rb, double Ca, double Cb, double fu, double AOL)
        {
            UseFiniteOpenLoopGain = true;
            filterType = type;
            ResistorA = Ra;
            ResistorB = Rb;
            CapacitorA = Ca;
            CapacitorB = Cb;
            if ((fu > 0) && double.IsFinite(fu) && (!double.IsNaN(fu)))
                UnityGainBandwidth = fu;
            else
                UnityGainBandwidth = 100000000; //100MHz
            if ((AOL > 0) && double.IsFinite(AOL) && (!double.IsNaN(AOL)))
                OpenLoopGain = AOL;
            else
                OpenLoopGain = 100000;          //100kV/V
        }
        
        //Private Calculation Methods
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
        
        //Public Calculation Methods
        public double GetGainBandwidthProduct(double frequency)
        {
            return 1 / (1 / OpenLoopGain + frequency / UnityGainBandwidth);                         //Returns the gain bandwidth product at a speciffic frequency
        }

        public double GetIdealGain(double frequency)
        {
            return 1 + (GetZa(frequency) / GetZb(frequency));
        }

        public double GetGain(double freqency)
        {
            if (!UseFiniteOpenLoopGain)
                return GetIdealGain(freqency);                                                      //Return the ideal gain of the filter
            else
                return 1 / Math.Sqrt(1 / Math.Pow(GetIdealGain(freqency), 2) + 1 / Math.Pow(GetGainBandwidthProduct(freqency), 2));    //Return the gain, including OpAmp characteristics, of the filter
        }

        public double GetPhase(double frequency)
        {
            double fLog = Math.Log10(frequency);                                                    //Logarythmic representation of the frequency
            double fPre = Math.Pow(10, fLog - 0.0001);
            double fPost = Math.Pow(10, fLog + 0.0001);

            return 0.5 * Math.PI * (Math.Log10(GetGain(fPost)) - Math.Log10(GetGain(fPre))) / 0.0002;
        }

        //Public Utility Methods
        public static double PythagoricTheorem(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
        public static double RadToDeg(double rad)
        {
            return rad / Math.PI * 180;
        }
        public static double GetDecibels(double value)
        {
            return 10 * Math.Log10(value);
        }
    }
}
