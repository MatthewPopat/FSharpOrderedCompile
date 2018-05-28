module GraphOperations

let private mapSnd f =
    List.map (fun (t1, t2) -> (t1, f t2))

let TopologicalSort nodes edges =
    let rec fromGroups groups =
        match groups with
        | [] -> Some []
        | _ ->
            groups
            |> List.tryFind (snd >> List.isEmpty)
            |> Option.bind (fun (node, _) ->
                groups
                |> List.filter (fun (gn, _) -> node <> gn)
                |> mapSnd (List.filter (fun dep -> node <> dep))
                |> fromGroups
                |> Option.map (fun rest -> node::rest))
    edges
    |> List.groupBy fst
    |> mapSnd (List.map snd)
    |> (fun groups ->
        let empties = 
            nodes
            |> List.filter (fun n -> groups |> List.forall (fun (gn, _) -> n <> gn))
            |> List.map (fun n -> (n, []))
        groups @ empties)
    |> fromGroups