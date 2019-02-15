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
    public static class DataValuesTools
    {


        /// <summary>
        /// Exporter un flux du datavalue
        /// </summary>
        /// <param name="datavalue"></param>
        /// <param name="Xml">true=xml, false=json</param>
        /// <returns></returns>
        public static string ToFlux(this DataValues datavalue, bool Xml = true)
        {
            DatavaluesSerializer serial = new DatavaluesSerializer(datavalue);
            return serial.tofluxXML();
            //return tofluxXML(datavalue);
            return null;
        }



        /// <summary>
        /// Chargement d'un flux(Json/Ou XML) dans le datavalue
        /// </summary>
        /// <param name="itemdv"></param>
        /// <param name="flux"></param>
        public static void FromFlux(this DataValues datavalue, string flux)
        {
            //clean
            if (string.IsNullOrWhiteSpace(flux)) return;
            flux = flux.Trim();

            DatavaluesSerializer serial = new DatavaluesSerializer(datavalue);
            serial.fromFluxXML(flux);

            //parse
            //if (flux.StartsWith("{")) this.FromFluxJson(flux);
            //if (flux.StartsWith("<")) fromFluxXML(datavalue,flux);
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







    }





}
