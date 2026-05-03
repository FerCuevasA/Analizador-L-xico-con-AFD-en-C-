# Especificación Léxica — Lenguaje Académico Simple

## 1. Descripción Informal del Lenguaje

El **Lenguaje Académico Simple** (extensión `.sl`) es un lenguaje de programación de propósito educativo con sintaxis similar a C. Su alcance cubre la declaración de funciones, variables enteras y flotantes, instrucciones de control de flujo (`if/else`, `while`, `for`), operaciones aritméticas básicas y asignación. Su diseño es deliberadamente reducido para demostrar el funcionamiento de un analizador léxico sin complicaciones innecesarias.

Un programa típico en este lenguaje tiene la siguiente forma:

```c
// Función que calcula la suma de dos enteros
int suma(int a, int b) {
    return a + b;
}
```

---

## 2. Descripción Informal de los Lexemas

### 2.1 Palabras Reservadas

Identificadores con significado fijo. No pueden ser usadas como nombres de variables o funciones.

| Token    | Lexema    | Descripción                              |
|----------|-----------|------------------------------------------|
| KW_IF    | `if`      | Inicio de estructura condicional         |
| KW_ELSE  | `else`    | Rama alternativa del condicional         |
| KW_WHILE | `while`   | Bucle con condición al inicio            |
| KW_FOR   | `for`     | Bucle con inicialización y paso          |
| KW_INT   | `int`     | Tipo de dato entero                      |
| KW_FLOAT | `float`   | Tipo de dato flotante                    |
| KW_RETURN| `return`  | Retorno de valor desde una función       |
| KW_VOID  | `void`    | Tipo de retorno vacío                    |
| KW_TRUE  | `true`    | Valor booleano verdadero                 |
| KW_FALSE | `false`   | Valor booleano falso                     |

### 2.2 Identificadores

Nombre dado a variables y funciones. Comienzan con letra o guion bajo (`_`) y pueden contener letras, dígitos o guiones bajos en las posiciones siguientes.

- **Válidos:** `x`, `contador`, `_temp`, `valor1`, `mi_variable`
- **Inválidos:** `1abc` (comienza con dígito), `mi-var` (guion medio), `@flag` (carácter especial)

### 2.3 Literales Enteros

Secuencia de uno o más dígitos decimales (0–9). Representan valores numéricos sin parte fraccionaria.

- **Ejemplos:** `0`, `1`, `42`, `1024`

### 2.4 Literales Flotantes

Secuencia de dígitos, seguida de un punto decimal, seguida de al menos un dígito. Se requiere al menos un dígito en cada lado del punto.

- **Válidos:** `3.14`, `0.5`, `100.0`
- **Inválidos:** `.5` (sin dígito inicial), `3.` (sin dígito final)

### 2.5 Operadores Aritméticos

Operan sobre valores numéricos.

| Token    | Símbolo | Operación      |
|----------|---------|----------------|
| OP_PLUS  | `+`     | Suma           |
| OP_MINUS | `-`     | Resta          |
| OP_MULT  | `*`     | Multiplicación |
| OP_DIV   | `/`     | División       |

### 2.6 Operadores Relacionales

Comparan dos valores y producen un resultado booleano.

| Token  | Símbolo | Operación            |
|--------|---------|----------------------|
| OP_EQ  | `==`    | Igual a              |
| OP_NEQ | `!=`    | Distinto de          |
| OP_LT  | `<`     | Menor que            |
| OP_GT  | `>`     | Mayor que            |
| OP_LEQ | `<=`    | Menor o igual que    |
| OP_GEQ | `>=`    | Mayor o igual que    |

### 2.7 Operadores de Asignación

Asignan un valor a una variable. El operador compuesto combina una operación aritmética con la asignación.

| Token      | Símbolo | Significado           |
|------------|---------|-----------------------|
| OP_ASSIGN  | `=`     | Asignación simple     |
| OP_PASSIGN | `+=`    | Suma y asigna         |
| OP_MASSIGN | `-=`    | Resta y asigna        |
| OP_STASSIGN| `*=`    | Multiplica y asigna   |
| OP_DASSIGN | `/=`    | Divide y asigna       |

### 2.8 Símbolos Especiales

Delimitadores y separadores del lenguaje.

