using System.Windows.Controls;

namespace Auna.Pantallas;

public partial class PantallaEnConstruccion : UserControl, IPantalla
{
    public PantallaEnConstruccion(string titulo, string responsable)
    {
        InitializeComponent();
        Titulo = titulo;
        TextoTitulo.Text = titulo;
        TextoAviso.Text = $"Esta pantalla está en construcción. La arma {responsable}.";
    }

    public string Titulo { get; }
    public string TextoEstado => "Esc para volver al inicio";
}
