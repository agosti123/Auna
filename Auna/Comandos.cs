using System.Windows.Input;

namespace Auna;

/// <summary>
/// Comandos de navegación de la aplicación. Cada comando tiene su atajo de
/// teclado, así el menú, los botones y el teclado hacen exactamente lo mismo.
/// </summary>
public static class Comandos
{
    public static readonly RoutedUICommand IrInicio = Crear(
        "Inicio", nameof(IrInicio), new KeyGesture(Key.Escape));

    public static readonly RoutedUICommand IrSenasTexto = Crear(
        "Señas a texto", nameof(IrSenasTexto), new KeyGesture(Key.D1, ModifierKeys.Control));

    public static readonly RoutedUICommand IrTextoSenas = Crear(
        "Texto a señas", nameof(IrTextoSenas), new KeyGesture(Key.D2, ModifierKeys.Control));

    public static readonly RoutedUICommand IrBiblioteca = Crear(
        "Biblioteca", nameof(IrBiblioteca), new KeyGesture(Key.D3, ModifierKeys.Control));

    public static readonly RoutedUICommand AcercaDe = Crear(
        "Acerca de Auna", nameof(AcercaDe), null);

    private static RoutedUICommand Crear(string texto, string nombre, KeyGesture? atajo)
    {
        var atajos = new InputGestureCollection();
        if (atajo != null) atajos.Add(atajo);
        return new RoutedUICommand(texto, nombre, typeof(Comandos), atajos);
    }
}
