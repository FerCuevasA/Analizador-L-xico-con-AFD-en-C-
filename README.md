# 📘 Analizador Léxico con AFD en C# (.NET 8)

## 1. Introducción

El presente proyecto implementa un **analizador léxico (lexer)** basado en **Autómatas Finitos Deterministas (AFD)**. Su objetivo es transformar una cadena de entrada en una secuencia de tokens válidos, los cuales serán utilizados en fases posteriores del proceso de compilación.

El análisis léxico constituye la primera etapa de un compilador, donde se identifican **lexemas** y se clasifican en **tokens** mediante patrones formales. Dado que estos patrones pertenecen a lenguajes regulares, pueden representarse mediante AFD, lo cual garantiza eficiencia en tiempo lineal.

El sistema ha sido diseñado siguiendo un enfoque formal, donde cada tipo de token es reconocido por un autómata independiente, permitiendo trazabilidad entre análisis, diseño e implementación.

---

## 2. Arquitectura del sistema

El proyecto se organiza de la siguiente forma:


/LexerProject
+-- Program.cs Punto de entrada y ejecución
+-- Lexer.cs Control del flujo de análisis
+-- Token.cs Representación de tokens
+-- Automata.cs Clase base para AFD
+-- AutomataDisplay.cs Visualización de tablas ASCII


El componente `Lexer` coordina la ejecución de los distintos autómatas, seleccionando cuál ejecutar en función del carácter actual.

---

## 3. Modelo formal del autómata

Cada autómata implementa los siguientes elementos:

- **Estado inicial**
- **Estados de aceptación**
- **Función de transición**: δ(q, c) → q'
- **Clasificación de caracteres (CharClass)**
- **Método de ejecución incremental (`Run`)**

El reconocimiento se basa en el principio de **máximo consumo (maximal munch)**, asegurando que siempre se capture el lexema más largo posible.

---

## 4. Tokens reconocidos

El sistema identifica los siguientes tokens:

| Token | Patrón |
|------|--------|
| IDENTIFIER | `[a-zA-Z_][a-zA-Z0-9_]*` |
| INTEGER | `[0-9]+` |
| REAL | `[0-9]+\.[0-9]+` |
| REL_OP | `<= >= == != < >` |
| STRING | `"[^"\n]*"` |
| LINE_COMMENT | `//[^\n]*` |
| KEYWORD | subconjunto de IDENTIFIER |
| WHITESPACE | descartado |
| UNKNOWN | error léxico |

Palabras reservadas:

if, else, while, for, int, float, string, return, void, class


---

## 5. Funcionamiento de los autómatas

### 5.1 Identificadores y Keywords

El autómata inicia en un estado que acepta letras o `_`, y entra en un ciclo donde acepta letras y dígitos.

Ejemplo:

Entrada: precio1
Salida: IDENTIFIER


Posteriormente:

Entrada: if
Salida: KEYWORD


---

### 5.2 Números

El autómata distingue entre enteros y reales mediante un estado intermedio.

Ejemplo:

42 → INTEGER
3.14 → REAL


Casos inválidos:

.5 → ERROR
5. → ERROR


---

### 5.3 Operadores relacionales

Se implementa un mecanismo de **lookahead** para distinguir operadores de uno y dos caracteres.

Ejemplo:

< → REL_OP
<= → REL_OP
!= → REL_OP


---

### 5.4 Cadenas de texto

Una cadena inicia y termina con comillas dobles. No se permiten saltos de línea internos.

Ejemplo:

"hola" → STRING


Error:

"hola → ERROR (cadena no terminada)


---

### 5.5 Comentarios de línea

Se detectan mediante `//` y se consumen hasta el final de línea.

Ejemplo:

// comentario


El token puede ser ignorado o registrado según configuración.

---

### 5.6 Espacios en blanco

Los espacios, tabs y saltos de línea son consumidos por un autómata específico y descartados silenciosamente.

---

### 5.7 Caracteres desconocidos

Cualquier símbolo no reconocido genera un error léxico:


@ → UNKNOWN


---

## 6. Salida del analizador

Ejemplo de ejecución:


Entrada:
int x = 3.14; // valor

Salida:

Pos Token Lexema Linea Col

0 KEYWORD int 1 1
4 IDENTIFIER x 1 5
6 REL_OP = 1 7
8 REAL 3.14 1 9
12 UNKNOWN ; 1 13
14 LINE_COMMENT // valor 1 15


---

## 7. Validación cruzada con expresiones regulares

Cada token reconocido es validado mediante su expresión regular equivalente:


Token: IDENTIFIER
Lexema: "precio"

Regex: ^[a-zA-Z_][a-zA-Z0-9_]*$
Resultado: True [OK]


Esta validación no forma parte del reconocimiento, sino de la verificación.

---

## 8. Pruebas unitarias

Se implementan pruebas utilizando **xUnit o MSTest**, incluyendo:

- Casos válidos por cada tipo de token
- Casos límite
- Casos de error léxico

Ejemplo:

"123" → INTEGER
"abc" → IDENTIFIER
"@" → ERROR


---

## 9. Restricciones del diseño

- No se utiliza `Regex` como mecanismo de tokenización
- No se emplea `String.Split`
- El reconocimiento se basa exclusivamente en AFD
- Las expresiones regulares se usan únicamente para validación
- Se emplean estructuras modernas de C# (.NET 8)

---

## 10. Ejecución

```bash
dotnet build
dotnet run
11. Conclusión

El sistema demuestra la aplicación práctica de la teoría de lenguajes formales en la construcción de analizadores léxicos. La implementación mediante AFD permite un reconocimiento eficiente, determinista y completamente trazable, alineado con los principios clásicos del diseño de compiladores.