| Token      | Símbolo | Uso                                      |
|------------|---------|------------------------------------------|
| SYM_LPAREN | `(`     | Apertura de paréntesis (condiciones, args)|
| SYM_RPAREN | `)`     | Cierre de paréntesis                     |
| SYM_LBRACE | `{`     | Apertura de bloque de sentencias         |
| SYM_RBRACE | `}`     | Cierre de bloque de sentencias           |
| SYM_COMMA  | `,`     | Separador de parámetros/argumentos       |
| SYM_SEMI   | `;`     | Terminador de sentencia                  |
| SYM_DOT    | `.`     | Acceso a miembro (reservado para ext.)   |

### 2.9 Comentarios de Línea

Comienzan con `//` y se extienden hasta el final de la línea (sin incluir el `\n`). No tienen relevancia semántica y no se almacenan en la tabla de símbolos.

- **Ejemplo:** `// este es un comentario`

### 2.10 Espacios en Blanco

Espacios (` `), tabulaciones (`\t`) y retornos de carro (`\r`) se consumen silenciosamente. Los saltos de línea (`\n`) son contados automáticamente por flex para el seguimiento de líneas.

### 2.11 Errores Léxicos

Cualquier carácter que no pertenezca al conjunto de lexemas reconocidos genera un error léxico. El scanner lo reporta en `stderr` con el número de línea y continúa el análisis (modo pánico: se descarta el carácter y se avanza al siguiente).

- **Ejemplos de error:** `$`, `@`, `~`, `` ` ``, `\`, `#`

---

## 3. Tabla de Tokens

| Token ID | Nombre        | Descripción                          | En Tabla de Símbolos |
|----------|---------------|--------------------------------------|----------------------|
| 100      | KW_IF         | Palabra reservada `if`               | No                   |
| 101      | KW_ELSE       | Palabra reservada `else`             | No                   |
| 102      | KW_WHILE      | Palabra reservada `while`            | No                   |
| 103      | KW_FOR        | Palabra reservada `for`              | No                   |
| 104      | KW_INT        | Palabra reservada `int`              | No                   |
| 105      | KW_FLOAT      | Palabra reservada `float`            | No                   |
| 106      | KW_RETURN     | Palabra reservada `return`           | No                   |
| 107      | KW_VOID       | Palabra reservada `void`             | No                   |
| 108      | KW_TRUE       | Palabra reservada `true`             | No                   |
| 109      | KW_FALSE      | Palabra reservada `false`            | No                   |
| 200      | IDENTIFIER    | Identificador de variable/función    | **Sí**               |
| 201      | INT_LIT       | Literal entero                       | **Sí**               |
| 202      | FLOAT_LIT     | Literal flotante                     | **Sí**               |
| 300      | OP_PLUS       | Operador `+`                         | No                   |
| 301      | OP_MINUS      | Operador `-`                         | No                   |
| 302      | OP_MULT       | Operador `*`                         | No                   |
| 303      | OP_DIV        | Operador `/`                         | No                   |
| 310      | OP_EQ         | Operador `==`                        | No                   |
| 311      | OP_NEQ        | Operador `!=`                        | No                   |
| 312      | OP_LT         | Operador `<`                         | No                   |
| 313      | OP_GT         | Operador `>`                         | No                   |
| 314      | OP_LEQ        | Operador `<=`                        | No                   |
| 315      | OP_GEQ        | Operador `>=`                        | No                   |
| 320      | OP_ASSIGN     | Operador `=`                         | No                   |
| 321      | OP_PASSIGN    | Operador `+=`                        | No                   |
| 322      | OP_MASSIGN    | Operador `-=`                        | No                   |
| 323      | OP_STASSIGN   | Operador `*=`                        | No                   |
| 324      | OP_DASSIGN    | Operador `/=`                        | No                   |
| 400      | SYM_LPAREN    | Símbolo `(`                          | No                   |
| 401      | SYM_RPAREN    | Símbolo `)`                          | No                   |
| 402      | SYM_LBRACE    | Símbolo `{`                          | No                   |
| 403      | SYM_RBRACE    | Símbolo `}`                          | No                   |
| 404      | SYM_COMMA     | Símbolo `,`                          | No                   |
| 405      | SYM_SEMI      | Símbolo `;`                          | No                   |
| 406      | SYM_DOT       | Símbolo `.`                          | No                   |
| 500      | COMMENT       | Comentario `// ...`                  | No                   |

