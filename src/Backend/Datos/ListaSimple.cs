public class ListaSimple
{
    private Nodo? primero;
    private int cantidad;

    public int Cantidad { get { return cantidad; } }
    public Nodo? Primero { get { return primero; } }

    public void Insertar(object dato)
    {
        Nodo nuevo = new Nodo(dato);
        if (primero == null) { 
            primero = nuevo; 
        }
        else
        {
            Nodo? actual = primero;
            while (actual.Siguiente != null) actual = actual.Siguiente;
            actual.Siguiente = nuevo;
        }
        cantidad++;
    }

    public object? ObtenerEn(int indice)
    {
        Nodo? actual = primero;
        for (int i = 0; i < indice && actual != null; i++) actual = actual.Siguiente;
        return actual == null ? null : actual.Dato;
    }
}
