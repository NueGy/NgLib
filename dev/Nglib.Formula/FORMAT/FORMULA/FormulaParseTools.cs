//Copyright Nglib 2020 - MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Outils pour découpper une fonction
    /// </summary>
    public class FormulaParseTools
    {



        /// <summary>
        /// Parser une formule en segments (hiérarchique/récursif comme ParseSub)
        /// </summary>
        /// <param name="text">Formule : 'test:'+mul(4,8)</param>
        /// <returns>Segment principal avec arbre hiérarchique de sous-segments</returns>
        public static FormulaSegmentModel Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = text.Trim();
            try
            {
                // Parse récursif depuis la racine
                var rootSegment = ParseRecursive(text, 0);
                return rootSegment;
            }
            catch (Exception ex)
            {
                throw new Exception($"Formula.Parse Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse récursif d'une formule en segments
        /// </summary>
        private static FormulaSegmentModel ParseRecursive(string text, int position)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = text.Trim();

            // Segment racine
            var segment = new FormulaSegmentModel
            {
                Text = text,
                Position = position,
                SegmentType = FormulaSegmentTypeEnum.FormulaContent
            };

            // 1. Détecter les constantes simples (si pas de parenthèses ni opérateurs)
            if (!text.Contains('(') && !ContainsOperator(text))
            {
                var constantSegment = ParseConstant(text, position);
                if (constantSegment != null) return constantSegment;
            }

            // 2. Détecter les formules multi-parts (avec opérateurs + - * / < > ==, etc.)
            var parts = SplitByOperators(text);
            if (parts.Count > 1)
            {
                segment.SegmentType = FormulaSegmentTypeEnum.FormulaContent;
                int currentPos = position;

                foreach (var part in parts)
                {
                    if (part.IsOperator)
                    {
                        // C'est un opérateur
                        var opSegment = new FormulaSegmentModel
                        {
                            SegmentType = FormulaSegmentTypeEnum.Operator,
                            Text = part.Text,
                            Position = currentPos
                        };
                        segment.SubSegments.Add(opSegment);
                    }
                    else
                    {
                        // C'est une valeur/fonction à parser récursivement
                        var subSegment = ParseRecursive(part.Text, currentPos);
                        if (subSegment != null)
                            segment.SubSegments.Add(subSegment);
                    }
                    currentPos += part.Text.Length;
                }
                return segment;
            }

            // 3. Détecter les fonctions : nom(args)
            var functionParts = SplitFunction(text);
            if (functionParts != null)
            {
                segment.SegmentType = FormulaSegmentTypeEnum.Function;
                segment.Text = functionParts.Item1; // Nom de la fonction

                // Parser les arguments
                if (!string.IsNullOrWhiteSpace(functionParts.Item2))
                {
                    var args = SplitArguments(functionParts.Item2);
                    int argPos = position + functionParts.Item1.Length + 1; // +1 pour la parenthèse

                    foreach (var arg in args)
                    {
                        var argSegment = ParseRecursive(arg, argPos);
                        if (argSegment != null)
                            segment.SubSegments.Add(argSegment);
                        argPos += arg.Length + 1; // +1 pour la virgule
                    }
                }
                return segment;
            }

            // 4. Détecter les sous-formules entre parenthèses : (a+b), ('Total:'+div(@price,2))
            if (text.StartsWith("(") && text.EndsWith(")"))
            {
                // Retirer les parenthèses externes et parser le contenu
                string innerContent = text.Substring(1, text.Length - 2).Trim();
                if (!string.IsNullOrWhiteSpace(innerContent))
                {
                    // Parser récursivement le contenu sans les parenthèses
                    var innerSegment = ParseRecursive(innerContent, position + 1);
                    if (innerSegment != null)
                    {
                        // Garder le type FormulaContent pour indiquer que c'est une sous-formule groupée
                        innerSegment.SegmentType = FormulaSegmentTypeEnum.FormulaContent;
                        return innerSegment;
                    }
                }
            }

            // 5. Si rien ne correspond, c'est peut-être une constante complexe
            var finalConstant = ParseConstant(text, position);
            if (finalConstant != null) return finalConstant;

            // 6. Retourner le segment brut si on ne sait pas quoi en faire
            return segment;
        }

        /// <summary>
        /// Parse une constante simple (nombre, string, paramètre)
        /// </summary>
        private static FormulaSegmentModel ParseConstant(string text, int position)
        {
            text = text.Trim();

            // NULL
            if (text.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                return new FormulaSegmentModel
                {
                    SegmentType = FormulaSegmentTypeEnum.ValString,
                    Text = "NULL",
                    Position = position
                };
            }

            // Paramètre @xxx
            if (text.StartsWith("@"))
            {
                return new FormulaSegmentModel
                {
                    SegmentType = FormulaSegmentTypeEnum.ValParameter,
                    Text = text,
                    Position = position
                };
            }

            // String entre quotes 'xxx' ou "xxx"
            if ((text.StartsWith("'") && text.EndsWith("'")) ||
                (text.StartsWith("\"") && text.EndsWith("\"")))
            {
                return new FormulaSegmentModel
                {
                    SegmentType = FormulaSegmentTypeEnum.ValString,
                    Text = text.Trim('\'', '"'),
                    Position = position
                };
            }

            // Nombre
            if (Nglib.FORMAT.NumberTools.IsNumeric(text))
            {
                return new FormulaSegmentModel
                {
                    SegmentType = FormulaSegmentTypeEnum.ValNumber,
                    Text = text,
                    Position = position
                };
            }

            return null;
        }

        /// <summary>
        /// Vérifie si le texte contient des opérateurs
        /// </summary>
        private static bool ContainsOperator(string text)
        {
            // Opérateurs standards
            string[] operators = { "+", "-", "*", "/", "==", "!=", ">=", "<=", ">", "<", "&&", "||" };
            foreach (var op in operators)
            {
                if (text.Contains(op))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Découpe le texte par opérateurs en respectant les parenthèses et les quotes
        /// </summary>
        private static List<FormulaPart> SplitByOperators(string text)
        {
            var parts = new List<FormulaPart>();
            if (string.IsNullOrWhiteSpace(text)) return parts;

            // Opérateurs à détecter (ordre important : les plus longs d'abord)
            string[] operators = { "==", "!=", ">=", "<=", "&&", "||", "+", "-", "*", "/", ">", "<" };

            int pos = 0;
            int lastPos = 0;
            bool inQuotes = false;
            char quoteChar = ' ';
            int parenthesesLevel = 0;

            while (pos < text.Length)
            {
                char c = text[pos];

                // Gestion des quotes
                if ((c == '\'' || c == '"') && (pos == 0 || text[pos - 1] != '\\'))
                {
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = c;
                    }
                    else if (c == quoteChar)
                    {
                        inQuotes = false;
                    }
                }

                // Gestion des parenthèses (uniquement si pas dans des quotes)
                if (!inQuotes)
                {
                    if (c == '(') parenthesesLevel++;
                    if (c == ')') parenthesesLevel--;

                    // Chercher un opérateur uniquement si on est au niveau 0 de parenthèses
                    if (parenthesesLevel == 0)
                    {
                        foreach (var op in operators)
                        {
                            if (pos + op.Length <= text.Length &&
                                text.Substring(pos, op.Length) == op)
                            {
                                // Ajouter la partie avant l'opérateur
                                if (pos > lastPos)
                                {
                                    parts.Add(new FormulaPart
                                    {
                                        Text = text.Substring(lastPos, pos - lastPos).Trim(),
                                        IsOperator = false
                                    });
                                }

                                // Ajouter l'opérateur
                                parts.Add(new FormulaPart
                                {
                                    Text = op,
                                    IsOperator = true
                                });

                                pos += op.Length;
                                lastPos = pos;
                                goto NextIteration; // Sortir de la boucle foreach
                            }
                        }
                    }
                }

                pos++;
            NextIteration:;
            }

            // Ajouter la dernière partie
            if (lastPos < text.Length)
            {
                parts.Add(new FormulaPart
                {
                    Text = text.Substring(lastPos).Trim(),
                    IsOperator = false
                });
            }

            return parts;
        }

        /// <summary>
        /// Extrait le nom de fonction et ses arguments : "add(1,2)" -> ("add", "1,2")
        /// Retourne null si c'est une sous-formule entre parenthèses sans fonction : "(a+b)"
        /// </summary>
        private static Tuple<string, string> SplitFunction(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = text.Trim();

            int firstParen = text.IndexOf('(');
            if (firstParen < 0) return null;

            int lastParen = text.LastIndexOf(')');
            if (lastParen < 0) return null;

            string functionName = text.Substring(0, firstParen).Trim();

            // Si pas de nom avant la parenthèse, c'est une sous-formule groupée, pas une fonction
            // Exemple : (a+b) ou ('Total:'+div(@price,2))
            if (string.IsNullOrWhiteSpace(functionName))
                return null;

            // Si le "nom" contient des espaces ou opérateurs, ce n'est pas une fonction valide
            if (functionName.Contains(' ') || ContainsOperator(functionName))
                return null;

            string arguments = text.Substring(firstParen + 1, lastParen - firstParen - 1).Trim();

            return new Tuple<string, string>(functionName, arguments);
        }

        /// <summary>
        /// Découpe les arguments en respectant les parenthèses et les quotes
        /// </summary>
        private static List<string> SplitArguments(string argumentsText)
        {
            var args = new List<string>();
            if (string.IsNullOrWhiteSpace(argumentsText)) return args;

            int pos = 0;
            int lastPos = 0;
            bool inQuotes = false;
            char quoteChar = ' ';
            int parenthesesLevel = 0;

            while (pos < argumentsText.Length)
            {
                char c = argumentsText[pos];

                // Gestion des quotes
                if ((c == '\'' || c == '"') && (pos == 0 || argumentsText[pos - 1] != '\\'))
                {
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = c;
                    }
                    else if (c == quoteChar)
                    {
                        inQuotes = false;
                    }
                }

                // Gestion des parenthèses et virgules
                if (!inQuotes)
                {
                    if (c == '(') parenthesesLevel++;
                    if (c == ')') parenthesesLevel--;

                    // Virgule au niveau 0 = séparateur d'arguments
                    if (c == ',' && parenthesesLevel == 0)
                    {
                        args.Add(argumentsText.Substring(lastPos, pos - lastPos).Trim());
                        lastPos = pos + 1;
                    }
                }

                pos++;
            }

            // Ajouter le dernier argument
            if (lastPos < argumentsText.Length)
            {
                args.Add(argumentsText.Substring(lastPos).Trim());
            }

            return args;
        }

        /// <summary>
        /// Classe interne pour représenter une partie de formule (valeur ou opérateur)
        /// </summary>
        private class FormulaPart
        {
            public string Text { get; set; }
            public bool IsOperator { get; set; }
        }




        /// <summary>
        /// Obtenir la liste de toutes les commands dans toutes les sous-fonctions
        /// </summary>
        public static List<string> GetRecursivesFunctionNames(FormulaSegmentModel item)
        {
            if (item == null) return new List<string>();
            var commands = new List<string>();
            if(item.SegmentType == FormulaSegmentTypeEnum.Function) commands.Add(item.Text);
            if (item.SubSegments != null)
                commands.AddRange(item.SubSegments.SelectMany(m => GetRecursivesFunctionNames(m)));
            return commands.Select(c => c?.ToUpper()).Distinct().ToList();
        }



        /// <summary>
        /// Pour savoir si une formule est une constante SIMPLE (string ou int)
        /// </summary>
        public static bool IsConstant(FormulaSegmentModel item)
        {
            if (item == null) return false;
            if (item.SegmentType== FormulaSegmentTypeEnum.ValNumber) return true;
            if (item.SegmentType == FormulaSegmentTypeEnum.ValString) return true;
            if (item.SegmentType == FormulaSegmentTypeEnum.ValParameter) return false;
            return false;
        }









        /// <summary>
        /// Identifie toutes les fonctions dans une chaine. Les fonctions sont identifiées par {=xxxxxx}
        /// Utilise StringTools.SplitEncapsuled pour gérer les accolades imbriquées
        /// </summary>
        public static Dictionary<int, string> ExtractComposedString(string complexstring)
        {
            Dictionary<int, string> listFormulas = new Dictionary<int, string>();
            if (string.IsNullOrWhiteSpace(complexstring)) return listFormulas;

            // Utiliser StringTools.SplitEncapsuled pour gérer les imbrications correctement
            string[] encapsulated = Nglib.FORMAT.StringTools.SplitEncapsuled(complexstring, "{=", "}");
            
            foreach (string formula in encapsulated)
            {
                if (string.IsNullOrWhiteSpace(formula)) continue;
                
                // Trouver la position dans la chaîne originale
                string formulaContent = formula.Substring(2, formula.Length - 3); // Enlever {= et }
                int position = complexstring.IndexOf(formula, StringComparison.Ordinal);
                if (position >= 0)
                {
                    listFormulas.Add(position, formulaContent);
                }
            }

            return listFormulas;
        }



    }
}
