namespace Derivate;
using PrefixParselet = Func<Token, IExpression>;
using InfixParselet = Func<IExpression, Token, IExpression>;

public enum Precedence
{
    Lowest,
    Sum,
    Product,
    Exponent,
    Prefix,
}

public class Parser
{
    readonly Dictionary<TokenType, PrefixParselet> prefixParselets = new();  
    readonly Dictionary<TokenType, InfixParselet> infixParselets = new();  
    static readonly Dictionary<TokenType, Precedence> precedence = new()
    {
        [TokenType.ADD] = Precedence.Sum,
        [TokenType.SUB] = Precedence.Sum,
        [TokenType.MUL] = Precedence.Product,
        [TokenType.DIV] = Precedence.Product,
        [TokenType.EXP] = Precedence.Exponent,
        [TokenType.SIN] = Precedence.Prefix,
        [TokenType.COS] = Precedence.Prefix,
        [TokenType.TAN] = Precedence.Prefix,
        [TokenType.CSC] = Precedence.Prefix,
        [TokenType.SEC] = Precedence.Prefix,
        [TokenType.COT] = Precedence.Prefix,
        [TokenType.LOG] = Precedence.Prefix,
        [TokenType.LN]  = Precedence.Prefix,
    };
    readonly List<Token> tokens;
    int pos;
    Token current;
    Token next;

    public Parser(List<Token> _tokens) 
    {
        tokens = _tokens;
        Advance();

        Register(TokenType.LPAREN, ParseGroup);
        Register(TokenType.INT, ParseInteger);
        Register(TokenType.FLOAT, ParseFloat);
        Register(TokenType.VAR, ParseSymbol);

        Register(TokenType.SIN, ParseUnary);
        Register(TokenType.COS, ParseUnary);
        Register(TokenType.TAN, ParseUnary);
        Register(TokenType.CSC, ParseUnary);
        Register(TokenType.SEC, ParseUnary);
        Register(TokenType.COT, ParseUnary);
        Register(TokenType.LOG, ParseUnary);
        Register(TokenType.LN,  ParseUnary);
        Register(TokenType.SUB, ParseUnary);

        Register(TokenType.ADD, ParseNary);
        Register(TokenType.MUL, ParseNary);
        Register(TokenType.SUB, ParseBinary);
        Register(TokenType.DIV, ParseBinary);
        Register(TokenType.EXP, ParsePower);
    }

    Symbols ParseSymbol(Token token) 
    {
        return token.value switch
        {
            Symbols.E => Func.E,
            Symbols.I => Func.I,
            Symbols.Pi => Func.Pi,
            _ => new Variable(token.value.ToString()!)
        };
    }

    Number ParseInteger(Token token) => new((int) token.value);

    Fraction ParseFloat(Token token) 
    {
        double a = Convert.ToDouble(token.value);
        string[] dec = a.ToString().Split(".");
        int numerator = int.Parse(string.Concat(dec));
        int denominator = (int) Math.Pow(10, dec.Last().Length);
        return new(numerator, denominator);
    }

    IExpression ParseGroup(Token token)
    {
        Advance();
        IExpression expr = Parse();
        Consume(TokenType.RPAREN, "Expected ')'");
        return expr;
    }

    IExpression ParseUnary(Token op) 
    {
        Advance();
        IExpression right = ParseExpression(Precedence.Prefix);
        return op.type switch
        {
            TokenType.SIN => Func.Sin(right), 
            TokenType.COS => Func.Cos(right), 
            TokenType.TAN => Func.Tan(right), 
            TokenType.CSC => Func.Csc(right), 
            TokenType.SEC => Func.Sec(right), 
            TokenType.COT => Func.Cot(right), 
            TokenType.LOG => Func.Log(right), 
            TokenType.LN  => Func.Ln(right), 
            TokenType.SUB => Func.Mul([new Number(-1), right]),

            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };
    }

    IExpression ParseNary(IExpression left, Token op)
    {
        List<IExpression> operandList = [left]; 

        do
        {
            Advance();
            IExpression operand = ParseExpression(GetPrecedence(op.type));

            operandList.Add(operand);
        } while (Match(op.type));

        return op.type switch
        {
            TokenType.ADD => Func.Add(operandList),
            TokenType.MUL => Func.Mul(operandList),

            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };
    }

    Power ParsePower(IExpression left, Token op)
    {
        Advance();
        IExpression right = ParseExpression(GetPrecedence(op.type - 1));
        return new(left, right);
    }

    IExpression ParseBinary(IExpression left, Token op)
    {
        Advance();
        IExpression right = ParseExpression(GetPrecedence(op.type));
        return op.type switch
        {
            TokenType.DIV 
            when left is Number a 
            && right is Number b 
                => Func.Frac(a.value, b.value),
            TokenType.SUB 
                => Func.Add([left, new Product([new Number(-1), right])]),
            TokenType.DIV   
                => Func.Mul([left, new Power(right, new Number(-1))]),

            _   => throw new ArgumentOutOfRangeException(nameof(op))
        };
    }

    #region Pratt Parser
    void Register(TokenType token, PrefixParselet parselet) 
        => prefixParselets.Add(token, parselet);
    void Register(TokenType token, InfixParselet parselet) 
        => infixParselets.Add(token, parselet);

    void Advance()
    {
        if (!IsAtEnd()) pos++;
        current = tokens[pos - 1];
        next = tokens[pos];
    }

    bool IsAtEnd() => next.type == TokenType.EOF;

    void Consume(TokenType type, string err) 
    {
        if (next.type != type)
            throw Derivate.ParserError(err, pos);
        
        Advance();
    }

    bool Match(TokenType type)
    {
        if (next.type != type)
            return false;

        Advance();
        return true;
    }

    Precedence GetPrecedence(TokenType type) 
        => precedence.TryGetValue(type, out var p) ? p : Precedence.Lowest;

    public IExpression ParseExpression(Precedence precedence)
    {
        PrefixParselet prefix = prefixParselets.GetValueOrDefault(current.type) 
            ?? throw Derivate.ParserError(
                $"Expected a valid expression [{tokens[pos]}]", pos + 1
            );

        IExpression left = prefix(current);
        while (precedence < GetPrecedence(next.type))
        {
            if (!infixParselets.TryGetValue(next.type, out var infix))
                return left;

            Advance();
            left = infix(left, current);
        }
        return left;
    }

    public IExpression Parse() => ParseExpression(0);
    #endregion
}