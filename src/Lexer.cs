namespace Derivate;
public static class Lexer 
{
    public static List<Token> ParseText(string input) 
    {
        var tokens = new List<Token>();
        int pos = 0;
        
        while (pos < input.Length)
        {
            char c = input[pos];
            if (char.IsWhiteSpace(c)) 
            {
                pos++;
                continue;
            }
            else if (char.IsAsciiDigit(c))
            {
                var token = parseNumber(input, ref pos);
                tokens.Add(token);
            }
            else if (char.IsAsciiLetter(c))
            {
                var startPos = pos;
                while (pos < input.Length && char.IsAsciiLetter(input[pos]))
                {
                    pos++;
                }
                string identifier = input.Substring(startPos, pos - startPos);
                
                while (identifier is not "")
                {
                    (Token token, string remaining) = parseIdent(identifier);

                    if (tokens.Count > 0 && hasImplicitMult(tokens.Last(), token)) {
                        tokens.Add(Token.MUL);
                    }
                    if (token.type is TokenType.ILLEGAL) {
                        Derivate.LexerError($"Invalid Character ({identifier}) [{input.Substring(pos, input.Length - pos)}]", pos);
                    }

                    tokens.Add(token);
                    identifier = remaining;
                }
            }
            else {
                var token = c switch
                {
                    '+' => Token.ADD,
                    '-' => Token.SUB,
                    '*' => Token.MUL,
                    '/' => Token.DIV,
                    '^' => Token.EXP,
                    '(' => Token.LPAREN,
                    ')' => Token.RPAREN,
                    _ => Token.ILLEGAL
                };

                if (token.type is TokenType.ILLEGAL) {
                    Derivate.LexerError($"Invalid Character ({c}) [{input.Substring(pos, input.Length - pos)}]", pos);
                }
                if (tokens.Count > 0 && hasImplicitMult(tokens.Last(), token)) {
                    tokens.Add(Token.MUL);
                }

                tokens.Add(token);
                pos++;
            }
        }
        tokens.Add(Token.EOF);
        
        return tokens;
    }

    static Token parseNumber(string input, ref int pos) 
    {
        var startPos = pos;
        var periods = 0;

        while (pos < input.Length && (char.IsDigit(input[pos]) || input[pos] == '.'))
        {
            if (input[pos] == '.') periods++;
            pos++;
            if (periods > 1) break;
        }

        string number = input.Substring(startPos, pos - startPos);
        if (periods == 1) return Token.FLOAT(double.Parse(number));
        return Token.INT(int.Parse(number));
    }

    static (Token, string) parseIdent(string identifier) {
        return identifier switch 
        {
            "sin" => (Token.SIN, ""),
            "cos" => (Token.COS, ""),
            "tan" => (Token.TAN, ""),
            "csc" => (Token.CSC, ""),
            "sec" => (Token.SEC, ""),
            "cot" => (Token.COT, ""),
            "log" => (Token.LOG, ""),
            "ln" => (Token.LN, ""),
            ['p', 'i', .. var remaining] => (Token.CONST(Math.PI), remaining),
            ['e', .. var remaining] => (Token.CONST(Math.E), remaining),
            [var x, .. var remaining] 
                when isVariable(x) => (Token.VAR(x), remaining),
            _ => (Token.ILLEGAL, "")
        };
    }

    static bool hasImplicitMult(Token prevToken, Token currToken)
    {
        // All pairs of tokens that have an implicit multiplication between them
        bool validPrevToken = new List<TokenType> { 
            TokenType.INT, TokenType.FLOAT, TokenType.CONST, TokenType.VAR, 
            TokenType.RPAREN
        }.Contains(prevToken.type);
        bool validCurrentToken = new List<TokenType> { 
            TokenType.SIN, TokenType.COS, TokenType.TAN, 
            TokenType.CSC, TokenType.SEC, TokenType.COT, 
            TokenType.LOG, TokenType.LN,
            TokenType.CONST, TokenType.VAR, 
            TokenType.LPAREN
        }.Contains(currToken.type);

        return validPrevToken && validCurrentToken;
    }

    static bool isVariable(char input) => "xyzabc".ToList().Contains(input);
}