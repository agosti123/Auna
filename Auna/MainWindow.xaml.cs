using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Auna.Pantallas;

namespace Auna;

/// <summary>
/// Ventana principal: menú, barra de estado, atajos y cambio de pantalla.
/// Responsable: Agostina.
/// </summary>
public partial class MainWindow : Window
{
    // Colección genérica: cada pantalla se busca por su identificador.
    private readonly Dictionary<IdPantalla, UserControl> _pantallas;
    private IdPantalla _actual;

    public MainWindow()
    {
        InitializeComponent();
        _pantallas = CrearPantallas();
        CambiarA(IdPantalla.Inicio);
    }

    private static Dictionary<IdPantalla, UserControl> CrearPantallas()
    {
        // Cuando cada integrante tenga su pantalla, reemplaza la línea de
        // PantallaEnConstruccion por su clase. Por ejemplo:
        //   [IdPantalla.TextoSenas] = new PantallaTextoSenas(),
        return new Dictionary<IdPantalla, UserControl>
        {
            [IdPantalla.Inicio]     = new PantallaInicio(),
            [IdPantalla.SenasTexto] = new PantallaEnConstruccion("Señas a texto", "Florencia"),
            [IdPantalla.TextoSenas] = new PantallaEnConstruccion("Texto a señas", "Tomás"),
            [IdPantalla.Biblioteca] = new BibliotecaView(),
        };
    }

    private void CambiarA(IdPantalla destino)
    {
        if (!_pantallas.TryGetValue(destino, out var pantalla)) return;

        _actual = destino;
        Contenido.Content = pantalla;

        // Retroalimentación: el título y la barra de estado dicen dónde estás.
        if (pantalla is IPantalla info)
        {
            Title = $"Auna - {info.Titulo}";
            TextoEstado.Text = info.TextoEstado;
        }

        // Ponemos el foco en el primer control para poder usar el teclado enseguida.
        Dispatcher.InvokeAsync(
            () => pantalla.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)),
            DispatcherPriority.Input);
    }

    // ----- Manejadores de los comandos (eventos) -----

    private void AlIrInicio(object sender, ExecutedRoutedEventArgs e)     => CambiarA(IdPantalla.Inicio);
    private void AlIrSenasTexto(object sender, ExecutedRoutedEventArgs e) => CambiarA(IdPantalla.SenasTexto);
    private void AlIrTextoSenas(object sender, ExecutedRoutedEventArgs e) => CambiarA(IdPantalla.TextoSenas);
    private void AlIrBiblioteca(object sender, ExecutedRoutedEventArgs e) => CambiarA(IdPantalla.Biblioteca);

    private void AlAcercaDe(object sender, ExecutedRoutedEventArgs e)
    {
        MessageBox.Show(this,
            "Auna\nTraductor entre Lengua de Señas Argentina y texto.\n\n" +
            "Proyecto integrador de Programación Genérica y Eventos.",
            "Acerca de Auna", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AlSalir(object sender, RoutedEventArgs e) => Close();

    private void AlCerrarVentana(object? sender, CancelEventArgs e)
    {
        // Más adelante (tarea C2): acá también se cierra el programa de la cámara.
    }
}
