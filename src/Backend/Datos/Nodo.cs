public class Nodo
{
    private object dato;
    public Nodo? Siguiente;
    public Nodo(object dato) { this.dato = dato; }
    public object Dato { get { return dato; } set { dato = value; } }
}