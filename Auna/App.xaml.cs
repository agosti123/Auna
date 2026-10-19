using System.Windows;
using System.Windows.Threading;
using Auna.Modelo;

namespace Auna;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Si ocurre un error que nadie atrapó, mostramos un mensaje claro
        // en lugar de que la aplicación se cierre sin avisar.
        DispatcherUnhandledException += AlOcurrirErrorNoControlado;
        base.OnStartup(e);
    }

    private void AlOcurrirErrorNoControlado(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Errores propios de Auna: ya traen un mensaje claro para la persona.
        bool esperado = e.Exception is SenaNoEncontradaException
                                    or CamaraNoDisponibleException
                                    or ArchivoInvalidoException;

        if (esperado)
        {
            MessageBox.Show(e.Exception.Message, "Auna",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else
        {
            MessageBox.Show($"Ocurrió un error inesperado:\n\n{e.Exception.Message}",
                            "Auna - Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        e.Handled = true; // la aplicación sigue abierta
    }
}