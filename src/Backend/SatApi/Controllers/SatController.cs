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
    public class SatController : ControllerBase
    {
        private readonly ServicioRtu _servicioRtu;
        private readonly ServicioAutorizacion _servicioAutorizacion;
        private readonly ServicioReportes _servicioReportes;

        public SatController()
        {
            _servicioRtu = new ServicioRtu();
            _servicioAutorizacion = new ServicioAutorizacion();
            _servicioReportes = new ServicioReportes();
        }

        // ---------------------------------------------------------------------
        // POST: /api/Procesar (Procesamiento unificado de RTU o Autorizaciones)
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

            // Identificación del tipo de datos por propiedad distintiva
            var primerElemento = arrayData[0];

            // Carga de catálogo de contribuyentes (RTU)
            if (primerElemento.TryGetProperty("nombre", out _))
            {
                var contribuyentes = JsonSerializer.Deserialize<ContribuyenteDto[]>(arrayData.GetRawText());
                var respuesta = _servicioRtu.ProcesarRtu(contribuyentes!);
                return Ok(respuesta);
            }
            // Solicitud de autorización de facturas (DTE)
            else if (primerElemento.TryGetProperty("VALOR", out _) || primerElemento.TryGetProperty("valor", out _))
            {
                var facturas = JsonSerializer.Deserialize<Factura[]>(arrayData.GetRawText());
                var dto = new SolicitudAutorizacionDto { Facturas = facturas! };
                var respuesta = _servicioAutorizacion.Procesar(dto);
                return Ok(respuesta);
            }

            return BadRequest(new { error = "Estructura JSON no reconocida. Envíe formato RTU o formato de Facturas." });
        }

        // ---------------------------------------------------------------------
        // GET: /api/ConsultaDatos (Facturación por contribuyente en rango)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ConsultaDatos([FromQuery] string nit, [FromQuery] bool emitidas, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            var respuesta = _servicioReportes.ConsultarContribuyente(nit, emitidas, desde, hasta);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // GET: /api/ResumenIva (Desglose de IVA emitido y recibido por fecha)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ResumenIva([FromQuery] DateTime fecha)
        {
            var respuesta = _servicioReportes.ResumenIvaPorFecha(fecha);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // GET: /api/ResumenRango (Montos autorizados por fecha en un rango)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ResumenRango([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] bool sinIva)
        {
            var respuesta = _servicioReportes.ResumenPorRango(desde, hasta, sinIva);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // GET: /api/ReporteErrores (Diagnóstico de facturas rechazadas)
        // ---------------------------------------------------------------------

        [HttpGet]
        public IActionResult ReporteErrores([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            var respuesta = _servicioReportes.ReporteErrores(desde, hasta);
            return Ok(respuesta);
        }

        // ---------------------------------------------------------------------
        // POST: /api/Inicializar (Reinicio total del estado en memoria y disco)
        // ---------------------------------------------------------------------

        [HttpPost]
        public IActionResult Inicializar()
        {
            Repositorio.Instancia.LimpiarTodo();
            return Ok(new { message = "Sistema devuelto al estado inicial, sin datos." });
        }
    }
}
