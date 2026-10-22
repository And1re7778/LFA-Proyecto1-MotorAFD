using System;
using System.Collections.Generic;
using System.IO;

namespace ProyectoAutomata
{
    class Program
    {
        static void Main(string[] args)
        {
            AutomataPila automataPila = new AutomataPila();
            bool salir = false;

            while (!salir)
            {
                LimpiarConsola();

                Console.WriteLine("==============================================");
                Console.WriteLine("    MOTOR DE AUTÓMATA DE PILA (APD) - FASE 3   ");
                Console.WriteLine("==============================================");
                Console.WriteLine();
                Console.WriteLine("1. Cargar autómata de pila desde archivo (.txt)");
                Console.WriteLine("2. Ingresar autómata de pila manualmente");
                Console.WriteLine("3. Mostrar definición formal y tabla de transición");
                Console.WriteLine("4. Evaluar cadena individual");
                Console.WriteLine("5. Evaluar lote de cadenas (desde archivo .txt)");
                Console.WriteLine("6. Reiniciar / Cargar nuevo autómata");
                Console.WriteLine("7. Salir");
                Console.WriteLine("8. Módulo AFD / AFND (Fases Anteriores)");
                Console.WriteLine();
                Console.Write("Seleccione una opción: ");

                string? opcion = Console.ReadLine();

                switch (opcion)
                {
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

        static void CargarDesdeArchivo(AutomataPila automata)
        {
            LimpiarConsola();
            Console.WriteLine("==============================================");
            Console.WriteLine("   CARGAR AUTÓMATA DE PILA DESDE ARCHIVO");
            Console.WriteLine("==============================================");
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
            Console.WriteLine("==============================================");
            Console.WriteLine("       INGRESAR AUTÓMATA DE PILA MANUAL");
            Console.WriteLine("==============================================");
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
                    Console.WriteLine($"---------- Transición {i + 1} ----------");

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

        static HashSet<string> CrearConjunto(string entrada)
        {
            HashSet<string> conjunto = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(entrada)) return conjunto;

            string[] elementos = entrada.Split(',');
            foreach (string elemento in elementos)
            {
                string valor = elemento.Trim();
                if (!string.IsNullOrEmpty(valor))
                {
                    conjunto.Add(valor);
                }
            }
            return conjunto;
        }

        static void MostrarDefinicion(AutomataPila automata)
        {
            LimpiarConsola();
            Console.WriteLine("==============================================");
            Console.WriteLine("      DEFINICIÓN Y TABLA DE TRANSICIÓN (APD)  ");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (!automata.EsValido)
            {
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

        static void EvaluarCadena(AutomataPila automata)
        {
            LimpiarConsola();
            Console.WriteLine("==============================================");
            Console.WriteLine("             EVALUAR CADENA INDIVIDUAL");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (!automata.EsValido)
            {
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

            try
            {
                ResultadoSimulacionAPD resultado = automata.EvaluarCadena(cadena);
                MostrarResultado(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo evaluar la cadena.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            Pausar();
        }

        static void EvaluarLote(AutomataPila automata)
        {
            LimpiarConsola();
            Console.WriteLine("==============================================");
            Console.WriteLine("        EVALUAR LOTE DE CADENAS (APD)");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (!automata.EsValido)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No hay un autómata válido cargado.");
                Console.WriteLine("Primero debe cargar o ingresar un autómata de pila.");
                Console.ResetColor();
                Pausar();
                return;
            }

            Console.Write("Ingrese el nombre o ruta del archivo .txt con las cadenas: ");
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
                if (!File.Exists(ruta)) throw new FileNotFoundException("El archivo no existe.", ruta);

                List<string> cadenas = new List<string>();
                string[] lineas = File.ReadAllLines(ruta);

                for (int i = 0; i < lineas.Length; i++)
                {
                    string cad = lineas[i].Trim();
                    if (string.IsNullOrEmpty(cad)) continue;
                    cadenas.Add(cad);
                }

                if (cadenas.Count == 0)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("El archivo no contiene cadenas para evaluar.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                List<ResultadoSimulacionAPD> resultados = automata.EvaluarLote(cadenas);

                Console.WriteLine();
                Console.WriteLine("==============================================");
                Console.WriteLine("             RESULTADOS DEL LOTE");
                Console.WriteLine("==============================================");

                int aceptadas = 0;
                int rechazadas = 0;

                for (int i = 0; i < resultados.Count; i++)
                {
                    ResultadoSimulacionAPD res = resultados[i];

                    Console.WriteLine();
                    Console.WriteLine("**********************************************");
                    Console.WriteLine($"Cadena #{i + 1}: '{res.CadenaOriginal}'");
                    Console.WriteLine("**********************************************");

                    if (res.Traza.Count == 0)
                    {
                        Console.WriteLine("Traza: [Sin transiciones ejecutadas]");
                    }
                    else
                    {
                        Console.WriteLine("TRAZA:");
                        foreach (PasoTrazaAPD paso in res.Traza)
                        {
                            Console.WriteLine("  " + paso);
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Estado final: {res.EstadoFinal} | Pila final: {res.PilaFinal}");

                    if (res.Aceptada)
                    {
                        aceptadas++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Veredicto: ACEPTADA");
                        Console.ResetColor();
                    }
                    else
                    {
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
                Console.WriteLine("==============================================");
                Console.WriteLine($"RESUMEN: Total: {resultados.Count} | Aceptadas: {aceptadas} | Rechazadas: {rechazadas}");
                Console.WriteLine("==============================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No se pudo procesar el archivo de lote.");
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

            Pausar();
        }

        static void MostrarResultado(ResultadoSimulacionAPD resultado)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("                  TRAZA");
            Console.WriteLine("==============================================");

            if (resultado.Traza.Count == 0)
            {
                Console.WriteLine("No se ejecutaron pasos de transición.");
            }
            else
            {
                foreach (PasoTrazaAPD paso in resultado.Traza)
                {
                    Console.WriteLine(paso);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Estado final: {resultado.EstadoFinal} | Pila final: {resultado.PilaFinal}");
            Console.WriteLine();

            if (resultado.Aceptada)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("VEREDICTO: Cadena ACEPTADA");
                Console.WriteLine($"Motivo: Se consumió la totalidad de la cadena y el autómata finalizó en el estado de aceptación '{resultado.EstadoFinal}'.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("VEREDICTO: Cadena RECHAZADA");
                Console.WriteLine($"Motivo: {resultado.Error}");
                Console.ResetColor();
            }
        }

        static void MenuFasesAnteriores()
        {
            Automata automataAFD = new Automata();
            bool volver = false;

            while (!volver)
            {
                LimpiarConsola();
                Console.WriteLine("==============================================");
                Console.WriteLine("       MÓDULO AFD / AFND (FASES 1 Y 2)        ");
                Console.WriteLine("==============================================");
                Console.WriteLine();
                Console.WriteLine("1. Cargar AFD desde archivo (.txt)");
                Console.WriteLine("2. Mostrar definición y tabla AFD");
                Console.WriteLine("3. Evaluar cadena individual en AFD");
                Console.WriteLine("4. Cargar AFND y convertir a AFD (Fase 2)");
                Console.WriteLine("5. Volver al Menú Principal");
                Console.WriteLine();
                Console.Write("Seleccione una opción: ");

                string? op = Console.ReadLine();

                switch (op)
                {
                    case "1":
                        LimpiarConsola();
                        Console.Write("Ingrese nombre de archivo AFD: ");
                        string? n1 = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(n1))
                        {
                            try
                            {
                                string r1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, n1.Trim());
                                if (!File.Exists(r1)) r1 = Path.Combine(Directory.GetCurrentDirectory(), n1.Trim());
                                automataAFD.CargarDesdeArchivo(r1);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("AFD cargado exitosamente.");
                                Console.ResetColor();
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: " + ex.Message);
                                Console.ResetColor();
                            }
                        }
                        Pausar();
                        break;
                    case "2":
                        LimpiarConsola();
                        if (automataAFD.EsValido) automataAFD.MostrarDefinicionYTabla();
                        else Console.WriteLine("No hay AFD válido cargado.");
                        Pausar();
                        break;
                    case "3":
                        LimpiarConsola();
                        if (!automataAFD.EsValido)
                        {
                            Console.WriteLine("No hay AFD cargado.");
                            Pausar();
                            break;
                        }
                        Console.Write("Ingrese cadena: ");
                        string? cad = Console.ReadLine() ?? "";
                        try
                        {
                            ResultadoSimulacion res = automataAFD.EvaluarCadena(cad);
                            foreach (var p in res.Traza) Console.WriteLine(p);
                            Console.WriteLine("Estado final: " + res.EstadoFinal);
                            Console.WriteLine(res.Aceptada ? "ACEPTADA" : "RECHAZADA");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                        Pausar();
                        break;
                    case "4":
                        LimpiarConsola();
                        Console.Write("Ingrese nombre de archivo AFND: ");
                        string? n2 = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(n2))
                        {
                            try
                            {
                                string r2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, n2.Trim());
                                if (!File.Exists(r2)) r2 = Path.Combine(Directory.GetCurrentDirectory(), n2.Trim());
                                AFND afnd = new AFND();
                                afnd.CargarDesdeArchivo(r2);
                                afnd.MostrarTablaTransicionAFND();
                                automataAFD = afnd.TransformarAAFD();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Transformación a AFD completada.");
                                Console.ResetColor();
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: " + ex.Message);
                                Console.ResetColor();
                            }
                        }
                        Pausar();
                        break;
                    case "5":
                        volver = true;
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        Pausar();
                        break;
                }
            }
        }

        static void Pausar()
        {
            Console.WriteLine();
            Console.WriteLine("Presione ENTER para continuar...");
            Console.ReadLine();
        }

        static void LimpiarConsola()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
            }
        }
    }
}