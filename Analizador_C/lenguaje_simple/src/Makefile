# ===========================================================================
# Makefile — Scanner Léxico, Lenguaje Académico Simple
# ===========================================================================
# Requisitos: flex, gcc
#
# Targets:
#   make          → compila todo
#   make test     → ejecuta con el ejemplo válido
#   make test_err → ejecuta con el ejemplo de errores
#   make clean    → elimina archivos generados
# ===========================================================================

CC      = gcc
LEX     = flex
CFLAGS  = -Wall -Wextra -std=c99
TARGET  = scanner
LEXOUT  = lex.yy.c

all: $(TARGET)

$(LEXOUT): scanner.l
	$(LEX) scanner.l

$(TARGET): $(LEXOUT) symbol_table.c main.c symbol_table.h token_ids.h
	$(CC) $(CFLAGS) $(LEXOUT) symbol_table.c main.c -o $(TARGET) -lfl

test: $(TARGET)
	./$(TARGET) ../examples/test_valid.sl

test_err: $(TARGET)
	./$(TARGET) ../examples/test_errors.sl

clean:
	rm -f $(LEXOUT) $(TARGET)

.PHONY: all test test_err clean
