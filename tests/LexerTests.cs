using Derivate;

namespace DerivateTests;

public class LexerTests
{
    [Fact]
    public Task ParseText_ShouldParseImplicitMultiplication()
    {
        string testPrev = ")3e3.5y("; // INT, CONST, FLOAT, VAR, RPAREN 
        string testCurr = ")x )sin )ln )pi )("; // VAR TRIGs LOGs CONST LPAREN
        var tokens = Lexer.ParseText(testCurr + testPrev).ToList();
        return Verify(tokens);
    }

    [Fact]
    public Task ParseText_ShouldParseVariablesAndConstants() => Verify(Lexer.ParseText("xyzpiabce").ToList());

    [Fact]
    public Task ParseText_ShouldParseNumbers() => Verify(Lexer.ParseText("1234567890 3.4").ToList());
}