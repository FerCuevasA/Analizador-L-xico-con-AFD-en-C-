"""
simulate.py — simulador de referencia del scanner léxico
Reproduce la salida exacta de scanner.l para validar el comportamiento
esperado cuando el entorno de compilación no está disponible.
NO es la implementación del scanner; esa es scanner.l + C.
"""
import re, sys

# ── Token IDs (deben coincidir con token_ids.h) ───────────────────────────
TOKENS = [
    # (pattern, id, name, to_symtable)
    (r'int\b',    104, 'KW_INT',     False),
    (r'float\b',  105, 'KW_FLOAT',   False),
    (r'if\b',     100, 'KW_IF',      False),
    (r'else\b',   101, 'KW_ELSE',    False),
    (r'while\b',  102, 'KW_WHILE',   False),
    (r'for\b',    103, 'KW_FOR',     False),
    (r'return\b', 106, 'KW_RETURN',  False),
    (r'void\b',   107, 'KW_VOID',    False),
    (r'true\b',   108, 'KW_TRUE',    False),
    (r'false\b',  109, 'KW_FALSE',   False),
    # literales (FLOAT antes que INT — maximal-munch)
    (r'[0-9]+\.[0-9]+', 202, 'FLOAT_LIT',  True),
    (r'[0-9]+',          201, 'INT_LIT',    True),
    # identificador
    (r'[a-zA-Z_][a-zA-Z0-9_]*', 200, 'IDENTIFIER', True),
    # operadores compuestos (antes que los simples)
    (r'\+=',  321, 'OP_PASSIGN',  False),
    (r'-=',   322, 'OP_MASSIGN',  False),
    (r'\*=',  323, 'OP_STASSIGN', False),
    (r'/=',   324, 'OP_DASSIGN',  False),
    (r'==',   310, 'OP_EQ',       False),
    (r'!=',   311, 'OP_NEQ',      False),
    (r'<=',   314, 'OP_LEQ',      False),
    (r'>=',   315, 'OP_GEQ',      False),
    # operadores simples
    (r'\+',   300, 'OP_PLUS',     False),
    (r'-',    301, 'OP_MINUS',    False),
    (r'\*',   302, 'OP_MULT',     False),
    (r'/',    303, 'OP_DIV',      False),
    (r'<',    312, 'OP_LT',       False),
    (r'>',    313, 'OP_GT',       False),
    (r'=',    320, 'OP_ASSIGN',   False),
    # símbolos especiales
    (r'\(',   400, 'SYM_LPAREN',  False),
    (r'\)',   401, 'SYM_RPAREN',  False),
    (r'\{',   402, 'SYM_LBRACE',  False),
    (r'\}',   403, 'SYM_RBRACE',  False),
    (r',',    404, 'SYM_COMMA',   False),
    (r';',    405, 'SYM_SEMI',    False),
    (r'\.',   406, 'SYM_DOT',     False),
    # comentario de línea (antes de /)
    (r'//[^\n]*', 500, 'COMMENT', False),
]

def scan(source):
    sym_table = []
    sym_seen  = set()
    token_rows = []
    lineno = 1
    pos = 0

    while pos < len(source):
        ch = source[pos]
        if ch == '\n':
            lineno += 1
            pos += 1
            continue
        if ch in ' \t\r':
            pos += 1
            continue

        matched = False
        for pattern, tid, name, to_sym in TOKENS:
            m = re.match(pattern, source[pos:])
            if m:
                lexeme = m.group(0)
                token_rows.append((tid, name, lexeme, lineno))
                if to_sym and lexeme not in sym_seen:
                    sym_seen.add(lexeme)
                    sym_table.append((lexeme, name, lineno))
                pos += len(lexeme)
                matched = True
                break
        if not matched:
            print(f"[ERROR LEXICO] linea {lineno}: simbolo no reconocido '{ch}'",
                  file=sys.stderr)
            pos += 1

    return token_rows, sym_table

def main():
    source = sys.stdin.read() if len(sys.argv) < 2 else open(sys.argv[1]).read()

    rows, syms = scan(source)

    print("+=======+================+===========================+=======+")
    print("|          SCANNER LEXICO — Lista de Tokens                  |")
    print("+=======+================+===========================+=======+")
    print(f"| {'ID':<5} | {'Nombre':<14} | {'Lexema':<25} | {'Linea':<5} |")
    print("+-------+----------------+---------------------------+-------+")
    for tid, name, lexeme, line in rows:
        print(f"| {tid:<5} | {name:<14} | {lexeme:<25} | {line:<5} |")
    print("+=======+================+===========================+=======+")

    print()
    print("+=======+===========================+==============+=======+")
    print("|              TABLA DE SIMBOLOS                          |")
    print("+=======+===========================+==============+=======+")
    print(f"| {'Indx':<5} | {'Lexema':<25} | {'Tipo':<12} | {'Linea':<5} |")
    print("+-------+---------------------------+--------------+-------+")
    for i, (lex, typ, line) in enumerate(syms):
        tipo = "IDENTIFIER" if "ID" in typ else ("FLOAT" if "FLT" in typ or "FLOAT" in typ else "INTEGER")
        print(f"| {i:<5} | {lex:<25} | {tipo:<12} | {line:<5} |")
    if not syms:
        print("|              (sin entradas)                             |")
    print("+=======+===========================+==============+=======+")
    print(f"  Total de entradas: {len(syms)}")
    print()
    print("[OK] Analisis lexico completado.")

if __name__ == "__main__":
    main()
