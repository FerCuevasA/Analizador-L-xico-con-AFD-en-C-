using LexerProject;
using Xunit;

namespace LexerProject.Tests;

// ── helpers compartidos ───────────────────────────────────────────────────────
file static class H
{
    public static Token              First(string src) => new Lexer(src).Tokenize().tokens[0];
    public static List<Token>        All  (string src) => new Lexer(src).Tokenize().tokens;
    public static List<ValidationResult> Val(string src) => new Lexer(src).Tokenize().validations;
}

// =============================================================================
// KEYWORDS
// =============================================================================
public class KeywordTests
{
    // Las 5 que no cubre la Theory existente: else, for, int, float, string
    [Theory]
    [InlineData("else")]
    [InlineData("for")]
    [InlineData("int")]
    [InlineData("float")]
    [InlineData("string")]
    public void MissingKeywords_AreRecognized(string kw)
    {
        var t = H.First(kw);
        Assert.Equal(TokenType.KEYWORD, t.Type);
        Assert.Equal(kw, t.Lexeme);
    }

    [Fact]
    public void All10Keywords_AreRecognized()
    {
        string[] all = ["if", "else", "while", "for", "int",
                        "float", "string", "return", "void", "class"];
        foreach (var kw in all)
        {
            var t = H.First(kw);
            Assert.Equal(TokenType.KEYWORD, t.Type);
            Assert.Equal(kw, t.Lexeme);
        }
    }

    // Un prefijo de keyword NO debe ser reconocido como keyword
    [Theory]
    [InlineData("iff")]
    [InlineData("returning")]
    [InlineData("whileloop")]
    [InlineData("forloop")]
    [InlineData("classes")]
    [InlineData("voidx")]
    public void Identifier_WithKeywordPrefix_IsIdentifier(string src)
    {
        var t = H.First(src);
        Assert.Equal(TokenType.IDENTIFIER, t.Type);
    }

    [Fact]
    public void Keywords_InSequence_AllRecognized()
    {
        var tokens = H.All("if else while for");
        Assert.Equal(4, tokens.Count);
        Assert.All(tokens, t => Assert.Equal(TokenType.KEYWORD, t.Type));
    }
}

// =============================================================================
// INTEGER — casos borde
// =============================================================================
public class IntegerEdgeCaseTests
{
    [Fact]
    public void Integer_LeadingZeros_IsInteger()
    {
        var t = H.First("007");
        Assert.Equal(TokenType.INTEGER, t.Type);
        Assert.Equal("007", t.Lexeme);
    }

    [Fact]
    public void Integer_DoubleZero_IsInteger()
    {
        var t = H.First("00");
        Assert.Equal(TokenType.INTEGER, t.Type);
        Assert.Equal("00", t.Lexeme);
    }

    [Fact]
    public void Integer_FollowedByLetter_OnlyDigitsCaptured()
    {
        var tokens = H.All("42abc");
        Assert.Equal(TokenType.INTEGER,    tokens[0].Type);
        Assert.Equal("42",                 tokens[0].Lexeme);
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type);
        Assert.Equal("abc",                tokens[1].Lexeme);
    }

    [Fact]
    public void Integer_FollowedByOperator_Separated()
    {
        var tokens = H.All("10+3");
        Assert.Equal(TokenType.INTEGER, tokens[0].Type); // 10
        Assert.Equal(TokenType.REL_OP,  tokens[1].Type); // +  (no hay aritmético distinto en C#)
        Assert.Equal(TokenType.INTEGER, tokens[2].Type); // 3
    }
}

// =============================================================================
// REAL — casos borde
// =============================================================================
public class RealEdgeCaseTests
{
    [Fact]
    public void Real_DotWithNoLeadingDigit_DotIsUnknown()
    {
        var tokens = H.All(".5");
        Assert.Equal(TokenType.UNKNOWN, tokens[0].Type);  // "." no inicia REAL
        Assert.Equal(TokenType.INTEGER, tokens[1].Type);  // "5" es INTEGER
    }

