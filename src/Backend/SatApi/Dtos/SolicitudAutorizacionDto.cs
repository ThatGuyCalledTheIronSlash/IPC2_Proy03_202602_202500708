namespace Backend.SatApi.Dtos
{
    // Usaremos arreglos [] en vez de listas nativas
    public class SolicitudAutorizacionDto
    {
        public Factura[] Facturas { get; set; } = new Factura[0];
    }
}
