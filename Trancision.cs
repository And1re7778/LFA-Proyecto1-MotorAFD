using System;

namespace ProyectoAutomata
{
    public class Transicion
    {
        public string EstadoOrigen { get; set; }
        public string Simbolo { get; set; }
        public string EstadoDestino { get; set; }

        public Transicion(string estadoOrigen, string simbolo, string estadoDestino)
        {
            EstadoOrigen = estadoOrigen;
            Simbolo = simbolo;
            EstadoDestino = estadoDestino;
        }

        // este es el formato que se va a mostrar en la lista de transiciones
        public override string ToString()
        {
            return $"({EstadoOrigen}, {Simbolo}) -> {EstadoDestino}";
        }
    }
}