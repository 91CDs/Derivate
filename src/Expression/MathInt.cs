/// <summary>Class for basic integer math functions</summary>
public static class MathInt {
    /// <summary> Finds the greatest common divisor of two integers 
    /// <paramref name="a"/> and <paramref name="b"/> </summary>
    /// <param name="a">The first integer</param>
    /// <param name="b">The second integer</param>
    public static int GCD(int a, int b)
    {
        int r;
        while (b != 0)
        {
            r = a % b;
            a = b;
            b = r;
        }
        return Math.Abs(a); 
    }

    /// <summary>
    /// Finds the greatest common divisor of two integers 
    /// <paramref name="a"/> and <paramref name="b"/> 
    /// as well as the Bézout coefficients 
    /// <paramref name="m"/> and <paramref name="n"/>
    /// </summary>
    /// <param name="a">The first integer</param>
    /// <param name="b">The second integer</param>
    public static (int gcd, int m, int n) ExtendedGCD(int a, int b)
    {
        (int s1, int s2) = (1, 0);
        (int t1, int t2) = (0, 1);
        int q, r;
        while (b != 0)
        {
            (q, r) = Math.DivRem(a, b);
            (a, b) = (b, r);
            (s1, s2) = (s2, s1 - q * s2);
            (t1, t2) = (t2, t1 - q * t2);
        }

        if (a >= 0)
            return (a, s1, t1);
        else
            return (a, s1, t1);
    } 
}