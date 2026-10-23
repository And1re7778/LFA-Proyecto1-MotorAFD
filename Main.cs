using System;
using System.Collections.Generic;
using System.IO;

namespace ProyectoAutomata {
    class Program {
        static void Main(string[] args) {
            AutomataPila automataPila = new AutomataPila();
            bool salir = false;

            while (!salir) {
                LimpiarConsola();

                Console.WriteLine("MOTOR DE AUTÓMATA DE PILA (APD)");
                Console.WriteLine();
                Console.WriteLine("1. Cargar autómata de pila desde archivo (.txt)");
                Console.WriteLine("2. Ingresar autómata de pila manualmente");
                Console.WriteLine("3. Mostrar definición formal y tabla de transición");
                Console.WriteLine("4. Evaluar cadena individual");
                Console.WriteLine("5. Evaluar lote de cadenas (desde archivo .txt)");
                Console.WriteLine("6. Reiniciar / Cargar nuevo autómata");
                Console.WriteLine("7. Salir");
                Console.WriteLine("8. Módulo AFD / AFND");
                Console.WriteLine();
                Console.Write("Seleccione una opción: ");

                string? opcion = Console.ReadLine();

                switch (opcion) {
                    case "1":
                        CargarDesdeArchivo(automataPila);
                        break;
                    case "2":
                        CargarManual(automataPila);
                        break;
                    case "3":
                        MostrarDefinicion(automataPila);
                        break;
                    case "4":
                        EvaluarCadena(automataPila);
                        break;
                    case "5":
                        EvaluarLote(automataPila);
                        break;
                    case "6":
                        automataPila.Reiniciar();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("El autómata de pila ha sido reiniciado correctamente.");
                        Console.ResetColor();
                        Pausar();
                        break;
                    case "7":
                        salir = true;
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Programa finalizado.");
                        Console.ResetColor();
                        break;
                    case "8":
                        MenuFasesAnteriores();
                        break;
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Opción inválida. Ingrese una opción válida.");
                        Pausar();
                        break;
                }
            }
        }

        // ============================================================
        // MÉTODOS APD (FASE 3)
        // ============================================================
        static void CargarDesdeArchivo(AutomataPila automata) {
            LimpiarConsola();
            Console.WriteLine("CARGAR AUTÓMATA DE PILA DESDE ARCHIVO");
            Console.WriteLine();

            Console.Write("Ingrese el nombre o ruta del archivo .txt: ");
            string? nombreArchivo = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: debe ingresar el nombre del archivo.");
                Console.ResetColor();
                Pausar();
                return;
            }

            string rutaLimpia = nombreArchivo.Trim().Trim('"', '\'');
            string ruta = Path.IsPathRooted(rutaLimpia)
                ? rutaLimpia
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rutaLimpia);

            if (!File.Exists(ruta))
            {
                string rutaDirecta = Path.Combine(Directory.GetCurrentDirectory(), rutaLimpia);
                if (File.Exists(rutaDirecta)) ruta = rutaDirecta;
            }

