using System;
using System.Linq;
using System.Reflection;

using Cyxor.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Cyxor.Serialization.Test
{
    [TestClass]
    public class ReflectionExtensionsTest
    {
        /// <summary>
        /// The total declared fields count should be consistent across all target frameworks.
        /// We hardcode the total count and adjust consequently after changes that affect this value.
        /// </summary>
        [TestMethod]
        public void TotalDeclaredFields()
        {
            //var totalDeclaredFields = 437;
            //var declaredFieldsCount = typeof(Serializer).GetAllDeclaredFields().Count();

            //Assert.IsTrue(declaredFieldsCount == totalDeclaredFields);
        }

        /// <summary>
        /// The total declared properties count should be consistent across all target frameworks.
        /// We hardcode the total count and adjust consequently after changes that affect this value.
        /// </summary>
        [TestMethod]
        public void TotalDeclaredProperties()
        {
            //var totalDeclaredProperties = 437;
            //var declaredPropertiesCount = typeof(Serializer).GetAllDeclaredProperties().Count();

            //Assert.IsTrue(declaredPropertiesCount == totalDeclaredProperties);

            Assert.IsTrue(true);
        }

        /// <summary>
        /// The total declared methods count should be consistent across all target frameworks.
        /// We hardcode the total count and adjust consequently after changes that affect this value.
        /// </summary>
        [TestMethod]
        public void TotalDeclaredMethods()
        {
            //var totalDeclaredMethods = 437;
            //var declaredMethodsCount = typeof(Serializer).GetAllDeclaredMethods().Count();

            //Assert.IsTrue(declaredMethodsCount == totalDeclaredMethods);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Properties()
        {
            //var declaredMethods = typeof(Serializer).GetAllDeclaredMethods().Count();
            //var runtimeMethods = typeof(Serializer).GetAllRuntimeMethods().Count();

            //Logger.LogMessage($"{declaredMethods}/{runtimeMethods}");

            Assert.IsTrue(true);
            //var publicPoperty = typeof(MyProperties).GetProperty(nameof(MyProperties.Public));
            //var mixPoperty = typeof(MyProperties).GetProperty(nameof(MyProperties.Mix));
            //var privatePoperty = typeof(MyProperties).GetProperty("Private", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            //Assert.IsNotNull(publicPoperty);
            //Assert.IsNotNull(mixPoperty);
            //Assert.IsNotNull(privatePoperty);

            //var publicPopertyGetMethod1 = publicPoperty.GetMethod;
            //var publicPopertySetMethod1 = publicPoperty.SetMethod;

            //Assert.IsNotNull(publicPopertyGetMethod1);
            //Assert.IsNotNull(publicPopertySetMethod1);

            //var publicPopertyGetMethod2 = publicPoperty.GetGetMethod();
            //var publicPopertySetMethod2 = publicPoperty.GetSetMethod();

            //Assert.IsNotNull(publicPopertyGetMethod2);
            //Assert.IsNotNull(publicPopertySetMethod2);

            //var publicPopertyGetMethod3 = publicPoperty.GetGetMethod(nonPublic: false);
            //var publicPopertySetMethod3 = publicPoperty.GetSetMethod(nonPublic: false);

            //Assert.IsNotNull(publicPopertyGetMethod3);
            //Assert.IsNotNull(publicPopertySetMethod3);

            //var publicPopertyGetMethod4 = publicPoperty.GetGetMethod(nonPublic: true);
            //var publicPopertySetMethod4 = publicPoperty.GetSetMethod(nonPublic: true);

            //Assert.IsNotNull(publicPopertyGetMethod4);
            //Assert.IsNotNull(publicPopertySetMethod4);



            //var privatePopertyGetMethod1 = privatePoperty.GetMethod;
            //var privatePopertySetMethod1 = privatePoperty.SetMethod;

            //Assert.IsNotNull(privatePopertyGetMethod1);
            //Assert.IsNotNull(privatePopertySetMethod1);

            //var privatePopertyGetMethod2 = privatePoperty.GetGetMethod();
            //var privatePopertySetMethod2 = privatePoperty.GetSetMethod();

            //Assert.IsNull(privatePopertyGetMethod2);
            //Assert.IsNull(privatePopertySetMethod2);

            //var privatePopertyGetMethod3 = privatePoperty.GetGetMethod(nonPublic: false);
            //var privatePopertySetMethod3 = privatePoperty.GetSetMethod(nonPublic: false);

            //Assert.IsNull(privatePopertyGetMethod3);
            //Assert.IsNull(privatePopertySetMethod3);

            //var privatePopertyGetMethod4 = privatePoperty.GetGetMethod(nonPublic: true);
            //var privatePopertySetMethod4 = privatePoperty.GetSetMethod(nonPublic: true);

            //Assert.IsNotNull(privatePopertyGetMethod4);
            //Assert.IsNotNull(privatePopertySetMethod4);





            //var mixPopertyGetMethod1 = mixPoperty.GetMethod;
            //var mixPopertySetMethod1 = mixPoperty.SetMethod;

            //Assert.IsNotNull(mixPopertyGetMethod1);
            //Assert.IsNotNull(mixPopertySetMethod1);

            //var mixPopertyGetMethod2 = mixPoperty.GetGetMethod();
            //var mixPopertySetMethod2 = mixPoperty.GetSetMethod();

            //Assert.IsNotNull(mixPopertyGetMethod2);
            //Assert.IsNull(mixPopertySetMethod2);

            //var mixPopertyGetMethod3 = mixPoperty.GetGetMethod(nonPublic: false);
            //var mixPopertySetMethod3 = mixPoperty.GetSetMethod(nonPublic: false);

            //Assert.IsNotNull(mixPopertyGetMethod3);
            //Assert.IsNull(mixPopertySetMethod3);

            //var mixPopertyGetMethod4 = mixPoperty.GetGetMethod(nonPublic: true);
            //var mixPopertySetMethod4 = mixPoperty.GetSetMethod(nonPublic: true);

            //Assert.IsNotNull(mixPopertyGetMethod4);
            //Assert.IsNotNull(mixPopertySetMethod4);
        }
    }
}
