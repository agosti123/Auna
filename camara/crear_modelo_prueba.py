"""
Crea un modelo DE PRUEBA con datos inventados (tarea C1, parte de Agostina).

Sirve para probar reconocedor.py antes de tener el modelo de verdad,
que lo entrena Tomi en C4 con las señas grabadas (C3 y C5).
Cuando esté el de Tomi, este archivo ya no hace falta.

Uso (desde la carpeta raíz del proyecto):
    .venv\\Scripts\\python.exe camara\\crear_modelo_prueba.py
"""
import os

import joblib
import numpy as np
from sklearn.ensemble import RandomForestClassifier

from puntos_mano import CANTIDAD_NUMEROS

# Las señas del proyecto (sprint D).
PALABRAS = [
    "hola", "chau", "gracias",
    "sí", "no", "no entiendo",
    "ayuda", "por favor", "perdón",
    "remedio", "dolor", "receta",
]

RUTA_MODELO = os.path.join(os.path.dirname(os.path.abspath(__file__)), "modelo.pkl")

# Inventamos una "forma de mano" fija para cada palabra (siempre la misma).
_generador = np.random.default_rng(seed=0)
FORMAS = {palabra: _generador.uniform(-1, 1, CANTIDAD_NUMEROS) for palabra in PALABRAS}


def puntos_inventados(palabra, ruido=0.15):
    """Devuelve los 126 números (dos manos) de alguien haciendo esa seña, con un poco de ruido."""
    return FORMAS[palabra] + np.random.normal(0, ruido, CANTIDAD_NUMEROS)


def main():
    datos = []
    etiquetas = []

    # 50 ejemplos inventados de cada palabra.
    for palabra in PALABRAS:
        for _ in range(50):
            datos.append(puntos_inventados(palabra))
            etiquetas.append(palabra)

    modelo = RandomForestClassifier(n_estimators=100, random_state=0)
    modelo.fit(np.array(datos), etiquetas)

    joblib.dump(modelo, RUTA_MODELO)
    print(f"Modelo de prueba guardado en {RUTA_MODELO}")


if __name__ == "__main__":
    main()
