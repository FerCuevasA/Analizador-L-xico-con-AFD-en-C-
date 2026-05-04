# =============================================================================
#  Analizador Léxico con AFD — Python (sin librerías externas)
#  Equivalente a la versión C# de este repositorio.
# =============================================================================
#
#  Tokens reconocidos:
#    KEYWORD      → if else while for int float string return void class
#    IDENTIFIER   → letra o _ seguido de letras, dígitos o _
#    INTEGER      → uno o más dígitos
#    REAL         → dígitos . dígitos
#    REL_OP       → < <= > >= = == !=
#    STRING       → "..." (sin salto de línea)
#    LINE_COMMENT → // hasta fin de línea
#    UNKNOWN      → cualquier carácter no reconocido (error léxico)
# =============================================================================


# =============================================================================
#  PASO 1: CLASIFICACIÓN DE CARACTERES
# =============================================================================
#
#  Un AFD no trabaja directamente con caracteres ('a', '3', '<'...) sino con
#  "clases" de caracteres. Por ejemplo, 'a', 'b', 'Z' son todos LETTER.
#  Esto simplifica las tablas de transición: en vez de una fila por cada
#  carácter posible, solo hay una fila por clase.
#
#  Aquí definimos las clases como constantes de texto.

LETTER     = "LETTER"       # a-z, A-Z
DIGIT      = "DIGIT"        # 0-9
UNDERSCORE = "UNDERSCORE"   # _
QUOTE      = "QUOTE"        # "
LT         = "LT"           # <
GT         = "GT"           # >
EQ         = "EQ"           # =
BANG       = "BANG"         # !
SLASH      = "SLASH"        # /
DOT        = "DOT"          # .
NEWLINE    = "NEWLINE"      # \n
SPACE      = "SPACE"        # espacio, tabulación, \r
OTHER      = "OTHER"        # cualquier otro carácter (ej. ; ( ) { } , + -)


def clasificar(c):
    """
    Recibe un carácter y devuelve su clase.
    Esta función es el puente entre el texto real y el AFD.
    """
    if c.isalpha():            return LETTER
    if c.isdigit():            return DIGIT
    if c == '_':               return UNDERSCORE
    if c == '"':               return QUOTE
    if c == '<':               return LT
    if c == '>':               return GT
    if c == '=':               return EQ
    if c == '!':               return BANG
    if c == '/':               return SLASH
    if c == '.':               return DOT
    if c == '\n':              return NEWLINE
    if c in (' ', '\t', '\r'): return SPACE
    return OTHER


# =============================================================================
#  PASO 2: TOKEN
# =============================================================================
#
#  Un Token es el resultado de reconocer un lexema. Guarda:
#    - tipo    : qué tipo de token es (KEYWORD, IDENTIFIER, etc.)
#    - lexema  : el texto exacto del código fuente (ej. "precio", "3.14")
#    - linea   : en qué línea del código fuente está
#    - columna : en qué columna empieza
#    - pos     : posición absoluta en el string del código fuente

class Token:
    def __init__(self, tipo, lexema, linea, columna, pos):
        self.tipo    = tipo
        self.lexema  = lexema
        self.linea   = linea
        self.columna = columna
        self.pos     = pos

    def __repr__(self):
        return f"Token({self.tipo}, {self.lexema!r}, L{self.linea}:C{self.columna})"


# =============================================================================
#  PASO 3: AUTÓMATA BASE
# =============================================================================
#
#  Un Autómata Finito Determinista (AFD) tiene:
#    - Un conjunto de estados (representados como enteros: 0, 1, 2...)
#    - Un estado inicial (siempre el 0)
#    - Un conjunto de estados de aceptación (los que "aceptan" el token)
#    - Una función de transición: δ(estado_actual, clase_char) → estado_siguiente
#
#  La función de transición se almacena como un diccionario:
#    { (estado, clase_char): estado_siguiente }
#
#  El método ejecutar() implementa el principio de MÁXIMO CONSUMO (maximal-munch):
#  consume la cadena más larga posible que el autómata pueda aceptar.
#  Por ejemplo, para ">=" no se queda con ">" sino que sigue y acepta ">=".

