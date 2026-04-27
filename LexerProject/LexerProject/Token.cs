namespace LexerProject;

public enum TokenType
{
    IDENTIFIER, INTEGER, REAL, REL_OP, STRING,
    LINE_COMMENT, KEYWORD, WHITESPACE, UNKNOWN
}

public record Token(TokenType Type, string Lexeme, int Line, int Col, int Pos);
