# Analizadores Léxicos — Diseño de Compiladores

Este repositorio contiene dos implementaciones de un analizador léxico desarrolladas para el curso de Diseño de Compiladores, cada una en un lenguaje distinto.

---

## ¿Qué es un analizador léxico?

El analizador léxico es la primera etapa de un compilador. Su función es leer el código fuente carácter a carácter y agruparlo en **tokens**: las unidades mínimas con significado (palabras reservadas, identificadores, operadores, literales, etc.).

Cada token tiene:
- Un **tipo** (ej. `KEYWORD`, `IDENTIFIER`, `INTEGER`)
- Un **lexema**: el texto exacto que lo forma (ej. `if`, `precio1`, `42`)
- Una **posición**: línea y columna donde aparece en el fuente

---

## Tokens reconocidos (ambos analizadores)

Ambas implementaciones reconocen el mismo conjunto base de tokens:

| Categoría | Ejemplos |
|---|---|
| Palabras reservadas | `if else while for int float return void` |
| Identificadores | `x`, `precio1`, `miVariable` |
| Enteros | `42`, `0`, `100` |
| Reales | `3.14`, `0.5` |
| Operadores aritméticos | `+ - * /` |
| Operadores relacionales | `== != < > <= >=` |
| Operadores de asignación | `= += -= *= /=` |
| Símbolos especiales | `( ) { } , ; .` |
| Comentarios de línea | `// texto` |
| Error léxico | cualquier carácter no reconocido |

---

## Salida (ambos analizadores)

Los dos producen la misma estructura de salida en `stdout`:

```
=== TOKEN TABLE ===
ID    Nombre          Lexema        Linea
---   ----------      ----------    -----
100   KEYWORD         if            3
200   IDENTIFIER      x             3
201   INTEGER         42            4
...

=== SYMBOL TABLE ===
(identificadores y literales numéricos encontrados)
```

Los errores léxicos se reportan en `stderr` con número de línea.

---

## Implementación en C# — AFD Manual

**Ubicación:** `analizador_C#/`

### ¿Cómo funciona?

Implementa el reconocimiento mediante **Autómatas Finitos Deterministas (AFD)** codificados a mano. Cada tipo de token tiene su propio autómata independiente. El componente `Lexer` los coordina: lee carácter a carácter y decide qué autómata ejecutar según el símbolo actual.

Después de reconocer cada token, lo **valida cruzadamente** con su expresión regular equivalente (solo para verificación, no para tokenizar).

### Arquitectura

```
Entrada (string)
      │
      ▼
  [Lexer.cs]  ←  coordina todos los autómatas
      │
      ├──▶ [IdentifierAutomata]   → IDENTIFIER / KEYWORD
      ├──▶ [IntegerAutomata]      → INTEGER
      ├──▶ [RealAutomata]         → REAL
      ├──▶ [RelOpAutomata]        → REL_OP  (con lookahead)
      ├──▶ [StringAutomata]       → STRING
      ├──▶ [LineCommentAutomata]  → LINE_COMMENT
      └──▶ [WhitespaceAutomata]   → (descartado)
      │
      ▼
  Lista de Token { Tipo, Lexema, Línea, Columna, Posición }
```

Cada autómata implementa:
- Estado inicial y estados de aceptación
- Función de transición δ(q, c) → q'
- Principio de **máximo consumo** (maximal munch): siempre captura el lexema más largo posible

### Tokens adicionales (solo C#)

| Token | Patrón |
|---|---|
| `STRING` | `"[^"\n]*"` — cadenas entre comillas dobles |
| `KEYWORD` | subconjunto de IDENTIFIER: `if else while for int float string return void class` |
| `WHITESPACE` | espacios, tabs y saltos de línea (consumidos y descartados) |
| `UNKNOWN` | cualquier símbolo no reconocido |

### Compilación y ejecución

Requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
cd analizador_C#/LexerProject
dotnet build
dotnet run --project LexerProject
```

Para correr las pruebas unitarias:

```bash
dotnet test
```

### Ejemplo de salida

Entrada: `int x = 3.14; // valor`

```
Pos   Token         Lexema        Linea  Col
0     KEYWORD       int           1      1
4     IDENTIFIER    x             1      5
6     REL_OP        =             1      7
8     REAL          3.14          1      9
12    UNKNOWN       ;             1      13
14    LINE_COMMENT  // valor      1      15
```

---

## Implementación en C — flex + C99

**Ubicación:** `analizador_C/lenguaje_simple/`

### ¿Cómo funciona?

Usa **flex** para generar automáticamente el scanner a partir de reglas escritas como expresiones regulares en el archivo `scanner.l`. Cuando flex empareja un patrón, ejecuta la acción C asociada: imprime el token y, si es un identificador o literal, lo registra en la tabla de símbolos.

### Arquitectura

```
Archivo fuente (.sl)
      │
      ▼
  [main.c]
  Abre el archivo y dirige la entrada al scanner.
  Al terminar imprime la tabla de tokens y la tabla de símbolos.
      │
      ▼
  [scanner.l  →  lex.yy.c  (generado por flex)]
  Reglas regex con acciones C para cada token.
      │
      ▼
  [symbol_table.c]
  Tabla de hasta 512 entradas.
  Almacena identificadores y literales numéricos.
```

**Pipeline para generar el ejecutable:**

```
scanner.l ──flex──▶ lex.yy.c ──┐
symbol_table.c ────────────────┤── gcc ──▶ ./scanner
main.c ────────────────────────┘
```

### Compilación y ejecución

Requisitos: `flex` y `gcc`

```bash
cd analizador_C/lenguaje_simple/src
make
```

Para ejecutar:

```bash
./scanner ../examples/test_valid.sl      # programa de prueba válido
./scanner ../examples/test_errors.sl     # programa con errores léxicos
```

---

## Estructura del repositorio

```
compilador/
├── analizador_C#/              ← implementación con AFD en C# (.NET 8)
│   ├── LexerProject/
│   │   ├── LexerProject/       ← código fuente principal
│   │   └── LexerProject.Tests/ ← pruebas unitarias
│   └── README.md
│
└── analizador_C/               ← implementación con flex en C99
    └── lenguaje_simple/
        ├── src/                ← scanner.l, main.c, symbol_table
        ├── examples/           ← archivos .sl de prueba
        └── docs/               ← especificación léxica y trazabilidad
```
