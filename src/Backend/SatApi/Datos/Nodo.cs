namespace Backend.SatApi.Datos
{
    public class Nodo
    {
        private object dato;
        public Nodo? Siguiente;

        public Nodo(object dato)
        {
            this.dato = dato;
            this.Siguiente = null;
        }

        public object Dato
        {
            get => dato;
            set => dato = value;
        }
    }
}