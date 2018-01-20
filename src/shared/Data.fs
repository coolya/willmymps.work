module WillMyMps.Work.Shared.Data

open FSharp.Core

open Helper
open Helper.TryParser

type MpsVersion =
    | Release of int * int * int
    // MPS-2017.3.3-EAP
    | Eap of int * int * int
    // "2017.3-RC2"
    | ReleaseCandidate of int * int * int
    with member x.ToString =
            match x with
                | Release(x , y, 0) -> sprintf "%d.%d" x y
                | Release(x, y, z) -> sprintf "%d.%d.%d" x y z
                | Eap(x, y, 0) -> sprintf "%d.%d-EAP" x y
                | Eap(x, y, z) -> sprintf "%d.%d-EAP%d" x y z
                | ReleaseCandidate(x, y, 0) -> sprintf "%d.%d-RC" x y
                | ReleaseCandidate(x, y, z) -> sprintf "%d.%d-RC%d" x y z



let parseMpsVersion str : MpsVersion option =
    match str with
    | Regex @"([0-9]{4})[.]([0-9]{1})[.]([0-9]{1})?" l ->
        match l |> List.choose parseInt with
        | [x; y; z] -> Some(Release(x, y, z))
        | [x; y] -> Some(Release(x, y, 0))
        | _ -> None
    | Regex @"([0-9]{4})[.]([0-9]{1})-EAP([0-9]{1})?" l ->
        match l |> List.choose parseInt with
        | [x; y; z] -> Some(Eap(x, y, z))
        | [x; y] -> Some(Eap(x, y, 0))
        | _ -> None
    | Regex @"([0-9]{4})[.]([0-9]{1})-RC([0-9]{1})?" l ->
        match l |> List.choose parseInt with
        | [x; y; z] -> Some(Eap(x, y, z))
        | [x; y] -> Some(Eap(x, y, 0))
        | _ -> None
    | _ -> None



let mpsUrl (version: MpsVersion) =
    let firstPart =
        version
        |> function
            | Release(x, y, _)
            | Eap(x, y, _)
            | ReleaseCandidate(x, y, _) -> sprintf "%d.%d" x y

    sprintf "http://download.jetbrains.com/mps/%s/MPS-%A.zip" firstPart version