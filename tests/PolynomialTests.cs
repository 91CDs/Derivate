using F = Derivate.Func;
using Derivate;

namespace DerivateTests;
public class PolynomialTests
{
    [Theory]
    [ClassData(typeof(PolynomialTestData))]
    public void isGeneralPolynomial_CheckIfExpressionIsPolynomial(
        string input,
        HashSet<IExpression> vars,
        bool expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var degree = expr.isGeneralPolynomial(vars);

        Assert.Equal(degree, expected);
    }

    [Theory]
    [ClassData(typeof(MonomialTestData))]
    public void isGeneralMonomial_CheckIfExpressionIsMonomial(
        string input,
        HashSet<IExpression> vars,
        bool expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var degree = expr.isGeneralMonomial(vars);

        Assert.Equal(degree, expected);
    }

    [Theory]
    [ClassData(typeof(DegreeTestData))]
    public void Degree_getDegree(
        string input,
        HashSet<IExpression> vars,
        int expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var degree = expr.Degree(vars);

        Assert.Equal(degree, expected);
    }

    [Theory]
    [ClassData(typeof(LeadingCoefficientTestData))]
    public void LeadingCoefficient_getLeadingCoefficient(
        string input,
        IExpression var,
        IExpression expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var term = expr.LeadingCoefficient(var);

        Assert.Equal(term.ConvertToString(), expected.Simplify().ConvertToString());
    }

    [Theory]
    [ClassData(typeof(VariablesTestData))]
    public void GetVariables_getVariables(
        string input,
        HashSet<IExpression> expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = Evaluator.Simplify(ast);
        var degree = expr.GetVariables();

        Assert.Equal(degree, expected);
    }

    [Theory]
    [ClassData(typeof(DivideTestData))]
    public void DivRem_DivideTwoPolynomials(
        string input,
        string input2,
        string input3,
        IExpression expectedQuot,
        IExpression expectedRem)
    {
        var dividend = new Parser(Lexer.ParseText(input)).Parse().Simplify();
        var divisor = new Parser(Lexer.ParseText(input2)).Parse().Simplify();
        var variable = new Parser(Lexer.ParseText(input3)).Parse().Simplify();
        var (quot, rem) = dividend.DivRem(divisor, variable);

        Assert.Equal(quot, expectedQuot.Simplify());
        Assert.Equal(rem, expectedRem.Simplify());
    }
}

public class MonomialTestData : TheoryData<string, HashSet<IExpression>, bool>
{
    public MonomialTestData()
    {
        Add("3x^2yz", [F.Var("x"), F.Var("y")], true); // Multiple vars
        Add("2x^2tan(x)", [F.Tan(F.Var("x"))], true);  // Allows functions as vars
        Add("2z(x+1)^2", [F.Add(F.Var("x"), F.Num(1))], true);  // Allows sums as vars
        Add("2yx^2/(y-1)", [F.Var("x")], true); // general expr as coefficients in monomial
        Add("9^(1/2)x", [F.Var("x")], true);

        Add("x^2 + y^2", [F.Var("x"), F.Var("y")], false); // Only Monomials allowed
        Add("3x^2z/y^2", [F.Var("x"), F.Var("y")], false); // Exponent should be > 1
    }
}
public class PolynomialTestData : TheoryData<string, HashSet<IExpression>, bool>
{
    public PolynomialTestData()
    {
        Add("3x^2 - 3y^2", [F.Var("x"), F.Var("y")], true); 
        Add("(2z/(2z+1))(x+1)^3 + 2y(x+1)^2 + 9^(1/2)x", 
            [F.Add(F.Var("x"), F.Num(1))], 
            true); 
    }
}
public class DegreeTestData : TheoryData<string, HashSet<IExpression>, int>
{
    public DegreeTestData()
    {
        Add("3yx^2 + 2xy^3 + 1", [F.Var("x")], 2);
        Add("3sin(x)^3 + 2sin(x)^4 + y", [F.Sin(F.Var("x"))], 4);
        Add("3x^2y^5z - 2xy^7 + 9x^2y", [F.Var("x"), F.Var("y")], 8);
    }
}
public class LeadingCoefficientTestData : TheoryData<string, IExpression, IExpression>
{
    public LeadingCoefficientTestData()
    {
        Add("3yx^2 + 2xy^3 + 1", 
            F.Var("x"), 
            F.Mul(F.Num(3), F.Var("y"))
        );
        Add("3sin(x)^3 + 2sin(x)^4 + y", 
            F.Sin(F.Var("x")), 
            F.Num(2)
        );
    }
}
public class VariablesTestData : TheoryData<string, HashSet<IExpression>>
{
    public VariablesTestData()
    {
        Add("3yx^2 + 2xy^3 + 1", [F.Var("x"), F.Var("y")]);
        Add("2yz^2cos(x^2)", [F.Var("y"), F.Var("z"), F.Cos(F.Pow(F.Var("x"), F.Num(2)))]);
        Add("3^(1/2)x + 2^(1/3)x + 1", [F.Root(F.Num(3), 2), F.Root(F.Num(2), 3), F.Var("x")]);
    }
}
public class DivideTestData : TheoryData<string, string, string, IExpression, IExpression>
{
    public DivideTestData()
    {
        Add("x^2 + 3x - 10", "x - 2", "x", 
            F.Add(F.Num(5), F.Var("x")), 
            F.Num(0));
        Add("2x^5 + 6x^4 - 20x^3 - 5x^2 - 13x + 45", "x^2 + 3x - 10", "x", 
            F.Add(F.Mul(F.Num(2), F.Pow(F.Var("x"), F.Num(3))), F.Num(-5)),
            F.Add(F.Mul(F.Num(2), F.Var("x")), F.Num(-5)));
        Add("x", "x + 2^(1/2)x", "x",
            F.Mul(F.Num(1), F.Div(
                F.Add(
                    F.Num(1), 
                    F.Pow(F.Num(2), F.Frac(1, 2))
                )
            )),
            F.Num(0));
    }
}