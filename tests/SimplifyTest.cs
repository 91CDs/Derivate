using Derivate;

namespace DerivateTests;

public class SimplifyTests
{
    [Theory]
    [InlineData("(x^2)*(x^4)", "x^6")]  // Product Rule
    [InlineData("(5x)^2", "25x^2")]     // Power of Product
    [InlineData("(x^2)^5", "x^10")]     // Power of Power
    public void eval_PowerRule(string input, string expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var expr = new Evaluator().eval(ast);

        Assert.Equal(new FunctionPrint().print(expr), expected);
    }
}