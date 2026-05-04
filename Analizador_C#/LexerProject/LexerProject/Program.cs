using LexerProject;

AutomataDisplay.ShowAllTables();

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║   ANALIZADOR LEXICO AFD - .NET 10    ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine("  1. Analizar texto de ejemplo");
Console.WriteLine("  2. Ingresar texto personalizado");
Console.WriteLine("  3. Salir");
Console.Write("Opcion: ");

switch (Console.ReadLine()?.Trim())
{
    case "1":
        RunAnalysis("int x = 3.14; // valor\nif (x != 0) { return x; }");
        break;
    case "2":
        Console.WriteLine("Ingrese el codigo (linea vacia para terminar):");
        var sb = new System.Text.StringBuilder();
        string? ln;
        while (!string.IsNullOrEmpty(ln = Console.ReadLine()))
            sb.AppendLine(ln);
        if (sb.Length > 0) RunAnalysis(sb.ToString());
        break;
    case "3":
        Console.WriteLine("Hasta luego.");
        break;
    default:
        Console.WriteLine("Opcion no valida. Ejecutando ejemplo por defecto...");
        RunAnalysis("int precio = 3.14; if (precio >= 0.0) // ok");
        break;
}

static void RunAnalysis(string source)
{
    Console.WriteLine();
    Console.WriteLine($"Entrada: \"{source.Replace("\n", "\\n")}\"");
    Console.WriteLine();

    var lexer = new Lexer(source);
    var (tokens, validations) = lexer.Tokenize();

    // ── Tabla de tokens ──────────────────────────────────────────────────────
    string hdr = $"{"ID",-6} {"Token",-16} {"Lexema",-25} {"Linea",-7} {"Col",-5}";
    Console.WriteLine(new string('=', hdr.Length));
    Console.WriteLine(hdr);
    Console.WriteLine(new string('-', hdr.Length));
    foreach (var t in tokens)
    {
        string error = t.Type == TokenType.UNKNOWN ? "  <-- ERROR LEXICO" : "";
        string idStr  = t.Type != TokenType.UNKNOWN ? t.Id.ToString() : "?";
        Console.WriteLine($"{idStr,-6} {t.Type,-16} {("\"" + t.Lexeme + "\""  ),-25} {t.Line,-7} {t.Col,-5}{error}");
    }
    Console.WriteLine(new string('=', hdr.Length));

    // ── Tabla de simbolos ────────────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine(new string('=', 55));
    Console.WriteLine($"{"Indx",-6} {"Lexema",-25} {"Tipo",-10} {"Linea"}");
    Console.WriteLine(new string('-', 55));
    foreach (var e in lexer.SymbolTable.Entries)
        Console.WriteLine($"{e.Index,-6} {e.Lexeme,-25} {e.Kind,-10} {e.Line}");
    Console.WriteLine(new string('=', 55));
    Console.WriteLine($"  Total de entradas: {lexer.SymbolTable.Entries.Count}");

    // ── Validacion cruzada con Regex ─────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine("=== VALIDACION CRUZADA CON REGEX ===");
    foreach (var v in validations)
    {
        string ok = v.IsMatch ? "[OK]" : "[FAIL]";
        Console.WriteLine($"Token: {v.Token.Type,-16} Lexema: \"{v.Token.Lexeme}\"");
        Console.WriteLine($"  Regex:  {v.Pattern}");
        Console.WriteLine($"  Regex.IsMatch --> {v.IsMatch} {ok}");
    }
}
