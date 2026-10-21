using System;
using Backend.SatApi.Datos;
using Backend.SatApi.Dominio;
using Backend.SatApi.Dtos;

namespace Backend.SatApi.Servicios
{
    // -------------------------------------------------------------------------
    // Generación y agregación de reportes estadísticos e históricos
    // -------------------------------------------------------------------------
    public class ServicioReportes
    {
        private readonly Repositorio _repositorio;

        public ServicioReportes(Repositorio? repositorio = null)
        {
            _repositorio = repositorio ?? Repositorio.Instancia;
        }

        // ---------------------------------------------------------------------
        // Reporte: Consultar Contribuyente (Facturas emitidas o recibidas)
        // ---------------------------------------------------------------------

        public RespuestaConsultaDatosDto ConsultarContribuyente(string nit, bool emitidas, DateTime inicio, DateTime fin)
        {
            var respuesta = new RespuestaConsultaDatosDto();
            ListaSimple resumenPorFecha = new ListaSimple();
            string nitLimpio = nit.Trim().ToUpper();

            Nodo? actual = _repositorio.FacturasAprobadas.Primero;

            while (actual != null)
            {
                Factura f = (Factura)actual.Dato;

                if (f.FechaParseada.Date >= inicio.Date && f.FechaParseada.Date <= fin.Date)
                {
                    bool coincide = emitidas 
                        ? (f.NitEmisor.Trim().ToUpper() == nitLimpio) 
                        : (f.NitReceptor.Trim().ToUpper() == nitLimpio);

                    if (coincide)
                    {
                        string fechaStr = f.FechaParseada.ToString("dd/MM/yyyy");
                        ReporteContribuyenteFechaDto? dtoFecha = null;

                        Nodo? actRes = resumenPorFecha.Primero;
                        while (actRes != null)
                        {
                            ReporteContribuyenteFechaDto r = (ReporteContribuyenteFechaDto)actRes.Dato;
                            if (r.Fecha == fechaStr)
                            {
                                dtoFecha = r;
                                break;
                            }
                            actRes = actRes.Siguiente;
                        }

                        if (dtoFecha == null)
                        {
                            dtoFecha = new ReporteContribuyenteFechaDto { Fecha = fechaStr };
                            resumenPorFecha.Insertar(dtoFecha);
                        }

                        dtoFecha.CantidadAutorizadas++;
                        dtoFecha.MontoTotalFacturado += f.Total;
                        respuesta.TotalFacturadoRango += f.Total;
                    }
                }

                actual = actual.Siguiente;
            }

            // Conversión a arreglo y ordenamiento cronológico por fecha
            respuesta.DetallePorFecha = new ReporteContribuyenteFechaDto[resumenPorFecha.Cantidad];
            Nodo? n = resumenPorFecha.Primero;
            int i = 0;

            while (n != null)
            {
                respuesta.DetallePorFecha[i++] = (ReporteContribuyenteFechaDto)n.Dato;
                n = n.Siguiente;
            }

            Array.Sort(respuesta.DetallePorFecha, (a, b) => ParsearFechaLocal(a.Fecha).CompareTo(ParsearFechaLocal(b.Fecha)));

            return respuesta;
        }

        // ---------------------------------------------------------------------
        // Reporte: Resumen de IVA por Fecha y NIT
        // ---------------------------------------------------------------------

        public ResumenIvaNitDto[] ResumenIvaPorFecha(DateTime fecha)
        {
            ListaSimple resumenIva = new ListaSimple();
            Nodo? actual = _repositorio.FacturasAprobadas.Primero;

            while (actual != null)
            {
                Factura f = (Factura)actual.Dato;

                if (f.FechaParseada.Date == fecha.Date)
                {
                    SumarIva(resumenIva, f.NitEmisor, f.Iva, true);
                    SumarIva(resumenIva, f.NitReceptor, f.Iva, false);
                }

                actual = actual.Siguiente;
            }

            ResumenIvaNitDto[] arreglo = new ResumenIvaNitDto[resumenIva.Cantidad];
            Nodo? n = resumenIva.Primero;
            int i = 0;

            while (n != null)
            {
                arreglo[i++] = (ResumenIvaNitDto)n.Dato;
                n = n.Siguiente;
            }

            Array.Sort(arreglo, (a, b) => string.Compare(a.Nit, b.Nit, StringComparison.OrdinalIgnoreCase));

            return arreglo;
        }

