namespace Derivate;
public enum TokenType
{ 
    SUB, ADD, MUL, DIV, EXP,        // Algebraic 
    LOG, LN,                        // Non Algebraic
    SIN, COS, TAN, CSC, SEC, COT,   // Trigonometric
    LPAREN, RPAREN,
    INT, FLOAT, CONST,
    VAR,
    ILLEGAL,
    UNDEFINED, DNE,                 // for no answer (used in evaluation)
    EOF,
}
public readonly record struct Token(TokenType type, object? value = null)
{
    public static readonly Token SUB = new(TokenType.SUB);
    public static readonly Token ADD = new(TokenType.ADD);
    public static readonly Token MUL = new(TokenType.MUL);
    public static readonly Token DIV = new(TokenType.DIV);
    public static readonly Token EXP = new(TokenType.EXP);
    public static readonly Token LOG = new(TokenType.LOG);
    public static readonly Token LN = new(TokenType.LN);
    public static readonly Token SIN = new(TokenType.SIN);
    public static readonly Token COS = new(TokenType.COS);
    public static readonly Token TAN = new(TokenType.TAN);
    public static readonly Token CSC = new(TokenType.CSC);
    public static readonly Token SEC = new(TokenType.SEC);
    public static readonly Token COT = new(TokenType.COT);
    public static readonly Token LPAREN = new(TokenType.LPAREN);
    public static readonly Token RPAREN = new(TokenType.RPAREN);
    public static Token INT(int number) => new(TokenType.INT, number);
    public static Token FLOAT(float number) => new(TokenType.FLOAT, number);
    public static Token CONST(double constant) => new(TokenType.CONST, constant);
    public static Token VAR(char variable) => new(TokenType.VAR, variable);
    public static readonly Token ILLEGAL = new(TokenType.ILLEGAL);
    public static readonly Token UNDEFINED = new(TokenType.UNDEFINED);
    public static readonly Token DNE = new(TokenType.DNE);
    public static readonly Token EOF = new(TokenType.EOF);

    public override string ToString()
    {
        return value != null ? $"{type}:{value}" : type.ToString();
    }
}
