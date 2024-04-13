using F = Derivate.Func;
using Derivate;

namespace DerivateTests;

public class DerivativeTests
{
    [Theory]
    [ClassData(typeof(DerivativeTestData))]
    public void Dx_ShouldFindDerivativeOfExpression(string input, IExpression expected)
    {
        Node ast = new Parser(Lexer.ParseText(input)).Parse();
        IExpression dx = Derivative.Dx(ast.ToFunction().Simplify());

        Assert.Equal(dx.ConvertToString(), expected.Simplify().ConvertToString());
    }
}

public class DerivativeTestData : TheoryData<string, IExpression>
{
    public DerivativeTestData()
    {
        var XSquared       = F.Pow(varX, F.Num(2));
        var XCubedPlus1    = F.Add(F.Pow(varX, F.Num(3)), F.Num(1));
        var dx_XSquared    = F.Mul(F.Num(2), varX);
        var dx_XCubedPlus1 = F.Mul(F.Num(3), F.Pow(varX, F.Num(2)));
        
        // All rules uses the generalized chain rule formula 
        // ex:: dx(u^n) = u' * nu^(n-1) for power rule
        Add( "20", F.Num(0) );      // Constant Rule
        Add( "2x", F.Num(2) );      // Constant Multiple Rule
        Add( "-x", F.Num(-1) );     // <-
        Add( "x^2", dx_XSquared );  // Power Rule
        Add( "x^3 + 1", dx_XCubedPlus1 );    // <-
        Add( "x^2 + x^2",           // Sum Rule
            F.Add(dx_XSquared, dx_XSquared));
        Add( "x^2 - x^2",           // Difference Rule
            F.Add(dx_XSquared, F.Sub(dx_XSquared))); 
        Add( "x^2 * (x^3 + 1)",     // Product Rule
            F.Add(
                F.Mul(XSquared, dx_XCubedPlus1), 
                F.Mul(XCubedPlus1, dx_XSquared)));
        Add( "x^2 / (x^3 + 1)",     // Quotient Rule 
            F.Mul(
                F.Add(
                    F.Mul(XCubedPlus1, dx_XSquared), 
                    F.Sub(F.Mul(XSquared, dx_XCubedPlus1))), 
                F.Div(F.Pow(XCubedPlus1, F.Num(2)))));
        Add( "5x^5 + 10x - 20",     // Polynomial
            F.Add(
                F.Num(10),
                F.Mul(
                    F.Num(25), 
                    F.Pow(varX, F.Num(4)))));
        Add("e^x", F.Pow(F.E, varX));
        Add("(x^3 + 1)^(x^2)", 
            F.Mul(
                F.Pow(XCubedPlus1, XSquared),
                F.Add(
                    F.Mul(XSquared, dx_XCubedPlus1, F.Div(XCubedPlus1)),
                    F.Mul(dx_XSquared, F.Ln(XCubedPlus1)))));

        // Trigonometric Functions
        Add( "sin(x)", F.Cos(varX));
        Add( "cos(x)", F.Sub(F.Sin(varX)));
        Add( "tan(x)", F.Pow(F.Sec(varX), F.Num(2)));
        Add( "csc(x)", F.Sub(F.Mul(F.Cot(varX), F.Csc(varX))));
        Add( "sec(x)", F.Mul(F.Sec(varX), F.Tan(varX)));
        Add( "cot(x)", F.Sub(F.Pow(F.Csc(varX), F.Num(2))));

        // Exponential & Logarithmic Functions
        Add( "ln(x)" , F.Pow(varX, F.Num(-1)));
        Add( "log(x)", F.Div(F.Mul(F.Ln(F.Num(10)), varX)));
    }

    private static IExpression varX => F.Var("x");
}