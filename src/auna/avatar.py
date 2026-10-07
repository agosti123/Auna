from __future__ import annotations

from typing import Callable, Protocol


class Avatar(Protocol):
    def update_message(self, message: str) -> None: ...

    def run(self, tick: Callable[[], None]) -> None: ...


class TerminalAvatar2D:
    def __init__(self) -> None:
        self._message = "Esperando señas..."

    def update_message(self, message: str) -> None:
        self._message = message
        print(f"[Avatar2D] {message}")

    def run(self, tick: Callable[[], None]) -> None:
        for _ in range(6):
            tick()
        print(f"[Avatar2D] Estado final: {self._message}")


def build_avatar() -> Avatar:
    try:
        import tkinter as tk
    except ModuleNotFoundError:
        return TerminalAvatar2D()

    class TkAvatar2D:
        def __init__(self) -> None:
            self.root = tk.Tk()
            self.root.title("Auna Avatar 2D")

            self.canvas = tk.Canvas(self.root, width=360, height=240, bg="#f6f8fa")
            self.canvas.pack(padx=12, pady=12)

            self.canvas.create_oval(120, 40, 240, 160, fill="#ffd7b5", outline="#333")
            self._mouth = self.canvas.create_line(150, 125, 210, 125, width=4, fill="#333")
            self._text = self.canvas.create_text(
                180, 200, text="Esperando señas...", font=("Arial", 12)
            )

        def update_message(self, message: str) -> None:
            self.canvas.itemconfigure(self._text, text=message)
            if "Hola" in message:
                self.canvas.coords(self._mouth, 150, 122, 180, 136, 210, 122)
            elif "Gracias" in message:
                self.canvas.coords(self._mouth, 150, 130, 210, 130)
            else:
                self.canvas.coords(self._mouth, 150, 136, 180, 122, 210, 136)

        def run(self, tick: Callable[[], None]) -> None:
            def loop() -> None:
                tick()
                self.root.after(600, loop)

            self.root.after(0, loop)
            self.root.mainloop()

    return TkAvatar2D()
