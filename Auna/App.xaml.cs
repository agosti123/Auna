using System.Windows;
using System.Windows.Threading;

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
        MessageBox.Show($"Ocurrió un error inesperado:\n\n{e.Exception.Message}",
                        "Auna - Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
