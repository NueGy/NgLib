using Nglib.DATA.ACCESSORS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.DATAPO
{
    /// <summary>
    /// Flux xml ou json 
    /// </summary>
    public interface IDataPOFlow 
    {
        /// <summary>
        /// Obtien le nom du champ à modifier dans la base
        /// </summary>
        /// <returns></returns>
        string GetFieldName();


        /// <summary>
        /// Obtient le type du champs dans la base
        /// </summary>
        /// <returns></returns>
        FlowTypeEnum GetFieldType();


        /// <summary>
        /// Si le champ à été entierement encrypté en base
        /// </summary>
        /// <returns></returns>
        bool IsFieldEncrypted();


        /// <summary>
        /// Définir le flux
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="EncryptedKey"></param>
        void DefineField(string fieldName, DATA.ACCESSORS.FlowTypeEnum fieldType, bool isFullEncrypted=false);



        /// <summary>
        /// Transforme toutes les données local pour les mettre à jours dans la base
        /// </summary>
        /// <returns></returns>
        string SerializeField();


        /// <summary>
        /// resynchronise les données provenant de la base dans le flow
        /// </summary>
        /// <param name="dataField"></param>
        void DeSerializeField(string dataField);


        /// <summary>
        /// Savoir si il y as eu des modifications parmi les objets (et pourrai nécessiter une mise à jour)
        /// </summary>
        /// <returns></returns>
        bool IsChanges();

        /// <summary>
        /// Signale que toute les modification ont été traité,
        /// </summary>
        /// <returns></returns>
        bool AcceptChanges();


    }
}
