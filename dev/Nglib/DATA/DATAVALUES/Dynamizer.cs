// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.DATAVALUES
{





    //public class Dynamizer
    //{
    //    public string UniqueName
    //    {
    //        get { return _UniqueName; }
    //        set { _UniqueName = value.ToLower(); }
    //    }

    //    private string _UniqueName = null;

    //    public Dynamizer(string UniqueMenuName)
    //    {
    //        this.UniqueName = UniqueMenuName;
    //    }

    //    public virtual DATAVALUES.DataValues_data calcul(DATAVALUES.DataValues_data element)
    //    {

    //        return element;
    //    }





    //    /// <summary>
    //    /// Dynamisation rapide d'un string (fonctions de bases)
    //    /// SAFE (retourne toujours un string)
    //    /// </summary>
    //    /// <returns></returns>
    //    public static string simpledynamisme(string origine, List<Dynamizer> dynamizers = null)
    //    {
    //        try
    //        {
    //            List<DATAVALUES.DataValues_data> tel = getelementdynamic(origine);
    //            tel = calculmenuList(tel, dynamizers);
    //            string retour = setelementdynamic(origine, tel);
    //            return retour;
    //        }
    //        catch (Exception)
    //        {
    //            return origine;
    //        }
    //    }

    //    /// <summary>
    //    /// DECOUPE des fonctions dynamique dans la chaine
    //    /// Format {!menu|param1|param2|paramN}
    //    /// </summary>
    //    /// <param name="chaineorigin"></param>
    //    /// <returns></returns>
    //    public static List<DATAVALUES.DataValues_data> getelementdynamic(string chaineorigin)
    //    {
    //        List<DATAVALUES.DataValues_data> retour = new List<DATAVALUES.DataValues_data>();


    //        int positionchaine = 0;

    //        for (int iteration = 0; iteration < 99; iteration++) //on limite à 99 éléments dynamique pour éviter les boucles folles
    //        {
    //            try
    //            {

    //                int positiondynstart = chaineorigin.IndexOf("{!", positionchaine);
    //                if (positiondynstart < 0) break; // rien trouvé
    //                int positiondynstop = chaineorigin.IndexOf("}", positiondynstart);
    //                positionchaine = positiondynstop; // permet de passer à la suite
    //                if (positiondynstop < positiondynstart) throw new Exception("erreur dans les découpes");
    //                int positiondyncount = positiondynstop - positiondynstart;
    //                if (positiondyncount < 5 || positiondyncount > 99) throw new Exception("erreur dans les découpes (chaine dynamique trop grande ou trop petite)");
    //                string subchainedyn = chaineorigin.Substring(positiondynstart, positiondyncount + 1);
    //                if (subchainedyn.IndexOf("{", 2) > 1) throw new Exception("le caractere { est interdit dans une chaine dynamique");
    //                DATAVALUES.DataValues_data elementdyn = new DATAVALUES.DataValues_data();
    //                elementdyn.name = "element" + (retour.Count + 1).ToString(); //affecte un nom noeud sans importance
    //                elementdyn.value = subchainedyn;

    //                string contenu = subchainedyn.Substring(2, subchainedyn.Length - 3);
    //                elementdyn["origine"] = subchainedyn;
    //                // découpe des parties
    //                string[] parties = contenu.Split('|');

    //                if (parties[0].Contains(':'))
    //                {
    //                    contenu = contenu.Replace(':', '|');
    //                    parties = contenu.Split('|');
    //                    //throw new Exception("!Chaine dynamiques Version obsolete, vous devez remplacer les separateurs : en |");
    //                }


    //                elementdyn["menu"] = parties[0];
    //                for (int iparties = 0; iparties < parties.Length; iparties++)
    //                {
    //                    elementdyn["partie" + iparties.ToString()] = parties[iparties];
    //                }
    //                retour.Add(elementdyn);
    //            }
    //            catch (Exception e)
    //            {
    //                if (e.Message[0] == '!') throw e;
    //                continue; // en cas d'erreur on continue
    //            }
    //        }
    //        return retour;
    //    }

    //    /// <summary>
    //    /// RECONCATENE le résultat des fonctions dynamiques dans la chaine
    //    /// </summary>
    //    public static string setelementdynamic(string chaineorigin, List<DATAVALUES.DataValues_data> elements)
    //    {
    //        string retour = chaineorigin;
    //        foreach (DATAVALUES.DataValues_data itemr in elements)
    //        {
    //            string valeur = itemr.value.ToString();
    //            if (valeur.Contains("{!")) valeur = ""; // on ne recopie pas une chaine dynamique
    //            retour = retour.Replace(itemr["origine"], valeur);
    //        }
    //        return retour;
    //    }



    //    /// <summary>
    //    /// CALCUL une liste
    //    /// </summary>
    //    /// <param name="elements"></param>
    //    /// <returns></returns>
    //    public static List<DATAVALUES.DataValues_data> calculmenuList(List<DATAVALUES.DataValues_data> elements, List<Dynamizer> dynamizers = null)
    //    {
    //        foreach (DATAVALUES.DataValues_data element in elements)
    //        {
    //            try
    //            {
    //                calculsimplemenu(element);
    //                if (dynamizers != null) foreach (Dynamizer dyn in dynamizers) dyn.calcul(element);
    //            }
    //            catch (Exception)
    //            {
    //                continue;
    //            }
    //        }
    //        return elements;
    //    }


    //    /// <summary>
    //    /// Fonction de calculs dynamiques de bases
    //    /// </summary>
    //    /// <param name="element"></param>
    //    /// <returns></returns>
    //    public static DATAVALUES.DataValues_data calculsimplemenu(DATAVALUES.DataValues_data element)
    //    {
    //        if (element["menu"] == "datenow")
    //        {
    //            if (element["partie1"] != "") element.value = DateTime.Now.ToString(element["partie1"]);
    //            else element.value = DateTime.Now.ToString();
    //        }
    //        else if (element["menu"] == "datenowadd")
    //        {
    //            if (element["partie1"] != "") element.value = DateTime.Now.AddSeconds(Convert.ToInt32(element["partie2"])).ToString(element["partie1"]);
    //            else element.value = DateTime.Now.AddSeconds(Convert.ToInt32(element["partie2"])).ToString();
    //        }
    //        else if (element["menu"] == "datenowaddjour" || element["menu"] == "datenowaddday") // {!datenowaddday|dd/MM/yyyy|-7}
    //        {
    //            if (element["partie1"] != "") element.value = DateTime.Now.AddDays(Convert.ToInt32(element["partie2"])).ToString(element["partie1"]);
    //            else element.value = DateTime.Now.AddDays(Convert.ToInt32(element["partie2"])).ToString();
    //        }
    //            /*
    //        else if (element["menu"] == "datenowaddjourouvrable")
    //        {
    //            // !!! implémenter avec le webservice
    //            DateTime actdat = SeyesLib3.GLOBAL.INFORMATIONS.DIVERS.Calendrier.nextJourOuvrable(DateTime.Now, Convert.ToInt32(element["partie2"])).datetraitement;
    //            if (element["partie1"] != "") element.value = actdat.ToString(element["partie1"]);
    //            else element.value = actdat.ToString();
    //        }
    //        else if (element["menu"] == "env")
    //        {
    //            GLOBAL.GLOBALPARAM.EnvWork paramdyn = new GLOBAL.GLOBALPARAM.EnvWork(element["partie1"]);
    //            element.value = paramdyn.getstring(element["partie2"]);
    //        }
    //        else if (element["menu"] == "globalparam")
    //        {
    //            GLOBAL.GLOBALPARAM.Param paramg = new GLOBAL.GLOBALPARAM.Param(element["partie1"]);
    //            element.value = paramg.flux.getstring(element["partie2"]);
    //        }*/
    //        else if (element["menu"] == "getonefile")
    //        {
    //            string repertoire = element["partie1"];
    //            string filtre = element["partie2"];
    //            //!!!
    //        }

    //        return element;
    //    }

    //    /// <summary>
    //    /// Pour le formatage d'un string dans une dynamisation
    //    /// Utilisé sur certaines fonctions.
    //    /// </summary>
    //    /// <param name="element"></param>
    //    /// <returns></returns>
    //    public static string ToolsDynamisefroString(DATAVALUES.DataValues_data element, string originalfind)
    //    {
    //        try
    //        {
    //            if (originalfind == null || originalfind == "" || element["partie2"] == "") return originalfind;
    //            if (element["partie2"] == "date" && element["partie3"] != "") //reformate la date
    //            {
    //                return Convert.ToDateTime(originalfind).ToString(element["partie3"]); // ajouter aussi l'ajout
    //            }
    //            else if (element["partie2"] == "filename")
    //            {
    //                return DATA.FORMAT.StringUtilities.FormatNameFile(originalfind);
    //            }

    //                /*
    //            else if (element["partie2"] == "filefrompath")
    //            {
    //                return SeyesLib3.GENERAL.FILES.Tools.getfilefrompath(originalfind);
    //            }*/
    //            // !!! faire aussi regex
    //        }
    //        catch (Exception) { }
    //        return originalfind;

    //    }








    //}


}
