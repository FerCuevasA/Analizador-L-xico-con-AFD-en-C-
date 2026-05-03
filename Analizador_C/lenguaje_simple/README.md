# Scanner Léxico — Lenguaje Académico Simple

Analizador léxico para un lenguaje de programación de propósito educativo,
implementado con UNIX flex y C estándar (C99).

## Estructura del proyecto

```
lenguaje_simple/
├── src/
│   ├── scanner.l        ← especificación flex (archivo principal)
│   ├── token_ids.h      ← IDs numéricos de todos los tokens
│   ├── symbol_table.h   ← interfaz de la tabla de símbolos
│   ├── symbol_table.c   ← implementación de la tabla de símbolos
│   ├── main.c           ← programa principal
│   └── Makefile
├── examples/
│   ├── test_valid.sl    ← programa de prueba sin errores
│   └── test_errors.sl   ← programa con errores léxicos intencionales
└── docs/
    ├── especificacion_lexica.md   ← especificación completa
    └── traceability_matrix.md     ← matriz de trazabilidad
```

## Arquitectura

El scanner sigue un pipeline de tres capas:

```
Archivo fuente (.sl)
        │
        ▼
  [main.c]
  Lee el archivo y dirige la entrada al scanner.
  Al finalizar, imprime la tabla de tokens y la tabla de símbolos.
        │
        ▼
  [scanner.l → lex.yy.c (generado por flex)]
  Define patrones regex para cada categoría de token.
  Cada regla ejecuta una acción: registrar el token y/o
  insertarlo en la tabla de símbolos.
        │
        ▼
  [symbol_table.c]
  Almacena identificadores y literales numéricos encontrados.
  Capacidad máxima: 512 entradas.
```

**Flujo de compilación del scanner:**

```
scanner.l  ──flex──▶  lex.yy.c
                            │
symbol_table.c ──────────── │
main.c ──────────────────── ┤
                            ▼
                     gcc → ./scanner
```

Cada token tiene un ID numérico definido en `token_ids.h`. Cuando flex
empareja un patrón, ejecuta la acción C asociada que imprime el token y,
si es un identificador o literal, lo registra en la tabla de símbolos.

## Compilación

Requisitos: `flex` y `gcc` instalados.

```bash
cd src/
make
```

## Uso

```bash
./scanner ../examples/test_valid.sl    # analiza el archivo válido
./scanner ../examples/test_errors.sl   # prueba el manejo de errores
```

## Tokens del lenguaje

| Rango ID | Categoría               | Ejemplos                   |
|----------|-------------------------|----------------------------|
| 100–109  | Palabras reservadas     | if, else, while, int, float|
| 200–202  | Identificadores/literales| x, 42, 3.14               |
| 300–303  | Operadores aritméticos  | + - * /                    |
| 310–315  | Operadores relacionales | == != < > <= >=            |
| 320–324  | Operadores de asignación| = += -= *= /=              |
| 400–406  | Símbolos especiales     | ( ) { } , ; .              |
| 500      | Comentarios             | // texto                   |

## Salida

- **stdout**: tabla de tokens con columnas ID | Nombre | Lexema | Línea
- **stdout**: tabla de símbolos al finalizar (solo ID, INT_LIT, FLOAT_LIT)
- **stderr**: mensajes de error léxico con número de línea
