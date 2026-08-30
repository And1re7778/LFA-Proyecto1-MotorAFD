using System;

namespace ProyectoAutomata {
    // Clase que representa una transición individual del AFD: (estadoOrigen, simbolo) -> estadoDestino
    public class Transicion {
        public string EstadoOrigen { get; set; }
        public string Simbolo { get; set; }
        public string EstadoDestino { get; set; }

        public Transicion(string estadoOrigen, string simbolo, string estadoDestino) {
            if (string.IsNullOrWhiteSpace(estadoOrigen))
                throw new ArgumentException("El estado origen de la transición no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(simbolo))
                throw new ArgumentException("El símbolo de la transición no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(estadoDestino))
                throw new ArgumentException("El estado destino de la transición no puede estar vacío.");

            EstadoOrigen = estadoOrigen.Trim();
            Simbolo = simbolo.Trim();
            EstadoDestino = estadoDestino.Trim();
        }

        // Este es el formato que se va a mostrar en la lista de transiciones
        public override string ToString() {
            return $"({EstadoOrigen}, {Simbolo}) -> {EstadoDestino}";
        }
    }
}