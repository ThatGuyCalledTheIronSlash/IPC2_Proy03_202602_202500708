using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _backendUrl;

    public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _backendUrl = (configuration["BackendUrl"] ?? "http://localhost:5000").TrimEnd('/');
    }

    [BindProperty]
    public List<IFormFile> ArchivosJson { get; set; } = new List<IFormFile>();

    public List<ResultadoProcesamientoArchivo> RespuestasProcesadas { get; set; } = new List<ResultadoProcesamientoArchivo>();
    public string? MensajeError { get; set; }
    public string? MensajeExito { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Inicializado { get; set; }

    public void OnGet()
    {
        if (Inicializado)
        {
            MensajeExito = "El sistema ha sido reiniciado a su estado inicial. Todos los datos en memoria y archivos fueron eliminados correctamente.";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ArchivosJson == null || ArchivosJson.Count == 0)
        {
            MensajeError = "Por favor selecciona al menos un archivo JSON válido.";
            return Page();
        }

        var client = _httpClientFactory.CreateClient();
        int procesadosExitosos = 0;

        foreach (var archivo in ArchivosJson)
        {
            if (archivo.Length == 0) continue;

            var resultadoArchivo = new ResultadoProcesamientoArchivo
            {
                NombreArchivo = archivo.FileName
            };

            try
            {
                using var reader = new StreamReader(archivo.OpenReadStream());
                string jsonContent = await reader.ReadToEndAsync();
                resultadoArchivo.ContenidoEnviado = jsonContent;

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_backendUrl}/api/Procesar", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                resultadoArchivo.CodigoEstado = (int)response.StatusCode;
                resultadoArchivo.EsExitoso = response.IsSuccessStatusCode;

                try
                {
                    using var jsonDoc = JsonDocument.Parse(responseBody);
                    resultadoArchivo.RespuestaFormateada = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                }
                catch
                {
                    resultadoArchivo.RespuestaFormateada = responseBody;
                }

                if (response.IsSuccessStatusCode)
                {
                    procesadosExitosos++;
                }
                else
                {
                    resultadoArchivo.Error = $"Código {(int)response.StatusCode}: {responseBody}";
                }
            }
            catch (HttpRequestException)
            {
                resultadoArchivo.EsExitoso = false;
                resultadoArchivo.Error = $"No se pudo conectar con la API en {_backendUrl}. Verifique que el Backend esté en ejecución.";
            }
            catch (Exception ex)
            {
                resultadoArchivo.EsExitoso = false;
                resultadoArchivo.Error = $"Error inesperado: {ex.Message}";
            }

            RespuestasProcesadas.Add(resultadoArchivo);
        }

        if (procesadosExitosos == ArchivosJson.Count)
        {
            MensajeExito = ArchivosJson.Count == 1 
                ? "El archivo fue procesado exitosamente por la API." 
                : $"Se procesaron exitosamente los {ArchivosJson.Count} archivos seleccionados.";
        }
        else if (procesadosExitosos > 0)
        {
            MensajeError = $"Se procesaron {procesadosExitosos} de {ArchivosJson.Count} archivos. Revise los detalles de respuesta a continuación.";
        }
        else
        {
            MensajeError = "No se pudo procesar la solicitud. Verifique los errores mostrados para cada archivo.";
        }

        return Page();
    }
}

public class ResultadoProcesamientoArchivo
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string ContenidoEnviado { get; set; } = string.Empty;
    public string RespuestaFormateada { get; set; } = string.Empty;
    public bool EsExitoso { get; set; }
    public int CodigoEstado { get; set; }
    public string? Error { get; set; }
}
