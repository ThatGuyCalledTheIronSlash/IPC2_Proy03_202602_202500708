using System.Text.Json.Serialization;

namespace Frontend.Models
{
    public class ReporteContribuyenteFecha
    {
        [JsonPropertyName("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonPropertyName("cantidadAutorizadas")]
        public int CantidadAutorizadas { get; set; }

        [JsonPropertyName("montoTotalFacturado")]
        public decimal MontoTotalFacturado { get; set; }
    }

    public class RespuestaConsultaDatos
    {
        [JsonPropertyName("detallePorFecha")]
        public ReporteContribuyenteFecha[] DetallePorFecha { get; set; } = new ReporteContribuyenteFecha[0];

        [JsonPropertyName("totalFacturadoRango")]
        public decimal TotalFacturadoRango { get; set; }
    }

    public class ResumenIvaNit
    {
        [JsonPropertyName("nit")]
        public string Nit { get; set; } = string.Empty;

        [JsonPropertyName("ivaEmitido")]
        public decimal IvaEmitido { get; set; }

        [JsonPropertyName("ivaRecibido")]
        public decimal IvaRecibido { get; set; }
    }

    public class ResumenRangoFecha
    {
        [JsonPropertyName("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonPropertyName("valorAutorizado")]
        public decimal ValorAutorizado { get; set; }
    }

    public class ReporteErroresFecha
    {
        [JsonPropertyName("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonPropertyName("totalRecibidas")]
        public int TotalRecibidas { get; set; }

        [JsonPropertyName("erroresNitEmisorInvalido")]
        public int ErroresNitEmisorInvalido { get; set; }

        [JsonPropertyName("erroresNitReceptorInvalido")]
        public int ErroresNitReceptorInvalido { get; set; }

        [JsonPropertyName("erroresNitEmisorInexistente")]
        public int ErroresNitEmisorInexistente { get; set; }

        [JsonPropertyName("erroresNitReceptorInexistente")]
        public int ErroresNitReceptorInexistente { get; set; }

        [JsonPropertyName("erroresIvaMalCalculado")]
        public int ErroresIvaMalCalculado { get; set; }

        [JsonPropertyName("erroresTotalMalCalculado")]
        public int ErroresTotalMalCalculado { get; set; }

        [JsonPropertyName("erroresReferenciaDuplicada")]
        public int ErroresReferenciaDuplicada { get; set; }

        [JsonPropertyName("totalSinErrores")]
        public int TotalSinErrores { get; set; }

        [JsonPropertyName("cantidadEmisoresDistintos")]
        public int CantidadEmisoresDistintos { get; set; }

        [JsonPropertyName("cantidadReceptoresDistintos")]
        public int CantidadReceptoresDistintos { get; set; }
    }
}
