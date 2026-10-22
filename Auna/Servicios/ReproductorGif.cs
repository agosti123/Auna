
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Auna.Servicios
{
    public class ReproductorGif
    {
        private readonly Image _imagen;
        private readonly DispatcherTimer _temporizador;

        private BitmapFrame[] _cuadros = Array.Empty<BitmapFrame>();
        private int _cuadroActual = 0;
        private double _factorVelocidad = 1.0;
        private bool _finalizado = false;

        public bool EstaPausado { get; private set; } = false;

        public ReproductorGif(Image imagen)
        {
            _imagen = imagen;

            _temporizador = new DispatcherTimer();
            _temporizador.Tick += AvanzarCuadro;
        }

        public void Cargar(string ruta)
        {
            Detener();

            if (!File.Exists(ruta))
                throw new FileNotFoundException(
                    "No se encontró el archivo GIF.", ruta);

            var decoder = new GifBitmapDecoder(
                new Uri(ruta, UriKind.Absolute),
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            if (decoder.Frames.Count == 0)
                throw new InvalidDataException(
                    "El GIF no contiene cuadros.");

            _cuadros = new BitmapFrame[decoder.Frames.Count];

            for (int i = 0; i < decoder.Frames.Count; i++)
            {
                _cuadros[i] = decoder.Frames[i];
            }

            _cuadroActual = 0;
            _factorVelocidad = 1.0;
            _finalizado = false;
            EstaPausado = false;

            _imagen.Source = _cuadros[_cuadroActual];
            ActualizarIntervalo();
            _temporizador.Start();
        }

        private void AvanzarCuadro(object? sender, EventArgs e)
        {
            if (_cuadros.Length == 0 || _finalizado)
                return;

            // Si ya se mostró el último cuadro durante su tiempo,
            // detenemos la animación sin volver al principio.
            if (_cuadroActual >= _cuadros.Length - 1)
            {
                _temporizador.Stop();
                _finalizado = true;
                EstaPausado = false;
                return;
            }

            _cuadroActual++;
            _imagen.Source = _cuadros[_cuadroActual];

            // Cada cuadro tiene su propia duración.
            ActualizarIntervalo();
        }

        private double ObtenerDuracionOriginal(BitmapFrame cuadro)
        {
            try
            {
                if (cuadro.Metadata is BitmapMetadata metadata)
                {
                    // El GIF almacena el retraso en centésimas de segundo.
                    object? valor = metadata.GetQuery("/grctlext/Delay");

                    if (valor != null)
                    {
                        int centesimas = Convert.ToInt32(valor);

                        if (centesimas > 0)
                            return centesimas * 10.0;
                    }
                }
            }
            catch (Exception)
            {
                // Algunos GIF no incluyen metadatos de duración válidos.
            }

            // Duración de respaldo si el GIF no indica un tiempo válido.
            return 100.0;
        }

        private void ActualizarIntervalo()
        {
            if (_cuadros.Length == 0)
                return;

            double duracion = ObtenerDuracionOriginal(
                _cuadros[_cuadroActual]);

            _temporizador.Interval = TimeSpan.FromMilliseconds(
                duracion * _factorVelocidad);
        }

        public void PausarOReanudar()
        {
            if (_cuadros.Length == 0 || _finalizado)
                return;

            if (EstaPausado)
            {
                _temporizador.Start();
                EstaPausado = false;
            }
            else
            {
                _temporizador.Stop();
                EstaPausado = true;
            }
        }

        public void Repetir()
        {
            if (_cuadros.Length == 0)
                return;

            _cuadroActual = 0;
            _finalizado = false;
            EstaPausado = false;

            _imagen.Source = _cuadros[_cuadroActual];
            ActualizarIntervalo();
            _temporizador.Start();
        }

        public void VelocidadNormal()
        {
            EstablecerVelocidad(1.0);
        }

        public void VelocidadLenta()
        {
            EstablecerVelocidad(2.0);
        }

        public void VelocidadMuyLenta()
        {
            EstablecerVelocidad(3.0);
        }

        
        private void EstablecerVelocidad(double factor)
        {
            if (_cuadros.Length == 0)
                return;

            _factorVelocidad = factor;

            // Si el GIF ya terminó, guardamos la velocidad elegida
            // para cuando la persona pulse Repetir.
            if (_finalizado)
                return;

            // Reiniciamos el temporizador para aplicar el nuevo intervalo.
            bool estabaPausado = EstaPausado;

            _temporizador.Stop();
            ActualizarIntervalo();

            if (!estabaPausado)
                _temporizador.Start();
        }


        public void Detener()
        {
            _temporizador.Stop();

            EstaPausado = false;
            _finalizado = false;
            _cuadroActual = 0;
            _factorVelocidad = 1.0;
            _cuadros = Array.Empty<BitmapFrame>();

            _imagen.Source = null;
        }
    }
}
