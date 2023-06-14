using System.Text;
using static nineT1CD.TokenType;

namespace nineT1CD;
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
        Node l = term();

        while (match(ADD, SUB))
        {
            Token op = previous();
            Node r = term();
            l = new Binary(l, op, r);
        }

        return l;
    }
    Node term()
    {
        var l = exponent();

        while (match(MUL, DIV))
        {
            Token op = previous();
            var r = exponent();

            l = new Binary(l, op, r);
        }
        return l;
    }
    Node exponent()
    {
        Node l = unary();

        if (match(EXP))
        {
            Token op = previous();
            Node r = exponent();
            l = new Binary(l, op, r);
        }

        return l;
    }

    Node unary()
    {
        while (match(SIN, COS, TAN, CSC, SEC, COT, LOG, LN, SUB))
        {
            Token op = previous();
            var r = factor();
            return new Unary(op, r);
        }

        return factor();
    }

    Node factor()
    {   
        if (match(INT))
            return new Literal(int.Parse(previous().value));
        if (match(FLOAT, CONST))
            return new Literal(double.Parse(previous().value));
        if (match(VAR))
            return new Literal(previous());

        if (match(LPAREN))
        {
            Node expr = expression();
            consume(RPAREN,  "Expected ')'");
            return new Grouping(expr);
        }

        Derivate.Error("Expected a valid expression", pos + 1);
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