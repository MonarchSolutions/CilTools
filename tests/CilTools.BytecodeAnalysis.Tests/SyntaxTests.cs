﻿/* CIL Tools
 * Copyright (c) 2022,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: BSD 2.0 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CilTools.Syntax;
using CilTools.Tests.Common;
using CilTools.Tests.Common.TestData;

namespace CilTools.BytecodeAnalysis.Tests
{
    public class TypeWithProperties
    {
        public string Name { get; set; }
        public int Number { get; }
        public string this[int i] { get { return i.ToString(); } }
    }

    [TestClass]
    public class SyntaxTests
    {
        [TestMethod]
        [MethodTestData(typeof(SampleMethods), "PrintHelloWorld", BytecodeProviders.All)]
        public void Test_ToSyntaxTree(MethodBase mi)
        {
            CilGraph graph = CilGraph.Create(mi);
            MethodDefSyntax mds = graph.ToSyntaxTree();
            AssertThat.IsSyntaxTreeCorrect(mds);
            Assert.AreEqual("method", mds.Signature.Name);

            AssertThat.HasOnlyOneMatch(
                mds.Signature.EnumerateChildNodes(),
                (x) => { return x is KeywordSyntax && (x as KeywordSyntax).Content == "public"; },
                "Method signature should contain 'public' keyword"
                );

            AssertThat.HasOnlyOneMatch(
                mds.Signature.EnumerateChildNodes(),
                (x) => {
                    return x is IdentifierSyntax && (x as IdentifierSyntax).Content == "PrintHelloWorld";
                },
                "Method signature should contain mathod name identifier"
                );

            AssertThat.HasOnlyOneMatch(
                mds.Body.Content,
                (x) => {
                    return x is InstructionSyntax && (x as InstructionSyntax).Operation == "ldstr";
                },
                "Method body should contain 'ldstr' instruction"
                );
        }

        [TestMethod]
        [MethodTestData(typeof(SampleMethods), "method", BytecodeProviders.All)]
        public void Test_KeywordAsIdentifier(MethodBase mi)
        {
            string str = CilAnalysis.MethodToText(mi);

            AssertThat.IsMatch(str, new Text[] {
                ".method", Text.Any, "public", Text.Any,
                "void", Text.Any,
                "'method'", Text.Any,
                "cil", Text.Any, "managed", Text.Any,
            });
        }

        [TestMethod]        
        public void Test_Properties()
        {
            IEnumerable<SyntaxNode> nodes=SyntaxNode.GetTypeDefSyntax(typeof(TypeWithProperties));            
            string s = Utils.SyntaxToString(nodes);
                        
            AssertThat.IsMatch(s, new Text[] {
                ".class", Text.Any,"public", Text.Any,"TypeWithProperties", Text.Any,"{", Text.Any,
                ".property", Text.Any,"instance", Text.Any,"string", Text.Any,"Name", Text.Any,"()", Text.Any,
                "{", Text.Any,
                ".get", Text.Any,"instance", Text.Any,"string", Text.Any,"get_Name", Text.Any,"()", Text.Any,
                ".set", Text.Any,"instance", Text.Any,"void", Text.Any,"set_Name", Text.Any,"(", Text.Any,
                "string", Text.Any,")", Text.Any,
                "}", Text.Any,
                "}", Text.Any
            });

            AssertThat.IsMatch(s, new Text[] {
                ".class", Text.Any,"public", Text.Any,"TypeWithProperties", Text.Any,"{", Text.Any,
                ".property", Text.Any,"instance", Text.Any,"int32", Text.Any,"Number", Text.Any,"()", Text.Any,
                "{", Text.Any,
                ".get", Text.Any,"instance", Text.Any,"int32", Text.Any,"get_Number", Text.Any,"()", Text.Any,                
                "}", Text.Any,
                "}", Text.Any
            });

            AssertThat.IsMatch(s, new Text[] {
                ".class", Text.Any,"public", Text.Any,"TypeWithProperties", Text.Any,"{", Text.Any,
                ".property", Text.Any,"instance", Text.Any,"string", Text.Any,"Item",Text.Any,
                "(", Text.Any,"int32", Text.Any,")", Text.Any,
                "{", Text.Any,
                ".get", Text.Any,"instance", Text.Any,"string", Text.Any,"get_Item",
                "(", Text.Any,"int32", Text.Any,")", Text.Any,
                "}", Text.Any,
                "}", Text.Any
            });
        }

        [TestMethod]
        [MethodTestData(typeof(SampleMethods), "PrintHelloWorld", BytecodeProviders.All)]
        public void Test_Syntax_EntryPoint_Library(MethodBase m)
        {
            string str = CilAnalysis.MethodToText(m);
            Assert.IsFalse(str.Contains(".entrypoint"));
        }

        static int PrintHelloWorld_GetCodeSize()
        {
            if (Utils.GetConfig() == "Release") return 11;
            else return 13;
        }

        [TestMethod]
        [MethodTestData(typeof(SampleMethods), "PrintHelloWorld", BytecodeProviders.All)]
        public void Test_CodeSize(MethodBase mi)
        {
            CilGraph graph = CilGraph.Create(mi);
            DisassemblerParams pars = new DisassemblerParams();
            pars.IncludeCodeSize = true;
            MethodDefSyntax syntax = graph.ToSyntaxTree(pars);
            string str = syntax.ToString();

            AssertThat.IsMatch(str, new Text[] {
                ".method", Text.Any,"public", Text.Any,"PrintHelloWorld", Text.Any,"{", Text.Any,
                "// Code size: " + PrintHelloWorld_GetCodeSize().ToString(), Text.Any,
                "call", Text.Any, "System.Console::WriteLine(string)", Text.Any, 
                "}", Text.Any
            });
        }

        [TestMethod]
        [MethodTestData(typeof(SampleMethods), "PrintHelloWorld", BytecodeProviders.All)]
        public void Test_CodeSize_Negative(MethodBase mi)
        {
            CilGraph graph = CilGraph.Create(mi);
            DisassemblerParams pars = new DisassemblerParams();
            pars.IncludeCodeSize = false;
            MethodDefSyntax syntax = graph.ToSyntaxTree(pars);
            string str = syntax.ToString();

            Assert.IsFalse(str.Contains("// Code size: "));
        }

        [TestMethod]
        [MethodTestData(typeof(SampleMethods), "Sum", BytecodeProviders.All)]
        [WorkItem(53)]
        public void Test_Syntax_GenericMethodParameterConstraints(MethodBase m)
        {
            string str = CilAnalysis.MethodToText(m);

            AssertThat.IsMatch(str, new Text[] {
                ".method", Text.Any, "public", Text.Any, "!!", Text.Any, "Sum", Text.Any,
                "<", Text.Any, "valuetype", Text.Any, ".ctor", Text.Any, "(", Text.Any,
                "System.ValueType", Text.Any,")", Text.Any, "T", Text.Any, ">", Text.Any,
                "{", Text.Any, "}", Text.Any
            });
        }

        [TestMethod]
        [WorkItem(53)]
        public void Test_GenericTypeParameterConstraints()
        {
            Type t = typeof(GenericConstraintsSample<>);
            IEnumerable<SyntaxNode> nodes = SyntaxNode.GetTypeDefSyntax(t);
            string str = Utils.SyntaxToString(nodes);

            AssertThat.IsMatch(str, new Text[] {
                ".class", Text.Any, "public", Text.Any, "GenericConstraintsSample", Text.Any, 
                "<", Text.Any, "valuetype", Text.Any, ".ctor", Text.Any, "(", Text.Any,
                "System.ValueType", Text.Any,")", Text.Any, "T", Text.Any, ">", Text.Any,
                "{", Text.Any, "}", Text.Any
            });
        }

        [TestMethod]
        [WorkItem(53)]
        public void Test_GenericTypeParameterFlags()
        {
            Type t = typeof(Action<>);
            IEnumerable<SyntaxNode> nodes = SyntaxNode.GetTypeDefSyntax(t);
            string str = Utils.SyntaxToString(nodes);

            AssertThat.IsMatch(str, new Text[] {
                ".class", Text.Any, "public", Text.Any, "Action", Text.Any,
                "<", Text.Any, "-", Text.Any, "T", Text.Any, ">", Text.Any,
                "{", Text.Any, "}", Text.Any
            });
        }
    }
}