    [Fact]
    public void Real_DotWithNoTrailingDigit_CapturesIntegerOnly()
    {
        // RealAutomata requiere dígito después del punto; "3." se tokeniza como INTEGER + UNKNOWN
        var tokens = H.All("3.");
        Assert.Equal(TokenType.INTEGER, tokens[0].Type);
        Assert.Equal("3",               tokens[0].Lexeme);
        Assert.Equal(TokenType.UNKNOWN, tokens[1].Type);
        Assert.Equal(".",               tokens[1].Lexeme);
    }

    [Fact]
    public void Real_ZeroPointZero_IsReal()
    {
        var t = H.First("0.0");
        Assert.Equal(TokenType.REAL, t.Type);
        Assert.Equal("0.0", t.Lexeme);
    }

    [Fact]
    public void Real_MaximalMunch_BeatsInteger()
    {
        // "3.14" debe ser un solo REAL, no INTEGER(3) + UNKNOWN(.) + INTEGER(14)
        var tokens = H.All("3.14");
        Assert.Single(tokens);
        Assert.Equal(TokenType.REAL, tokens[0].Type);
        Assert.Equal("3.14",         tokens[0].Lexeme);
    }
}

// =============================================================================
// STRING — casos borde
// =============================================================================
public class StringEdgeCaseTests
{
    [Fact]
    public void String_WithNumbers_IsString()
    {
        var t = H.First("\"abc123\"");
        Assert.Equal(TokenType.STRING, t.Type);
        Assert.Equal("\"abc123\"", t.Lexeme);
    }

    [Fact]
    public void String_WithSpecialChars_IsString()
    {
        // Los caracteres especiales dentro de "" son válidos (no son \n)
        var t = H.First("\"hello!@#$%\"");
        Assert.Equal(TokenType.STRING, t.Type);
    }

    [Fact]
    public void String_UnclosedBeforeNewline_QuoteBecomesUnknown()
    {
        // StringAutomata no cruza \n; la " de apertura queda sin emparejar → UNKNOWN
        var tokens = H.All("\"hello\n");
        Assert.Equal(TokenType.UNKNOWN,    tokens[0].Type);
        Assert.Equal("\"",                 tokens[0].Lexeme);
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type); // "hello"
    }

    [Fact]
    public void String_AdjacentToOtherTokens_AllRecognized()
    {
        var tokens = H.All("x=\"hola\"");
        Assert.Equal(TokenType.IDENTIFIER, tokens[0].Type); // x
        Assert.Equal(TokenType.REL_OP,     tokens[1].Type); // =
        Assert.Equal(TokenType.STRING,     tokens[2].Type); // "hola"
    }
}

// =============================================================================
// ENTRADAS LÍMITE
// =============================================================================
public class EdgeInputTests
{
    [Fact]
    public void EmptySource_ProducesNoTokens()
    {
        var (tokens, _) = new Lexer("").Tokenize();
        Assert.Empty(tokens);
    }

    [Fact]
    public void WhitespaceOnly_ProducesNoTokens()
    {
        var (tokens, _) = new Lexer("   \t\t  \n  \r\n  ").Tokenize();
        Assert.Empty(tokens);
    }

    [Fact]
    public void MultipleUnknowns_EachBecomesOwnToken()
    {
        var (tokens, _) = new Lexer("@#;").Tokenize();
        Assert.Equal(3, tokens.Count);
        Assert.All(tokens, t => Assert.Equal(TokenType.UNKNOWN, t.Type));
    }

