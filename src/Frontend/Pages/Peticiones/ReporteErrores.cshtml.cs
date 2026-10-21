using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages.Peticiones
{
    public class ReporteErroresModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _backendUrl;

        public ReporteErroresModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _backendUrl = (configuration["BackendUrl"] ?? "http://localhost:5000").TrimEnd('/');
        }

        [BindProperty]
        public DateTime? Desde { get; set; }

        [BindProperty]
        public DateTime? Hasta { get; set; }

        public ReporteErroresFecha[]? Resultados { get; set; }
        public string? MensajeError { get; set; }
        public bool ConsultaRealizada { get; set; }

        public void OnGet()
        {
            // Rango predeterminado que abarca las fechas de prueba del enunciado (2025 en adelante)
            Desde = new DateTime(2025, 1, 1);
            Hasta = DateTime.Now;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!Desde.HasValue || !Hasta.HasValue)
            {
                MensajeError = "Debe ingresar un rango de fechas válido.";
                return Page();
            }

            if (Desde.Value > Hasta.Value)
            {
                MensajeError = "La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                string url = $"{_backendUrl}/api/ReporteErrores?desde={Desde.Value:yyyy-MM-dd}&hasta={Hasta.Value:yyyy-MM-dd}";

                var response = await client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Resultados = JsonSerializer.Deserialize<ReporteErroresFecha[]>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ReporteErroresFecha[0];
                    ConsultaRealizada = true;
                }
                else
                {
                    MensajeError = $"Error de la API: {responseBody}";
                }
            }
            catch (HttpRequestException)
            {
                MensajeError = $"No se pudo conectar con la API de la SAT en {_backendUrl}. Asegúrese de que el Backend esté en ejecución.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error inesperado: {ex.Message}";
            }

            return Page();
        }
    }
}
