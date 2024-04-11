namespace Derivate;

/// <summary>Represents general mathematical expressions</summary>
public interface Expression {
    /// <summary> Compares two expression to determine whether the first 
    /// expression precedes or follows the second expression </summary>
    /// <param name="gx">The first expression</param>
    /// <returns> A boolean that returns true if the expression precedes
    /// <paramref name="gx"/> and false if not</returns>
    bool CompareTo(Expression gx);
}

// Constants
/// <summary>Represents numerical expressions</summary>
public interface Constant: Expression
{
    public int numerator { get; }
    public int denominator { get; }
    bool Expression.CompareTo(Expression gx)
    {
        return gx switch
        {
            Constant n => numerator / denominator < n.numerator / n.denominator,
            _          => true,
        };
    }
}
public readonly record struct Number(int value): Constant
{
    public int numerator { get => value; }
    public int denominator { get => 1; }
    public static Number GCD(Number a, Number b)
    {
        return Func.Num(MathInt.GCD(a.value, b.value)); 
    }

    public static implicit operator Fraction(Number a) => new(a.value, 1);

    public static Number operator +(Number a) => a;
    public static Number operator -(Number a) => new(-a.value);
    public static Number operator +(Number a, Number b) => new(a.value + b.value);
    public static Number operator -(Number a, Number b) => new(a.value - b.value);
    public static Number operator *(Number a, Number b) => new(a.value * b.value);
    public static Fraction operator /(Number a, Number b) 
    {
        if (b.value == 0) 
            throw new DivideByZeroException();
        return Func.Frac(a.value, b.value);
    }
    public static Expression Pow(Number a, Number b)
    {
        return (a.value, b.value) switch
        {
            (0, 0)   => Func.Undefined,
            (_, >=0) => Func.Num((int)Math.Pow(a.value, b.value)),
            (_, < 0) 
                => Evaluator.simplify(
                    Func.Frac(1, (int)Math.Pow(a.value, -b.value))),
        };
    }
}
public readonly record struct Fraction(int numerator, int denominator): Constant
{
    public bool CompareTo(Expression gx)
    {
        return gx switch
        {
            Fraction n => numerator / denominator < n.numerator / n.denominator,
            Number n   => numerator / denominator < n.value,
            _ => true,
        };
    }
    
    public static Fraction operator +(Fraction a) => a;
    public static Fraction operator -(Fraction a) 
        => new(-a.numerator, a.denominator);
    public static Fraction operator +(Fraction a, Fraction b)
        => new(a.numerator * b.denominator + b.numerator * a.denominator, 
            a.denominator * b.denominator);
    public static Fraction operator -(Fraction a, Fraction b) => a + (-b);
    public static Fraction operator *(Fraction a, Fraction b)
        => new(a.numerator * b.numerator, a.denominator * b.denominator);
    public static Fraction operator /(Fraction a, Fraction b) 
    {
        if (b.numerator == 0) 
            throw new DivideByZeroException();
        return new(a.numerator * b.denominator, a.denominator * b.numerator);
    }
    public static Fraction Pow(Fraction a, Number b)
    {
        return (a, b) switch
        {
            (_, Number(0)) 
                => Func.Num(1),
            (var l, Number(< 0) n) 
                => Func.Frac(
                    l.denominator, 
                    (int) Math.Pow(l.numerator, n.value)),
            (var l, Number(> 0) n) 
                => Func.Frac(
                    (int)Math.Pow(l.numerator, n.value), 
                    (int)Math.Pow(l.denominator, n.value)),
        };
    }
}

// Symbols
/// <summary>Represents an unknown mathematical object</summary>
public interface Symbols: Expression
{
    public string identifier { get; }
    bool Expression.CompareTo(Expression gx)
    {
        return gx switch
        {
            Symbols n  => identifier.CompareTo(n.identifier) < 0,
            Function n => identifier.CompareTo(n.name) < 0,
            _ => !gx.CompareTo(this)
        };
    }
}
public readonly record struct Variable(string identifier): Symbols 
{
    public const string Pi = "pi";
    public const string E = "e";
}
public readonly record struct Undefined: Symbols 
{
    public Undefined() {}
    public string identifier { get; } = "Undefined";
}
public readonly record struct Pi: Symbols 
{
    public Pi() {}
    public string identifier { get; } = Variable.Pi;
}
public readonly record struct E: Symbols 
{
    public E() {}
    public string identifier { get; } = Variable.E;
}

