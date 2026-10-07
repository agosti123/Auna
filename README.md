# Auna

Auna es un proyecto integrador de **Programación Genérica y Eventos** para una demo de:

- traducción básica de señas LSA,
- reconocimiento de símbolos desde cámara,
- representación de texto mediante un personaje 2D.

## MVP implementado

- **Programación genérica**: `GenericTranslator[T]` para traducir símbolos a texto.
- **Eventos**: `EventBus[T]` para desacoplar reconocimiento y visualización.
- **Reconocimiento por cámara**:
  - `OpenCVCameraRecognizer` (opcional, usando `opencv-python`).
  - Si OpenCV/cámara no está disponible, fallback automático a `DemoRecognizer`.
- **Personaje 2D**: `Avatar2D` con `tkinter` que actualiza expresión y texto traducido.
  - Si `tkinter` no está disponible en el sistema, usa fallback a modo terminal para demo.

## Ejecutar

```bash
python -m pip install -e .
python -m auna.main
```

Para habilitar cámara real con OpenCV:

```bash
python -m pip install -e .[camera]
python -m auna.main
```

## Pruebas

```bash
python -m pip install -e .
python -m unittest tests/test_events.py tests/test_translator.py
```
