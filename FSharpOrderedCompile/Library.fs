namespace FSharpOrderedCompile

open Microsoft.Build.Utilities
open Microsoft.Build.Framework
open System.IO
open DependencyFinder
open GraphOperations

type FSharpOrderedCompile() =
    inherit Task()

    member val Files: ITaskItem[] = Array.empty with get, set

    [<Output>]
    member val ToCompile: ITaskItem[] = Array.empty with get, set

    override _this.Execute() =
        let dependencies = _this.Files |> Array.collect (fun f1 ->
            _this.Files |> Array.choose (fun f2 ->
                match f1.ItemSpec = f2.ItemSpec with
                | true -> None
                | false ->
                    let f1Text = File.ReadAllText f1.ItemSpec
                    let f2Text = File.ReadAllText f2.ItemSpec
                    match HasDependency f1Text f2Text (Path.GetFileNameWithoutExtension f2.ItemSpec) with
                    | true ->
                        Some (f1.ItemSpec, f2.ItemSpec)
                    | false -> None)) |> Array.toList
        let allFiles = _this.Files |> Array.map (fun f -> f.ItemSpec) |> Array.toList
        match dependencies |> TopologicalSort allFiles with
        | None -> 
            _this.Log.LogError("Found dependency cycle")
            false
        | Some order -> 
            _this.ToCompile <- order |> List.map (fun itemSpec ->
                _this.Files |> Array.find (fun f -> f.ItemSpec = itemSpec)) |> List.toArray
            true