        private void SumarIva(ListaSimple lista, string nit, decimal iva, bool esEmitido)
        {
            string nitLimpio = nit.Trim().ToUpper();
            Nodo? act = lista.Primero;
            ResumenIvaNitDto? encontrado = null;

            while (act != null)
            {
                ResumenIvaNitDto r = (ResumenIvaNitDto)act.Dato;
                if (r.Nit == nitLimpio)
                {
                    encontrado = r;
                    break;
                }
                act = act.Siguiente;
            }

            if (encontrado == null)
            {
                encontrado = new ResumenIvaNitDto { Nit = nitLimpio };
                lista.Insertar(encontrado);
            }

            if (esEmitido)
            {
                encontrado.IvaEmitido += iva;
            }
            else
            {
                encontrado.IvaRecibido += iva;
            }
        }

        // ---------------------------------------------------------------------
        // Reporte: Resumen por Rango de Fechas (Con IVA o Sin IVA)
        // ---------------------------------------------------------------------

        public ResumenRangoFechaDto[] ResumenPorRango(DateTime inicio, DateTime fin, bool sinIva)
        {
            ListaSimple resumenRango = new ListaSimple();
            Nodo? actual = _repositorio.FacturasAprobadas.Primero;

            while (actual != null)
            {
                Factura f = (Factura)actual.Dato;

                if (f.FechaParseada.Date >= inicio.Date && f.FechaParseada.Date <= fin.Date)
                {
                    string fechaStr = f.FechaParseada.ToString("dd/MM/yyyy");
                    ResumenRangoFechaDto? dto = null;

                    Nodo? actRes = resumenRango.Primero;
                    while (actRes != null)
                    {
                        ResumenRangoFechaDto r = (ResumenRangoFechaDto)actRes.Dato;
                        if (r.Fecha == fechaStr)
                        {
                            dto = r;
                            break;
                        }
                        actRes = actRes.Siguiente;
                    }

                    if (dto == null)
                    {
                        dto = new ResumenRangoFechaDto { Fecha = fechaStr };
                        resumenRango.Insertar(dto);
                    }

                    dto.ValorAutorizado += sinIva ? f.Valor : f.Total;
                }

                actual = actual.Siguiente;
            }

            ResumenRangoFechaDto[] arreglo = new ResumenRangoFechaDto[resumenRango.Cantidad];
            Nodo? n = resumenRango.Primero;
            int i = 0;

            while (n != null)
            {
                arreglo[i++] = (ResumenRangoFechaDto)n.Dato;
                n = n.Siguiente;
            }

            Array.Sort(arreglo, (a, b) => ParsearFechaLocal(a.Fecha).CompareTo(ParsearFechaLocal(b.Fecha)));

            return arreglo;
        }

        // ---------------------------------------------------------------------
        // Reporte: Diagnóstico de Errores por Rango de Fechas
        // ---------------------------------------------------------------------

