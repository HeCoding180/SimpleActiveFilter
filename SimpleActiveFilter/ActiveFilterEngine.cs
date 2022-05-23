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

        public double ResistorA { set; get; }   //Resistance, for the lowpass characteristic
        public double CapacitorA { set; get; }  //Capacitance, for the lowpass characteristic
        public double ResistorB { set; get; }   //Resistance, for the highpass characteristic
        public double CapacitorB { set; get; }  //Capacitance, for the highpass characteristic

        public ActiveFilterEngine(FilterType type)
        {
            filterType = type;
            ResistorA = 0.0f;
            ResistorB = 0.0f;
            CapacitorA = 0.0f;
            CapacitorB = 0.0f;
        }
        public ActiveFilterEngine(FilterType type, double Ra, double Rb, double Ca, double Cb)
        {
            filterType = type;
            ResistorA = Ra;
            ResistorB = Rb;
            CapacitorA = Ca;
            CapacitorB = Cb;
        }
        public ActiveFilterEngine(double Ra, double Rb, double Ca, double Cb)
        {
            filterType = FilterType.BP;
            ResistorA = Ra;
            ResistorB = Rb;
            CapacitorA = Ca;
            CapacitorB = Cb;
        }

        private ComplexImpedance getZa(double frequency)
        {
            ComplexImpedance result = new ComplexImpedance();
            switch (filterType)
            {
                case FilterType.HP:
                    result.Impedance = ResistorA;
                    result.Phi = 0;
                    break;
                default:
                    break;
            }
            return result;
        }
        
        private ComplexImpedance getZa(double frequency)
        {
            ComplexImpedance result = new ComplexImpedance();
            switch (filterType)
            {
                case FilterType.LP:
                    result.Impedance = ResistorB;
                    result.Phi = 0;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
