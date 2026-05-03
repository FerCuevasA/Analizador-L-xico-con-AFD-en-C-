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

## Implementación en C# — AFD Manual

**Ubicación:** `analizador_C#/`

### ¿Cómo funciona?

Implementa el reconocimiento mediante **Autómatas Finitos Deterministas (AFD)** codificados a mano. Cada tipo de token tiene su propio autómata independiente. El componente `Lexer` los coordina: lee carácter a carácter y decide qué autómata ejecutar según el símbolo actual.

Después de reconocer cada token, lo valida cruzadamente con su expresión regular equivalente (solo para verificación, no para tokenizar).

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

### Cómo correrlo

Requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
cd analizador_C#/Analizador_C#/LexerProject
dotnet build
dotnet run --project LexerProject
```

Al arrancar muestra un menú. Elegir la opción `2` para ingresar texto propio, o la opción `1` para correr el ejemplo predefinido.

Para correr las pruebas unitarias:

```bash
dotnet test
```

### Prueba con `int precio = 3.14; if (precio >= 0.0) // ok`

Seleccionar opción `2`, pegar la línea y dejar una línea vacía para terminar.

Salida:

```
Pos   Token            Lexema       Linea  Col
----------------------------------------------------
0     KEYWORD          "int"        1      1
4     IDENTIFIER       "precio"     1      5
11    REL_OP           "="          1      12
13    REAL             "3.14"       1      14
17    UNKNOWN          ";"          1      18    <-- ERROR LEXICO
19    KEYWORD          "if"         1      20
22    UNKNOWN          "("          1      23    <-- ERROR LEXICO
23    IDENTIFIER       "precio"     1      24
30    REL_OP           ">="         1      31
33    REAL             "0.0"        1      34
36    UNKNOWN          ")"          1      37    <-- ERROR LEXICO
38    LINE_COMMENT     "// ok"      1      39
```

> Los tokens marcados como `UNKNOWN` son errores léxicos esperados: este analizador no tiene definidos `;`, `(` ni `)` dentro de su conjunto de tokens.

---

## Implementación en C — flex + C99

**Ubicación:** `analizador_C#/Analizador_C/lenguaje_simple/`

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

### Cómo correrlo

Requisitos: `flex` y `gcc`. En Windows se necesita **WSL** o una terminal Linux.

```bash
cd analizador_C#/Analizador_C/lenguaje_simple/src
make
```

Para ejecutar con un archivo de prueba:

```bash
./scanner ../examples/test_valid.sl      # programa de prueba válido
./scanner ../examples/test_errors.sl     # programa con errores léxicos
```

### Prueba con `int precio = 3.14; if (precio >= 0.0) // ok`

Crear un archivo con el contenido de la prueba y pasárselo al scanner:

```bash
echo "int precio = 3.14; if (precio >= 0.0) // ok" > prueba.sl
./scanner prueba.sl
```

Salida esperada:

```
=== TOKEN TABLE ===
ID    Nombre          Lexema        Linea
---   ----------      ----------    -----
100   KEYWORD         int           1
200   IDENTIFIER      precio        1
320   ASSIGN_OP       =             1
202   FLOAT_LIT       3.14          1
400   SEMICOLON       ;             1
100   KEYWORD         if            1
401   LPAREN          (             1
200   IDENTIFIER      precio        1
312   GEQ             >=            1
202   FLOAT_LIT       0.0           1
402   RPAREN          )             1
500   COMMENT         // ok         1

=== SYMBOL TABLE ===
[0] precio  (ID)
[1] 3.14    (FLOAT_LIT)
[2] 0.0     (FLOAT_LIT)
```

---

## Estructura del repositorio

```
compilador/
├── analizador_C#/                  ← repo git principal
│   ├── Analizador_C#/              ← implementación C# con AFD manual (.NET 8)
│   │   └── LexerProject/
│   │       ├── LexerProject/       ← código fuente
│   │       └── LexerProject.Tests/ ← pruebas unitarias
│   ├── Analizador_C/               ← implementación C con flex + C99
│   │   └── lenguaje_simple/
│   │       ├── src/                ← scanner.l, main.c, symbol_table
│   │       ├── examples/           ← archivos .sl de prueba
│   │       └── docs/
│   └── recursos/                   ← material de referencia del curso
│
└── analizador_C/                   ← copia local del analizador C
    └── lenguaje_simple/
```
