namespace nineT1CD;

public static class Debug
{
    public static string Repr(this List<Token> tokens)
    {
        return $"[ { String.Join(" , ", tokens.Select(t => t.ToString())) } ]";
    }
}

public sealed class ASTPrint : NodeVisitor<string>
{
    public string print(Node expr)
    {
        return expr.accept<string>(this);
    }
    public string visitBinary(Binary node)
    {
        return $"( {print(node.left)} {node.operation} {print(node.right)} )";
    }
    public string visitUnary(Unary node)
    {
        return $"( {node.operation} {print(node.right)} )";
    }
    public string visitLiteral(Literal node)
    {
        return node.value.ToString()!;
    }

    public string visitGrouping(Grouping grouping)
    {
        return $"( g {print(grouping.expr)} )";
    }
}
public sealed class FunctionPrint : NodeVisitor<string>
{
    Dictionary<TokenType, string> converter = 
    new Dictionary<TokenType, string>()
    {
        {TokenType.SIN, "sin"},
        {TokenType.COS, "cos"},
        {TokenType.TAN, "tas"},
        {TokenType.CSC, "csc"},
        {TokenType.SEC, "sec"},
        {TokenType.COT, "cot"},
        {TokenType.LOG, "log"},
        {TokenType.LN,  "ln"},
        {TokenType.ADD, "+"},
        {TokenType.SUB, "-"},
        {TokenType.MUL, "*"},
        {TokenType.DIV, "/"},
        {TokenType.EXP, "^"}
    };
    public string print(Node expr)
    {
        return expr.accept<string>(this);
    }
    public string visitBinary(Binary node)
    {
        if (node.operation.type == TokenType.MUL)
        {
            bool isVar = node.right.getType() == TokenType.VAR;
            bool isGrouping = node.left as Grouping != null || node.right as Grouping != null;

            if (isVar || isGrouping)
                return $"{print(node.left)}{print(node.right)}";
        }

        if (node.operation.type == TokenType.EXP)
            return $"{print(node.left)}{converter.GetValueOrDefault(node.operation.type)}{print(node.right)}";

        return $"{print(node.left)} {converter.GetValueOrDefault(node.operation.type)} {print(node.right)}";
    }
    public string visitUnary(Unary node)
    {
        if (node.operation.type == TokenType.SUB)
            return $"-{print(node.right)}";

        return $"{converter.GetValueOrDefault(node.operation.type)}({print(node.right)})";
    }
    public string visitLiteral(Literal node)
    {
        return node.value.ToString()!;
    }

    public string visitGrouping(Grouping node)
    {
        return $"({print(node.expr)})";
    }
}