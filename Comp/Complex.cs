using System;

namespace Complex
{
    public struct complex
    {
        public double mod { get; set; }
        public double arg { get; set; }

        public complex(double mod, double arg, int mode) { this.mod = mod; this.arg = arg; }
        public complex(double re, double im)
        {
            mod = Math.Sqrt(re * re + im * im);
            arg = 0;

            //i r => a, -i r => -a, -i -r => 180 + a, i -r => 180 - a
            if (re < 0) { im *= -1; arg = Math.PI; }
            arg = Math.Atan(im / re);
        }

        public double re { get => mod * Math.Cos(arg); }
        public double im { get => mod * Math.Sin(arg); }
        public complex conj { get => new complex(mod, -arg, 0); }
        public complex i(double a) => new complex(0, a);
        public complex i(complex a) => a * i(1);
        
        public static complex operator +(complex a, complex b) => new complex(a.re + b.re, a.im + b.im);
        public static complex operator -(complex a, complex b) => a + new complex(-b.re, -b.im);
        public static complex operator *(complex a, complex b) => new complex(b.mod * a.mod, b.arg + a.arg);
        public static complex operator /(complex a, complex b) => a * new complex(1 / b.mod, -b.arg, 1);

        #region arithmetic with real numbers
        public static complex operator +(complex a, double b) => new complex(a.re + b, a.im);
        public static complex operator -(complex a, double b) => a + -b;
        public static complex operator *(complex a, double b) => new complex(b * a.mod, a.arg);
        public static complex operator /(complex a, double b) => a * 1 / b;
        public static complex operator +(double a, complex b) => new complex(b.re + a, b.im);
        public static complex operator -(double a, complex b) => b + -a;
        public static complex operator *(double a, complex b) => new complex(a * b.mod, b.arg);
        public static complex operator /(double a, complex b) => b * 1 / a;
        #endregion

        public static complex operator ^(complex a, complex b) => ((a.mod^b) / Math.Pow(Math.E, b.im * a.arg)) * new complex(1, a.arg * b.re, 1);
        public static complex operator ^(complex a, double b) => new complex(Math.Pow(a.mod, b), a.arg * b);
        public static complex operator ^(double a, complex b) => new complex(Math.Pow(Math.E, b.re * Math.Log(a)), b.im * Math.Log(a), 1);

        public static complex ln(complex z) => new complex(Math.Log(z.mod), z.arg);
        public static complex log(complex a, complex b) => ln(a) / ln(b);
        public static complex log(double a, complex b) => Math.Log(a) / ln(b);
        public static complex log(complex a, double b) => ln(a) / Math.Log(b);
    }
}