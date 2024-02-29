using System.Diagnostics;

namespace Derivate;

// TODO: Change token to use 1 record struct instead of enum & record structs (for easy member access & no type bs)
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

public readonly record struct Token(TokenType type, object value)
{
    public static readonly Token EOF = new Token(TokenType.EOF, "EOF");
    public static readonly Token ILLEGAL = new Token(TokenType.ILLEGAL, "ILLEGAL");

    public static Token INT(int number) => new Token(TokenType.INT, number);
    public static Token FLOAT(double number) => new Token(TokenType.FLOAT, number);
    public static Token CONST(double constant) => new Token(TokenType.CONST, constant);
    public static Token VAR(char variable) => new Token(TokenType.VAR, variable);

    public static readonly Token SUB = new Token(TokenType.SUB, "-");
    public static readonly Token ADD = new Token(TokenType.ADD, "+");
    public static readonly Token MUL = new Token(TokenType.MUL, "*");
    public static readonly Token DIV = new Token(TokenType.DIV, "/");
    public static readonly Token EXP = new Token(TokenType.EXP, "^");
    public static readonly Token LOG = new Token(TokenType.LOG, "log");
    public static readonly Token LN = new Token(TokenType.LN, "ln");
    public static readonly Token SIN = new Token(TokenType.SIN, "sin");
    public static readonly Token COS = new Token(TokenType.COS, "cos");
    public static readonly Token TAN = new Token(TokenType.TAN, "tan");
    public static readonly Token CSC = new Token(TokenType.CSC, "csc");
    public static readonly Token SEC = new Token(TokenType.SEC, "sec");
    public static readonly Token COT = new Token(TokenType.COT, "cot");
    public static readonly Token LPAREN = new Token(TokenType.LPAREN, "(");
    public static readonly Token RPAREN = new Token(TokenType.RPAREN, ")");
    public static readonly Token UNDEFINED = new Token(TokenType.UNDEFINED, "UNDEFINED");
    public static readonly Token DNE = new Token(TokenType.DNE, "DOES NOT EXIST");

    public override string ToString()
    {
        return type switch {
            TokenType.CONST => $"{type}:{MathConstants((double) value)}",
            _ => $"{type}:{value}",
        };
    }

    public static string MathConstants(double constant) 
    {
        return constant switch
        {
            Math.PI => "pi",
            Math.E => "e",
            _ => throw new UnreachableException(),
        };
    }
}