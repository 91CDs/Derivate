using System.Text;
using static nineT1CD.TokenType;

namespace nineT1CD;

public class Node
{

    public Token value { get; set; }
    public Node? left { get; set; }
    public Node? right { get; set; }

    public Node(Token value)
    {
        this.value = value;
    }

    public Node(Token value, Node right)
    {
        this.value = value;
        this.right = right;
    }

    public Node(Token value, Node left, Node right)
    {
        this.value = value;
        this.left = left;
        this.right = right;
    }

    public override string ToString()
    {
        StringBuilder nodeRepr = new StringBuilder("()");

        int index() {
            return nodeRepr.Length == 0 ? 0 : nodeRepr.Length - 1;
        }

        if (left == null && right == null)
            nodeRepr.Clear();

        if (left != null)
            nodeRepr.Insert(index(), $" { left.ToString() } ");

        nodeRepr.Insert(index(), value.ToString());

        if (right != null)
            nodeRepr.Insert(index(), $" { right.ToString() } ");

        return nodeRepr.ToString();
    }
}

public class Parser
{
    int pos = 0;
    List<Token> tokens;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    Token advance()
    {
        if (!isAtEnd()) pos++;
        return previous();
    }

    Token current()
    {
        return tokens[pos];
    }

    Token previous()
    {
        return tokens[pos - 1];
    }  
    bool isAtEnd()
    {
        return pos == tokens.Count;
    }

    bool check(TokenType type)
    {
        if (isAtEnd()) return false;
        return current().type == type;
    }

    /* matches current token and if its true, advances to the next token */
    bool match(params TokenType[] types) 
    {
        foreach (var type in types)
        {
            if (check(type))
            {
                advance();
                return true;
            }
        }

        return false;
    }

    void consume(TokenType type, string err) 
    {
        if (check(type))
        {
            advance();
            return;
        }

        Derivate.Error(err, pos);
    }

    Node expression()
    {
        Node left = exponent();

        while (match(ADD, SUB))
        {
            Token op = previous();
            Node right = exponent();
            left = new Node(op, left, right);
        }

        return left;
    }
    Node exponent()
    {
        Node right = term();

        while (match(EXP))
        {
            Token op = previous();
            Node left = term();
            right = new Node(op, left, right);
        }

        return right;
    }
    Node term()
    {
        Node left = unary();

        while (match(MUL, DIV))
        {
            Token op = previous();
            Node right = unary();
            left = new Node(op, left, right);
        }

        return left;
    }

    Node unary()
    {
        while (match(SIN, COS, TAN, CSC, SEC, COT, LOG, LN, SUB))
        {
            Token op = previous();
            Node right = factor();
            return new Node(op, right);
        }

        return factor();
    }

    Node factor()
    {   
        if (match(INT, FLOAT, VAR, CONST))
            return new Node(previous());

        if (match(LPAREN))
        {
            Node expr = expression();
            consume(RPAREN,  "Expected ')'");
            return expr;
        }

        Derivate.Error("Expected a valid expression", pos);
        throw new Exception();
    }

    public Node Parse()
    {
        return expression();
    }
}

/* 
Factor
    Number
    | Variable
    | Constant
    | 

Unary
    TRIGONOMETRY( <factor> ) 
    | LOGARITHM( <factor> )
    | factor

Term
    <unary> * <unary>
    | <unary> / <unary>
    | <unary> ^ <unary>
    | unary

Expression
    <term> Plus <term>
    | <term> Minus <term>
    | term
*/

/* 
    * Example: 8x^6
    * 8 * x ^ 6    | expr()
    * <8> * x ^ 6  | match is true > factor | *for each true match, it advances to next thing
    * 8 <*> x ^ 6  | match is true > term
    * 8 * <x> ^ 6  | match is true > factor
    * 8 * x <^> 6  | match is true > term
    * 8 * x ^ <6>  | match is true > factor
    * 8*x^6 </>    | end
*/