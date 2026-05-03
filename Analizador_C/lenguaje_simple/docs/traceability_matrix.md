# Matriz de Trazabilidad — Scanner Léxico, Lenguaje Académico Simple

## Requisitos Funcionales → Diseño → Código

| ID    | Requisito Funcional                                               | Diseño                                     | Archivo / Línea de Código                                     |
|-------|-------------------------------------------------------------------|--------------------------------------------|---------------------------------------------------------------|
| RF-01 | Reconocer palabras reservadas                                     | Tokens KW_* (ID 100–109); reglas literales | `scanner.l`: reglas `"if"` … `"false"` (antes que ID)        |
| RF-02 | Reconocer identificadores                                         | Token IDENTIFIER (ID 200); AFD-1           | `scanner.l`: regla `{ID}` con macro `LETTER`/`DIGIT`          |
| RF-03 | Reconocer literales enteros                                       | Token INT_LIT (ID 201); AFD-2              | `scanner.l`: regla `{INT}` con macro `DIGIT+`                 |
| RF-04 | Reconocer literales flotantes                                     | Token FLOAT_LIT (ID 202); AFD-2            | `scanner.l`: regla `{FLOAT}` antes que `{INT}` (max-munch)    |
| RF-05 | Reconocer operadores aritméticos (+, -, *, /)                     | Tokens OP_* (ID 300–303)                   | `scanner.l`: reglas `"+"` `"-"` `"*"` `"/"`                  |
| RF-06 | Reconocer operadores relacionales (==, !=, <, >, <=, >=)          | Tokens OP_* (ID 310–315); AFD-4            | `scanner.l`: reglas `"=="` `"!="` `"<="` `">="` `"<"` `">"`  |
| RF-07 | Reconocer operadores de asignación (=, +=, -=, *=, /=)           | Tokens OP_* (ID 320–324); AFD-4            | `scanner.l`: reglas `"+="` `"-="` `"*="` `"/="` `"="`        |
| RF-08 | Reconocer símbolos especiales ( ) { } , ; .                       | Tokens SYM_* (ID 400–406)                  | `scanner.l`: reglas `"("` `")"` `"{"` `"}"` `","` `";"` `"."`|
| RF-09 | Reconocer comentarios de línea (//)                               | Token COMMENT (ID 500); AFD-3              | `scanner.l`: regla `{COMMENT}` con macro `\/\/[^\n]*`         |
| RF-10 | Ignorar espacios en blanco y saltos de línea                      | Macros BLANK y `\n` sin acción            | `scanner.l`: reglas `{BLANK}` y `\n` con acción vacía        |
| RF-11 | Reportar error léxico con número de línea                         | Función `lex_error()`; regla `.` final     | `scanner.l`: regla `.` llama `lex_error(yytext)` en stderr    |
| RF-12 | Proporcionar lista de tokens del archivo fuente                   | Función `emit()`; encabezado en main       | `scanner.l`: `emit()` en cada acción; `main.c`: encabezado    |
| RF-13 | Proporcionar tabla de símbolos para ID, INT_LIT y FLOAT_LIT       | Módulo `symbol_table`; `SymEntry`          | `symbol_table.h/.c`: `sym_insert()` + `sym_print()`           |
| RF-14 | El software debe correr desde línea de comandos                   | Programa principal con apertura de archivo | `main.c`: `argc`/`argv`, `fopen()`, `yylex()`, `sym_print()`  |

---

## Rúbrica → Implementación

| Criterio de Rúbrica                                              | Cumplimiento                                                               |
|------------------------------------------------------------------|----------------------------------------------------------------------------|
| El software corre correctamente                                  | Compilable con `flex` + `gcc -lfl`; Makefile incluido                     |
| El scanner reconoce todos los tokens definidos                   | 35 tipos de token cubiertos en `scanner.l`                                 |
| Reconoce símbolos inválidos y muestra mensaje de error           | Regla `.` en `scanner.l` → `lex_error()` → stderr con línea               |
| Proporciona lista de tokens del archivo fuente                   | `emit()` llamada en cada acción; tabla formateada en `main.c`              |
| Proporciona tabla de símbolos                                    | `sym_insert()` en acciones de ID/INT/FLOAT; `sym_print()` al finalizar    |
| Código fuente muy bien documentado                               | Bloque de descripción en los 5 archivos; comentarios por sección en `.l`  |
| Cada archivo incluye descripción y relación con otros            | Sección "RELACION CON OTROS ARCHIVOS" en todos los archivos fuente         |
| Todas las funciones explicadas                                   | `emit`, `lex_error`, `sym_insert`, `sym_print`, `main` documentadas        |
| Comentarios claros y correctos                                   | Comentarios explican el *por qué* (orden de reglas, max-munch, etc.)      |
| Trazabilidad requisito → código                                  | Esta matriz + comentarios de sección en `scanner.l` por categoría          |
| El código cumple con el diseño                                   | Token IDs en `token_ids.h` coinciden con la Tabla de Tokens del documento  |
| El diseño se mapea con los entregables                           | Cada token de `especificacion_lexica.md` aparece como regla en `scanner.l` |

---

## Correspondencia Token Name → Constante → Valor

| Nombre (Diseño)  | Constante en token_ids.h | Valor |
|------------------|--------------------------|-------|
| KW_IF            | `TK_IF`                  | 100   |
| KW_ELSE          | `TK_ELSE`                | 101   |
| KW_WHILE         | `TK_WHILE`               | 102   |
| KW_FOR           | `TK_FOR`                 | 103   |
| KW_INT           | `TK_INT`                 | 104   |
| KW_FLOAT         | `TK_FLOAT`               | 105   |
| KW_RETURN        | `TK_RETURN`              | 106   |
| KW_VOID          | `TK_VOID`                | 107   |
| KW_TRUE          | `TK_TRUE`                | 108   |
| KW_FALSE         | `TK_FALSE`               | 109   |
| IDENTIFIER       | `TK_ID`                  | 200   |
| INT_LIT          | `TK_INT_LIT`             | 201   |
| FLOAT_LIT        | `TK_FLT_LIT`             | 202   |
| OP_PLUS          | `TK_PLUS`                | 300   |
| OP_MINUS         | `TK_MINUS`               | 301   |
| OP_MULT          | `TK_MULT`                | 302   |
| OP_DIV           | `TK_DIV`                 | 303   |
| OP_EQ            | `TK_EQ`                  | 310   |
| OP_NEQ           | `TK_NEQ`                 | 311   |
| OP_LT            | `TK_LT`                  | 312   |
| OP_GT            | `TK_GT`                  | 313   |
| OP_LEQ           | `TK_LEQ`                 | 314   |
| OP_GEQ           | `TK_GEQ`                 | 315   |
| OP_ASSIGN        | `TK_ASSIGN`              | 320   |
| OP_PASSIGN       | `TK_PASSIGN`             | 321   |
| OP_MASSIGN       | `TK_MASSIGN`             | 322   |
| OP_STASSIGN      | `TK_STASSIGN`            | 323   |
| OP_DASSIGN       | `TK_DASSIGN`             | 324   |
| SYM_LPAREN       | `TK_LPAREN`              | 400   |
| SYM_RPAREN       | `TK_RPAREN`              | 401   |
| SYM_LBRACE       | `TK_LBRACE`              | 402   |
| SYM_RBRACE       | `TK_RBRACE`              | 403   |
| SYM_COMMA        | `TK_COMMA`               | 404   |
| SYM_SEMI         | `TK_SEMI`                | 405   |
| SYM_DOT          | `TK_DOT`                 | 406   |
| COMMENT          | `TK_COMMENT`             | 500   |
