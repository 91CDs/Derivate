using F = Derivate.Func;
using Derivate;

namespace DerivateTests;
public class FormatterTests
{
    [Theory]
    [ClassData(typeof(FormatTestData))]
    public void Format_FormatExpressions(IExpression input, string expected)
    {
        Assert.Equal(input.Format(), expected);
    }
}

public class FormatTestData : TheoryData<IExpression, string>
{
    public FormatTestData()
    {
        // Power Formatting
        Add(F.Pow(F.Num(2), F.Var("x")), "2^x");
        Add(F.Pow(F.Var("x"), F.Num(2)), "x^2");
        Add(F.Pow(
            F.Sin(F.Var("x")), 
            F.Sin(F.Var("x"))
        ), "sin(x)^sin(x)");
        Add(F.Pow(
            F.Add(F.Var("x"), F.Num(1)),
            F.Add(F.Var("x"), F.Num(1))
        ), "(x + 1)^(x + 1)");
        Add(F.Pow(
            F.Pow(F.Var("x"), F.Num(2)),
            F.Pow(F.Var("x"), F.Num(2))
        ), "(x^2)^(x^2)");
        Add(F.Pow(
            F.Mul(F.Num(2), F.Var("x")),
            F.Mul(F.Num(2), F.Var("x"))
        ), "(2x)^(2x)");
        Add(F.Pow(
            F.Frac(1, 2),
            F.Frac(3, 4)
        ), "(1/2)^(3/4)");
        Add(F.Pow(
            F.E,
            F.Mul(F.Pi, F.I)
        ), "e^(pi*i)");

        // Multiplication & Division Formatting
        Add(F.Mul(F.Num(-2), F.Var("x"), F.Var("x"), F.Num(2)), "-2x*x*2");
        Add(F.Mul(
            F.Num(2), F.Var("x"), F.Var("y"), F.Pow(F.Var("z"), F.Num(2))
        ), "2xyz^2");
        Add(F.Mul(F.Num(3), F.Var("x"), F.Num(-3)), "3x*-3");
        Add(F.Mul(
            F.Num(-1),
            F.Sin(F.Var("x")), 
            F.Add(F.Var("x"), F.Num(1)),
            F.Pow(F.Var("x"), F.Num(2)),
            F.Var("x"),
            F.Mul(F.Num(2), F.Var("x")),
            F.Frac(1,2),
            F.Pow(F.Var("x"), F.Num(2))
        ), "-sin(x)(x + 1)(x^2)x(2x)(1/2)x^2");
        Add(F.Mul(
            F.Add(
                F.Mul(F.Num(3), F.Pow(F.Var("x"), F.Num(2))),
                F.Mul(F.Num(2), F.Var("x")),
                F.Num(1)
            ),
            F.Pow(
                F.Add(
                    F.Mul(F.Num(2), F.Var("x")),
                    F.Num(-7)
                ),
                F.Num(-1)
            )
        ), "(3x^2 + 2x + 1) / (2x - 7)");
        Add(F.Mul(
            F.Pow(
                F.Add(
                    F.Mul(F.Num(3), F.Pow(F.Var("x"), F.Num(2))),
                    F.Mul(F.Num(2), F.Var("x")),
                    F.Num(1)
                ),
                F.Num(-4)
            ),
            F.Pow(
                F.Add(
                    F.Mul(F.Num(2), F.Var("x")),
                    F.Num(1)
                ),
                F.Num(-2)
            )
        ), "1 / ((3x^2 + 2x + 1)^4)(2x + 1)^2");
        Add(F.Mul(
            F.Mul(F.Var("a"), F.Pow(F.Var("b"), F.Num(-1))),
            F.Pow(F.Mul(F.Var("y"), F.Pow(F.Var("z"), F.Num(-1))), F.Num(-1))
        ), "(a / b) / (y / z)");

        // Addition & Subtraction Formatting
        Add(F.Add(
            F.Mul(F.Num(-2), F.Pow(F.Var("x"), F.Num(2))),
            F.Mul(F.Num(3), F.Var("x")),
            F.Add(F.Var("x"), F.Num(-1)),
            F.Pow(F.Add(F.Var("x"), F.Num(1)), F.Num(2)),
            F.Frac(1,2),
            F.Sin(F.Var("x"))
        ), "- 2x^2 + 3x + (x - 1) + (x + 1)^2 + (1/2) + sin(x)");
        Add(F.Add(
            F.Mul(F.Num(3), F.Pow(F.Var("x"), F.Num(-2))),
            F.Mul(F.Num(-2), F.Pow(F.Var("x"), F.Num(-1))),
            F.Num(2)
        ), "(3 / x^2) - (2 / x) + 2");
    }
}