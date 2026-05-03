/*
 * ===========================================================================
 * Archivo : symbol_table.h
 * Proyecto: Scanner Léxico — Lenguaje Académico Simple
 * ===========================================================================
 * DESCRIPCION:
 *   Interfaz pública de la tabla de símbolos del scanner.
 *   La tabla registra los lexemas que necesitan ser recordados en fases
 *   posteriores del compilador: identificadores y literales numéricos.
 *
 *   Tokens que SE registran (requieren tabla de símbolos):
 *     TK_ID      — el nombre de la variable/función debe conocerse en análisis
 *                  semántico para verificar declaraciones y tipos.
 *     TK_INT_LIT — el valor constante puede usarse en evaluación o propagación.
 *     TK_FLT_LIT — igual que TK_INT_LIT.
 *
 *   Tokens que NO se registran:
 *     Palabras reservadas — semántica fija, no necesitan entrada en tabla.
 *     Operadores y símbolos — reconocidos por su forma, sin identidad propia.
 *     Comentarios — sin relevancia semántica.
 *
 * ESTRUCTURA DE CADA ENTRADA:
 *     lexeme   — texto exacto del token en el código fuente
 *     token_id — tipo de token (TK_ID, TK_INT_LIT o TK_FLT_LIT)
 *     lineno   — primera línea de aparición
 *
 * RELACION CON OTROS ARCHIVOS:
 *   - token_ids.h    — incluido aquí; define las constantes TK_*
 *   - symbol_table.c — implementa las funciones declaradas aquí
 *   - scanner.l      — llama a sym_insert() en las acciones de lex
 *   - main.c         — llama a sym_print() al finalizar el escaneo
 * ===========================================================================
 */

#ifndef SYMBOL_TABLE_H
#define SYMBOL_TABLE_H

#include "token_ids.h"

#define SYM_MAX 512   /* entradas máximas en la tabla                */
#define LEX_MAX 128   /* longitud máxima de un lexema (con \0)       */

/* Una entrada de la tabla de símbolos */
typedef struct {
    char lexeme[LEX_MAX];
    int  token_id;
    int  lineno;
} SymEntry;

/*
 * sym_insert — inserta lexema si no existe aún.
 *   lexeme   : texto del token
 *   token_id : tipo (TK_ID, TK_INT_LIT, TK_FLT_LIT)
 *   lineno   : línea de primera aparición
 */
void sym_insert(const char *lexeme, int token_id, int lineno);

/*
 * sym_print — imprime la tabla completa en stdout al finalizar el escaneo.
 */
void sym_print(void);

#endif /* SYMBOL_TABLE_H */
