# FSharpOrderedCompile

FSharpOrderedCompile is an MSBuild task to automatically import and pre-order
F# files before they are compiled, removing the need to manually include
every file for compilation. It works by detecting dependencies between F# files
then topologically sorting the dependency graph.

[![Build status](https://ci.appveyor.com/api/projects/status/a4kbwgyhgoew5c0f/branch/master?svg=true)](https://ci.appveyor.com/project/MatthewPopat/fsharporderedcompile/branch/master)

## Installation

Simply add the nuget package
[FSharpOrderedCompile](https://www.nuget.org/packages/FSharpOrderedCompile/)
to your project and remove all
```xml
    <ItemGroup>
        <Compile Include="filename.fs" />
    </ItemGroup>
```
lines from your project. It should now automatically include any `.fs` files
within that project's folder structure.

## Known issues

In F#, if two namespaces are `open`ed that contain the same type or module name
then the compiler uses which ever one contains the referenced members.
This project cannot detect this situation and will assume that both are used.
i.e. This project will say there is a dependency between these two files when
in fact there is not:
```fsharp
    namespace MyStrings
    module String =
        let empty = ""
```
```fsharp
    namespace UsingStrings
    open MyStrings
    module UsesString =
        let x = String.replicate 2 " "
```

IntelliSense for both VSCode and Visual Studio can be slow to update when new
dependencies are added and will not include suggestions for items that are not
yet referenced.

If you have solutions to these problems or any other improvements then pull
requests are welcome.

## License

This project can be used and distributed under the [MIT License](LICENSE).