    [Fact]
    public void UnknownsSurroundingValidToken_AllThreePresent()
    {
        // "@int@" → UNKNOWN, KEYWORD, UNKNOWN
        var tokens = H.All("@int@");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.UNKNOWN, tokens[0].Type);
        Assert.Equal(TokenType.KEYWORD, tokens[1].Type);
        Assert.Equal(TokenType.UNKNOWN, tokens[2].Type);
    }

    [Fact]
    public void SourceWithOnlyComment_ProducesOneToken()
    {
        var tokens = H.All("// solo un comentario");
        Assert.Single(tokens);
        Assert.Equal(TokenType.LINE_COMMENT, tokens[0].Type);
    }

    [Fact]
    public void NewlineNormalization_CRLF_TreatedSameAsLF()
    {
        var tokensLF   = H.All("int\nx");
        var tokensCRLF = H.All("int\r\nx");
        Assert.Equal(tokensLF.Count, tokensCRLF.Count);
        Assert.Equal(tokensLF[1].Line, tokensCRLF[1].Line);
    }
}

// =============================================================================
// SEGUIMIENTO DE POSICIÓN (Pos, Line, Col)
// =============================================================================
public class PositionTrackingTests
{
    [Fact]
    public void Token_Pos_ReflectsOffsetInSource()
    {
        // "int x" → 'int' empieza en 0, 'x' empieza en 4
        var tokens = H.All("int x");
        Assert.Equal(0, tokens[0].Pos);
        Assert.Equal(4, tokens[1].Pos);
    }

    [Fact]
    public void Token_Line_StartsAt1()
    {
        var t = H.First("x");
        Assert.Equal(1, t.Line);
    }

    [Fact]
    public void Token_Col_StartsAt1()
    {
        var t = H.First("x");
        Assert.Equal(1, t.Col);
    }

    [Fact]
    public void Token_Line_IncrementsPerNewline()
    {
        var tokens = H.All("a\nb\nc");
        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(2, tokens[1].Line);
        Assert.Equal(3, tokens[2].Line);
    }

    [Fact]
    public void Token_Col_ResetsAfterNewline()
    {
        var tokens = H.All("ab\ncd");
        // 'ab' col 1, 'cd' col 1 en línea 2
        Assert.Equal(1, tokens[0].Col);
        Assert.Equal(1, tokens[1].Col);
    }

    [Fact]
    public void Token_Col_TracksWithinLine()
    {
        // "int x" → 'int' col 1 (len 3), espacio, 'x' col 5
        var tokens = H.All("int x");
        Assert.Equal(1, tokens[0].Col);
        Assert.Equal(5, tokens[1].Col);
    }

    [Fact]
    public void Token_Col_AfterMulticharToken()
    {
        // "if else" → 'if' col 1, 'else' col 4
        var tokens = H.All("if else");
        Assert.Equal(1, tokens[0].Col);
        Assert.Equal(4, tokens[1].Col);
    }
}

// =============================================================================
// PROGRAMAS COMPLETOS
// =============================================================================
public class FullProgramTests
{
    [Fact]
    public void Program_FunctionDeclaration_ContainsExpectedTypes()
    {
        string src = "int suma(int a, int b) { return a; }";
        var tokens = H.All(src);
        var types = tokens.Select(t => t.Type).ToHashSet();
        Assert.Contains(TokenType.KEYWORD,    types);
        Assert.Contains(TokenType.IDENTIFIER, types);
        Assert.Contains(TokenType.UNKNOWN,    types); // parens, braces, comma, semicolon
    }

    [Fact]
    public void Program_IfElseBlock_AllKeywordsFound()
    {
        string src = "if (x == 0) { return 1; } else { return 0; }";
        var tokens = H.All(src);
        Assert.Contains(tokens, t => t.Type == TokenType.KEYWORD    && t.Lexeme == "if");
        Assert.Contains(tokens, t => t.Type == TokenType.KEYWORD    && t.Lexeme == "else");
        Assert.Contains(tokens, t => t.Type == TokenType.KEYWORD    && t.Lexeme == "return");
        Assert.Contains(tokens, t => t.Type == TokenType.REL_OP     && t.Lexeme == "==");
        Assert.Contains(tokens, t => t.Type == TokenType.IDENTIFIER && t.Lexeme == "x");
        Assert.Contains(tokens, t => t.Type == TokenType.INTEGER    && t.Lexeme == "0");
    }

