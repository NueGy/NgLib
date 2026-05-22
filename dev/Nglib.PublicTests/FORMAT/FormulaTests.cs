using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.COLLECTIONS;
using Nglib.FORMAT;
using Nglib.FORMAT.FORMULA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
    [TestClass()]
    public class FormulaTests
    {
        /// <summary>
        /// Simple Exemple de formule
        /// </summary>
        [TestMethod, TestCategory("code")]
        public void SimpleFormulaTest()
        {
            string res = FormulaTools.Eval("'Nous somme le '+ day(now()) ");
            Assert.AreEqual(res, $"Nous somme le {DateTime.Now.Day}");

            string res2 = FormulaTools.EvalComposedString("Nous somme le {=day(now())}");
            Assert.AreEqual(res2, $"Nous somme le {DateTime.Now.Day}");
        }



        /// <summary>
        /// Test des formules
        /// </summary>
        [TestMethod, TestCategory("code")]
        public void FormulesTest()
        {
            // Forcer la DLL à se charger en appelant une de ses classes
            var ti = typeof(FormulaCryptoFunctions);
            FormulaTools.LoadFunctions(typeof(FormulaCryptoFunctions));
            FormulaTools.LoadFunctions(typeof(FormulaDatasFunctions));

            //Liste de formules et le résultat attendu
            var formules = GetFormulaTestsBasic(); // Utilise seulement les formules de base
            formules.AddRange(GetFormulaCompleteTests()); // Ajoute les formules complètes
            int NbFunctions = FormulaExecuteTools.GetLoadedFunctions().Count;
            Assert.IsTrue(NbFunctions > 0, "Aucune fonction chargée");
            var parameters = this.PrepareParameters();

            foreach (var item in formules)
            {
                try
                {
                    var res = FormulaTools.Eval(item.Key, parameters);
                    Assert.AreEqual(item.Value, res, $"Formule:{item.Key}  CALC:{res}!={item.Value}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Erreur sur la formule: {item.Key}\nErreur: {ex.Message}");
                }
            }
            Console.WriteLine($"Test ok de {formules.Count} formules avec {NbFunctions} fonctions chargées.");
        }

        // Tests de base sans data() et param() dépréciés
        private static Dictionary<string, string> GetFormulaTestsBasic()
        {
            Dictionary<string, string> formules = new Dictionary<string, string>();
            formules.Add("@MaChaine", "HelloWorld!!!");
            formules.Add("@MonNombre+4", "123460");
            formules.Add("'AB'+'CD'", "ABCD");
            formules.Add("4+5", "9");
            formules.Add("'4'+5", "45");
            formules.Add("'4'+'5'", "45");
            
            // COMPARAISONS
            formules.Add("eq(\"99999\", \"99999\")", "1");
            formules.Add("eq(\"77777\", \"99999\")", "0");
            formules.Add("equal(\"test\", \"test\")", "1");
            formules.Add("gt(10, 5)", "1");
            formules.Add("lt(5, 10)", "1");
            formules.Add("gteq(10, 10)", "1");
            formules.Add("lteq(5, 5)", "1");
            
            // DATES
            formules.Add("todate(now(),'dd/MM/yyyy HH') + '78' + 'C'", DateTime.Now.ToString("dd/MM/yyyy HH") + "78C");
            formules.Add("year(now())", DateTime.Now.Year.ToString());
            formules.Add("month(now())", DateTime.Now.Month.ToString());
            formules.Add("day(now())", DateTime.Now.Day.ToString());
            formules.Add("addday('01/01/2020', 10)", "11/01/2020 00:00:00");
            formules.Add("addmonth('01/01/2020', 2)", "01/03/2020 00:00:00");
            formules.Add("addyear('01/01/2020', 1)", "01/01/2021 00:00:00");
            formules.Add("dateabs('15/06/2020 14:30:00')", "15/06/2020");
            
            // PARAMETRES DIRECTS
            formules.Add("@MonNombre", "123456");
            formules.Add("@MonPo.Nom", "Toto");
            formules.Add("@MonPo.Age+2", "27");
            formules.Add("todate(@MonPo.DateLecture,'dd/MM/yyyy')", "06/10/2020");
            
            // TEXTE
            formules.Add("upper('hello')", "HELLO");
            formules.Add("lower('WORLD')", "world");
            formules.Add("left('Hello', 3)", "Hel");
            formules.Add("right('World', 3)", "rld");
            formules.Add("len('Test')", "4");
            formules.Add("upper(@MaChaine)", "HELLOWORLD!!!");
            formules.Add("trim('  hello  ')", "hello");
            formules.Add("ltrim('  hello')", "hello");
            formules.Add("rtrim('hello  ')", "hello");
            formules.Add("replace('Hello World', 'World', 'Test')", "Hello Test");
            formules.Add("concat('Hello', ' ', 'World')", "Hello World");
            formules.Add("Contains('HelloWorld', 'World')", "1");
            formules.Add("Contains('HelloWorld', 'xyz')", "0");
            formules.Add("StartsWith('HelloWorld', 'Hello')", "1");
            formules.Add("EndsWith('HelloWorld', 'World')", "1");
            formules.Add("strindex('HelloWorld', 'World')", "5");
            formules.Add("Substring('HelloWorld', 5, 5)", "World");
            formules.Add("PadLeft('42', 5, '0')", "00042");
            formules.Add("PadRight('42', 5, '0')", "42000");
            
            // MATHEMATIQUES
            formules.Add("abs(-5)", "5");
            formules.Add("mod(10, 3)", "1");
            formules.Add("add(5, 3)", "8");
            formules.Add("sub(10, 3)", "7");
            formules.Add("mul(4, 3)", "12");
            formules.Add("div(15, 3)", "5");
            formules.Add("min(5, 3, 8)", "3");
            formules.Add("max(5, 3, 8)", "8");
            formules.Add("div(314159, 100000)", "3,14159");
            formules.Add("ceil(div(314159, 100000), 2)", "4");
            formules.Add("Floor(div(314159, 100000), 2)", "3");
            formules.Add("round(div(314159, 100000), 2)", "3,14");
            formules.Add("round(div(314159, 100000), 3)", "3,142");
            formules.Add("round(div(314159, 100000), 1)", "3,1");
            formules.Add("round(div(7, 2), 0)", "4");
 
            formules.Add("floor(div(38, 10))", "3");
            //formules.Add("floor(mul(-1, div(23, 10)))", "-3");
            formules.Add("ceil(div(32, 10))", "4");
            //formules.Add("ceil(mul(-1, div(27, 10)))", "-3");
            formules.Add("sum(5, 10, 15)", "30");
            formules.Add("sum(2, 3)", "5");
            formules.Add("avg(10, 20, 30)", "20");
            formules.Add("avg(5, 15)", "10");
            
            // LOGIQUE
            formules.Add("and('1', '1')", "1");
            formules.Add("and('1', '0')", "0");
            formules.Add("or('0', '1')", "1");
            formules.Add("or('0', '0')", "0");
            formules.Add("not('1')", "0");
            formules.Add("not('0')", "1");
            formules.Add("if(eq('test', 'test'), 'Vrai', 'Faux')", "Vrai");
            formules.Add("if(gt(10, 5), 'OK', 'NOK')", "OK");
            
            // CONVERSION
            formules.Add("ToDouble('123')", "123");
            formules.Add("ToInt('456')", "456");
            formules.Add("IsNumeric('123')", "1");
            formules.Add("IsNumeric('abc')", "0");
            formules.Add("ToDecimal('789')", "789");

            // CONVERSION - chaines formatees (espaces, virgules, points)
            // TODO: ces valeurs attendues dependent de la culture systeme (fr-FR => virgule).
            // Pour etre portable, il faudrait fixer les formules avec InvariantCulture.
            formules.Add("ToDouble('1 234,56')", "1234,56");
            formules.Add("ToDouble('1.234,56')", "1234,56");
            formules.Add("ToDouble('1,234.56')", "1234,56");
            formules.Add("ToInt('1 234')", "1234");
            formules.Add("ToDecimal('1 234,56')", "1234,56");
            formules.Add("IsNumeric('1 234,56')", "1");

            // FILTRES CHAINES
            formules.Add("OnlyAlphanumeric('abc 123!')", "abc123");
            formules.Add("OnlyNumeric('Year: 2024')", "2024");

            // MONTANTS EN CENTIMES (amtct)
            formules.Add("amtct('1586.45')", "158645");
            formules.Add("amtct('1 586,45')", "158645");
            formules.Add("amtct('1.586,45')", "158645");
            formules.Add("amtct('0.50')", "50");
            formules.Add("amtct('100')", "10000");

            // PARAMETRES CASSE INSENSIBLE
            formules.Add("@MACHAINE", "HelloWorld!!!");
            formules.Add("@machaine", "HelloWorld!!!");
            
            // FONCTIONS COMPOSEES
            formules.Add("upper(left(@MaChaine, 5))", "HELLO");
            formules.Add("add(mul(3, 4), 2)", "14");
            formules.Add("upper(@MonPo.Nom)", "TOTO");
            formules.Add("Between(15, 10, 20)", "1");
            formules.Add("Between(5, 10, 20)", "0");
            
            // CONCATENATIONS
            formules.Add("upper('hello') + ' ' + lower('WORLD')", "HELLO world");
            formules.Add("left(@MaChaine, 5) + right(@MaChaine, 3)", "Hello!!!");
            formules.Add("@MonPo.Nom + ' ' + @MonPo.Nom", "Toto Toto");
            formules.Add("'[' + @MonNombre + ']'", "[123456]");
            formules.Add("'Age: ' + @MonPo.Age", "Age: 25");
            formules.Add("@MonPo.Nom + ' a ' + @MonPo.Age + ' ans'", "Toto a 25 ans");
            
            // CALCULS COMPLEXES
            formules.Add("add(mul(add(2, 3), sub(10, 2)), div(100, 4))", "65");
            formules.Add("mul(add(abs(-5), abs(-3)), sub(max(10, 15, 20), min(2, 5, 8)))", "144");
            
            return formules;
        }




        private Dictionary<string, object> PrepareParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("MaChaine", "HelloWorld!!!");
            parameters.Add("MonNombre", 123456);
            parameters.Add("MonChar", new { Nom = "Toto", Age = 25 });
            var monpo = new Nglib.DATA.DATAPO.DataPO();
            monpo["Nom"] = "Toto";
            monpo["Age"] = 25;
            monpo["DateLecture"] = new DateTime(2020, 10, 06);
            var poflux = monpo.GetOrDefineFlow<Nglib.DATA.KEYVALUES.KeyValuesPOFlow>("flux", Nglib.DATA.ACCESSORS.FlowTypeEnum.JSON);
            poflux["numero4"] = "1234";
            parameters.Add("MonPo", monpo);

            return parameters;

        }


        // liste de formules et le résultat attendu
        private static Dictionary<string, string> GetFormulaCompleteTests()
        {
            Dictionary<string, string> formules = new Dictionary<string, string>();
           
            // === FONCTIONS COMPOSEES ===
            formules.Add("upper(left(@MaChaine, 5))", "HELLO");
            formules.Add("add(mul(3, 4), 2)", "14");
            // formules.Add("upper(data('@MonPo','Nom'))", "TOTO"); // data() deprecated
            formules.Add("Between(15, 10, 20)", "1");
            formules.Add("Between(5, 10, 20)", "0");
            
            // === TESTS COMPLEXES AVEC IMBRICATIONS MULTIPLES ===
            // Combinaisons texte + math + logique (3-4 niveaux)
            formules.Add("if(gt(len(@MaChaine), 10), upper(left(@MaChaine, 5)), lower(right(@MaChaine, 3)))", "HELLO");
            formules.Add("if(lt(@MonPo.Age, 18), 'Mineur', if(lt(@MonPo.Age, 65), 'Adulte', 'Senior'))", "Adulte");
            formules.Add("add(mul(@MonPo.Age, 2), div(120000, 1000))", "170");
            
            // Conditions imbriquées avec calculs
            formules.Add("if(and(gt(@MonPo.Age, 18), lt(@MonPo.Age, 60)), add(@MonPo.Age, 5), sub(@MonPo.Age, 5))", "30");
            formules.Add("if(or(eq(upper(@MonPo.Nom), 'TOTO'), eq(@MonPo.Age, 25)), 'Match', 'NoMatch')", "Match");
            formules.Add("if(Between(len(@MaChaine), 10, 20), upper(@MaChaine), lower(@MaChaine))", "HELLOWORLD!!!");
            
            // === CONCATENATIONS ENTRE FORMULES ===
            // Concaténations simples avec fonctions texte uniquement
            formules.Add("upper('hello') + ' ' + lower('WORLD')", "HELLO world");
            formules.Add("left(@MaChaine, 5) + right(@MaChaine, 3)", "Hello!!!");
            formules.Add("@MonPo.Nom + ' ' + @MonPo.Nom", "Toto Toto");
            
            // Concaténations mixtes (texte + nombre = concaténation)
            formules.Add("'[' + @MonNombre + ']'", "[123456]");
            formules.Add("'Age: ' + @MonPo.Age", "Age: 25");
            formules.Add("@MonPo.Nom + ' a ' + @MonPo.Age + ' ans'", "Toto a 25 ans");
            formules.Add("'Result: ' + add(5, 3)", "Result: 8");
            formules.Add("'Total: ' + mul(10, 5) + ' euros'", "Total: 50 euros");
            formules.Add("upper(@MonPo.Nom) + ' - Age: ' + @MonPo.Age", "TOTO - Age: 25");
            

            // Concaténations avec conditions (retournent des chaînes)
            formules.Add("if(gt(5, 3), 'A', 'B') + if(lt(2, 4), 'C', 'D')", "AC");
            //formules.Add("'Status: ' + if(gteq(data('@MonPo','Age'), 18), 'Majeur', 'Mineur')", "Status: Majeur");
            formules.Add("upper(left(@MaChaine, 5)) + ' ' + lower(right(@MaChaine, 5))", "HELLO ld!!!"); // Correction: right 5 chars de "HelloWorld!!!" = "ld!!!"
            
            // Concaténations complexes imbriquées
            formules.Add("'Bonjour ' + upper(@MonPo.Nom) + ', vous avez ' + @MonPo.Age + ' ans!'", "Bonjour TOTO, vous avez 25 ans!");
            formules.Add("left(@MaChaine, 5) + ' - ' + if(gt(len(@MaChaine), 10), 'Long', 'Court') + ' - ' + right(@MaChaine, 3)", "Hello - Long - !!!");
            formules.Add("'[' + upper(left(@MonPo.Nom, 2)) + '] Age: ' + @MonPo.Age", "[TO] Age: 25");
            formules.Add("if(IsNumeric('123'), 'NUM:', 'TXT:') + '123'", "NUM:123");
            
            // Concaténations avec dates
            formules.Add("'Année: ' + year(now())", "Année: " + DateTime.Now.Year.ToString());
            formules.Add("day(now()) + '/' + month(now()) + '/' + year(now())", DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year);
            formules.Add("'Date: ' + todate(@MonPo.DateLecture,'dd/MM/yyyy')", "Date: 06/10/2020");
            formules.Add("upper(@MonPo.Nom) + ' - ' + todate(@MonPo.DateLecture,'yyyy')", "TOTO - 2020");
            
            // Calculs mathématiques complexes imbriqués (4-5 niveaux)
            formules.Add("add(mul(add(2, 3), sub(10, 2)), div(100, 4))", "65");
            formules.Add("mul(add(abs(-5), abs(-3)), sub(max(10, 15, 20), min(2, 5, 8)))", "144");
            formules.Add("ceil(div(add(mul(4, 5), mul(3, 2)), sub(10, 4)))", "5"); // (20+6)/6 = 26/6 = 4.333... arrondi au dessus = 5
            formules.Add("sum(mul(2, 3), add(4, 5), sub(20, 10))", "25");
            
            // Manipulation de texte complexe avec conditions
            formules.Add("upper(if(gt(len(@MaChaine), 5), left(@MaChaine, 8), right(@MaChaine, 3)))", "HELLOWOR");
            formules.Add("if(eq(lower(left(@MaChaine, 5)), 'hello'), len(upper(@MaChaine)), len(lower(@MaChaine)))", "13");
            formules.Add("left(upper(@MonPo.Nom), add(1, 2))", "TOT");
            
            // Logique booléenne complexe (3-4 niveaux)
            formules.Add("and(gt(len(@MaChaine), 10), and(lt(@MonPo.Age, 30), eq(upper(@MonPo.Nom), 'TOTO')))", "1");
            formules.Add("or(and(gt(10, 5), lt(3, 8)), and(eq('a', 'b'), eq('c', 'c')))", "1");
            formules.Add("not(or(and(eq('test', 'test'), gt(5, 10)), and(lt(20, 10), eq('x', 'y'))))", "1");
            
            // Dates avec conditions et calculs
            formules.Add("if(gteq(year(now()), 2020), add(year(now()), 5), sub(year(now()), 5))", (DateTime.Now.Year + 5).ToString());
            formules.Add("add(mul(day(now()), 2), month(now()))", (DateTime.Now.Day * 2 + DateTime.Now.Month).ToString());
            formules.Add("if(eq(month(@MonPo.DateLecture), 10), day(@MonPo.DateLecture), 0)", "6");
            
            // Conversion avec imbrications
            formules.Add("if(IsNumeric(@MonNombre), add(ToInt(@MonNombre), 1000), 0)", "124456");
            formules.Add("mul(ToInt(if(gt(5, 3), '10', '5')), add(ToInt('2'), ToInt('3')))", "50");
            formules.Add("ToDouble(add(ToInt('100'), mul(ToInt('5'), ToInt('4'))))", "120");
            
            // Combinaisons extrêmes (5+ niveaux d'imbrication)
            formules.Add("if(gt(len(upper(left(@MaChaine, 10))), 5), add(mul(2, 5), div(20, 4)), sub(100, 50))", "15");
            formules.Add("upper(left(if(gt(@MonPo.Age, 20), @MonPo.Nom, 'Unknown'), len(@MonPo.Nom)))", "TOTO");
            formules.Add("add(mul(ToInt(if(IsNumeric('5'), '5', '0')), 3), div(ToInt(if(IsNumeric('20'), '20', '0')), 4))", "20");
            
            // Tests Between avec calculs imbriqués
            formules.Add("if(Between(add(10, 5), 12, 20), 'InRange', 'OutRange')", "InRange");
            formules.Add("Between(len(upper(@MaChaine)), add(5, 5), mul(10, 2))", "1");
            formules.Add("if(Between(@MonPo.Age, sub(30, 10), add(20, 10)), upper('ok'), lower('NOK'))", "OK");
            
            return formules;
        }





    }
}