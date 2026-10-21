using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Ayuda
{
    public class DocumentacionModel : PageModel
    {
        // -------------------------------------------------------------------------
        // Redirige de forma directa al archivo PDF de la documentacion oficial
        // -------------------------------------------------------------------------
        public IActionResult OnGet()
        {
            return Redirect("~/docs/documentacion.pdf");
        }
    }
}
