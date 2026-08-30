using System;
using System.Collections.Generic;
using System.IO;

namespace ProyectoAutomata {
    class Program {
        static void Main(string[] args) {
            Automata automata = new Automata();
            bool salir = false;

            while (!salir) {
                Console.Clear();

                Console.WriteLine("==============================================");
                Console.WriteLine("                    MENÚ                      ");
                Console.WriteLine();
                Console.WriteLine("1. Cargar quintúpla desde archivo (.txt)");
                Console.WriteLine("2. Ingresar quintúpla manualmente");
                Console.WriteLine("3. Mostrar definición formal y tabla de transisión");
                Console.WriteLine("4. Evaluar cadena individual");
                Console.WriteLine("5. Evaluar lote de cadenas (desde archivo .txt)");
                Console.WriteLine("6. Reiniciar / Cargar nuevo autómata");
                Console.WriteLine("7. Salir");
                Console.WriteLine();
                Console.Write("Seleccione una opción: ");

                string opcion = Console.ReadLine();

                switch (opcion) {
                    // cargar quitupla desde archivo
                    case "1":
                        CargarDesdeArchivo(automata);
                        break;
                    // ingresar quintupla manualmente
                    case "2":
                        CargarManual(automata);
                        break;
                    // mostrar definicion formal y tabla de transicion
                    case "3":
                        MostrarDefinicion(automata);
                        break;
                    // evaluar la cadena individual
                    case "4":
                        EvaluarCadena(automata);
                        break;
                    // evaluar lote de cadenas
                    case "5":
                        EvaluarLote(automata);
                        break;
                    // reiniciar / cargar nuevo automata
                    case "6":
                        automata.Reiniciar();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("El autómata ha sido reiniciado correctamente.");
                        Console.ResetColor();
                        Pausar();
                        break;
                    // salir
                    case "7":
                        salir = true;
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Programa finalizado.");
                        Console.ResetColor();
                        break;
                    // default
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Opción inválida. Ingrese un número del 1 al 7.");
                        Pausar();
                        break;
                }
            }
        }

