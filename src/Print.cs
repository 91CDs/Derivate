namespace Derivate;

public static class Print
{
    public static string Repr(this List<Token> tokens)
    {
        return $"[ { String.Join(" , ", tokens.Select(t => t.ToString())) } ]";
    }
    
    public static void PolynomialChecker(Node expr)
    {
        string printed = new ASTPrint().print(expr);

        bool isTerm = Term.TryParseTerm(expr, out Term term);
        Console.WriteLine($"Is it a term? [{printed}] : {isTerm} => {term}");

        bool isPolynomial = Polynomial.TryParsePolynomial(expr, out Polynomial polyn);
        Console.WriteLine($"Is it a polynomial? [{printed}] : {isPolynomial} => {polyn}");
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
    public bool hasVar(Node node)
    {
        Binary? binary = node as Binary;
        if (binary == null) return false;

        return binary.left.getType() == TokenType.VAR;
    }
    public string print(Node expr)
    {
        return expr.accept<string>(this);
    }
    public string visitBinary(Binary node)
    {
        if (node.operation.type == TokenType.MUL)
        {
            bool isVar = node.right.getType() == TokenType.VAR;
            bool isVarExp = node.right.getType() == TokenType.EXP && hasVar(node.right);
            bool isGrouping = node.left as Grouping != null || node.right as Grouping != null;

            if (isVar || isGrouping || isVarExp)
                return $"{print(node.left)}{print(node.right)}";
        }

        if (node.operation.type == TokenType.EXP)
            return $"{print(node.left)}{converter.GetValueOrDefault(node.operation.type)}{print(node.right)}";

        return $"({print(node.left)} {converter.GetValueOrDefault(node.operation.type)} {print(node.right)})";
    }
    public string visitUnary(Unary node)
    {
        if (node.operation.type == TokenType.SUB)
            return $"-{print(node.right)}";

        return $"{converter.GetValueOrDefault(node.operation.type)}({print(node.right)})";
    }
    public string visitLiteral(Literal node)
    {
        double num;
        double.TryParse(node.value.ToString(), out num);
        if (num == Math.E)
            return "e";
        if (num == Math.PI)
            return "pi";

        
        return node.value.ToString()!;
    }

    public string visitGrouping(Grouping node)
    {
        return $"({print(node.expr)})";
    }
}