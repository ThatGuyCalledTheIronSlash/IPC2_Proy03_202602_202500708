using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public IFormFile? ArchivoJson { get; set; }

    public string? ContenidoEnviado { get; set; }
    public string? RespuestaJson { get; set; }
    public string? MensajeError { get; set; }
    public string? MensajeExito { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ArchivoJson == null || ArchivoJson.Length == 0)
        {
            MensajeError = "Por favor selecciona un archivo JSON válido.";
            return Page();
        }

        try
        {
            using var reader = new StreamReader(ArchivoJson.OpenReadStream());
            string jsonContent = await reader.ReadToEndAsync();
            ContenidoEnviado = jsonContent;

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Llamada a la Web API (SatApi)
            var response = await client.PostAsync("http://localhost:5000/api/Procesar", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Formatear el JSON para mostrarlo indentado y legible
                try
                {
                    using var jsonDoc = JsonDocument.Parse(responseBody);
                    RespuestaJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                }
                catch
                {
                    RespuestaJson = responseBody;
                }
                MensajeExito = "El archivo fue procesado exitosamente por la API.";
            }
            else
            {
                MensajeError = $"La API respondió con error (Código {(int)response.StatusCode}): {responseBody}";
            }
        }
        catch (HttpRequestException)
        {
            MensajeError = "No se pudo conectar con la API de la SAT en http://localhost:5000/. Asegúrate de que el proyecto Backend/SatApi esté corriendo con 'dotnet run'.";
        }
        catch (Exception ex)
        {
            MensajeError = $"Error inesperado al procesar el archivo: {ex.Message}";
        }

        return Page();
    }
}
