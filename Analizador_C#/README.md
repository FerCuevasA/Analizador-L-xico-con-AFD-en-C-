# Analizador Léxico en C#

El analizador léxico está desarrollado en C# con .NET 8. Su función es tomar un fragmento de código fuente y dividirlo en tokens: las unidades mínimas con significado que un compilador necesita para procesar un programa, como palabras clave, nombres de variables, números u operadores.

Lo que hace diferente a este analizador es que el reconocimiento de tokens no se apoya en expresiones regulares para tokenizar. En cambio, cada tipo de token tiene su propio autómata finito determinista (AFD) programado a mano. Una vez que termina el análisis, el programa verifica cada token contra su expresión regular equivalente solo para confirmar que el autómata lo reconoció correctamente.

---

## Estructura del proyecto

```
Analizador_C#/
└── LexerProject/
    ├── LexerProject.sln             — solución de Visual Studio
    ├── LexerProject/                — código fuente
    │   ├── Program.cs               — menú de entrada e inicio del programa
    │   ├── Lexer.cs                 — coordinador del análisis
    │   ├── Token.cs                 — definición de la estructura de un token
    │   ├── Automata.cs              — los autómatas de cada tipo de token
    │   └── AutomataDisplay.cs       — tablas de transición en consola
    └── LexerProject.Tests/          — pruebas unitarias
```

---

## Cómo compilarlo y correrlo

Se necesita tener instalado el [SDK de .NET 8](https://dotnet.microsoft.com/download).

```bash
cd LexerProject
dotnet build
dotnet run --project LexerProject
```

Al arrancar, el programa muestra primero las tablas de transición de cada autómata y luego un menú con tres opciones:

```
1. Analizar texto de ejemplo
2. Ingresar texto personalizado
3. Salir
```

La opción 1 corre un ejemplo predefinido. La opción 2 permite escribir cualquier fragmento de código y ver cómo lo analiza el sistema.

Para correr las pruebas unitarias:

```bash
dotnet test
```

---

## Qué produce

El analizador genera dos cosas al procesar un texto:

**1. Tabla de tokens**
Muestra cada token encontrado con su tipo, el texto exacto que lo forma (lexema), la línea y la columna donde aparece. Los errores léxicos se marcan en la misma tabla.

Ejemplo con la entrada `int x = 3.14; // valor`:

```
Pos   Token         Lexema        Linea  Col
0     KEYWORD       int           1      1
4     IDENTIFIER    x             1      5
6     REL_OP        =             1      7
8     REAL          3.14          1      9
12    UNKNOWN       ;             1      13
14    LINE_COMMENT  // valor      1      15
```

**2. Validación cruzada**
Después de tokenizar, el sistema comprueba cada token contra su expresión regular equivalente e imprime si el resultado coincide. Esto sirve para verificar que los autómatas funcionan bien.

```
Token: KEYWORD       Lexema: "int"
  Regex: ^(if|else|while|...)$
  Regex.IsMatch --> True [OK]
```

---

## Cómo funciona por dentro

El núcleo del sistema está en `Lexer.cs`. Recorre el texto carácter a carácter y en cada posición prueba los autómatas en orden hasta que alguno encaje. El orden de los autómatas importa: `RealAutomata` va antes que `IntegerAutomata` para que un número como `3.14` se capture completo y no como `3` seguido de `.14`.

Cada autómata en `Automata.cs` es una máquina de estados. Lee el carácter actual, decide a qué estado pasar y así sucesivamente. Si al terminar está en un estado de aceptación, el lexema es válido.

Los tokens que el sistema reconoce son:

| Token | Qué reconoce |
|---|---|
| `KEYWORD` | palabras reservadas: `if else while for int float string return void class` |
| `IDENTIFIER` | nombres de variables y funciones |
| `INTEGER` | números enteros como `42` |
| `REAL` | números con decimal como `3.14` |
| `REL_OP` | comparadores: `< <= > >= == != =` |
| `STRING` | texto entre comillas dobles como `"hola"` |
| `LINE_COMMENT` | comentarios que empiezan con `//` |
| `WHITESPACE` | espacios, tabs y saltos de línea — se consumen y se descartan |
| `UNKNOWN` | cualquier carácter que el sistema no puede clasificar |
