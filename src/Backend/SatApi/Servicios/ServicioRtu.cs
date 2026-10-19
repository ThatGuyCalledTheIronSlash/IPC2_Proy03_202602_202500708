using Backend.SatApi.Datos;
using Backend.SatApi.Dominio;
using Backend.SatApi.Dtos;

namespace Backend.SatApi.Servicios
{
// -------------------------------------------------------------------------
// Lógica de procesamiento y actualización del Registro Tributario Unificado
// -------------------------------------------------------------------------
    public class ServicioRtu
    {
        private readonly Repositorio _repositorio;

        public ServicioRtu(Repositorio? repositorio = null)
        {
            _repositorio = repositorio ?? Repositorio.Instancia;
        }

// ---------------------------------------------------------------------
// Procesamiento en lote de contribuyentes
// ---------------------------------------------------------------------

        public RespuestaRtu ProcesarRtu(ContribuyenteDto[] listaEntrada)
        {
            RespuestaRtu respuesta = new RespuestaRtu();

            if (listaEntrada == null || listaEntrada.Length == 0)
            {
                return respuesta;
            }

            for (int i = 0; i < listaEntrada.Length; i++)
            {
                ContribuyenteDto contribuyente = listaEntrada[i];

                // Validación de formato y dígito verificador
                if (!ValidadorNit.EsValido(contribuyente.NIT))
                {
                    respuesta.Invalidos++;
                    continue;
                }

                // Verificación de existencia previa en el registro
                ContribuyenteDto? existente = _repositorio.BuscarContribuyente(contribuyente.NIT);

                if (existente != null)
                {
                    existente.Nombre = contribuyente.Nombre;
                    respuesta.Actualizados++;
                }
                else
                {
                    ContribuyenteDto nuevo = new ContribuyenteDto
                    {
                        NIT = contribuyente.NIT.Trim().ToUpper(),
                        Nombre = contribuyente.Nombre
                    };

                    _repositorio.Contribuyentes.Insertar(nuevo);
                    respuesta.Nuevos++;
                }
            }

            // Persistencia del catálogo actualizado en disco
            _repositorio.GuardarRtu();

            return respuesta;
        }
    }
}
