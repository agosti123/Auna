"""
Convierte videos de señas en ejemplos para entrenar el modelo.

Hace lo mismo que grabar_ejemplos.py, pero en vez de usar la cámara en vivo
lee videos ya grabados. Usa la MISMA función (extraer_puntos de puntos_mano.py),
así los números son iguales a los que ve la app en vivo.

Cómo nombrar los videos (en la carpeta camara/videos):
    hola.mp4, hola_2.mp4, chau.mov, si.mp4, no_1.mp4 ...
La palabra sale del nombre del archivo (lo que está antes de "_" o del número).

Uso (desde la carpeta raíz del proyecto):
    .venv\\Scripts\\python.exe camara\\videos_a_ejemplos.py --persona agostina

Guarda todo en camara/datos/ejemplos_<persona>-videos.csv. Ese archivo se
vuelve a armar completo cada vez que corrés el programa, así no se duplican
ejemplos si lo corrés dos veces.
"""
import argparse
import csv
import re
from pathlib import Path

import cv2
import mediapipe as mp

from puntos_mano import NOMBRES_COLUMNAS, extraer_puntos

CARPETA = Path(__file__).parent
EXTENSIONES = {".mp4", ".mov", ".avi", ".mkv", ".m4v", ".webm"}

# Para los nombres de archivo sin tilde: el modelo tiene que usar la palabra
# exactamente igual que el resto del proyecto.
CON_TILDE = {"si": "sí", "perdon": "perdón"}


def palabra_del_archivo(ruta):
    """'hola_2.mp4' -> 'hola', 'Por favor 3.mov' -> 'por favor', 'si.mp4' -> 'sí'"""
    nombre = ruta.stem.lower()
    nombre = re.sub(r"[\s_\-]*\(?\d+\)?$", "", nombre)  # saca _2, -3, " (4)" del final
    nombre = nombre.replace("_", " ").strip()
    return CON_TILDE.get(nombre, nombre)


def procesar_video(ruta, detector, escritor):
    """Lee el video cuadro por cuadro y guarda los cuadros donde se ven manos.
    Devuelve (cuadros leídos, cuadros guardados)."""
    video = cv2.VideoCapture(str(ruta))
    leidos = guardados = 0
    etiqueta = palabra_del_archivo(ruta)

    while True:
        correcto, imagen = video.read()
        if not correcto:
            break
        leidos += 1

        # Espejo, igual que la cámara en vivo (ventana_camara.py y grabar_ejemplos.py).
        imagen = cv2.flip(imagen, 1)
        resultados = detector.process(cv2.cvtColor(imagen, cv2.COLOR_BGR2RGB))

        puntos = extraer_puntos(resultados)  # 126 números o None
        if puntos is not None:
            escritor.writerow([etiqueta] + puntos)
            guardados += 1

    video.release()
    return leidos, guardados


def main():
    parser = argparse.ArgumentParser(description="Convierte videos de señas en ejemplos para entrenar.")
    parser.add_argument("--persona", required=True, help="Tu nombre (por ejemplo: agostina)")
    parser.add_argument("--carpeta", default=str(CARPETA / "videos"), help="Carpeta con los videos")
    args = parser.parse_args()

    carpeta = Path(args.carpeta)
    videos = sorted(p for p in carpeta.glob("*") if p.suffix.lower() in EXTENSIONES)
    if not videos:
        print(f"No encontré videos en {carpeta}.")
        print("Poné ahí los videos con el nombre de la seña: hola.mp4, chau.mp4, si.mp4...")
        return

    salida = CARPETA / "datos" / f"ejemplos_{args.persona.strip().lower()}-videos.csv"
    salida.parent.mkdir(exist_ok=True)

    with open(salida, "w", newline="", encoding="utf-8") as archivo, \
            mp.solutions.hands.Hands(
                static_image_mode=False,
                max_num_hands=2,
                min_detection_confidence=0.5,
                min_tracking_confidence=0.5,
            ) as detector:
        escritor = csv.writer(archivo)
        escritor.writerow(["etiqueta"] + NOMBRES_COLUMNAS)

        print(f"{'video':<25} {'seña':<12} {'cuadros':>8} {'con manos':>10}")
        for ruta in videos:
            leidos, guardados = procesar_video(ruta, detector, escritor)
            aviso = ""
            if leidos == 0:
                aviso = "  <- no se pudo leer: convertilo a .mp4"
            elif guardados < leidos * 0.3:
                aviso = "  <- casi no se ven las manos"
            print(f"{ruta.name:<25} {palabra_del_archivo(ruta):<12} {leidos:>8} {guardados:>10}{aviso}")

    print(f"\nListo. Ejemplos guardados en {salida}")
    print("Ahora entrená el modelo con: .venv\\Scripts\\python.exe camara\\entrenar.py")


if __name__ == "__main__":
    main()
