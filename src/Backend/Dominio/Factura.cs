using System;
using System.Text.Json.Serialization;

public class Factura
{
    // Propiedades en el JSON
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

    // Propiedades adicionales para el manejo interno
    
    [JsonIgnore]
    public bool FueAprobada { get; set; }

    [JsonIgnore]
    public long CodigoAutorizacion { get; set; }

    [JsonIgnore]
    public string MotivoRechazo { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime FechaParseada { get; set; } //Para filtrar en los reportes
}
