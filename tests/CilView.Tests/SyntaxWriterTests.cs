﻿/* CIL Tools
 * Copyright (c) 2022,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: BSD 2.0 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using CilTools.Metadata;
using CilTools.Reflection;
using CilTools.Syntax;
using CilTools.Tests.Common;
using CilView.Core.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilView.Tests
{
    [TestClass]
    public class SyntaxWriterTests
    {
        static InstructionEncoder EmitMethodBody_HelloWorld(
            MetadataBuilder metadata, 
            AssemblyReferenceHandle corlibAssemblyRef)
        {
            var codeBuilder = new BlobBuilder();
            var encoder = new InstructionEncoder(codeBuilder, new ControlFlowBuilder());

            AssemblyReferenceHandle consoleAssemblyRef = AssemblyEmitter.AddAssemblyReference(
                metadata,
                "System.Console",
                new Version(4, 0, 0, 0));

            TypeReferenceHandle systemConsoleTypeRefHandle = metadata.AddTypeReference(
                consoleAssemblyRef,
                metadata.GetOrAddString("System"),
                metadata.GetOrAddString("Console"));

            var consoleWriteLineSignature = new BlobBuilder();

            new BlobEncoder(consoleWriteLineSignature).
                MethodSignature().
                Parameters(1,
                    returnType => returnType.Void(),
                    parameters => parameters.AddParameter().Type().String());

            MemberReferenceHandle consoleWriteLineMemberRef = metadata.AddMemberReference(
                systemConsoleTypeRefHandle,
                metadata.GetOrAddString("WriteLine"),
                metadata.GetOrAddBlob(consoleWriteLineSignature));

            // ldstr "Hello, world"
            encoder.LoadString(metadata.GetOrAddUserString("Hello, world"));

            // call void [System.Console]System.Console::WriteLine(string)
            encoder.Call(consoleWriteLineMemberRef);

            // ret
            encoder.OpCode(ILOpCode.Ret);

            return encoder;
        }

        const string IL_HelloWorld = @"// CIL Tools @version
// https://github.com/MSDN-WhiteKnight/CilTools

.assembly extern System.Runtime
{
  .ver 4:0:0:0
}

.assembly extern System.Console
{
  .ver 4:0:0:0
}

.assembly MyAssembly
{
  .ver 1:0:0:0
}

.module MyAssembly.dll
//MVID: @mvid
.imagebase  0x400000
.file  alignment 0x200
.stackreserve  0x100000
.subsystem  0x3 // WINDOWS_CUI
.corflags  0x1

.class public auto ansi beforefieldinit MyAssembly.Program
extends [System.Runtime]System.Object
{

 .method  public hidebysig instance .ctor() cil managed
 {
  .maxstack 8

           ldarg.0 
           call         instance void [System.Runtime]System.Object::.ctor()
           ret 
 }

 .method public hidebysig static void TestMethod() cil managed
 {
  .maxstack 8

           ldstr        ""Hello, world""
           call         void [System.Console]System.Console::WriteLine(string)
           ret
 }

}";

        [TestMethod]
        public async Task Test_SyntaxWriter_DisassembleAsync()
        {
            //emit test data assembly
            AssemblyEmitter emitter = new AssemblyEmitter("MyAssembly", EmitMethodBody_HelloWorld);
            byte[] bytes = emitter.GetAssemblyBytes();
            MemoryImage img = new MemoryImage(bytes, "MyAssembly.dll", true);
            AssemblyReader reader = new AssemblyReader();
            StringBuilder sb = new StringBuilder(1000);
            StringWriter wr = new StringWriter(sb);
            string mvid;
            string ver = typeof(SyntaxWriter).Assembly.GetName().Version.ToString();
            
            using (reader)
            {
                //disassemble assembly
                Assembly ass = reader.LoadImage(img);
                Assert.AreEqual("MyAssembly", ass.GetName().Name);
                await SyntaxWriter.DisassembleAsync(ass, new DisassemblerParams(), wr);
                mvid = ass.ManifestModule.ModuleVersionId.ToString();
            }

            //verify output
            string il = IL_HelloWorld.Replace("@version", ver).Replace("@mvid", mvid);
            string str = sb.ToString().Trim();
            AssertThat.AreLexicallyEqual(il, str);
        }
    }
}
