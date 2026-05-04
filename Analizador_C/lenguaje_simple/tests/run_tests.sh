#!/usr/bin/env bash
# =============================================================================
# run_tests.sh — Suite de pruebas automatizadas para el scanner léxico (Flex/C)
#
# USO:
#   cd lenguaje_simple/tests && bash run_tests.sh
#   O bien desde src/: make test_all
#
# REQUISITOS:
#   flex, gcc disponibles en PATH.
#   El scanner se compila automáticamente si el binario no existe.
#
# SALIDA:
#   [PASS] / [FAIL] por cada assertion.
#   Código de salida 0 si todo pasa, 1 si algún test falla.
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SRC_DIR="$SCRIPT_DIR/../src"
EXAMPLES_DIR="$SCRIPT_DIR/../examples"
INPUTS_DIR="$SCRIPT_DIR/inputs"
SCANNER="$SRC_DIR/scanner"

# ── Colores ────────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; BOLD='\033[1m'; NC='\033[0m'

# ── Contadores ─────────────────────────────────────────────────────────────────
PASS=0; FAIL=0; TOTAL=0

# Variables globales para la salida del scanner (se rellenan con run_scanner)
STDOUT=""; STDERR=""

# =============================================================================
# Helpers
# =============================================================================

# Compila el scanner si el binario no existe.
build_if_needed() {
    if [ ! -f "$SCANNER" ]; then
        printf "${YELLOW}=== Compilando scanner ===${NC}\n"
        cd "$SRC_DIR" || exit 1
        flex scanner.l                                              || { echo "ERROR: flex fallo"; exit 1; }
        gcc -Wall -Wextra -std=c99 lex.yy.c symbol_table.c main.c \
            -o scanner -lfl                                        || { echo "ERROR: gcc fallo"; exit 1; }
        printf "${GREEN}Compilacion exitosa.${NC}\n\n"
        cd "$SCRIPT_DIR" || exit 1
    fi
}

# Ejecuta el scanner contra un archivo.
# Captura stdout y stderr en las variables globales STDOUT y STDERR.
run_scanner() {
    local input_file="$1"
    local tmp
    tmp=$(mktemp /tmp/sc_stderr.XXXXXX)
    STDOUT=$("$SCANNER" "$input_file" 2>"$tmp") || true
    STDERR=$(cat "$tmp")
    rm -f "$tmp"
}

# Registra un resultado de assertion.
# $1 = nombre del test
# $2 = "true" | "false"
# $3 = mensaje de detalle en caso de FAIL (opcional)
check() {
    local name="$1" result="$2" detail="${3:-}"
    TOTAL=$((TOTAL + 1))
    if [ "$result" = "true" ]; then
        printf "  ${GREEN}[PASS]${NC} %s\n" "$name"
        PASS=$((PASS + 1))
    else
        printf "  ${RED}[FAIL]${NC} %s\n" "$name"
        [ -n "$detail" ] && printf "         %s\n" "$detail"
        FAIL=$((FAIL + 1))
    fi
}

# Devuelve "true" si $STDOUT contiene la cadena exacta $1.
has_out()    { echo "$STDOUT" | grep -qF  "$1" && echo true || echo false; }

# Devuelve "true" si $STDERR contiene la cadena exacta $1.
has_err()    { echo "$STDERR" | grep -qF  "$1" && echo true || echo false; }

# Devuelve "true" si $STDERR está vacío.
empty_err()  { [ -z "$STDERR" ] && echo true || echo false; }

# Cuenta las filas de tokens (líneas que empiezan con "| " seguido de dígito).
token_count() { echo "$STDOUT" | grep -c '^| [0-9]' 2>/dev/null || echo 0; }

# =============================================================================
# Suite
# =============================================================================
printf "${BOLD}==============================================${NC}\n"
printf "${BOLD}  Suite de pruebas — Scanner Léxico (Flex/C)${NC}\n"
printf "${BOLD}==============================================${NC}\n\n"

build_if_needed

