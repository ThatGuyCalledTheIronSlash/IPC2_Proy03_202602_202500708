using System;
using Backend.SatApi.Datos;
using Backend.SatApi.Dominio;
using Backend.SatApi.Dtos;

namespace Backend.SatApi.Servicios
{
    public class ServicioAutorizacion
    {
        private readonly Repositorio _repositorio;

        public ServicioAutorizacion(Repositorio? repositorio = null)
        {
            _repositorio = repositorio ?? Repositorio.Instancia;
        }

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
                
                // 1. Extraer Fecha (ignora texto basura como "Guatemala, ")
                DateTime? fechaOpt = ExtractorTiempo.Extraer(factura.Tiempo);
                if (fechaOpt.HasValue) 
                {
                    factura.FechaParseada = fechaOpt.Value;
                }
                else
                {
                    // Si no tiene fecha parseable, asignamos la de hoy como fallback
                    factura.FechaParseada = DateTime.Now;
                }

                // 2. Validar formato NIT Emisor y Receptor
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

                // 3. Validar que existan en el RTU
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

                // 4. Validar referencia única
                if (_repositorio.ExisteReferencia(factura.Referencia)) 
                { 
                    RechazarFactura(factura, TipoError.ReferenciaDuplicada); 
                    continue; 
                }

                // 5. Validar matemáticas
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

                // --- FACTURA APROBADA ---
                factura.FueAprobada = true;
                factura.CodigoAutorizacion = _repositorio.GenerarCodigoAutorizacion(factura.FechaParseada);
                _repositorio.FacturasAprobadas.Insertar(factura);

                respuesta.Resultado.Aprobadas++;
                
                detallesAprobados.Insertar(new DetalleAprobacion { 
                    Referencia = factura.Referencia, 
                    CodigoAprobacion = factura.CodigoAutorizacion 
                });
            }
            
            // Convertir la ListaSimple de detalles a un arreglo para el JSON de salida
            respuesta.Resultado.Detalles = new DetalleAprobacion[detallesAprobados.Cantidad];
            Nodo? actual = detallesAprobados.Primero;
            int index = 0;
            while(actual != null) 
            {
                respuesta.Resultado.Detalles[index++] = (DetalleAprobacion)actual.Dato;
                actual = actual.Siguiente;
            }

            // Persistir los cambios (Facturas aprobadas, rechazadas y correlativos)
            _repositorio.GuardarFacturasYCorrelativos();

            return respuesta;
        }
        
        private void RechazarFactura(Factura factura, TipoError motivo)
        {
            factura.FueAprobada = false;
            factura.MotivoRechazo = motivo.ToString();
            _repositorio.FacturasRechazadas.Insertar(factura);
        }
    }
}