            try
            {
                automata.CargarDesdeArchivo(ruta);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡Autómata de Pila cargado y validado correctamente!");
                Console.WriteLine("Se comprobó la integridad de conjuntos y la condición estricta de determinismo.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo cargar el autómata.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            Pausar();
        }
        static void CargarManual(AutomataPila automata)
        {
            LimpiarConsola();
            Console.WriteLine("INGRESAR AUTÓMATA DE PILA MANUAL");
            Console.WriteLine();

            try
            {
                Console.Write("Ingrese los estados Q separados por coma: ");
                string? entradaQ = Console.ReadLine();
                HashSet<string> q = CrearConjunto(entradaQ ?? "");

                Console.Write("Ingrese los símbolos del alfabeto de entrada A separados por coma: ");
                string? entradaA = Console.ReadLine();
                HashSet<string> a = CrearConjunto(entradaA ?? "");

                Console.Write("Ingrese los símbolos del alfabeto de pila R separados por coma: ");
                string? entradaR = Console.ReadLine();
                HashSet<string> r = CrearConjunto(entradaR ?? "");

                Console.Write("Ingrese el estado inicial S: ");
                string? s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) throw new Exception("El estado inicial no puede estar vacío.");
                s = s.Trim();

                Console.Write("Ingrese los estados de aceptación F separados por coma: ");
                string? entradaF = Console.ReadLine();
                HashSet<string> f = CrearConjunto(entradaF ?? "");

                Console.Write("Ingrese el símbolo inicial de la pila Z0: ");
                string? z0 = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(z0)) throw new Exception("El símbolo inicial de pila Z0 no puede estar vacío.");
                z0 = z0.Trim();

                Console.WriteLine();
                Console.Write("¿Cuántas transiciones desea ingresar?: ");
                string? entradaCantidad = Console.ReadLine();

                if (!int.TryParse(entradaCantidad, out int cantidadTransiciones) || cantidadTransiciones < 0)
                {
                    throw new Exception("La cantidad de transiciones debe ser un número entero no negativo.");
                }

                List<TransicionAPD> transiciones = new List<TransicionAPD>();

                for (int i = 0; i < cantidadTransiciones; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine($"--- Transición {i + 1} ---");

                    Console.Write("Estado origen: ");
                    string? origen = Console.ReadLine();

                    Console.Write("Símbolo de entrada ('e' para épsilon): ");
                    string? simbolo = Console.ReadLine();

                    Console.Write("Tope de pila a desapilar: ");
                    string? tope = Console.ReadLine();

                    Console.Write("Estado destino: ");
                    string? destino = Console.ReadLine();

                    Console.Write("Cadena a apilar ('e' para no apilar nada): ");
                    string? apilar = Console.ReadLine();

                    transiciones.Add(new TransicionAPD(origen ?? "", simbolo ?? "", tope ?? "", destino ?? "", apilar ?? ""));
                }

                automata.CargarManual(q, a, r, s, f, z0, transiciones);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡Autómata de pila ingresado y validado correctamente!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo cargar el autómata.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            Pausar();
        }
        static void MostrarDefinicion(AutomataPila automata){
            LimpiarConsola();
            Console.WriteLine("DEFINICIÓN Y TABLA DE TRANSICIÓN (APD)");
            Console.WriteLine();
            if (!automata.EsValido) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar una tupla válida.");
                Console.ResetColor();
                Pausar();
                return;
            }
            automata.MostrarDefinicionYTabla();
            Pausar();
        }
        static void EvaluarCadena(AutomataPila automata) {
            LimpiarConsola();
            Console.WriteLine("EVALUAR CADENA INDIVIDUAL (APD)");
            Console.WriteLine();

            if (!automata.EsValido) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar un autómata de pila.");
                Console.ResetColor();
                Pausar();
                return;
            }

            Console.WriteLine("Ingrese la cadena que desea evaluar.");
            Console.WriteLine("(Presione ENTER sin escribir nada para evaluar la cadena vacía ε)");
            Console.WriteLine();
            Console.Write("Cadena: ");

            string? cadena = Console.ReadLine();
            if (cadena == null) cadena = "";

            try {
                ResultadoSimulacionAPD resultado = automata.EvaluarCadena(cadena);
                MostrarResultado(resultado);
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo evaluar la cadena.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
            Pausar();
        }
        static void EvaluarLote(AutomataPila automata) {
            LimpiarConsola();
            Console.WriteLine("EVALUAR LOTE DE CADENAS (APD)");
            Console.WriteLine();

            if (!automata.EsValido) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar un autómata de pila.");
                Console.ResetColor();
                Pausar();
                return;
            }
            Console.Write("Ingrese el nombre o ruta del archivo .txt con las cadenas: ");
            string? nombreArchivo = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nombreArchivo)) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: debe ingresar el nombre del archivo.");
                Console.ResetColor();
                Pausar();
                return;
            }
            string rutaLimpia = nombreArchivo.Trim().Trim('"', '\'');
            string ruta = Path.IsPathRooted(rutaLimpia)
                ? rutaLimpia
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rutaLimpia);

            if (!File.Exists(ruta)) {
                string rutaDirecta = Path.Combine(Directory.GetCurrentDirectory(), rutaLimpia);
                if (File.Exists(rutaDirecta)) ruta = rutaDirecta;
            }

            try {
                if (!File.Exists(ruta)) throw new FileNotFoundException("El archivo no existe.", ruta);

                List<string> cadenas = new List<string>();
                string[] lineas = File.ReadAllLines(ruta);

                for (int i = 0; i < lineas.Length; i++) {
                    string cad = lineas[i].Trim();
                    if (string.IsNullOrEmpty(cad)) continue;
                    cadenas.Add(cad);
                }
                if (cadenas.Count == 0) {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("El archivo no contiene cadenas para evaluar.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                List<ResultadoSimulacionAPD> resultados = automata.EvaluarLote(cadenas);

                Console.WriteLine();
                Console.WriteLine("RESULTADOS DEL LOTE");

                int aceptadas = 0;
                int rechazadas = 0;

                for (int i = 0; i < resultados.Count; i++) {
                    ResultadoSimulacionAPD res = resultados[i];

                    Console.WriteLine();
                    Console.WriteLine($"Cadena #{i + 1}: '{res.CadenaOriginal}'");

                    if (res.Traza.Count == 0) { Console.WriteLine("Traza: [Sin transiciones ejecutadas]"); }
                    else {
                        Console.WriteLine("TRAZA:");
                        foreach (PasoTrazaAPD paso in res.Traza) { Console.WriteLine("  " + paso); }
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Estado final: {res.EstadoFinal} | Pila final: {res.PilaFinal}");

                    if (res.Aceptada) {
                        aceptadas++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Veredicto: ACEPTADA");
                        Console.ResetColor();
                    }
                    else {
                        rechazadas++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Veredicto: RECHAZADA");
                        if (!string.IsNullOrEmpty(res.Error))
                        {
                            Console.WriteLine($"Motivo: {res.Error}");
                        }
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"RESUMEN: Total: {resultados.Count} | Aceptadas: {aceptadas} | Rechazadas: {rechazadas}");
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo procesar el archivo de lote.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            Pausar();
        }
        static void MostrarResultado(ResultadoSimulacionAPD resultado) {
            Console.WriteLine();
            Console.WriteLine("TRAZA");

            if (resultado.Traza.Count == 0) { Console.WriteLine("No se ejecutaron pasos de transición."); }
            else { foreach (PasoTrazaAPD paso in resultado.Traza) { Console.WriteLine(paso); } }

            Console.WriteLine();
            Console.WriteLine($"Estado final: {resultado.EstadoFinal} | Pila final: {resultado.PilaFinal}");
            Console.WriteLine();

            if (resultado.Aceptada) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("VEREDICTO: Cadena ACEPTADA");
                Console.WriteLine($"Motivo: Se consumió la totalidad de la cadena y el autómata finalizó en el estado de aceptación '{resultado.EstadoFinal}'.");
                Console.ResetColor();
            }
            else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("VEREDICTO: Cadena RECHAZADA");
                Console.WriteLine($"Motivo: {resultado.Error}");
                Console.ResetColor();
            }
        }

        // ============================================================
        // MÓDULO AFD / AFND (FASES 1 Y 2)
        // ============================================================
        static void MenuFasesAnteriores()
        {
            Automata automataAFD = new Automata();
            bool volver = false;

            while (!volver)
            {
                LimpiarConsola();
                Console.WriteLine("MÓDULO AFD / AFND (FASES 1 Y 2)");
                Console.WriteLine();
                Console.WriteLine("1. Cargar AFD desde archivo (.txt)");
                Console.WriteLine("2. Cargar AFND desde archivo (.txt) y transformar a AFD");
                Console.WriteLine("3. Ingresar AFD manualmente");
                Console.WriteLine("4. Ingresar AFND manualmente y transformar a AFD");
                Console.WriteLine("5. Mostrar definición formal y tabla del AFD actual");
                Console.WriteLine("6. Evaluar cadena individual en AFD");
                Console.WriteLine("7. Evaluar lote de cadenas en AFD (.txt)");
                Console.WriteLine("8. Reiniciar autómata AFD");
                Console.WriteLine("9. Volver al Menú Principal");
                Console.WriteLine();
                Console.Write("Seleccione una opción: ");

                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        CargarDesdeArchivo(automataAFD);
                        break;
                    case "2":
                        automataAFD = CargarAFNDDesdeArchivo();
                        break;
                    case "3":
                        CargarManual(automataAFD);
                        break;
                    case "4":
                        automataAFD = CargarAFNDManual();
                        break;
                    case "5":
                        MostrarDefinicion(automataAFD);
                        break;
                    case "6":
                        EvaluarCadena(automataAFD);
                        break;
                    case "7":
                        EvaluarLote(automataAFD);
                        break;
                    case "8":
                        automataAFD.Reiniciar();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("El autómata ha sido reiniciado correctamente.");
                        Console.ResetColor();
                        Pausar();
                        break;
                    case "9":
                        volver = true;
                        break;
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Opción inválida.");
                        Pausar();
                        break;
                }
            }
        }
        static void CargarDesdeArchivo(Automata automata) {
            LimpiarConsola();
            Console.WriteLine("CARGAR QUÍNTUPLA AFD DESDE ARCHIVO");
            Console.WriteLine();

            Console.Write("Ingrese el nombre del archivo .txt: ");
            string? nombreArchivo = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreArchivo)) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: debe ingresar el nombre del archivo.");
                Console.ResetColor();
                Pausar();
                return;
            }
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo.Trim());
            if (!File.Exists(ruta)) ruta = Path.Combine(Directory.GetCurrentDirectory(), nombreArchivo.Trim());
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
        static Automata CargarAFNDDesdeArchivo() {
            LimpiarConsola();
            Console.WriteLine("CARGAR AFND DESDE ARCHIVO Y TRANSFORMAR A AFD");
            Console.WriteLine();

            Console.Write("Ingrese el nombre del archivo .txt: ");
            string? nombreArchivo = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreArchivo)) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: debe ingresar el nombre del archivo.");
                Console.ResetColor();
                Pausar();
                return new Automata();
            }
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo.Trim());
            if (!File.Exists(ruta)) ruta = Path.Combine(Directory.GetCurrentDirectory(), nombreArchivo.Trim());
            try {
                AFND afnd = new AFND();
                afnd.CargarDesdeArchivo(ruta);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡AFND cargado correctamente!");
                Console.WriteLine("La quíntupla es válida.");
                Console.ResetColor();

                Console.WriteLine();
                Console.WriteLine("TABLA DEL AFND");
                Console.WriteLine();
                afnd.MostrarDefinicionYTabla();

                Automata nuevoAFD = afnd.TransformarAAFD();
                Console.WriteLine();
                Console.WriteLine("AFD EQUIVALENTE");
                Console.WriteLine();
                nuevoAFD.MostrarDefinicionYTabla();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("El AFND fue transformado correctamente a un AFD.");
                Console.WriteLine("Las evaluaciones posteriores se realizarán sobre este AFD.");
                Console.ResetColor();

                Pausar();
                return nuevoAFD;
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo cargar o transformar el AFND.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
                Pausar();
                return new Automata();
            }
        }
        static void CargarManual(Automata automata) {
            LimpiarConsola();
            Console.WriteLine("INGRESAR QUÍNTUPLA AFD MANUAL");
            Console.WriteLine();

            try {
                Console.Write("Ingrese los estados Q separados por coma: ");
                string? entradaQ = Console.ReadLine();
                HashSet<string> q = CrearConjunto(entradaQ ?? "");

                Console.Write("Ingrese los símbolos del alfabeto A separados por coma: ");
                string? entradaA = Console.ReadLine();
                HashSet<string> a = CrearConjunto(entradaA ?? "");

                Console.Write("Ingrese el estado inicial S: ");
                string? s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s)) throw new Exception("El estado inicial no puede estar vacío.");
                s = s.Trim();

                Console.Write("Ingrese los estados finales F separados por coma: ");
                string? entradaF = Console.ReadLine();
                HashSet<string> f = CrearConjunto(entradaF ?? "");

                Console.WriteLine();
                Console.Write("¿Cuántas transiciones desea ingresar?: ");
                string? entradaCantidad = Console.ReadLine();

                if (!int.TryParse(entradaCantidad, out int cantidadTransiciones)) throw new Exception("La cantidad de transiciones debe ser un número entero.");
                if (cantidadTransiciones < 0) throw new Exception("La cantidad de transiciones no puede ser negativa.");

                List<Transicion> transiciones = new List<Transicion>();

                for (int i = 0; i < cantidadTransiciones; i++) {
                    Console.WriteLine();
                    Console.WriteLine($"--- Transición {i + 1} ---");

                    Console.Write("Estado origen: ");
                    string? origen = Console.ReadLine();

                    Console.Write("Símbolo: ");
                    string? simbolo = Console.ReadLine();

                    Console.Write("Estado destino: ");
                    string? destino = Console.ReadLine();

                    transiciones.Add(new Transicion(origen ?? "", simbolo ?? "", destino ?? ""));
                }

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
        static Automata CargarAFNDManual() {
            LimpiarConsola();
            Console.WriteLine("INGRESAR QUÍNTUPLA AFND MANUAL");
            Console.WriteLine();
            try {
                Console.Write("Ingrese los estados Q separados por coma: ");
                string? entradaQ = Console.ReadLine();
                HashSet<string> q = CrearConjunto(entradaQ ?? "");

                Console.Write("Ingrese los símbolos del alfabeto A separados por coma: ");
                string? entradaA = Console.ReadLine();
                HashSet<string> a = CrearConjunto(entradaA ?? "");

                Console.Write("Ingrese el estado inicial S: ");
                string? s = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(s)) throw new Exception("El estado inicial no puede estar vacío.");
                s = s.Trim();

                Console.Write("Ingrese los estados finales F separados por coma: ");
                string? entradaF = Console.ReadLine();
                HashSet<string> f = CrearConjunto(entradaF ?? "");

                Console.WriteLine();
                Console.Write("¿Cuántas transiciones desea ingresar?: ");
                string? entradaCantidad = Console.ReadLine();

                if (!int.TryParse(entradaCantidad, out int cantidadTransiciones)) throw new Exception("La cantidad de transiciones debe ser un número entero.");
                if (cantidadTransiciones < 0) throw new Exception("La cantidad de transiciones no puede ser negativa.");

                List<Transicion> transiciones = new List<Transicion>();

                for (int i = 0; i < cantidadTransiciones; i++) {
                    Console.WriteLine();
                    Console.WriteLine($"--- Transición {i + 1} ---");

                    Console.Write("Estado origen: ");
                    string? origen = Console.ReadLine();

                    Console.Write("Símbolo: ");
                    string? simbolo = Console.ReadLine();

                    Console.Write("Estado destino: ");
                    string? destino = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(origen) || string.IsNullOrWhiteSpace(simbolo) || string.IsNullOrWhiteSpace(destino)) {
                        throw new Exception("El origen, símbolo y destino no pueden estar vacíos.");
                    }

                    transiciones.Add(new Transicion(origen.Trim(), simbolo.Trim(), destino.Trim()));
                }

                AFND afnd = new AFND();
                afnd.CargarManual(q, a, s, f, transiciones);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡AFND ingresado correctamente!");
                Console.WriteLine("La quíntupla es válida.");
                Console.ResetColor();

                Console.WriteLine();
                Console.WriteLine("TABLA DEL AFND");
                Console.WriteLine();
                afnd.MostrarDefinicionYTabla();

                Automata nuevoAFD = afnd.TransformarAAFD();
                Console.WriteLine();
                Console.WriteLine("AFD EQUIVALENTE");
                Console.WriteLine();
                nuevoAFD.MostrarDefinicionYTabla();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("El AFND fue transformado correctamente a un AFD.");
                Console.WriteLine("Las evaluaciones posteriores se realizarán sobre este AFD.");
                Console.ResetColor();

                Pausar();
                return nuevoAFD;
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo cargar o transformar el AFND.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
                Pausar();
                return new Automata();
            }
        }
        static void MostrarDefinicion(Automata automata) {
            LimpiarConsola();
            Console.WriteLine("DEFINICIÓN Y TABLA (AFD)");
            Console.WriteLine();

            if (!automata.EsValido) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar una quíntupla.");
                Console.ResetColor();
                Pausar();
                return;
            }
            automata.MostrarDefinicionYTabla();
            Pausar();
        }
        static void EvaluarCadena(Automata automata) {
            LimpiarConsola();
            Console.WriteLine("EVALUAR CADENA INDIVIDUAL (AFD)");
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
            Console.WriteLine("Si el alfabeto tiene símbolos de varios caracteres, deben estar separados por espacios.");
            Console.WriteLine();
            Console.Write("Cadena: ");

            string? cadena = Console.ReadLine();
            if (cadena == null) cadena = "";

            try {
                ResultadoSimulacion resultado = automata.EvaluarCadena(cadena);
                MostrarResultado(resultado);
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo evaluar la cadena.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
            Pausar();
        }
        static void EvaluarLote(Automata automata) {
            LimpiarConsola();
            Console.WriteLine("EVALUAR LOTE DE CADENAS (AFD)");
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
            string? nombreArchivo = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombreArchivo)) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: debe ingresar el nombre del archivo.");
                Console.ResetColor();
                Pausar();
                return;
            }
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo.Trim());
            if (!File.Exists(ruta)) ruta = Path.Combine(Directory.GetCurrentDirectory(), nombreArchivo.Trim());

            try {
                if (!File.Exists(ruta)) throw new FileNotFoundException("El archivo no existe.", ruta);

                List<string> cadenas = new List<string>();
                string[] lineas = File.ReadAllLines(ruta);

                for (int i = 0; i < lineas.Length; i++) {
                    string cad = lineas[i].Trim();
                    if (string.IsNullOrEmpty(cad)) continue;
                    cadenas.Add(cad);
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
                Console.WriteLine("RESULTADOS DEL LOTE");

                for (int i = 0; i < resultados.Count; i++) {
                    ResultadoSimulacion resultado = resultados[i];
                    Console.WriteLine();
                    Console.WriteLine($"Cadena #{i + 1}: {resultado.CadenaOriginal}");

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
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo procesar el archivo.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            Pausar();
        }
        static void MostrarResultado(ResultadoSimulacion resultado) {
            Console.WriteLine();
            Console.WriteLine("TRAZA");
            if (resultado.Error != null) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + resultado.Error);
                Console.ResetColor();
                return;
            }

            if (resultado.Traza.Count == 0) { Console.WriteLine("Cadena vacía: no se realizaron transiciones."); }
            else { foreach (PasoTraza paso in resultado.Traza) { Console.WriteLine(paso); } }

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
        
        // ============================================================
        // UTILIDADES COMUNES
        // ============================================================
        static HashSet<string> CrearConjunto(string entrada) {
            HashSet<string> conjunto = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(entrada)) return conjunto;

            string[] elementos = entrada.Split(',');
            foreach (string elemento in elementos) {
                string valor = elemento.Trim();
                if (!string.IsNullOrEmpty(valor)) { conjunto.Add(valor); }
            }
            return conjunto;
        }
        static void Pausar() {
            Console.WriteLine();
            Console.WriteLine("Presione ENTER para continuar...");
            Console.ReadLine();
        }
        static void LimpiarConsola() {
            try { Console.Clear(); }
            catch {}
        }
    }
}