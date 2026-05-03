/*
 * ===========================================================================
 * Archivo : symbol_table.c
 * Proyecto: Scanner Léxico — Lenguaje Académico Simple
 * ===========================================================================
 * DESCRIPCION:
 *   Implementación de la tabla de símbolos. Usa un arreglo estático con
 *   búsqueda lineal para verificar duplicados antes de insertar.
 *   Suficiente para el tamaño de programas de prueba académicos.
 *
 * RELACION CON OTROS ARCHIVOS:
 *   - symbol_table.h — declara la interfaz que este archivo implementa
 *   - scanner.l      — invoca sym_insert() al reconocer ID y literales
 *   - main.c         — invoca sym_print() al terminar el escaneo
 * ===========================================================================
 */

#include <stdio.h>
#include <string.h>
#include "symbol_table.h"

/* Tabla estática y contador de entradas actuales */
static SymEntry table[SYM_MAX];
static int      count = 0;

/* Convierte un token_id a cadena legible (solo para los que van en tabla) */
static const char *type_name(int id)
{
    switch (id) {
        case TK_ID:      return "IDENTIFIER";
        case TK_INT_LIT: return "INTEGER";
        case TK_FLT_LIT: return "FLOAT";
        default:         return "UNKNOWN";
    }
}

/*
 * sym_insert
 *   Busca linealmente el lexema; si no existe lo agrega al final.
 *   Si la tabla está llena, emite advertencia y descarta la entrada.
 */
void sym_insert(const char *lexeme, int token_id, int lineno)
{
    int i;
    for (i = 0; i < count; i++)
        if (strcmp(table[i].lexeme, lexeme) == 0)
            return;   /* ya registrado, no duplicar */

    if (count >= SYM_MAX) {
        fprintf(stderr, "[AVISO] Tabla de símbolos llena. '%s' no registrado.\n",
                lexeme);
        return;
    }

    strncpy(table[count].lexeme, lexeme, LEX_MAX - 1);
    table[count].lexeme[LEX_MAX - 1] = '\0';
    table[count].token_id = token_id;
    table[count].lineno   = lineno;
    count++;
}

/*
 * sym_print
 *   Imprime la tabla en formato de columnas alineadas:
 *   Índice | Lexema | Tipo | Línea
 */
void sym_print(void)
{
    int i;
    printf("\n");
    printf("+=======+===========================+==============+=======+\n");
    printf("|              TABLA DE SIMBOLOS                          |\n");
    printf("+=======+===========================+==============+=======+\n");
    printf("| %-5s | %-25s | %-12s | %-5s |\n",
           "Indx", "Lexema", "Tipo", "Linea");
    printf("+-------+---------------------------+--------------+-------+\n");
    for (i = 0; i < count; i++)
        printf("| %-5d | %-25s | %-12s | %-5d |\n",
               i, table[i].lexeme,
               type_name(table[i].token_id),
               table[i].lineno);
    if (count == 0)
        printf("|              (sin entradas)                             |\n");
    printf("+=======+===========================+==============+=======+\n");
    printf("  Total de entradas: %d\n", count);
}
