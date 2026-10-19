using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages.Peticiones
{
    public class ReporteErroresModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReporteErroresModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
            Desde = new DateTime(DateTime.Now.Year, 1, 1);
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
                string url = $"http://localhost:5000/api/ReporteErrores?desde={Desde.Value:yyyy-MM-dd}&hasta={Hasta.Value:yyyy-MM-dd}";

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
                MensajeError = "No se pudo conectar con la API de la SAT en http://localhost:5000/. Asegúrese de que el Backend esté en ejecución.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error inesperado: {ex.Message}";
            }

            return Page();
        }
    }
}
