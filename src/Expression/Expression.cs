using System.Diagnostics;

namespace Derivate;

/// <summary>Represents general mathematical expressions</summary>
public interface IExpression {
    /// <summary> Compares two expression to determine whether the first 
    /// expression precedes or follows the second expression </summary>
    /// <param name="gx">The first expression</param>
    /// <returns> A boolean that returns true if the expression precedes
    /// <paramref name="gx"/> and false if not</returns>
    bool CompareTo(IExpression gx);
}

// Constants
/// <summary>Represents numerical expressions</summary>
[DebuggerDisplay("{this.ConvertToString()}")]
public abstract record Constant: IExpression
{
    public abstract double GetValue();
    public bool CompareTo(IExpression gx)
    {
        return gx switch
        {
            Constant n => GetValue() < n.GetValue(),
            _          => true,
        };
    }
}
// TODO: Support arbitrary length integers (BigInteger)
public sealed record Number(int value): Constant
{
    public override double GetValue() => value;

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
    public static IExpression Pow(Number a, Number b)
    {
        return (a.value, b.value) switch
        {
            (0, 0)   => Func.Undefined,
            (_, >=0) => Func.Num((int)Math.Pow(a.value, b.value)),
            (_, < 0) 
                => Evaluator.Simplify(
                    Func.Frac(1, (int)Math.Pow(a.value, -b.value))),
        };
    }
}
// TODO: Support arbitrary length fraction (BigInteger)
public sealed record Fraction(int numerator, int denominator): Constant
{
    public override double GetValue() => numerator / denominator;

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
[DebuggerDisplay("{this.ConvertToString()}")]
public record Symbols(string identifier): IExpression
{
    public const string Pi = "pi";
    public const string E = "e";
    public const string I = "i";
    public const string Undefined = "Undefined";
    public bool CompareTo(IExpression gx)
    {
        return gx switch
        {
            Symbols n  => identifier.CompareTo(n.identifier) < 0,
            Function n => identifier.CompareTo(n.name) < 0,
            _ => !gx.CompareTo(this)
        };
    }
}
public sealed record Variable(string identifier): Symbols(identifier) {}
public sealed record Undefined(): Symbols(Undefined) {}
public sealed record Pi(): Symbols(Pi) {}
public sealed record E(): Symbols(E) {}
public sealed record I(): Symbols(I) {}

// Operators
[DebuggerDisplay("{this.ConvertToString()}")]
public sealed record Sum(List<IExpression> value) : IExpression
{
    public bool Equals(Sum? other)
    {
        if (other is null) 
            return false;
        
        return value.SequenceEqual(other.value);
    }
    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
    public bool CompareTo(IExpression gx)
    {
        return gx switch
        {
            Sum n 
                => Func.CompareList(value, n.value),
            var n when n is Symbols or Function
                => Func.CompareList(value, [n]),
            _ => !gx.CompareTo(this)
        };
    }
}
[DebuggerDisplay("{this.ConvertToString()}")]
public sealed record Product(List<IExpression> value) : IExpression
{
    public bool Equals(Product? other)
    {
        if (other is null) 
            return false;
        
        return value.SequenceEqual(other.value);
    }
    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
    public bool CompareTo(IExpression gx)
    {
        return gx switch
        {
            Product n 
                => Func.CompareList(value, n.value),
            var n when n is Power or Sum or Symbols or Function
                => Func.CompareList(value, [n]),
            _ => !gx.CompareTo(this)
        };
    }
}
[DebuggerDisplay("{this.ConvertToString()}")]
public sealed record Power(IExpression Base, IExpression Exponent) : IExpression
{
    public bool CompareTo(IExpression gx)
    {
        return gx switch
        {
            Power n when !Base.Equals(n.Base) 
                => Base.CompareTo(n.Base),
            Power n when Base.Equals(n.Base) 
                => Exponent.CompareTo(n.Exponent),
            var n when n is Sum or Symbols or Function
                => CompareTo(Func.Pow(n, Func.Num(1))),
            _ => !gx.CompareTo(this)
        };
    }
}

// Functions
/// <summary>Represents relations of a 
/// set of inputs to a unique output</summary>
[DebuggerDisplay("{this.ConvertToString()}")]
public record Function(string name, IExpression value) : IExpression
{
    public bool CompareTo(IExpression gx)
    {
        return gx switch
        {
            Function n 
            when name.CompareTo(n.name) < 0 
                => true,
            Function n 
            when name.CompareTo(n.name) == 0 
                => value.CompareTo(n.value),
            Symbols n 
                => name.CompareTo(n.identifier) < 0,
            _ => !gx.CompareTo(this)
        };
    }
}
public sealed record Sine(IExpression value): Function("sin", value) {}
public sealed record Cosine(IExpression value): Function("cos", value) {}
public sealed record Tangent(IExpression value): Function("tan", value) {}
public sealed record Cosecant(IExpression value): Function("csc", value) {}
public sealed record Secant(IExpression value): Function("sec", value) {}
public sealed record Cotangent(IExpression value): Function("cot", value) {}
public sealed record Log(IExpression value, int Base): Function("log", value) {}
public sealed record NaturalLog(IExpression value): Function("ln", value) {}

public static partial class Func
{
    public static readonly Undefined Undefined = new();
    public static readonly E E = new();
    public static readonly Pi Pi = new();
    public static readonly I I = new();
    public static Symbols Var(string identifier) => identifier switch
    {
        Symbols.Pi => Pi,
        Symbols.E  => E,
        Symbols.I  => I,
        _          => new Variable(identifier),
    };
    public static Number Num(int value) => new(value);
    public static Fraction Frac(int numerator, int denominator) => new(numerator, denominator);
    public static Sum Add(params IExpression[] value) => new(value.ToList());
    public static Sum Add(IEnumerable<IExpression> value) => new(value.ToList());
    public static Product Sub(IExpression value) => Mul(Num(-1), value);
    public static Product Mul(params IExpression[] value) => new(value.ToList());
    public static Product Mul(IEnumerable<IExpression> value) => new(value.ToList());
    public static Power Div(IExpression value) => Pow(value, Num(-1));
    public static Power Pow(IExpression Base, IExpression Exponent) => new(Base, Exponent);
    public static Power Root(IExpression Radicand, int degree) => new(Radicand, Frac(1, degree));
    public static Sine Sin(IExpression value) => new(value);
    public static Cosine Cos(IExpression value) => new(value);
    public static Tangent Tan(IExpression value) => new(value);
    public static Secant Sec(IExpression value) => new(value);
    public static Cosecant Csc(IExpression value) => new(value);
    public static Cotangent Cot(IExpression value) => new(value);
    public static Log Log(IExpression value) => new(value, 10);
    public static NaturalLog Ln(IExpression value) => new(value);
}