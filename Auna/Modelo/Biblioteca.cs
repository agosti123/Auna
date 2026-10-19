using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Auna.Modelo
{
    public class Biblioteca
    {
        // La clave es la palabra sin tildes ni mayúsculas
        private readonly Dictionary<string, Sena> _senas = new();

        public Biblioteca(IEnumerable<Sena> senas)
        {
            foreach (var sena in senas)
                _senas[Normalizar(sena.Palabra)] = sena;
        }

        public IEnumerable<Sena> Buscar(string texto)
        {
            string buscado = Normalizar(texto);
            return _senas
                .Where(par => par.Key.Contains(buscado))
                .Select(par => par.Value)
                .OrderBy(sena => sena.Palabra);
        }

        public IEnumerable<string> Categorias()
        {
            return _senas.Values.Select(s => s.Categoria).Distinct().OrderBy(c => c);
        }

        public IEnumerable<Sena> PorCategoria(string categoria)
        {
            return _senas.Values.Where(s => s.Categoria == categoria);
        }

        public static string Normalizar(string texto)
        {
            string descompuesto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var resultado = new StringBuilder();
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    resultado.Append(c);
            }
            return resultado.ToString().Normalize(NormalizationForm.FormC);
        }

        public static T CargarDesdeArchivo<T>(string ruta)
        {
            string json = File.ReadAllText(ruta);
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(json, opciones)
                ?? throw new InvalidDataException($"No se pudo leer {ruta}");
        }

        public static Biblioteca Cargar(string ruta)
        {
            return new Biblioteca(CargarDesdeArchivo<List<Sena>>(ruta));
        }
    }
}