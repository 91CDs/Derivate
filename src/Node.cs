using System.Diagnostics;

namespace Derivate;
public interface NodeVisitor<T>
{
    T visitLiteral(Literal literal);
    T visitUnary(Unary unary);
    T visitBinary(Binary binary);
}

public abstract class Node { 
    public abstract Token token { get; set; } 
    public abstract T accept<T>(NodeVisitor<T> visitor);
    public static Literal f(Token token) => new Literal(token);
    public static Literal f(int value) => new Literal(value);
    public static Literal f(double value) => new Literal(value);
    public static Literal f(char value) => new Literal(value);
    public static Unary f(Token token, Node right) => new Unary(token, right);
    public static Binary f(Node left, Token token, Node right) => new Binary(left, token, right);
}
public sealed class Literal : Node
{
    public override Token token { get; set; }
    public Literal(Token token)
    {
        Debug.Assert( token.type.match(TokenType.INT, TokenType.FLOAT, TokenType.CONST, TokenType.VAR) );
        this.token = token;
    }
    public Literal(int value)
    {
        token = Token.INT(value);
    }
    public Literal(double value)
    {
        token = value switch
        {
            Math.PI or Math.E => Token.CONST(value),
            _ => Token.FLOAT(value),
        };
    }
    public Literal(char value)
    {
        token = Token.VAR(value);
    }

    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitLiteral(this);
    }
}
public sealed class Unary : Node
{
    public Node right { get; set; }
    public override Token token { get; set; }
    public Unary(Token token, Node right)
    {
        this.token = token;
        this.right = right;
    }
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitUnary(this);
    }
}
public sealed class Binary : Node
{
    public Node left { get; set; }
    public Node right { get; set; }
    public override Token token { get; set; }
    public Binary(Node left, Token token, Node right)
    {
        this.token = token;
        this.left = left;
        this.right = right;
    }
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitBinary(this);
    }
}