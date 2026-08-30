using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProyectoAutomata {
    // Representa un paso individual dentro de la traza de ejecución de una cadena.
    public class PasoTraza {
        public string EstadoActual { get; set; }
        public string Simbolo { get; set; }
        public string EstadoSiguiente { get; set; }

        public PasoTraza(string estadoActual, string simbolo, string estadoSiguiente) {
            EstadoActual = estadoActual;
            Simbolo = simbolo;
            EstadoSiguiente = estadoSiguiente;
        }

        public override string ToString() {
            return $"Estado actual: {EstadoActual} | Símbolo leído: '{Simbolo}' | Estado siguiente: {EstadoSiguiente}";
        }
    }
    // Contiene el resultado completo de simular una cadena sobre el AFD:
    // la traza paso a paso, el estado final y el veredicto (aceptada/rechazada).
    public class ResultadoSimulacion {
        public string CadenaOriginal { get; set; }
        public List<PasoTraza> Traza { get; set; }
        public string EstadoFinal { get; set; }
        public bool Aceptada { get; set; }
        public string Error { get; set; } // si no es null, la cadena no se pudo procesar

        public ResultadoSimulacion(string cadenaOriginal) {
            CadenaOriginal = cadenaOriginal;
            Traza = new List<PasoTraza>();
            Aceptada = false;
            Error = null;
        }
    }
    // Representa un Autómata Finito Determinista (AFD) mediante su quíntupla M = (Q, Σ, δ, q0, F).
    // Permite cargar la definición (manual o desde archivo), validar su integridad estructural,
    // mostrar su definición formal / tabla de transición y simular cadenas de entrada.
    public class Automata {
        public HashSet<string> Q { get; private set; }
        public HashSet<string> A { get; private set; }
        public string S { get; private set; }
        public HashSet<string> F { get; private set; }

        private List<Transicion> TransicionesOriginales;
        private Dictionary<string, string> TablaTransiciones;

        public bool EsValido { get; private set; }
        private bool RequiereEspaciosEnCadenas; // true si algún símbolo del alfabeto tiene más de un carácter en ese caso las cadenas de entrada deben venir separadas por espacios.

        public Automata() {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            F = new HashSet<string>();
            TransicionesOriginales = new List<Transicion>();
            TablaTransiciones = new Dictionary<string, string>();
            EsValido = false;
            RequiereEspaciosEnCadenas = false;
        }

        // ---------------------------------------------------------------
        // CARGA
        public void CargarManual(HashSet<string> q, HashSet<string> a, string s, HashSet<string> f, List<Transicion> transiciones) {
            Q = q;
            A = a;
            S = s;
            F = f;
            TransicionesOriginales = transiciones;
            ValidarAutomata();
        }
        public void CargarDesdeArchivo(string rutaArchivo) {
            if (!File.Exists(rutaArchivo)) {
                throw new Exception($"El archivo '{rutaArchivo}' no existe en el directorio actual.");
            }

            string contenido = File.ReadAllText(rutaArchivo);

            // Reiniciamos por si se estaba recargando un autómata anterior
            Q = new HashSet<string>();
            A = new HashSet<string>();
            F = new HashSet<string>();
            TransicionesOriginales = new List<Transicion>();

            Match matchQ = Regex.Match(contenido, @"^Q\s*=\s*\{([^}]*)\}", RegexOptions.Multiline);
            if (!matchQ.Success) throw new Exception("Error de sintaxis: No se encontró la definición de Q o tiene formato incorrecto.");
            LlenarConjunto(Q, matchQ.Groups[1].Value);

            Match matchA = Regex.Match(contenido, @"^A\s*=\s*\{([^}]*)\}", RegexOptions.Multiline);
            if (!matchA.Success) throw new Exception("Error de sintaxis: No se encontró la definición de A (Alfabeto).");
            LlenarConjunto(A, matchA.Groups[1].Value);

            Match matchS = Regex.Match(contenido, @"^S\s*=\s*([a-zA-Z0-9_]+)", RegexOptions.Multiline);
            if (!matchS.Success) throw new Exception("Error de sintaxis: No se encontró la definición del estado inicial S.");
            S = matchS.Groups[1].Value.Trim();

            Match matchF = Regex.Match(contenido, @"^F\s*=\s*\{([^}]*)\}", RegexOptions.Multiline);
            if (!matchF.Success) throw new Exception("Error de sintaxis: No se encontró la definición de F.");
            LlenarConjunto(F, matchF.Groups[1].Value);

            Match matchT = Regex.Match(contenido, @"T\s*=\s*\{([^}]*)\}", RegexOptions.Singleline);
            if (!matchT.Success) throw new Exception("Error de sintaxis: No se encontró el bloque de transiciones T = { ... }.");

            string bloqueTransiciones = matchT.Groups[1].Value;

            MatchCollection matchesTransicion = Regex.Matches(bloqueTransiciones, @"\(\s*([^,]+)\s*,\s*([^)]+)\s*\)\s*->\s*([^,\s]+)");

            if (matchesTransicion.Count == 0 && Q.Count > 0 && A.Count > 0) {
                throw new Exception("Error de sintaxis: El bloque T está vacío o las transiciones no tienen el formato (estado,simbolo)->estado.");
            }

            foreach (Match m in matchesTransicion) {
                string origen = m.Groups[1].Value.Trim();
                string simbolo = m.Groups[2].Value.Trim();
                string destino = m.Groups[3].Value.Trim();
                TransicionesOriginales.Add(new Transicion(origen, simbolo, destino));
            }
            ValidarAutomata();
        }
        private void LlenarConjunto(HashSet<string> conjunto, string valores) {
            if (string.IsNullOrWhiteSpace(valores)) return;
            string[] partes = valores.Split(',');
            foreach (string p in partes) {
                string valorLimpio = p.Trim();
                if (!string.IsNullOrEmpty(valorLimpio)) { conjunto.Add(valorLimpio); }
            }
        }

        // ---------------------------------------------------------------
        // VALIDACIÓN
        private void ValidarAutomata() {
            EsValido = false;

            if (Q.Count == 0) throw new Exception("El conjunto de estados Q no puede estar vacío.");
            if (A.Count == 0) throw new Exception("El alfabeto A no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(S)) throw new Exception("Debe definirse el estado inicial S.");
            if (!Q.Contains(S)) throw new Exception($"El estado inicial S ('{S}') no pertenece al conjunto de estados Q.");

            foreach (string f in F) {
                if (!Q.Contains(f)) throw new Exception($"El estado final '{f}' no pertenece al conjunto de estados Q.");
            }

            // Usamos un diccionario temporal: si algo falla a mitad de camino,
            // TablaTransiciones (la oficial) no queda con datos a medias.
            Dictionary<string, string> tablaTemporal = new Dictionary<string, string>();
            HashSet<string> paresVistos = new HashSet<string>();

            foreach (var t in TransicionesOriginales) {
                if (!Q.Contains(t.EstadoOrigen)) throw new Exception($"Transición inválida: El estado origen '{t.EstadoOrigen}' no existe en Q.");
                if (!A.Contains(t.Simbolo)) throw new Exception($"Transición inválida: El símbolo '{t.Simbolo}' no existe en el alfabeto A.");
                if (!Q.Contains(t.EstadoDestino)) throw new Exception($"Transición inválida: El estado destino '{t.EstadoDestino}' no existe en Q.");

                string clave = $"{t.EstadoOrigen}|{t.Simbolo}";
                if (paresVistos.Contains(clave))
                {
                    throw new Exception($"Error de determinismo: Transición múltiple detectada para el estado '{t.EstadoOrigen}' con el símbolo '{t.Simbolo}'. Esto corresponde a un AFND, no a un AFD.");
                }

                paresVistos.Add(clave);
                tablaTemporal.Add(clave, t.EstadoDestino);
            }

            foreach (string q in Q) {
                foreach (string a in A) {
                    string clave = $"{q}|{a}";
                    if (!paresVistos.Contains(clave)) {
                        throw new Exception($"Error de determinismo: Falta la transición para el estado '{q}' con el símbolo '{a}'. Un AFD debe tener exactamente una transición por cada par (estado, símbolo).");
                    }
                }
            }
            // Todo pasó: ahora sí se confirma la tabla oficial
            TablaTransiciones = tablaTemporal;

            // Si algún símbolo del alfabeto tiene más de un carácter, las cadenas
            // de prueba deberán venir separadas por espacios para poder tokenizarlas.
            RequiereEspaciosEnCadenas = false;
            foreach (string a in A) {
                if (a.Length > 1) { RequiereEspaciosEnCadenas = true; break; }
            }
            EsValido = true;
        }
        // ---------------------------------------------------------------
        // DEFINICIÓN FORMAL Y TABLA DE TRANSICIÓN
        public void MostrarDefinicionYTabla() {
            if (!EsValido) {
                Console.WriteLine("El autómata no es válido o no ha sido cargado. No se puede mostrar la información.");
                return;
            }
            Console.WriteLine("DEFINICIÓN FORMAL DEL AFD: ");
            Console.WriteLine($"Q  (Estados)         = {{ {string.Join(", ", Q)} }}");
            Console.WriteLine($"Sigma (Alfabeto)     = {{ {string.Join(", ", A)} }}");
            Console.WriteLine($"q0 (Estado inicial)  = {S}");
            Console.WriteLine($"F  (Estados finales) = {{ {string.Join(", ", F)} }}");
            Console.WriteLine();
            Console.WriteLine(GenerarTablaTransicionTexto());
        }
        // Devuelve la tabla de transición como texto formateado, por si Main.cs
        // quiere reutilizarla (por ejemplo, para guardarla en un archivo de salida).
        public string GenerarTablaTransicionTexto() {
            if (!EsValido) { return "No se puede generar la tabla: el autómata no es válido."; }

            List<string> simbolosOrdenados = new List<string>(A);
            List<string> estadosOrdenados = new List<string>(Q);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TABLA DE TRANSICION:");

            sb.Append(string.Format("{0,-10}", "Estado"));
            foreach (string simbolo in simbolosOrdenados) { sb.Append(string.Format("{0,-10}", simbolo)); }
            sb.AppendLine();

            foreach (string estado in estadosOrdenados) {
                string etiquetaEstado = estado;
                if (estado == S) etiquetaEstado = "->" + etiquetaEstado;
                if (F.Contains(estado)) etiquetaEstado = "*" + etiquetaEstado;

                sb.Append(string.Format("{0,-10}", etiquetaEstado));

                foreach (string simbolo in simbolosOrdenados) {
                    string clave = $"{estado}|{simbolo}";
                    string destino = TablaTransiciones.ContainsKey(clave) ? TablaTransiciones[clave] : "-";
                    sb.Append(string.Format("{0,-10}", destino));
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("-> indica el estado inicial, * indica un estado final.");

            return sb.ToString();
        }

        // ---------------------------------------------------------------
        // SIMULACIÓN
        // Divide la cadena de entrada en símbolos individuales, respetando
        // si el alfabeto usa símbolos de un solo carácter o de varios.
        private List<string> TokenizarCadena(string cadena) {
            List<string> simbolos = new List<string>();

            if (string.IsNullOrEmpty(cadena)) { return simbolos; } // cadena vacía (epsilon)

            if (RequiereEspaciosEnCadenas) {
                // Alfabeto con símbolos de más de un carácter: se espera la cadena separada por espacios, por ejemplo: "ab cd 01"
                string[] partes = cadena.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                simbolos.AddRange(partes);
            }
            else { // Alfabeto de un solo carácter por símbolo: se procesa letra por letra
                foreach (char c in cadena)
                {
                    simbolos.Add(c.ToString());
                }
            }
            return simbolos;
        }
        // Evalúa una única cadena y devuelve la traza completa de ejecución
        // junto con el veredicto (aceptada o rechazada).
        public ResultadoSimulacion EvaluarCadena(string cadena) {
            if (!EsValido) {
                throw new Exception("No se puede evaluar: el autómata no es válido o no ha sido cargado correctamente.");
            }

            List<string> simbolos = TokenizarCadena(cadena);

            ResultadoSimulacion resultado = new ResultadoSimulacion(cadena);
            string estadoActual = S;

            foreach (string simbolo in simbolos) {
                if (!A.Contains(simbolo)) {
                    throw new Exception($"La cadena '{cadena}' contiene el símbolo '{simbolo}', que no pertenece al alfabeto Sigma definido.");
                }

                string clave = $"{estadoActual}|{simbolo}";
                // Como el autómata ya fue validado como AFD completo, esta clave siempre existe.
                string estadoSiguiente = TablaTransiciones[clave];

                resultado.Traza.Add(new PasoTraza(estadoActual, simbolo, estadoSiguiente));
                estadoActual = estadoSiguiente;
            }
            resultado.EstadoFinal = estadoActual;
            resultado.Aceptada = F.Contains(estadoActual);

            return resultado;
        }
        // Evalúa un conjunto de cadenas (por ejemplo, leídas línea por línea de un archivo).
        // Si una cadena en particular falla (símbolo inválido, etc.), no detiene el resto del lote:
        // guarda el error en ese ResultadoSimulacion y continúa con las demás.
        public List<ResultadoSimulacion> EvaluarLote(List<string> cadenas) {
            List<ResultadoSimulacion> resultados = new List<ResultadoSimulacion>();

            foreach (string cadena in cadenas) {
                try { resultados.Add(EvaluarCadena(cadena)); }
                catch (Exception ex) {
                    ResultadoSimulacion errorResultado = new ResultadoSimulacion(cadena);
                    errorResultado.Error = ex.Message;
                    resultados.Add(errorResultado);
                }
            }
            return resultados;
        }

        // ---------------------------------------------------------------
        // REINICIO
        // Limpia el autómata actual para poder cargar uno nuevo sin reiniciar el programa.
        public void Reiniciar() {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            F = new HashSet<string>();
            S = null;
            TransicionesOriginales = new List<Transicion>();
            TablaTransiciones = new Dictionary<string, string>();
            EsValido = false;
            RequiereEspaciosEnCadenas = false;
        }
    }
}