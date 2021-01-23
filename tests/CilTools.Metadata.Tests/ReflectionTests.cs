﻿/* CilTools.Metadata tests
 * Copyright (c) 2020,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: BSD 2.0 */
using System;
using System.Reflection;
using CilTools.Reflection;
using CilTools.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilTools.Metadata.Tests
{
    [TestClass]
    public class ReflectionTests
    {
        [TestMethod]
        public void Test_PInvoke()
        {
            AssemblyReader reader = new AssemblyReader();

            using (reader)
            {
                Assembly ass = reader.LoadFrom(typeof(SampleMethods).Assembly.Location);
                Type t = ass.GetType("CilTools.Tests.Common.SampleMethods");
                CustomMethod mi = t.GetMember("ShowWindow")[0] as CustomMethod;

                Assert.IsTrue(
                    mi.Attributes.HasFlag(MethodAttributes.PinvokeImpl),
                    "ShowWindow should have PinvokeImpl attribute"
                    );

                PInvokeParams pars = mi.GetPInvokeParams();

                Assert.AreEqual("user32.dll", pars.ModuleName);
                Assert.IsTrue(pars.SetLastError, "SetLastError should be true for ShowWindow");
            }
        }

        [TestMethod]
        public void Test_NavigationExternal()
        {
            AssemblyReader reader = new AssemblyReader();

            using (reader)
            {
                Assembly ass = reader.LoadFrom(typeof(SampleMethods).Assembly.Location);
                Type t = ass.GetType("CilTools.Tests.Common.SampleMethods");
                MethodBase mi = t.GetMember("PrintHelloWorld")[0] as MethodBase;
                ReflectionTestsCore.Test_NavigationExternal(mi);
            }
        }

        [TestMethod]
        public void Test_NavigationInternal()
        {
            AssemblyReader reader = new AssemblyReader();

            using (reader)
            {
                Assembly ass = reader.LoadFrom(typeof(SampleMethods).Assembly.Location);
                Type t = ass.GetType("CilTools.Tests.Common.SampleMethods");
                MethodBase mi = t.GetMember("CreatePoint")[0] as MethodBase;
                ReflectionTestsCore.Test_NavigationInternal(mi);
            }
        }

        [TestMethod]
        public void Test_TypedReferenceParam()
        {
            AssemblyReader reader = new AssemblyReader();

            using (reader)
            {
                Assembly ass = reader.LoadFrom(typeof(SampleMethods).Assembly.Location);
                Type t = ass.GetType("CilTools.Tests.Common.SampleMethods");
                MethodBase mi = t.GetMember("TypedRefTest")[0] as MethodBase;
                ReflectionTestsCore.Test_TypedReferenceParam(mi);
            }
        }

        [TestMethod]
        public void Test_BaseTypes()
        {
            AssemblyReader reader = new AssemblyReader();

            using (reader)
            {
                Assembly ass = reader.LoadFrom(typeof(object).Assembly.Location);
                Type tException = ass.GetType("System.Exception");
                Type tString = ass.GetType("System.String");
                Type tObject = ass.GetType("System.Object");
                Type t = ass.GetType("System.TypeLoadException");

                Assert.IsTrue(tException.IsAssignableFrom(tException));

                //TypeLoadException is derived from System.Exception
                Assert.IsTrue(tException.IsAssignableFrom(t));

                //reverse is not true
                Assert.IsFalse(t.IsAssignableFrom(tException));

                //String is not derived from Exception
                Assert.IsFalse(tException.IsAssignableFrom(tString));

                //every reference type is derived from object
                Assert.IsTrue(tObject.IsAssignableFrom(t));
                Assert.IsTrue(tObject.IsAssignableFrom(tException));
                Assert.IsTrue(tObject.IsAssignableFrom(tString));

                //check that we are compatible with standard reflection
                Assert.IsTrue(tException.IsAssignableFrom(typeof(Exception)));
                Assert.IsTrue(tException.IsAssignableFrom(typeof(TypeLoadException)));
                Assert.IsFalse(tException.IsAssignableFrom(typeof(string)));
            }
        }

        [TestMethod]
        public void Test_ExternalType()
        {
            AssemblyReader reader = new AssemblyReader();

            using (reader)
            {
                Assembly ass = reader.LoadFrom(typeof(SampleMethods).Assembly.Location);
                Type t = ass.GetType("CilTools.Tests.Common.SampleMethods");
                MethodBase mi = t.GetMember("GetInterfaceCount")[0] as MethodBase;
                Type paramtype = mi.GetParameters()[0].ParameterType;

                Assert.IsTrue(paramtype.Attributes.HasFlag(TypeAttributes.Public));
                Assert.AreEqual("MemberInfo", paramtype.BaseType.Name);

                Assert.IsTrue(paramtype.IsAssignableFrom(paramtype));
                Assert.IsTrue(paramtype.IsAssignableFrom(typeof(Type)));
            }
        }
    }
}
