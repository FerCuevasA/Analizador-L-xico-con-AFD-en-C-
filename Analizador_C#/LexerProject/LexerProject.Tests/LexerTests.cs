using LexerProject;
using Xunit;

namespace LexerProject.Tests;

public class LexerTests
{
    // ── helpers ──────────────────────────────────────────────────────────────
    private static Token First(string src)
    {
        var (tokens, _) = new Lexer(src).Tokenize();
        return tokens[0];
    }

    private static List<Token> All(string src)
    {
        var (tokens, _) = new Lexer(src).Tokenize();
        return tokens;
    }

    private static List<ValidationResult> Validations(string src)
    {
        var (_, v) = new Lexer(src).Tokenize();
        return v;
    }

    // ── IDENTIFIER ──────────────────────────────────────────────────────────
    [Fact] public void Identifier_Simple()
    {
        var t = First("hello");
        Assert.Equal(TokenType.IDENTIFIER, t.Type);
        Assert.Equal("hello", t.Lexeme);
    }

    [Fact] public void Identifier_StartsWithUnderscore()
    {
        var t = First("_var");
        Assert.Equal(TokenType.IDENTIFIER, t.Type);
        Assert.Equal("_var", t.Lexeme);
    }

    [Fact] public void Identifier_AlphanumericMix()
    {
        var t = First("myVar123");
        Assert.Equal(TokenType.IDENTIFIER, t.Type);
        Assert.Equal("myVar123", t.Lexeme);
    }

    // ── INTEGER ─────────────────────────────────────────────────────────────
    [Fact] public void Integer_Simple()
    {
        var t = First("42");
        Assert.Equal(TokenType.INTEGER, t.Type);
        Assert.Equal("42", t.Lexeme);
    }

    [Fact] public void Integer_Zero()
    {
        var t = First("0");
        Assert.Equal(TokenType.INTEGER, t.Type);
        Assert.Equal("0", t.Lexeme);
    }

    [Fact] public void Integer_LongNumber()
    {
        var t = First("1234567890");
        Assert.Equal(TokenType.INTEGER, t.Type);
        Assert.Equal("1234567890", t.Lexeme);
    }

    // ── REAL ────────────────────────────────────────────────────────────────
    [Fact] public void Real_StandardFloat()
    {
        var t = First("3.14");
        Assert.Equal(TokenType.REAL, t.Type);
        Assert.Equal("3.14", t.Lexeme);
    }

    [Fact] public void Real_ZeroPoint()
    {
        var t = First("0.5");
        Assert.Equal(TokenType.REAL, t.Type);
        Assert.Equal("0.5", t.Lexeme);
    }

    [Fact] public void Real_MultipleDecimalDigits()
    {
        var t = First("100.001");
        Assert.Equal(TokenType.REAL, t.Type);
        Assert.Equal("100.001", t.Lexeme);
    }

    [Fact] public void Real_NotMatchedWhenNoDot()
    {
        // "3" alone must be INTEGER, not REAL
        var t = First("3");
        Assert.Equal(TokenType.INTEGER, t.Type);
    }

    // ── REL_OP ──────────────────────────────────────────────────────────────
    [Theory]
    [InlineData("<=", "<=")]
    [InlineData(">=", ">=")]
    [InlineData("==", "==")]
    [InlineData("!=", "!=")]
    [InlineData("<",  "<")]
    [InlineData(">",  ">")]
    [InlineData("=",  "=")]
    public void RelOp_AllOperators(string src, string expected)
    {
        var t = First(src);
        Assert.Equal(TokenType.REL_OP, t.Type);
        Assert.Equal(expected, t.Lexeme);
    }

    // ── STRING ──────────────────────────────────────────────────────────────
    [Fact] public void String_SimpleWord()
    {
        var t = First("\"hello\"");
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("\"hello\"", t.Lexeme);
    }

    [Fact] public void String_WithSpaces()
    {
        var t = First("\"hello world\"");
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("\"hello world\"", t.Lexeme);
    }

    [Fact] public void String_Empty()
    {
        var t = First("\"\"");
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("\"\"", t.Lexeme);
    }

    // ── LINE_COMMENT ────────────────────────────────────────────────────────
    [Fact] public void LineComment_WithText()
    {
        var t = First("// este es un comentario");
        Assert.Equal(TokenType.LINE_COMMENT, t.Type);
        Assert.Equal("// este es un comentario", t.Lexeme);
    }

    [Fact] public void LineComment_Empty()
    {
        var t = First("//");
        Assert.Equal(TokenType.LINE_COMMENT, t.Type);
        Assert.Equal("//", t.Lexeme);
    }

