using F = Derivate.Func;
using Derivate;

namespace DerivateTests;

public class SimplifyTests
{
    [Theory]
    [ClassData(typeof(SimplifyTestData))]
    public void eval_PowerRule(string input, Expression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast.ToFunction());

        Assert.Equal(expr.ConvertToString(), expected.ConvertToString());
    }
}

public class SimplifyTestData : TheoryData<string, Expression>
{
    public SimplifyTestData()
    {
        Add("(x^2)*(x^4)", // Product Rule
            Term(1, 6));
        Add("5x^4 / x^2",  // Quotient Rule
            Term(5, 2));
        Add("(5x)^2",      // Power of Product
            Term(25, 2));
        Add("(x^2)^5",     // Power of Power
            Term(1, 10)); 
        Add("2x^5 + 3x^5", // Adding Like Terms
            Term(5, 5));
        Add("2x^5 - 3x^5", // Subtracting Like Terms
            Term(-1, 5));
        Add("1 + 3sin(x) - 2x^2 + 5sin(x) + 7x^2 - 2", // Adding Like Terms 2
            F.Add(Term(-1, 0), Term(8, 1, F.Sin(F.Var("x"))), Term(5, 2)));
        Add("2x^2(5)(sin(x))x^3(sin(x))", // Product Rule 2
            F.Mul(Term(10, 0), Term(1, 2, F.Sin(F.Var("x"))), Term(1, 5)));

        // Identity Properties
        Add("0^0", F.Undefined);
        Add("0^x", F.Undefined);
        Add("0^-1", F.Undefined);
        Add("x / 0", F.Undefined);
        Add("x^0", F.Num(1));
        Add("0^(1/2)", F.Num(0));
        Add("x * 0", F.Num(0));
        Add("0 / x", F.Num(0));
        Add("x^1", F.Var("x"));
        Add("x + 0", F.Var("x"));
        Add("x - 0", F.Var("x"));
        Add("x * 1", F.Var("x"));
        Add("x / 1", F.Var("x"));

        // Numerical Transformations
        Add("5 + 20", F.Num(25));
        Add("20 - 5", F.Num(15));
        Add("20 / 5", F.Num(4));
        Add("20 * 5", F.Num(100));
        Add("2 ^ 5" , F.Num(32));

        // Unary Transformations
        Add("-x", Term(-1, 1));

    }

    private static Expression Term(int lc, int exp, Expression? b = null)
    {
        b ??= F.Var("x");
        return (lc, exp) switch
        {
            (0, _) => F.Num(0),
            (_, 0) => F.Num(lc),
            (1, 1) => b,
            (1, _) => F.Pow(b, F.Num(exp)),
            (_, 1) => F.Mul(F.Num(lc), b),
            (_, _) => F.Mul(F.Num(lc), F.Pow(b, F.Num(exp))),
        };
    }
}