# ─ T1: test_valid.sl — entrada completamente válida ──────────────────────────
printf "${BOLD}[T1] Entrada válida — test_valid.sl${NC}\n"
run_scanner "$EXAMPLES_DIR/test_valid.sl"

check "T1-01 sin mensajes de error en stderr"           "$(empty_err)"                 "stderr: $STDERR"
check "T1-02 encabezado de tabla de tokens presente"    "$(has_out 'SCANNER LEXICO')"  ""
check "T1-03 tabla de símbolos presente"                "$(has_out 'TABLA DE SIMBOLOS')" ""
check "T1-04 mensaje de completado presente"            "$(has_out '[OK] Analisis lexico completado')" ""
check "T1-05 KW_INT reconocido"                        "$(has_out 'KW_INT')"          ""
check "T1-06 KW_FLOAT reconocido"                      "$(has_out 'KW_FLOAT')"        ""
check "T1-07 KW_RETURN reconocido"                     "$(has_out 'KW_RETURN')"       ""
check "T1-08 KW_IF reconocido"                         "$(has_out 'KW_IF')"           ""
check "T1-09 KW_ELSE reconocido"                       "$(has_out 'KW_ELSE')"         ""
check "T1-10 KW_WHILE reconocido"                      "$(has_out 'KW_WHILE')"        ""
check "T1-11 KW_FOR reconocido"                        "$(has_out 'KW_FOR')"          ""
check "T1-12 IDENTIFIER reconocido"                    "$(has_out 'IDENTIFIER')"      ""
check "T1-13 FLOAT_LIT reconocido"                     "$(has_out 'FLOAT_LIT')"       ""
check "T1-14 INT_LIT reconocido"                       "$(has_out 'INT_LIT')"         ""
check "T1-15 COMMENT reconocido"                       "$(has_out 'COMMENT')"         ""
check "T1-16 OP_PLUS reconocido"                       "$(has_out 'OP_PLUS')"         ""
check "T1-17 OP_ASSIGN reconocido"                     "$(has_out 'OP_ASSIGN')"       ""
check "T1-18 OP_PASSIGN (+= ) reconocido"              "$(has_out 'OP_PASSIGN')"      ""
check "T1-19 OP_GEQ (>= ) reconocido"                  "$(has_out 'OP_GEQ')"          ""
check "T1-20 OP_MASSIGN (-= ) reconocido"              "$(has_out 'OP_MASSIGN')"      ""
check "T1-21 SYM_SEMI reconocido"                      "$(has_out 'SYM_SEMI')"        ""
check "T1-22 SYM_LPAREN reconocido"                    "$(has_out 'SYM_LPAREN')"      ""
check "T1-23 SYM_LBRACE reconocido"                    "$(has_out 'SYM_LBRACE')"      ""
check "T1-24 'suma' en tabla de símbolos"              "$(has_out '| suma')"          ""
check "T1-25 'promedio' en tabla de símbolos"          "$(has_out '| promedio')"      ""
check "T1-26 'main' en tabla de símbolos"              "$(has_out '| main')"          ""
check "T1-27 '3.14' en tabla de símbolos como FLOAT"   "$(has_out '3.14')"            ""
printf "\n"

# ─ T2: test_errors.sl — manejo de errores léxicos ────────────────────────────
printf "${BOLD}[T2] Manejo de errores — test_errors.sl${NC}\n"
run_scanner "$EXAMPLES_DIR/test_errors.sl"

N_ERRORS=$(echo "$STDERR" | grep -c '\[ERROR LEXICO\]' 2>/dev/null || echo 0)
check "T2-01 stderr contiene '[ERROR LEXICO]'"         "$(has_err '[ERROR LEXICO]')"  ""
check "T2-02 se reportan exactamente 4 errores"        "$( [ "$N_ERRORS" -eq 4 ] && echo true || echo false )" "encontrados: $N_ERRORS"
check "T2-03 error para '@' en stderr"                 "$(has_err '@')"               ""
check "T2-04 error para '~' en stderr"                 "$(has_err '~')"               ""
check "T2-05 error para '\`' en stderr"                "$(has_err '`')"               ""
check "T2-06 error para '\$' en stderr"                "$(echo "$STDERR" | grep -qF '$' && echo true || echo false)" ""
check "T2-07 scanner continúa tras errores (stdout tiene KW_INT)" "$(has_out 'KW_INT')"  ""
check "T2-08 tabla de símbolos generada pese a errores"  "$(has_out 'TABLA DE SIMBOLOS')" ""
check "T2-09 mensaje de completado presente"             "$(has_out '[OK] Analisis lexico completado')" ""
printf "\n"

