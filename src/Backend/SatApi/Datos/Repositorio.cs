namespace Backend.SatApi.Datos
{
    public class Repositorio
    {
        // Instancias estáticas o variables para manejar los archivos JSON.
        // Aquí utilizaremos las TDAs (Pila, Cola, ListaSimple) para manejar la información en memoria.
        
        public ListaSimple Contribuyentes { get; set; } = new ListaSimple();
        public ListaSimple FacturasAprobadas { get; set; } = new ListaSimple();
        public ListaSimple FacturasRechazadas { get; set; } = new ListaSimple();
        
        // Métodos para Cargar y Guardar en los archivos .json de la carpeta Almacen
    }
}
