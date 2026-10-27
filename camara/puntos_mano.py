"""
Convierte las manos que ve MediaPipe en la lista de números que usa el modelo.

LA USAN TODOS: grabar_ejemplos.py (para grabar), entrenar.py (para saber
cuántas columnas hay) y reconocer.py (en vivo). Si cada uno calculara los
números a su manera, el modelo aprendería con unos y en vivo recibiría otros,
y no acertaría nunca. Por eso hay una sola función para todos.

Formato de cada cuadro: siempre 126 números.
    - primero la mano IZQUIERDA (21 puntos x 3 = 63 números)
    - después la mano DERECHA (63 números)
    - si falta una mano, sus 63 lugares van en 0
    - si no hay ninguna mano, el cuadro es None

Nota: la imagen se da vuelta como espejo (cv2.flip) antes de buscar las manos,
tanto al grabar como en vivo, así que "izquierda" y "derecha" son siempre
las mismas en los dos casos.
"""

PUNTOS_POR_MANO = 21 * 3              # x, y, z de los 21 puntos
CANTIDAD_NUMEROS = PUNTOS_POR_MANO * 2  # mano izquierda + mano derecha = 126

# Nombres de las columnas del CSV: izq_x0, izq_y0, ..., der_z20
NOMBRES_COLUMNAS = [
    f"{lado}_{eje}{i}"
    for lado in ("izq", "der")
    for i in range(21)
    for eje in "xyz"
]

_SIN_MANO = [0.0] * PUNTOS_POR_MANO


def normalizar(puntos):
    """Pasa los 21 puntos de UNA mano a valores que no dependen de dónde está
    la mano ni de qué tan cerca está de la cámara (la muñeca es el centro)."""
    base = puntos[0]  # la muñeca es el origen
    coords = [(p.x - base.x, p.y - base.y, p.z - base.z) for p in puntos]
    escala = max(max(abs(valor) for valor in punto) for punto in coords) or 1.0
    return [round(valor / escala, 5) for punto in coords for valor in punto]


def extraer_puntos(resultados):
    """
    Recibe lo que devuelve detector.process(imagen) de MediaPipe Hands
    y devuelve 126 números (izquierda + derecha), o None si no hay manos.
    """
    if not resultados.multi_hand_landmarks:
        return None

    izquierda = _SIN_MANO
    derecha = _SIN_MANO

    for mano, lado in zip(resultados.multi_hand_landmarks, resultados.multi_handedness):
        puntos = normalizar(mano.landmark)
        if lado.classification[0].label == "Left":
            izquierda = puntos
        else:
            derecha = puntos

    return izquierda + derecha
