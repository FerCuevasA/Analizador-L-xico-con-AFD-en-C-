/*
 * ===========================================================================
 * Archivo : scanner.l
 * Proyecto: Scanner Léxico — Lenguaje Académico Simple
 * ===========================================================================
 * DESCRIPCION:
 *   Especificación léxica para un lenguaje académico simple con sintaxis
 *   similar a C/Java. Este archivo es procesado por flex para generar
 *   el código C del analizador léxico (lex.yy.c).
 *
 *   Tokens reconocidos (ver token_ids.h para los IDs numéricos):
 *     - Palabras reservadas : if else while for int float return void true false
 *     - Identificadores     : letra seguida de letras/dígitos/guion_bajo
 *     - Literales enteros   : uno o más dígitos
 *     - Literales flotantes : dígitos PUNTO dígitos
 *     - Operadores aritmét. : + - * /
 *     - Operadores relac.   : == != < > <= >=
 *     - Operadores asignac. : = += -= *= /=
 *     - Símbolos especiales : ( ) { } , ; .
 *     - Comentarios de línea: // hasta fin de línea
 *     - Errores léxicos     : cualquier carácter no reconocido
 *
 * ORDEN DE REGLAS (crítico en flex — gana la regla más larga; si empatan,
 * gana la que aparece primero):
 *   1. Palabras reservadas  — antes que IDENTIFIER para mayor prioridad
 *   2. Literal FLOAT        — antes que INTEGER (maximal-munch: "3.14" completo)
 *   3. Literal INTEGER
 *   4. IDENTIFIER
 *   5. Operadores compuestos (+=, ==, <=, etc.) — antes que los simples
 *   6. Operadores simples
 *   7. Símbolos especiales
 *   8. Comentarios
 *   9. Espacios en blanco   — consumidos silenciosamente
 *  10. Regla de error       — ÚLTIMA, cubre cualquier carácter restante
 *
 * COMPILACION:
 *   flex scanner.l
 *   gcc lex.yy.c symbol_table.c main.c -o scanner -lfl
 *
 * RELACION CON OTROS ARCHIVOS:
 *   - token_ids.h    — constantes TK_* (vía symbol_table.h)
 *   - symbol_table.h — interfaz de tabla de símbolos
 *   - symbol_table.c — implementación de tabla de símbolos
 *   - main.c         — llama a yylex() e imprime tabla al final
 * ===========================================================================
 */

/* ── SECCIÓN 1: CÓDIGO C QUE VA AL INICIO DE lex.yy.c ──────────────────── */
%{
#include <stdio.h>
#include <string.h>
#include "symbol_table.h"
/* Todos los TK_* llegan transitivamente vía symbol_table.h → token_ids.h */

/* yylineno es declarada automáticamente por flex con %option yylineno */

/* Prototipos de funciones auxiliares definidas en la Sección 3 */
void emit(int id, const char *name, const char *lexeme);
void lex_error(const char *bad_char);
%}

/* ── OPCIONES DE FLEX ────────────────────────────────────────────────────── */
%option noyywrap   /* no necesita función yywrap() al llegar a EOF          */
%option yylineno   /* flex mantiene yylineno con el número de línea actual  */

/* ── MACROS DE EXPRESIONES REGULARES ────────────────────────────────────── */
/*
 * Se definen aquí para que las reglas sean más legibles.
 * Cada macro aparece documentada con su expresión regular equivalente.
 */
LETTER   [a-zA-Z_]              /* letra o guion bajo (inicio de ID)        */
DIGIT    [0-9]                  /* dígito decimal                           */
ID       {LETTER}({LETTER}|{DIGIT})*   /* identificador                    */
INT      {DIGIT}+               /* uno o más dígitos                        */
FLOAT    {DIGIT}+\.{DIGIT}+     /* dígitos, punto, dígitos                  */
COMMENT  \/\/[^\n]*             /* // seguido de cualquier cosa hasta \n    */
BLANK    [ \t\r]+               /* espacios/tabulaciones (no \n)            */

/* ── SECCIÓN 2: REGLAS ───────────────────────────────────────────────────── */
%%

    /* ── 1. PALABRAS RESERVADAS ─────────────────────────────────────────── */
    /* Deben ir antes de la regla de ID para que "if" no sea un identifier. */

"if"        { emit(TK_IF,     "KW_IF",     yytext); }
"else"      { emit(TK_ELSE,   "KW_ELSE",   yytext); }
"while"     { emit(TK_WHILE,  "KW_WHILE",  yytext); }
"for"       { emit(TK_FOR,    "KW_FOR",    yytext); }
"int"       { emit(TK_INT,    "KW_INT",    yytext); }
"float"     { emit(TK_FLOAT,  "KW_FLOAT",  yytext); }
"return"    { emit(TK_RETURN, "KW_RETURN", yytext); }
"void"      { emit(TK_VOID,   "KW_VOID",   yytext); }
"true"      { emit(TK_TRUE,   "KW_TRUE",   yytext); }
"false"     { emit(TK_FALSE,  "KW_FALSE",  yytext); }

    /* ── 2. LITERAL FLOTANTE ────────────────────────────────────────────── */
    /* Debe ir ANTES que INTEGER. Si fuera después, "3.14" se tokenizaría   */
    /* como INTEGER(3) + DOT(.) + INTEGER(14) — incorrecto.                 */

