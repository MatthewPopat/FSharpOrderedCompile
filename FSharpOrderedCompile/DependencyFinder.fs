module DependencyFinder

open StringUtils
open System.Text.RegularExpressions

let rec private removeComments target =
    let rec ignoringUntilEndComment target =
        match target with
        | Prefix "*)" rest -> rest
        | "" -> ""
        | FirstChar(_, rest) -> ignoringUntilEndComment rest
    let rec ignoringUntilEndOfLine target =
        match target with
        | Prefix "\n" _ -> target
        | Prefix "\r\n" _ -> target
        | "" -> ""
        | FirstChar(_, rest) -> ignoringUntilEndOfLine rest
    match target with
    | Prefix "//" rest -> rest |> ignoringUntilEndOfLine |> removeComments
    | Prefix "(*" rest -> rest |> ignoringUntilEndComment |> removeComments
    | "" -> ""
    | FirstChar(first, rest) -> first + removeComments rest
    
let private isCodeLine line =
    not (isNullOrWhiteSpace line || line |> trim |> startsWith "#")

let private splitLines target =
    target
    |> removeComments
    |> split [|"\r\n"; "\r"; "\n"|]
    |> Array.toList
    |> List.filter isCodeLine

let private getModuleName line =
    let equalsIndex = (line |> indexOf "=")
    let trimmedLine =
        match equalsIndex with
        | x when x < 0 -> line |> trim
        | _ -> line |> before equalsIndex |> trim
    match trimmedLine with
    | Prefix "public " rest -> rest |> trim
    | Prefix "internal " rest -> rest |> trim
    | Prefix "private " rest -> rest |> trim
    | _ -> trimmedLine

let private startsAfter position s =
    s |> startsWith (" " |> String.replicate position)

let private (|StartsBefore|_|) position s =
    match s |> startsAfter position with
    | true -> None
    | false -> Some()

let private leadingSpaces s =
    (s |> length) - (s |> trimStart |> length)

let private getModuleIndent firstLine remainingLines =
    match firstLine with
    | SuffixTrimmed "=" _ ->
        match remainingLines with
        | [] -> firstLine |> length
        | nextLine::_ -> leadingSpaces nextLine
    | _ ->
        let afterEqualsIndex = (firstLine |> indexOf "=") + 1
        let spacesAfterEquals = firstLine |> substring afterEqualsIndex |> leadingSpaces
        afterEqualsIndex + spacesAfterEquals

let private appearsInLine line name =
    Regex.IsMatch(line, sprintf "^(.*[^\\w.])?%s(\\W.*)?$" name)

let private orDefault defaultValue opt =
    defaultArg opt defaultValue

let private refineNames opened names =
    let openedClean = opened + "." |> trim
    names
    |> List.tryPick (fun name ->
        match name with
        | Prefix openedClean remainingName -> Some (remainingName::names)
        | _ -> None)
    |> orDefault names

let private appearsIn dependorLines moduleOrType =
    let refineContext firstLine remainingLines indent names outerContext =
        match firstLine with
        | StartsBefore indent -> outerContext
        | PrefixTrimmed "open " rest -> (indent, names |> refineNames rest)::outerContext
        | PrefixTrimmed "module " _ ->
            ((getModuleIndent firstLine remainingLines), names)::(indent, names)::outerContext
        | PrefixTrimmed "namespace " rest -> [(0, [moduleOrType] |> refineNames rest)]
        | _ -> (indent, names)::outerContext
    let rec contextAppearsIn dependorLines context =
        match context with
        | [] -> failwith "no root context provided"
        | (indent, names)::outerContext ->
            match dependorLines with
            | [] -> false
            | firstLine::remainingLines ->
                names |> List.exists (appearsInLine firstLine) ||
                outerContext
                |> refineContext firstLine remainingLines indent names
                |> contextAppearsIn remainingLines
    [(0, [moduleOrType])] |> contextAppearsIn dependorLines

let rec private splitNamespaces dependeeLines =
    match dependeeLines with
    | [] -> []
    | firstLine::remainingLines ->
        match firstLine |> trim with
        | Prefix "namespace " rest ->
            let notNamespace l = not (l |> trim |> startsWith "namespace ")
            let lines = remainingLines |> List.takeWhile notNamespace
            (rest |> trim, lines)::splitNamespaces (remainingLines |> List.skipWhile notNamespace)
        | _ -> invalidArg "dependeeLines" "must start with a namespace"

let private (|Prefices|_|) ps s =
    ps |> List.tryPick (fun p -> (|Prefix|_|) p s)

let rec private moduleAndTypeNames dependeeLines =
    match dependeeLines with
    | [] -> []
    | firstLine::remainingLines ->
        match firstLine |> trim with
        | Prefices ["module "; "type "] rest ->
            let indent = getModuleIndent firstLine remainingLines
            (rest |> getModuleName)::moduleAndTypeNames (remainingLines |> List.skipWhile (startsAfter indent))
        | _ -> moduleAndTypeNames remainingLines

let HasDependency dependor dependee dependeeFileName =
    let dependeeLines = dependee |> splitLines
    let dependorLines = dependor |> splitLines
    match dependeeLines with
    | [] -> false
    | firstLine::_ ->
        match firstLine |> trim with
        | Prefix "module " rest -> rest |> getModuleName |> appearsIn dependorLines 
        | Prefix "namespace " _ ->
            dependeeLines
            |> splitNamespaces
            |> List.collect (fun (namespaceName, lines) ->
                lines
                |> moduleAndTypeNames 
                |> List.map (fun name -> namespaceName + "." + name))
            |> List.exists (appearsIn dependorLines)
        | _ -> dependeeFileName |> appearsIn dependorLines