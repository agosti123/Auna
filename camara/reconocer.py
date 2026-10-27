"""
Programa de la cámara que abre la app Auna (pantalla Señas → Texto).

Junta las partes del sprint C:
  - La ventana con la cámara y el botón Iniciar/Detener: ventana_camara.py (Flor).
  - Los puntos de las manos: extraer_puntos() de puntos_mano.py, la MISMA
    función que usa grabar_ejemplos.py (Anto, C3) para grabar los ejemplos.
  - El reconocimiento y el mensaje para la app: reconocedor.py (Agostina, C1).
  - El modelo: modelo.pkl, entrenado con entrenar.py (Tomi, C4).

Cómo se usa:
  1. Tocar "Iniciar detección".
  2. Hacer la seña.
  3. Tocar "Detener detección": se manda el resultado a la app, por ejemplo
         hola:0.91,gracias:0.05,ayuda:0.02

IMPORTANTE: la app lee todo lo que se imprime con print(). Por eso acá solo
se imprimen los resultados; los errores van a sys.stderr.
"""
import sys
import tkinter as tk
from tkinter import messagebox

from puntos_mano import extraer_puntos
from reconocedor import ModeloNoEncontrado, Reconocedor
from ventana_camara import VentanaCamara


class DetectorQueGuarda:
    """
    Envuelve el detector de MediaPipe de la ventana de Flor.
    Cada vez que la ventana busca la mano, nos avisa el resultado
    para que podamos guardar los puntos sin tocar su código.
    """

    def __init__(self, detector, al_detectar):
        self.detector = detector
        self.al_detectar = al_detectar

    def process(self, imagen):
        resultados = self.detector.process(imagen)
        self.al_detectar(resultados)
        return resultados

    def close(self):
        self.detector.close()


class VentanaReconocer(VentanaCamara):
    def __init__(self, ventana, reconocedor):
        self.reconocedor = reconocedor
        self.cuadros = []  # puntos de la mano de cada cuadro mientras se detecta

        super().__init__(ventana)

        if self.cerrando:  # no se pudo abrir la cámara
            return

        self.ventana.title("Auna - Hacé la seña")
        self.ventana.attributes("-topmost", True)  # que quede adelante de la app
        self.estado.config(text="Tocá «Iniciar detección», hacé la seña y tocá «Detener detección»")

        self.detector = DetectorQueGuarda(self.detector, self.guardar_cuadro)

    def guardar_cuadro(self, resultados):
        # 126 números (mano izquierda + mano derecha), o None si no se vio ninguna mano.
        self.cuadros.append(extraer_puntos(resultados))

    def alternar_deteccion(self):
        super().alternar_deteccion()

        if self.deteccion_activa:
            # Empieza una seña nueva: borramos lo de la anterior.
            self.cuadros = []
            return

        # Se tocó "Detener": reconocemos y mandamos el resultado a la app.
        if self.reconocedor.enviar_a_la_app(self.cuadros):
            self.estado.config(text="¡Listo! Mirá el resultado en Auna. Podés hacer otra seña.")
        else:
            self.estado.config(text="No vi ninguna mano. Probá de nuevo con la mano a la vista.")


def main():
    ventana = tk.Tk()

    try:
        reconocedor = Reconocedor()
    except ModeloNoEncontrado as error:
        print(error, file=sys.stderr)
        ventana.withdraw()
        messagebox.showerror(
            "Auna - falta el modelo",
            "Todavía no hay un modelo entrenado.\n\n"
            "Desde la carpeta raíz del proyecto, entrenalo con:\n"
            ".venv\\Scripts\\python.exe camara\\entrenar.py\n\n"
            "O, para probar, creá uno de prueba con:\n"
            ".venv\\Scripts\\python.exe camara\\crear_modelo_prueba.py",
        )
        ventana.destroy()
        return

    VentanaReconocer(ventana, reconocedor)
    ventana.mainloop()


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        sys.exit(0)
