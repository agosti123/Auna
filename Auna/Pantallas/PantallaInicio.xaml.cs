using System.Windows.Controls;

namespace Auna.Pantallas;

public partial class PantallaInicio : UserControl, IPantalla
{
    public PantallaInicio()
    {
        InitializeComponent();
    }

    public string Titulo => "Inicio";
    public string TextoEstado => "Tab para moverse entre botones · Ctrl+1, Ctrl+2 y Ctrl+3 para elegir un modo";
}
