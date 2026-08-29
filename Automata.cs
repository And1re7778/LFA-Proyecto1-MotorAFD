using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ProyectoAutomata
{
    public class Automata
    {
        public HashSet<string> Q { 
            get; private set; 
            }
        public HashSet<string> A { 
            get; private set; 
            }
        public string S { 
            get; private set; 
            }
        public HashSet<string> F { 
            get; private set; 
            }
        
        private List<Transicion> TransicionesOriginales;
                private Dictionary<string, string> TablaTransiciones;

        public bool EsValido { 
            get; private set; 
            }
        private bool RequiereEspaciosEnCadenas;

        public Automata()
        {
            Q = new HashSet<string>();
            A = new HashSet<string>();
            F = new HashSet<string>();
            TransicionesOriginales = new List<Transicion>();
            TablaTransiciones = new Dictionary<string, string>();
            EsValido = false;
        }


        public void CargarManual(HashSet<string> q, HashSet<string> a, string s, HashSet<string> f, List<Transicion> transiciones)
        {
            Q = q;
            A = a;
            S = s;
            F = f;
            TransicionesOriginales = transiciones;
            ValidarAutomata();
        }


        public void CargarDesdeArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                throw new Exception($"El archivo '{rutaArchivo}' no existe en el directorio actual.");
            }

            string contenido = File.ReadAllText(rutaArchivo);


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
            {
                throw new Exception("Error de sintaxis: El bloque T está vacío o las transiciones no tienen el formato (estado,simbolo)->estado.");
            }

            foreach (Match m in matchesTransicion)
            {
                string origen = m.Groups[1].Value.Trim();
                string simbolo = m.Groups[2].Value.Trim();
                string destino = m.Groups[3].Value.Trim();
                TransicionesOriginales.Add(new Transicion(origen, simbolo, destino));
            }

            ValidarAutomata();
        }

        private void LlenarConjunto(HashSet<string> conjunto, string valores)
        {
            if (string.IsNullOrWhiteSpace(valores)) return;
            string[] partes = valores.Split(',');
            foreach (string p in partes)
            {
                string valorLimpio = p.Trim();
                if (!string.IsNullOrEmpty(valorLimpio))
                {
                    conjunto.Add(valorLimpio);
                }
            }
        }

        private void ValidarAutomata()
        {
            EsValido = false; 
            TablaTransiciones.Clear();

            if (Q.Count == 0) throw new Exception("El conjunto de estados Q no puede estar vacío.");
            if (A.Count == 0) throw new Exception("El alfabeto A no puede estar vacío.");
            if (!Q.Contains(S)) throw new Exception($"El estado inicial S ('{S}') no pertenece al conjunto de estados Q.");

            foreach (string f in F)
            {
                if (!Q.Contains(f)) throw new Exception($"El estado final '{f}' no pertenece al conjunto de estados Q.");
            }

            HashSet<string> paresVistos = new HashSet<string>();

            foreach (var t in TransicionesOriginales)
            {
                if (!Q.Contains(t.EstadoOrigen)) throw new Exception($"Transición inválida: El estado origen '{t.EstadoOrigen}' no existe en Q.");
                if (!A.Contains(t.Simbolo)) throw new Exception($"Transición inválida: El símbolo '{t.Simbolo}' no existe en el alfabeto A.");
                if (!Q.Contains(t.EstadoDestino)) throw new Exception($"Transición inválida: El estado destino '{t.EstadoDestino}' no existe en Q.");

                string clave = $"{t.EstadoOrigen}|{t.Simbolo}";
                if (paresVistos.Contains(clave))
                {
                    throw new Exception($"Error de determinismo: Transición múltiple detectada para el estado '{t.EstadoOrigen}' con el símbolo '{t.Simbolo}'. Esto es un AFND.");
                }
                
                paresVistos.Add(clave);
                TablaTransiciones.Add(clave, t.EstadoDestino);
            }

            foreach (string q in Q)
            {
                foreach (string a in A)
                {
                    string clave = $"{q}|{a}";
                    if (!paresVistos.Contains(clave))
                    {
                        throw new Exception($"Error de determinismo: Falta la transición para el estado '{q}' con el símbolo '{a}'.");
                    }
                }
            }

            RequiereEspaciosEnCadenas = false;
            foreach (string a in A)
            {
                if (a.Length > 1)
                {
                    RequiereEspaciosEnCadenas = true;
                    break;
                }
            }

            EsValido = true;
        }


        public void MostrarDefinicionYTabla()
        {
            //falta implementar
        }

        public void EvaluarCadena()
        {
            //falta implementar
        }
    }
}