    [Fact]
    public void Program_CommentDoesNotAffectNextLineTokens()
    {
        string src = "// ignorar\nint x";
        var tokens = H.All(src);
        Assert.Equal(TokenType.LINE_COMMENT, tokens[0].Type);
        Assert.Equal(TokenType.KEYWORD,      tokens[1].Type);
        Assert.Equal(TokenType.IDENTIFIER,   tokens[2].Type);
        Assert.Equal(2, tokens[1].Line); // 'int' debe estar en línea 2
    }

    [Fact]
    public void Program_AllRelOpsInExpression_CorrectOrder()
    {
        string src = "a < b <= c == d != e >= f > g";
        var tokens = H.All(src);
        var ops = tokens.Where(t => t.Type == TokenType.REL_OP)
                        .Select(t => t.Lexeme)
                        .ToArray();
        Assert.Equal(["<", "<=", "==", "!=", ">=", ">"], ops);
    }

    [Fact]
    public void Program_MixedLine_CorrectSequence()
    {
        // "int x = 3.14; // val" → KEYWORD, ID, REL_OP, REAL, UNKNOWN(;), COMMENT
        string src = "int x = 3.14; // val";
        var tokens = H.All(src);
        Assert.Equal(TokenType.KEYWORD,      tokens[0].Type);
        Assert.Equal(TokenType.IDENTIFIER,   tokens[1].Type);
        Assert.Equal(TokenType.REL_OP,       tokens[2].Type);
        Assert.Equal(TokenType.REAL,         tokens[3].Type);
        Assert.Equal(TokenType.UNKNOWN,      tokens[4].Type);
        Assert.Equal(TokenType.LINE_COMMENT, tokens[5].Type);
    }

    [Fact]
    public void Program_MultilineFunction_LineNumbersCorrect()
    {
        string src = "int f() {\n  return 0;\n}";
        var tokens = H.All(src);
        Assert.Contains(tokens, t => t.Lexeme == "int"    && t.Line == 1);
        Assert.Contains(tokens, t => t.Lexeme == "return" && t.Line == 2);
    }

    [Fact]
    public void Program_WhileLoop_AllTokensPresent()
    {
        string src = "while (i <= 10) { i = i + 1; }";
        var tokens = H.All(src);
        Assert.Contains(tokens, t => t.Type == TokenType.KEYWORD    && t.Lexeme == "while");
        Assert.Contains(tokens, t => t.Type == TokenType.IDENTIFIER && t.Lexeme == "i");
        Assert.Contains(tokens, t => t.Type == TokenType.REL_OP     && t.Lexeme == "<=");
        Assert.Contains(tokens, t => t.Type == TokenType.INTEGER    && t.Lexeme == "10");
        Assert.Contains(tokens, t => t.Type == TokenType.REL_OP     && t.Lexeme == "=");
        Assert.Contains(tokens, t => t.Type == TokenType.INTEGER    && t.Lexeme == "1");
    }
}

// =============================================================================
// AUTÓMATAS — cobertura de los no cubiertos en LexerTests.cs
// =============================================================================
public class AutomataExtendedTests
{
    // ── IntegerAutomata ───────────────────────────────────────────────────────

    [Fact]
    public void IntegerAutomata_ValidNumber_Matches()
    {
        var (ok, lex) = new IntegerAutomata().Run("42", 0);
        Assert.True(ok);
        Assert.Equal("42", lex);
    }

    [Fact]
    public void IntegerAutomata_SingleDigit_Matches()
    {
        var (ok, lex) = new IntegerAutomata().Run("0", 0);
        Assert.True(ok);
        Assert.Equal("0", lex);
    }

    [Fact]
    public void IntegerAutomata_LetterStart_DoesNotMatch()
    {
        var (ok, _) = new IntegerAutomata().Run("abc", 0);
        Assert.False(ok);
    }

    [Fact]
    public void IntegerAutomata_PartialFromOffset_CapturesCorrectSlice()
    {
        var (ok, lex) = new IntegerAutomata().Run("abc42rest", 3);
        Assert.True(ok);
        Assert.Equal("42", lex);
    }

