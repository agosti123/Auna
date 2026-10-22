using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Auna.Pantallas;
using Auna.Servicios;

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

    // Conexión con el programa de Python de la cámara (tarea B1).
    private readonly ConexionCamara _camara = new();

    public MainWindow()
    {
        InitializeComponent();
        _pantallas = CrearPantallas();
        CambiarA(IdPantalla.Inicio);
    }

    private Dictionary<IdPantalla, UserControl> CrearPantallas()
    {
        return new Dictionary<IdPantalla, UserControl>
        {
            [IdPantalla.Inicio] = new PantallaInicio(),
            // La pantalla de Señas a texto recibe la conexión con la cámara (tarea C2).
            [IdPantalla.SenasTexto] = new PantallaSenasTexto(_camara),
            [IdPantalla.TextoSenas] = new PantallaTextoSenas(),
            [IdPantalla.Biblioteca] = new BibliotecaView(),
        };
    }

    private void CambiarA(IdPantalla destino)
    {
        if (!_pantallas.TryGetValue(destino, out var pantalla)) return;

        // Avisamos a la pantalla que se va (por ejemplo, para que cierre la cámara).
        if (Contenido.Content is IPantalla anterior && !ReferenceEquals(anterior, pantalla))
            anterior.AlOcultar();

        _actual = destino;
        Contenido.Content = pantalla;

        // Retroalimentación: el título y la barra de estado dicen dónde estás.
        if (pantalla is IPantalla info)
        {
            Title = $"Auna - {info.Titulo}";
            TextoEstado.Text = info.TextoEstado;

            // Avisamos a la pantalla que aparece (por ejemplo, para que abra la cámara).
            info.AlMostrar();
        }

        // Ponemos el foco en el primer control para poder usar el teclado enseguida.
        Dispatcher.InvokeAsync(
            () => pantalla.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)),
            DispatcherPriority.Input);
    }

    // ----- Manejadores de los comandos (eventos) -----

    private void AlIrInicio(object sender, ExecutedRoutedEventArgs e) => CambiarA(IdPantalla.Inicio);
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
        // Al cerrar la app, cerramos también el programa de la cámara (tarea C2).
        _camara.Detener();
    }
}
