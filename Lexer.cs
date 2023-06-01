using System.Text;

namespace nineT1CD;

public enum TokenType
{ 
    SUB, ADD, MUL, DIV, EXP, /* Algebraic */ 
    LOG, LN, /* Non Algebraic */
    SIN, COS, TAN, CSC, SEC, COT,
    LPAREN, RPAREN, 
    INT, FLOAT, CONST,
    VAR
}
public class Token
{
    public const string Numbers = "0123456789";
    public const string Variables = "abcxyz";
    public const double PI = Math.PI;
    public const double E = Math.E;
    public static readonly HashSet<string> Trigonometry = new HashSet<string>() 
        {"sin", "cos", "tan", "csc", "sec", "cot"};

    public TokenType type { get; set; }
    public string value { get; set; }
    public Token(TokenType type, string value = "")
    {
        this.type = type;
        this.value = value;
    }

    public override string ToString()
    {
        return value != "" ? $"{type}:{value}" : type.ToString();
    }
}

public class Lexer
{
    string text;
    int pos = 0;
    char currentChar;
    public Lexer(string text)
    {
        this.text = text;
        this.currentChar = text[0];
    }

    string getString(int offset)
    {   
        while (offset > text.Length - pos)
            offset--;
        
        return text.Substring(pos, offset); 
    }

    void next(int step = 1) 
    {
        pos = pos + step;
        currentChar = pos < text.Length ? text[pos] : '\0';
    }
    Token parseNumber()
    {
        Token numberToken;
        StringBuilder number = new StringBuilder();
        int periods = 0;
        
        if (text[pos] == '-')
        {
            number.Append(currentChar);
            next();
        }

        while (Token.Numbers.Contains(currentChar) || currentChar == '-')
        {            
            if (text[pos] == '.')
            {
                if (periods > 1) break;
                periods++;
            }

            number.Append(currentChar);
            next();
        }

        if (periods == 1)
            numberToken = new Token(TokenType.FLOAT, number.ToString());
        else
            numberToken = new Token(TokenType.INT, number.ToString());

        return numberToken;
    }

    Token parseTrigFunctions()
    {
        string trig = getString(3);
        return trig switch
        {
            "sin" => new Token(TokenType.SIN),
            "cos" => new Token(TokenType.COS),
            "tan" => new Token(TokenType.TAN),
            "csc" => new Token(TokenType.CSC),
            "sec" => new Token(TokenType.SEC),
            "cot" => new Token(TokenType.COT),
            _ => throw new ArgumentOutOfRangeException(nameof(currentChar), $"Invalid Character: {currentChar}")
        };
    }
    public List<Token> parseText()
    {
        List<Token> tokens = new List<Token>();

        while (pos < text.Length)
        {
            if (currentChar == ' ')
            {
                next();
            }
            else if (Token.Numbers.Contains(currentChar))
            {
                tokens.Add(parseNumber());

                if (Token.Trigonometry.Contains(getString(3))   /* 3sin(3) */
                    || Token.Variables.Contains(currentChar)    /* 3x */
                    || getString(3) == "log"                    /* 3log(3) */
                    || getString(2) == "ln"                     /* 3ln(3) */
                    || getString(2) == "pi"                     /* 3π */
                    || currentChar == 'e'                       /* 3e */
                    || currentChar == '('                       /* 3(10) */
                )
                    tokens.Add(new Token(TokenType.MUL)); 

            }
            else if (Token.Trigonometry.Contains(getString(3)))
            {
                tokens.Add(parseTrigFunctions());
                next(3);
            }
            else if (Token.Variables.Contains(currentChar))
            {
                tokens.Add(new Token(TokenType.VAR));
                next();
            }
            else if (currentChar == '+')
            {
                tokens.Add(new Token(TokenType.ADD));
                next();
            }
            else if (currentChar == '-')
            {
                tokens.Add(new Token(TokenType.SUB));
                next();
            }
            else if (currentChar == '*')
            {
                tokens.Add(new Token(TokenType.MUL));
                next();
            }
            else if (currentChar == '/')
            {
                tokens.Add(new Token(TokenType.DIV));
                next();
            }
            else if (currentChar == '^')
            {
                tokens.Add(new Token(TokenType.EXP));
                next();
            }
            else if (getString(2) == "ln")
            {
                tokens.Add(new Token(TokenType.LN));
                next(2);
            }
            else if (getString(3) == "log")
            {
                tokens.Add(new Token(TokenType.LOG));
                next(3);
            }
            else if (getString(2) == "pi")
            {
                tokens.Add(new Token(TokenType.CONST, Math.PI.ToString()));
                next(2);
            }
            else if (currentChar == 'e')
            {
                tokens.Add(new Token(TokenType.CONST, Math.E.ToString()));
                next();
            }
            else if (currentChar == '(')
            {
                tokens.Add(new Token(TokenType.LPAREN));
                next();
            }
            else if (currentChar == ')')
            {
                tokens.Add(new Token(TokenType.RPAREN));
                next();

                if (currentChar == '(') /* (3)(3) */
                    tokens.Add(new Token(TokenType.MUL));
            }
            else
            {
                char charException = currentChar;
                string unparsed = text.Substring(pos, text.Length - pos);
                throw new ArgumentOutOfRangeException(nameof(charException), $"Invalid Character: {charException} => {unparsed}");
            }
        }

        return tokens;
    }
}