---

## 4. Expresiones Regulares

| Token         | Expresión Regular      | Explicación                                           |
|---------------|------------------------|-------------------------------------------------------|
| KW_IF … KW_FALSE | literal exacto      | e.g. `if` — coincide solo con esa cadena              |
| IDENTIFIER    | `[a-zA-Z_][a-zA-Z0-9_]*` | Letra/`_`, seguido de cero o más letras/dígitos/`_` |
| INT_LIT       | `[0-9]+`               | Uno o más dígitos                                     |
| FLOAT_LIT     | `[0-9]+\.[0-9]+`       | Dígitos, punto literal, dígitos                       |
| OP_PLUS       | `\+`                   | Carácter `+`                                          |
| OP_MINUS      | `-`                    | Carácter `-`                                          |
| OP_MULT       | `\*`                   | Carácter `*`                                          |
| OP_DIV        | `\/`                   | Carácter `/`                                          |
| OP_EQ         | `==`                   | Dos signos de igual consecutivos                      |
| OP_NEQ        | `!=`                   | Exclamación seguida de igual                          |
| OP_LT         | `<`                    | Carácter menor que                                    |
| OP_GT         | `>`                    | Carácter mayor que                                    |
| OP_LEQ        | `<=`                   | Menor que seguido de igual                            |
| OP_GEQ        | `>=`                   | Mayor que seguido de igual                            |
| OP_ASSIGN     | `=`                    | Un solo signo de igual                                |
| OP_PASSIGN    | `\+=`                  | Suma seguida de igual                                 |
| OP_MASSIGN    | `-=`                   | Resta seguida de igual                                |
| OP_STASSIGN   | `\*=`                  | Multiplicación seguida de igual                       |
| OP_DASSIGN    | `\/=`                  | División seguida de igual                             |
| SYM_LPAREN    | `\(`                   | Paréntesis izquierdo                                  |
| SYM_RPAREN    | `\)`                   | Paréntesis derecho                                    |
| SYM_LBRACE    | `\{`                   | Llave izquierda                                       |
| SYM_RBRACE    | `\}`                   | Llave derecha                                         |
| SYM_COMMA     | `,`                    | Coma                                                  |
| SYM_SEMI      | `;`                    | Punto y coma                                          |
| SYM_DOT       | `\.`                   | Punto decimal/acceso                                  |
| COMMENT       | `\/\/[^\n]*`           | `//` seguido de cualquier carácter que no sea `\n`    |
| WHITESPACE    | `[ \t\r]+`             | Espacios y tabulaciones (descartados)                 |
| NEWLINE       | `\n`                   | Salto de línea (contado, descartado)                  |
| ERROR         | `.`                    | Cualquier otro carácter (modo pánico)                 |

---

## 5. Autómatas de Estado Finito (AFD)

### AFD-1: IDENTIFIER

Reconoce cadenas que comienzan con letra o `_` y continúan con letras, dígitos o `_`.

```
         [a-zA-Z_]          [a-zA-Z0-9_]
  →  q0 ──────────→  q1* ←──────────────┘
```

**Tabla de transición AFD-1:**

| Estado | `[a-zA-Z_]` | `[0-9]` | otro |
|--------|-------------|---------|------|
| q0     | q1          | —       | err  |
| q1 *   | q1          | q1      | fin  |

> Estado inicial: q0. Estado aceptor: q1 (marcado con `*`).

---

### AFD-2: INT_LIT y FLOAT_LIT

Reconoce enteros (q1) y flotantes (q3).

```
         [0-9]         [0-9]
  →  q0 ───────→ q1* ──────→  q1*
                  │
                  │  [.]
                  ↓
                 q2
                  │  [0-9]
                  ↓
                 q3* ──[0-9]──→ q3*
```

**Tabla de transición AFD-2:**

| Estado | `[0-9]` | `[.]` | otro |
|--------|---------|-------|------|
| q0     | q1      | —     | err  |
| q1 *   | q1      | q2    | fin  |
| q2     | q3      | —     | err  |
| q3 *   | q3      | —     | fin  |

> `q1` acepta INTEGER. `q3` acepta FLOAT_LIT.
> FLOAT_LIT debe probarse **antes** que INT_LIT en flex (maximal-munch).

