using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.ACCESSORS;
using Nglib.TESTS.MODELS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.ACCESSORS
{
    [TestClass()]
    public class DataAccessorTests
    {


        [TestMethod()]
        public void DataAccessorMultiTest()
        {
            ValuesDemo demo = new ValuesDemo();
            demo.SetData("test", "test", DataAccessorOptionEnum.Default); //Ajout d'une valeur standard
            demo.SetObject("test2", 123); //Ajout simplifié d'une valeur (méthode d'extension)
            demo.SetObject("testdate", "20/04/2024");  
            demo.SetObject("testbool", true);  

            Assert.AreEqual(demo.GetData("test", DataAccessorOptionEnum.Default), "test"); //Obtention de la valeur standard
            Assert.AreEqual(demo.GetString("test"), "test");//Obtention simplifié de la valeur (méthode d'extension)
            Assert.AreEqual(demo.GetObject("test2"), 123); //Obtention simplifié de la valeur (méthode d'extension)
            Assert.AreEqual(demo.GetInt("test2"),123); //Obtention simplifié de la valeur (méthode d'extension)
            Assert.AreEqual(demo.GetDateTime("testdate"), new DateTime(2024, 4, 20));  
            Assert.AreEqual(demo.GetBoolean("testbool"), true);

            Assert.IsTrue(demo.ListFieldsKeys().Length>3); //Liste des champs


        }


        [TestMethod()]
        public void DataAccessorToolsCopyToTest()
        {
            ValuesDemo demo1 = new ValuesDemo();
            demo1.SetObject("test2", 123);
            demo1.SetObject("testdate", "20/04/2024");

            ValuesDemo demo2 = new ValuesDemo();
            DataAccessorTools.CopyTo(demo1, demo2);
            Assert.AreEqual(demo2.GetInt("test2"), 123);
            Assert.AreEqual(demo2.GetDateTime("testdate"), new DateTime(2024, 4, 20));
        }












        [TestMethod()]
        public void GetEnumDefaultValueTest()
        {

        }
    }
}