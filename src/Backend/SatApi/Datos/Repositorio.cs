using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Backend.SatApi.Dtos;
using Backend.SatApi.Dominio;

namespace Backend.SatApi.Datos
{
    // -------------------------------------------------------------------------
    // Repositorio central de datos y persistencia en archivos JSON (Singleton)
    // -------------------------------------------------------------------------
    public class Repositorio
    {
        private static Repositorio? _instancia;
        public static Repositorio Instancia => _instancia ??= new Repositorio();

        // Estructuras en memoria
        public ListaSimple Contribuyentes { get; set; } = new ListaSimple();
        public ListaSimple FacturasAprobadas { get; set; } = new ListaSimple();
        public ListaSimple FacturasRechazadas { get; set; } = new ListaSimple();
        public ListaSimple CorrelativosDiarios { get; set; } = new ListaSimple();

        // Conjunto indexado para validación de unicidad de referencias en tiempo O(1)
        private readonly HashSet<string> _referenciasAprobadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Rutas de archivos en disco
        private readonly string rutaAlmacen;
        private readonly string rutaRtu;
        private readonly string rutaAprobadas;
        private readonly string rutaRechazadas;
        private readonly string rutaCorrelativos;

        // ---------------------------------------------------------------------
        // Inicialización y configuración de rutas
        // ---------------------------------------------------------------------

        public Repositorio()
        {
            string dirActual = Path.Combine(Directory.GetCurrentDirectory(), "Almacen");

            if (Directory.Exists(dirActual))
            {
                rutaAlmacen = dirActual;
            }
            else
            {
                rutaAlmacen = Path.Combine(AppContext.BaseDirectory, "Almacen");
                if (!Directory.Exists(rutaAlmacen))
                {
                    Directory.CreateDirectory(rutaAlmacen);
                }
            }

            rutaRtu = Path.Combine(rutaAlmacen, "rtu.json");
            rutaAprobadas = Path.Combine(rutaAlmacen, "aprobadas.json");
            rutaRechazadas = Path.Combine(rutaAlmacen, "rechazadas.json");
            rutaCorrelativos = Path.Combine(rutaAlmacen, "correlativos.json");

            CargarDatos();
        }

        // ---------------------------------------------------------------------
        // Generación de Códigos de Autorización DTE (yyyymmdd########)
        // ---------------------------------------------------------------------

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
                registro = new RegistroCorrelativo 
                { 
                    Fecha = fechaSinHora, 
                    UltimoCorrelativo = 0 
                };
                CorrelativosDiarios.Insertar(registro);
            }

            registro.UltimoCorrelativo++;

            string prefijoFecha = fechaSinHora.ToString("yyyyMMdd");
            string sufijoCorrelativo = registro.UltimoCorrelativo.ToString("D8");
            
            return long.Parse(prefijoFecha + sufijoCorrelativo);
        }

        // ---------------------------------------------------------------------
        // Búsquedas y validaciones de unicidad
        // ---------------------------------------------------------------------

        public bool ExisteReferencia(string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
            {
                return false;
            }

            string refLimpia = referencia.Trim();
            return _referenciasAprobadas.Contains(refLimpia);
        }

        public void AgregarReferencia(string referencia)
        {
            if (!string.IsNullOrWhiteSpace(referencia))
            {
                _referenciasAprobadas.Add(referencia.Trim());
            }
        }

        public ContribuyenteDto? BuscarContribuyente(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit))
            {
                return null;
            }

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

        // ---------------------------------------------------------------------
        // Persistencia y guardado en disco
        // ---------------------------------------------------------------------

        public void GuardarRtu()
        {
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

        // ---------------------------------------------------------------------
        // Carga de datos desde archivos JSON
        // ---------------------------------------------------------------------

        private void CargarDatos()
        {
            CargarRtu();
            CargarLista(FacturasAprobadas, rutaAprobadas, typeof(Factura[]));
            CargarLista(FacturasRechazadas, rutaRechazadas, typeof(Factura[]));
            CargarLista(CorrelativosDiarios, rutaCorrelativos, typeof(RegistroCorrelativo[]));

            // Sincronizar conjunto de referencias a partir de facturas aprobadas cargadas
            _referenciasAprobadas.Clear();
            Nodo? actual = FacturasAprobadas.Primero;
            while (actual != null)
            {
                if (actual.Dato is Factura f && !string.IsNullOrWhiteSpace(f.Referencia))
                {
                    _referenciasAprobadas.Add(f.Referencia.Trim());
                }
                actual = actual.Siguiente;
            }
        }

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

        // ---------------------------------------------------------------------
        // Reinicio completo del sistema
        // ---------------------------------------------------------------------

        public void LimpiarTodo()
        {
            Contribuyentes = new ListaSimple();
            FacturasAprobadas = new ListaSimple();
            FacturasRechazadas = new ListaSimple();
            CorrelativosDiarios = new ListaSimple();
            _referenciasAprobadas.Clear();

            try { if (File.Exists(rutaRtu)) File.Delete(rutaRtu); } catch { }
            try { if (File.Exists(rutaAprobadas)) File.Delete(rutaAprobadas); } catch { }
            try { if (File.Exists(rutaRechazadas)) File.Delete(rutaRechazadas); } catch { }
            try { if (File.Exists(rutaCorrelativos)) File.Delete(rutaCorrelativos); } catch { }
        }
    }
}
