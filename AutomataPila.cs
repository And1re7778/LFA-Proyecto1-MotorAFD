using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProyectoAutomata
{
    public class PasoTrazaAPD
    {
        public string EstadoActual { get; set; }
        public string SimboloLeido { get; set; }
        public string TopeDesapilado { get; set; }
        public string CadenaApilada { get; set; }
        public string EstadoPila { get; set; }
        public string EstadoSiguiente { get; set; }

        public PasoTrazaAPD(string estadoActual, string simboloLeido, string topeDesapilado, string cadenaApilada, string estadoPila, string estadoSiguiente)
        {
            EstadoActual = estadoActual;
            SimboloLeido = simboloLeido;
            TopeDesapilado = topeDesapilado;
            CadenaApilada = cadenaApilada;
            EstadoPila = estadoPila;
            EstadoSiguiente = estadoSiguiente;
        }

        public override string ToString()
        {
            return $"Estado actual: {EstadoActual} | Entrada: '{SimboloLeido}' | Desapila: '{TopeDesapilado}' | Apila: '{CadenaApilada}' | Pila: {EstadoPila} | Estado siguiente: {EstadoSiguiente}";
        }
    }

    public class ResultadoSimulacionAPD
    {
        public string CadenaOriginal { get; set; }
        public List<PasoTrazaAPD> Traza { get; set; }
        public string? EstadoFinal { get; set; }
        public string? PilaFinal { get; set; }
        public bool Aceptada { get; set; }
        public string? Error { get; set; }

        public ResultadoSimulacionAPD(string cadenaOriginal)
        {
            CadenaOriginal = cadenaOriginal;
            Traza = new List<PasoTrazaAPD>();
            Aceptada = false;
            Error = null;
        }
    }

    public class AutomataPila
    {
        public HashSet<string> Q { get; private set; }
        public HashSet<string> A { get; private set; }
        public HashSet<string> R { get; private set; }
        public string? S { get; private set; }
        public HashSet<string> F { get; private set; }
        public string? Z0 { get; private set; }

        public List<TransicionAPD> TransicionesOriginales { get; private set; }
        private Dictionary<string, TransicionAPD> TablaTransiciones;

        public bool EsValido { get; private set; }

        public AutomataPila()
        {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            R = new HashSet<string>();
            F = new HashSet<string>();
            TransicionesOriginales = new List<TransicionAPD>();
            TablaTransiciones = new Dictionary<string, TransicionAPD>();
            EsValido = false;
        }

        public void CargarManual(HashSet<string> q, HashSet<string> a, HashSet<string> r, string s, HashSet<string> f, string z0, List<TransicionAPD> transiciones)
        {
            Q = q;
            A = a;
            R = r;
            S = s;
            F = f;
            Z0 = z0;
            TransicionesOriginales = transiciones;
            ValidarAutomata();
        }

        public void CargarDesdeArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException($"El archivo '{rutaArchivo}' no existe en la ruta especificada.");
            }

            string[] lineas = File.ReadAllLines(rutaArchivo);

            Q = new HashSet<string>();
            A = new HashSet<string>();
            R = new HashSet<string>();
            F = new HashSet<string>();
            S = null;
            Z0 = null;
            TransicionesOriginales = new List<TransicionAPD>();
            TablaTransiciones = new Dictionary<string, TransicionAPD>();
            EsValido = false;

            bool dentroDeT = false;
            bool seDefinioQ = false;
            bool seDefinioA = false;
            bool seDefinioR = false;
            bool seDefinioS = false;
            bool seDefinioF = false;
            bool seDefinioZ0 = false;
            bool seEncontroBloqueT = false;

            for (int i = 0; i < lineas.Length; i++)
            {
                string lineaOriginal = lineas[i];
                string linea = lineaOriginal.Trim();

                if (string.IsNullOrEmpty(linea))
                {
                    continue;
                }

                if (dentroDeT)
                {
                    if (linea.StartsWith("}") || linea.EndsWith("}"))
                    {
                        string contenidoPrevio = linea.TrimEnd('}').Trim();
                        if (!string.IsNullOrEmpty(contenidoPrevio))
                        {
                            ProcesarLineaTransicion(contenidoPrevio, i + 1);
                        }
                        dentroDeT = false;
                        continue;
                    }

                    ProcesarLineaTransicion(linea, i + 1);
                }
                else
                {
                    Match matchQ = Regex.Match(linea, @"^Q\s*=\s*\{([^}]*)\}\s*$", RegexOptions.IgnoreCase);
                    if (matchQ.Success)
                    {
                        LlenarConjunto(Q, matchQ.Groups[1].Value);
                        seDefinioQ = true;
                        continue;
                    }

                    Match matchA = Regex.Match(linea, @"^A\s*=\s*\{([^}]*)\}\s*$", RegexOptions.IgnoreCase);
                    if (matchA.Success)
                    {
                        LlenarConjunto(A, matchA.Groups[1].Value);
                        seDefinioA = true;
                        continue;
                    }

                    Match matchR = Regex.Match(linea, @"^R\s*=\s*\{([^}]*)\}\s*$", RegexOptions.IgnoreCase);
                    if (matchR.Success)
                    {
                        LlenarConjunto(R, matchR.Groups[1].Value);
                        seDefinioR = true;
                        continue;
                    }

                    Match matchS = Regex.Match(linea, @"^S\s*=\s*([a-zA-Z0-9_]+)\s*$", RegexOptions.IgnoreCase);
                    if (matchS.Success)
                    {
                        S = matchS.Groups[1].Value.Trim();
                        seDefinioS = true;
                        continue;
                    }

                    Match matchF = Regex.Match(linea, @"^F\s*=\s*\{([^}]*)\}\s*$", RegexOptions.IgnoreCase);
                    if (matchF.Success)
                    {
                        LlenarConjunto(F, matchF.Groups[1].Value);
                        seDefinioF = true;
                        continue;
                    }

                    Match matchZ0 = Regex.Match(linea, @"^(?:Z0|Z_0)\s*=\s*([a-zA-Z0-9_]+)\s*$", RegexOptions.IgnoreCase);
                    if (matchZ0.Success)
                    {
                        Z0 = matchZ0.Groups[1].Value.Trim();
                        seDefinioZ0 = true;
                        continue;
                    }

                    Match matchTInicio = Regex.Match(linea, @"^T\s*=\s*\{", RegexOptions.IgnoreCase);
                    if (matchTInicio.Success)
                    {
                        seEncontroBloqueT = true;
                        if (linea.EndsWith("}") && linea.Length > 4)
                        {
                            string contenidoInterno = linea.Substring(linea.IndexOf('{') + 1);
                            contenidoInterno = contenidoInterno.Substring(0, contenidoInterno.LastIndexOf('}')).Trim();
                            if (!string.IsNullOrEmpty(contenidoInterno))
                            {
                                ProcesarMultiplesTransiciones(contenidoInterno, i + 1);
                            }
                        }
                        else
                        {
                            dentroDeT = true;
                            string resto = linea.Substring(linea.IndexOf('{') + 1).Trim();
                            if (!string.IsNullOrEmpty(resto))
                            {
                                ProcesarLineaTransicion(resto, i + 1);
                            }
                        }
                        continue;
                    }

                    throw new Exception($"Error sintáctico en la línea {i + 1}: '{lineaOriginal}'. No corresponde a una estructura válida (Q, A, R, S, F, Z0 o T).");
                }
            }

            if (dentroDeT)
            {
                throw new Exception("Error sintáctico: El bloque de transiciones T = { ... } no fue cerrado con '}'.");
            }

            if (!seDefinioQ) throw new Exception("Error de estructura: No se encontró la definición del conjunto de estados Q.");
            if (!seDefinioA) throw new Exception("Error de estructura: No se encontró la definición del alfabeto de entrada A.");
            if (!seDefinioR) throw new Exception("Error de estructura: No se encontró la definición del alfabeto de pila R.");
            if (!seDefinioS) throw new Exception("Error de estructura: No se encontró la definición del estado inicial S.");
            if (!seDefinioF) throw new Exception("Error de estructura: No se encontró la definición de los estados de aceptación F.");
            if (!seDefinioZ0) throw new Exception("Error de estructura: No se encontró la definición del símbolo inicial de pila Z0.");
            if (!seEncontroBloqueT || TransicionesOriginales.Count == 0) throw new Exception("Error de estructura: No se encontró el bloque de transiciones T o no contiene ninguna transición.");

            ValidarAutomata();
        }

        private void ProcesarLineaTransicion(string linea, int numeroLinea)
        {
            string lineaLimpia = linea.Trim();
            if (string.IsNullOrEmpty(lineaLimpia)) return;

            Match match = Regex.Match(lineaLimpia, @"^\s*\(\s*([^,\s]+)\s*,\s*([^,\s]+)\s*,\s*([^,\s\)]+)\s*\)\s*->\s*\(?\s*([^,\s\)]+)\s*,\s*([^,\s\)]+)\s*\)?\s*,?\s*$");

            if (!match.Success)
            {
                throw new Exception($"Error sintáctico en la línea {numeroLinea}: '{linea}'. La transición no cumple con el formato '(p, a, X) -> (q, gamma)' o tiene caracteres inválidos.");
            }

            string origen = match.Groups[1].Value.Trim();
            string entrada = match.Groups[2].Value.Trim();
            string tope = match.Groups[3].Value.Trim();
            string destino = match.Groups[4].Value.Trim();
            string apilar = match.Groups[5].Value.Trim();

            TransicionesOriginales.Add(new TransicionAPD(origen, entrada, tope, destino, apilar));
        }

        private void ProcesarMultiplesTransiciones(string bloque, int numeroLinea)
        {
            MatchCollection matches = Regex.Matches(bloque, @"\(\s*([^,\s]+)\s*,\s*([^,\s]+)\s*,\s*([^,\s\)]+)\s*\)\s*->\s*\(?\s*([^,\s\)]+)\s*,\s*([^,\s\)]+)\s*\)?");
            if (matches.Count == 0)
            {
                throw new Exception($"Error sintáctico en la línea {numeroLinea}: No se pudieron reconocer transiciones válidas en el bloque T.");
            }

            foreach (Match m in matches)
            {
                string origen = m.Groups[1].Value.Trim();
                string entrada = m.Groups[2].Value.Trim();
                string tope = m.Groups[3].Value.Trim();
                string destino = m.Groups[4].Value.Trim();
                string apilar = m.Groups[5].Value.Trim();

                TransicionesOriginales.Add(new TransicionAPD(origen, entrada, tope, destino, apilar));
            }
        }

        private void LlenarConjunto(HashSet<string> conjunto, string valores)
        {
            if (string.IsNullOrWhiteSpace(valores)) return;
            string[] partes = valores.Split(',');
            foreach (string p in partes)
            {
                string limpio = p.Trim();
                if (!string.IsNullOrEmpty(limpio))
                {
                    conjunto.Add(limpio);
                }
            }
        }

        public void ValidarAutomata()
        {
            EsValido = false;

            if (Q.Count == 0) throw new Exception("El conjunto de estados Q no puede estar vacío.");
            if (A.Count == 0) throw new Exception("El alfabeto de entrada A no puede estar vacío.");
            if (R.Count == 0) throw new Exception("El alfabeto de pila R no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(S)) throw new Exception("Debe definirse el estado inicial S.");
            if (!Q.Contains(S)) throw new Exception($"El estado inicial S ('{S}') no pertenece al conjunto de estados Q.");

            foreach (string f in F)
            {
                if (!Q.Contains(f))
                {
                    throw new Exception($"El estado de aceptación '{f}' no pertenece al conjunto de estados Q.");
                }
            }

            if (string.IsNullOrWhiteSpace(Z0)) throw new Exception("Debe definirse el símbolo inicial de pila Z0.");
            if (!R.Contains(Z0)) throw new Exception($"El símbolo inicial de pila Z0 ('{Z0}') no pertenece al alfabeto de pila R.");

            foreach (var t in TransicionesOriginales)
            {
                if (!Q.Contains(t.EstadoOrigen))
                    throw new Exception($"Transición inválida '{t}': El estado origen '{t.EstadoOrigen}' no existe en Q.");

                if (t.SimboloEntrada != "e" && !A.Contains(t.SimboloEntrada))
                    throw new Exception($"Transición inválida '{t}': El símbolo de entrada '{t.SimboloEntrada}' no pertenece al alfabeto A ni es 'e'.");

                if (!R.Contains(t.TopePila))
                    throw new Exception($"Transición inválida '{t}': El tope de pila '{t.TopePila}' no pertenece al alfabeto de pila R.");

                if (!Q.Contains(t.EstadoDestino))
                    throw new Exception($"Transición inválida '{t}': El estado destino '{t.EstadoDestino}' no existe en Q.");

                if (t.CadenaApilar != "e")
                {
                    foreach (char c in t.CadenaApilar)
                    {
                        if (!R.Contains(c.ToString()))
                        {
                            throw new Exception($"Transición inválida '{t}': El símbolo '{c}' en la cadena a apilar '{t.CadenaApilar}' no pertenece al alfabeto de pila R.");
                        }
                    }
                }
            }

            Dictionary<string, TransicionAPD> tablaTemporal = new Dictionary<string, TransicionAPD>();

            foreach (var t in TransicionesOriginales)
            {
                string clave = $"{t.EstadoOrigen}|{t.SimboloEntrada}|{t.TopePila}";
                if (tablaTemporal.ContainsKey(clave))
                {
                    TransicionAPD existente = tablaTemporal[clave];
                    throw new Exception($"Conflicto de unicidad de determinismo:\nSe encontraron múltiples transiciones para la combinación ({t.EstadoOrigen}, {t.SimboloEntrada}, {t.TopePila}):\n  - {existente}\n  - {t}");
                }
                tablaTemporal.Add(clave, t);
            }

            foreach (string q in Q)
            {
                foreach (string x in R)
                {
                    string claveEpsilon = $"{q}|e|{x}";
                    if (tablaTemporal.ContainsKey(claveEpsilon))
                    {
                        TransicionAPD transEpsilon = tablaTemporal[claveEpsilon];
                        foreach (string a in A)
                        {
                            string claveSimbolo = $"{q}|{a}|{x}";
                            if (tablaTemporal.ContainsKey(claveSimbolo))
                            {
                                TransicionAPD transSimbolo = tablaTemporal[claveSimbolo];
                                throw new Exception($"Conflicto de determinismo (Exclusión mutua con transición épsilon 'e'):\nPara el estado '{q}' y tope de pila '{x}' entran en conflicto:\n  - Transición con 'e': {transEpsilon}\n  - Transición con entrada '{a}': {transSimbolo}\nEn un APD, si existe una transición con 'e', no puede coexistir con transiciones para ningún símbolo del alfabeto de entrada.");
                            }
                        }
                    }
                }
            }

            TablaTransiciones = tablaTemporal;
            EsValido = true;
        }

        public void MostrarDefinicionYTabla()
        {
            if (!EsValido)
            {
                Console.WriteLine("El autómata de pila no es válido o no ha sido cargado.");
                return;
            }

            Console.WriteLine("=========================================================================");
            Console.WriteLine("                DEFINICIÓN FORMAL DEL APD (M)");
            Console.WriteLine("=========================================================================");
            Console.WriteLine($"  Q  (Conjunto de Estados)        = {{ {string.Join(", ", Q)} }}");
            Console.WriteLine($"  A  (Alfabeto de Entrada - Σ)    = {{ {string.Join(", ", A)} }}");
            Console.WriteLine($"  R  (Alfabeto de Pila - Γ)       = {{ {string.Join(", ", R)} }}");
            Console.WriteLine($"  S  (Estado Inicial - q0)        = {S}");
            Console.WriteLine($"  F  (Estados de Aceptación)      = {{ {string.Join(", ", F)} }}");
            Console.WriteLine($"  Z0 (Símbolo Inicial de Pila)    = {Z0}");
            Console.WriteLine("=========================================================================");
            Console.WriteLine();
            Console.WriteLine(GenerarTablaTransicionTexto());
        }

        public string GenerarTablaTransicionTexto()
        {
            if (!EsValido) return "No se puede generar la tabla: el autómata no es válido.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================================");
            sb.AppendLine("                              TABLA DE TRANSICIONES");
            sb.AppendLine("=========================================================================================");
            sb.AppendLine(string.Format("| {0,-15} | {1,-17} | {2,-14} | {3,-18} | {4,-15} |",
                "Estado Actual", "Símbolo Entrada", "Tope de Pila", "Estado Siguiente", "Cadena a Apilar"));
            sb.AppendLine("-----------------------------------------------------------------------------------------");

            foreach (var t in TransicionesOriginales)
            {
                string etiquetaOrigen = t.EstadoOrigen;
                if (t.EstadoOrigen == S) etiquetaOrigen = "->" + etiquetaOrigen;
                if (F.Contains(t.EstadoOrigen)) etiquetaOrigen = "*" + etiquetaOrigen;

                string etiquetaDestino = t.EstadoDestino;
                if (F.Contains(t.EstadoDestino)) etiquetaDestino = "*" + etiquetaDestino;

                sb.AppendLine(string.Format("| {0,-15} | {1,-17} | {2,-14} | {3,-18} | {4,-15} |",
                    etiquetaOrigen, t.SimboloEntrada, t.TopePila, etiquetaDestino, t.CadenaApilar));
            }

            sb.AppendLine("=========================================================================================");
            sb.AppendLine("Convenciones: '->' Estado Inicial | '*' Estado de Aceptación | 'e' Cadena Vacía (ε)");
            return sb.ToString();
        }

        private List<string> TokenizarCadena(string cadena)
        {
            List<string> simbolos = new List<string>();
            if (string.IsNullOrEmpty(cadena)) return simbolos;

            bool simbolosLargos = false;
            foreach (string a in A)
            {
                if (a.Length > 1) { simbolosLargos = true; break; }
            }

            if (simbolosLargos)
            {
                string[] partes = cadena.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                simbolos.AddRange(partes);
            }
            else
            {
                foreach (char c in cadena)
                {
                    simbolos.Add(c.ToString());
                }
            }

            return simbolos;
        }

        public static string FormatearPila(Stack<string> pila)
        {
            if (pila == null || pila.Count == 0)
            {
                return "[Vacía]";
            }
            return "[" + string.Join("|", pila) + "]";
        }

        public ResultadoSimulacionAPD EvaluarCadena(string cadena)
        {
            if (!EsValido)
            {
                throw new Exception("No se puede evaluar la cadena: el autómata no es válido o no ha sido cargado.");
            }

            ResultadoSimulacionAPD resultado = new ResultadoSimulacionAPD(cadena);
            List<string> entrada = TokenizarCadena(cadena);

            foreach (string s in entrada)
            {
                if (!A.Contains(s))
                {
                    resultado.Error = $"La cadena contiene el símbolo '{s}', que no pertenece al alfabeto de entrada A.";
                    resultado.Aceptada = false;
                    return resultado;
                }
            }

            Stack<string> pila = new Stack<string>();
            pila.Push(Z0!);

            string estadoActual = S!;
            int indice = 0;
            int contadorEpsilon = 0;
            const int MAX_PASOS_EPSILON = 1000;
            HashSet<string> configuracionesEpsilon = new HashSet<string>();

            while (true)
            {
                if (pila.Count == 0)
                {
                    if (indice < entrada.Count)
                    {
                        resultado.Error = "La pila se vació antes de terminar de procesar la totalidad de la cadena de entrada.";
                    }
                    else
                    {
                        if (!F.Contains(estadoActual))
                        {
                            resultado.Error = $"La pila quedó vacía y el autómata terminó en el estado no de aceptación '{estadoActual}'.";
                        }
                    }
                    break;
                }

                string topePila = pila.Peek();

                string claveEpsilon = $"{estadoActual}|e|{topePila}";
                if (TablaTransiciones.ContainsKey(claveEpsilon))
                {
                    string config = $"{estadoActual}|{indice}|{string.Join(",", pila)}";
                    if (configuracionesEpsilon.Contains(config) || contadorEpsilon > MAX_PASOS_EPSILON)
                    {
                        resultado.Error = "Se detectó un bucle infinito de transiciones épsilon (e).";
                        break;
                    }

                    configuracionesEpsilon.Add(config);
                    contadorEpsilon++;

                    TransicionAPD trans = TablaTransiciones[claveEpsilon];
                    pila.Pop();

                    if (trans.CadenaApilar != "e")
                    {
                        for (int i = trans.CadenaApilar.Length - 1; i >= 0; i--)
                        {
                            pila.Push(trans.CadenaApilar[i].ToString());
                        }
                    }

                    string estadoPilaVisual = FormatearPila(pila);
                    resultado.Traza.Add(new PasoTrazaAPD(
                        estadoActual,
                        "e",
                        topePila,
                        trans.CadenaApilar,
                        estadoPilaVisual,
                        trans.EstadoDestino
                    ));

                    estadoActual = trans.EstadoDestino;
                    continue;
                }

                if (indice < entrada.Count)
                {
                    string simbolo = entrada[indice];
                    string clave = $"{estadoActual}|{simbolo}|{topePila}";

                    if (TablaTransiciones.ContainsKey(clave))
                    {
                        TransicionAPD trans = TablaTransiciones[clave];
                        pila.Pop();

                        if (trans.CadenaApilar != "e")
                        {
                            for (int i = trans.CadenaApilar.Length - 1; i >= 0; i--)
                            {
                                pila.Push(trans.CadenaApilar[i].ToString());
                            }
                        }

                        string estadoPilaVisual = FormatearPila(pila);
                        resultado.Traza.Add(new PasoTrazaAPD(
                            estadoActual,
                            simbolo,
                            topePila,
                            trans.CadenaApilar,
                            estadoPilaVisual,
                            trans.EstadoDestino
                        ));

                        estadoActual = trans.EstadoDestino;
                        indice++;
                        contadorEpsilon = 0;
                        configuracionesEpsilon.Clear();
                        continue;
                    }
                    else
                    {
                        resultado.Error = $"No existe una transición definida para el estado '{estadoActual}', símbolo de entrada '{simbolo}' y tope de pila '{topePila}'.";
                        break;
                    }
                }

                break;
            }

            resultado.EstadoFinal = estadoActual;
            resultado.PilaFinal = FormatearPila(pila);

            if (resultado.Error == null)
            {
                if (indice == entrada.Count && F.Contains(estadoActual))
                {
                    resultado.Aceptada = true;
                }
                else if (indice < entrada.Count)
                {
                    resultado.Aceptada = false;
                    resultado.Error = $"No se terminó de consumir la cadena. Quedaron símbolos pendientes desde la posición {indice + 1}.";
                }
                else
                {
                    resultado.Aceptada = false;
                    resultado.Error = $"Se consumió toda la cadena de entrada pero el estado final '{estadoActual}' no pertenece al conjunto de estados de aceptación F.";
                }
            }
            else
            {
                resultado.Aceptada = false;
            }

            return resultado;
        }

        public List<ResultadoSimulacionAPD> EvaluarLote(List<string> cadenas)
        {
            List<ResultadoSimulacionAPD> resultados = new List<ResultadoSimulacionAPD>();

            foreach (string cadena in cadenas)
            {
                try
                {
                    resultados.Add(EvaluarCadena(cadena));
                }
                catch (Exception ex)
                {
                    ResultadoSimulacionAPD err = new ResultadoSimulacionAPD(cadena);
                    err.Error = ex.Message;
                    err.Aceptada = false;
                    resultados.Add(err);
                }
            }

            return resultados;
        }

        public void Reiniciar()
        {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            R = new HashSet<string>();
            S = null;
            F = new HashSet<string>();
            Z0 = null;
            TransicionesOriginales = new List<TransicionAPD>();
            TablaTransiciones = new Dictionary<string, TransicionAPD>();
            EsValido = false;
        }
    }
}