{FLOAT}     {
                emit(TK_FLT_LIT, "FLOAT_LIT", yytext);
                sym_insert(yytext, TK_FLT_LIT, yylineno);
            }

    /* ── 3. LITERAL ENTERO ──────────────────────────────────────────────── */

{INT}       {
                emit(TK_INT_LIT, "INT_LIT", yytext);
                sym_insert(yytext, TK_INT_LIT, yylineno);
            }

    /* ── 4. IDENTIFICADOR ───────────────────────────────────────────────── */
    /* Solo llega aquí si ninguna palabra reservada lo capturó primero.     */

{ID}        {
                emit(TK_ID, "IDENTIFIER", yytext);
                sym_insert(yytext, TK_ID, yylineno);
            }

    /* ── 5. OPERADORES COMPUESTOS ───────────────────────────────────────── */
    /* Deben ir antes que los simples: "+=" debe reconocerse completo        */
    /* antes de que "+" y "=" se reconozcan por separado.                    */

"+="        { emit(TK_PASSIGN,  "OP_PASSIGN",  yytext); }
"-="        { emit(TK_MASSIGN,  "OP_MASSIGN",  yytext); }
"*="        { emit(TK_STASSIGN, "OP_STASSIGN", yytext); }
"/="        { emit(TK_DASSIGN,  "OP_DASSIGN",  yytext); }
"=="        { emit(TK_EQ,       "OP_EQ",       yytext); }
"!="        { emit(TK_NEQ,      "OP_NEQ",      yytext); }
"<="        { emit(TK_LEQ,      "OP_LEQ",      yytext); }
">="        { emit(TK_GEQ,      "OP_GEQ",      yytext); }

    /* ── 6. OPERADORES SIMPLES ──────────────────────────────────────────── */

"+"         { emit(TK_PLUS,   "OP_PLUS",   yytext); }
"-"         { emit(TK_MINUS,  "OP_MINUS",  yytext); }
"*"         { emit(TK_MULT,   "OP_MULT",   yytext); }
"/"         { emit(TK_DIV,    "OP_DIV",    yytext); }
"<"         { emit(TK_LT,     "OP_LT",     yytext); }
">"         { emit(TK_GT,     "OP_GT",     yytext); }
"="         { emit(TK_ASSIGN, "OP_ASSIGN", yytext); }

    /* ── 7. SÍMBOLOS ESPECIALES ─────────────────────────────────────────── */

"("         { emit(TK_LPAREN, "SYM_LPAREN", yytext); }
")"         { emit(TK_RPAREN, "SYM_RPAREN", yytext); }
"{"         { emit(TK_LBRACE, "SYM_LBRACE", yytext); }
"}"         { emit(TK_RBRACE, "SYM_RBRACE", yytext); }
","         { emit(TK_COMMA,  "SYM_COMMA",  yytext); }
";"         { emit(TK_SEMI,   "SYM_SEMI",   yytext); }
"."         { emit(TK_DOT,    "SYM_DOT",    yytext); }

    /* ── 8. COMENTARIOS DE LÍNEA ────────────────────────────────────────── */
    /* Se registra como token pero NO va a la tabla de símbolos.            */

{COMMENT}   { emit(TK_COMMENT, "COMMENT", yytext); }

    /* ── 9. SALTOS DE LÍNEA ─────────────────────────────────────────────── */
    /* yylineno los cuenta automáticamente; no necesitan emitirse.          */

\n          { /* consumido silenciosamente; yylineno ya fue incrementado */ }

    /* ── 10. ESPACIOS EN BLANCO ─────────────────────────────────────────── */

{BLANK}     { /* ignorados; no generan token                               */ }

    /* ── 11. ERROR LÉXICO (regla de último recurso) ─────────────────────── */
    /* El punto "." en flex coincide con cualquier carácter excepto \n.     */
    /* Al ir al final, solo atrapa lo que ninguna regla anterior reconoció. */

.           { lex_error(yytext); }

%%

/* ── SECCIÓN 3: FUNCIONES AUXILIARES EN C ────────────────────────────────── */

/*
 * emit — imprime una fila en la tabla de tokens.
 *   id     : Token ID numérico (de token_ids.h)
 *   name   : nombre descriptivo del token (p. ej. "KW_IF")
 *   lexeme : texto exacto del código fuente reconocido (yytext)
 */
void emit(int id, const char *name, const char *lexeme)
{
    printf("| %-5d | %-14s | %-25s | %-5d |\n",
           id, name, lexeme, yylineno);
}

/*
 * lex_error — reporta un símbolo no reconocido en stderr.
 *   El análisis continúa (recuperación por pánico: descartar el carácter).
 *   bad_char : el carácter (o cadena mínima) que causó el error
 */
void lex_error(const char *bad_char)
{
    fprintf(stderr,
            "[ERROR LEXICO] linea %d: simbolo no reconocido '%s'\n",
            yylineno, bad_char);
}
