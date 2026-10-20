
import tkinter as tk
from tkinter import messagebox

import cv2
import mediapipe as mp
from PIL import Image, ImageTk


class VentanaCamara:
    def __init__(self, ventana):
        self.ventana = ventana
        self.ventana.title("Auna - Detección de manos")
        self.ventana.geometry("800x650")

        self.camara = None
        self.deteccion_activa = False
        self.cerrando = False
        self.imagen_tk = None
        self.after_id = None

        self.mp_hands = mp.solutions.hands
        self.mp_drawing = mp.solutions.drawing_utils

        # Área donde se mostrará el video.
        self.video = tk.Label(self.ventana, bg="black")
        self.video.pack(fill="both", expand=True, padx=10, pady=10)

        # Estado de la detección.
        self.estado = tk.Label(
            self.ventana,
            text="Detección detenida",
            font=("Arial", 12)
        )
        self.estado.pack(pady=5)

        # Botón para iniciar o detener la detección.
        self.boton_deteccion = tk.Button(
            self.ventana,
            text="Iniciar detección",
            command=self.alternar_deteccion,
            width=22
        )
        self.boton_deteccion.pack(pady=10)

        self.detector = self.mp_hands.Hands(
            static_image_mode=False,
            max_num_hands=2,
            min_detection_confidence=0.5,
            min_tracking_confidence=0.5
        )

        self.ventana.protocol("WM_DELETE_WINDOW", self.cerrar)
        self.abrir_camara()

    def abrir_camara(self):
        self.camara = cv2.VideoCapture(0)

        if not self.camara.isOpened():
            self.camara.release()
            self.camara = None

            messagebox.showerror(
                "Error de cámara",
                "No se pudo abrir la cámara. "
                "Comprobá que esté conectada y disponible."
            )
            self.cerrar()
            return

        self.actualizar_video()

    def alternar_deteccion(self):
        self.deteccion_activa = not self.deteccion_activa

        if self.deteccion_activa:
            self.boton_deteccion.config(
                text="Detener detección"
            )
            self.estado.config(text="Detección de manos activa")
        else:
            self.boton_deteccion.config(
                text="Iniciar detección"
            )
            self.estado.config(text="Detección detenida")

    
    def actualizar_video(self):
        self.after_id = None

        if self.cerrando or self.camara is None:
            return

        try:
            correcto, imagen = self.camara.read()

            if correcto:
                imagen = cv2.flip(imagen, 1)

                if self.deteccion_activa:
                    imagen_rgb = cv2.cvtColor(
                        imagen, cv2.COLOR_BGR2RGB
                    )

                    resultados = self.detector.process(imagen_rgb)

                    if resultados.multi_hand_landmarks:
                        for mano in resultados.multi_hand_landmarks:
                            self.mp_drawing.draw_landmarks(
                                imagen,
                                mano,
                                self.mp_hands.HAND_CONNECTIONS
                            )

                imagen_rgb = cv2.cvtColor(
                    imagen, cv2.COLOR_BGR2RGB
                )

                imagen_pil = Image.fromarray(imagen_rgb)
                imagen_pil.thumbnail((760, 500))

                self.imagen_tk = ImageTk.PhotoImage(imagen_pil)
                self.video.config(image=self.imagen_tk)

        except Exception as error:
            if not self.cerrando:
                print(f"Error al actualizar el video: {error}")

        finally:
            if not self.cerrando:
                self.after_id = self.ventana.after(
                    15, self.actualizar_video
                )


    
    
    def cerrar(self):
        if self.cerrando:
            return

        self.cerrando = True

        # Cancelamos la próxima actualización del video.
        if self.after_id is not None:
            try:
                self.ventana.after_cancel(self.after_id)
            except tk.TclError:
                pass

            self.after_id = None

        # Liberamos la cámara.
        if self.camara is not None:
            try:
                self.camara.release()
            except Exception as error:
                print(f"Error al liberar la cámara: {error}")
            finally:
                self.camara = None

        # Cerramos MediaPipe.
        try:
            self.detector.close()
        except Exception as error:
            print(f"Error al cerrar el detector: {error}")

        # Destruimos la ventana.
        try:
            self.ventana.destroy()
        except tk.TclError:
            pass

if __name__ == "__main__":
    ventana = tk.Tk()
    app = VentanaCamara(ventana)
    ventana.mainloop()
