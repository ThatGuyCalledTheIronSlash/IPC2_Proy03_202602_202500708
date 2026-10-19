namespace Backend.SatApi.Datos
{

    public class Pila
    {
        private Nodo? tope;
        private int cantidad;

        public int Cantidad => cantidad;

        public bool EstaVacia => tope == null;

 

        public void Apilar(object dato)
        {
            Nodo nuevo = new Nodo(dato);
            nuevo.Siguiente = tope;
            tope = nuevo;
            cantidad++;
        }

        public object? Desapilar()
        {
            if (tope == null)
            {
                return null;
            }

            object dato = tope.Dato;
            tope = tope.Siguiente;
            cantidad--;

            return dato;
        }
    }
}
