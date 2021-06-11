using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapper
{
    public class SimpleClass
    {
        public int A { get; set; }
        public Struct B { get; set; }
    }

    public class SimpleClassDTO
    {
        public int A { get; set; }
        public Struct B { get; set; }
    }

    public struct Struct
    {
        private int _a;
        private double _b;
        private string _c;

        public int A => _a;
        public double B => _b;
        public string C => _c;

        public Struct(int a, double b, string c)
        {
            _a = a;
            _b = b;
            _c = c;
        }
    }

    public class ComplexClass
    {
        public int A { get; set; }
        public string B { get; set; }
        public SimpleClass C { get; set; }
        public int? D { get; set; }
        public List<List<int>> E { get; set; }
        public int[] F { get; set; }
    }

    public class ComplexClassDTO
    {
        public int A { get; set; }
        public string B { get; set; }
        public SimpleClassDTO C { get; set; }
        public int? D { get; set; }
        public List<List<int>> E { get; set; }
        public int[] F { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var mapper = new Mapper();

            var a = new ComplexClass
            {
                A = 42,
                B = "abc",
                C = new SimpleClass { A = 42, B = new Struct(42, 42.42, "Hey") },
                D = 1,
                E = new List<List<int>> {
                    new List<int> { 1, 2, 3 },
                    new List<int> { 4, 5, 6 }
                },
                F = Enumerable.Range(0, 7).ToArray()
            };

            mapper
                .Add<ComplexClass, ComplexClassDTO>()
                .Add<SimpleClass, SimpleClassDTO>();

            var b = mapper.Map<ComplexClassDTO>(a);

            Console.WriteLine();

            Console.WriteLine("A object:");
            Console.WriteLine(JsonConvert.SerializeObject(a));
            Console.WriteLine("\nB object after mapping:");
            Console.WriteLine(JsonConvert.SerializeObject(b));

            Console.WriteLine();
            a.A = 43;
            a.B = "b";
            a.C.A = 43;
            a.E[0][1] = 5;
            a.F[0] = 9;

            Console.WriteLine("\nA object after changes:");
            Console.WriteLine(JsonConvert.SerializeObject(a));
            Console.WriteLine("\nObject b after object a changes:");
            Console.WriteLine(JsonConvert.SerializeObject(b));
        }
    }
}
