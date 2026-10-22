using System;

namespace ProyectoAutomata
{
    public class TransicionAPD
    {
        public string EstadoOrigen { get; set; }
        public string SimboloEntrada { get; set; }
        public string TopePila { get; set; }
        public string EstadoDestino { get; set; }
        public string CadenaApilar { get; set; }

        public TransicionAPD(string estadoOrigen, string simboloEntrada, string topePila, string estadoDestino, string cadenaApilar)
        {
            if (string.IsNullOrWhiteSpace(estadoOrigen))
                throw new ArgumentException("El estado origen no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(simboloEntrada))
                throw new ArgumentException("El símbolo de entrada no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(topePila))
                throw new ArgumentException("El tope de pila no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(estadoDestino))
                throw new ArgumentException("El estado destino no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(cadenaApilar))
                throw new ArgumentException("La cadena a apilar no puede estar vacía.");

            EstadoOrigen = estadoOrigen.Trim();
            SimboloEntrada = simboloEntrada.Trim();
            TopePila = topePila.Trim();
            EstadoDestino = estadoDestino.Trim();
            CadenaApilar = cadenaApilar.Trim();
        }

        public override string ToString()
        {
            return $"({EstadoOrigen}, {SimboloEntrada}, {TopePila}) -> ({EstadoDestino}, {CadenaApilar})";
        }
    }
}
