using System;

namespace Backend.SatApi.Dtos
{
    // DTO para Consultar Contribuyente (/ConsultaDatos)
    public class ReporteContribuyenteFechaDto
    {
        public string Fecha { get; set; } = string.Empty;
        public int CantidadAutorizadas { get; set; }
        public decimal MontoTotalFacturado { get; set; }
    }

    public class RespuestaConsultaDatosDto
    {
        public ReporteContribuyenteFechaDto[] DetallePorFecha { get; set; } = new ReporteContribuyenteFechaDto[0];
        public decimal TotalFacturadoRango { get; set; }
    }

    // DTO para Resumen de IVA (/ResumenIva)
    public class ResumenIvaNitDto
    {
        public string Nit { get; set; } = string.Empty;
        public decimal IvaEmitido { get; set; }
        public decimal IvaRecibido { get; set; }
    }

    // DTO para Resumen por Rango (/ResumenRango)
    public class ResumenRangoFechaDto
    {
        public string Fecha { get; set; } = string.Empty;
        public decimal ValorAutorizado { get; set; } // Puede ser con o sin IVA según el parámetro
    }

    // DTO para Reporte de Errores por Fecha (/ReporteErrores)
    public class ReporteErroresFechaDto
    {
        public string Fecha { get; set; } = string.Empty;
        public int TotalRecibidas { get; set; }
        public int ErroresNitEmisorInvalido { get; set; }
        public int ErroresNitReceptorInvalido { get; set; }
        public int ErroresNitEmisorInexistente { get; set; }
        public int ErroresNitReceptorInexistente { get; set; }
        public int ErroresIvaMalCalculado { get; set; }
        public int ErroresTotalMalCalculado { get; set; }
        public int ErroresReferenciaDuplicada { get; set; }
        public int TotalSinErrores { get; set; }
        public int CantidadEmisoresDistintos { get; set; }
        public int CantidadReceptoresDistintos { get; set; }
    }
}
