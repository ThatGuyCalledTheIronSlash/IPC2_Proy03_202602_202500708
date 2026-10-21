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

                // Extracción y parseo de fecha y hora del DTE
                // Si el formato de tiempo no contiene una fecha parseable, se toma la fecha del sistema como fallback
                DateTime? fechaOpt = ExtractorTiempo.Extraer(factura.Tiempo);
                factura.FechaParseada = fechaOpt ?? DateTime.Now;

                // Validación de formato de NIT para emisor y receptor (algoritmo Módulo 11)
                if (!ValidadorNit.EsValido(factura.NitEmisor))
                {
                    RechazarFactura(factura, TipoError.NitEmisorInvalido.ToString());
                    continue;
                }

                if (!ValidadorNit.EsValido(factura.NitReceptor))
                {
                    RechazarFactura(factura, TipoError.NitReceptorInvalido.ToString());
                    continue;
                }

                // Verificación de existencia de contribuyentes en el catálogo RTU
                bool emisorExiste   = _repositorio.BuscarContribuyente(factura.NitEmisor)   != null;
                bool receptorExiste = _repositorio.BuscarContribuyente(factura.NitReceptor) != null;

                if (!emisorExiste || !receptorExiste)
                {
                    if (!emisorExiste)   respuesta.Resultado.RechazadasEmisor++;
                    if (!receptorExiste) respuesta.Resultado.RechazadasReceptor++;

                    // Si faltan ambos, se registran ambos motivos separados por coma para que el reporte de errores los totalice
                    string motivoExistencia;
                    if (!emisorExiste && !receptorExiste)
                    {
                        motivoExistencia = $"{TipoError.NitEmisorInexistente},{TipoError.NitReceptorInexistente}";
                    }
                    else if (!emisorExiste)
                    {
                        motivoExistencia = TipoError.NitEmisorInexistente.ToString();
                    }
                    else
                    {
                        motivoExistencia = TipoError.NitReceptorInexistente.ToString();
                    }

                    RechazarFactura(factura, motivoExistencia);
                    continue;
                }

                // Verificación de unicidad de referencia (máximo 40 caracteres)
                if (string.IsNullOrWhiteSpace(factura.Referencia) || _repositorio.ExisteReferencia(factura.Referencia))
                {
                    RechazarFactura(factura, TipoError.ReferenciaDuplicada.ToString());
                    continue;
                }

                // Validación de valor real positivo
                if (factura.Valor <= 0)
                {
                    RechazarFactura(factura, TipoError.TotalMalCalculado.ToString());
                    continue;
                }

                // Validación de cálculos matemáticos de IVA (12%) y Total
                // Se acepta tanto redondeo comercial (AwayFromZero) como bancario (ToEven)
                decimal ivaComercial = Math.Round(factura.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
                decimal ivaBancario  = Math.Round(factura.Valor * 0.12m, 2, MidpointRounding.ToEven);
                if (factura.Iva != ivaComercial && factura.Iva != ivaBancario)
                {
                    RechazarFactura(factura, TipoError.IvaMalCalculado.ToString());
                    continue;
                }

                decimal totalCalculado = factura.Valor + factura.Iva;
                if (factura.Total != totalCalculado)
                {
                    RechazarFactura(factura, TipoError.TotalMalCalculado.ToString());
                    continue;
                }

                // -------------------------------------------------------------
                // Aprobación y asignación de código correlativo diario
                // El código se genera con la fecha actual de solicitud (DateTime.Now)
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
