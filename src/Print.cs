namespace Derivate;

public static class Print
{
    public static bool match(this TokenType type, params TokenType[] types)
    {
        return new List<TokenType>(types).Contains(type);
    }
    public static string Repr(this List<Token> tokens)
    {
        return $"[ {string.Join(" , ", tokens.Select(t => t.ToString())) } ]";
    }
    
    public static void printTokens(this List<Token> tokens)
    {
        Console.WriteLine("\nTOKENIZER::");
        Console.WriteLine(":::::::::::");
        Console.WriteLine(tokens.Repr());
    }

    public static void printAST(this Node AST, Node? simplifiedAST = null)
    {
        Console.WriteLine("\nAST::");
        Console.WriteLine(":::::");
        Console.WriteLine(new ASTPrint().print(AST));
        if (simplifiedAST != null)
            Console.WriteLine(new ASTPrint().print(simplifiedAST));
    }

    public static void printFunction(this Node Function, Node? simplifiedFunction = null)
    {
        Console.WriteLine("\nFUNCTION::");
        Console.WriteLine("::::::::::");
        Console.WriteLine(new FunctionPrint().print(Function));
        if (simplifiedFunction != null)
            Console.WriteLine(new FunctionPrint().print(simplifiedFunction));
    }

    // public static void PolynomialChecker(INode expr)
    // {
    //     string printed = new ASTPrint().print(expr);

    //     bool isTerm = Term.TryParseTerm(expr, out Term term);
    //     Console.WriteLine($"Is it a term? [{printed}] : {isTerm} => {term}");

    //     bool isPolynomial = Polynomial.TryParsePolynomial(expr, out Polynomial polyn);
    //     Console.WriteLine($"Is it a polynomial? [{printed}] : {isPolynomial} => {polyn}");
    // }
}

public sealed class ASTPrint : NodeVisitor<string>
{
    public string print(Node expr) => expr.accept(this);
    public string visitBinary(Binary node)
    {
        return $"({print(node.left)}){node.token.value}({print(node.right)})";
    }
    public string visitUnary(Unary node)
    {
        return $"{node.token.value}({print(node.right)})";
    }
    public string visitLiteral(Literal node)
    {
        if (node.type is TokenType.CONST)
            return Token.MathConstants((double) node.value);
            
        return node.value.ToString()!;
    }

    public string visitVariable(Variable node)
    {
        return node.value.ToString()!;
    }
}
public sealed class FunctionPrint : NodeVisitor<string>
{
    public string print(Node expr)
    {
        return expr.accept(this);
    }
    public bool hasVariable(Node node)
    {
        if (node is Binary binary)
        {
            return binary.left.token.type == TokenType.VAR;
        }

        return false;
    }
    public string visitBinary(Binary node)
    {
        bool isVar = node.right.type == TokenType.VAR;
        bool isVarExp = node.right.type == TokenType.EXP && hasVariable(node.right);
        if (node.type is TokenType.MUL && (isVar || isVarExp))
        {
            return $"{print(node.left)}{print(node.right)}";
        }
        else if (node.type.match(TokenType.MUL, TokenType.DIV, TokenType.EXP))
        {
            return $"{print(node.left)}{node.token.value}{print(node.right)}";
        }

        return $"{print(node.left)} {node.token.value} {print(node.right)}";
    }
    public string visitUnary(Unary node)
    {
        return $"{node.token.value}({print(node.right)})";
    }
    public string visitLiteral(Literal node)
    {
        return node.value.ToString()!;
    }
    public string visitVariable(Variable node)
    {
        return node.value.ToString()!;
    }
}