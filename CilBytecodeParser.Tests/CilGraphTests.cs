﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CilBytecodeParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilBytecodeParser.Tests
{
    [TestClass]
    public class CilGraphTests
    {
        [TestMethod]
        public void Test_CilGraph_HelloWorld()
        {
            MethodInfo mi = typeof(SampleMethods).GetMethod("PrintHelloWorld");
            CilGraph graph = CilAnalysis.GetGraph(mi);
            AssertThat.IsCorrect(graph);

            CilGraphNode[] nodes = graph.GetNodes().ToArray();

            AssertThat.NotEmpty(nodes, "The result of PrintHelloWorld method parsing should not be empty collection");
                        
            AssertThat.HasOnlyOneMatch(
                nodes,
                (x) => x.Instruction.OpCode == OpCodes.Ldstr && x.Instruction.ReferencedString == "Hello, World",
                "The result of PrintHelloWorld method parsing should contain a single 'ldstr' instruction referencing \"Hello, World\" literal"
                );

            //verify that node sequence contains a single call to Console.WriteLine
            AssertThat.HasOnlyOneMatch(
                nodes,
                (x) =>
                {
                    if (x.Instruction.OpCode != OpCodes.Call) return false;
                    var method = x.Instruction.ReferencedMember as MethodBase;
                    if (method == null) return false;

                    return method.Name == "WriteLine" && method.DeclaringType.Name == "Console";
                },
                "The result of PrintHelloWorld method parsing should contain a single call to Console.WriteLine"
                );

            CilGraphNode last = nodes[nodes.Length - 1];                       
            Assert.IsTrue(last.Instruction.OpCode == OpCodes.Ret, "The last instruction of PrintHelloWorld method should be 'ret'");
            Assert.IsNull(last.BranchTarget, "The 'ret' instruction should not have branch target");

            //Test conversion to string
            string str = graph.ToString();
            AssertThat.IsMatch(str, new MatchElement[] { new Literal(".method"), MatchElement.Any, new Literal("public") });
            AssertThat.IsMatch(str, new MatchElement[] { new Literal(".method"), MatchElement.Any, new Literal("static") });

            AssertThat.IsMatch(str, new MatchElement[] { 
                new Literal(".method"), MatchElement.Any, new Literal("void"), MatchElement.Any, 
                new Literal("PrintHelloWorld"), MatchElement.Any, 
                new Literal("cil"), MatchElement.Any, new Literal("managed"), MatchElement.Any, 
                new Literal("{"), MatchElement.Any, 
                new Literal("ldstr"), MatchElement.Any, new Literal("\"Hello, World\""), MatchElement.Any,
                new Literal("call"), MatchElement.Any, 
                new Literal("void"), MatchElement.Any, new Literal("System.Console::WriteLine"), MatchElement.Any,
                new Literal("ret"), MatchElement.Any, 
                new Literal("}") 
            });
            

            //Test EmitTo: only NetFX
#if !NETSTANDARD
            DynamicMethod dm = new DynamicMethod("CilGraphTests_PrintHelloWorldDynamic", typeof(void), new Type[] { }, typeof(SampleMethods).Module);
            ILGenerator ilg = dm.GetILGenerator(512);
            graph.EmitTo(ilg);
            Action deleg = (Action)dm.CreateDelegate(typeof(Action));
            deleg();
#endif
        }

        [TestMethod]
        public void Test_CilGraph_Loop()
        {
            MethodInfo mi = typeof(SampleMethods).GetMethod("PrintTenNumbers");
            CilGraph graph = CilAnalysis.GetGraph(mi);
            AssertThat.IsCorrect(graph);

            CilGraphNode[] nodes = graph.GetNodes().ToArray();

            AssertThat.NotEmpty(nodes, "The result of PrintTenNumbers method parsing should not be empty collection");

            AssertThat.HasAtLeastOneMatch(
                nodes,
                (x) => x.BranchTarget != null,
                "The 'for' loop parsing results should contain at least one node with branch target"
                );

            CilGraphNode last = nodes[nodes.Length - 1];
            Assert.IsTrue(last.Instruction.OpCode == OpCodes.Ret, "The last instruction of PrintTenNumbers method should be 'ret'");
            Assert.IsNull(last.BranchTarget, "The 'ret' instruction should not have branch target");

            //Test conversion to string
            string str = graph.ToString();
            AssertThat.IsMatch(str, new MatchElement[] { new Literal(".method"), MatchElement.Any, new Literal("public") });
            AssertThat.IsMatch(str, new MatchElement[] { new Literal(".method"), MatchElement.Any, new Literal("static") });

            AssertThat.IsMatch(str, new MatchElement[] { 
                new Literal(".method"), MatchElement.Any, new Literal("void"), MatchElement.Any, 
                new Literal("PrintTenNumbers"), MatchElement.Any, 
                new Literal("cil"), MatchElement.Any, new Literal("managed"), MatchElement.Any, 
                new Literal("{"), MatchElement.Any, 
                new Literal(".locals"), MatchElement.Any, new Literal("int32"), MatchElement.Any, 
                new Literal("call"), MatchElement.Any, 
                new Literal("void"), MatchElement.Any, new Literal("System.Console::WriteLine"), MatchElement.Any,
                new Literal("ret"), MatchElement.Any, 
                new Literal("}") 
            });

            AssertThat.IsMatch(str, new MatchElement[] { new Literal("IL_"), MatchElement.Any, new Literal(":") });

            //Test EmitTo: only NetFX
#if !NETSTANDARD
            DynamicMethod dm = new DynamicMethod("CilGraphTests_PrintTenNumbersDynamic", typeof(void), new Type[] { }, typeof(SampleMethods).Module);
            ILGenerator ilg = dm.GetILGenerator(512);
            graph.EmitTo(ilg);
            Action deleg = (Action)dm.CreateDelegate(typeof(Action));
            deleg();
#endif
        }
                
#if !NETSTANDARD
        [TestMethod]
        public void Test_CilGraph_Emit() //Test EmitTo: only NetFX
        {
            DynamicMethod dm = new DynamicMethod("CilGraphTests_EmitTest", typeof(void), new Type[] { }, typeof(SampleMethods).Module);
            ILGenerator ilg = dm.GetILGenerator(512);
            MethodInfo miTemplate = typeof(SampleMethods).GetMethod("TemplateMethod");
            CilGraph graph = CilAnalysis.GetGraph(miTemplate);
            MethodInfo miTarget = typeof(SampleMethods).GetMethod("IncrementCounter");

            graph.EmitTo(ilg, (instr) =>
            {
                if ((instr.OpCode.Equals(OpCodes.Call) || instr.OpCode.Equals(OpCodes.Callvirt)) &&
                    instr.ReferencedMember.Name == "DoSomething")
                {
                    //replace every DoSomething call by IncrementCounter call

                    ilg.EmitCall(OpCodes.Call,miTarget,null);
                    return true; //handled
                }
                else return false;
            });

            //create and execute delegate
            Action deleg = (Action)dm.CreateDelegate(typeof(Action));
            SampleMethods.counter = 0;
            deleg();
            Assert.AreEqual<int>(15, SampleMethods.counter); 
        }
#endif
    }
}
