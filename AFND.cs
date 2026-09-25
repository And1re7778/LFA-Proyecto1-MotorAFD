using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProyectoAutomata
{
    // Permite cargar (archivo o manual), validar su estructura, mostrar su tabla de transición
    // y transformarse a un AFD equivalente mediante el Algoritmo de Construcción de Subconjuntos.
    public class AFND
    {
        // Representa un Autómata Finito No Determinista (AFND) mediante su quíntupla M = (Q, Σ, δ, q0, F),
        public HashSet<string> Q { get; private set; }
        public HashSet<string> A { get; private set; }
        public string S { get; private set; }
        public HashSet<string> F { get; private set; }

        private List<Transicion> TransicionesOriginales;
        private Dictionary<string, HashSet<string>> TablaTransiciones;
        public bool EsValido { get; private set; }

        public AFND() {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            F = new HashSet<string>();
            TransicionesOriginales = new List<Transicion>();
            TablaTransiciones = new Dictionary<string, HashSet<string>>();
            EsValido = false;
        }

        // ---------------------------------------------------------------
        // CARGA
        public void CargarManual(HashSet<string> q, HashSet<string> a, string s, HashSet<string> f, List<Transicion> transiciones) {
            Q = q;
            A = a;
            S = s;
            F = f;
            TransicionesOriginales = transiciones;
            ValidarAFND();
        }

        public void CargarDesdeArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new Exception($"El archivo '{rutaArchivo}' no existe en el directorio actual.");

            string contenido = File.ReadAllText(rutaArchivo);

            // se reinicia por si se estaba recargando un AFND anterior
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

            if (matchesTransicion.Count == 0 && Q.Count > 0 && A.Count > 0)
                throw new Exception("Error de sintaxis: El bloque T está vacío o las transiciones no tienen el formato (estado,simbolo)->estado.");

            foreach (Match m in matchesTransicion)
            {
                string origen = m.Groups[1].Value.Trim();
                string simbolo = m.Groups[2].Value.Trim();
                string destino = m.Groups[3].Value.Trim();
                TransicionesOriginales.Add(new Transicion(origen, simbolo, destino));
            }

            ValidarAFND();
        }

        private void LlenarConjunto(HashSet<string> conjunto, string valores) {
            if (string.IsNullOrWhiteSpace(valores)) return;
            foreach (string p in valores.Split(','))
            {
                string valorLimpio = p.Trim();
                if (!string.IsNullOrEmpty(valorLimpio)) conjunto.Add(valorLimpio);
            }
        }

        // ---------------------------------------------------------------
        // VALIDACIÓN
        // solo se valida que todo lo referenciado exista dentro de Q y Σ.
        private void ValidarAFND() {
            EsValido = false;

            if (Q.Count == 0) throw new Exception("El conjunto de estados Q no puede estar vacío.");
            if (A.Count == 0) throw new Exception("El alfabeto A no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(S)) throw new Exception("Debe definirse el estado inicial S.");
            if (!Q.Contains(S)) throw new Exception($"El estado inicial S ('{S}') no pertenece al conjunto de estados Q.");

            foreach (string f in F)
                if (!Q.Contains(f)) throw new Exception($"El estado final '{f}' no pertenece al conjunto de estados Q.");

            Dictionary<string, HashSet<string>> tablaTemporal = new Dictionary<string, HashSet<string>>();

            foreach (var t in TransicionesOriginales)
            {
                if (!Q.Contains(t.EstadoOrigen)) throw new Exception($"Transición inválida: el estado origen '{t.EstadoOrigen}' no existe en Q.");
                if (!A.Contains(t.Simbolo)) throw new Exception($"Transición inválida: el símbolo '{t.Simbolo}' no existe en el alfabeto A.");
                if (!Q.Contains(t.EstadoDestino)) throw new Exception($"Transición inválida: el estado destino '{t.EstadoDestino}' no existe en Q.");

                string clave = $"{t.EstadoOrigen}|{t.Simbolo}";
                if (!tablaTemporal.ContainsKey(clave))
                    tablaTemporal[clave] = new HashSet<string>();
                tablaTemporal[clave].Add(t.EstadoDestino);
            }

            TablaTransiciones = tablaTemporal;
            EsValido = true;
        }

        // ---------------------------------------------------------------
        // DEFINICIÓN FORMAL Y TABLA DE TRANSICIÓN
        public void MostrarDefinicionYTabla() {
            if (!EsValido) {
                Console.WriteLine("El AFND no es válido o no ha sido cargado.");
                return;
            }
            Console.WriteLine("========== DEFINICIÓN FORMAL DEL AFND ==========");
            Console.WriteLine($"Q  (Estados)         = {{ {string.Join(", ", Q)} }}");
            Console.WriteLine($"Sigma (Alfabeto)     = {{ {string.Join(", ", A)} }}");
            Console.WriteLine($"q0 (Estado inicial)  = {S}");
            Console.WriteLine($"F  (Estados finales) = {{ {string.Join(", ", F)} }}");
            Console.WriteLine();
            Console.WriteLine(GenerarTablaTransicionTexto());
        }
        public string GenerarTablaTransicionTexto() {
            if (!EsValido) return "No se puede generar la tabla: el AFND no es válido.";

            List<string> simbolosOrdenados = new List<string>(A);
            List<string> estadosOrdenados = new List<string>(Q);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("========== TABLA DE TRANSICION (AFND) ==========");
            sb.Append(string.Format("{0,-15}", "Estado"));
            foreach (string simbolo in simbolosOrdenados) sb.Append(string.Format("{0,-15}", simbolo));
            sb.AppendLine();

            foreach (string estado in estadosOrdenados) {
                string etiquetaEstado = estado;
                if (estado == S) etiquetaEstado = "->" + etiquetaEstado;
                if (F.Contains(estado)) etiquetaEstado = "*" + etiquetaEstado;
                sb.Append(string.Format("{0,-15}", etiquetaEstado));

                foreach (string simbolo in simbolosOrdenados)
                {
                    string clave = $"{estado}|{simbolo}";
                    string destinos = TablaTransiciones.ContainsKey(clave)
                        ? "{" + string.Join(",", TablaTransiciones[clave]) + "}"
                        : "-";
                    sb.Append(string.Format("{0,-15}", destinos));
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("-> indica el estado inicial, * indica un estado final, - indica que no hay transición definida.");
            return sb.ToString();
        }

        // ---------------------------------------------------------------
        // TRANSFORMACIÓN DE AFND A AFD 
        public Automata TransformarAAFD() {
            if (!EsValido)
                throw new Exception("No se puede transformar: el AFND no es válido o no ha sido cargado correctamente.");

            Console.WriteLine();
            Console.WriteLine("--- INICIANDO TRANSFORMACIÓN AFND -> AFD (Construcción de Subconjuntos) ---");

            Queue<HashSet<string>> colaMacroEstados = new Queue<HashSet<string>>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();          // "q0,q1" -> "A"
            Dictionary<string, HashSet<string>> mapaInverso = new Dictionary<string, HashSet<string>>(); // "A" -> {q0,q1}

            List<Transicion> nuevasTransiciones = new List<Transicion>();
            HashSet<string> nuevosEstadosF = new HashSet<string>();
            int contadorNombres = 0;
            string estadoMuerto = null; // estado trampa; solo se crea si realmente se necesita

            HashSet<string> subconjuntoInicial = new HashSet<string> { S };
            string claveInicial = ObtenerClave(subconjuntoInicial);
            string nombreInicial = GenerarNombreMacroEstado(contadorNombres++);

            equivalencias[claveInicial] = nombreInicial;
            mapaInverso[nombreInicial] = subconjuntoInicial;
            colaMacroEstados.Enqueue(subconjuntoInicial);

            while (colaMacroEstados.Count > 0) {
                HashSet<string> actual = colaMacroEstados.Dequeue();
                string claveActual = ObtenerClave(actual);
                string nombreMacroActual = equivalencias[claveActual];

                if (actual.Any(estado => F.Contains(estado)))
                    nuevosEstadosF.Add(nombreMacroActual);

                foreach (string simbolo in A) {
                    HashSet<string> alcanzables = new HashSet<string>();

                    foreach (string estado in actual) {
                        string transClave = $"{estado}|{simbolo}";
                        if (TablaTransiciones.ContainsKey(transClave))
                            alcanzables.UnionWith(TablaTransiciones[transClave]);
                    }

                    if (alcanzables.Count > 0) {
                        string claveAlcanzables = ObtenerClave(alcanzables);
                        if (!equivalencias.ContainsKey(claveAlcanzables)) {
                            string nuevoNombre = GenerarNombreMacroEstado(contadorNombres++);
                            equivalencias[claveAlcanzables] = nuevoNombre;
                            mapaInverso[nuevoNombre] = alcanzables;
                            colaMacroEstados.Enqueue(alcanzables);
                        }
                        nuevasTransiciones.Add(new Transicion(nombreMacroActual, simbolo, equivalencias[claveAlcanzables]));
                    }
                    else {
                        // Ningún estado del subconjunto actual tiene transición con este símbolo.
                        // Para que el AFD resultante siga siendo una función total (como exige
                        // la definición de AFD), se crea un único estado trampa con auto-transiciones.
                        if (estadoMuerto == null) estadoMuerto = "∅";
                        nuevasTransiciones.Add(new Transicion(nombreMacroActual, simbolo, estadoMuerto));
                    }
                }
            }
            HashSet<string> nuevosEstadosQ = new HashSet<string>(equivalencias.Values);

            if (estadoMuerto != null) {
                nuevosEstadosQ.Add(estadoMuerto);
                foreach (string simbolo in A)
                    nuevasTransiciones.Add(new Transicion(estadoMuerto, simbolo, estadoMuerto));
            }

            Console.WriteLine();
            Console.WriteLine("TABLA DE EQUIVALENCIAS (Macro-Estados):");
            foreach (var kvp in equivalencias)
                Console.WriteLine($"Macro-Estado {kvp.Value} = {{ {kvp.Key} }}");
            if (estadoMuerto != null)
                Console.WriteLine($"Macro-Estado {estadoMuerto} = {{ }}  (estado trampa, no acepta ninguna cadena)");

            Automata nuevoAFD = new Automata();
            nuevoAFD.CargarManual(nuevosEstadosQ, new HashSet<string>(A), nombreInicial, nuevosEstadosF, nuevasTransiciones);
            return nuevoAFD;
        }
        // los macro estados son, por ejemplo A = q0, q1 y B = q0    
        private string GenerarNombreMacroEstado(int indice) {
            indice++;
            string nombre = "";
            while (indice > 0)
            {
                int resto = (indice - 1) % 26;
                nombre = (char)('A' + resto) + nombre;
                indice = (indice - 1) / 26;
            }
            return nombre;
        }

        private string ObtenerClave(HashSet<string> conjunto) {
            var ordenado = conjunto.OrderBy(x => x).ToList();
            return string.Join(",", ordenado);
        }

        // ---------------------------------------------------------------
        // REINICIO
        public void Reiniciar()
        {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            F = new HashSet<string>();
            S = null;
            TransicionesOriginales = new List<Transicion>();
            TablaTransiciones = new Dictionary<string, HashSet<string>>();
            EsValido = false;
        }
    }
}