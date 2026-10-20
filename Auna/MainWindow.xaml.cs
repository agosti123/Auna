using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        // Nos suscribimos a los eventos de la cámara.
        _camara.ResultadoRecibido += AlRecibirResultadoCamara;
        _camara.ErrorOcurrido += AlOcurrirErrorCamara;
        _camara.Detenida += AlDetenerseCamara;
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
            [IdPantalla.Biblioteca] = new PantallaEnConstruccion("Biblioteca", "Antonella"),
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
        // Al cerrar la app, cerramos también el programa de la cámara (tarea C2).
        _camara.Detener();
    }

    // ----- Cámara (tarea B1) -----

    private void AlProbarCamara(object sender, RoutedEventArgs e)
    {
        TextoEstado.Text = "Probando la conexión con la cámara…";
        _camara.Iniciar();
    }

    private void AlRecibirResultadoCamara(object? sender, ResultadoReconocimiento resultado)
    {
        // Por ahora lo mostramos en la barra de estado. Cuando Flor tenga la
        // pantalla Señas a texto, ella se suscribe a este mismo evento.
        if (resultado.EsSeguro)
        {
            TextoEstado.Text = $"Cámara: {resultado.Mejor.Palabra.ToUpper()} ({resultado.Mejor.Confianza:P0})";
        }
        else
        {
            var sugerencias = string.Join(", ", resultado.Opciones.Select(o => o.Palabra));
            TextoEstado.Text = $"Cámara: no estoy seguro. ¿Es alguna de estas? {sugerencias}";
        }
    }

    private void AlOcurrirErrorCamara(object? sender, string mensaje)
    {
        MessageBox.Show(this, mensaje, "Auna - Cámara", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void AlDetenerseCamara(object? sender, EventArgs e)
    {
        TextoEstado.Text += "  ·  La cámara se detuvo.";
    }
}
