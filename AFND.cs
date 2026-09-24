using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProyectoAutomata
{
    public class AFND
    {
        public HashSet<string> Q { get; private set; }
        public HashSet<string> A { get; private set; }
        public string S { get; private set; }
        public HashSet<string> F { get; private set; }
        
        private Dictionary<string, HashSet<string>> TablaTransiciones;

        public AFND()
        {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            F = new HashSet<string>();
            TablaTransiciones = new Dictionary<string, HashSet<string>>();
        }

        public void CargarDesdeArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new Exception($"El archivo '{rutaArchivo}' no existe en el directorio actual.");

            string contenido = File.ReadAllText(rutaArchivo);
            
            Match matchQ = Regex.Match(contenido, @"^Q\s*=\s*\{([^}]*)\}", RegexOptions.Multiline);
            if (matchQ.Success) LlenarConjunto(Q, matchQ.Groups[1].Value);

            Match matchA = Regex.Match(contenido, @"^A\s*=\s*\{([^}]*)\}", RegexOptions.Multiline);
            if (matchA.Success) LlenarConjunto(A, matchA.Groups[1].Value);

            Match matchS = Regex.Match(contenido, @"^S\s*=\s*([a-zA-Z0-9_]+)", RegexOptions.Multiline);
            if (matchS.Success) S = matchS.Groups[1].Value.Trim();

            Match matchF = Regex.Match(contenido, @"^F\s*=\s*\{([^}]*)\}", RegexOptions.Multiline);
            if (matchF.Success) LlenarConjunto(F, matchF.Groups[1].Value);

            Match matchT = Regex.Match(contenido, @"T\s*=\s*\{([^}]*)\}", RegexOptions.Singleline);
            if (!matchT.Success) throw new Exception("Error de sintaxis: No se encontró el bloque de transiciones T.");

            MatchCollection matchesTransicion = Regex.Matches(matchT.Groups[1].Value, @"\(\s*([^,]+)\s*,\s*([^)]+)\s*\)\s*->\s*([^,\s]+)");

            foreach (Match m in matchesTransicion)
            {
                string origen = m.Groups[1].Value.Trim();
                string simbolo = m.Groups[2].Value.Trim();
                string destino = m.Groups[3].Value.Trim();

                string clave = $"{origen}|{simbolo}";
                if (!TablaTransiciones.ContainsKey(clave))
                {
                    TablaTransiciones[clave] = new HashSet<string>();
                }
                TablaTransiciones[clave].Add(destino); 
            }
        }

        private void LlenarConjunto(HashSet<string> conjunto, string valores)
        {
            foreach (string p in valores.Split(','))
            {
                string valorLimpio = p.Trim();
                if (!string.IsNullOrEmpty(valorLimpio)) conjunto.Add(valorLimpio);
            }
        }

        public void MostrarTablaTransicionAFND()
        {
            Console.WriteLine("\nTABLA DE TRANSICIÓN (AFND):");
            Console.Write(string.Format("{0,-15}", "Estado"));
            foreach (string sim in A) Console.Write(string.Format("{0,-15}", sim));
            Console.WriteLine();

            foreach (string q in Q)
            {
                string eq = q;
                if (q == S) eq = "->" + eq;
                if (F.Contains(q)) eq = "*" + eq;
                Console.Write(string.Format("{0,-15}", eq));

                foreach (string sim in A)
                {
                    string clave = $"{q}|{sim}";
                    string destinos = TablaTransiciones.ContainsKey(clave) 
                        ? "{" + string.Join(",", TablaTransiciones[clave]) + "}" 
                        : "-";
                    Console.Write(string.Format("{0,-15}", destinos));
                }
                Console.WriteLine();
            }
        }

        public Automata TransformarAAFD()
        {
            Console.WriteLine("\n--- INICIANDO TRANSFORMACIÓN AFND -> AFD ---");
            
            Queue<HashSet<string>> colaMacroEstados = new Queue<HashSet<string>>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>(); // {q0,q1} -> A
            Dictionary<string, HashSet<string>> mapaInverso = new Dictionary<string, HashSet<string>>(); // A -> {q0,q1}
            
            List<Transicion> nuevasTransiciones = new List<Transicion>();
            HashSet<string> nuevosEstadosF = new HashSet<string>();
            char generadorNombres = 'A';

            HashSet<string> subconjuntoInicial = new HashSet<string> { S };
            string claveInicial = ObtenerClave(subconjuntoInicial);
            
            string nombreInicial = generadorNombres.ToString();
            generadorNombres++;

            equivalencias[claveInicial] = nombreInicial;
            mapaInverso[nombreInicial] = subconjuntoInicial;
            colaMacroEstados.Enqueue(subconjuntoInicial);

            while (colaMacroEstados.Count > 0)
            {
                HashSet<string> actual = colaMacroEstados.Dequeue();
                string claveActual = ObtenerClave(actual);
                string nombreMacroActual = equivalencias[claveActual];

                if (actual.Any(estado => F.Contains(estado)))
                {
                    nuevosEstadosF.Add(nombreMacroActual);
                }

                foreach (string simbolo in A)
                {
                    HashSet<string> alcanzables = new HashSet<string>();
                    
                    foreach (string estado in actual)
                    {
                        string transClave = $"{estado}|{simbolo}";
                        if (TablaTransiciones.ContainsKey(transClave))
                        {
                            alcanzables.UnionWith(TablaTransiciones[transClave]);
                        }
                    }

                    if (alcanzables.Count > 0)
                    {
                        string claveAlcanzables = ObtenerClave(alcanzables);
                        if (!equivalencias.ContainsKey(claveAlcanzables))
                        {
                            string nuevoNombre = generadorNombres.ToString();
                            generadorNombres++;
                            equivalencias[claveAlcanzables] = nuevoNombre;
                            mapaInverso[nuevoNombre] = alcanzables;
                            colaMacroEstados.Enqueue(alcanzables);
                        }
                        
                        nuevasTransiciones.Add(new Transicion(nombreMacroActual, simbolo, equivalencias[claveAlcanzables]));
                    }
                }
            }

            Console.WriteLine("\nTABLA DE EQUIVALENCIAS (Macro-Estados):");
            foreach (var kvp in equivalencias)
            {
                Console.WriteLine($"Macro-Estado {kvp.Value} = {{ {kvp.Key} }}");
            }

            HashSet<string> nuevosEstadosQ = new HashSet<string>(equivalencias.Values);
            Automata nuevoAFD = new Automata();
            nuevoAFD.CargarManual(nuevosEstadosQ, new HashSet<string>(A), nombreInicial, nuevosEstadosF, nuevasTransiciones);
            
            return nuevoAFD;
        }

        private string ObtenerClave(HashSet<string> conjunto)
        {
            var ordenado = conjunto.OrderBy(x => x).ToList();
            return string.Join(",", ordenado);
        }
    }
}