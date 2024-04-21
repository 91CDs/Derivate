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
        var expr = Evaluator.Simplify(ast.ToFunction());
        var constant = expr.Const();

        Assert.Equal(constant.ConvertToString(), expected.ConvertToString());
    }

    [Theory]
    [ClassData(typeof(TermTestData))]
    public void Term_getTerm(string input, IExpression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast.ToFunction());
        var term = expr.Term();

        Assert.Equal(term.ConvertToString(), expected.ConvertToString());
    }

    [Theory]
    [ClassData(typeof(CompareTestData))]
    public void Term_CompareTwoExpressions(string first, string other)
    {
        var exprFirst = new Parser(Lexer.ParseText(first))
            .Parse()
            .ToFunction();
        var exprOther = new Parser(Lexer.ParseText(other))
            .Parse()
            .ToFunction();
        Assert.True(exprFirst.CompareTo(exprOther));
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