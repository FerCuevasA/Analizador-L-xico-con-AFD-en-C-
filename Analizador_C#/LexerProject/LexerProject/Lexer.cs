using System.Text.RegularExpressions;

namespace LexerProject;

public record ValidationResult(Token Token, string Pattern, bool IsMatch);

public class Lexer
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "if", "else", "while", "for", "int",
        "float", "return", "void", "true", "false"
    };

    // Regex SOLO para validacion cruzada (no para reconocer)
    private static readonly Dictionary<TokenType, string> RegexPatterns = new()
    {
        [TokenType.IDENTIFIER]  = @"^[a-zA-Z_][a-zA-Z0-9_]*$",
        [TokenType.INTEGER]     = @"^[0-9]+$",
        [TokenType.REAL]        = @"^[0-9]+\.[0-9]+$",
        [TokenType.REL_OP]      = @"^(<=|>=|==|!=|<|>)$",
        [TokenType.ASSIGN_OP]   = @"^(=|\+=|-=|\*=|/=)$",
        [TokenType.ARITH_OP]    = @"^[+\-*/]$",
        [TokenType.SPECIAL_SYM] = @"^[(){},;.]$",
        [TokenType.STRING]      = "^\"[^\"\n]*\"$",
        [TokenType.LINE_COMMENT]= @"^//[^\n]*$",
        [TokenType.KEYWORD]     = @"^(if|else|while|for|int|float|return|void|true|false)$",
    };

    private readonly string _source;

    // Orden critico: maximal-munch requiere probar compuestos antes que simples
    private readonly List<Automata> _automata =
    [
        new RealAutomata(),           // float antes que int
        new IntegerAutomata(),
        new IdentifierAutomata(),     // keywords se detectan post-emision
        new LineCommentAutomata(),    // // antes que /
        new CompoundAssignAutomata(), // += -= *= /= antes que + - * / y =
        new RelOpAutomata(),          // == != <= >= < > (sin = simple)
        new AssignAutomata(),         // = simple
        new ArithOpAutomata(),        // + - * /
        new SpecialSymAutomata(),     // ( ) { } , ; .
        new StringAutomata(),
        new WhitespaceAutomata(),
    ];

    public SymbolTable SymbolTable { get; } = new();

    public Lexer(string source) => _source = source.Replace("\r\n", "\n").Replace("\r", "\n");

    public (List<Token> tokens, List<ValidationResult> validations) Tokenize()
    {
        var tokens      = new List<Token>();
        int pos = 0, line = 1, col = 1;

        while (pos < _source.Length)
        {
            bool matched = false;

            foreach (var automata in _automata)
            {
                var (ok, lexeme) = automata.Run(_source, pos);
                if (!ok || lexeme.Length == 0) continue;

                if (automata.TokenType == TokenType.WHITESPACE)
                {
                    (pos, line, col) = Advance(pos, line, col, lexeme);
                    matched = true;
                    break;
                }

                var type = automata.TokenType;
                if (type == TokenType.IDENTIFIER && Keywords.Contains(lexeme))
                    type = TokenType.KEYWORD;

                var token = new Token(type, lexeme, line, col, pos);
                tokens.Add(token);
                SymbolTable.TryInsert(token);
                (pos, line, col) = Advance(pos, line, col, lexeme);
                matched = true;
                break;
            }

            if (!matched)
            {
                char ch = _source[pos];
                tokens.Add(new Token(TokenType.UNKNOWN, ch.ToString(), line, col, pos));
                if (ch == '\n') { line++; col = 1; } else col++;
                pos++;
            }
        }

        var validations = tokens
            .Where(t => t.Type != TokenType.UNKNOWN)
            .Select(CrossValidate)
            .ToList();

        return (tokens, validations);
    }

    private static ValidationResult CrossValidate(Token token)
    {
        if (RegexPatterns.TryGetValue(token.Type, out string? pattern))
            return new ValidationResult(token, pattern, Regex.IsMatch(token.Lexeme, pattern));
        return new ValidationResult(token, "N/A", false);
    }

    private static (int pos, int line, int col) Advance(int pos, int line, int col, string lexeme)
    {
        foreach (char c in lexeme)
        {
            pos++;
            if (c == '\n') { line++; col = 1; }
            else col++;
        }
        return (pos, line, col);
    }
}
