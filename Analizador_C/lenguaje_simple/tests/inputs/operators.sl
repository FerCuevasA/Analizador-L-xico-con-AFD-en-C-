// operators.sl — prueba: todos los operadores deben reconocerse con su token correcto

// Aritmeticos (OP_PLUS, OP_MINUS, OP_MULT, OP_DIV)
a + b
a - b
a * b
a / b

// Relacionales compuestos primero (maximal-munch): OP_EQ, OP_NEQ, OP_LEQ, OP_GEQ
a == b
a != b
a <= b
a >= b

// Relacionales simples: OP_LT, OP_GT
a < b
a > b

// Asignacion simple: OP_ASSIGN
a = b

// Asignacion compuesta: OP_PASSIGN, OP_MASSIGN, OP_STASSIGN, OP_DASSIGN
a += b
a -= b
a *= b
a /= b
