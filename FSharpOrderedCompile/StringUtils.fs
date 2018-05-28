module StringUtils

open System

let startsWith prefix (target: string) =
    target.StartsWith prefix

let endsWith suffix (target: string) =
    target.EndsWith suffix
    
let split (strings: string[]) (target: string) =
    target.Split(strings, StringSplitOptions.RemoveEmptyEntries)

let containsString pattern (target: string) =
    target.Contains(pattern)

let trim (target: string) =
    target.Trim()

let trimStart (target: string) =
    target.TrimStart()

let substring startIndex (target: string) =
    target.Substring(startIndex)
    
let before endIndex (target: string) =
    target.Substring(0, endIndex)

let isNullOrWhiteSpace target =
    String.IsNullOrWhiteSpace target

let length (target:string) =
    target.Length

let indexOf (pattern: string) (target:string) =
    target.IndexOf pattern

let (|Prefix|_|) p s =
    if s |> startsWith p then
        Some(s |> substring (p |> length))
    else
        None

let (|PrefixTrimmed|_|) p s =
    (|Prefix|_|) p (s |> trim)

let (|Suffix|_|) p s =
    if s |> endsWith p then
        Some(s |> substring (p |> length))
    else
        None

let (|SuffixTrimmed|_|) p s =
    (|Suffix|_|) p (s |> trim)

let (|FirstChar|) s =
    if s |> length > 0 then
        (s.[0..0], s.[1..])
    else
        ("", "")
