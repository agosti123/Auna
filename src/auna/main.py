from __future__ import annotations

from auna.avatar import build_avatar
from auna.camera import DemoRecognizer, OpenCVCameraRecognizer
from auna.events import EventBus
from auna.translator import GenericTranslator


def build_app():
    bus: EventBus[str] = EventBus()
    translator = GenericTranslator[str](
        dictionary={
            "HOLA": "Hola",
            "GRACIAS": "Gracias",
            "ADIOS": "Adiós",
        }
    )
    avatar = build_avatar()

    try:
        recognizer = OpenCVCameraRecognizer()
    except RuntimeError:
        recognizer = DemoRecognizer()

    last = {"token": None}

    def on_symbol_detected(symbol: str) -> None:
        avatar.update_message(f"Texto reconocido: {translator.translate(symbol)}")

    bus.subscribe("symbol_detected", on_symbol_detected)

    def tick() -> None:
        symbol = recognizer.recognize()
        if symbol and symbol != last["token"]:
            last["token"] = symbol
            bus.publish("symbol_detected", symbol)

    return avatar, tick, recognizer


def main() -> None:
    avatar, tick, recognizer = build_app()
    try:
        avatar.run(tick)
    finally:
        recognizer.close()


if __name__ == "__main__":
    main()
