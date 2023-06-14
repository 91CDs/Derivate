
namespace nineT1CD;

public interface NodeVisitor<T>
{
    T visitLiteral(Literal literal);
    T visitGrouping(Grouping grouping);
    T visitUnary(Unary unary);
    T visitBinary(Binary binary);
}
public abstract class Node
{
    public abstract T accept<T>(NodeVisitor<T> visitor);
    public abstract TokenType getType();
}

public sealed class Literal : Node
{
    public Object value { get; set; }
    public TokenType type { get; set; }
    public Literal(Token token)
    {
        value = token.value;
        type = token.type;
    }
    public Literal(int value)
    {
        this.value = value;
        this.type = TokenType.INT;
    }
    public Literal(double value)
    {
        this.value = value;
        if (value == Math.E || value == Math.PI)
        {
            type = TokenType.CONST;
        }
        else
        {
            type = TokenType.FLOAT;
        }
    }

    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitLiteral(this);
    }

    public override TokenType getType()
    {
        return type;
    }
}

public sealed class Grouping : Node
{

    public Node expr { get; set; }
    public Grouping(Node expr)
    {
        this.expr = expr;
    }
    
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitGrouping(this);
    }

    public override TokenType getType()
    {
        return expr.getType();
    }
}
public sealed class Unary : Node
{
    public Node right { get; set; }
    public Token operation { get; set; }
    public Unary(Token operation, Node right)
    {
        this.operation = operation;
        this.right = right;
    }
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitUnary(this);
    }
    public override TokenType getType()
    {
        return operation.type;
    }
}
public sealed class Binary : Node
{
    public Node left { get; set; }
    public Node right { get; set; }
    public Token operation { get; set; }
    public Binary(Node left, Token operation, Node right)
    {
        this.operation = operation;
        this.left = left;
        this.right = right;
    }
    public override T accept<T>(NodeVisitor<T> visitor)
    {
        return visitor.visitBinary(this);
    }
    public override TokenType getType()
    {
        return operation.type;
    }
}