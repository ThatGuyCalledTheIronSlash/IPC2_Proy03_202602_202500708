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
}
