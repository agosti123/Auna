using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
            txtBuscar.TextChanged += (s, e) => ActualizarLista();
            lstPalabras.SelectionChanged += (s, e) =>
                txtNombreSena.Text = (lstPalabras.SelectedItem as Sena)?.Palabra ?? "";

            ActualizarLista();
        }

        private void ActualizarLista()
        {
            string categoria = cmbCategoria.SelectedItem as string ?? "Todas";
            string texto = txtBuscar.Text.Trim();

            var resultado = senas
                .Where(s => (categoria == "Todas" || s.Categoria == categoria)
                         && s.Palabra.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ToList();

            lstPalabras.ItemsSource = resultado;

            if (resultado.Count == 0 && texto.Length > 0)
            {
                var cercana = senas
                    .OrderBy(s => Distancia(s.Palabra.ToLower(), texto.ToLower()))
                    .First();
                bool parecida = Distancia(cercana.Palabra.ToLower(), texto.ToLower()) <= 2;

                txtSugerencia.Text = parecida
                    ? $"¿Quisiste decir «{cercana.Palabra.ToLower()}»?"
                    : "Probá con otra palabra.";
                panelSinResultados.Visibility = Visibility.Visible;
            }
            else
            {
                panelSinResultados.Visibility = Visibility.Collapsed;
            }
        }

        // Cuenta cuántas letras hay que cambiar, agregar o sacar para pasar de una palabra a otra
        private static int Distancia(string a, string b)
        {
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                {
                    int costo = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + costo);
                }
            return d[a.Length, b.Length];
        }
    }
}