# ─ T3: keywords.sl — todas las palabras reservadas ───────────────────────────
printf "${BOLD}[T3] Palabras reservadas — keywords.sl${NC}\n"
run_scanner "$INPUTS_DIR/keywords.sl"

check "T3-01 KW_IF"                                    "$(has_out 'KW_IF')"     ""
check "T3-02 KW_ELSE"                                  "$(has_out 'KW_ELSE')"   ""
check "T3-03 KW_WHILE"                                 "$(has_out 'KW_WHILE')"  ""
check "T3-04 KW_FOR"                                   "$(has_out 'KW_FOR')"    ""
check "T3-05 KW_INT"                                   "$(has_out 'KW_INT')"    ""
check "T3-06 KW_FLOAT"                                 "$(has_out 'KW_FLOAT')"  ""
check "T3-07 KW_RETURN"                                "$(has_out 'KW_RETURN')" ""
check "T3-08 KW_VOID"                                  "$(has_out 'KW_VOID')"   ""
check "T3-09 KW_TRUE"                                  "$(has_out 'KW_TRUE')"   ""
check "T3-10 KW_FALSE"                                 "$(has_out 'KW_FALSE')"  ""
check "T3-11 ninguna palabra reservada clasificada como IDENTIFIER" \
    "$( ! has_out 'IDENTIFIER' | grep -q true && echo true || echo false)" \
    "se encontró IDENTIFIER inesperado"
check "T3-12 sin errores léxicos"                      "$(empty_err)"           "stderr: $STDERR"
printf "\n"

# ─ T4: operators.sl — todos los operadores ───────────────────────────────────
printf "${BOLD}[T4] Operadores — operators.sl${NC}\n"
run_scanner "$INPUTS_DIR/operators.sl"

check "T4-01 OP_PLUS"                                  "$(has_out 'OP_PLUS')"    ""
check "T4-02 OP_MINUS"                                 "$(has_out 'OP_MINUS')"   ""
check "T4-03 OP_MULT"                                  "$(has_out 'OP_MULT')"    ""
check "T4-04 OP_DIV"                                   "$(has_out 'OP_DIV')"     ""
check "T4-05 OP_EQ (==)"                               "$(has_out 'OP_EQ')"      ""
check "T4-06 OP_NEQ (!=)"                              "$(has_out 'OP_NEQ')"     ""
check "T4-07 OP_LT (<)"                                "$(has_out 'OP_LT')"      ""
check "T4-08 OP_GT (>)"                                "$(has_out 'OP_GT')"      ""
check "T4-09 OP_LEQ (<=)"                              "$(has_out 'OP_LEQ')"     ""
check "T4-10 OP_GEQ (>=)"                              "$(has_out 'OP_GEQ')"     ""
check "T4-11 OP_ASSIGN (=)"                            "$(has_out 'OP_ASSIGN')"  ""
check "T4-12 OP_PASSIGN (+=)"                          "$(has_out 'OP_PASSIGN')" ""
check "T4-13 OP_MASSIGN (-=)"                          "$(has_out 'OP_MASSIGN')" ""
check "T4-14 OP_STASSIGN (*=)"                         "$(has_out 'OP_STASSIGN')" ""
check "T4-15 OP_DASSIGN (/=)"                          "$(has_out 'OP_DASSIGN')" ""
check "T4-16 sin errores léxicos"                      "$(empty_err)"            "stderr: $STDERR"
printf "\n"

# ─ T5: multiline.sl — tracking de número de línea ────────────────────────────
printf "${BOLD}[T5] Números de línea — multiline.sl${NC}\n"
run_scanner "$INPUTS_DIR/multiline.sl"

