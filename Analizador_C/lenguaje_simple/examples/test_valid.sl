// test_valid.sl — programa de prueba para el scanner léxico
// Contiene un ejemplo de función con operaciones aritméticas y control de flujo

int suma(int a, int b) {
    return a + b;
}

float promedio(float x, float y) {
    float resultado;
    resultado = (x + y) / 2.0;
    return resultado;
}

void clasificar(int n) {
    if (n >= 0) {
        int positivo;
        positivo = 1;
    } else {
        int negativo;
        negativo = 0;
    }
}

int main() {
    int x;
    int y;
    float z;
    x = 10;
    y = 3;
    z = 3.14;
    x += 5;
    while (x > 0) {
        x -= 1;
    }
    for (int i = 0; i <= 5; i += 1) {
        y *= 2;
    }
    return 0;
}
