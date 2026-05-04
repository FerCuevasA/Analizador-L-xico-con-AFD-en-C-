// mixed_errors.sl — prueba: errores lexicos mezclados con tokens validos
// El scanner debe reportar cada error en stderr y continuar reconociendo
// los tokens validos siguientes (modo panico: descartar caracter invalido).

int x;
x = 10$;
int y$y;
float z = 3.14@;
return ~x;
void test#();
