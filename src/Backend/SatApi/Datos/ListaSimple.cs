namespace Backend.SatApi.Datos
{
    // -------------------------------------------------------------------------
    // Estructura lineal personalizada (Lista Simplemente Enlazada)
    // -------------------------------------------------------------------------
    public class ListaSimple
    {
        private Nodo? primero;
        private Nodo? ultimo;
        private int cantidad;

        public int Cantidad => cantidad;
        public Nodo? Primero => primero;
        public Nodo? Ultimo => ultimo;

        // Inserción al final de la lista en tiempo O(1)
        public void Insertar(object dato)
        {
            Nodo nuevo = new Nodo(dato);

            if (primero == null)
            {
                primero = nuevo;
                ultimo = nuevo;
            }
            else
            {
                ultimo!.Siguiente = nuevo;
                ultimo = nuevo;
            }

            cantidad++;
        }

        // Búsqueda por índice en tiempo O(N)
        public object? ObtenerEn(int indice)
        {
            if (indice < 0 || indice >= cantidad)
            {
                return null;
            }

            Nodo? actual = primero;
            for (int i = 0; i < indice && actual != null; i++)
            {
                actual = actual.Siguiente;
            }

            return actual?.Dato;
        }
    }
}
