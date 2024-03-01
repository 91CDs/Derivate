using System.Diagnostics;

namespace Derivate;
public interface NodeVisitor<T>
{
    T visitVariable(Variable variable);
    T visitLiteral(Literal literal);
    T visitUnary(Unary unary);
    T visitBinary(Binary binary);
}

public abstract class Node { 
    public abstract Token token { get; set; } 
    public TokenType type { get => token.type; }
    public object value { get => token.value; }
    public abstract T accept<T>(NodeVisitor<T> visitor);

    public static Variable f(char value) => new Variable(value);
    public static Literal f(double value) => new Literal(value);
    public static Unary f(Token token, Node right) => new Unary(token, right);
    public static Binary f(Node left, Token token, Node right) => new Binary(left, token, right);
}
public sealed class Variable : Node
{
    public override Token token { get; set; }
    public Variable(Token token)
    {
        Debug.Assert( token.type is TokenType.VAR );
        this.token = token;
    }
    public Variable(char value): this(Token.VAR(value)) {}
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitVariable(this);
    }
    public void Deconstruct(out TokenType type, out object value) => (type, value) = (this.type, this.value);
}
public sealed class Literal : Node
{
    public override Token token { get; set; }
    public Literal(Token token) 
    {
        Debug.Assert( token.type.match(TokenType.INT, TokenType.FLOAT, TokenType.CONST) );
        this.token = token;
    }
    public Literal(double value)
    {
        token = value switch
        {
            Math.PI or Math.E => Token.CONST(value),
            var n when double.IsInteger(n) => Token.INT((int) n),
            _ => Token.FLOAT(value),
        };
    }
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitLiteral(this);
    }
    public void Deconstruct(out TokenType type, out object value) => (type, value) = (this.type, this.value);
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
    public void Deconstruct(out Token token, out Node right) => (token, right) = (this.token, this.right);
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
    public void Deconstruct(out Node left, out Token token, out Node right) 
        => (left, token, right) = (this.left, this.token, this.right);
}