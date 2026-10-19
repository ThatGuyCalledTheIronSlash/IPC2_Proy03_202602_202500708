using System;
using Backend.SatApi.Datos;
using Backend.SatApi.Dominio;
using Backend.SatApi.Dtos;

namespace Backend.SatApi.Servicios
{
    // -------------------------------------------------------------------------
    // Validación, autorización y generación de correlativos de facturas DTE
    // -------------------------------------------------------------------------
    public class ServicioAutorizacion
    {
        private readonly Repositorio _repositorio;

        public ServicioAutorizacion(Repositorio? repositorio = null)
        {
            _repositorio = repositorio ?? Repositorio.Instancia;
        }

        // ---------------------------------------------------------------------
        // Procesamiento en lote de solicitudes de autorización
        // ---------------------------------------------------------------------

        public RespuestaAutorizaciones Procesar(SolicitudAutorizacionDto solicitud)
        {
            RespuestaAutorizaciones respuesta = new RespuestaAutorizaciones();

            if (solicitud == null || solicitud.Facturas == null || solicitud.Facturas.Length == 0)
            {
                return respuesta;
            }

            ListaSimple detallesAprobados = new ListaSimple();

            for (int i = 0; i < solicitud.Facturas.Length; i++)
            {
                Factura factura = solicitud.Facturas[i];

                //Extracción y parseo de fecha y hora
                DateTime? fechaOpt = ExtractorTiempo.Extraer(factura.Tiempo);
                factura.FechaParseada = fechaOpt ?? DateTime.Now;

                //Validación de formato de NIT para emisor y receptor
                if (!ValidadorNit.EsValido(factura.NitEmisor))
                {
                    RechazarFactura(factura, TipoError.NitEmisorInvalido);
                    continue;
                }

                if (!ValidadorNit.EsValido(factura.NitReceptor))
                {
                    RechazarFactura(factura, TipoError.NitReceptorInvalido);
                    continue;
                }

                //Verificación de existencia de contribuyentes en el RTU
                if (_repositorio.BuscarContribuyente(factura.NitEmisor) == null)
                {
                    respuesta.Resultado.RechazadasEmisor++;
                    RechazarFactura(factura, TipoError.NitEmisorInexistente);
                    continue;
                }

                if (_repositorio.BuscarContribuyente(factura.NitReceptor) == null)
                {
                    respuesta.Resultado.RechazadasReceptor++;
                    RechazarFactura(factura, TipoError.NitReceptorInexistente);
                    continue;
                }

                //Verificación de unicidad de referencia
                if (_repositorio.ExisteReferencia(factura.Referencia))
                {
                    RechazarFactura(factura, TipoError.ReferenciaDuplicada);
                    continue;
                }

                //Validación de cálculos matemáticos de IVA (12%) y Total
                decimal ivaCalculado = Math.Round(factura.Valor * 0.12m, 2);
                if (factura.Iva != ivaCalculado)
                {
                    RechazarFactura(factura, TipoError.IvaMalCalculado);
                    continue;
                }

                decimal totalCalculado = factura.Valor + factura.Iva;
                if (factura.Total != totalCalculado)
                {
                    RechazarFactura(factura, TipoError.TotalMalCalculado);
                    continue;
                }

// -------------------------------------------------------------
// Aprobación y asignación de código correlativo
// -------------------------------------------------------------
                factura.FueAprobada = true;
                factura.CodigoAutorizacion = _repositorio.GenerarCodigoAutorizacion(factura.FechaParseada);
                _repositorio.FacturasAprobadas.Insertar(factura);

                respuesta.Resultado.Aprobadas++;

                detallesAprobados.Insertar(new DetalleAprobacion
                {
                    Referencia = factura.Referencia,
                    CodigoAprobacion = factura.CodigoAutorizacion
                });
            }

            // Conversión de la lista enlazada a arreglo para el JSON de salida
            respuesta.Resultado.Detalles = new DetalleAprobacion[detallesAprobados.Cantidad];
            Nodo? actual = detallesAprobados.Primero;
            int index = 0;

            while (actual != null)
            {
                respuesta.Resultado.Detalles[index++] = (DetalleAprobacion)actual.Dato;
                actual = actual.Siguiente;
            }

            // Persistencia de facturas y correlativos en disco
            _repositorio.GuardarFacturasYCorrelativos();

            return respuesta;
        }

// ---------------------------------------------------------------------
// Registro de rechazo de factura
// ---------------------------------------------------------------------

        private void RechazarFactura(Factura factura, TipoError motivo)
        {
            factura.FueAprobada = false;
            factura.MotivoRechazo = motivo.ToString();
            _repositorio.FacturasRechazadas.Insertar(factura);
        }
    }
}
