using Nglib.DATA.ACCESSORS;
using Nglib.DATA.COLLECTIONS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Outils pour l'execution des formules
    /// </summary>
    public static class FormulaExecuteTools
    {


        /// <summary>
        /// Cache interne, le chargement est assez lourd: env200ms
        /// Thread-safe via ConcurrentDictionary
        /// </summary>
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<FormulaAttribute, MethodInfo> cacheFunctions 
            = new System.Collections.Concurrent.ConcurrentDictionary<FormulaAttribute, MethodInfo>();
        private static volatile bool isInit = false;
        private static readonly object initLock = new object();


        /// <summary>
        /// Calcule récursivement un segment et stocke le résultat dans le context
        /// Utilise le cache CalcValues pour éviter les recalculs
        /// </summary>
        internal static object CalculateSegment(FormulaContext context, FormulaSegmentModel segment)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            // Cache : si déjà calculé, retourne la valeur
            if (context.CalcValues.ContainsKey(segment))
                return context.CalcValues[segment];

            object result = null;

            // Calcul selon type de segment
            switch (segment.SegmentType)
            {
                case FormulaSegmentTypeEnum.ValNumber:
                    result = CalculateValueNumber(segment);
                    break;

                case FormulaSegmentTypeEnum.ValString:
                    result = CalculateValueString(segment);
                    break;

                case FormulaSegmentTypeEnum.ValParameter:
                    result = CalculateParameter(context, segment);
                    break;

                case FormulaSegmentTypeEnum.Function:
                    result = CalculateFunction(context, segment);
                    break;

                case FormulaSegmentTypeEnum.Operator:
                    result = CalculateOperator(context, segment);
                    break;

                case FormulaSegmentTypeEnum.FormulaContent:
                    result = CalculateFormulaContent(context, segment);
                    break;

                default:
                    throw new FormulaException(segment.Text, $"Unknown segment type: {segment.SegmentType}");
            }

            // Stocker résultat dans cache
            context.CalcValues[segment] = result;

            return result;
        }


        /// <summary>
        /// Calcule une constante numérique
        /// </summary>
        private static object CalculateValueNumber(FormulaSegmentModel segment)
        {
            if (int.TryParse(segment.Text, out int intVal))
                return intVal;
            if (double.TryParse(segment.Text, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out double dblVal))
                return dblVal;
            throw new FormulaException(segment.Text, $"Invalid number: {segment.Text}");
        }


        /// <summary>
        /// Calcule une constante string
        /// </summary>
        private static object CalculateValueString(FormulaSegmentModel segment)
        {
            // Le texte est déjà nettoyé dans le parsing (sans quotes)
            return segment.Text;
        }


        /// <summary>
        /// Résout un paramètre depuis context.Parameters (supporte @Param.Property.SubProperty)
        /// </summary>
        private static object CalculateParameter(FormulaContext context, FormulaSegmentModel segment)
        {
            string fullPath = segment.Text.TrimStart('@');

            // Supporte @MonPo.Nom ou @MonPo.Age
            string[] parts = fullPath.Split('.');
            string paramName = parts[0];

            if (context.Parameters == null || !context.Parameters.ContainsKey(paramName))
            {
                throw new FormulaException(segment.Text, $"Parameter not found: {paramName}({segment.Text})");
            }

            object value = context.Parameters[paramName];

            // Accès aux propriétés imbriquées (ex: @MonPo.Nom)
            for (int i = 1; i < parts.Length; i++)
            {
                if (value == null)
                {
                    throw new FormulaException(segment.Text, $"Cannot access property '{parts[i]}' on null value");
                }

                // Support DataPO (IDataAccessor)
                if (value is DATA.ACCESSORS.IDataAccessor accessor)
                {
                    value = accessor.GetObject(parts[i]);
                }
                // Support objets .NET standard
                else
                {
                    var propInfo = value.GetType().GetProperty(parts[i]);
                    if (propInfo == null)
                    {
                        throw new FormulaException(segment.Text, $"Property '{parts[i]}' not found on type {value.GetType().Name}");
                    }
                    value = propInfo.GetValue(value);
                }
            }

            return value;
        }


        /// <summary>
        /// Calcule une fonction avec ses arguments
        /// </summary>
        private static object CalculateFunction(FormulaContext context, FormulaSegmentModel segment)
        {
            string functionName = segment.Text.ToLower();

            // Calculer tous les arguments (SubSegments) récursivement
            object[] args = null;
            if (segment.SubSegments != null && segment.SubSegments.Count > 0)
            {
                args = segment.SubSegments
                    .Select(sub => CalculateSegment(context, sub))
                    .ToArray();
            }
            else
            {
                args = new object[0];
            }

            // Rechercher et invoquer la fonction
            return InvokeFunction(functionName, args);
        }


        /// <summary>
        /// Calcule un opérateur avec ses opérandes
        /// </summary>
        private static object CalculateOperator(FormulaContext context, FormulaSegmentModel segment)
        {
            // Note: Les opérateurs sont gérés dans FormulaContent qui contient opérateurs + opérandes
            // Ce cas ne devrait pas être appelé directement
            throw new FormulaException(segment.Text, "Operator segment should not be calculated directly");
        }


        /// <summary>
        /// Calcule le contenu d'une formule (peut contenir opérateurs, fonctions, etc.)
        /// </summary>
        private static object CalculateFormulaContent(FormulaContext context, FormulaSegmentModel segment)
        {
            if (segment.SubSegments == null || segment.SubSegments.Count == 0)
            {
                // Pas de sous-segments, essayer de parser comme constante
                var constValue = TryParseConstant(segment.Text);
                if (constValue != null) return constValue;
                throw new FormulaException(segment.Text, "Empty formula content");
            }

            // Si contient des opérateurs, traiter comme expression avec opérateurs
            var operators = segment.SubSegments.Where(s => s.SegmentType == FormulaSegmentTypeEnum.Operator).ToList();
            
            if (operators.Count > 0)
            {
                // Traiter l'expression avec opérateurs
                return EvaluateExpressionWithOperators(context, segment.SubSegments);
            }
            else if (segment.SubSegments.Count == 1)
            {
                // Un seul sous-segment, calculer directement
                return CalculateSegment(context, segment.SubSegments[0]);
            }
            else
            {
                // Plusieurs segments sans opérateurs = mode multi-parts (concaténation/addition)
                return EvaluateMultiParts(context, segment.SubSegments);
            }
        }


        /// <summary>
        /// Évalue une expression contenant des opérateurs (ex: a + b * c)
        /// </summary>
        private static object EvaluateExpressionWithOperators(FormulaContext context, List<FormulaSegmentModel> segments)
        {
            // Cette implémentation simplifiée traite les opérateurs de gauche à droite
            // TODO: Implémenter priorités des opérateurs (* / avant + -)
            
            if (segments.Count == 0) return null;

            // Calculer la première valeur
            var valueSegments = segments.Where(s => s.SegmentType != FormulaSegmentTypeEnum.Operator).ToList();
            var operatorSegments = segments.Where(s => s.SegmentType == FormulaSegmentTypeEnum.Operator).ToList();

            if (valueSegments.Count == 0) return null;

            object result = CalculateSegment(context, valueSegments[0]);

            // Appliquer les opérateurs séquentiellement
            for (int i = 0; i < operatorSegments.Count && i + 1 < valueSegments.Count; i++)
            {
                string op = operatorSegments[i].Text;
                object rightValue = CalculateSegment(context, valueSegments[i + 1]);
                result = ApplyOperator(op, result, rightValue);
            }

            return result;
        }


        /// <summary>
        /// Évalue plusieurs parties sans opérateurs (concaténation ou addition)
        /// </summary>
        private static object EvaluateMultiParts(FormulaContext context, List<FormulaSegmentModel> segments)
        {
            // Calculer toutes les valeurs
            var values = segments.Select(s => CalculateSegment(context, s)).ToArray();

            // Si toutes sont numériques → addition, sinon → concaténation
            bool allNumeric = values.All(v => Nglib.FORMAT.NumberTools.IsTypeNumeric(v));

            if (allNumeric)
            {
                // Addition
                double sum = 0;
                foreach (var val in values)
                {
                    if (val != null)
                        sum += Convert.ToDouble(val);
                }
                return sum;
            }
            else
            {
                // Concaténation
                StringBuilder sb = new StringBuilder();
                foreach (var val in values)
                {
                    if (val != null)
                        sb.Append(val.ToString());
                }
                return sb.ToString();
            }
        }


        /// <summary>
        /// Applique un opérateur entre deux valeurs
        /// </summary>
        private static object ApplyOperator(string op, object left, object right)
        {
            // Gestion NULL
            if (left == null || right == null)
            {
                if (op == "+" && (left != null || right != null))
                {
                    // Concaténation avec null
                    return (left?.ToString() ?? "") + (right?.ToString() ?? "");
                }
                return null;
            }

            // Détecter si opération numérique ou string
            bool isNumeric = Nglib.FORMAT.NumberTools.IsTypeNumeric(left) && 
                            Nglib.FORMAT.NumberTools.IsTypeNumeric(right);

            switch (op)
            {
                case "+":
                    if (isNumeric)
                        return Convert.ToDouble(left) + Convert.ToDouble(right);
                    else
                        return left.ToString() + right.ToString();

                case "-":
                    if (!isNumeric) throw new FormulaException(op, "Operator '-' requires numeric values");
                    return Convert.ToDouble(left) - Convert.ToDouble(right);

                case "*":
                    if (!isNumeric) throw new FormulaException(op, "Operator '*' requires numeric values");
                    return Convert.ToDouble(left) * Convert.ToDouble(right);

                case "/":
                    if (!isNumeric) throw new FormulaException(op, "Operator '/' requires numeric values");
                    double divisor = Convert.ToDouble(right);
                    if (divisor == 0) throw new FormulaException(op, "Division by zero");
                    return Convert.ToDouble(left) / divisor;

                case "==":
                case "=":
                    return left.ToString() == right.ToString();

                case "!=":
                case "<>":
                    return left.ToString() != right.ToString();

                case ">":
                    if (!isNumeric) return string.Compare(left.ToString(), right.ToString()) > 0;
                    return Convert.ToDouble(left) > Convert.ToDouble(right);

                case ">=":
                    if (!isNumeric) return string.Compare(left.ToString(), right.ToString()) >= 0;
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);

                case "<":
                    if (!isNumeric) return string.Compare(left.ToString(), right.ToString()) < 0;
                    return Convert.ToDouble(left) < Convert.ToDouble(right);

                case "<=":
                    if (!isNumeric) return string.Compare(left.ToString(), right.ToString()) <= 0;
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);

                case "&&":
                case "and":
                    return Convert.ToBoolean(left) && Convert.ToBoolean(right);

                case "||":
                case "or":
                    return Convert.ToBoolean(left) || Convert.ToBoolean(right);

                default:
                    throw new FormulaException(op, $"Unknown operator: {op}");
            }
        }


        /// <summary>
        /// Invoquer une fonction via réflexion
        /// </summary>
        private static object InvokeFunction(string functionName, object[] args)
        {
            InitDefaultsFunctions();

            // Rechercher la fonction
            var fcnFind = cacheFunctions.Keys.FirstOrDefault(x => functionName.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (fcnFind == null)
                throw new FormulaException(functionName, $"Function not found: {functionName}");

            // Vérifier nombre d'arguments requis
            if (fcnFind.ReqArgs > 0 && (args == null || args.Length < fcnFind.ReqArgs))
                throw new FormulaException(functionName, 
                    $"Insufficient arguments for function ({functionName}) (got:{args?.Length ?? 0}, required:{fcnFind.ReqArgs})");

            // Préparer les arguments selon le mode de la fonction
            object[] methodArgs = null;

            if (fcnFind.MethodMode == MethodModeEnum.StringArray)
            {
                // Convertir tous les arguments en string[]
                string[] strArgs = args?.Select(a => a?.ToString() ?? "").ToArray() ?? new string[0];
                methodArgs = new object[] { strArgs };
            }
            else if (fcnFind.MethodMode == MethodModeEnum.ObjectArray)
            {
                // Passer directement le tableau d'objets
                methodArgs = new object[] { args ?? new object[0] };
            }
            else if (fcnFind.MethodMode == MethodModeEnum.Context)
            {
                // Mode Context non supporté dans nouveau système
                throw new FormulaException(functionName, 
                    $"MethodMode.Context not supported in new segment system");
            }
            else
            {
                throw new Exception($"(dev) Unsupported MethodMode: {fcnFind.MethodMode}");
            }

            // Invoquer la méthode
            if (!cacheFunctions.TryGetValue(fcnFind, out var method))
                throw new FormulaException(functionName, $"Method implementation not found for function ({functionName})");

            object result = method.Invoke(null, methodArgs);

            // Corrections
            if (result != null && result == DBNull.Value) result = null;

            return result;
        }


        /// <summary>
        /// Essaie de parser une constante simple depuis le texte
        /// </summary>
        private static object TryParseConstant(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = text.Trim();

            // Nombre
            if (Nglib.FORMAT.NumberTools.IsNumeric(text))
            {
                if (int.TryParse(text, out int intVal))
                    return intVal;
                if (double.TryParse(text, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double dblVal))
                    return dblVal;
            }

            // String entre quotes
            if ((text.StartsWith("'") && text.EndsWith("'")) ||
                (text.StartsWith("\"") && text.EndsWith("\"")))
            {
                return text.Substring(1, text.Length - 2);
            }

            return null;
        }


        /// <summary>
        /// Petit factory avec les fonctions les plus utilisées (DEPRECATED - non utilisé dans nouveau système)
        /// </summary>
        [Obsolete("Not used in new segment system", true)]
        private static object CalcDirectFactory(string command, string[] args)
        {
            if (command.Equals("eq") && args.Length > 1) return FORMULA.FormulaTextFunctions.Eq(args);
            else if (command.Equals("or") && args.Length > 1) return FormulaOperatorFunctions.Or(args);
            else if (command.Equals("and") && args.Length > 1) return FormulaOperatorFunctions.And(args);
            else if (command.Equals("upper") && args.Length > 1) return FormulaTextFunctions.Upper(args);
            return null;
        }



        /// <summary>
        /// Chargement des fonctions principales avec protection thread-safe
        /// </summary>
        internal static void InitDefaultsFunctions()
        {
            if (isInit) return; // Double-check avant lock pour performance
            
            lock (initLock) // Protection thread-safe
            {
                if (isInit) return; // Double-check après lock
                
                // Chargement des functions les plus basiques
                LoadFunctions(typeof(FormulaOperatorFunctions));
                LoadFunctions(typeof(FormulaArrayFunctions));
                LoadFunctions(typeof(FormulaDateFunctions));
                LoadFunctions(typeof(FormulaNumericFunctions));
                LoadFunctions(typeof(FormulaTextFunctions));
                LoadFunctions(typeof(FormulaDatasFunctions));
                LoadFunctions(typeof(FormulaCryptoFunctions));


                ////Chargements des fonctions contenus dans la DLL supérieur si chargé (Docalad.Data.Dll)
                //Type FormulaCryptoFunctionsType = Nglib.APP.CODE.ReflectionTools.GetType("Docalad.COMPONENTS.FORMULA.FormulaCryptoFunctions");
                //if(FormulaCryptoFunctionsType!=null) LoadFunctions(FormulaCryptoFunctionsType);
                //Type FormulaDatasFunctionsType = Nglib.APP.CODE.ReflectionTools.GetType("Docalad.COMPONENTS.FORMULA.FormulaDatasFunctions");
                //if(FormulaDatasFunctionsType!=null) LoadFunctions(FormulaDatasFunctionsType);

                isInit = true; // Marqué comme initialisé à la fin
            }
        }


        /// <summary>
        /// Chargement de toutes les fonctions de toutes les DLL chargées
        /// Uniquement les classes qui se terminent pas Functions.cs.
        /// Lent prend 200ms
        /// </summary>
        public static void LoadFunctionsFullAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<FormulaAttribute, MethodInfo> list = new Dictionary<FormulaAttribute, MethodInfo>();
            var types = assemblies.SelectMany(dll => dll.GetTypes());
            types = types.Where(t => t.Name.EndsWith("functions", StringComparison.OrdinalIgnoreCase));
            types.ForEach(a => LoadFunctions(a));
        }



        /// <summary>
        /// Toutes les fonctions de toutes les DLL chargées
        /// Thread-safe via ConcurrentDictionary
        /// </summary>
        internal static int LoadFunctions(Type classFunctions)
        {
            if (classFunctions == null) return 0;
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                if (!classFunctions.IsClass) throw new Exception("Type must be a class");
                var methods = classFunctions.GetMethods();
                int addedCount = 0;

                // Recherche des fonctions par methodes
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttributes(typeof(FormulaAttribute), true).FirstOrDefault() as FormulaAttribute;
                    if (attr == null) continue;
                    if (string.IsNullOrEmpty(attr.Groupe)) attr.Groupe = classFunctions.Name; // permet de regroupper par classe
                    
                    // Ajout thread-safe avec TryAdd (ne remplace pas si existe déjà)
                    if (cacheFunctions.TryAdd(attr, method))
                        addedCount++;
                }

                stopwatch.Stop();
                return addedCount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Formula.LoadFunctions({classFunctions?.Name}) " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Toutes les fonctions disponbles dans l'applications
        /// </summary>
        public static List<FormulaAttribute> GetLoadedFunctions()
        {
            InitDefaultsFunctions();
            if (cacheFunctions == null) return new List<FormulaAttribute>();
            return cacheFunctions.Keys.ToList();
        }







    }



}
