using Derivate;

namespace DerivateTests;

public class DerivativeTests
{
    // All rules uses the generalized chain rule formula 
    // ex:: dx(u^n) = u' * nu^(n-1) for power rule

    // Algebraic Functions
    [Theory]
    [InlineData("20", "0")]                                     // Constant Rule
    [InlineData("2x", "2*1")]                                   // Constant Multiple Rule
    [InlineData("-x", "-(1)")]
    [InlineData("x^2", "1*2x^1")]                               // Power Rule
    [InlineData("x^2 + x^2", "1*2x^1 + 1*2x^1")]                // Sum Rule
    [InlineData("x^2 - x^2", "1*2x^1 - 1*2x^1")]                // Difference Rule
    [InlineData("x^2 * x^3", "x^2*1*3x^2 + x^3*1*2x^1")]        // Product Rule
    [InlineData("x^2 / x^3", "x^3*1*2x^1 - x^2*1*3x^2/x^3^2")]  // Quotient Rule
    [InlineData("5x^5 + 10x - 20", "5*1*5x^4 + 10*1 - 0")]      // -> polynomial
    // Trigonometric Functions
    [InlineData("sin(x)", "1*cos(x)")]
    [InlineData("cos(x)", "1*-(sin(x))")]
    [InlineData("tan(x)", "1*sec(x)^2")]
    [InlineData("csc(x)", "1*-(csc(x)*cot(x))")]
    [InlineData("sec(x)", "1*sec(x)*tan(x)")]
    [InlineData("cot(x)", "1*-(csc(x)^2)")]
    // Exponential & Logarithmic Functions
    [InlineData("ln(x)", "1*1/x")]
    [InlineData("log(x)", "1*1/x*ln(10)")]
    public void Dx_ShouldFindDerivativeOfExpression(string input, string expected)
    {
        var ast = new Parser(Lexer.ParseText(input)).Parse();
        var dx = new Derivative().dx(ast);

        Assert.Equal(new FunctionPrint().print(dx), expected);
    }
}