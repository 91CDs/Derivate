using System.Diagnostics;

namespace Derivate;
public interface NodeVisitor<T>
{
    T visitVariable(Variable variable);
    T visitLiteral(Literal literal);
    T visitUnary(Unary unary);
    T visitBinary(Binary binary);
}

public abstract record Node { 
    public abstract Token token { get; init; }
    public abstract T accept<T>(NodeVisitor<T> visitor);
    public TokenType type { get => token.type; }
    public object value { get => token.value; }

    public static Variable f(char value) => new Variable(value);
    public static Literal f(double value) => new Literal(value);
    public static Unary f(Token token, Node right) => new Unary(token, right);
    public static Binary f(Node left, Token token, Node right) => new Binary(left, token, right);
}
public sealed record Variable(Token token) : Node
{
    public Variable(char value): this(Token.VAR(value)) {}
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitVariable(this);
    }
    public void Deconstruct(out TokenType type, out object value) => (type, value) = (this.type, this.value);
}
public sealed record Literal(Token token) : Node
{
    public new double value { get => Convert.ToDouble(token.value); }
    public Literal(double value): this(
        value switch
        {
            Math.PI or Math.E => Token.CONST(value),
            var n when double.IsInteger(n) => Token.INT((int) n),
            _ => Token.FLOAT(value),
        }
    ) {}
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitLiteral(this);
    }
    public void Deconstruct(out TokenType type, out object value) => (type, value) = (this.type, this.value);
}
public sealed record Unary : Node
{
    public Node right { get; init; }
    public override Token token { get; init; }
    public Unary(Token token, Node right)
    {
        this.token = token;
        this.right = right;
    }
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitUnary(this);
    }
    public void Deconstruct(out TokenType type, out Node right) => (type, right) = (this.type, this.right);
}
public sealed record Binary : Node
{
    public Node left { get; init; }
    public Node right { get; init; }
    public override Token token { get; init; }
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
    public void Deconstruct(out Node left, out TokenType type, out Node right) 
        => (left, type, right) = (this.left, this.type, this.right);
}