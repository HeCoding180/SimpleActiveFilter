using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleActiveFilter
{
    public enum FilterType
    {
        HP, //Highpass
        LP, //Lowpass
        BP  //Bandpass
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
                //Check for validity of the value set
                if ((value > 0) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _fu = value;
                else
                    UseFiniteOpenLoopGain = false;
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
                //Check for validity of the value set
                if ((value >= 1.0f) && double.IsFinite(value) && (!double.IsNaN(value)))
                    _A = value;
                else
                    UseFiniteOpenLoopGain = false;
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
        //Resistance, for the lowpass characteristic
        public double ResistorA
        {
            set
            {
                //Check for validity of the value set
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
        //Capacitance, for the lowpass characteristic
        public double CapacitorA
        {
            set
            {
                //Check for validity of the value set
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
        //Resistance, for the highpass characteristic
        public double ResistorB
        {
            set
            {
                //Check for validity of the value set
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
        //Capacitance, for the highpass characteristic
        public double CapacitorB
        {
            set
            {
                //Check for validity of the value set
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
        public ActiveFilterEngine()
        {
            //Set default values for all attributes
            UseFiniteOpenLoopGain = false;
            filterType = FilterType.BP;
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
            //Set default values for amplifier characteristic values
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
            UnityGainBandwidth = fu;
            OpenLoopGain = AOL;
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
        //Returns the gain bandwidth product approximation at a speciffic frequency
        public double GetGainBandwidthProduct(double frequency)
        {
            return 1 / (1 / OpenLoopGain + frequency / UnityGainBandwidth);                         
        }
        //Returns the gain approximation of the ideal filter
        public double GetIdealGain(double frequency)
        {
            return 1 + (GetZa(frequency) / GetZb(frequency));
        }
        //Returns the gain approximation depending on whether the generated bode plot should be ideal or real
        public double GetGain(double freqency)
        {
            if (!UseFiniteOpenLoopGain)
                //Return the gain approximation of the ideal filter
                return GetIdealGain(freqency);
            else
                //Return the gain approximation, including OpAmp characteristics, of the filter
                return 1 / Math.Sqrt(1 / Math.Pow(GetIdealGain(freqency), 2) + 1 / Math.Pow(GetGainBandwidthProduct(freqency), 2));
        }
        //Returns the phase shift approximation
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
