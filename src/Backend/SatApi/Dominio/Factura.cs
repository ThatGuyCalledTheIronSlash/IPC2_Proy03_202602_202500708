using System;
using System.Text.Json.Serialization;

namespace Backend.SatApi.Dominio
{
    // -------------------------------------------------------------------------
    // Modelo de entidad para Facturas DTE (solicitud y almacenamiento interno)
    // -------------------------------------------------------------------------
    public class Factura
    {
        // Propiedades presentes en el mensaje JSON de entrada
        [JsonPropertyName("TIEMPO")]
        public string Tiempo { get; set; } = string.Empty;

        [JsonPropertyName("REFERENCIA")]
        public string Referencia { get; set; } = string.Empty;

        [JsonPropertyName("NIT_EMISOR")]
        public string NitEmisor { get; set; } = string.Empty;

        [JsonPropertyName("NIT_RECEPTOR")]
        public string NitReceptor { get; set; } = string.Empty;

        [JsonPropertyName("VALOR")]
        public decimal Valor { get; set; }

        [JsonPropertyName("IVA")]
        public decimal Iva { get; set; }

        [JsonPropertyName("TOTAL")]
        public decimal Total { get; set; }

        // Propiedades de estado y persistencia interna
        public bool FueAprobada { get; set; }

        public long CodigoAutorizacion { get; set; }

        public string MotivoRechazo { get; set; } = string.Empty;

        public DateTime FechaParseada { get; set; }
    }
}
