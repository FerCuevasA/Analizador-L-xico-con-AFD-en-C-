namespace LexerProject;

public record SymbolEntry(int Index, string Lexeme, string Kind, int Line);

public class SymbolTable
{
    private readonly List<SymbolEntry> _entries = [];

    public IReadOnlyList<SymbolEntry> Entries => _entries;

    public void TryInsert(Token token)
    {
        if (token.Type is not (TokenType.IDENTIFIER or TokenType.INTEGER or TokenType.REAL))
            return;
        if (_entries.Any(e => e.Lexeme == token.Lexeme))
            return;

        string kind = token.Type switch
        {
            TokenType.IDENTIFIER => "ID",
            TokenType.INTEGER    => "INTEGER",
            TokenType.REAL       => "FLOAT",
            _                    => "?"
        };
        _entries.Add(new SymbolEntry(_entries.Count, token.Lexeme, kind, token.Line));
    }
}
