using System;
using System.Text.RegularExpressions;

namespace Backend.SatApi.Dominio
{
    public class ExtractorTiempo
    {
        public static DateTime? Extraer(string tiempoStr)
        {
            if (string.IsNullOrWhiteSpace(tiempoStr)) return null;

            var match = Regex.Match(tiempoStr, @"(\d{1,2})/(\d{1,2})/(\d{4})\s+(\d{1,2}):(\d{2})");
            
            if (match.Success)
            {
                int dia = int.Parse(match.Groups[1].Value);
                int mes = int.Parse(match.Groups[2].Value);
                int anio = int.Parse(match.Groups[3].Value);
                int hora = int.Parse(match.Groups[4].Value);
                int minuto = int.Parse(match.Groups[5].Value);

                try 
                {
                    return new DateTime(anio, mes, dia, hora, minuto, 0);
                }
                catch 
                {
                    return null;
                }
            }
            return null;
        }
    }
}
