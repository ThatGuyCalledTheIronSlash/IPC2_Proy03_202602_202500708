public class Cola
{
    private Nodo? frente, final;
    public bool EstaVacia { get { return frente == null; } }

    public void Encolar(object dato)
    {
        Nodo nuevo = new Nodo(dato);
        if (final == null) { frente = final = nuevo; }
        else { final.Siguiente = nuevo; final = nuevo; }
    }

    public object? Desencolar()
    {
        if (frente == null) return null;
        object dato = frente.Dato;
        frente = frente.Siguiente;
        if (frente == null) final = null;
        return dato;
    }
}