using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages.Peticiones
{
    public class ResumenIvaModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResumenIvaModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public DateTime? Fecha { get; set; }

        public ResumenIvaNit[]? Resultados { get; set; }
        public string? MensajeError { get; set; }
        public bool ConsultaRealizada { get; set; }

        public void OnGet()
        {
            // Fecha por defecto: hoy
            Fecha = DateTime.Now;
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
                string url = $"http://localhost:5000/api/ResumenIva?fecha={Fecha.Value:yyyy-MM-dd}";

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
                MensajeError = "No se pudo conectar con la API de la SAT en http://localhost:5000/. Verifique que el Backend esté encendido.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error inesperado: {ex.Message}";
            }

            return Page();
        }
    }
}
