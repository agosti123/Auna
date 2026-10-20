
import cv2
import mediapipe as mp

# Para probar el funcionamiento de la camara, en la terminal se puede ejecutar:
# python probar_mediapipe.py (o fijarse la version de python)
def main():
    # Herramientas de MediaPipe para detectar manos.
    mp_hands = mp.solutions.hands
    mp_drawing = mp.solutions.drawing_utils

    # Abrimos la cámara predeterminada.
    camara = cv2.VideoCapture(0)

    if not camara.isOpened():
        print("Error: no se pudo abrir la cámara.")
        return

    print("Cámara iniciada. Presioná Q para salir.")

    # Configuramos el detector de manos.
    with mp_hands.Hands(
        static_image_mode=False,
        max_num_hands=2,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5
    ) as detector:

        while True:
            correcto, imagen = camara.read()

            if not correcto:
                print("Error: no se pudo leer la imagen.")
                break

            # Convertimos la imagen de BGR a RGB.
            imagen_rgb = cv2.cvtColor(
                imagen, cv2.COLOR_BGR2RGB
            )

            # Detectamos las manos.
            resultados = detector.process(imagen_rgb)

            # Dibujamos puntos y conexiones de las manos.
            if resultados.multi_hand_landmarks:
                for mano in resultados.multi_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        imagen,
                        mano,
                        mp_hands.HAND_CONNECTIONS
                    )

                texto = "Mano detectada"
            else:
                texto = "Mostra una mano a la camara"

            cv2.putText(
                imagen,
                texto,
                (20, 40),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.8,
                (0, 255, 0),
                2
            )

            # Mostramos el video.
            cv2.imshow("Prueba de MediaPipe - Auna", imagen)

            # Salimos al presionar Q.
            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

    camara.release()
    cv2.destroyAllWindows()


if __name__ == "__main__":
    main()
