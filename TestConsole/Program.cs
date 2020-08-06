using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    internal class Program
    {
        private static IEnumerable<IEnumerable<T>> Combs<T>(IEnumerable<T> theSet, int size)
        {
            if (size == 1)
            {
                foreach (var c in theSet)
                    yield return new List<T> { c };
            }
            else if (size == theSet.Count())
            {
                yield return theSet;
            }
            else
            {
                var n = theSet.First();
                var restOfItems = theSet.Skip(1);
                foreach (var m in Combs(restOfItems, size-1))
                {
                    var result = new List<T> { n };
                    result.AddRange(m);
                    yield return result;
                }

                if (restOfItems.Count() >= size)
                    foreach (var nnn in Combs(restOfItems, size))
                        yield return nnn;
            }
        }

        private static void Main(string[] args)
        {
            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine("Hello World!");
            foreach (var x in Combs("ABCDE",2))
            {
                Console.WriteLine(string.Join("", x));
            }
            Console.ReadKey();
        }
    }
}