class Automata:
    estado_inicial     = 0
    estados_aceptacion = set()   # estados donde el autómata "acepta" el token
    transiciones       = {}      # tabla δ: {(estado, clase) -> estado_siguiente}
    tipo_token         = "BASE"  # nombre del token que reconoce esta subclase

    def ejecutar(self, fuente, inicio):
        """
        Recorre fuente desde la posición 'inicio' aplicando transiciones.
        Guarda la última posición donde estuvo en un estado aceptante.
        Retorna (True, lexema) si reconoció algo, o (False, "") si no.

        Ejemplo para AutomataReal sobre "3.14xyz":
          pos=0 '3' DIGIT  → estado 1 (aceptante para entero, pero seguimos)
          pos=1 '.' DOT    → estado 2 (no aceptante, seguimos)
          pos=2 '1' DIGIT  → estado 3 (aceptante, guardamos pos=3)
          pos=3 '4' DIGIT  → estado 3 (aceptante, guardamos pos=4)
          pos=4 'x' LETTER → no hay transición, paramos
          → retorna (True, "3.14")
        """
        estado          = self.estado_inicial
        pos             = inicio
        ultimo_aceptado = -1   # -1 significa "todavía no hemos aceptado nada"

        while pos < len(fuente):
            cls       = clasificar(fuente[pos])
            siguiente = self.transiciones.get((estado, cls))

            if siguiente is None:
                # No hay transición desde el estado actual con este carácter.
                # Paramos aquí (el AFD no puede seguir).
                break

            estado = siguiente
            pos   += 1

            if estado in self.estados_aceptacion:
                # Guardamos hasta dónde llegamos mientras el estado sea aceptante.
                # Si luego encontramos un estado mejor, lo sobreescribimos.
                ultimo_aceptado = pos

        if ultimo_aceptado > inicio:
            # Recortamos el lexema desde inicio hasta el último punto aceptado
            return True, fuente[inicio:ultimo_aceptado]

        return False, ""


# =============================================================================
#  PASO 4: AUTÓMATAS CONCRETOS
# =============================================================================
#
#  Cada subclase representa un AFD para un tipo de token específico.
#  Solo necesitan declarar su tabla de transiciones, sus estados aceptantes
#  y su tipo de token. El algoritmo de reconocimiento lo hereda de Automata.
#
#  Notación de los diagramas:
#    q0 = estado inicial
#    q1*, q2* = estados de aceptación (marcados con *)
#    [X] = clase de carácter X
#    --> = transición


class AutomataIdentificador(Automata):
    """
    Reconoce: x  _var  precio1  miVariable

    Diagrama:
      q0 --[LETTER|UNDERSCORE]--> q1*
      q1* --[LETTER|DIGIT|UNDERSCORE]--> q1*

    El estado q1 es de aceptación desde el primer carácter válido.
    Los identificadores se reconocen primero como IDENTIFIER;
    después el Lexer verifica si el lexema es una palabra reservada
    y cambia el tipo a KEYWORD si corresponde.
    """
    estado_inicial     = 0
    estados_aceptacion = {1}
    tipo_token         = "IDENTIFIER"
    transiciones = {
        (0, LETTER):     1,   # primer carácter: letra
        (0, UNDERSCORE): 1,   # primer carácter: guión bajo
        (1, LETTER):     1,   # siguientes: letra (se queda en q1)
        (1, DIGIT):      1,   # siguientes: dígito
        (1, UNDERSCORE): 1,   # siguientes: guión bajo
    }


class AutomataEntero(Automata):
    """
    Reconoce: 0  42  100

    Diagrama:
      q0 --[DIGIT]--> q1*
      q1* --[DIGIT]--> q1*

    Nota: AutomataReal se ejecuta ANTES que este en la lista de autómatas,
    así que "3.14" siempre se reconoce como REAL, no como INTEGER + punto + INTEGER.
    """
    estado_inicial     = 0
    estados_aceptacion = {1}
    tipo_token         = "INTEGER"
    transiciones = {
        (0, DIGIT): 1,
        (1, DIGIT): 1,
    }


