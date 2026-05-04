#!/usr/bin/env bash
set -e

# Busca python3 o python
if command -v python3 &>/dev/null; then
    PY=python3
elif command -v python &>/dev/null; then
    PY=python
else
    echo "Error: no se encontró Python. Instálalo desde https://python.org" >&2
    exit 1
fi

# Verifica versión mínima (3.8+)
version=$("$PY" -c "import sys; print(sys.version_info >= (3, 8))")
if [ "$version" != "True" ]; then
    echo "Error: se requiere Python 3.8 o superior." >&2
    exit 1
fi

cd "$(dirname "$0")"
"$PY" analizador.py
