﻿/* CIL Tools
 * Copyright (c) 2023,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: BSD 2.0 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace CilTools.Tests.Common
{
    public static class SampleMethods
    {
        public static void PrintHelloWorld()
        {
            Console.WriteLine("Hello, World");
        }

        public static double CalcSum(double x, double y)
        {
            return x + y;
        }

        public static int Foo = 2;

        public static void SquareFoo()
        {
            Foo = Foo * Foo;
        }

        public static int GetInterfaceCount(Type t)
        {
            Type[] array = t.GetInterfaces();
            return array.Length;
        }

        public static void PrintList()
        {
            List<string> lst = new List<string>();
            lst.Add("Bob");
            lst.Add("Alice");
            Console.WriteLine(String.Join(";", lst));
        }

        public static T[] GenerateArray<T>(int len)
        {
            return new T[len];
        }

        public static void PrintTenNumbers()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
        }

        public static void PrintProcessId()
        {
            int x = System.Diagnostics.Process.GetCurrentProcess().Id;
            Console.WriteLine(x.ToString());
        }

        public static void DoSomething(int i)
        {
            //placeholder method            
        }

        public static void TemplateMethod() //template for a dynamic method
        {            
            for (int i = 1; i <= 5; i++)
            {
                DoSomething(i);                
            }
        }

        public static int counter;

        public static void IncrementCounter(int i)
        {
            counter += i;
        }

        public delegate bool DivideNumbersDelegate(int x, int y, out int result);

        public static bool DivideNumbers(int x,int y,out int result)
        {
            int z = 0;
            try
            {
                z = x / y;
            }
            catch (DivideByZeroException)
            {
                return false;
            }
            finally
            {
                result = z;
            }
            return true;
        }

        public static string SwitchTest(int x)
        {
            switch (x)
            {
                case 1: return "One";
                case 2: return "Two";
                case 3: return "Three";
                default: return x.ToString();
            }
        }

        public static int f;

        public static void TestTokens()
        {
            Console.WriteLine(typeof(int));
            Console.WriteLine(typeof(SampleMethods));
        }

        public static bool TestEmptyString(string str)
        {
            return str == "";
        }

        public static void TestOptionalParams(string str="",int x=0)
        {
            Console.WriteLine(str + x.ToString());
        }

        public static MyPoint CreatePoint(float x, float y)
        {
            MyPoint p;
            p = new MyPoint();
            p.X = x;
            p.Y = y;
            return p;
        }

        public static unsafe int PointerTest()
        {
            fixed (int* p = &f)
            {
                int* q = p + 1;
                return *q;
            }
        }

        public static void TypedRefTest(TypedReference tr)
        {

        }

        public static void ConstrainedTest<T>(T x)
        {
            Console.WriteLine(x.ToString());
        }

        [STAThread]
        [My(1)]
        public static void AttributeTest() { }

        [return: My(999)]
        public static void ReturnTypeAttributeTest() { }

        public static int ParameterAttributesTest([My(123)] int x)
        { 
            return x * 2; 
        }

        [DllImport("user32.dll",SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void GenericsTest()
        {
            int[] arr=GenerateArray<int>(10);
            Console.WriteLine(arr.Rank);
        }

        public static void method() { }

        public static string FilteredExceptionsTest()
        {
            string ret;

            try
            {
                ret=File.ReadAllLines("test.txt")[0];
            }
            catch (Exception ex) when (ex.Message.Contains("does not exist"))
            {
                ret=string.Empty;
            }
            
            return ret;
        }

        public static void TestEscaping()
        {
            Console.WriteLine("\"English - Русский - Ελληνικά - Español\r\n\tąęėšų,.\\x<y+z\"");
        }

        public static void TestMethodRefGenericParameter()
        {
            object loc = null;
            Interlocked.CompareExchange<object>(ref loc, new object(), new object());
            Console.WriteLine((loc == null).ToString());
        }

        public static T Sum<T>(T x, T y) where T : struct
        {
            if (typeof(T) == typeof(int))
            {
                int res = (Convert.ToInt32(x) + Convert.ToInt32(y));
                return (T)Convert.ChangeType(res, typeof(int));
            }
            else if (typeof(T) == typeof(float))
            {
                float res = (Convert.ToSingle(x) + Convert.ToSingle(y));
                return (T)Convert.ChangeType(res, typeof(int));
            }
            else throw new NotSupportedException();
        }

        public static void TestByRefTypes(ref DateTime x, ref Attribute y) { }

        public static void TestGenericStruct(ArraySegment<int> x) { }

        public static void TestNestedTypes(Environment.SpecialFolder sf) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TestAggressiveInlining() { }
    }

    public class MyPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    [My(560)]
    public class TestType
    {
        public int x;
    }
}