class AutomataReal(Automata):
    """
    Reconoce: 3.14  0.5  100.0

    Diagrama:
      q0 --[DIGIT]--> q1 --[DIGIT]--> q1
      q1 --[DOT]--> q2 --[DIGIT]--> q3*
      q3* --[DIGIT]--> q3*

    q1 y q2 NO son estados de aceptación: un número como "3." no es válido.
    Solo q3 acepta, lo que garantiza que debe haber al menos un dígito después del punto.
    """
    estado_inicial     = 0
    estados_aceptacion = {3}
    tipo_token         = "REAL"
    transiciones = {
        (0, DIGIT): 1,   # primer dígito
        (1, DIGIT): 1,   # más dígitos antes del punto
        (1, DOT):   2,   # el punto decimal
        (2, DIGIT): 3,   # primer dígito después del punto → acepta
        (3, DIGIT): 3,   # más dígitos decimales
    }


class AutomataRelOp(Automata):
    """
    Reconoce: <  <=  >  >=  =  ==  !=

    Diagrama (cada rama del AFD):
      q0 --[LT]-->   q1*  (acepta "<")
      q1 --[EQ]-->   q2*  (acepta "<=")

      q0 --[GT]-->   q3*  (acepta ">")
      q3 --[EQ]-->   q4*  (acepta ">=")

      q0 --[EQ]-->   q5*  (acepta "=")
      q5 --[EQ]-->   q6*  (acepta "==")

      q0 --[BANG]--> q7   (estado intermedio, NO acepta solo "!")
      q7 --[EQ]-->   q8*  (acepta "!=")

    El maximal-munch asegura que al ver "<=", el AFD no se queda con "<"
    sino que sigue para reconocer "<=".
    """
    estado_inicial     = 0
    estados_aceptacion = {1, 2, 3, 4, 5, 6, 8}   # q7 no está incluido: "!" solo no es válido
    tipo_token         = "REL_OP"
    transiciones = {
        (0, LT):   1,
        (1, EQ):   2,
        (0, GT):   3,
        (3, EQ):   4,
        (0, EQ):   5,
        (5, EQ):   6,
        (0, BANG): 7,
        (7, EQ):   8,
    }


class AutomataCadena(Automata):
    """
    Reconoce: "hola"  "precio 1"  ""

    Diagrama:
      q0 --[QUOTE]--> q1
      q1 --[cualquier cosa excepto QUOTE y NEWLINE]--> q1
      q1 --[QUOTE]--> q2*

    Las cadenas no pueden tener saltos de línea (son literales de una sola línea).
    El truco aquí: en vez de escribir una fila por cada clase válida dentro de la cadena,
    generamos las transiciones con un bucle para todas las clases excepto QUOTE y NEWLINE.
    """
    estado_inicial     = 0
    estados_aceptacion = {2}
    tipo_token         = "STRING"

    # Todas las clases posibles que pueden ir dentro de una cadena
    _internas = [LETTER, DIGIT, UNDERSCORE, LT, GT, EQ, BANG, SLASH, DOT, SPACE, OTHER]

    transiciones = {
        (0, QUOTE): 1,   # abre la cadena con "
        (1, QUOTE): 2,   # cierra la cadena con "  → acepta
    }
    # Dentro de la cadena (q1), cualquier carácter que no sea " ni \n se queda en q1
    for _cls in _internas:
        transiciones[(1, _cls)] = 1


class AutomataComentario(Automata):
    """
    Reconoce: // esto es un comentario

    Diagrama:
      q0 --[SLASH]--> q1
      q1 --[SLASH]--> q2*   (acepta "//" vacío)
      q2* --[cualquier cosa excepto NEWLINE]--> q3*
      q3* --[cualquier cosa excepto NEWLINE]--> q3*

    q2 acepta "//" (comentario vacío al final de línea).
    q3 acepta el comentario con contenido.
    Al llegar \n, no hay transición → el autómata para y no consume el salto de línea.
    """
    estado_inicial     = 0
    estados_aceptacion = {2, 3}
    tipo_token         = "LINE_COMMENT"

    _contenido = [LETTER, DIGIT, UNDERSCORE, QUOTE, LT, GT, EQ, BANG, SLASH, DOT, SPACE, OTHER]

    transiciones = {
        (0, SLASH): 1,   # primer /
        (1, SLASH): 2,   # segundo / → ya es comentario
    }
    # Dentro del comentario (q2 y q3), consumir todo excepto \n
    for _cls in _contenido:
        transiciones[(2, _cls)] = 3
        transiciones[(3, _cls)] = 3


