
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Auna.Pantallas;

public partial class PantallaSenasTexto : UserControl, IPantalla
{
    private Process? procesoCamara;

    public PantallaSenasTexto()
    {
        InitializeComponent();
    }

    public string Titulo => "Señas a texto";

    public string TextoEstado =>
        "Modo Señas a Texto · Cámara pendiente de conexión";

    private void AlAbrirCamara(object sender, RoutedEventArgs e)
    {
        // Evitamos abrir dos ventanas de cámara al mismo tiempo.
        if (procesoCamara is { HasExited: false })
        {
            TextoEstadoCamara.Text = "La ventana de cámara ya está abierta.";
            return;
        }

        string? rutaScript = BuscarScriptCamara();

        if (rutaScript is null)
        {
            MessageBox.Show(
                "No se encontró camara/ventana_camara.py.\n" +
                "Verificá que el archivo exista dentro de la carpeta camara.",
                "No se encontró la cámara",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        string? carpetaCamara = Path.GetDirectoryName(rutaScript);

        string? raizProyecto = carpetaCamara is null
            ? null
            : Directory.GetParent(carpetaCamara)?.FullName;

        string? rutaPython = raizProyecto is null
            ? null
            : Path.Combine(
                raizProyecto,
                ".venv",
                "Scripts",
                "python.exe"
            );

        if (rutaPython is null || !File.Exists(rutaPython))
        {
            MessageBox.Show(
                "No se encontró el entorno virtual de Python.\n" +
                "Crealo desde la carpeta raíz con:\n" +
                "py -3.12 -m venv .venv",
                "Python no encontrado",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            return;
        }

        try
        {
            var inicio = new ProcessStartInfo
            {
                FileName = rutaPython,
                Arguments = $"\"{rutaScript}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            procesoCamara = new Process
            {
                StartInfo = inicio,
                EnableRaisingEvents = true
            };

            procesoCamara.Exited += (_, _) =>
            {
                // Actualizamos la interfaz sin bloquear el hilo
                // que detecta que Python terminó.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextoEstadoCamara.Text = "Cámara cerrada.";
                    procesoCamara?.Dispose();
                    procesoCamara = null;
                }));
            };

            if (procesoCamara.Start())
            {
                TextoEstadoCamara.Text = "Ventana de cámara abierta.";
            }
            else
            {
                TextoEstadoCamara.Text = "No se pudo abrir la cámara.";
            }
        }
        catch (Exception ex)
        {
            procesoCamara?.Dispose();
            procesoCamara = null;

            TextoEstadoCamara.Text = "Error al abrir la cámara.";

            MessageBox.Show(
                "No se pudo iniciar Python.\n\n" + ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static string? BuscarScriptCamara()
    {
        DirectoryInfo? carpeta = new(AppContext.BaseDirectory);

        while (carpeta is not null)
        {
            string ruta = Path.Combine(
                carpeta.FullName,
                "camara",
                "ventana_camara.py");

            if (File.Exists(ruta))
                return ruta;

            carpeta = carpeta.Parent;
        }

        return null;
    }

    private void AlNoEntender(object sender, RoutedEventArgs e)
    {
        TextoMensaje.Text =
            "No se entendió la seña. Intentá hacerla nuevamente.";
    }

    private void AlEscuchar(object sender, RoutedEventArgs e)
    {
        TextoMensaje.Text =
            "La reproducción de audio todavía no está implementada.";
    }
}
