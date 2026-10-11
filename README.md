# Auna

Aplicación de escritorio para comunicar personas sordas y oyentes. Reconoce señas de Lengua de Señas Argentina (LSA) con la cámara y muestra texto a señas con un personaje animado.

Proyecto integrador de Programación Genérica y Eventos.
Equipo: Agostina Corvalan, Florencia García Zurlo, Tomás Astudillo y Antonella Juárez.

## Tecnologías

- **App:** C# con WPF (.NET 10), en Visual Studio 2026 Community.
- **Cámara:** Python con OpenCV y MediaPipe, en VS Code.

## Cómo abrir el proyecto

1. Clonar el repositorio desde Visual Studio: **Clonar un repositorio** y pegar el link.
2. Abrir `Auna.sln`.
3. Apretar **F5** para compilar y ejecutar.

## Estructura

```
Auna.sln
Auna/
├── App.xaml                 Arranque y estilos globales
├── MainWindow.xaml          Ventana, menú, atajos y barra de estado (Agostina)
├── Comandos.cs              Comandos de navegación con sus atajos
├── Pantallas/
│   ├── IPantalla.cs             Lo que toda pantalla informa (título y estado)
│   ├── PantallaInicio.xaml      Inicio (Agostina)
│   └── PantallaEnConstruccion   Lugar provisorio para las pantallas que faltan
├── Modelo/                  Sena, Biblioteca, excepciones (Antonella, Tomás)
├── Servicios/               Reproductor de GIF, conexión con Python (Florencia, Agostina)
└── Recursos/
    ├── Estilo.xaml          Colores y estilos
    ├── senas.json           Vocabulario
    └── animaciones/         Un GIF por seña
camara/                      Python (por ahora, simulación de mensajes)
docs/                        Bocetos, documentos y registro de reuniones
```

## Cómo sumar tu pantalla

1. En `Pantallas/`, agregar un **Control de usuario (WPF)** que implemente `IPantalla`.
2. Usar los estilos de `Recursos/Estilo.xaml` (`Titulo`, `Texto`, `Boton`, `BotonPrincipal`).
3. En `MainWindow.xaml.cs`, dentro de `CrearPantallas()`, reemplazar la línea de `PantallaEnConstruccion` por tu clase.

## Cómo trabajamos con Git

1. Antes de empezar: actualizar `main` (*pull*).
2. Crear una rama por tarea, por ejemplo `pantalla-biblioteca`.
3. Hacer commits chicos con mensajes claros.
4. Subir la rama y abrir un **Pull Request**. Otro integrante lo revisa antes de unirlo a `main`.

## Atajos de teclado

| Atajo | Acción |
|---|---|
| Ctrl+1 | Señas a texto |
| Ctrl+2 | Texto a señas |
| Ctrl+3 | Biblioteca |
| Esc | Volver al inicio |
| Tab | Moverse entre botones |
