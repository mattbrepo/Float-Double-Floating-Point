using System;
using System.Collections.Generic;
using System.Text;

namespace Float-Double-Floating-Point
{
    // https://en.wikipedia.org/wiki/Single-precision_floating-point_format
    //
    // 
    // IEEE Standard for Binary Floating-Point Arithmetic (ANSI/IEEE Std 754-1985)
    // 
    // 32 bit precision number
    //
    //  1     8               23               bit length
    // +-+--------+-----------------------+
    // |S|  Exp.  |  Fraction             |
    // +-+--------+-----------------------+
    // 31 30      22                      0    bit index
    //
    // Single-precision floating-point format is a computer number format that occupies 4 bytes (32 bits) in computer memory and represents 
    // a wide dynamic range of values by using a floating point.
    //
    //IEEE 754 single-precision binary floating-point format: binary32
    //
    //The IEEE 754 standard specifies a binary32 as having:
    //
    //    Sign bit: 1 bit
    //    Exponent width: 8 bits
    //    Significand precision: 24 (23 explicitly stored)
    //
    // This gives from 6 to 9 significant decimal digits precision (if a decimal string with at most 6 significant decimal is converted to 
    // IEEE 754 single precision and then converted back to the same number of significant decimal, then the final string should match the original; 
    // and if an IEEE 754 single precision is converted to a decimal string with at least 9 significant decimal and then converted back to single, 
    // then the final number must match the original [3]).
    //
    // Sign bit determines the sign of the number, which is the sign of the significand as well. Exponent is either an 8 bit signed integer 
    // from −128 to 127 (2's Complement) or an 8 bit unsigned integer from 0 to 255 which is the accepted biased form in IEEE 754 binary32 definition. 
    // For this case an exponent value of 127 represents the actual zero.
    //
    // The true significand includes 23 fraction bits to the right of the binary point and an implicit leading bit (to the left of the binary point) 
    // with value 1 unless the exponent is stored with all zeros. Thus only 23 fraction bits of the significand appear in the memory format but the 
    // total precision is 24 bits (equivalent to log10(224) ≈ 7.225 decimal digits). 
    // 
    // The single-precision binary floating-point exponent is encoded using an offset-binary representation, with the zero offset being 127; 
    // also known as exponent bias in the IEEE 754 standard.
    //
    //    Emin = 01H−7FH = −126
    //    Emax = FEH−7FH = 127
    //    Exponent bias = 7FH = 127
    //
    //
    // Exponent  |   Significand zero    |    Significand non-zero    |                Equation
    //-------------------------------------------------------------------------------------------------------------------
    //   00H     |        zero           |      subnormal numbers     |    (−1)^signbits * 2^−126 * 0.significandbits
    // 01H..FEH  |                 normalized value                   |    (−1)^signbits * 2^(exponentbits−127) * 1.significandbits
    //   FFH      |      ±infinity        |           NaN              |
    //
    // The minimum positive (subnormal) value is 2−149 ≈ 1.4 × 10−45. The minimum positive normal value is 2−126 ≈ 1.18 × 10−38. 
    // The maximum representable value is (2−2−23) × 2127 ≈ 3.4 × 1038.
    class Program
    {
        static void Main(string[] args)
        {
            #region tests...
            //double d = 41943041;
            //float f = (float)d;
            //int i = (int)d;
            //ConvertAndPrint(f);

            //double d = 21.48;
            //byte[] b = BitConverter.GetBytes(d);
            //Console.WriteLine("Double: " + d + ", byte[]: " + BitConverter.ToString(b));

            //keyence TM-3000 TCP/IP measurement value
            byte[] b1 = new byte[] { 0x10, 0xC5, 0x20, 0 };
            int ires = BitConverter.ToInt32(b1, 0);
            double dres = (double)ires / 100000;
            
            #endregion

            ConvertAndPrint(1);
            ConvertAndPrint(-1);
            ConvertAndPrint(0);
            ConvertAndPrint(10);
            ConvertAndPrint(10.2F);
            ConvertAndPrint(-10322.23232F);
            ConvertAndPrint((float)5/3);
            ConvertAndPrint((float)Math.Pow(2, -149));
            ConvertAndPrint(float.MaxValue);
            ConvertAndPrint(float.MinValue);
            ConvertAndPrint(float.PositiveInfinity);
            ConvertAndPrint(float.NegativeInfinity);
            ConvertAndPrint(float.NaN);
            ConvertAndPrint(0.5F);
            Console.ReadKey();
        }

        static void ConvertAndPrint(float f)
        {
            const int BIAS = 127;
            
            float f2 = float.NaN;
            byte[] b = BitConverter.GetBytes(f);
            string descr = null;

            //  1     8               23              
            // +-+---------+-------------------------+
            // |S|  Esp    |  Mantissa               |
            // |1|1000010 1|1101101 01000000 00000000|
            // +-+---------+-------------------------+
            // 31 30      22                        0            
            //
            // |---------|---------|--------|--------|
            // |  b[3]   |  b[2]   |  b[1]  |  b[0]  |
            // |---------|---------|--------|--------|

            //-------------------------------------------------------------- calcolo segno
            int s = (b[3] & (1 << 7)) >> 7;

            //-------------------------------------------------------------- calcolo esponente
            int e = ((byte)(b[3] << 1)) + ((b[2] & (1 << 7)) >> 7);
            int E = e - BIAS; //esponenete effettivo utilizzato

            //-------------------------------------------------------------- calcolo mantissa
            float significand = 1;
            float significand_zero = 0;  
            int bit = 0;
            int k = 0;
            significand = 1;

            for (int i = 1; i <= 23; i++) 
            {
                if (i <= 7) 
                {
                    k = 7 - i;
                    bit = b[2] & (1 << k);
                }
                else if (i <= 15) 
                {
                    k = 15 - i;
                    bit = b[1] & (1 << k);
                }
                else 
                {
                    k = 23 - i;
                    bit = b[0] & (1 << k);
                }

                bit = bit >> k;
                significand += (float)(bit * Math.Pow(2, -i));
                significand_zero += (float)(bit * Math.Pow(2, -i));
            }

            //-------------------------------------------------------------- (ri)calcolo float
            if (e == 0)
            {
                if (significand_zero == 0) 
                {
                    descr = "Zero";
                    f2 = 0;
                }
                else 
                {
                    descr = "Subnormal number";
                    f2 = (float)(Math.Pow(-1, s) * Math.Pow(2, -126)) * significand_zero; 
                }
            }
            else if (e == 255)
            {
                if (significand_zero == 0) 
                {
                    if (s == 0) 
                    {
                        descr = "PositiveInfinity";
                        f2 = float.PositiveInfinity;
                    }
                    else 
                    {
                        descr = "NegativeInfinity";
                        f2 = float.NegativeInfinity;
                    }
                }
                else 
                {
                    descr = "NaN";
                    f2 = float.NaN;
                }
            }
            else
            {
                descr = "Normalized value";
                f2 = (float)(Math.Pow(-1, s) * Math.Pow(2, E)) * significand; 
            }

            //-------------------------------------------------------------- verifica float ricalcolato
            if (!f.Equals(f2)) return; //NB: usato Equals e non != perché non funziona con NaN

            Console.WriteLine("------------------------");
            Console.WriteLine((descr == null ? "" : descr + ": ") + f + ": " + BitConverter.ToString(b));
            Console.WriteLine("sign: " + s + ", exp: " + e + ", significand: " + significand);
        }
    }
}
