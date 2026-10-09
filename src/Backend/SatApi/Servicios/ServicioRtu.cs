using Backend.SatApi.Datos;
using Backend.SatApi.Dominio;
using Backend.SatApi.Dtos;

namespace Backend.SatApi.Servicios
{
    public class ServicioRtu
    {
        private readonly Repositorio _repositorio;

        public ServicioRtu(Repositorio? repositorio = null)
        {
            _repositorio = repositorio ?? Repositorio.Instancia;
        }

        /// Procesa un arreglo de ContribuyenteDto, aplicando las reglas de negocio del enunciado.
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

                //Validar el algoritmo de módulo 11
                if (!ValidadorNit.EsValido(contribuyente.NIT))
                {
                    respuesta.Invalidos++;
                    // "si el NIT no es válido no tiene ningún efecto sobre el RTU"
                    continue;
                }

                //Si es válido, comprobar si ya existe en la lista
                ContribuyenteDto? existente = _repositorio.BuscarContribuyente(contribuyente.NIT);

                if (existente != null)
                {
                    // "si el NIT es válido y ya existe se actualizará el contribuyente en el RTU"
                    existente.Nombre = contribuyente.Nombre;
                    respuesta.Actualizados++;
                }
                else
                {
                    // "Si el NIT es válido y no existe en el RTU se crea un nuevo contribuyente"
                    ContribuyenteDto nuevo = new ContribuyenteDto
                    {
                        NIT = contribuyente.NIT.Trim().ToUpper(),
                        Nombre = contribuyente.Nombre
                    };

                    _repositorio.Contribuyentes.Insertar(nuevo);
                    respuesta.Nuevos++;
                }
            }

            // Persistir los cambios en disco
            _repositorio.GuardarRtu();

            return respuesta;
        }
    }
}
