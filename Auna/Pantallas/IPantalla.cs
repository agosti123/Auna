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
}
