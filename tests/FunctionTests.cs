using F = Derivate.Func;
using Derivate;

namespace DerivateTests;
public class FunctionTests
{
    [Theory]
    [ClassData(typeof(ConstTestData))]
    public void Const_getConstant(string input, IExpression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var constant = expr.Const();

        Assert.Equal(constant.ConvertToString(), expected.ConvertToString());
    }

    [Theory]
    [ClassData(typeof(TermTestData))]
    public void Term_getTerm(string input, IExpression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var term = expr.Term();

        Assert.Equal(term.ConvertToString(), expected.ConvertToString());
    }

    [Theory]
    [ClassData(typeof(CompareTestData))]
    public void Compare_CompareTwoExpressions(string first, string other)
    {
        var exprFirst = new Parser(Lexer.ParseText(first)).Parse();
        var exprOther = new Parser(Lexer.ParseText(other)).Parse();
        Assert.True(exprFirst.CompareTo(exprOther));
    }

    [Theory]
    [ClassData(typeof(ExpandTestData))]
    public void Expand_ExpandExpressions(string input, IExpression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var expanded = expr.Expand();

        Assert.Equal(expanded.ConvertToString(), expected.Simplify().ConvertToString());
    }
}
public class ConstTestData : TheoryData<string, IExpression>
{
    public ConstTestData()
    {
        Add("3", F.Undefined);
        Add("x^2", F.Num(1));
        Add("3x", F.Num(3));
        Add("4xsin(x)", F.Num(4));
    }
}
public class TermTestData : TheoryData<string, IExpression>
{
    public TermTestData()
    {
        Add("3", F.Undefined);
        Add("x^2", F.Mul(F.Pow(F.Var("x"), F.Num(2))));
        Add("3x", F.Mul(F.Var("x")));
        Add("4xsin(x)", F.Mul(F.Sin(F.Var("x")), F.Var("x")));
    }
}
public class ExpandTestData : TheoryData<string, IExpression>
{
    public ExpandTestData()
    {
        Add("(2x + 5)(x + 2)", 
            F.Add(
                F.Num(10),
                F.Mul(F.Num(9), F.Var("x")),
                F.Mul(F.Num(2), F.Pow(F.Var("x"), F.Num(2)))
            ));
        Add("(x + y)^3",
            F.Add(
                F.Pow(F.Var("x"), F.Num(3)),
                F.Pow(F.Var("y"), F.Num(3)),
                F.Mul(F.Num(3), F.Pow(F.Var("x"), F.Num(2)), F.Var("y")),
                F.Mul(F.Num(3), F.Pow(F.Var("y"), F.Num(2)), F.Var("x"))
            ));
        Add("(x + 2)^2 + (y + 2)^2",
            F.Add(
                F.Num(8),
                F.Mul(F.Num(4), F.Var("x")),
                F.Mul(F.Num(4), F.Var("y")),
                F.Pow(F.Var("x"), F.Num(2)),
                F.Pow(F.Var("y"), F.Num(2))
            ));
        Add("(x + y + z)^3",
            F.Add(
                F.Pow(F.Var("x"), F.Num(3)),
                F.Pow(F.Var("y"), F.Num(3)),
                F.Pow(F.Var("z"), F.Num(3)),
                F.Mul(F.Num(3), F.Pow(F.Var("x"), F.Num(2)), F.Var("y")),
                F.Mul(F.Num(3), F.Pow(F.Var("x"), F.Num(2)), F.Var("z")),
                F.Mul(F.Num(3), F.Pow(F.Var("y"), F.Num(2)), F.Var("x")),
                F.Mul(F.Num(3), F.Pow(F.Var("y"), F.Num(2)), F.Var("z")),
                F.Mul(F.Num(3), F.Pow(F.Var("z"), F.Num(2)), F.Var("x")),
                F.Mul(F.Num(3), F.Pow(F.Var("z"), F.Num(2)), F.Var("y")),
                F.Mul(F.Num(6), F.Var("x"), F.Var("y"), F.Var("z"))
            ));
        Add("(x + 2)(x + 3)(x + 4)",
            F.Add(
                F.Num(24),
                F.Mul(F.Num(26), F.Var("x")),
                F.Mul(F.Num(9), F.Pow(F.Var("x"), F.Num(2))),
                F.Pow(F.Var("x"), F.Num(3))
            ));
        Add("(2x(x + 1)^2 + 2)^2",
            F.Add(
                F.Num(4),
                F.Mul(F.Num(8), F.Var("x")),
                F.Mul(F.Num(20), F.Pow(F.Var("x"), F.Num(2))),
                F.Mul(F.Num(24), F.Pow(F.Var("x"), F.Num(3))),
                F.Mul(F.Num(24), F.Pow(F.Var("x"), F.Num(4))),
                F.Mul(F.Num(16), F.Pow(F.Var("x"), F.Num(5))),
                F.Mul(F.Num(4), F.Pow(F.Var("x"), F.Num(6)))
            ));
        Add("(x + 2)(x + 2)^2",
            F.Add(
                F.Num(8),
                F.Mul(F.Num(12), F.Var("x")),
                F.Mul(F.Num(6), F.Pow(F.Var("x"), F.Num(2))),
                F.Pow(F.Var("x"), F.Num(3))
            ));
    }
}
public class CompareTestData : TheoryData<string, string>
{
    public CompareTestData()
    {
        Add("2", "11/2");
        Add("1/2", "10");
        Add("2", "x");

        Add("x","y");
        Add("e","pi");
        Add("a","cos(x)");
        Add("a","2 + x");
        Add("a","2x");
        Add("x","x^2");

        Add("x + y","x + z");
        Add("y + z","x + y + z");
        Add("1 + x", "y");

        Add("2xy","3xz");
        Add("zx","3zx");
        Add("ax^2", "x^3");
        
        Add("x^2","x^3");
        Add("x^2","y^2");
        Add("(1 + x)^3", "(1 + y)^2");

        Add("cos(x)", "sin(x)");
        Add("cos(2)", "cos(y)");
        Add("cos(4x)", "z");
    }
}