---

### AFD-3: COMMENT

Reconoce comentarios de línea que comienzan con `//`.

```
        [/]       [/]       [^\n]
  →  q0 ───→ q1 ───→ q2* ─────────→ q2*
```

**Tabla de transición AFD-3:**

| Estado | `/` | `\n` | `[^\n]` |
|--------|-----|------|---------|
| q0     | q1  | —    | —       |
| q1     | q2  | —    | —       |
| q2 *   | q2  | fin  | q2      |

> Estado aceptor: q2. El `\n` **no** se consume (permite que yylineno lo cuente).

---

### AFD-4: Operadores compuestos

Reconoce `==`, `!=`, `<=`, `>=`, `+=`, `-=`, `*=`, `/=`.

```
  → q0  ─[=]→  q1  ─[=]→  q2*   (== )
     │  ─[!]→  q3  ─[=]→  q4*   (!=)
     │  ─[<]→  q5* ─[=]→  q6*   (< o <=)
     │  ─[>]→  q7* ─[=]→  q8*   (> o >=)
     │  ─[+]→  q9  ─[=]→  q10*  (+=)
     │  ─[-]→  q11 ─[=]→  q12*  (-=)
     │  ─[*]→  q13 ─[=]→  q14*  (*=)
     └  ─[/]→  q15 ─[=]→  q16*  (/=)
```

> Los estados aceptores intermedios (q5, q7) permiten reconocer `<` y `>` como tokens independientes si no les sigue `=`.

---

## 6. Tabla de Transición General

La siguiente tabla resume las clases de entrada y la acción que toma el scanner:

| Clase de Entrada           | Ejemplo  | Acción del Scanner                         |
|----------------------------|----------|--------------------------------------------|
| `[a-zA-Z_]`                | `x`      | Iniciar ID (o palabra reservada)           |
| `[0-9]`                    | `3`      | Iniciar INT_LIT (o FLOAT si sigue `.`)     |
| `[0-9]+ . [0-9]+`          | `3.14`   | Reconocer FLOAT_LIT completo               |
| `//`                       | `// txt` | Iniciar COMMENT hasta fin de línea         |
| `+=`, `-=`, `*=`, `/=`     | `+=`     | Operador de asignación compuesto           |
| `==`, `!=`, `<=`, `>=`     | `==`     | Operador relacional de dos caracteres      |
| `+`, `-`, `*`, `/`         | `+`      | Operador aritmético simple                 |
| `<`, `>`, `=`              | `<`      | Operador relacional/asignación simple      |
| `(`, `)`, `{`, `}`         | `(`      | Símbolo de agrupación                      |
| `,`, `;`, `.`              | `;`      | Símbolo separador/terminador               |
| `\n`                       | —        | Incrementar número de línea (descartar)    |
| `[ \t\r]`                  | —        | Espacio en blanco (descartar)              |
| cualquier otro             | `$`      | **ERROR LÉXICO**                           |

---

## 7. Errores Léxicos

| Caso de Error                  | Mensaje de Salida (stderr)                                      |
|--------------------------------|-----------------------------------------------------------------|
| Signo de dólar `$`             | `[ERROR LEXICO] linea N: simbolo no reconocido '$'`            |
| Arroba `@`                     | `[ERROR LEXICO] linea N: simbolo no reconocido '@'`            |
| Virgulilla `~`                 | `[ERROR LEXICO] linea N: simbolo no reconocido '~'`            |
| Acento grave `` ` ``           | ``[ERROR LEXICO] linea N: simbolo no reconocido '`'``          |
| Barra invertida `\`            | `[ERROR LEXICO] linea N: simbolo no reconocido '\'`            |
| Signo de número `#`            | `[ERROR LEXICO] linea N: simbolo no reconocido '#'`            |

**Estrategia de recuperación:** modo pánico. Se descarta el carácter no reconocido y el análisis continúa con el siguiente carácter. Esto permite detectar múltiples errores en una sola pasada.

---

## 8. Ejemplo de Código Fuente de Prueba

```c
// test_valid.sl — programa de prueba para el scanner léxico

int suma(int a, int b) {
    return a + b;
}

float promedio(float x, float y) {
    float resultado;
    resultado = (x + y) / 2.0;
    return resultado;
}

int main() {
    int x;
    x = 10;
    x += 5;
    while (x > 0) {
        x -= 1;
    }
    return 0;
}
```

