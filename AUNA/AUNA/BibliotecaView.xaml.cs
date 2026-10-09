using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace AUNA
{
    public class Sena
    {
        public string Palabra { get; set; } = "";
        public string Categoria { get; set; } = "";
        public override string ToString() => Palabra;
    }

    public partial class BibliotecaView : UserControl
    {
        private readonly List<Sena> senas = new()
        {
            new Sena { Palabra = "Hola", Categoria = "Saludos" },
            new Sena { Palabra = "Gracias", Categoria = "Saludos" },
            new Sena { Palabra = "Ayuda", Categoria = "Saludos" },
            new Sena { Palabra = "Remedio", Categoria = "Salud" },
            new Sena { Palabra = "Receta", Categoria = "Salud" },
            new Sena { Palabra = "Dolor", Categoria = "Salud" }
        };

        public BibliotecaView()
        {
            InitializeComponent();

            var categorias = new List<string> { "Todas" };
            categorias.AddRange(senas.Select(s => s.Categoria).Distinct());
            cmbCategoria.ItemsSource = categorias;
            cmbCategoria.SelectedIndex = 0;

            cmbCategoria.SelectionChanged += (s, e) => ActualizarLista();
            lstPalabras.SelectionChanged += (s, e) =>
                txtNombreSena.Text = (lstPalabras.SelectedItem as Sena)?.Palabra ?? "";

            ActualizarLista();
        }

        private void ActualizarLista()
        {
            string categoria = cmbCategoria.SelectedItem as string ?? "Todas";
            lstPalabras.ItemsSource = senas
                .Where(s => categoria == "Todas" || s.Categoria == categoria)
                .ToList();
        }
    }
}
