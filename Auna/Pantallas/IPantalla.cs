namespace Auna.Pantallas;

/// <summary>
/// Lo que toda pantalla le informa a la ventana principal.
/// Cada pantalla es un UserControl que implementa esta interfaz.
/// </summary>
public interface IPantalla
{
    /// <summary>Se muestra en la barra de título: "Auna - {Titulo}".</summary>
    string Titulo { get; }

    /// <summary>Se muestra en la barra de estado (por ejemplo, los atajos disponibles).</summary>
    string TextoEstado { get; }

    /// <summary>
    /// Se llama cuando la pantalla aparece. Por defecto no hace nada;
    /// una pantalla lo usa si necesita arrancar algo (por ejemplo, la cámara).
    /// </summary>
    void AlMostrar() { }

    /// <summary>Se llama cuando se deja la pantalla (para detener lo que haya arrancado).</summary>
    void AlOcultar() { }
}
