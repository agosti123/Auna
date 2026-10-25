import argparse
import csv
from pathlib import Path

import cv2
import mediapipe as mp

CARPETA = Path(__file__).parent


def normalizar(puntos):
    """Pasa los 21 puntos a valores que no dependen de dónde está la mano
    ni de qué tan cerca está de la cámara."""
    base = puntos[0]  # la muñeca es el origen
    coords = [(p.x - base.x, p.y - base.y, p.z - base.z) for p in puntos]
    escala = max(max(abs(valor) for valor in punto) for punto in coords) or 1.0
    return [round(valor / escala, 5) for punto in coords for valor in punto]


def abrir_csv(ruta):
    ruta.parent.mkdir(exist_ok=True)
    nuevo = not ruta.exists()
    archivo = open(ruta, "a", newline="", encoding="utf-8")
    escritor = csv.writer(archivo)
    if nuevo:
        escritor.writerow(["etiqueta"] + [f"{eje}{i}" for i in range(21) for eje in "xyz"])
    return archivo, escritor


def main():
    parser = argparse.ArgumentParser(description="Graba ejemplos de una seña con la cámara.")
    parser.add_argument("sena", help='Palabra de la seña, como en senas.json (por ejemplo: hola o "por favor")')
    parser.add_argument("--persona", required=True, help="Tu nombre; define el archivo de salida")
    args = parser.parse_args()

    etiqueta = args.sena.strip().lower()
    salida = CARPETA / "datos" / f"ejemplos_{args.persona.strip().lower()}.csv"

    mp_hands = mp.solutions.hands
    mp_drawing = mp.solutions.drawing_utils

    camara = cv2.VideoCapture(0)
    if not camara.isOpened():
        print("Error: no se pudo abrir la cámara. Fijate que no la esté usando otro programa.")
        return

    archivo, escritor = abrir_csv(salida)
    grabando = False
    muestras = 0

    with mp_hands.Hands(
        static_image_mode=False,
        max_num_hands=1,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    ) as detector:
        while True:
            correcto, imagen = camara.read()
            if not correcto:
                print("Error: la cámara dejó de enviar imagen.")
                break

            imagen = cv2.flip(imagen, 1)  # espejo, igual que ventana_camara.py
            resultados = detector.process(cv2.cvtColor(imagen, cv2.COLOR_BGR2RGB))

            mano = None
            if resultados.multi_hand_landmarks:
                mano = resultados.multi_hand_landmarks[0]
                mp_drawing.draw_landmarks(imagen, mano, mp_hands.HAND_CONNECTIONS)
                if grabando:
                    escritor.writerow([etiqueta] + normalizar(mano.landmark))
                    muestras += 1

            estado = "GRABANDO" if grabando else "en pausa"
            deteccion = "mano detectada" if mano else "no veo la mano"
            cv2.putText(imagen, f"Sena: {etiqueta} | Muestras: {muestras} | {estado} | {deteccion}",
                        (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)
            cv2.putText(imagen, "ESPACIO: grabar/pausar   Q: salir",
                        (10, imagen.shape[0] - 15), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)
            cv2.imshow("Auna - grabar ejemplos", imagen)

            tecla = cv2.waitKey(1) & 0xFF
            if tecla == ord(" "):
                grabando = not grabando
            elif tecla in (ord("q"), 27):
                break

    archivo.close()
    camara.release()
    cv2.destroyAllWindows()
    print(f"Listo: {muestras} muestras de '{etiqueta}' guardadas en {salida}")


if __name__ == "__main__":
    main()