---

## 9. Salida Esperada del Scanner

Para el archivo `test_valid.sl`, la salida en `stdout` comienza así:

```
+=======+================+===========================+=======+
|          SCANNER LEXICO — Lista de Tokens                  |
+=======+================+===========================+=======+
| ID    | Nombre         | Lexema                    | Linea |
+-------+----------------+---------------------------+-------+
| 500   | COMMENT        | // test_valid.sl — prog...| 1     |
| 104   | KW_INT         | int                       | 3     |
| 200   | IDENTIFIER     | suma                      | 3     |
| 400   | SYM_LPAREN     | (                         | 3     |
| 104   | KW_INT         | int                       | 3     |
| 200   | IDENTIFIER     | a                         | 3     |
| 404   | SYM_COMMA      | ,                         | 3     |
| 104   | KW_INT         | int                       | 3     |
| 200   | IDENTIFIER     | b                         | 3     |
| 401   | SYM_RPAREN     | )                         | 3     |
| 402   | SYM_LBRACE     | {                         | 3     |
| 106   | KW_RETURN      | return                    | 4     |
| 200   | IDENTIFIER     | a                         | 4     |
| 300   | OP_PLUS        | +                         | 4     |
| 200   | IDENTIFIER     | b                         | 4     |
| 405   | SYM_SEMI       | ;                         | 4     |
| 403   | SYM_RBRACE     | }                         | 5     |
...
+=======+================+===========================+=======+

+=======+===========================+==============+=======+
|              TABLA DE SIMBOLOS                          |
+=======+===========================+==============+=======+
| Indx  | Lexema                    | Tipo         | Linea |
+-------+---------------------------+--------------+-------+
| 0     | suma                      | IDENTIFIER   | 3     |
| 1     | a                         | IDENTIFIER   | 3     |
| 2     | b                         | IDENTIFIER   | 3     |
| 3     | promedio                  | IDENTIFIER   | 7     |
| 4     | x                         | IDENTIFIER   | 7     |
| 5     | y                         | IDENTIFIER   | 7     |
| 6     | resultado                 | IDENTIFIER   | 8     |
| 7     | 2.0                       | FLOAT        | 9     |
| 8     | main                      | IDENTIFIER   | 13    |
| 9     | 10                        | INTEGER      | 15    |
| 10    | 5                         | INTEGER      | 16    |
| 11    | 0                         | INTEGER      | 17    |
| 12    | 1                         | INTEGER      | 18    |
+=======+===========================+==============+=======+
  Total de entradas: 13
```

Para `test_errors.sl`, en `stderr` aparece:

```
[ERROR LEXICO] linea 8: simbolo no reconocido '$'
[ERROR LEXICO] linea 11: simbolo no reconocido '@'
[ERROR LEXICO] linea 14: simbolo no reconocido '~'
[ERROR LEXICO] linea 17: simbolo no reconocido '`'
```

---

## 10. Tabla de Símbolos — Estructura y Justificación

### ¿Por qué solo IDENTIFIER, INT_LIT y FLOAT_LIT?

| Tipo de Token  | ¿En tabla? | Razón                                                            |
|----------------|------------|------------------------------------------------------------------|
| IDENTIFIER     | **Sí**     | El análisis semántico necesita verificar si fue declarado y su tipo |
| INT_LIT        | **Sí**     | El valor puede usarse en evaluación de constantes y optimizaciones  |
| FLOAT_LIT      | **Sí**     | Igual que INT_LIT                                                |
| Palabras KW    | No         | Semántica fija; el compilador la conoce sin tabla                |
| Operadores     | No         | Su forma los identifica unívocamente; no tienen nombre           |
| Comentarios    | No         | Sin relevancia en fases posteriores                              |

### Campos de cada entrada

| Campo      | Tipo   | Descripción                                 |
|------------|--------|---------------------------------------------|
| `lexeme`   | char[] | Texto exacto tal como aparece en el fuente  |
| `token_id` | int    | Tipo: TK_ID=200, TK_INT_LIT=201, TK_FLT_LIT=202 |
| `lineno`   | int    | Número de línea de primera aparición        |
