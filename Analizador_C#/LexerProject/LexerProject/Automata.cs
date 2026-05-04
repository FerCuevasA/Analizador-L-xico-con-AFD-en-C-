namespace LexerProject;

// ── Clasificacion de caracteres para los AFD ─────────────────────────────────
public enum CharClass
{
    Letter, Digit, Underscore, Quote,
    LT, GT, EQ, Bang, Slash, Dot, Newline, Space,
    Plus, Minus, Star,
    LParen, RParen, LBrace, RBrace, Comma, Semi,
    Other
}

// ── Clase base abstracta ──────────────────────────────────────────────────────
public abstract class Automata
{
    public abstract int          InitialState    { get; }
    public abstract int[]        AcceptingStates { get; }
    public abstract Dictionary<(int state, CharClass cls), int> Transitions { get; }
    public abstract TokenType    TokenType       { get; }
    public abstract string       Name            { get; }

    public virtual CharClass Classify(char c) => c switch
    {
        (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') => CharClass.Letter,
        >= '0' and <= '9'                            => CharClass.Digit,
        '_'                                          => CharClass.Underscore,
        '"'                                          => CharClass.Quote,
        '<'                                          => CharClass.LT,
        '>'                                          => CharClass.GT,
        '='                                          => CharClass.EQ,
        '!'                                          => CharClass.Bang,
        '/'                                          => CharClass.Slash,
        '.'                                          => CharClass.Dot,
        '\n'                                         => CharClass.Newline,
        ' ' or '\t' or '\r'                          => CharClass.Space,
        '+'                                          => CharClass.Plus,
        '-'                                          => CharClass.Minus,
        '*'                                          => CharClass.Star,
        '('                                          => CharClass.LParen,
        ')'                                          => CharClass.RParen,
        '{'                                          => CharClass.LBrace,
        '}'                                          => CharClass.RBrace,
        ','                                          => CharClass.Comma,
        ';'                                          => CharClass.Semi,
        _                                            => CharClass.Other
    };

    // Maximal-munch: avanza mientras haya transicion valida, guarda la ultima posicion aceptante.
    public (bool ok, string lexeme) Run(string src, int startPos)
    {
        int state         = InitialState;
        int pos           = startPos;
        int lastAcceptPos = -1;

        while (pos < src.Length)
        {
            CharClass cls = Classify(src[pos]);
            if (!Transitions.TryGetValue((state, cls), out int next))
                break;

            state = next;
            pos++;

            if (AcceptingStates.Contains(state))
                lastAcceptPos = pos;
        }

        return lastAcceptPos > startPos
            ? (true, src[startPos..lastAcceptPos])
            : (false, string.Empty);
    }
}

// ── IDENTIFICADORES ───────────────────────────────────────────────────────────
// q0 --[letra|_]--> q1* --[letra|digito|_]--> q1*
public sealed class IdentifierAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.Letter)]     = 1,
        [(0, CharClass.Underscore)] = 1,
        [(1, CharClass.Letter)]     = 1,
        [(1, CharClass.Digit)]      = 1,
        [(1, CharClass.Underscore)] = 1,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [1];
    public override TokenType    TokenType       => TokenType.IDENTIFIER;
    public override string       Name            => "IDENTIFICADORES";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── ENTEROS ───────────────────────────────────────────────────────────────────
// q0 --[digito]--> q1* --[digito]--> q1*
public sealed class IntegerAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.Digit)] = 1,
        [(1, CharClass.Digit)] = 1,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [1];
    public override TokenType    TokenType       => TokenType.INTEGER;
    public override string       Name            => "ENTEROS";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── REALES ────────────────────────────────────────────────────────────────────
// q0 --[d]--> q1 --[.]--> q2 --[d]--> q3*
public sealed class RealAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.Digit)] = 1,
        [(1, CharClass.Digit)] = 1,
        [(1, CharClass.Dot)]   = 2,
        [(2, CharClass.Digit)] = 3,
        [(3, CharClass.Digit)] = 3,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [3];
    public override TokenType    TokenType       => TokenType.REAL;
    public override string       Name            => "REALES";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── OPERADORES RELACIONALES ───────────────────────────────────────────────────
// Reconoce: <  <=  >  >=  ==  !=   (NO acepta = simple — lo maneja AssignAutomata)
// q1*=<  q2*=<=  q3*=>  q4*=>=  q5=primer=  q6*===  q7=!  q8*=!=
public sealed class RelOpAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.LT)]   = 1,
        [(1, CharClass.EQ)]   = 2,
        [(0, CharClass.GT)]   = 3,
        [(3, CharClass.EQ)]   = 4,
        [(0, CharClass.EQ)]   = 5,   // q5 NO es aceptante — espera segundo '='
        [(5, CharClass.EQ)]   = 6,
        [(0, CharClass.Bang)] = 7,
        [(7, CharClass.EQ)]   = 8,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [1, 2, 3, 4, 6, 8]; // sin q5
    public override TokenType    TokenType       => TokenType.REL_OP;
    public override string       Name            => "OPERADORES RELACIONALES";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── OPERADORES DE ASIGNACION COMPUESTOS ──────────────────────────────────────