// Operators
public readonly record struct Sum(List<Expression> value) : Expression
{
    public bool Equals(Sum other)
    {
        return value.SequenceEqual(other.value);
    }
    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
    public bool CompareTo(Expression gx)
    {
        return gx switch
        {
            Sum n 
                => Func.CompareList(value, n.value),
            var n when n is Variable or Function
                => Func.CompareList(value, [n]),
            _ => !gx.CompareTo(this)
        };
    }
}
public readonly record struct Product(List<Expression> value) : Expression
{
    public bool Equals(Product other)
    {
        return value.SequenceEqual(other.value);
    }
    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
    public bool CompareTo(Expression gx)
    {
        return gx switch
        {
            Product n 
                => Func.CompareList(value, n.value),
            var n when n is Power or Sum or Variable or Function
                => Func.CompareList(value, [n]),
            _ => !gx.CompareTo(this)
        };
    }
}
public readonly record struct Power(Expression Base, Expression Exponent) : Expression
{
    public bool CompareTo(Expression gx)
    {
        return gx switch
        {
            Power n when Base.Equals(n.Base) 
                => Base.CompareTo(n.Base),
            Power n when !Base.Equals(n.Base) 
                => Exponent.CompareTo(n.Exponent),
            var n when n is Sum or Variable or Function
                => CompareTo(Func.Pow(n, Func.Num(1))),
            _ => !gx.CompareTo(this)
        };
    }
}

// Functions
/// <summary>Represents relations of a 
/// set of inputs to a unique output</summary>
public interface Function : Expression
{
    public Expression value { get; }
    public string name { get; }
    bool Expression.CompareTo(Expression gx)
    {
        return gx switch
        {
            Function n => name.CompareTo(n.name) < 0,
            Variable n => name.CompareTo(n.identifier) < 0,
            _ => !gx.CompareTo(this)
        };
    }
}
public readonly record struct Sine(Expression value, string name = "sin"): Function {}
public readonly record struct Cosine(Expression value, string name = "cos"): Function {}
public readonly record struct Tangent(Expression value, string name = "tan"): Function {}
public readonly record struct Cosecant(Expression value, string name = "csc"): Function {}
public readonly record struct Secant(Expression value, string name = "sec"): Function {}
public readonly record struct Cotangent(Expression value, string name = "cot"): Function {}
public readonly record struct Log(Expression value, int Base, string name = "log"): Function {}
public readonly record struct NaturalLog(Expression value, string name = "ln"): Function {}

public static class Func
{ 
    /// <summary>
    /// Returns the base of the given function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static Expression Base(this Expression function)
    {
        return function switch
        {
            Number or Fraction => Undefined,
            Power f => f.Base,
            _ => function
        };
    }

    /// <summary>
    /// Returns the exponent part of the given function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static Expression Exponent(this Expression function)
    {
        return function switch
        {
            Number or Fraction => Undefined,
            Power f => f.Exponent,
            _ => Num(1)
        };
    }

    /// <summary>
    /// Returns the term part of the function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static Expression Term(this Expression function)
    {
        return function switch
        {
            Number or Fraction
                => Undefined,
            Product u when u.value.First() is Number or Fraction
                => Mul(u.value.Skip(1)),
            Product u
                => u,
            _ => Mul(function)
        };
    }

    /// <summary>
    /// Returns the constant part of the function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static Expression Const(this Expression function)
    {
        return function switch
        {
            Number or Fraction
                => Undefined,
            Product u when u.value.First() is Number or Fraction
                => u.value.First(),
            Product
                => Num(1),
            _ => Num(1),
        };
    }
    
    public static bool CompareList(List<Expression> m, List<Expression> n)
    {
        int minCount = Math.Min(m.Count, n.Count);

        for (int i = 1; i <= minCount; i++)
        {
            int mi = m.Count - i, ni = n.Count - i;
            if (m[mi] != n[ni])
                return m[mi].CompareTo(n[ni]);
        }
        return m.Count < n.Count;
    }
    public static readonly Undefined Undefined = new();
    public static readonly E E = new();
    public static readonly Pi Pi = new();
    public static Symbols Var(string identifier) => identifier switch
    {
        Variable.Pi => Pi,
        Variable.E  => E,
        _           => new Variable(identifier),
    };
    public static Number Num(int value) => new(value);
    public static Fraction Frac(int numerator, int denominator) => new(numerator, denominator);
    public static Sum Add(params Expression[] value) => new(value.ToList());
    public static Sum Add(IEnumerable<Expression> value) => new(value.ToList());
    public static Product Sub(Expression value) => Mul(Num(-1), value);
    public static Product Mul(params Expression[] value) => new(value.ToList());
    public static Product Mul(IEnumerable<Expression> value) => new(value.ToList());
    public static Power Div(Expression value) => Pow(value, Num(-1));
    public static Power Pow(Expression Base, Expression Exponent) => new(Base, Exponent);
    public static Sine Sin(Expression value) => new(value);
    public static Cosine Cos(Expression value) => new(value);
    public static Tangent Tan(Expression value) => new(value);
    public static Secant Sec(Expression value) => new(value);
    public static Cosecant Csc(Expression value) => new(value);
    public static Cotangent Cot(Expression value) => new(value);
    public static Log Log(Expression value) => new(value, 10);
    public static NaturalLog Ln(Expression value) => new(value);
}