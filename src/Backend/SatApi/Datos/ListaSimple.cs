namespace Backend.SatApi.Datos
{

    public class ListaSimple
    {
        private Nodo? primero;
        private int cantidad;

        public int Cantidad => cantidad;
        public Nodo? Primero => primero;


        public void Insertar(object dato)
        {
            Nodo nuevo = new Nodo(dato);

            if (primero == null)
            {
                primero = nuevo;
            }
            else
            {
                Nodo actual = primero;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevo;
            }

            cantidad++;
        }

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
