using System;
using System.IO;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using Auna.Servicios;

namespace Auna.Pantallas;

public partial class PantallaTextoSenas : UserControl, IPantalla
{
    private string? _rutaActual;
    private readonly SpeechSynthesizer _voz = new();
    private readonly ReproductorGif _reproductor;
    private int _nivelVelocidad = 0;

    public PantallaTextoSenas()
    {
        InitializeComponent();
        _reproductor = new ReproductorGif(Personaje);
        ElegirVozEnEspanol();
        Loaded += (_, _) => CuadroTexto.Focus();
        // Si te vas de la pantalla, la voz deja de hablar.
        Unloaded += (_, _) =>
        {
            _voz.SpeakAsyncCancelAll();
            _reproductor.Detener();
        };
    }

    public string Titulo => "Texto → Señas";
    public string TextoEstado => "Escribí una palabra y apretá Enter para ver la seña";

    private void ElegirVozEnEspanol()
    {
        try
        {
            foreach (var voz in _voz.GetInstalledVoices())
            {
                if (voz.Enabled && voz.VoiceInfo.Culture.TwoLetterISOLanguageName == "es")
                {
                    _voz.SelectVoice(voz.VoiceInfo.Name);
                    break;
                }
            }
        }
        catch (Exception)
        {
            // Si no se puede elegir, se usa la voz que traiga Windows por defecto.
        }
    }

    private void Escuchar_Click(object sender, RoutedEventArgs e)
    {
        string texto = CuadroTexto.Text.Trim();
        if (texto.Length == 0) return;

        try
        {
            _voz.SpeakAsyncCancelAll();
            _voz.SpeakAsync(texto);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No pude reproducir la voz: {ex.Message}", "Auna",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Mostrar_Click(object sender, RoutedEventArgs e)
    {
        string palabra = CuadroTexto.Text.Trim().ToLowerInvariant();
        if (palabra.Length == 0) return;

        string ruta = Path.Combine(AppContext.BaseDirectory,
                                   "Recursos", "animaciones", palabra + ".gif");

        if (!File.Exists(ruta))
        {
            MessageBox.Show($"Todavía no tengo la seña de \"{palabra}\".", "Auna");
            return;
        }

        Reproducir(ruta);
    }

    private void Reproducir(string ruta)
    {
        try
        {
            _reproductor.Cargar(ruta);
            _rutaActual = ruta;
            BotonPausa.Content = "Pausa";
            _nivelVelocidad = 0;
            BotonVelocidad.Content = "Velocidad: Normal";
        }
        catch (Exception ex)
        {
            _reproductor.Detener();
            _rutaActual = null;

            MessageBox.Show(
                $"No se pudo reproducir la animación: {ex.Message}",
                "Auna",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void Pausa_Click(object sender, RoutedEventArgs e)
    {
        _reproductor.PausarOReanudar();

        BotonPausa.Content = _reproductor.EstaPausado
            ? "Reanudar"
            : "Pausa";
    }

    private void Repetir_Click(object sender, RoutedEventArgs e)
    {
        if (_rutaActual != null)
        {
            _reproductor.Repetir();
            BotonPausa.Content = "Pausa";
        }
    }

    private void CambiarVelocidad_Click(object sender, RoutedEventArgs e)
    {
        _nivelVelocidad = (_nivelVelocidad + 1) % 3;

        switch (_nivelVelocidad)
        {
            case 0:
                _reproductor.VelocidadNormal();
                BotonVelocidad.Content = "Velocidad: Normal";
                break;

            case 1:
                _reproductor.VelocidadLenta();
                BotonVelocidad.Content = "Velocidad: Lenta";
                break;

            case 2:
                _reproductor.VelocidadMuyLenta();
                BotonVelocidad.Content = "Velocidad: Muy lenta";
                break;
        }
    }
}