    [Fact] public void LineComment_StopsAtNewline()
    {
        var tokens = All("// comentario\nint x");
        Assert.Equal(TokenType.LINE_COMMENT, tokens[0].Type);
        Assert.Equal("// comentario", tokens[0].Lexeme);
        Assert.Equal(TokenType.KEYWORD, tokens[1].Type);
    }

    // ── KEYWORD ─────────────────────────────────────────────────────────────
    [Theory]
    [InlineData("if")]
    [InlineData("while")]
    [InlineData("return")]
    [InlineData("class")]
    [InlineData("void")]
    public void Keyword_RecognizedCorrectly(string kw)
    {
        var t = First(kw);
        Assert.Equal(TokenType.KEYWORD, t.Type);
        Assert.Equal(kw, t.Lexeme);
    }

    [Fact] public void Keyword_IdentifierNotMistaken()
    {
        // "iff" is not a keyword
        var t = First("iff");
        Assert.Equal(TokenType.IDENTIFIER, t.Type);
    }

    // ── UNKNOWN / ERRORES ────────────────────────────────────────────────────
    [Fact] public void Unknown_Semicolon()
    {
        var t = First(";");
        Assert.Equal(TokenType.UNKNOWN, t.Type);
        Assert.Equal(";", t.Lexeme);
    }

    [Fact] public void Unknown_AtSign()
    {
        var t = First("@");
        Assert.Equal(TokenType.UNKNOWN, t.Type);
    }

    [Fact] public void Unknown_Hash()
    {
        var t = First("#");
        Assert.Equal(TokenType.UNKNOWN, t.Type);
    }

    // ── COMBINACIONES ────────────────────────────────────────────────────────
    [Fact] public void Combined_IntDeclaration()
    {
        var tokens = All("int x = 3.14;");
        Assert.Equal(TokenType.KEYWORD,    tokens[0].Type); // int
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type); // x
        Assert.Equal(TokenType.REL_OP,     tokens[2].Type); // =
        Assert.Equal(TokenType.REAL,       tokens[3].Type); // 3.14
        Assert.Equal(TokenType.UNKNOWN,    tokens[4].Type); // ;
    }

    [Fact] public void Combined_IfStatement()
    {
        var tokens = All("if (x != 0)");
        Assert.Equal(TokenType.KEYWORD,    tokens[0].Type); // if
        Assert.Equal(TokenType.UNKNOWN,    tokens[1].Type); // (
        Assert.Equal(TokenType.IDENTIFIER, tokens[2].Type); // x
        Assert.Equal(TokenType.REL_OP,     tokens[3].Type); // !=
        Assert.Equal(TokenType.INTEGER,    tokens[4].Type); // 0
        Assert.Equal(TokenType.UNKNOWN,    tokens[5].Type); // )
    }

    [Fact] public void Combined_LineAndColumn()
    {
        var tokens = All("int\nx");
        Assert.Equal(1, tokens[0].Line); Assert.Equal(1, tokens[0].Col);
        Assert.Equal(2, tokens[1].Line); Assert.Equal(1, tokens[1].Col);
    }

    // ── VALIDACION CRUZADA ───────────────────────────────────────────────────
    [Fact] public void CrossValidation_IdentifierMatchesRegex()
    {
        var v = Validations("precio").First(x => x.Token.Type == TokenType.IDENTIFIER);
        Assert.True(v.IsMatch);
    }

    [Fact] public void CrossValidation_RealMatchesRegex()
    {
        var v = Validations("3.14").First(x => x.Token.Type == TokenType.REAL);
        Assert.True(v.IsMatch);
    }

    [Fact] public void CrossValidation_KeywordMatchesRegex()
    {
        var v = Validations("if").First(x => x.Token.Type == TokenType.KEYWORD);
        Assert.True(v.IsMatch);
    }

    [Fact] public void CrossValidation_NoValidationForUnknown()
    {
        var (tokens, validations) = new Lexer(";").Tokenize();
        Assert.Equal(TokenType.UNKNOWN, tokens[0].Type);
        Assert.Empty(validations); // UNKNOWN no entra en validacion cruzada
    }

    // ── AUTOMATA UNITARIOS ───────────────────────────────────────────────────
    [Fact] public void Automata_IdentifierDoesNotMatchDigitStart()
    {
        var (ok, _) = new IdentifierAutomata().Run("123abc", 0);
        Assert.False(ok);
    }

    [Fact] public void Automata_RealRequiresDigitAfterDot()
    {
        var (ok, _) = new RealAutomata().Run("3.", 0);
        Assert.False(ok); // "3." no es REAL valido
    }

    [Fact] public void Automata_RelOpMaximalMunch()
    {
        // "<=" debe ganar sobre "<" solo
        var (ok, lex) = new RelOpAutomata().Run("<=", 0);
        Assert.True(ok);
        Assert.Equal("<=", lex);
    }
}
