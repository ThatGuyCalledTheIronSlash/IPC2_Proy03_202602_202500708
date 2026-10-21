using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages.Peticiones
{
    public class ResumenIvaModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _backendUrl;

        public ResumenIvaModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _backendUrl = (configuration["BackendUrl"] ?? "http://localhost:5000").TrimEnd('/');
        }

        [BindProperty]
        public DateTime? Fecha { get; set; }

        public ResumenIvaNit[]? Resultados { get; set; }
        public string? MensajeError { get; set; }
        public bool ConsultaRealizada { get; set; }

        public void OnGet()
        {
            // Fecha predeterminada de prueba (15/01/2025)
            Fecha = new DateTime(2025, 1, 15);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!Fecha.HasValue)
            {
                MensajeError = "Debe seleccionar una fecha para consultar el resumen.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                string url = $"{_backendUrl}/api/ResumenIva?fecha={Fecha.Value:yyyy-MM-dd}";

                var response = await client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Resultados = JsonSerializer.Deserialize<ResumenIvaNit[]>(responseBody) ?? new ResumenIvaNit[0];
                    ConsultaRealizada = true;
                }
                else
                {
                    MensajeError = $"Error de la API: {responseBody}";
                }
            }
            catch (HttpRequestException)
            {
                MensajeError = $"No se pudo conectar con la API de la SAT en {_backendUrl}. Verifique que el Backend esté encendido.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error inesperado: {ex.Message}";
            }

            return Page();
        }
    }
}
