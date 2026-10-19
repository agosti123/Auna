using System;

namespace Auna.Modelo;

/// <summary>Se lanza cuando una palabra no está en la biblioteca de señas.</summary>
public class SenaNoEncontradaException : Exception
{
    public string Palabra { get; }

    public SenaNoEncontradaException(string palabra)
        : base($"Todavía no tengo la seña de \"{palabra}\".")
    {
        Palabra = palabra;
    }
}

/// <summary>Se lanza cuando no se puede usar la cámara o el programa de Python.</summary>
public class CamaraNoDisponibleException : Exception
{
    public CamaraNoDisponibleException()
        : base("No pude usar la cámara. Revisá que esté conectada y que otra aplicación no la esté usando.")
    {
    }

    public CamaraNoDisponibleException(Exception causa)
        : base("No pude usar la cámara. Revisá que esté conectada y que otra aplicación no la esté usando.", causa)
    {
    }
}

/// <summary>Se lanza cuando un archivo (por ejemplo senas.json o un GIF) falta o está dañado.</summary>
public class ArchivoInvalidoException : Exception
{
    public string Ruta { get; }

    public ArchivoInvalidoException(string ruta)
        : base($"No pude leer el archivo \"{ruta}\". Puede faltar o estar dañado.")
    {
        Ruta = ruta;
    }

    public ArchivoInvalidoException(string ruta, Exception causa)
        : base($"No pude leer el archivo \"{ruta}\". Puede faltar o estar dañado.", causa)
    {
        Ruta = ruta;
    }
}