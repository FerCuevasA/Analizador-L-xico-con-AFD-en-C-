# Analizador Léxico — Python

Implementación del analizador léxico en Python puro (sin librerías externas), equivalente a la versión C# de este repositorio. Usa Autómatas Finitos Deterministas (AFD) codificados a mano.

## Cómo correrlo

```bash
cd Analizador_Python
bash run.sh
```

Requiere Python 3.8 o superior. El script `run.sh` lo detecta automáticamente.

## Cómo funciona

### 1. Clasificación de caracteres

El AFD no trabaja con caracteres individuales (`'a'`, `'3'`, `'<'`) sino con **clases**. La función `clasificar(c)` convierte cada carácter en su clase:

| Clase | Caracteres |
|---|---|
| `LETTER` | a-z, A-Z |
| `DIGIT` | 0-9 |
| `UNDERSCORE` | `_` |
| `QUOTE` | `"` |
| `LT`, `GT`, `EQ`, `BANG` | `<`, `>`, `=`, `!` |
| `SLASH`, `DOT` | `/`, `.` |
| `NEWLINE`, `SPACE` | `\n`, espacios, tabs |
| `OTHER` | todo lo demás (`;`, `(`, `+`, etc.) |

Esto reduce las tablas de transición: en vez de una fila por carácter, una fila por clase.

### 2. Autómata base y maximal-munch

Cada autómata es un diccionario de transiciones:

```
{ (estado_actual, clase_char): estado_siguiente }
```

El método `ejecutar()` implementa **maximal-munch**: avanza mientras haya una transición válida y guarda la última posición donde estuvo en un estado aceptante. Esto garantiza que `>=` se reconoce completo y no como `>` + `=`.

```
Ejemplo: AutomataReal sobre "3.14xyz"
  '3' DIGIT  → estado 1
  '.' DOT    → estado 2
  '1' DIGIT  → estado 3 ✓ (aceptante, guarda posición)
  '4' DIGIT  → estado 3 ✓ (aceptante, guarda posición)
  'x' LETTER → sin transición, para
  → retorna "3.14"
```

### 3. Autómatas por tipo de token

| Autómata | Reconoce | Estados |
|---|---|---|
| `AutomataIdentificador` | `precio`, `x`, `_var` | q0 → q1\* |
| `AutomataEntero` | `42`, `0` | q0 → q1\* |
| `AutomataReal` | `3.14`, `0.5` | q0 → q1 → q2 → q3\* |
| `AutomataRelOp` | `<` `<=` `>` `>=` `=` `==` `!=` | 9 estados |
| `AutomataCadena` | `"hola mundo"` | q0 → q1 → q2\* |
| `AutomataComentario` | `// comentario` | q0 → q1 → q2\* → q3\* |
| `AutomataEspacio` | espacios, tabs, `\n` | q0 → q1\* |

> `*` = estado de aceptación

### 4. El Lexer (`tokenizar`)

Recorre el código fuente posición a posición. En cada posición prueba los autómatas **en orden**:

```
[AutomataReal, AutomataEntero, AutomataIdentificador, AutomataRelOp,
 AutomataCadena, AutomataComentario, AutomataEspacio]
```

El primero que reconozca algo gana. El orden importa: **AutomataReal va antes que AutomataEntero** para que `3.14` se lea como REAL y no como INTEGER `3` + punto + INTEGER `14`.

Si ningún autómata reconoce el carácter actual, se emite un token `UNKNOWN` (error léxico) y se avanza un carácter.

Después del reconocimiento, si el token es `IDENTIFIER` y el lexema está en la lista de palabras reservadas, su tipo cambia a `KEYWORD`.

### 5. Flujo completo

```
Código fuente (string)
        │
        ▼
  clasificar(c)           → convierte cada char en su clase
        │
        ▼
  ejecutar(fuente, pos)   → maximal-munch con la tabla de transiciones
        │
        ├── WHITESPACE    → descartado (solo mueve el cursor)
        ├── IDENTIFIER    → si está en KEYWORDS, se reclasifica
        └── resto         → Token(tipo, lexema, línea, columna, pos)
        │
        ▼
  Lista de tokens
```

## Tokens reconocidos

| Tipo | Ejemplos |
|---|---|
| `KEYWORD` | `if` `else` `while` `for` `int` `float` `return` `void` `class` |
| `IDENTIFIER` | `x` `precio1` `_var` |
| `INTEGER` | `42` `0` `100` |
| `REAL` | `3.14` `0.5` `100.0` |
| `REL_OP` | `<` `<=` `>` `>=` `=` `==` `!=` |
| `STRING` | `"hola"` `"precio 1"` |
| `LINE_COMMENT` | `// comentario` |
| `UNKNOWN` | `;` `(` `)` `+` (error léxico) |

## Ejemplo de salida

Entrada: `int precio = 3.14; if (precio >= 0.0) // ok`

```
Pos   Token           Lexema          Línea   Col
---------------------------------------------------------
0     KEYWORD         'int'           1       1
4     IDENTIFIER      'precio'        1       5
11    REL_OP          '='             1       12
13    REAL            '3.14'          1       14
17    UNKNOWN         ';'             1       18  <-- ERROR LÉXICO
19    KEYWORD         'if'            1       20
22    UNKNOWN         '('             1       23  <-- ERROR LÉXICO
23    IDENTIFIER      'precio'        1       24
30    REL_OP          '>='            1       31
33    REAL            '0.0'           1       34
36    UNKNOWN         ')'             1       37  <-- ERROR LÉXICO
38    LINE_COMMENT    '// ok'         1       39
```

> Los `UNKNOWN` son esperados: este lenguaje de prueba no tiene definidos `;`, `(` ni `)`.

## Estructura del archivo

```
analizador.py
├── Paso 1: clasificar(c)              → clases de caracteres
├── Paso 2: class Token                → estructura de un token
├── Paso 3: class Automata             → AFD genérico + maximal-munch
├── Paso 4: AutomataXxx (×7)           → un AFD por tipo de token
├── Paso 5: tokenizar(fuente)          → el lexer principal
└── Paso 6: imprimir_tabla + menu()    → interfaz de usuario
```