    [Fact]
    public void IntegerAutomata_StopsAtNonDigit()
    {
        var (ok, lex) = new IntegerAutomata().Run("99abc", 0);
        Assert.True(ok);
        Assert.Equal("99", lex); // solo captura dígitos
    }

    // ── StringAutomata ────────────────────────────────────────────────────────

    [Fact]
    public void StringAutomata_ValidString_Matches()
    {
        var (ok, lex) = new StringAutomata().Run("\"hello\"", 0);
        Assert.True(ok);
        Assert.Equal("\"hello\"", lex);
    }

    [Fact]
    public void StringAutomata_EmptyString_Matches()
    {
        var (ok, lex) = new StringAutomata().Run("\"\"", 0);
        Assert.True(ok);
        Assert.Equal("\"\"", lex);
    }

    [Fact]
    public void StringAutomata_UnclosedString_DoesNotMatch()
    {
        var (ok, _) = new StringAutomata().Run("\"hello", 0);
        Assert.False(ok);
    }

    [Fact]
    public void StringAutomata_NewlineInsideString_DoesNotMatch()
    {
        var (ok, _) = new StringAutomata().Run("\"hello\nworld\"", 0);
        Assert.False(ok);
    }

    [Fact]
    public void StringAutomata_NoBoundaryQuote_DoesNotMatch()
    {
        var (ok, _) = new StringAutomata().Run("hello", 0);
        Assert.False(ok);
    }

    // ── LineCommentAutomata ───────────────────────────────────────────────────

    [Fact]
    public void LineCommentAutomata_FullComment_Matches()
    {
        var (ok, lex) = new LineCommentAutomata().Run("// hola mundo", 0);
        Assert.True(ok);
        Assert.Equal("// hola mundo", lex);
    }

    [Fact]
    public void LineCommentAutomata_EmptyComment_Matches()
    {
        var (ok, lex) = new LineCommentAutomata().Run("//", 0);
        Assert.True(ok);
        Assert.Equal("//", lex);
    }

    [Fact]
    public void LineCommentAutomata_SingleSlash_DoesNotMatch()
    {
        var (ok, _) = new LineCommentAutomata().Run("/ not a comment", 0);
        Assert.False(ok);
    }

    [Fact]
    public void LineCommentAutomata_StopsBeforeNewline()
    {
        var (ok, lex) = new LineCommentAutomata().Run("// texto\nsiguiente", 0);
        Assert.True(ok);
        Assert.Equal("// texto", lex); // no incluye el \n ni lo de la siguiente línea
    }

    // ── WhitespaceAutomata ────────────────────────────────────────────────────

    [Fact]
    public void WhitespaceAutomata_Spaces_Matches()
    {
        var (ok, lex) = new WhitespaceAutomata().Run("   ", 0);
        Assert.True(ok);
        Assert.Equal("   ", lex);
    }

    [Fact]
    public void WhitespaceAutomata_MixedSpacesAndTabs_Matches()
    {
        var (ok, lex) = new WhitespaceAutomata().Run("  \t  ", 0);
        Assert.True(ok);
        Assert.Equal("  \t  ", lex);
    }

    [Fact]
    public void WhitespaceAutomata_Newline_Matches()
    {
        var (ok, lex) = new WhitespaceAutomata().Run("\n", 0);
        Assert.True(ok);
        Assert.Equal("\n", lex);
    }

    [Fact]
    public void WhitespaceAutomata_MixedWithNewlines_Matches()
    {
        var (ok, lex) = new WhitespaceAutomata().Run("  \n  \t", 0);
        Assert.True(ok);
        Assert.Equal("  \n  \t", lex);
    }

    [Fact]
    public void WhitespaceAutomata_Letter_DoesNotMatch()
    {
        var (ok, _) = new WhitespaceAutomata().Run("a", 0);
        Assert.False(ok);
    }

    // ── RelOpAutomata — casos borde ───────────────────────────────────────────

