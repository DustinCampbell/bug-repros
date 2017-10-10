The repro is intended to highlight differences in behavior between the .NET Framework on Windows and Mono with regard to AppDomains, hooking the AppDomain.AssemblyResolve event, and Assembly.LoadFrom(...).

## Content

This repro contains the following projects and types:

* `app` - A console application with a reference to `lib2`.
* `lib1` - A library that defines an interface, `IBaseInterface`. This project is built directly to a folder called `app\bin\Debug\libs`.
* `lib2` - A library with a reference to `lib1` that contains a single class, `MyClass`, which implements `IBaseInterface`. This project is built to `app\bin\Debug` but does not copy its reference to `lib1`.

The folder layout after building `app` looks like so:

* `app\bin\Debug`:

  ![app folder layout](https://github.com/DustinCampbell/bug-repros/blob/master/images/bug1_folder_layout_1.png)
* `app\bin\Debug\libs`:

  ![app folder layout](https://github.com/DustinCampbell/bug-repros/blob/master/images/bug1_folder_layout_2.png)

At runtime, `app` performs the following steps:

1. Load `app\bin\Debug\lib2.dll` using `Assembly.LoadFrom(...)` and access `Assembly.DefinedTypes` on the loaded assembly. It is expected that this will result in a `ReflectionTypeLoadException` because `MyClass` implements `IBaseInterface`, which is defined in an assembly that is not loaded, `lib1`.
2. Create an instance of `MyClass` from `lib2`. It is expect that this will fail with a `FileNotFoundException` because `lib1` can't be found.
3. Hook the `AppDomain.CurrentDomain.AssemblyResolve` event with a handler that loads and returns the assembly, `lib1`, if requested.
4. Try step 1 again. This time, the `AssemblyResolve` handler should be called and successfully resolve `lib1`. So, no `ReflectionTypeLoadException` is thrown.
5. Try step 2 again. Because `lib1` is now successfully loaded, no exception is thrown and the instance is created.

## Repro

1. `git clone https://github.com/DustinCampbell/bug-repros.git`
2. Open `bug-repros.sln` in Visual Studio 2017 on Windows
3. Set `bug1\app` as the start up project
4. <kbd>Ctrl+F5</kbd> to run without the debugger

The output of the program should be:

```
Before AssemblyResolve hooked
SUCCESS(LoadLib2): Caught System.Reflection.ReflectionTypeLoadException
SUCCESS(CreateClass): Caught System.IO.FileNotFoundException

After AssemblyResolve hooked
SUCCESS(LoadLib2): No System.Reflection.ReflectionTypeLoadException thrown
SUCCESS(CreateClass): No System.IO.FileNotFoundException thrown
```

Now, try the same on macOS with Visual Studio for Mac.

```
Before AssemblyResolve hooked
FAIL(LoadLib2): Expected to catch System.Reflection.ReflectionTypeLoadException, but no excpetion was thrown
FAIL(CreateClass): No System.IO.FileNotFoundException thrown, caught System.TypeLoadException instead

After AssemblyResolve hooked
SUCCESS(LoadLib2): No System.Reflection.ReflectionTypeLoadException thrown
FAIL(CreateClass): Did not expect any exception, but caught System.TypeLoadException
```

## Problems

There are several issues highlighted by this sample:

1. Touching `System.Assembly.DefinedTypes` throws a `System.Reflection.ReflectionTypeLoadException` on .NET Framework. On Mono, this exception will not be thrown until the `System.Assembly.DefinedTypes` is enumerated.
2. Creating an instance of a class that implements an interface defined in an assembly that cannot be found throws a `System.IO.FileNotFoundException` on .NET Framework, and a `System.TypeLoadException` on Mono.
3. On Mono, the `AssemblyResolve` event will not be triggered for a library if it has already failed to load. You can verify this by commenting out [lines 11-13](https://github.com/DustinCampbell/bug-repros/blob/master/bug1/app/Program.cs#L11-L13) in `but1\app\Program.cs` and running again.