        public ReporteErroresFechaDto[] ReporteErrores(DateTime inicio, DateTime fin)
        {
            ListaSimple resumenErrores = new ListaSimple();

            // Conteo de facturas rechazadas agrupadas por tipo de error
            Nodo? actualRechazadas = _repositorio.FacturasRechazadas.Primero;
            while (actualRechazadas != null)
            {
                Factura f = (Factura)actualRechazadas.Dato;

                if (f.FechaParseada.Date >= inicio.Date && f.FechaParseada.Date <= fin.Date)
                {
                    ReporteErroresFechaDto dto = ObtenerOCrearReporte(resumenErrores, f.FechaParseada);
                    dto.TotalRecibidas++;

                    if (f.MotivoRechazo.Contains(TipoError.NitEmisorInvalido.ToString()))         dto.ErroresNitEmisorInvalido++;
                    if (f.MotivoRechazo.Contains(TipoError.NitReceptorInvalido.ToString()))       dto.ErroresNitReceptorInvalido++;
                    if (f.MotivoRechazo.Contains(TipoError.NitEmisorInexistente.ToString()))      dto.ErroresNitEmisorInexistente++;
                    if (f.MotivoRechazo.Contains(TipoError.NitReceptorInexistente.ToString()))    dto.ErroresNitReceptorInexistente++;
                    if (f.MotivoRechazo.Contains(TipoError.IvaMalCalculado.ToString()))           dto.ErroresIvaMalCalculado++;
                    if (f.MotivoRechazo.Contains(TipoError.TotalMalCalculado.ToString()))         dto.ErroresTotalMalCalculado++;
                    if (f.MotivoRechazo.Contains(TipoError.ReferenciaDuplicada.ToString()))       dto.ErroresReferenciaDuplicada++;

                    AgregarNitDistinto(dto.NitsEmisoresUnicos,   f.NitEmisor);
                    AgregarNitDistinto(dto.NitsReceptoresUnicos, f.NitReceptor);
                }

                actualRechazadas = actualRechazadas.Siguiente;
            }

            // Conteo de facturas aprobadas (sin error)
            Nodo? actualAprobadas = _repositorio.FacturasAprobadas.Primero;
            while (actualAprobadas != null)
            {
                Factura f = (Factura)actualAprobadas.Dato;

                if (f.FechaParseada.Date >= inicio.Date && f.FechaParseada.Date <= fin.Date)
                {
                    ReporteErroresFechaDto dto = ObtenerOCrearReporte(resumenErrores, f.FechaParseada);
                    dto.TotalRecibidas++;
                    dto.TotalSinErrores++;

                    AgregarNitDistinto(dto.NitsEmisoresUnicos,   f.NitEmisor);
                    AgregarNitDistinto(dto.NitsReceptoresUnicos, f.NitReceptor);
                }

                actualAprobadas = actualAprobadas.Siguiente;
            }

            // Transferir conteos de NITs únicos al DTO y armar arreglo final ordenado por fecha
            ReporteErroresFechaDto[] arreglo = new ReporteErroresFechaDto[resumenErrores.Cantidad];
            Nodo? n = resumenErrores.Primero;
            int i = 0;

            while (n != null)
            {
                ReporteErroresFechaDto dto = (ReporteErroresFechaDto)n.Dato;
                dto.CantidadEmisoresDistintos  = dto.NitsEmisoresUnicos.Cantidad;
                dto.CantidadReceptoresDistintos = dto.NitsReceptoresUnicos.Cantidad;
                arreglo[i++] = dto;
                n = n.Siguiente;
            }

            Array.Sort(arreglo, (a, b) => ParsearFechaLocal(a.Fecha).CompareTo(ParsearFechaLocal(b.Fecha)));

            return arreglo;
        }

        private ReporteErroresFechaDto ObtenerOCrearReporte(ListaSimple lista, DateTime fecha)
        {
            string fechaStr = fecha.ToString("dd/MM/yyyy");
            Nodo? actRes = lista.Primero;

            while (actRes != null)
            {
                ReporteErroresFechaDto r = (ReporteErroresFechaDto)actRes.Dato;
                if (r.Fecha == fechaStr)
                {
                    return r;
                }
                actRes = actRes.Siguiente;
            }

            var nuevo = new ReporteErroresFechaDto { Fecha = fechaStr };
            lista.Insertar(nuevo);
            return nuevo;
        }

        private static void AgregarNitDistinto(ListaSimple lista, string nit)
        {
            if (string.IsNullOrWhiteSpace(nit)) return;

            string nitLimpio = nit.Trim().ToUpper();
            Nodo? act = lista.Primero;

            while (act != null)
            {
                if ((string)act.Dato == nitLimpio) return;
                act = act.Siguiente;
            }

            lista.Insertar(nitLimpio);
        }

        private static DateTime ParsearFechaLocal(string fechaStr)
        {
            if (DateTime.TryParseExact(fechaStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime resultado))
            {
                return resultado;
            }
            return DateTime.MinValue;
        }
    }
}
