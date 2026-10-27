using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Auna.Servicios;

/// <summary>
/// Abre el programa de Python de la cámara y escucha lo que imprime.
/// Cada línea que llega se convierte en un evento ResultadoRecibido.
/// Responsable: Agostina (tareas B1 y C2).
/// </summary>
public class ConexionCamara : IDisposable
{
    // Guardamos el "hilo de la interfaz" para avisar los eventos ahí:
    // las pantallas solo se pueden modificar desde ese hilo.
    private readonly SynchronizationContext? _contextoInterfaz = SynchronizationContext.Current;
    private Process? _python;

    /// <summary>
    /// Python que se usa. Si existe el entorno virtual del proyecto (.venv, en la
    /// carpeta raíz del repositorio), usamos ese, que tiene MediaPipe y las demás
    /// librerías instaladas. Si no, probamos con el "python" del sistema.
    /// </summary>
    public string EjecutablePython { get; set; } =
        BuscarEnElProyecto(".venv", "Scripts", "python.exe") ?? "python";

    /// <summary>
    /// Programa de la cámara. Usamos el de la carpeta camara del repositorio, porque
    /// ahí están también los archivos que necesita (ventana_camara.py, reconocedor.py,
    /// grabar_ejemplos.py y modelo.pkl). Si no lo encuentra, usa la copia junto al .exe.
    /// </summary>
    public string RutaScript { get; set; } =
        BuscarEnElProyecto("camara", "reconocer.py")
        ?? Path.Combine(AppContext.BaseDirectory, "camara", "reconocer.py");

    public bool EstaActiva => _python is { HasExited: false };

    // ----- Eventos que pueden escuchar las pantallas -----

    /// <summary>Llegó un resultado válido desde la cámara.</summary>
    public event EventHandler<ResultadoReconocimiento>? ResultadoRecibido;

    /// <summary>Algo salió mal (no hay Python, falta el programa, mensaje inválido).</summary>
    public event EventHandler<string>? ErrorOcurrido;

    /// <summary>El programa de la cámara terminó.</summary>
    public event EventHandler? Detenida;

    public void Iniciar()
    {
        if (EstaActiva) return;

        if (!File.Exists(RutaScript))
        {
            AvisarError($"No se encontró el programa de la cámara en:\n{RutaScript}");
            return;
        }

        var info = new ProcessStartInfo
        {
            FileName = EjecutablePython,
            Arguments = $"-u \"{RutaScript}\"",   // -u: Python manda cada línea en el momento
            RedirectStandardOutput = true,          // queremos leer lo que imprime
            UseShellExecute = false,
            CreateNoWindow = true,                  // sin ventana negra de consola
            StandardOutputEncoding = Encoding.UTF8,
        };

        _python = new Process { StartInfo = info, EnableRaisingEvents = true };
        _python.OutputDataReceived += AlRecibirLinea;   // evento: llegó una línea
        _python.Exited += AlTerminarPython;             // evento: Python se cerró

        try
        {
            _python.Start();
            _python.BeginOutputReadLine();
        }
        catch (Win32Exception)
        {
            _python.Dispose();
            _python = null;
            AvisarError("No se pudo abrir Python. Creá el entorno virtual desde la carpeta raíz del " +
                        "proyecto con: py -3.12 -m venv .venv (ver camara/requirements.txt).");
        }
    }

    public void Detener()
    {
        if (_python == null) return;
        try
        {
            if (!_python.HasExited) _python.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // Ya se había cerrado: no hay nada que hacer.
        }
        _python.Dispose();
        _python = null;
    }

    public void Dispose() => Detener();

    /// <summary>
    /// Busca un archivo de la carpeta raíz del repositorio (por ejemplo
    /// .venv\Scripts\python.exe o camara\reconocer.py) subiendo desde la carpeta
    /// del .exe. Devuelve null si no lo encuentra.
    /// </summary>
    private static string? BuscarEnElProyecto(params string[] partes)
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta != null)
        {
            string ruta = Path.Combine(carpeta.FullName, Path.Combine(partes));
            if (File.Exists(ruta)) return ruta;
            carpeta = carpeta.Parent;
        }
        return null;
    }

    // ----- Manejadores internos -----

    private void AlRecibirLinea(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data)) return;

        try
        {
            var resultado = ResultadoReconocimiento.Interpretar(e.Data);
            EnHiloDeInterfaz(() => ResultadoRecibido?.Invoke(this, resultado));
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            AvisarError($"Llegó un mensaje de la cámara que no se entiende: \"{e.Data}\"");
        }
    }

    private void AlTerminarPython(object? sender, EventArgs e) =>
        EnHiloDeInterfaz(() => Detenida?.Invoke(this, EventArgs.Empty));

    private void AvisarError(string mensaje) =>
        EnHiloDeInterfaz(() => ErrorOcurrido?.Invoke(this, mensaje));

    private void EnHiloDeInterfaz(Action accion)
    {
        if (_contextoInterfaz != null) _contextoInterfaz.Post(_ => accion(), null);
        else accion();
    }
}
