/*
 * ===========================================================================
 * Archivo : token_ids.h
 * Proyecto: Scanner Léxico — Lenguaje Académico Simple
 * ===========================================================================
 * DESCRIPCION:
 *   Fuente única de verdad para todos los Token IDs del scanner.
 *   Cualquier archivo que necesite referenciar un tipo de token debe
 *   incluir este header; así se evita duplicar definiciones y se garantiza
 *   consistencia entre el lexer, la tabla de símbolos y el programa principal.
 *
 * RELACION CON OTROS ARCHIVOS:
 *   - scanner.l      -> incluye symbol_table.h que incluye este archivo
 *   - symbol_table.h -> incluye este archivo
 *   - symbol_table.c -> recibe IDs via symbol_table.h
 *   - main.c         -> recibe IDs via symbol_table.h
 * ===========================================================================
 */

#ifndef TOKEN_IDS_H
#define TOKEN_IDS_H

/* ── Palabras reservadas: 100–109 ─────────────────────────────────────────── */
#define TK_IF       100   /* if                  */
#define TK_ELSE     101   /* else                */
#define TK_WHILE    102   /* while               */
#define TK_FOR      103   /* for                 */
#define TK_INT      104   /* int  (tipo dato)    */
#define TK_FLOAT    105   /* float (tipo dato)   */
#define TK_RETURN   106   /* return              */
#define TK_VOID     107   /* void                */
#define TK_TRUE     108   /* true                */
#define TK_FALSE    109   /* false               */

/* ── Identificadores y literales: 200–202 ────────────────────────────────── */
#define TK_ID       200   /* identificador       */
#define TK_INT_LIT  201   /* literal entero      */
#define TK_FLT_LIT  202   /* literal flotante    */

/* ── Operadores aritméticos: 300–303 ─────────────────────────────────────── */
#define TK_PLUS     300   /* +                   */
#define TK_MINUS    301   /* -                   */
#define TK_MULT     302   /* *                   */
#define TK_DIV      303   /* /                   */

/* ── Operadores relacionales: 310–315 ────────────────────────────────────── */
#define TK_EQ       310   /* ==                  */
#define TK_NEQ      311   /* !=                  */
#define TK_LT       312   /* <                   */
#define TK_GT       313   /* >                   */
#define TK_LEQ      314   /* <=                  */
#define TK_GEQ      315   /* >=                  */

/* ── Operadores de asignación: 320–324 ───────────────────────────────────── */
#define TK_ASSIGN   320   /* =                   */
#define TK_PASSIGN  321   /* +=                  */
#define TK_MASSIGN  322   /* -=                  */
#define TK_STASSIGN 323   /* *=                  */
#define TK_DASSIGN  324   /* /=                  */

/* ── Símbolos especiales: 400–406 ────────────────────────────────────────── */
#define TK_LPAREN   400   /* (                   */
#define TK_RPAREN   401   /* )                   */
#define TK_LBRACE   402   /* {                   */
#define TK_RBRACE   403   /* }                   */
#define TK_COMMA    404   /* ,                   */
#define TK_SEMI     405   /* ;                   */
#define TK_DOT      406   /* .                   */

/* ── Comentarios: 500 ────────────────────────────────────────────────────── */
#define TK_COMMENT  500   /* // comentario       */

#endif /* TOKEN_IDS_H */