    [Fact]
    public void RelOpAutomata_ExclamationAlone_DoesNotMatch()
    {
        // "!" sin "=" no es un operador válido
        var (ok, _) = new RelOpAutomata().Run("!", 0);
        Assert.False(ok);
    }

    [Fact]
    public void RelOpAutomata_MaximalMunch_PrefersLongerOp()
    {
        var (ok, lex) = new RelOpAutomata().Run("<=x", 0);
        Assert.True(ok);
        Assert.Equal("<=", lex); // captura "<=" completo, no solo "<"
    }

    // ── RealAutomata — casos borde ────────────────────────────────────────────

    [Fact]
    public void RealAutomata_DotFirst_DoesNotMatch()
    {
        var (ok, _) = new RealAutomata().Run(".5", 0);
        Assert.False(ok);
    }

    [Fact]
    public void RealAutomata_IntegerWithoutDot_DoesNotProduceReal()
    {
        // "42" no debe ser REAL (sin punto)
        var (ok, _) = new RealAutomata().Run("42", 0);
        Assert.False(ok);
    }

    // ── IdentifierAutomata ────────────────────────────────────────────────────

    [Fact]
    public void IdentifierAutomata_MixedCase_Matches()
    {
        var (ok, lex) = new IdentifierAutomata().Run("myVar_1", 0);
        Assert.True(ok);
        Assert.Equal("myVar_1", lex);
    }

    [Fact]
    public void IdentifierAutomata_StopsAtOperator()
    {
        var (ok, lex) = new IdentifierAutomata().Run("precio=", 0);
        Assert.True(ok);
        Assert.Equal("precio", lex);
    }
}

// =============================================================================
// VALIDACIÓN CRUZADA — tipos no cubiertos en LexerTests.cs
// =============================================================================
public class CrossValidationExtendedTests
{
    [Fact]
    public void CrossValidation_Integer_MatchesRegex()
    {
        var v = H.Val("42").First(x => x.Token.Type == TokenType.INTEGER);
        Assert.True(v.IsMatch);
    }

    [Fact]
    public void CrossValidation_String_MatchesRegex()
    {
        var v = H.Val("\"hola\"").First(x => x.Token.Type == TokenType.STRING);
        Assert.True(v.IsMatch);
    }

    [Fact]
    public void CrossValidation_LineComment_MatchesRegex()
    {
        var v = H.Val("// texto").First(x => x.Token.Type == TokenType.LINE_COMMENT);
        Assert.True(v.IsMatch);
    }

    [Fact]
    public void CrossValidation_AllNonUnknownTokens_HaveIsMatchTrue()
    {
        string src = "int x = 3.14; // comentario";
        foreach (var v in H.Val(src))
            Assert.True(v.IsMatch,
                $"Token {v.Token.Type} '{v.Token.Lexeme}' falló regex '{v.Pattern}'");
    }

    [Fact]
    public void CrossValidation_Count_EqualsNonUnknownTokens()
    {
        string src = "if x == 42";
        var (tokens, validations) = new Lexer(src).Tokenize();
        int nonUnknown = tokens.Count(t => t.Type != TokenType.UNKNOWN);
        Assert.Equal(nonUnknown, validations.Count);
    }

    [Fact]
    public void CrossValidation_UnknownTokens_NeverAppearInValidations()
    {
        string src = "@#;";
        var (tokens, validations) = new Lexer(src).Tokenize();
        Assert.All(tokens, t => Assert.Equal(TokenType.UNKNOWN, t.Type));
        Assert.Empty(validations);
    }

    [Fact]
    public void CrossValidation_MixedProgram_NoFalseValidations()
    {
        string src = "float precio = 9.99; // descuento\nif (precio >= 10.0) { return precio; }";
        var validations = H.Val(src);
        Assert.All(validations, v =>
            Assert.True(v.IsMatch,
                $"Fallo inesperado: {v.Token.Type} '{v.Token.Lexeme}'"));
    }
}
