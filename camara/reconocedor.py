"""
Reconocedor de señas de Auna (tarea C1, parte de Agostina).

Recibe los puntos de la mano que juntó la ventana de la cámara (parte de Flor)
mientras la detección estaba activa, le pregunta al modelo qué seña fue
y le manda a la app las 3 opciones más probables.

Cómo se usa desde la ventana de la cámara:

    from reconocedor import Reconocedor

    reconocedor = Reconocedor()            # carga camara/modelo.pkl
    ...
    # al tocar "Detener detección":
    reconocedor.enviar_a_la_app(self.cuadros)

Donde self.cuadros es una lista con los puntos de cada cuadro de video
(lo que devuelve extraer_puntos() de puntos_mano.py: 126 números, mano
izquierda + mano derecha, o None si en ese cuadro no había ninguna mano).

Formato del mensaje que recibe la app (tarea B1):
    hola:0.91,gracias:0.05,ayuda:0.02
"""
import os
import sys

import joblib
import numpy as np

# Cuántas opciones le mandamos a la app.
CANTIDAD_OPCIONES = 3

# El modelo está en la misma carpeta que este archivo (camara/modelo.pkl).
# Lo arma Tomi en C4; mientras tanto se usa el de crear_modelo_prueba.py.
RUTA_MODELO = os.path.join(os.path.dirname(os.path.abspath(__file__)), "modelo.pkl")


class ModeloNoEncontrado(Exception):
    """No existe el archivo modelo.pkl."""


class Reconocedor:
    def __init__(self, ruta_modelo=RUTA_MODELO):
        if not os.path.exists(ruta_modelo):
            raise ModeloNoEncontrado(
                f"No se encontró el modelo en {ruta_modelo}. "
                "Para probar, crealo con: python camara/crear_modelo_prueba.py"
            )

        self.modelo = joblib.load(ruta_modelo)

        # La app lee los mensajes en UTF-8. Sin esto, en Windows palabras como
        # "perdón" o "sí" le llegarían con letras raras.
        sys.stdout.reconfigure(encoding="utf-8")

        # Cuántos números espera el modelo por cada cuadro (126 = 2 manos x 21 puntos x 3).
        self.cantidad_numeros = self.modelo.n_features_in_

    def reconocer(self, cuadros):
        """
        Recibe la lista de cuadros y devuelve las opciones más probables,
        de mayor a menor: [("hola", 0.91), ("gracias", 0.05), ("ayuda", 0.02)].
        Devuelve una lista vacía si en ningún cuadro había mano.
        """
        # Nos quedamos solo con los cuadros donde se vio la mano.
        cuadros_validos = [
            c for c in cuadros
            if c is not None and len(c) == self.cantidad_numeros
        ]

        if not cuadros_validos:
            return []

        # El modelo da, para cada cuadro, la probabilidad de cada palabra.
        probabilidades = self.modelo.predict_proba(np.array(cuadros_validos))

        # Promediamos todos los cuadros: así un cuadro malo no arruina el resultado.
        promedio = probabilidades.mean(axis=0)

        # Ordenamos de mayor a menor y nos quedamos con las primeras.
        mejores = np.argsort(promedio)[::-1][:CANTIDAD_OPCIONES]

        return [
            (str(self.modelo.classes_[i]), float(promedio[i]))
            for i in mejores
        ]

    @staticmethod
    def armar_mensaje(opciones):
        """[("hola", 0.91), ("gracias", 0.05)] -> "hola:0.91,gracias:0.05" """
        partes = []
        for palabra, confianza in opciones:
            # Sacamos ":" y "," de la palabra, porque la app los usa para separar.
            palabra_limpia = palabra.replace(":", " ").replace(",", " ")
            partes.append(f"{palabra_limpia}:{confianza:.2f}")  # siempre con punto
        return ",".join(partes)

    def enviar_a_la_app(self, cuadros):
        """
        Reconoce la seña y la imprime para que la app la reciba.
        Devuelve True si se mandó algo, False si no se vio ninguna mano.
        """
        opciones = self.reconocer(cuadros)

        if not opciones:
            return False

        # flush=True: la app recibe el mensaje en el momento, sin esperar.
        print(self.armar_mensaje(opciones), flush=True)
        return True
