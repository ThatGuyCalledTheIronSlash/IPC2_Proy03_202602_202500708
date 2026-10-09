using System;
using System.IO;
using System.Text.Json;
using Backend.SatApi.Dtos;
using Backend.SatApi.Dominio;

namespace Backend.SatApi.Datos
{
    public class Repositorio
    {
        // Instancia única (Singleton) para mantener los datos en memoria mientras la API corre
        private static Repositorio? _instancia;
        public static Repositorio Instancia => _instancia ??= new Repositorio();

        public ListaSimple Contribuyentes { get; set; } = new ListaSimple();
        public ListaSimple FacturasAprobadas { get; set; } = new ListaSimple();
        public ListaSimple FacturasRechazadas { get; set; } = new ListaSimple();
        public ListaSimple CorrelativosDiarios { get; set; } = new ListaSimple();

        private readonly string rutaAlmacen;
        private readonly string rutaRtu;
        private readonly string rutaAprobadas;
        private readonly string rutaRechazadas;
        private readonly string rutaCorrelativos;

        public Repositorio()
        {
            // Ubicación de la carpeta Almacen
            rutaAlmacen = Path.Combine(AppContext.BaseDirectory, "Almacen");
            if (!Directory.Exists(rutaAlmacen))
            {
                Directory.CreateDirectory(rutaAlmacen);
            }

            rutaRtu = Path.Combine(rutaAlmacen, "rtu.json");
            rutaAprobadas = Path.Combine(rutaAlmacen, "aprobadas.json");
            rutaRechazadas = Path.Combine(rutaAlmacen, "rechazadas.json");
            rutaCorrelativos = Path.Combine(rutaAlmacen, "correlativos.json");

            CargarDatos();
        }

        public long GenerarCodigoAutorizacion(DateTime fecha)
        {
            DateTime fechaSinHora = fecha.Date;
            RegistroCorrelativo? registro = null;

            Nodo? actual = CorrelativosDiarios.Primero;
            while (actual != null)
            {
                RegistroCorrelativo r = (RegistroCorrelativo)actual.Dato;
                if (r.Fecha == fechaSinHora)
                {
                    registro = r;
                    break;
                }
                actual = actual.Siguiente;
            }

            if (registro == null)
            {
                registro = new RegistroCorrelativo { Fecha = fechaSinHora, UltimoCorrelativo = 0 };
                CorrelativosDiarios.Insertar(registro);
            }

            registro.UltimoCorrelativo++;

            // Formato: yyyyMMdd + ########
            string prefijoFecha = fechaSinHora.ToString("yyyyMMdd");
            string sufijoCorrelativo = registro.UltimoCorrelativo.ToString("D8"); // 8 ceros
            
            return long.Parse(prefijoFecha + sufijoCorrelativo);
        }

        public bool ExisteReferencia(string referencia)
        {
            string refLimpia = referencia.Trim();
            Nodo? actual = FacturasAprobadas.Primero;
            while (actual != null)
            {
                Factura f = (Factura)actual.Dato;
                if (f.Referencia.Trim() == refLimpia) return true;
                actual = actual.Siguiente;
            }
            return false;
        }

        public void GuardarFacturasYCorrelativos()
        {
            GuardarLista(FacturasAprobadas, rutaAprobadas, typeof(Factura[]));
            GuardarLista(FacturasRechazadas, rutaRechazadas, typeof(Factura[]));
            GuardarLista(CorrelativosDiarios, rutaCorrelativos, typeof(RegistroCorrelativo[]));
        }

        private void GuardarLista(ListaSimple lista, string ruta, Type tipoArreglo)
        {
            Array arreglo = Array.CreateInstance(tipoArreglo.GetElementType()!, lista.Cantidad);
            Nodo? actual = lista.Primero;
            int i = 0;
            while (actual != null)
            {
                arreglo.SetValue(actual.Dato, i++);
                actual = actual.Siguiente;
            }
            string json = JsonSerializer.Serialize(arreglo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ruta, json);
        }

        private void CargarDatos()
        {
            CargarRtu();
            CargarLista(FacturasAprobadas, rutaAprobadas, typeof(Factura[]));
            CargarLista(FacturasRechazadas, rutaRechazadas, typeof(Factura[]));
            CargarLista(CorrelativosDiarios, rutaCorrelativos, typeof(RegistroCorrelativo[]));
        }

        private void CargarLista(ListaSimple listaDestino, string ruta, Type tipoArreglo)
        {
            if (!File.Exists(ruta)) return;
            string json = File.ReadAllText(ruta);
            if (string.IsNullOrWhiteSpace(json)) return;

            Array? cargados = (Array?)JsonSerializer.Deserialize(json, tipoArreglo);
            if (cargados != null)
            {
                for (int i = 0; i < cargados.Length; i++)
                {
                    listaDestino.Insertar(cargados.GetValue(i)!);
                }
            }
        }

        /// <summary>
        /// Recorre la ListaSimple nodo por nodo para buscar un contribuyente por su NIT.
        /// </summary>
        public ContribuyenteDto? BuscarContribuyente(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit)) return null;
            string nitLimpio = nit.Trim().ToUpper();

            Nodo? actual = Contribuyentes.Primero;
            while (actual != null)
            {
                ContribuyenteDto c = (ContribuyenteDto)actual.Dato;
                if (c.NIT.Trim().ToUpper() == nitLimpio)
                {
                    return c;
                }
                actual = actual.Siguiente;
            }

            return null;
        }

        /// <summary>
        /// Guarda todos los contribuyentes de la ListaSimple en Almacen/rtu.json
        /// </summary>
        public void GuardarRtu()
        {
            // Convertimos la ListaSimple a un arreglo primitivo para poder serializarlo a JSON
            ContribuyenteDto[] arreglo = new ContribuyenteDto[Contribuyentes.Cantidad];
            Nodo? actual = Contribuyentes.Primero;
            int i = 0;
            while (actual != null)
            {
                arreglo[i++] = (ContribuyenteDto)actual.Dato;
                actual = actual.Siguiente;
            }

            string json = JsonSerializer.Serialize(arreglo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(rutaRtu, json);
        }

        /// <summary>
        /// Carga los datos de Almacen/rtu.json a la ListaSimple en memoria al iniciar
        /// </summary>
        public void CargarRtu()
        {
            if (!File.Exists(rutaRtu)) return;

            string json = File.ReadAllText(rutaRtu);
            if (string.IsNullOrWhiteSpace(json)) return;

            ContribuyenteDto[]? cargados = JsonSerializer.Deserialize<ContribuyenteDto[]>(json);
            if (cargados != null)
            {
                Contribuyentes = new ListaSimple();
                for (int i = 0; i < cargados.Length; i++)
                {
                    Contribuyentes.Insertar(cargados[i]);
                }
            }
        }

        /// <summary>
        /// Reinicia todos los datos en memoria y borra los archivos almacenados.
        /// </summary>
        public void LimpiarTodo()
        {
            Contribuyentes = new ListaSimple();
            FacturasAprobadas = new ListaSimple();
            FacturasRechazadas = new ListaSimple();

            if (File.Exists(rutaRtu)) File.Delete(rutaRtu);
        }
    }
}
