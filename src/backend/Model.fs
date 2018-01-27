module WillMyMps.Work.Backend.Model

open System

type Branches =
    | All
    | DefaultOnly
    | Additional of string list


type DependencyManager =
    | Gradle of path:string * var:string


let gradle path var = Gradle(path, var)

type Repository =
    {
        Url: Uri
        Name: string
        Branches: Branches
        DefaultBranch: string
        DependencyManager: DependencyManager
    }

let extractInfo (uri: Uri) =
    match uri.Segments |> Array.length with
    | x when x >= 3 -> Some(uri.Segments.[1], uri.Segments.[2])
    | _ -> None