﻿// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.DATA.PARAMVALUES
{
    public static class ParamValuesTools
    {


        /// <summary>
        /// Exporter un flux du datavalue
        /// </summary>
        /// <param name="datavalue"></param>
        /// <param name="Xml">true=xml, false=json</param>
        /// <returns></returns>
        public static string ToFlux(this ParamValues datavalue, bool isXml = true)
        {
            IParamValuesSerializer serial = ParamValuesTools.SerializerFactory(isXml);
            return serial.Serialize(datavalue);
        }



        /// <summary>
        /// Chargement d'un flux(Json/Ou XML) dans le datavalue
        /// </summary>
        /// <param name="itemdv"></param>
        /// <param name="flux"></param>
        public static void FromFlux(this ParamValues datavalue, string flux, bool? isXml = null)
        {
            //clean
            if (string.IsNullOrWhiteSpace(flux)) return;
            flux = flux.Trim();

            if (!isXml.HasValue)
            {
                isXml = flux.TrimStart().StartsWith("<");
            }


            IParamValuesSerializer serial = ParamValuesTools.SerializerFactory(isXml.Value);
            serial.DeSerialize(flux, datavalue);

            //parse
            //if (flux.StartsWith("{")) this.FromFluxJson(flux);
            //if (flux.StartsWith("<")) fromFluxXML(datavalue,flux);
        }



        private static Type DatavaluesSerializerJsonType { get; set; }

        public static IParamValuesSerializer SerializerFactory(bool isxml)
        {
            IParamValuesSerializer retour = null;
            if (isxml)
            {
                retour = new ParamValuesSerializerXml(); 
            }
            else
            {
                retour = new ParamValuesSerializerJson();
                //if (ParamValuesTools.DatavaluesSerializerJsonType == null) // la fonction ne peus pas être incluse dans le code car nécessite d'autre DLL (attendre .netcore 3 pour simplifier cette étape)
                //    ParamValuesTools.DatavaluesSerializerJsonType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("Nglib.DATA..ParamValuesSerializerJson, Nglib.Extras");

                ////if (DataValuesTools.DatavaluesSerializerJsonType == null) // on prend un json dérivé
                ////    DataValuesTools.DatavaluesSerializerJsonType = typeof(DATAVALUES.DatavamueSerializerJsonLite);

                //if (ParamValuesTools.DatavaluesSerializerJsonType == null) 
                //    throw new Exception("DataValueSerializer For JSON not found. Please import Nglib.Extras.Dll AND Newtonsoft.Json");


                //retour = CODE.REFLEXION.ReflexionTools.NewInstance<IParamValuesSerializer>(ParamValuesTools.DatavaluesSerializerJsonType);
                //if (retour == null) throw new Exception("DatavaluesSerializerJsonType NewInstance error");
            }
            return retour;
        }



        /*
        /// <summary>
        /// utilisera
        /// </summary>
        /// <param name="origine"></param>
        /// <param name="ligne"></param>
        public static void PopulateFromBdd(GENERAL.DATAVALUES.DataValues origine, System.Data.DataRow ligne, string attributflag = "bdd")
        {
            try
            {
                foreach (GENERAL.DATAVALUES.DataValues_data item in origine.liste)
                {
                    if (item[attributflag] != "")
                    {
                        item.value = SeyesLib3.DATA.CONNECTOR.BaseBDD.getrowobject(ligne, item[attributflag]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("populate row " + attributflag + " Error " + ex.Message);
            }
        }


        */





        public static ParamValuesNodeHierarchical GetHierarchicalNodes(List<ParamValuesNode> nodes, string subNodePath=null)
        {
            if (subNodePath == null) subNodePath = "/param/";
            subNodePath = subNodePath.ToLower();

            ParamValuesNodeHierarchical firstnode = new ParamValuesNodeHierarchical() { NodePath = subNodePath };
            firstnode.NodeName = firstnode.NodePath.TrimEnd('/').Substring(firstnode.NodePath.TrimEnd('/').LastIndexOf('/') + 1);

            // Tous les noeuds inférieurs
            List<ParamValuesNode> subnodes = nodes.Where(n => n.Name.StartsWith(subNodePath)).ToList();

            // Si c'est un noeud final qui contient la valeur
            firstnode.ValueNode = subnodes.FirstOrDefault(n => subNodePath.Equals(n.Name));
            if (firstnode.ValueNode!=null) return firstnode; // on authorise pas des sous noeuds si il y as une valeur

            // Obtenir la premiere partie du chemin des autres subnodes
            Dictionary<string, List<ParamValuesNode>> othernodes = new Dictionary<string, List<ParamValuesNode>>();
            foreach (ParamValuesNode subnode in subnodes)
            {
                string nextnodepath = subnode.Name.Substring(subNodePath.Length);
                int nextbar = nextnodepath.IndexOf('/');
                if (nextbar <1) nextnodepath = subNodePath + nextnodepath; // on ne met pas le slash car il s'agit d'une valeur finale
                else nextnodepath = subNodePath + nextnodepath.Substring(0, nextbar) + '/'; // on remet toujours le slash
                if (!othernodes.ContainsKey(nextnodepath)) othernodes.Add(nextnodepath, new List<ParamValuesNode>());
                othernodes[nextnodepath].Add(subnode);
            }

            foreach (var item in othernodes)
            {
                var child = GetHierarchicalNodes(item.Value, item.Key); // RECURSIF
                if(child!=null)
                    firstnode.ChildrenNodes.Add(child);
            }


            return firstnode;
        }

        private static ParamValuesNodeHierarchical GetHierarchicalNodesSub(List<ParamValuesNode> nodes)
        {
            ParamValuesNodeHierarchical firstnode = new ParamValuesNodeHierarchical() { NodePath = "/param" };




            return firstnode;
        }


    }







}
