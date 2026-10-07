namespace Backend.SatApi.Dominio
{
    public enum TipoError
    {
        Ninguno,
        NitEmisorInvalido,
        NitReceptorInvalido,
        NitEmisorInexistente,
        NitReceptorInexistente,
        IvaMalCalculado,
        TotalMalCalculado,
        ReferenciaDuplicada
    }
}
