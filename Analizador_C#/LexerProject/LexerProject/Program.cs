using LexerProject;

// ── Tablas de transicion al arrancar ─────────────────────────────────────────
AutomataDisplay.ShowAllTables();

// ── Menu principal ────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║   ANALIZADOR LEXICO AFD - .NET 8     ║");
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
        RunAnalysis("int precio = 3.14; // descuento\nif (precio >= 10) { return precio; }");
        break;
}

// ── Funcion de analisis y reporte ─────────────────────────────────────────────
static void RunAnalysis(string source)
{
    Console.WriteLine();
    Console.WriteLine($"Entrada: \"{source.Replace("\n", "\\n")}\"");
    Console.WriteLine();

    var lexer = new Lexer(source);
    var (tokens, validations) = lexer.Tokenize();

    // Tabla de tokens
    Console.WriteLine($"{"Pos",-5} {"Token",-16} {"Lexema",-22} {"Linea",-7} {"Col",-5}");
    Console.WriteLine(new string('-', 60));
    foreach (var t in tokens)
    {
        string error = t.Type == TokenType.UNKNOWN ? "  <-- ERROR LEXICO" : "";
        string lexDisplay = "\"" + t.Lexeme + "\"";
        Console.WriteLine($"{t.Pos,-5} {t.Type,-16} {lexDisplay,-22} {t.Line,-7} {t.Col,-5}{error}");
    }

    // Validacion cruzada con Regex
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
