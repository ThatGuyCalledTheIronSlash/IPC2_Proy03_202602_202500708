using System;
using System.Text.Json;
using Backend.SatApi.Datos;
using Backend.SatApi.Dominio;
using Backend.SatApi.Dtos;
using Backend.SatApi.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Backend.SatApi.Controllers
{
    // -------------------------------------------------------------------------
    // Controlador REST HTTP para operaciones y consultas del simulador SAT
    // -------------------------------------------------------------------------
    [ApiController]
    [Route("api/[action]")]
    [Route("[action]")]           // ruta alternativa sin prefijo /api/
    public class SatController : ControllerBase
    {
        private readonly ServicioRtu _servicioRtu;
        private readonly ServicioAutorizacion _servicioAutorizacion;
        private readonly ServicioReportes _servicioReportes;

        // Opciones de deserialización tolerantes a mayúsculas/minúsculas
        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public SatController()
        {
            _servicioRtu = new ServicioRtu();
            _servicioAutorizacion = new ServicioAutorizacion();
            _servicioReportes = new ServicioReportes();
        }

        // ---------------------------------------------------------------------
        // POST: /api/Procesar  o  /Procesar
        // (Procesamiento unificado de RTU o Autorizaciones)
        // ---------------------------------------------------------------------

        [HttpPost]
        public IActionResult Procesar([FromBody] JsonElement arrayData)
        {
            if (arrayData.ValueKind != JsonValueKind.Array)
            {
                return BadRequest(new { error = "El cuerpo de la solicitud debe ser un arreglo JSON." });
            }

            if (arrayData.GetArrayLength() == 0)
            {
                return Ok(new { message = "Arreglo vacío recibido." });
            }

            // Identificación del tipo de datos por propiedad distintiva (case-insensitive)
            var primerElemento = arrayData[0];

            // Carga de catálogo de contribuyentes (RTU): busca "nombre" / "Nombre" / "NOMBRE"
            if (TienePropiedad(primerElemento, "nombre"))
            {
                var contribuyentes = JsonSerializer.Deserialize<ContribuyenteDto[]>(arrayData.GetRawText(), OpcionesJson);
                var respuesta = _servicioRtu.ProcesarRtu(contribuyentes!);
                return Ok(respuesta);
            }
            // Solicitud de autorización de facturas (DTE): busca "VALOR" / "valor" / "Valor"
            else if (TienePropiedad(primerElemento, "valor"))
            {
                var facturas = JsonSerializer.Deserialize<Factura[]>(arrayData.GetRawText(), OpcionesJson);
                var dto = new SolicitudAutorizacionDto { Facturas = facturas! };
                var respuesta = _servicioAutorizacion.Procesar(dto);
                return Ok(respuesta);
            }

            return BadRequest(new { error = "Estructura JSON no reconocida. Envíe formato RTU o formato de Facturas." });
        }

        // ---------------------------------------------------------------------
        // GET: /api/ConsultaDatos  o  /ConsultaDatos
        // (Facturación por contribuyente en rango)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ConsultaDatos([FromQuery] string nit, [FromQuery] bool emitidas, [FromQuery] string desde, [FromQuery] string hasta)
        {
            if (!ParsearFecha(desde, out DateTime fechaDesde))
                return BadRequest(new { error = $"Formato de fecha 'desde' no válido: '{desde}'. Use yyyy-MM-dd o dd/MM/yyyy." });

            if (!ParsearFecha(hasta, out DateTime fechaHasta))
                return BadRequest(new { error = $"Formato de fecha 'hasta' no válido: '{hasta}'. Use yyyy-MM-dd o dd/MM/yyyy." });

            var respuesta = _servicioReportes.ConsultarContribuyente(nit, emitidas, fechaDesde, fechaHasta);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // GET: /api/ResumenIva  o  /ResumenIva
        // (Desglose de IVA emitido y recibido por fecha)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ResumenIva([FromQuery] string fecha)
        {
            if (!ParsearFecha(fecha, out DateTime fechaParsed))
                return BadRequest(new { error = $"Formato de fecha no válido: '{fecha}'. Use yyyy-MM-dd o dd/MM/yyyy." });

            var respuesta = _servicioReportes.ResumenIvaPorFecha(fechaParsed);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // GET: /api/ResumenRango  o  /ResumenRango
        // (Montos autorizados por fecha en un rango)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ResumenRango([FromQuery] string desde, [FromQuery] string hasta, [FromQuery] bool sinIva)
        {
            if (!ParsearFecha(desde, out DateTime fechaDesde))
                return BadRequest(new { error = $"Formato de fecha 'desde' no válido: '{desde}'. Use yyyy-MM-dd o dd/MM/yyyy." });

            if (!ParsearFecha(hasta, out DateTime fechaHasta))
                return BadRequest(new { error = $"Formato de fecha 'hasta' no válido: '{hasta}'. Use yyyy-MM-dd o dd/MM/yyyy." });

            var respuesta = _servicioReportes.ResumenPorRango(fechaDesde, fechaHasta, sinIva);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // GET: /api/ReporteErrores  o  /ReporteErrores
        // (Diagnóstico de facturas rechazadas)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ReporteErrores([FromQuery] string desde, [FromQuery] string hasta)
        {
            if (!ParsearFecha(desde, out DateTime fechaDesde))
                return BadRequest(new { error = $"Formato de fecha 'desde' no válido: '{desde}'. Use yyyy-MM-dd o dd/MM/yyyy." });

            if (!ParsearFecha(hasta, out DateTime fechaHasta))
                return BadRequest(new { error = $"Formato de fecha 'hasta' no válido: '{hasta}'. Use yyyy-MM-dd o dd/MM/yyyy." });

            var respuesta = _servicioReportes.ReporteErrores(fechaDesde, fechaHasta);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // POST: /api/Inicializar  o  /Inicializar
        // (Reinicio total del estado en memoria y disco)
        // ---------------------------------------------------------------------

        [HttpPost]
        public IActionResult Inicializar()
        {
            Repositorio.Instancia.LimpiarTodo();
            return Ok(new { message = "Sistema devuelto al estado inicial, sin datos." });
        }

        // ---------------------------------------------------------------------
        // Utilidades privadas
        // ---------------------------------------------------------------------

        /// <summary>
        /// Busca una propiedad en el JsonElement de manera insensible a mayúsculas/minúsculas.
        /// </summary>
        private static bool TienePropiedad(JsonElement elemento, string nombrePropiedad)
        {
            foreach (var prop in elemento.EnumerateObject())
            {
                if (string.Equals(prop.Name, nombrePropiedad, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Parsea una cadena de fecha aceptando múltiples formatos comunes:
        /// yyyy-MM-dd (ISO), dd/MM/yyyy (local guatemalteco), d/M/yyyy, etc.
        /// </summary>
        private static bool ParsearFecha(string valor, out DateTime resultado)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                resultado = default;
                return false;
            }

            string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "d/MM/yyyy", "dd/M/yyyy", "yyyy/MM/dd" };
            return DateTime.TryParseExact(
                valor.Trim(),
                formatos,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out resultado);
        }
    }
}
