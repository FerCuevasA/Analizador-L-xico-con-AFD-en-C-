namespace LexerProject;

public enum TokenType
{
    KEYWORD, IDENTIFIER, INTEGER, REAL,
    REL_OP, ASSIGN_OP, ARITH_OP, SPECIAL_SYM,
    STRING, LINE_COMMENT, WHITESPACE, UNKNOWN
}

public record Token(TokenType Type, string Lexeme, int Line, int Col, int Pos)
{
    public int Id => TokenIds.Get(Type, Lexeme);
}

public static class TokenIds
{
    public static int Get(TokenType type, string lexeme) => type switch
    {
        TokenType.KEYWORD => lexeme switch
        {
            "if"     => 100,
            "else"   => 101,
            "while"  => 102,
            "for"    => 103,
            "int"    => 104,
            "float"  => 105,
            "return" => 106,
            "void"   => 107,
            "true"   => 108,
            "false"  => 109,
            _        => 100,
        },
        TokenType.IDENTIFIER => 200,
        TokenType.INTEGER    => 201,
        TokenType.REAL       => 202,
        TokenType.ARITH_OP => lexeme switch
        {
            "+" => 300,
            "-" => 301,
            "*" => 302,
            "/" => 303,
            _   => 300,
        },
        TokenType.REL_OP => lexeme switch
        {
            "==" => 310,
            "!=" => 311,
            "<"  => 312,
            ">"  => 313,
            "<=" => 314,
            ">=" => 315,
            _    => 310,
        },
        TokenType.ASSIGN_OP => lexeme switch
        {
            "="  => 320,
            "+=" => 321,
            "-=" => 322,
            "*=" => 323,
            "/=" => 324,
            _    => 320,
        },
        TokenType.SPECIAL_SYM => lexeme switch
        {
            "(" => 400,
            ")" => 401,
            "{" => 402,
            "}" => 403,
            "," => 404,
            ";" => 405,
            "." => 406,
            _   => 400,
        },
        TokenType.LINE_COMMENT => 500,
        TokenType.STRING       => 600,
        TokenType.UNKNOWN      => 999,
        _                      => 0,
    };
}
