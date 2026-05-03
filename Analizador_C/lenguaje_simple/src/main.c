/*
 * ===========================================================================
 * Archivo : main.c
 * Proyecto: Scanner Léxico — Lenguaje Académico Simple
 * ===========================================================================
 * DESCRIPCION:
 *   Programa principal del scanner. Su única responsabilidad es:
 *     1. Abrir el archivo fuente (argumento) o leer desde stdin.
 *     2. Imprimir el encabezado de la tabla de tokens.
 *     3. Invocar yylex(), que procesa el archivo y emite cada token.
 *     4. Al terminar, imprimir la tabla de símbolos via sym_print().
 *
 * RELACION CON OTROS ARCHIVOS:
 *   - scanner.l      — genera lex.yy.c que define yylex() y yyin
 *   - symbol_table.h — declara sym_print() que se llama al finalizar
 *   - symbol_table.c — implementa sym_print()
 *   - token_ids.h    — constantes TK_* (recibidas vía symbol_table.h)
 *
 * COMPILACION (desde src/):
 *   flex scanner.l
 *   gcc lex.yy.c symbol_table.c main.c -o scanner -lfl
 *
 * USO:
 *   ./scanner programa.sl       <- desde archivo
 *   ./scanner < programa.sl     <- desde stdin
 * ===========================================================================
 */

#include <stdio.h>
#include <stdlib.h>
#include "symbol_table.h"

/* Declaraciones externas exportadas por lex.yy.c (generado por flex) */
extern int   yylex(void);   /* función principal del scanner              */
extern FILE *yyin;          /* archivo de entrada del scanner             */
extern int   yylineno;      /* contador de líneas (con %option yylineno)  */

int main(int argc, char *argv[])
{
    /* Abrir archivo si se pasó como argumento; de lo contrario usar stdin */
    if (argc == 2) {
        yyin = fopen(argv[1], "r");
        if (!yyin) {
            fprintf(stderr, "Error: no se puede abrir '%s'\n", argv[1]);
            return 1;
        }
    }

    /* Encabezado de la tabla de tokens */
    printf("+=======+================+===========================+=======+\n");
    printf("|          SCANNER LEXICO — Lista de Tokens                  |\n");
    printf("+=======+================+===========================+=======+\n");
    printf("| %-5s | %-14s | %-25s | %-5s |\n",
           "ID", "Nombre", "Lexema", "Linea");
    printf("+-------+----------------+---------------------------+-------+\n");

    /* Invocar el scanner hasta EOF */
    yylex();

    /* Línea de cierre de la tabla */
    printf("+=======+================+===========================+=======+\n");

    if (argc == 2) fclose(yyin);

    /* Imprimir tabla de símbolos */
    sym_print();

    printf("\n[OK] Analisis lexico completado.\n");
    return 0;
}
