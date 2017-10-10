The repro is intended to highlight differences in behavior between the .NET Framework on Windows and Mono with regard to
AppDomains, hooking the AssemblyResolve event, and Assembly.LoadFrom(...).

1. `git clone https://github.com/DustinCampbell/bug-repros.git`
2. Open `bug-repros.sln` in Visual Studio 2017 on Windows
3. Set `bug\app` as the start up project
4. <kbd>F5</kbd> to debug

The output of the program should be:

```
Before AssemblyResolve hooked
EXPECTED: ReflectionTypeLoadException.
EXPECTED: FileNotFoundException.

After AssemblyResolve hooked
EXPECTED: no ReflectionTypeLoadException.
EXPECTED: no FileNotFoundException.
```

Now, try the same on macOS with Visual Studio for Mac.

```
```
