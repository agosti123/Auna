using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Auna.Pantallas;

public partial class PantallaTextoSenas : UserControl, IPantalla
{
    private string? _rutaActual;
    private double _velocidad = 1.0;

    public PantallaTextoSenas()
    {
        InitializeComponent();
        Loaded += (_, _) => CuadroTexto.Focus();
    }

    public string Titulo => "Texto → Señas";
    public string TextoEstado => "Escribí una palabra y apretá Enter para ver la seña";

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
        _rutaActual = ruta;

        var imagen = new BitmapImage();
        imagen.BeginInit();
        imagen.UriSource = new Uri(ruta);
        imagen.CacheOption = BitmapCacheOption.OnLoad;
        imagen.EndInit();

        ImageBehavior.SetRepeatBehavior(Personaje, new RepeatBehavior(1));
        ImageBehavior.SetAnimationSpeedRatio(Personaje, _velocidad);
        ImageBehavior.SetAnimatedSource(Personaje, imagen);
        BotonPausa.Content = "Pausa";
    }

    private void Pausa_Click(object sender, RoutedEventArgs e)
    {
        var control = ImageBehavior.GetAnimationController(Personaje);
        if (control == null) return;

        if (control.IsPaused)
        {
            control.Play();
            BotonPausa.Content = "Pausa";
        }
        else
        {
            control.Pause();
            BotonPausa.Content = "Reanudar";
        }
    }

    private void Repetir_Click(object sender, RoutedEventArgs e)
    {
        if (_rutaActual != null) Reproducir(_rutaActual);
    }

    private void MasLento_Click(object sender, RoutedEventArgs e)
    {
        _velocidad = _velocidad > 0.25 ? _velocidad - 0.25 : 1.0;
        ImageBehavior.SetAnimationSpeedRatio(Personaje, _velocidad);
    }
}