// Reconoce: +=  -=  *=  /=
// q0 --[+]--> q1 --[=]--> q2*
// q0 --[-]--> q3 --[=]--> q4*
// q0 --[*]--> q5 --[=]--> q6*
// q0 --[/]--> q7 --[=]--> q8*
public sealed class CompoundAssignAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.Plus)]  = 1,
        [(1, CharClass.EQ)]    = 2,
        [(0, CharClass.Minus)] = 3,
        [(3, CharClass.EQ)]    = 4,
        [(0, CharClass.Star)]  = 5,
        [(5, CharClass.EQ)]    = 6,
        [(0, CharClass.Slash)] = 7,
        [(7, CharClass.EQ)]    = 8,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [2, 4, 6, 8];
    public override TokenType    TokenType       => TokenType.ASSIGN_OP;
    public override string       Name            => "ASIGNACION COMPUESTA";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── ASIGNACION SIMPLE ─────────────────────────────────────────────────────────
// Reconoce: =    (q0 --[=]--> q1*)
public sealed class AssignAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.EQ)] = 1,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [1];
    public override TokenType    TokenType       => TokenType.ASSIGN_OP;
    public override string       Name            => "ASIGNACION SIMPLE";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── OPERADORES ARITMETICOS ────────────────────────────────────────────────────
// Reconoce: +  -  *  /   (cada uno en un estado distinto)
public sealed class ArithOpAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.Plus)]  = 1,
        [(0, CharClass.Minus)] = 2,
        [(0, CharClass.Star)]  = 3,
        [(0, CharClass.Slash)] = 4,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [1, 2, 3, 4];
    public override TokenType    TokenType       => TokenType.ARITH_OP;
    public override string       Name            => "OPERADORES ARITMETICOS";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── SIMBOLOS ESPECIALES ───────────────────────────────────────────────────────
// Reconoce: ( ) { } , ; .
public sealed class SpecialSymAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.LParen)] = 1,
        [(0, CharClass.RParen)] = 2,
        [(0, CharClass.LBrace)] = 3,
        [(0, CharClass.RBrace)] = 4,
        [(0, CharClass.Comma)]  = 5,
        [(0, CharClass.Semi)]   = 6,
        [(0, CharClass.Dot)]    = 7,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [1, 2, 3, 4, 5, 6, 7];
    public override TokenType    TokenType       => TokenType.SPECIAL_SYM;
    public override string       Name            => "SIMBOLOS ESPECIALES";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── CADENAS ───────────────────────────────────────────────────────────────────
// q0 --["]--> q1 --[^"\n]--> q1 --["]--> q2*
public sealed class StringAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = BuildTransitions();

    private static Dictionary<(int, CharClass), int> BuildTransitions()
    {
        var t = new Dictionary<(int, CharClass), int>
        {
            [(0, CharClass.Quote)] = 1,
            [(1, CharClass.Quote)] = 2,
        };
        foreach (CharClass cls in Enum.GetValues<CharClass>())
            if (cls != CharClass.Quote && cls != CharClass.Newline)
                t[(1, cls)] = 1;
        return t;
    }

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [2];
    public override TokenType    TokenType       => TokenType.STRING;
    public override string       Name            => "CADENAS";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── COMENTARIOS DE LINEA ──────────────────────────────────────────────────────
// q0 --[/]--> q1 --[/]--> q2* --[^\n]--> q3*
public sealed class LineCommentAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = BuildTransitions();

    private static Dictionary<(int, CharClass), int> BuildTransitions()
    {
        var t = new Dictionary<(int, CharClass), int>
        {
            [(0, CharClass.Slash)] = 1,
            [(1, CharClass.Slash)] = 2,
        };
        foreach (CharClass cls in Enum.GetValues<CharClass>())
            if (cls != CharClass.Newline)
            {
                t[(2, cls)] = 3;
                t[(3, cls)] = 3;
            }
        return t;
    }

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [2, 3];
    public override TokenType    TokenType       => TokenType.LINE_COMMENT;
    public override string       Name            => "COMENTARIOS DE LINEA";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}

// ── ESPACIOS EN BLANCO ────────────────────────────────────────────────────────
public sealed class WhitespaceAutomata : Automata
{
    private static readonly Dictionary<(int, CharClass), int> _t = new()
    {
        [(0, CharClass.Space)]   = 1,
        [(0, CharClass.Newline)] = 1,
        [(1, CharClass.Space)]   = 1,
        [(1, CharClass.Newline)] = 1,
    };

    public override int          InitialState    => 0;
    public override int[]        AcceptingStates => [1];
    public override TokenType    TokenType       => TokenType.WHITESPACE;
    public override string       Name            => "ESPACIOS";
    public override Dictionary<(int, CharClass), int> Transitions => _t;
}
