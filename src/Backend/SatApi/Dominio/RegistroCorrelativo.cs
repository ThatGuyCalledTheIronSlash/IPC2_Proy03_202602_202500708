using System;
using System.Text.Json.Serialization;

namespace Backend.SatApi.Dominio
{
    public class RegistroCorrelativo
    {
        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }
        
        [JsonPropertyName("ultimoCorrelativo")]
        public int UltimoCorrelativo { get; set; }
    }
}
