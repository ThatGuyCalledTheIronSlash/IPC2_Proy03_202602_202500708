using System;

namespace Backend.SatApi.Dominio
{
    public class ValidadorNit
    {
        public static bool EsValido(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit)) return false;
            
            nit = nit.Trim().ToUpper();
            if (nit.Length < 2) return false;
            
            int suma = 0;
            int posicion = 2; 
            
            for (int i = nit.Length - 2; i >= 0; i--)
            {
                if (!char.IsDigit(nit[i])) return false; 
                
                int valor = int.Parse(nit[i].ToString());
                suma += valor * posicion;
                posicion++;
            }
            
            int mod11 = suma % 11;
            int resta = 11 - mod11;
            int digitoEsperado = resta % 11;
            
            string verificadorCalculado = digitoEsperado == 10 ? "K" : digitoEsperado.ToString();
            string verificadorIngresado = nit[nit.Length - 1].ToString();
            
            return verificadorCalculado == verificadorIngresado;
        }
    }
}