class AutomataEspacio(Automata):
    """
    Consume espacios, tabulaciones y saltos de línea.
    Los espacios no generan tokens, solo se usan para avanzar la posición.
    """
    estado_inicial     = 0
    estados_aceptacion = {1}
    tipo_token         = "WHITESPACE"
    transiciones = {
        (0, SPACE):   1,
        (0, NEWLINE): 1,
        (1, SPACE):   1,
        (1, NEWLINE): 1,
    }


# =============================================================================
#  PASO 5: LEXER — coordina todos los autómatas
# =============================================================================

# Palabras reservadas del lenguaje.
# Los identificadores que coincidan con estas se reclasifican como KEYWORD.
KEYWORDS = {
    "if", "else", "while", "for", "int",
    "float", "string", "return", "void", "class"
}

# Lista de autómatas en orden de prioridad.
# IMPORTANTE: AutomataReal va ANTES que AutomataEntero.
# Sin esto, "3.14" se reconocería como INTEGER "3", punto, INTEGER "14".
AUTOMATAS = [
    AutomataReal(),
    AutomataEntero(),
    AutomataIdentificador(),
    AutomataRelOp(),
    AutomataCadena(),
    AutomataComentario(),
    AutomataEspacio(),
]


def tokenizar(fuente):
    """
    Función principal del analizador léxico.

    Recorre el código fuente posición por posición. En cada posición prueba
    cada autómata en orden; el primero que reconozca algo "gana" y produce
    un token. Si ninguno reconoce el carácter, se emite un token UNKNOWN
    (error léxico) y se avanza un carácter.

    Retorna una lista de Token.
    """
    # Normalizar saltos de línea de Windows (\r\n) y Mac antiguo (\r) a \n
    fuente  = fuente.replace("\r\n", "\n").replace("\r", "\n")

    tokens  = []
    pos     = 0    # posición absoluta en el string
    linea   = 1    # número de línea actual (empieza en 1)
    columna = 1    # columna actual (empieza en 1)

    while pos < len(fuente):
        reconocido = False

        # Probar cada autómata en orden hasta que uno tenga éxito
        for automata in AUTOMATAS:
            ok, lexema = automata.ejecutar(fuente, pos)

            if not ok or len(lexema) == 0:
                continue   # este autómata no reconoció nada, probar el siguiente

            if automata.tipo_token == "WHITESPACE":
                # Los espacios solo mueven el cursor, no generan token
                pos, linea, columna = _avanzar(pos, linea, columna, lexema)
                reconocido = True
                break

            # Verificar si el identificador es en realidad una palabra reservada
            tipo = automata.tipo_token
            if tipo == "IDENTIFIER" and lexema in KEYWORDS:
                tipo = "KEYWORD"

            tokens.append(Token(tipo, lexema, linea, columna, pos))
            pos, linea, columna = _avanzar(pos, linea, columna, lexema)
            reconocido = True
            break

        if not reconocido:
            # Ningún autómata pudo reconocer este carácter → error léxico
            c = fuente[pos]
            tokens.append(Token("UNKNOWN", c, linea, columna, pos))
            # Avanzar manualmente un carácter
            if c == '\n':
                linea  += 1
                columna = 1
            else:
                columna += 1
            pos += 1

    return tokens


def _avanzar(pos, linea, columna, lexema):
    """
    Avanza el cursor (pos, linea, columna) en función del lexema consumido.
    Necesario para mantener el rastreo de posición correcto cuando hay saltos de línea.
    """
    for c in lexema:
        pos += 1
        if c == '\n':
            linea  += 1
            columna = 1   # al saltar de línea, la columna vuelve a 1
        else:
            columna += 1
    return pos, linea, columna


