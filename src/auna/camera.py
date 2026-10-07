from __future__ import annotations

from dataclasses import dataclass


@dataclass
class OpenCVCameraRecognizer:
    camera_index: int = 0

    def __post_init__(self) -> None:
        try:
            import cv2  # type: ignore
        except ModuleNotFoundError as exc:
            raise RuntimeError(
                "OpenCV no está instalado. Instala dependencia opcional 'opencv-python'."
            ) from exc

        self._cv2 = cv2
        self._capture = cv2.VideoCapture(self.camera_index)
        if not self._capture.isOpened():
            raise RuntimeError("No se pudo abrir la cámara.")

    def recognize(self) -> str | None:
        ok, frame = self._capture.read()
        if not ok:
            return None

        gray = self._cv2.cvtColor(frame, self._cv2.COLOR_BGR2GRAY)
        mean_brightness = float(gray.mean())

        if mean_brightness > 170:
            return "HOLA"
        if mean_brightness > 120:
            return "GRACIAS"
        if mean_brightness > 70:
            return "ADIOS"
        return None

    def close(self) -> None:
        self._capture.release()


class DemoRecognizer:
    def __init__(self) -> None:
        self._tokens = ["HOLA", "GRACIAS", "ADIOS"]
        self._index = -1

    def recognize(self) -> str | None:
        self._index = (self._index + 1) % len(self._tokens)
        return self._tokens[self._index]

    def close(self) -> None:
        return None
