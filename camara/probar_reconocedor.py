"""
Prueba reconocedor.py sin cámara (tarea C1, parte de Agostina).

Simula lo que va a mandar la ventana de Flor al tocar "Detener detección":
una lista con los puntos de cada cuadro.

Uso (desde la carpeta raíz del proyecto):
    .venv\\Scripts\\python.exe camara\\crear_modelo_prueba.py
    .venv\\Scripts\\python.exe camara\\probar_reconocedor.py
"""
from crear_modelo_prueba import puntos_inventados
from reconocedor import Reconocedor

reconocedor = Reconocedor()

print("1) Seña clara de 'hola' (30 cuadros). Tiene que ganar hola con mucha seguridad:")
cuadros = [puntos_inventados("hola") for _ in range(30)]
reconocedor.enviar_a_la_app(cuadros)

print("\n2) Seña dudosa: mitad 'gracias' y mitad 'perdón'. La seguridad tiene que ser baja:")
cuadros = [puntos_inventados("gracias") for _ in range(15)]
cuadros += [puntos_inventados("perdón") for _ in range(15)]
reconocedor.enviar_a_la_app(cuadros)

print("\n3) Algunos cuadros sin mano (None). Igual tiene que reconocer 'ayuda':")
cuadros = [puntos_inventados("ayuda") for _ in range(20)] + [None] * 10
reconocedor.enviar_a_la_app(cuadros)

print("\n4) Ningún cuadro con mano. No tiene que mandar nada:")
se_mando = reconocedor.enviar_a_la_app([None, None, None])
print("   (no se mandó nada)" if not se_mando else "   ERROR: se mandó algo")
