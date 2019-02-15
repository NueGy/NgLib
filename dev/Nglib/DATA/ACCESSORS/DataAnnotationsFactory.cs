using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nglib.DATA.ACCESSORS
{
    /// <summary>
    /// Astuce pour utiliser des annotations sans les importer (Reflxion)
    /// </summary>
    public static class DataAnnotationsFactory
    {


        /// <summary>
        /// System.ComponentModel.DataAnnotations.Schema.TableAttribute
        /// </summary>
        public static Type TableAttributeType
        {
            get
            {
                if (_TableAttributeType == null) _TableAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.TableAttribute, System.ComponentModel.DataAnnotations");
                if (_TableAttributeType == null) _TableAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.TableAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                

                return _TableAttributeType;
            }
        }
        private static Type _TableAttributeType { get; set; }



        /// <summary>
        /// System.ComponentModel.DataAnnotations.RequiredAttribute
        /// </summary>
        public static Type RequiredAttributeType
        {
            get
            {
                if (_RequiredAttributeType == null) _RequiredAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.RequiredAttribute, System.ComponentModel.DataAnnotations");
                if (_RequiredAttributeType == null) _RequiredAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.RequiredAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _RequiredAttributeType;
            }
        }
        private static Type _RequiredAttributeType { get; set; }



        /// <summary>
        /// System.ComponentModel.DataAnnotations.KeyAttribute
        /// </summary>
        public static Type KeyAttributeType
        {
            get
            {
                if (_KeyAttributeType == null) _KeyAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.KeyAttribute, System.ComponentModel.DataAnnotations");
                if (_KeyAttributeType == null) _KeyAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.KeyAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _KeyAttributeType;
            }
        }
        private static Type _KeyAttributeType { get; set; }



        /// <summary>
        /// System.ComponentModel.DataAnnotations.Schema.ColumnAttribute
        /// </summary>
        public static Type ColumnAttributeType
        {
            get
            {
                if (_ColumnAttributeType == null) _ColumnAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.ColumnAttribute, System.ComponentModel.DataAnnotations");
                if (_ColumnAttributeType == null) _ColumnAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.ColumnAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _ColumnAttributeType;
            }
        }
        private static Type _ColumnAttributeType { get; set; }



        /// <summary>
        /// System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute
        /// </summary>
        public static Type DatabaseGeneratedAttributeType
        {
            get
            {
                if (_DatabaseGeneratedAttributeType == null) _DatabaseGeneratedAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute, System.ComponentModel.DataAnnotations");
                if (_DatabaseGeneratedAttributeType == null) _DatabaseGeneratedAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _DatabaseGeneratedAttributeType;
            }
        }
        private static Type _DatabaseGeneratedAttributeType { get; set; }



        /// <summary>
        /// System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute
        /// </summary>
        public static Type ForeignKeyAttributeType
        {
            get
            {
                if (_ForeignKeyAttributeType == null) _ForeignKeyAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute, System.ComponentModel.DataAnnotations");
                if (_ForeignKeyAttributeType == null) _ForeignKeyAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _ForeignKeyAttributeType;
            }
        }
        private static Type _ForeignKeyAttributeType { get; set; }



        /// <summary>
        /// System.ComponentModel.DataAnnotations.DisplayColumnAttribute
        /// </summary>
        public static Type DisplayColumnAttributeType
        {
            get
            {
                if (_DisplayColumnAttributeType == null) _DisplayColumnAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.DisplayColumnAttribute, System.ComponentModel.DataAnnotations");
                if (_DisplayColumnAttributeType == null) _DisplayColumnAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.DisplayColumnAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _DisplayColumnAttributeType;
            }
        }
        private static Type _DisplayColumnAttributeType { get; set; }



        /// <summary>
        /// System.ComponentModel.DataAnnotations.EditableAttribute
        /// </summary>
        public static Type EditableAttributeType
        {
            get
            {
                if (_EditableAttributeType == null) _EditableAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.EditableAttribute, System.ComponentModel.DataAnnotations");
                if (_EditableAttributeType == null) _EditableAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.EditableAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _EditableAttributeType;
            }
        }
        private static Type _EditableAttributeType { get; set; }




        /// <summary>
        /// System.ComponentModel.DataAnnotations.AssociationAttribute
        /// </summary>
        public static Type AssociationAttributeType
        {
            get
            {
                if (_AssociationAttributeType == null) _AssociationAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.AssociationAttribute, System.ComponentModel.DataAnnotations");
                if (_AssociationAttributeType == null) _AssociationAttributeType = CODE.REFLEXION.ReflexionTools.GetTypeByReflexion("System.ComponentModel.DataAnnotations.AssociationAttribute, System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return _AssociationAttributeType;
            }
        }
        private static Type _AssociationAttributeType { get; set; }






        public static bool AnnotationClassExist(object objClass, Type type)
        {
            if (CODE.REFLEXION.AttributesTools.FindObjectAttribute(objClass, type) != null) return true;
            return false;
        }

        public static bool AnnotationExist(PropertyInfo property, Type type)
        {
            if (CODE.REFLEXION.AttributesTools.FindAttribute(property, type) != null) return true;
            return false;
        }
        public static string AnnotationGetString(PropertyInfo property, Type type, string valuename)
        {
            return CODE.REFLEXION.AttributesTools.FindAttributeGetString(property, type, valuename);
        }




    }
}
