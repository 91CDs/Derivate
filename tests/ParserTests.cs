using Derivate;

namespace DerivateTests;

public class ParserTests
{
    [Theory]
    [InlineData("-5x^2 + 4x/2"      ,"((-(5))*((x)^(2)))+(((4)*(x))/(2))")] 
    [InlineData("3cos(2x)/2"        ,"((3)*(cos((2)*(x))))/(2)")]
    [InlineData("2xln(3e^(2x-1))"   ,"((2)*(x))*(ln((3)*((e)^(((2)*(x))-(1)))))")]
    public void Parse_ShouldParseMathExpression(string input, string expected)
    {
        var parser = new Parser(Lexer.ParseText(input));
        var ast = parser.Parse();

        Assert.Equal(ast.Format(), expected);
    }
}