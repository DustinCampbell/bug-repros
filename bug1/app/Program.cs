using System;
using System.IO;
using System.Reflection;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Before AssemblyResolve hooked");
            LoadLib2(expectException: true);
            CreateClass(expectException: true);

            HookAssemblyResolve();

            Console.WriteLine();
            Console.WriteLine("After AssemblyResolve hooked");

            LoadLib2(expectException: false);
            CreateClass(expectException: false);
        }

        private static void HookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs", assemblyName.Name + ".dll");
            if (File.Exists(assemblyFilePath))
            {
                return Assembly.LoadFrom(assemblyFilePath);
            }

            return null;
        }

        private static void LoadLib2(bool expectException)
        {
            var assemblyFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lib2.dll");
            var assembly = Assembly.LoadFrom(assemblyFilePath);

            Exception caughtException = null;

            try
            {
                var types = assembly.DefinedTypes;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            TestForException<ReflectionTypeLoadException>(expectException, caughtException);
        }

        private static void CreateClass(bool expectException)
        {
            Exception caughtException = null;

            try
            {
                InnerCreateClass();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            TestForException<FileNotFoundException>(expectException, caughtException);
        }

        private static void InnerCreateClass()
        {
            var c = new lib2.MyClass();
        }

        private static void TestForException<TException>(bool isExpected, Exception caughtException)
            where TException : Exception
        {
            if (isExpected)
            {
                if (caughtException != null)
                {
                    if (caughtException is TException)
                    {
                        Console.WriteLine($"EXPECTED: caught {typeof(TException).FullName}");
                    }
                    else
                    {
                        Console.WriteLine($"UNEXPECTED: no {typeof(TException).FullName} thrown, caught {caughtException.GetType().FullName} instead");
                    }
                }
                else
                {
                    Console.WriteLine($"UNEXPECTED: no exception thrown!");
                }
            }
            else
            {
                if (caughtException != null)
                {
                    Console.WriteLine($"UNEXPECTED: caught {caughtException.GetType().FullName}");
                }
                else
                {
                    Console.WriteLine($"EXPECTED: no {typeof(TException).FullName} thrown");
                }
            }

        }
    }
}
