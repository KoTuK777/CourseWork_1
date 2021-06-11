using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Examples
{
    public class Test1
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Surname { get; set; }
    }

    public class Test2
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Surname { get; set; }
    }

    public struct Struct
    {
        private int m_a;
        private double m_b;
        private string m_c;

        public int A => m_a;
        public double B => m_b;
        public string C => m_c;

        public Struct(int a, double b, string c)
        {
            m_a = a;
            m_b = b;
            m_c = c;
        }
    }

    public class SimpleClass
    {
        public int A { get; set; }
        public Struct B { get; set; }
    }

    public class ComplexClass
    {
        public int A { get; set; }
        public IntPtr B { get; set; }
        public UIntPtr C { get; set; }
        public string D { get; set; }
        public SimpleClass E { get; set; }
        public int? F { get; set; }
        public int[] G { get; set; }
        public List<int> H { get; set; }
        public double I { get; set; }
        public float J { get; set; }
    }

    class Program
    {
        private static int[] MakeArray(int count)
        {
            return Enumerable.Range(0, count).ToArray();
        }

        private static List<int> MakeList(int count)
        {
            return MakeArray(count).ToList();
        }

        static void Main(string[] args)
        {            
            var oc = new ObjectComparer();

            ComplexClass x = new ComplexClass
            {
                A = 42,
                B = new IntPtr(42),
                C = new UIntPtr(42),
                D = "abc",
                E = new SimpleClass { A = 42, B = new Struct(42, 42.42, "Hey") },
                F = 1,
                G = MakeArray(1337),
                H = MakeList(1337),
                I = double.MaxValue,
                J = float.MinValue
            };

            ComplexClass y = new ComplexClass
            {
                A = 42,
                B = new IntPtr(42),
                C = new UIntPtr(42),
                D = "abc",
                E = new SimpleClass { A = 42, B = new Struct(42, 42.42, "Hey") },
                F = 1,
                G = MakeArray(1337),
                H = MakeList(1337),
                I = double.MaxValue,
                J = float.MinValue
            };

            Console.WriteLine($"x     with  null: {oc.Equals(x, null)}");
            Console.WriteLine($"null  with     x: {oc.Equals(null, x)}");
            Console.WriteLine($"x     with     x: {oc.Equals(x, x)}");
            Console.WriteLine($"x     with     y: {oc.Equals(x, y)}");
            Console.WriteLine($"x     with 'abc': {oc.Equals(x, "abc")}");
            Console.WriteLine($"'abc' with 'abc': {oc.Equals("abc", "abc")}");
            Console.WriteLine($"'abc' with 'abb': {oc.Equals("abc", "abb")}");
            Console.WriteLine($"1337  with  1337: {oc.Equals(1337, 1337)}");
            Console.WriteLine($"1337  with -1337: {oc.Equals(1337, -1337)}");
        }
    }
}
