namespace LexerProject;

public static class AutomataDisplay
{
    public static void ShowAllTables()
    {
        ShowIdentifierTable();
        ShowIntegerTable();
        ShowRealTable();
        ShowRelOpTable();
        ShowStringTable();
        ShowLineCommentTable();
    }

    public static void ShowIdentifierTable()
    {
        Console.WriteLine("""
            +------------------------------------------------------------+
            |      AFD IDENTIFICADORES - Tabla de transiciones           |
            +--------+------------+----------+-------------+-------------+
            | Estado |  letra/_   |  digito  |  separador  |   Accion   |
            +--------+------------+----------+-------------+-------------+
            | q0  >  |   -> q1    |  ERROR   |    ERROR    |   inicio   |
            | [q1 *] |   -> q1    |  -> q1   |    EMIT     |   bucle    |
            +--------+------------+----------+-------------+-------------+
            |  >  = estado inicial     * = estado de aceptacion          |
            +------------------------------------------------------------+
            """);
    }

    public static void ShowIntegerTable()
    {
        Console.WriteLine("""
            +------------------------------------------------------------+
            |        AFD ENTEROS - Tabla de transiciones                 |
            +--------+------------+-------------+------------------------+
            | Estado |   digito   |    otro     |         Accion        |
            +--------+------------+-------------+------------------------+
            | q0  >  |   -> q1    |    ERROR    |         inicio        |
            | [q1 *] |   -> q1    |    EMIT     |     leer digitos      |
            +--------+------------+-------------+------------------------+
            |  >  = estado inicial     * = estado de aceptacion          |
            +------------------------------------------------------------+
            """);
    }

    public static void ShowRealTable()
    {
        Console.WriteLine("""
            +------------------------------------------------------------+
            |         AFD REALES - Tabla de transiciones                 |
            +--------+----------+----------+----------+------------------+
            | Estado |  digito  |  punto   |   otro   |     Accion      |
            +--------+----------+----------+----------+------------------+
            | q0  >  |  -> q1   |  ERROR   |  ERROR   |     inicio      |
            |  q1    |  -> q1   |  -> q2   |  FAIL    |  parte entera   |
            |  q2    |  -> q3   |  ERROR   |  FAIL    |  tras el punto  |
            | [q3 *] |  -> q3   |   EMIT   |   EMIT   |  parte decimal  |
            +--------+----------+----------+----------+------------------+
            |  >  = estado inicial     * = estado de aceptacion          |
            +------------------------------------------------------------+
            """);
    }

    public static void ShowRelOpTable()
    {
        Console.WriteLine("""
            +------------------------------------------------------------------------+
            |       AFD OPERADORES RELACIONALES - Tabla de transiciones              |
            +--------+--------+--------+--------+--------+--------+----------------+
            | Estado |   <    |   >    |   =    |   !    |  otro  |    Accion      |
            +--------+--------+--------+--------+--------+--------+----------------+
            | q0  >  | ->q1*  | ->q3*  | ->q5*  |  ->q7  | ERROR  |    inicio      |
            | [q1 *] |  EMIT  |  EMIT  | ->q2*  |  EMIT  |  EMIT  | vio  <         |
            | [q2 *] |  EMIT  |  EMIT  |  EMIT  |  EMIT  |  EMIT  | vio <=         |
            | [q3 *] |  EMIT  |  EMIT  | ->q4*  |  EMIT  |  EMIT  | vio  >         |
            | [q4 *] |  EMIT  |  EMIT  |  EMIT  |  EMIT  |  EMIT  | vio >=         |
            | [q5 *] |  EMIT  |  EMIT  | ->q6*  |  EMIT  |  EMIT  | vio  =         |
            | [q6 *] |  EMIT  |  EMIT  |  EMIT  |  EMIT  |  EMIT  | vio ==         |
            |   q7   | ERROR  | ERROR  | ->q8*  | ERROR  | ERROR  | vio  !         |
            | [q8 *] |  EMIT  |  EMIT  |  EMIT  |  EMIT  |  EMIT  | vio !=         |
            +--------+--------+--------+--------+--------+--------+----------------+
            |  >  = estado inicial     * = estado de aceptacion                     |
            +------------------------------------------------------------------------+
            """);
    }

    public static void ShowStringTable()
    {
        Console.WriteLine("""
            +------------------------------------------------------------+
            |         AFD CADENAS - Tabla de transiciones                |
            +--------+------------+----------+----------+----------------+
            | Estado |    "       |   \n     |  otro    |    Accion     |
            +--------+------------+----------+----------+----------------+
            | q0  >  |   -> q1    |  ERROR   |  ERROR   |    inicio     |
            |   q1   |   -> q2    |  ERROR   |  -> q1   | dentro cadena |
            | [q2 *] |    EMIT    |   EMIT   |   EMIT   |  cierre "     |
            +--------+------------+----------+----------+----------------+
            |  >  = estado inicial     * = estado de aceptacion          |
            +------------------------------------------------------------+
            """);
    }

    public static void ShowLineCommentTable()
    {
        Console.WriteLine("""
            +------------------------------------------------------------+
            |    AFD COMENTARIOS DE LINEA - Tabla de transiciones        |
            +--------+------------+----------+----------+----------------+
            | Estado |    /       |   \n     |  otro    |    Accion     |
            +--------+------------+----------+----------+----------------+
            | q0  >  |   -> q1    |  ERROR   |  ERROR   |    inicio     |
            |   q1   |   -> q2    |  ERROR   |  ERROR   |  primer /     |
            | [q2 *] |   -> q3    |   EMIT   |  -> q3   | vio  //       |
            | [q3 *] |   -> q3    |   EMIT   |  -> q3   | en comentario |
            +--------+------------+----------+----------+----------------+
            |  >  = estado inicial     * = estado de aceptacion          |
            +------------------------------------------------------------+
            """);
    }
}
