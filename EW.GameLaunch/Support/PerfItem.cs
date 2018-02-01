using System;
using System.Collections.Generic;
using EW.Framework;
using System.Drawing;
namespace EW.Support
{
    public class PerfItem
    {

        public readonly Color C;
        public readonly string Name;

        public double Val = 0.0;
        double[] samples = new double[100];

        public bool HasNormalTick = true;

        int head = 1, tail = 0;

        public PerfItem(string name,Color c)
        {
            Name = name;
            C = c;
        }

        public void Tick()
        {
            samples[head++] = Val;
            if (head == samples.Length)
                head = 0;
            if (head == tail && ++tail == samples.Length)
                tail = 0;
            Val = 0.0;
        }


        public double Average(int count)
        {
            var i = 0;
            var n = head;
            double sum = 0;
            while(i<count && n != tail)
            {
                if (--n < 0)
                    n = samples.Length - 1;
                sum += samples[n];
                i++;
            }
            return i == 0 ? sum : sum / i;
        }

        public IEnumerable<double> Samples()
        {
            var n = head;
            while (n != tail)
            {
                --n;
                if (n < 0)
                    n = samples.Length - 1;
                yield return samples[n];
            }
        }

        public double LastValue
        {
            get
            {
                var n = head;
                if (--n < 0)
                    n = samples.Length - 1;
                return samples[n];
            }
        }
    }
}