        // ------------------------------------------------------------
        // OPCIÓN 1: CARGAR QUÍNTUPLA DESDE ARCHIVO
        static void CargarDesdeArchivo(Automata automata) {
            Console.Clear();

            Console.WriteLine("==============================================");
            Console.WriteLine("       CARGAR QUÍNTUPLA DESDE ARCHIVO");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            Console.Write("Ingrese el nombre del archivo .txt: ");
            string nombreArchivo = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreArchivo)) {
                Console.WriteLine();
                Console.WriteLine("Error: debe ingresar el nombre del archivo.");
                Pausar();
                return;
            }
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,nombreArchivo.Trim());

            try {
                automata.CargarDesdeArchivo(ruta);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡Autómata cargado correctamente!");
                Console.WriteLine("La quíntupla es válida.");
                Console.ResetColor();
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo cargar el autómata.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
            Pausar();
        }

        // ------------------------------------------------------------
        // OPCIÓN 2: INGRESAR QUÍNTUPLA MANUALMENTE
        static void CargarManual(Automata automata) {
            Console.Clear();
            Console.WriteLine("==============================================");
            Console.WriteLine("          INGRESAR QUÍNTUPLA MANUAL");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            try {
                // Q
                Console.Write("Ingrese los estados Q separados por coma: ");
                string entradaQ = Console.ReadLine();
                HashSet<string> q = CrearConjunto(entradaQ);
                // A 
                Console.Write("Ingrese los símbolos del alfabeto A separados por coma: ");
                string entradaA = Console.ReadLine();
                HashSet<string> a = CrearConjunto(entradaA);
                //  S 
                Console.Write("Ingrese el estado inicial S: ");
                string s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s)) { throw new Exception("El estado inicial no puede estar vacío."); }
                s = s.Trim();

                //  F 
                Console.Write("Ingrese los estados finales F separados por coma: ");
                string entradaF = Console.ReadLine();
                HashSet<string> f = CrearConjunto(entradaF);

                // TRANSICIONES
                Console.WriteLine();
                Console.Write("¿Cuántas transiciones desea ingresar?: ");

                string entradaCantidad = Console.ReadLine();

                if (!int.TryParse(entradaCantidad, out int cantidadTransiciones)) { throw new Exception("La cantidad de transiciones debe ser un número entero."); }
                if (cantidadTransiciones < 0) { throw new Exception("La cantidad de transiciones no puede ser negativa."); }

                List<Transicion> transiciones = new List<Transicion>();

                for (int i = 0; i < cantidadTransiciones; i++) {
                    Console.WriteLine();
                    Console.WriteLine("---------- Transición " + (i + 1) + " ----------");

                    Console.Write("Estado origen: ");
                    string origen = Console.ReadLine();

                    Console.Write("Símbolo: ");
                    string simbolo = Console.ReadLine();

                    Console.Write("Estado destino: ");
                    string destino = Console.ReadLine();

                    Transicion transicion = new Transicion(origen, simbolo, destino);
                    transiciones.Add(transicion);
                }
                // Cargar y validar el autómata
                automata.CargarManual(q, a, s, f, transiciones);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡Autómata ingresado correctamente!");
                Console.WriteLine("La quíntupla es válida.");
                Console.ResetColor();
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo cargar el autómata.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
            Pausar();
        }
        // CREAR HASHSET A PARTIR DE UNA ENTRADA SEPARADA POR COMAS
        static HashSet<string> CrearConjunto(string entrada) {
            HashSet<string> conjunto = new HashSet<string>();

            if (string.IsNullOrWhiteSpace(entrada)) { return conjunto; }

            string[] elementos = entrada.Split(',');

            foreach (string elemento in elementos){
                string valor = elemento.Trim();
                if (!string.IsNullOrEmpty(valor)) { conjunto.Add(valor); }
            }
            return conjunto;
        }

        // ------------------------------------------------------------
        // OPCIÓN 3: MOSTRAR DEFINICIÓN Y TABLA
        static void MostrarDefinicion(Automata automata){
            Console.Clear();

            Console.WriteLine("==============================================");
            Console.WriteLine("      DEFINICIÓN Y TABLA DE TRANSICIÓN");
            Console.WriteLine();

            if (!automata.EsValido) {
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar una quíntupla.");
                Pausar();
                return;
            }
            automata.MostrarDefinicionYTabla();
            Pausar();
        }

        // ------------------------------------------------------------
        // OPCIÓN 4: EVALUAR UNA CADENA
        static void EvaluarCadena(Automata automata) {
            Console.Clear();

            Console.WriteLine("==============================================");
            Console.WriteLine("             EVALUAR CADENA INDIVIDUAL");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (!automata.EsValido) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar una quíntupla.");
                Console.ResetColor();
                Pausar();
                return;
            }
            Console.WriteLine("Ingrese la cadena que desea evaluar.");
            Console.WriteLine("Si el alfabeto tiene símbolos de varios caracteres,");
            Console.WriteLine("deben estar separados por espacios.");
            Console.WriteLine();
            Console.Write("Cadena: ");

            string cadena = Console.ReadLine();
            if (cadena == null) { cadena = ""; }

            try {
                ResultadoSimulacion resultado = automata.EvaluarCadena(cadena);
                MostrarResultado(resultado);
            } catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo evaluar la cadena.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
            Pausar();
        }

        // ------------------------------------------------------------
        // OPCIÓN 5: EVALUAR LOTE DESDE ARCHIVO
        static void EvaluarLote(Automata automata) {
            Console.Clear();

            Console.WriteLine("==============================================");
            Console.WriteLine("        EVALUAR LOTE DE CADENAS");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (!automata.EsValido) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar una quíntupla.");
                Console.ResetColor();
                Pausar();
                return;
            }

            Console.Write("Ingrese el nombre del archivo .txt: ");
            string nombreArchivo = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreArchivo)) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: debe ingresar el nombre del archivo.");
                Console.ResetColor();
                Pausar();
                return;
            }
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,nombreArchivo.Trim());

            try {
                if (!File.Exists(ruta)) { throw new FileNotFoundException("El archivo no existe.", ruta); }

                List<string> cadenas = new List<string>();
                string[] lineas = File.ReadAllLines(ruta);

                for (int i = 0; i < lineas.Length; i++) {
                    string cadena = lineas[i].Trim();
                    
                    // Ignoramos líneas completamente vacías
                    if (string.IsNullOrEmpty(cadena)) { continue; }
                    cadenas.Add(cadena);
                }
                if (cadenas.Count == 0) {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("El archivo no contiene cadenas para evaluar.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                List<ResultadoSimulacion> resultados = automata.EvaluarLote(cadenas);

                Console.WriteLine();
                Console.WriteLine("==============================================");
                Console.WriteLine("             RESULTADOS DEL LOTE");
                Console.WriteLine("==============================================");

                for (int i = 0; i < resultados.Count; i++) {
                    ResultadoSimulacion resultado = resultados[i];

                    Console.WriteLine();
                    Console.WriteLine("**********************************************");
                    Console.WriteLine("Cadena #" + (i + 1) + ": " + resultado.CadenaOriginal);
                    Console.WriteLine("**********************************************");

                    if (!string.IsNullOrEmpty(resultado.Error)) { Console.WriteLine("Error: " + resultado.Error); }
                    else {
                        Console.WriteLine();
                        Console.WriteLine("TRAZA:");

                        foreach (PasoTraza paso in resultado.Traza) { Console.WriteLine(paso); }

                        Console.WriteLine();
                        Console.WriteLine("Estado final: " + resultado.EstadoFinal);

                        if (resultado.Aceptada) { 
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Resultado: ACEPTADA");
                            Console.ResetColor(); 
                        }
                        else { 
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Resultado: RECHAZADA");
                            Console.ResetColor();
                        } 
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo procesar el archivo.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            Pausar();
        }
        
        // ------------------------------------------------------------
        // MOSTRAR RESULTADO DE UNA SIMULACIÓN
        static void MostrarResultado(ResultadoSimulacion resultado) {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("                  TRAZA");
            Console.WriteLine("==============================================");

            if (resultado.Traza.Count == 0) {
                Console.WriteLine("La cadena es vacía (ε).");
            }
            else {
                foreach (PasoTraza paso in resultado.Traza) { Console.WriteLine(paso); }
            }
            Console.WriteLine();
            Console.WriteLine("Estado final: " + resultado.EstadoFinal);
            Console.WriteLine();

            if (resultado.Aceptada) { 
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("VEREDICTO: ACEPTADA");
                Console.ResetColor(); 
            }
            else { 
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("VEREDICTO: RECHAZADA");
                Console.ResetColor();
            }
        }
        // PAUSAR PARA QUE EL USUARIO PUEDA LEER EL RESULTADO
        static void Pausar() {
            Console.WriteLine();
            Console.WriteLine("Presione ENTER para continuar...");
            Console.ReadLine();
        }
    }
}