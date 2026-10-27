using System;
using System.Windows;
using System.Windows.Controls;
using Auna.Servicios;

namespace Auna.Pantallas;

/// <summary>
/// Pantalla Señas a texto. Diseño: Florencia (A3).
/// Conexión con la cámara: Agostina (C2), usando ConexionCamara (B1).
/// </summary>
public partial class PantallaSenasTexto : UserControl, IPantalla
{
    private readonly ConexionCamara _camara;

    public PantallaSenasTexto(ConexionCamara camara)
    {
        InitializeComponent();

        _camara = camara;
        _camara.ResultadoRecibido += AlRecibirResultado;
        _camara.ErrorOcurrido += AlOcurrirError;
        _camara.Detenida += AlDetenerseCamara;
    }

    public string Titulo => "Señas a texto";

    public string TextoEstado => "Hacé una seña frente a la cámara · Esc para volver al inicio";

    // ----- Cuándo se abre y se cierra la cámara -----

    /// <summary>Al entrar a esta pantalla, la cámara se abre sola.</summary>
    public void AlMostrar() => AbrirCamara();

    /// <summary>Al salir de esta pantalla, la cámara se cierra.</summary>
    public void AlOcultar()
    {
        _camara.Detener();
        TextoEstadoCamara.Text = "Cámara cerrada";
    }

    private void AbrirCamara()
    {
        TextoMensaje.Text = "";
        _camara.Iniciar();

        if (_camara.EstaActiva)
        {
            TextoEstadoCamara.Text = "Se abrió la ventana de la cámara: tocá «Iniciar detección», hacé la seña y tocá «Detener detección».";
            BotonAbrirCamara.Content = "Reiniciar cámara";
        }
    }

    // El botón queda por si la cámara se cerró y la persona quiere volver a abrirla.
    private void AlAbrirCamara(object sender, RoutedEventArgs e)
    {
        _camara.Detener();
        AbrirCamara();
    }

    // ----- Eventos que llegan de la cámara -----

    private void AlRecibirResultado(object? sender, ResultadoReconocimiento resultado)
    {
        PanelSugerencias.Children.Clear();
        TextoMensaje.Text = "";

        if (resultado.EsSeguro)
        {
            // La cámara está segura: mostramos la palabra en grande.
            TextoResultado.Text = resultado.Mejor.Palabra.ToUpper();
            TextoSugerencias.Text = $"Seguridad: {resultado.Mejor.Confianza:P0}";
        }
        else
        {
            // No está segura: no mostramos algo que podría estar mal,
            // ofrecemos las opciones para que la persona elija.
            TextoResultado.Text = "No estoy seguro";
            TextoSugerencias.Text = "Tocá la palabra correcta o repetí la seña:";

            foreach (var opcion in resultado.Opciones)
            {
                var boton = new Button
                {
                    Content = opcion.Palabra,
                    Style = (Style)FindResource("Boton"),
                    Margin = new Thickness(0, 0, 8, 8),
                };
                boton.Click += (_, _) => ElegirSugerencia(opcion.Palabra);
                PanelSugerencias.Children.Add(boton);
            }
        }
    }

    private void ElegirSugerencia(string palabra)
    {
        TextoResultado.Text = palabra.ToUpper();
        TextoSugerencias.Text = "Elegida por la persona.";
        PanelSugerencias.Children.Clear();
    }

    private void AlOcurrirError(object? sender, string mensaje)
    {
        // Mostramos el problema en la pantalla, sin cerrar la app.
        TextoEstadoCamara.Text = mensaje;
        BotonAbrirCamara.Content = "Volver a intentar";
    }

    private void AlDetenerseCamara(object? sender, EventArgs e)
    {
        TextoEstadoCamara.Text = "La cámara se detuvo. Tocá «Reiniciar cámara» para volver a empezar.";
    }

    // ----- Botones de abajo -----

    private void AlNoEntender(object sender, RoutedEventArgs e)
    {
        TextoMensaje.Text = "No se entendió la seña. Intentá hacerla nuevamente.";
    }

    private void AlEscuchar(object sender, RoutedEventArgs e)
    {
        // Tarea B5 (Tomás): leer la palabra en voz alta.
        TextoMensaje.Text = "La reproducción de audio todavía no está implementada.";
    }
}
