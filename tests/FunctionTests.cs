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