"""
Módulo de cámara de Auna (Python).

Por ahora es una SIMULACIÓN: imprime mensajes de prueba para que la app WPF
pueda probar la recepción (tarea B1) sin tener la cámara funcionando.
En el sprint C se reemplaza por el reconocimiento real con OpenCV + MediaPipe.

Formato del mensaje (una línea por resultado):
    palabra:confianza,palabra:confianza,palabra:confianza
Ejemplo:
    hola:0.91,gracias:0.05,ayuda:0.02
"""
import sys
import time

MENSAJES_DE_PRUEBA = [
    "hola:0.95,gracias:0.03,ayuda:0.02",      # seguro -> la app muestra "HOLA"
    "gracias:0.55,hola:0.30,ayuda:0.15",      # dudoso -> la app muestra sugerencias
    "ayuda:0.88,remedio:0.07,dolor:0.05",
]


def main() -> None:
    for mensaje in MENSAJES_DE_PRUEBA:
        print(mensaje, flush=True)   # flush=True para que la app lo reciba en el momento
        time.sleep(3)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        sys.exit(0)
