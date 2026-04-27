# Preguntas de Extensión — Analizador Léxico con AFD

Estas preguntas son para profundizar en el tema después de implementar el lexer base.

---

## 1. Agregar operadores aritméticos `+ - * /`

### ¿Qué estados nuevos se necesitan?

El AFD para operadores aritméticos es el más simple posible: **un solo carácter** siempre es un token completo, por lo que solo necesita **2 estados**:

```
q0 --[+|-|*|/]--> q1*
```

| Estado | `+` | `-` | `*` | `/`  | otro  | Acción  |
|--------|-----|-----|-----|------|-------|---------|
| q0 >   | →q1 | →q1 | →q1 | →q1  | ERROR | inicio  |
| [q1 *] | EMIT| EMIT| EMIT| EMIT | EMIT  | aceptar |

> **Cuidado con `/`:** el lexer actual usa `/` para `LINE_COMMENT`. Hay que aplicar **maximal munch**: si después del `/` viene otro `/`, gana el automata de comentarios; si no, gana el aritmético. Basta con ordenar `LineCommentAutomata` **antes** que `ArithOpAutomata` en la lista del Lexer.

### Pasos concretos en el código

1. Agregar `ARITH_OP` a `TokenType`.
2. Crear `ArithOpAutomata : Automata` con la tabla anterior y los CharClass `Plus`, `Minus`, `Star` (ampliar el enum `CharClass`).
3. Agregar `CharClass.Slash` como transición en el nuevo automata.
4. Insertar la instancia **después** de `LineCommentAutomata` en `Lexer._automata`.
5. Agregar el patrón regex de validación cruzada: `@"^[+\-*/]$"`.

---

## 2. AFD vs solución naíve con `if-else` anidados

### Diferencia conceptual

| Aspecto | `if-else` anidados | AFD explícito |
|---|---|---|
| Estructura | Código imperativo, ramas ad hoc | Tabla de transiciones declarativa |
| Mantenimiento | Agregar un token = reescribir ramas | Agregar filas a la tabla |
| Corrección | Difícil de verificar formalmente | Se puede probar exhaustivamente |
| Rendimiento | Igual en teoría (O(n)) | Igual en teoría, pero la tabla puede cachearse |

### Comparación de tiempo de reconocimiento

Ambos son **O(n)** en el tamaño de la entrada, porque recorren cada carácter exactamente una vez. La diferencia práctica está en:

- **if-else**: muchas comparaciones de caracteres en cascada por cada posición.
- **AFD con tabla**: una sola búsqueda en diccionario (`O(1)` amortizado) por carácter.

Para medir en este proyecto:

```csharp
using System.Diagnostics;

var sw = Stopwatch.StartNew();
for (int i = 0; i < 100_000; i++)
    new Lexer(codigoGrande).Tokenize();
sw.Stop();
Console.WriteLine($"AFD: {sw.ElapsedMilliseconds} ms");
```

En benchmarks reales el AFD con diccionario suele ser **2-5× más rápido** que if-else equivalente para vocabularios grandes, gracias al CPU branch predictor y la localidad de caché de la tabla.

---

## 3. Herramientas que generan AFD automáticamente

### ANTLR 4

ANTLR toma una **gramática `.g4`** y genera:
- Un **lexer** (AFD para tokens).
- Un **parser** (LALR/LL(*) para gramática).

```antlr
// fragmento de gramática .g4
INT     : [0-9]+ ;
REAL    : [0-9]+ '.' [0-9]+ ;
ID      : [a-zA-Z_][a-zA-Z0-9_]* ;
```

ANTLR convierte cada regla léxica en un AFD y los combina en un gran AFD determinista minimizado. Lo que tú implementaste a mano es exactamente lo que ANTLR genera automáticamente.

### Lex / Flex (C/C++)

```lex
[0-9]+\.[0-9]+   { return REAL; }
[0-9]+           { return INT;  }
[a-zA-Z_][a-zA-Z0-9_]*  { return ID; }
```

Flex convierte estas expresiones regulares en un **AFD minimizado** usando el algoritmo de Thompson (ER → AFN) + construcción de subconjuntos (AFN → AFD) + minimización de Hopcroft.

### Roslyn Scripting API (.NET)

El compilador de C# (Roslyn) usa internamente un **lexer manual** altamente optimizado (no usa Regex) que implementa exactamente el mismo principio de AFD. Puedes acceder a él vía:

```csharp
using Microsoft.CodeAnalysis.CSharp;

var tree = CSharpSyntaxTree.ParseText("int x = 42;");
foreach (var token in tree.GetRoot().DescendantTokens())
    Console.WriteLine($"{token.Kind()}: {token.Text}");
```

### Conexión con lo que implementaste

Lo que hiciste a mano en `Automata.cs` es **equivalente** a lo que estas herramientas generan:

```
Expresión Regular  →  AFN (Thompson)  →  AFD (subconjuntos)  →  AFD mínimo
tu clase Automata                         tu Dictionary<(state,cls),int>
```

---

## 4. ¿Qué pasaría si los identificadores permitieran puntos? (`mi.Objeto`)

### Problema

Con la gramática actual, `mi.Objeto` se tokeniza como:
```
IDENTIFIER "mi"
UNKNOWN    "."
IDENTIFIER "Objeto"
```

Si el lenguaje permite `mi.Objeto` como un solo token (estilo acceso a miembro en el léxico), el AFD del identificador debe cambiar.

### AFD modificado

```
q0 --[letra|_]--> q1* --[letra|digit|_]--> q1*
                        |
                        v
                       [.] --> q2 --[letra|_]--> q1*
```

| Estado | letra/_ | dígito | `.`  | otro | Acción       |
|--------|---------|--------|------|------|--------------|
| q0 >   | → q1   | ERROR  |ERROR | ERROR| inicio       |
| [q1 *] | → q1   | → q1   | → q2 | EMIT | bucle        |
| q2     | → q1   | ERROR  |ERROR | FAIL | tras punto   |

> Estado `q2` es **no aceptante**: `mi.` solo no es válido; requiere al menos una letra después del punto.

### Conflicto con REAL

Si `3.14` también es válido, el punto es **ambiguo** para el lexer:
- `3.14` → REAL
- `mi.Objeto` → IDENTIFIER con punto

La solución estándar es resolver la ambigüedad en el **contexto**:
- Si el estado anterior fue `DIGIT → q1`, el punto inicia la parte decimal.
- Si el estado anterior fue `LETTER → q1`, el punto es separador de miembro.

Esto se puede implementar manteniendo el **contexto del carácter anterior** en el lexer, o creando dos automatas separados con prioridad (REAL antes que IDENTIFIER-con-punto).

### Implicaciones generales

- El AFD pasa de 2 estados a **3 estados** mínimo.
- El lexer ahora emite tokens como `"mi.Objeto"` que el parser deberá descomponer, **o** el parser maneja el punto como operador de acceso (enfoque más común en lenguajes reales como Java/C#).
- Lenguajes como Python usan el segundo enfoque: el punto es siempre un operador separado, nunca parte del identificador.
