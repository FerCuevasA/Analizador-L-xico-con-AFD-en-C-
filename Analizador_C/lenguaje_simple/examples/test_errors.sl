// test_errors.sl — prueba de manejo de errores léxicos
// Contiene símbolos inválidos para verificar la detección de errores

int calculo(int a) {
    int resultado;
    resultado = a * 2;

    // Símbolo inválido: signo de dólar
    int $monto = 100;

    // Símbolo inválido: arroba
    int @flag = 1;

    // Símbolo inválido: virgulilla
    resultado = resultado ~ 5;

    // Símbolo inválido: acento grave
    int `temp = 0;

    return resultado;
}
