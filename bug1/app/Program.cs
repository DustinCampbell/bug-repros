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

            var exceptionCaught = false;

            try
            {
                var types = assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException)
            {
                exceptionCaught = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught unexpected exception: {ex.GetType().Name}");
                throw;
            }

            if (expectException)
            {
                if (exceptionCaught)
                {
                    Console.WriteLine($"EXPECTED: {nameof(ReflectionTypeLoadException)}.");
                }
                else
                {
                    Console.WriteLine($"UNEXPECTED: no {nameof(ReflectionTypeLoadException)}");
                }
            }
            else
            {
                if (exceptionCaught)
                {
                    Console.WriteLine($"UNEXPECTED: {nameof(ReflectionTypeLoadException)}");
                }
                else
                {
                    Console.WriteLine($"EXPECTED: no {nameof(ReflectionTypeLoadException)}.");
                }
            }
        }

        private static void CreateClass(bool expectException)
        {
            var exceptionCaught = false;

            try
            {
                InnerCreateClass();
            }
            catch (FileNotFoundException)
            {
                exceptionCaught = true;
            }

            if (expectException)
            {
                if (exceptionCaught)
                {
                    Console.WriteLine($"EXPECTED: {nameof(FileNotFoundException)}.");
                }
                else
                {
                    Console.WriteLine($"UNEXPECTED: no {nameof(FileNotFoundException)}");
                }
            }
            else
            {
                if (exceptionCaught)
                {
                    Console.WriteLine($"UNEXPECTED: {nameof(FileNotFoundException)}");
                }
                else
                {
                    Console.WriteLine($"EXPECTED: no {nameof(FileNotFoundException)}.");
                }
            }
        }

        private static void InnerCreateClass()
        {
            var c = new lib2.MyClass();
        }
    }
}