# =============================================================================
#  PASO 6: PRESENTACIÓN DE RESULTADOS
# =============================================================================

def imprimir_tabla(tokens):
    """Imprime la lista de tokens en formato de tabla alineada."""
    if not tokens:
        print("(sin tokens)")
        return

    # Calcular anchos de columna según el contenido más largo
    ancho_tipo   = max(max(len(t.tipo)   for t in tokens), 10)
    ancho_lexema = max(max(len(t.lexema) for t in tokens), 10)

    separador  = "-" * (6 + ancho_tipo + ancho_lexema + 20)
    encabezado = (f"{'Pos':<6}{'Token':<{ancho_tipo + 2}}"
                  f"{'Lexema':<{ancho_lexema + 4}}{'Línea':<8}{'Col'}")

    print(encabezado)
    print(separador)

    for t in tokens:
        marca = "  <-- ERROR LÉXICO" if t.tipo == "UNKNOWN" else ""
        print(f"{t.pos:<6}{t.tipo:<{ancho_tipo + 2}}"
              f"{repr(t.lexema):<{ancho_lexema + 4}}{t.linea:<8}{t.columna}"
              f"{marca}")


# =============================================================================
#  MENÚ INTERACTIVO
# =============================================================================

import os

EJEMPLO = 'int precio = 3.14;\nif (precio >= 0.0) // ok\nreturn precio;'

# Directorio de tests relativo a este archivo
TESTS_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "tests")


def correr_tests():
    """
    Lee todos los archivos .txt de la carpeta tests/, los tokeniza
    y muestra los resultados uno por uno con un resumen al final.
    """
    if not os.path.isdir(TESTS_DIR):
        print(f"No se encontró la carpeta tests/ en {TESTS_DIR}")
        return

    archivos = sorted(f for f in os.listdir(TESTS_DIR) if f.endswith(".txt"))

    if not archivos:
        print("No hay archivos .txt en la carpeta tests/")
        return

    total_tokens  = 0
    total_errores = 0

    for nombre in archivos:
        ruta = os.path.join(TESTS_DIR, nombre)

        with open(ruta, encoding="utf-8") as f:
            codigo = f.read()

        tokens  = tokenizar(codigo)
        errores = [t for t in tokens if t.tipo == "UNKNOWN"]

        total_tokens  += len(tokens)
        total_errores += len(errores)

        # Encabezado del archivo
        ancho = 50
        print("=" * ancho)
        print(f"  {nombre}")
        estado = "CON ERRORES LÉXICOS" if errores else "sin errores léxicos"
        print(f"  {len(tokens)} tokens — {estado}")
        print("=" * ancho)

        imprimir_tabla(tokens)
        print()

    # Resumen global
    print("-" * 50)
    print(f"  RESUMEN: {len(archivos)} archivos | "
          f"{total_tokens} tokens | {total_errores} errores léxicos")
    print("-" * 50)


def menu():
    print("=" * 50)
    print("   Analizador Léxico con AFD — Python")
    print("=" * 50)
    print("1. Ejecutar ejemplo predefinido")
    print("2. Ingresar código propio")
    print("3. Correr tests (carpeta tests/)")
    print("4. Salir")
    print()

    while True:
        opcion = input("Opción: ").strip()

        if opcion == "1":
            print(f"\nEntrada:\n{EJEMPLO}\n")
            tokens = tokenizar(EJEMPLO)
            imprimir_tabla(tokens)

        elif opcion == "2":
            print("Escribe el código (deja una línea vacía para terminar):")
            lineas = []
            while True:
                linea = input()
                if linea == "":
                    break
                lineas.append(linea)
            codigo = "\n".join(lineas)
            if codigo:
                print()
                tokens = tokenizar(codigo)
                imprimir_tabla(tokens)
            else:
                print("(entrada vacía)")

        elif opcion == "3":
            print()
            correr_tests()

        elif opcion == "4":
            print("Hasta luego.")
            break

        elif opcion != "":
            print("Opción no válida.")

        print()


if __name__ == "__main__":
    menu()
