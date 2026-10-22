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

                // Extracción y parseo de fecha y hora del DTE. Si el formato no es válido o fecha inexistente, se descarta
                DateTime? fechaOpt = ExtractorTiempo.Extraer(factura.Tiempo);
                if (!fechaOpt.HasValue)
                {
                    // Descarte por formato: no contiene fecha y hora válidas
                    continue;
                }
                factura.FechaParseada = fechaOpt.Value;

                ListaSimple erroresFactura = new ListaSimple();

                //Verificación de NIT Emisor (formato Módulo 11 y existencia en RTU)
                if (!ValidadorNit.EsValido(factura.NitEmisor))
                {
                    erroresFactura.Insertar(TipoError.NitEmisorInvalido.ToString());
                }
                else if (_repositorio.BuscarContribuyente(factura.NitEmisor) == null)
                {
                    erroresFactura.Insertar(TipoError.NitEmisorInexistente.ToString());
                    respuesta.Resultado.RechazadasEmisor++;
                }

                //Verificación de NIT Receptor (formato Módulo 11 y existencia en RTU)
                if (!ValidadorNit.EsValido(factura.NitReceptor))
                {
                    erroresFactura.Insertar(TipoError.NitReceptorInvalido.ToString());
                }
                else if (_repositorio.BuscarContribuyente(factura.NitReceptor) == null)
                {
                    erroresFactura.Insertar(TipoError.NitReceptorInexistente.ToString());
                    respuesta.Resultado.RechazadasReceptor++;
                }

                //Verificación de Referencia única (máximo 40 posiciones)
                if (string.IsNullOrWhiteSpace(factura.Referencia) || _repositorio.ExisteReferencia(factura.Referencia))
                {
                    erroresFactura.Insertar(TipoError.ReferenciaDuplicada.ToString());
                }

                //Verificación de IVA (12% con redondeo a 2 decimales)
                decimal ivaComercial = Math.Round(factura.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
                decimal ivaBancario  = Math.Round(factura.Valor * 0.12m, 2, MidpointRounding.ToEven);
                if (factura.Iva != ivaComercial && factura.Iva != ivaBancario)
                {
                    erroresFactura.Insertar(TipoError.IvaMalCalculado.ToString());
                }

                //Verificación matemática de Total (VALOR + IVA y valor positivo)
                decimal totalCalculado = factura.Valor + factura.Iva;
                if (factura.Valor <= 0 || factura.Total != totalCalculado)
                {
                    erroresFactura.Insertar(TipoError.TotalMalCalculado.ToString());
                }

                // Si se detectó uno o más errores, se registra el rechazo con todos los motivos
                if (erroresFactura.Cantidad > 0)
                {
                    string[] motivosArray = new string[erroresFactura.Cantidad];
                    Nodo? actErr = erroresFactura.Primero;
                    int idxErr = 0;
                    while (actErr != null)
                    {
                        motivosArray[idxErr++] = (string)actErr.Dato;
                        actErr = actErr.Siguiente;
                    }

                    RechazarFactura(factura, string.Join(",", motivosArray));
                    continue;
                }

                // -------------------------------------------------------------
                // Aprobación y asignación de código correlativo diario
                // -------------------------------------------------------------
                factura.FueAprobada = true;
                factura.CodigoAutorizacion = _repositorio.GenerarCodigoAutorizacion(DateTime.Now);
                _repositorio.FacturasAprobadas.Insertar(factura);
                _repositorio.AgregarReferencia(factura.Referencia);

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

        private void RechazarFactura(Factura factura, string motivo)
        {
            factura.FueAprobada = false;
            factura.MotivoRechazo = motivo;
            _repositorio.FacturasRechazadas.Insertar(factura);
        }
    }
}
