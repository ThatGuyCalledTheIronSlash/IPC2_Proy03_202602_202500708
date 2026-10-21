using System;

namespace Backend.SatApi.Dominio
{
    // -------------------------------------------------------------------------
    // Validador matemático de NIT mediante el algoritmo Módulo 11
    // -------------------------------------------------------------------------
    public class ValidadorNit
    {
        public static bool EsValido(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit))
            {
                return false;
            }

            nit = nit.Trim().ToUpper();

            // Longitud mínima: 2 (1 dígito + verificador). Máxima: 21 (hasta 20 dígitos + verificador)
            if (nit.Length < 2 || nit.Length > 21)
            {
                return false;
            }

            // -----------------------------------------------------------------
            // Cálculo de ponderación de derecha a izquierda (iniciando en peso 2)
            // -----------------------------------------------------------------
            int suma = 0;
            int posicion = 2;

            for (int i = nit.Length - 2; i >= 0; i--)
            {
                if (!char.IsDigit(nit[i]))
                {
                    return false;
                }

                int valor = int.Parse(nit[i].ToString());
                suma += valor * posicion;
                posicion++;
            }

            // -----------------------------------------------------------------
            // Aplicación de Módulo 11 y determinación del dígito verificador
            // -----------------------------------------------------------------
            int mod11 = suma % 11;
            int resta = 11 - mod11;
            int digitoEsperado = resta % 11;

            string verificadorCalculado = (digitoEsperado == 10) ? "K" : digitoEsperado.ToString();
            string verificadorIngresado = nit[nit.Length - 1].ToString();

            return verificadorCalculado == verificadorIngresado;
        }
    }
}
