using F = Derivate.Func;
using Derivate;

namespace DerivateTests;

public class FunctionTests
{
    [Theory]
    [ClassData(typeof(ConstTestData))]
    public void Const_getConstant(string input, Expression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.simplify(ast.ToFunction());
        var constant = expr.Const();

        Assert.Equal(constant.Format(), expected.Format());
    }

    [Theory]
    [ClassData(typeof(TermTestData))]
    public void Term_getTerm(string input, Expression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.simplify(ast.ToFunction());
        var term = expr.Term();

        Assert.Equal(term.Format(), expected.Format());
    }
}
public class ConstTestData : TheoryData<string, Expression>
{
    public ConstTestData()
    {
        Add("3", F.Undefined);
        Add("x^2", F.Num(1));
        Add("3x", F.Num(3));
        Add("4xsin(x)", F.Num(4));
    }
}
public class TermTestData : TheoryData<string, Expression>
{
    public TermTestData()
    {
        Add("3", F.Undefined);
        Add("x^2", F.Pow(F.Var("x"), F.Num(2)));
        Add("3x", F.Var("x"));
        Add("4xsin(x)", F.Mul(F.Sin(F.Var("x")), F.Var("x")));
    }
}