check "T5-01 KW_INT reconocido en múltiples líneas"    "$(has_out 'KW_INT')"    ""
check "T5-02 KW_FLOAT reconocido"                      "$(has_out 'KW_FLOAT')"  ""
check "T5-03 'alpha' en tabla de símbolos"             "$(has_out '| alpha')"   ""
check "T5-04 'beta' en tabla de símbolos"              "$(has_out '| beta')"    ""
check "T5-05 'gamma' en tabla de símbolos"             "$(has_out '| gamma')"   ""
check "T5-06 'delta' en tabla de símbolos"             "$(has_out '| delta')"   ""
check "T5-07 'epsilon' en tabla de símbolos"           "$(has_out '| epsilon')" ""
# Los tokens de las líneas 4-8 deben tener números de línea >= 4
LINE4=$(echo "$STDOUT" | grep -F '| alpha' | grep -oE '[0-9]+[[:space:]]*\|$' | grep -oE '^[0-9]+' || echo 0)
check "T5-08 'alpha' aparece en línea 4"               "$( [ "$LINE4" -eq 4 ] && echo true || echo false)" "línea encontrada: $LINE4"
check "T5-09 sin errores léxicos"                      "$(empty_err)"           "stderr: $STDERR"
printf "\n"

# ─ T6: empty.sl — archivo vacío ──────────────────────────────────────────────
printf "${BOLD}[T6] Archivo vacío — empty.sl${NC}\n"
run_scanner "$INPUTS_DIR/empty.sl"

check "T6-01 sin errores léxicos"                      "$(empty_err)"                           "stderr: $STDERR"
check "T6-02 mensaje de completado presente"           "$(has_out '[OK] Analisis lexico completado')" ""
check "T6-03 tabla de símbolos vacía (0 entradas)"     "$(has_out 'Total de entradas: 0')"     ""
N_TOKENS=$(token_count)
check "T6-04 cero filas de tokens en la tabla"         "$( [ "$N_TOKENS" -eq 0 ] && echo true || echo false)" "filas encontradas: $N_TOKENS"
printf "\n"

# ─ T7: mixed_errors.sl — errores mezclados con tokens válidos ────────────────
printf "${BOLD}[T7] Errores mezclados — mixed_errors.sl${NC}\n"
run_scanner "$INPUTS_DIR/mixed_errors.sl"

N_MIX_ERRORS=$(echo "$STDERR" | grep -c '\[ERROR LEXICO\]' 2>/dev/null || echo 0)
check "T7-01 al menos un error léxico reportado"       "$( [ "$N_MIX_ERRORS" -gt 0 ] && echo true || echo false)" "errores: $N_MIX_ERRORS"
check "T7-02 tokens válidos aún reconocidos (IDENTIFIER)" "$(has_out 'IDENTIFIER')"  ""
check "T7-03 KW_INT reconocido pese a errores"         "$(has_out 'KW_INT')"         ""
check "T7-04 KW_FLOAT reconocido pese a errores"       "$(has_out 'KW_FLOAT')"       ""
check "T7-05 scanner completó el análisis"             "$(has_out '[OK] Analisis lexico completado')" ""
check "T7-06 tabla de símbolos generada"               "$(has_out 'TABLA DE SIMBOLOS')" ""
printf "\n"

# =============================================================================
# Resumen
# =============================================================================
printf "${BOLD}==============================================${NC}\n"
if [ "$FAIL" -eq 0 ]; then
    printf "${GREEN}${BOLD}  TODOS LOS TESTS PASARON${NC}\n"
else
    printf "${RED}${BOLD}  ALGUNOS TESTS FALLARON${NC}\n"
fi
printf "  Resultados: ${GREEN}%d PASS${NC}  ${RED}%d FAIL${NC}  / %d total\n" \
       "$PASS" "$FAIL" "$TOTAL"
printf "${BOLD}==============================================${NC}\n"

if [ "$FAIL" -eq 0 ]; then
    exit 0
else
    exit 1
fi
