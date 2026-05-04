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

    private static SymbolTable Symbols(string src)
    {
        var lexer = new Lexer(src);
        lexer.Tokenize();
        return lexer.SymbolTable;
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
    public void RelOp_AllOperators(string src, string expected)
    {
        var t = First(src);
        Assert.Equal(TokenType.REL_OP, t.Type);
        Assert.Equal(expected, t.Lexeme);
    }

    // ── ASSIGN_OP ───────────────────────────────────────────────────────────
    [Theory]
    [InlineData("=",  "=")]
    [InlineData("+=", "+=")]
    [InlineData("-=", "-=")]
    [InlineData("*=", "*=")]
    [InlineData("/=", "/=")]
    public void AssignOp_AllOperators(string src, string expected)
    {
        var t = First(src);
        Assert.Equal(TokenType.ASSIGN_OP, t.Type);
        Assert.Equal(expected, t.Lexeme);
    }

    [Fact] public void AssignOp_NotConfusedWithEqEq()
    {
        // == debe ser REL_OP, no ASSIGN_OP
        var t = First("==");
        Assert.Equal(TokenType.REL_OP, t.Type);
        Assert.Equal("==", t.Lexeme);
    }

    // ── ARITH_OP ────────────────────────────────────────────────────────────
    [Theory]
    [InlineData("+", "+")]
    [InlineData("-", "-")]
    [InlineData("*", "*")]
    [InlineData("/", "/")]
    public void ArithOp_AllOperators(string src, string expected)
    {
        var t = First(src);
        Assert.Equal(TokenType.ARITH_OP, t.Type);
        Assert.Equal(expected, t.Lexeme);
    }

    [Fact] public void ArithOp_PlusNotConfusedWithPlusAssign()
    {
        // += debe ser ASSIGN_OP, no ARITH_OP
        var t = First("+=");
        Assert.Equal(TokenType.ASSIGN_OP, t.Type);
        Assert.Equal("+=", t.Lexeme);
    }

    [Fact] public void ArithOp_SlashNotConfusedWithComment()
    {
        // // debe ser LINE_COMMENT, no ARITH_OP
        var t = First("// comentario");
        Assert.Equal(TokenType.LINE_COMMENT, t.Type);
    }

    // ── SPECIAL_SYM ─────────────────────────────────────────────────────────
    [Theory]
    [InlineData("(", "(")]
    [InlineData(")", ")")]
    [InlineData("{", "{")]
    [InlineData("}", "}")]
    [InlineData(",", ",")]
    [InlineData(";", ";")]
    [InlineData(".", ".")]
    public void SpecialSym_AllSymbols(string src, string expected)
    {
        var t = First(src);
        Assert.Equal(TokenType.SPECIAL_SYM, t.Type);
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
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("void")]
    public void Keyword_RecognizedCorrectly(string kw)
    {
        var t = First(kw);
        Assert.Equal(TokenType.KEYWORD, t.Type);
        Assert.Equal(kw, t.Lexeme);
    }

    [Fact] public void Keyword_IdentifierNotMistaken()
    {
        var t = First("iff");
        Assert.Equal(TokenType.IDENTIFIER, t.Type);
    }

    // ── TOKEN IDS ────────────────────────────────────────────────────────────
    [Fact] public void TokenId_Keyword_Int()   => Assert.Equal(104, First("int").Id);
    [Fact] public void TokenId_Keyword_If()    => Assert.Equal(100, First("if").Id);
    [Fact] public void TokenId_Keyword_True()  => Assert.Equal(108, First("true").Id);
    [Fact] public void TokenId_Keyword_False() => Assert.Equal(109, First("false").Id);
    [Fact] public void TokenId_Identifier()    => Assert.Equal(200, First("x").Id);
    [Fact] public void TokenId_Integer()       => Assert.Equal(201, First("42").Id);
    [Fact] public void TokenId_Real()          => Assert.Equal(202, First("3.14").Id);
    [Fact] public void TokenId_Plus()          => Assert.Equal(300, First("+").Id);
    [Fact] public void TokenId_Assign()        => Assert.Equal(320, First("=").Id);
    [Fact] public void TokenId_PlusAssign()    => Assert.Equal(321, First("+=").Id);
    [Fact] public void TokenId_EqEq()          => Assert.Equal(310, First("==").Id);
    [Fact] public void TokenId_GEQ()           => Assert.Equal(315, First(">=").Id);
    [Fact] public void TokenId_LParen()        => Assert.Equal(400, First("(").Id);
    [Fact] public void TokenId_Semi()          => Assert.Equal(405, First(";").Id);

    // ── TABLA DE SIMBOLOS ────────────────────────────────────────────────────
    [Fact] public void SymbolTable_InsertsIdentifier()
    {
        var st = Symbols("int precio");
        Assert.Contains(st.Entries, e => e.Lexeme == "precio" && e.Kind == "ID");
    }

    [Fact] public void SymbolTable_InsertsFloat()
    {
        var st = Symbols("3.14");
        Assert.Contains(st.Entries, e => e.Lexeme == "3.14" && e.Kind == "FLOAT");
    }

    [Fact] public void SymbolTable_InsertsInteger()
    {
        var st = Symbols("42");
        Assert.Contains(st.Entries, e => e.Lexeme == "42" && e.Kind == "INTEGER");
    }

    [Fact] public void SymbolTable_NoDuplicates()
    {
        var st = Symbols("x + x + x");
        Assert.Single(st.Entries.Where(e => e.Lexeme == "x"));
    }

    [Fact] public void SymbolTable_NoKeywords()
    {
        var st = Symbols("int if while");
        Assert.DoesNotContain(st.Entries, e => e.Lexeme == "int");
        Assert.DoesNotContain(st.Entries, e => e.Lexeme == "if");
    }

    [Fact] public void SymbolTable_NoOperators()
    {
        var st = Symbols("x += 1");
        Assert.DoesNotContain(st.Entries, e => e.Lexeme == "+=");
    }

    // ── UNKNOWN / ERRORES ────────────────────────────────────────────────────
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

    [Fact] public void Unknown_Dollar()
    {
        var t = First("$");
        Assert.Equal(TokenType.UNKNOWN, t.Type);
    }

    // ── COMBINACIONES ────────────────────────────────────────────────────────
    [Fact] public void Combined_IntDeclaration()
    {
        var tokens = All("int x = 3.14;");
        Assert.Equal(TokenType.KEYWORD,    tokens[0].Type); // int
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type); // x
        Assert.Equal(TokenType.ASSIGN_OP,  tokens[2].Type); // =  (no REL_OP)
        Assert.Equal(TokenType.REAL,       tokens[3].Type); // 3.14
        Assert.Equal(TokenType.SPECIAL_SYM,tokens[4].Type); // ;  (no UNKNOWN)
    }

    [Fact] public void Combined_IfStatement()
    {
        var tokens = All("if (x != 0)");
        Assert.Equal(TokenType.KEYWORD,    tokens[0].Type); // if
        Assert.Equal(TokenType.SPECIAL_SYM,tokens[1].Type); // (
        Assert.Equal(TokenType.IDENTIFIER, tokens[2].Type); // x
        Assert.Equal(TokenType.REL_OP,     tokens[3].Type); // !=
        Assert.Equal(TokenType.INTEGER,    tokens[4].Type); // 0
        Assert.Equal(TokenType.SPECIAL_SYM,tokens[5].Type); // )
    }

    [Fact] public void Combined_LineAndColumn()
    {
        var tokens = All("int\nx");
        Assert.Equal(1, tokens[0].Line); Assert.Equal(1, tokens[0].Col);
        Assert.Equal(2, tokens[1].Line); Assert.Equal(1, tokens[1].Col);
    }

    [Fact] public void Combined_CompoundAssign()
    {
        var tokens = All("x += 1");
        Assert.Equal(TokenType.IDENTIFIER, tokens[0].Type); // x
        Assert.Equal(TokenType.ASSIGN_OP,  tokens[1].Type); // +=
        Assert.Equal("+=", tokens[1].Lexeme);
        Assert.Equal(TokenType.INTEGER,    tokens[2].Type); // 1
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

    [Fact] public void CrossValidation_AssignOpMatchesRegex()
    {
        var v = Validations("+=").First(x => x.Token.Type == TokenType.ASSIGN_OP);
        Assert.True(v.IsMatch);
    }

    [Fact] public void CrossValidation_ArithOpMatchesRegex()
    {
        var v = Validations("+").First(x => x.Token.Type == TokenType.ARITH_OP);
        Assert.True(v.IsMatch);
    }

    [Fact] public void CrossValidation_SpecialSymMatchesRegex()
    {
        var v = Validations(";").First(x => x.Token.Type == TokenType.SPECIAL_SYM);
        Assert.True(v.IsMatch);
    }

    [Fact] public void CrossValidation_NoValidationForUnknown()
    {
        var (tokens, validations) = new Lexer("$").Tokenize();
        Assert.Equal(TokenType.UNKNOWN, tokens[0].Type);
        Assert.Empty(validations);
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
        Assert.False(ok);
    }

    [Fact] public void Automata_RelOpMaximalMunch()
    {
        var (ok, lex) = new RelOpAutomata().Run("<=", 0);
        Assert.True(ok);
        Assert.Equal("<=", lex);
    }

    [Fact] public void Automata_RelOpDoesNotAcceptBareAssign()
    {
        // = solo NO debe ser aceptado por RelOpAutomata
        var (ok, _) = new RelOpAutomata().Run("= ", 0);
        Assert.False(ok);
    }

    [Fact] public void Automata_CompoundAssignMaximalMunch()
    {
        var (ok, lex) = new CompoundAssignAutomata().Run("+=", 0);
        Assert.True(ok);
        Assert.Equal("+=", lex);
    }
}
