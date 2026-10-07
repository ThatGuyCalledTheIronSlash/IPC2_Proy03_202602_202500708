using System.Text.Json.Serialization;

namespace Backend.SatApi.Dtos
{
    public class ContribuyenteDto
    {
        [JsonPropertyName("NIT")]
        public string NIT { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
