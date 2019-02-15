//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Nglib.BASICS.ACCESSORS
//{

//    // -----------------------------------------------------------------------------------

//    /// <summary>
//    /// OBSOLETE (Use IDataAccessor and DataAccessorExtentions)
//    /// </summary>
//    [Obsolete("Use IDataAccessor and DataAccessorExtentions")]
//    public abstract class DataAccessorBase : IDataAccessor
//    {


//        /// <summary>
//        /// Obtenir l'objet de base (METHODE PRINCIPALE)
//        /// </summary>
//        /// <param name="nameValue"></param>
//        /// <param name="AccesOptions"></param>
//        /// <returns></returns>
//        public virtual object GetObject(string nameValue, DataAccessorOptionEnum AccesOptions)
//        {
//            throw new NotImplementedException();
//        }


//        /// <summary>
//        /// Définir l'objet de base  (METHODE PRINCIPALE)
//        /// </summary>
//        /// <param name="nameValue"></param>
//        /// <param name="obj"></param>
//        /// <param name="AccesOptions"></param>
//        /// <returns></returns>
//        public virtual bool SetObject(string nameValue, object obj, DataAccessorOptionEnum AccesOptions)
//        {
//            throw new NotImplementedException();
//        }














//        public virtual string GetString(string nameValue)
//        {
//            return GetString(nameValue, DataAccessorOptionEnum.None);
//        }

//        public virtual string GetString(string nameValue, DataAccessorOptionEnum AccesOptions)
//        {
//            return DataAccessorExtentions.GetString(this, nameValue, AccesOptions);
//        }




//        public virtual int GetInt(string nameValue)
//        {
//            return this.GetInt(nameValue, 0);
//        }
//        public virtual int GetInt(string nameValue, int DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
//        {
//            return DataAccessorExtentions.GetInt(this, nameValue, DefaultValue, AccesOptions);
//        }
//        public virtual int? GetInt(string nameValue, DataAccessorOptionEnum AccesOptions, bool dbo)
//        {
//            return DataAccessorExtentions.GetInt(this, nameValue, AccesOptions, null);
//        }



//        public virtual DateTime GetDateTime(string nameValue)
//        {
//            return this.GetDateTime(nameValue, new DateTime());
//        }

//        public virtual DateTime GetDateTime(string nameValue, DateTime DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
//        {
//            return DataAccessorExtentions.GetDateTime(this, nameValue, DefaultValue, AccesOptions);
//        }
//        public virtual DateTime? GetDateTime(string nameValue, DataAccessorOptionEnum AccesOptions, bool dbo)
//        {
//            return DataAccessorExtentions.GetDateTime(this, nameValue, AccesOptions, null);
//        }






//        public virtual long GetLong(string nameValue)
//        {
//            return GetLong(nameValue, 0);
//        }
//        public virtual long GetLong(string nameValue, long DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
//        {
//            return DataAccessorExtentions.GetLong(this, nameValue, DefaultValue, AccesOptions);
//        }
//        public virtual long? GetLong(string nameValue, DataAccessorOptionEnum AccesOptions, bool dbo)
//        {
//            return DataAccessorExtentions.GetLong(this, nameValue, AccesOptions, null);
//        }



//        public virtual double GetDouble(string nameValue)
//        {
//            return GetLong(nameValue, 0);
//        }
//        public virtual double GetDouble(string nameValue, long DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
//        {
//            return DataAccessorExtentions.GetDouble(this, nameValue, DefaultValue, AccesOptions);
//        }
//        public virtual double? GetDouble(string nameValue, DataAccessorOptionEnum AccesOptions, bool dbo)
//        {
//            return DataAccessorExtentions.GetDouble(this, nameValue, AccesOptions, null);
//        }







//        public virtual bool GetBoolean(string nameValue)
//        {
//            return GetBoolean(nameValue, false);
//        }
//        public virtual bool GetBoolean(string nameValue, bool DefaultValue, DataAccessorOptionEnum AccesOptions = 0)
//        {
//            return DataAccessorExtentions.GetBoolean(this, nameValue, DefaultValue, AccesOptions);
//        }
//        public virtual bool? GetBoolean(string nameValue, DataAccessorOptionEnum AccesOptions, bool dbo)
//        {
//            return DataAccessorExtentions.GetBoolean(this, nameValue, AccesOptions, dbo);
//        }







//        public TEnum GetEnum<TEnum>(string fieldname) where TEnum : struct
//        {
//            return DataAccessorExtentions.GetEnum<TEnum>(this, fieldname);
//        }
//        public TEnum GetEnum<TEnum>(string fieldname, TEnum defaultValue, DataAccessorOptionEnum AccesOptions = 0) where TEnum : struct
//        {
//            return DataAccessorExtentions.GetEnum<TEnum>(this, fieldname, defaultValue, AccesOptions);
//        }









//    }

//}
