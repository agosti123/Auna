using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Auna.Servicios;

/// <summary>Una opción que manda la cámara: una palabra y qué tan segura está (de 0 a 1).</summary>
public record OpcionSena(string Palabra, double Confianza);

/// <summary>
/// Lo que llega en cada mensaje de la cámara: hasta 3 opciones, de la más
/// probable a la menos probable. Responsable: Agostina (tarea B1).
/// </summary>
public class ResultadoReconocimiento
{
    /// <summary>A partir de esta confianza mostramos la palabra; por debajo, sugerencias.</summary>
    public const double UmbralSeguridad = 0.8;

    public IReadOnlyList<OpcionSena> Opciones { get; }

    public ResultadoReconocimiento(IEnumerable<OpcionSena> opciones)
    {
        // LINQ: ordenamos de mayor a menor confianza.
        Opciones = opciones.OrderByDescending(o => o.Confianza).ToList();
        if (Opciones.Count == 0)
            throw new ArgumentException("El mensaje de la cámara no trae ninguna opción.");
    }

    /// <summary>La opción más probable.</summary>
    public OpcionSena Mejor => Opciones[0];

    /// <summary>True si la cámara está segura; false si hay que mostrar sugerencias.</summary>
    public bool EsSeguro => Mejor.Confianza >= UmbralSeguridad;

    /// <summary>
    /// Convierte una línea como "hola:0.91,gracias:0.05,ayuda:0.02" en un resultado.
    /// Lanza FormatException si la línea no tiene ese formato.
    /// </summary>
    public static ResultadoReconocimiento Interpretar(string linea)
    {
        var opciones = new List<OpcionSena>();

        var partes = linea.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var parte in partes)
        {
            var datos = parte.Split(':');
            if (datos.Length != 2)
                throw new FormatException($"Parte mal formada: \"{parte}\"");

            // InvariantCulture: Python manda "0.91" con punto. Con la configuración
            // de Argentina, C# esperaría coma ("0,91") y fallaría sin esto.
            double confianza = double.Parse(datos[1], CultureInfo.InvariantCulture);
            opciones.Add(new OpcionSena(datos[0].Trim(), confianza));
        }

        return new ResultadoReconocimiento(opciones);
    }
}
