using System.Text.Json.Serialization;

// Modelos para la Respuesta de Actualizar RTU
public class RespuestaRtu
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")]
    public int Nuevos { get; set; }

    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")]
    public int Actualizados { get; set; }

    [JsonPropertyName("NITS_INVALIDOS")]
    public int Invalidos { get; set; }
}

// Modelos para la Respuesta de Autorización de Facturas
public class DetalleAprobacion
{
    [JsonPropertyName("referencia")]
    public string Referencia { get; set; } = string.Empty;

    [JsonPropertyName("codigoAprobacion")]
    public long CodigoAprobacion { get; set; }
}

public class ResultadoAutorizaciones
{
    [JsonPropertyName("cantidadAutorizacionesAprobadas")]
    public int Aprobadas { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")]
    public int RechazadasEmisor { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")]
    public int RechazadasReceptor { get; set; }

    [JsonPropertyName("detalleAprobaciones")]
    public DetalleAprobacion[] Detalles { get; set; } = new DetalleAprobacion[0];
}

public class RespuestaAutorizaciones
{
    [JsonPropertyName("resultadoAutorizaciones")]
    public ResultadoAutorizaciones Resultado { get; set; } = new ResultadoAutorizaciones();
}
