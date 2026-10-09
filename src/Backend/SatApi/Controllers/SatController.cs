using Backend.SatApi.Dtos;
using Backend.SatApi.Servicios;
using Backend.SatApi.Datos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System;

namespace Backend.SatApi.Controllers
{
    [ApiController]
    [Route("api/[action]")]
    public class SatController : ControllerBase
    {
        private readonly ServicioRtu _servicioRtu;
        private readonly ServicioAutorizacion _servicioAutorizacion;
        private readonly ServicioReportes _servicioReportes;

        public SatController()
        {
            // Instancia servicios directamente
            _servicioRtu = new ServicioRtu();
            _servicioAutorizacion = new ServicioAutorizacion();
            _servicioReportes = new ServicioReportes();
        }

        [HttpPost]
        public IActionResult Procesar([FromBody] JsonElement arrayData)
        {
            if (arrayData.ValueKind != JsonValueKind.Array)
            {
                return BadRequest(new { error = "El mensaje debe ser un arreglo JSON." });
            }

            if (arrayData.GetArrayLength() == 0)
            {
                return Ok(new { message = "Arreglo vacío" });
            }

            // Usamos el primer elemento para adivinar si nos enviaron RTU o Facturas
            var primerElemento = arrayData[0];
            
            // Si el JSON tiene una propiedad llamada "nombre",es RTU
            if (primerElemento.TryGetProperty("nombre", out _))
            {
                var contribuyentes = JsonSerializer.Deserialize<ContribuyenteDto[]>(arrayData.GetRawText());
                var respuesta = _servicioRtu.ProcesarRtu(contribuyentes!);
                return Ok(respuesta);
            }
            // Si tiene una propiedad llamada "VALOR", son Facturas
            else if (primerElemento.TryGetProperty("VALOR", out _))
            {
                var facturas = JsonSerializer.Deserialize<Factura[]>(arrayData.GetRawText());
                var dto = new SolicitudAutorizacionDto { Facturas = facturas! };
                var respuesta = _servicioAutorizacion.Procesar(dto);
                return Ok(respuesta);
            }

            return BadRequest(new { error = "Estructura JSON no reconocida. Asegúrese de enviar RTU o Autorizaciones." });
        }

        [HttpGet]
        public IActionResult ConsultaDatos([FromQuery] string nit, [FromQuery] bool emitidas, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            var respuesta = _servicioReportes.ConsultarContribuyente(nit, emitidas, desde, hasta);
            return Ok(respuesta);
        }

        [HttpGet]
        public IActionResult ResumenIva([FromQuery] DateTime fecha)
        {
            var respuesta = _servicioReportes.ResumenIvaPorFecha(fecha);
            return Ok(respuesta);
        }

        [HttpGet]
        public IActionResult ResumenRango([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] bool sinIva)
        {
            var respuesta = _servicioReportes.ResumenPorRango(desde, hasta, sinIva);
            return Ok(respuesta);
        }

        [HttpGet]
        public IActionResult ReporteErrores([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            var respuesta = _servicioReportes.ReporteErrores(desde, hasta);
            return Ok(respuesta);
        }

        [HttpPost]
        public IActionResult Inicializar()
        {
            Repositorio.Instancia.LimpiarTodo();
            return Ok(new { message = "Sistema devuelto al estado inicial, sin datos." });
        }
    }
}
