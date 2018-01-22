module WillMyMps.Work.Backend.Github

open System
open System.IO
open System.Net.Http


open FSharp.Core

open Giraffe.Tasks

open Model
open WillMyMps.Work.Shared.Data

let getFilePath repo =
    repo.DependencyManager |> function
    | Gradle(file, _) -> file

let parseVerionLine repo line =
    match repo.DependencyManager with
    | Gradle _ ->
        line |> String.split '=' |> Array.tryLast |> Option.bind (String.trim >> parseMpsVersion)


let parseFileContent repo (content: Stream) =
    let strToLookUp =
        repo.DependencyManager |> function
        | Gradle(_, var) -> var

    let rec getVersionLine (reader: TextReader) =
        task {
            let! next = reader.ReadLineAsync()
            match next |> String.contains strToLookUp with
            | true -> return next
            | _ -> return! getVersionLine reader
        }
    task {
        use reader = new StreamReader(content)
        let! versionStr =  getVersionLine reader
        return versionStr |> parseVerionLine repo
    }

let getMpsVersion repo branch =
    task {
        let path = getFilePath repo
        let prjPath (url: Uri) =
            let len = url.Segments |> Array.length
            match len with
            | x when x >= 3 -> url.Segments.[1] + url.Segments.[2]
            | _ -> failwithf "Uri (%A) for repo: %s is invalid" url repo.Name

        let url = sprintf "https://raw.githubusercontent.com/%s/%s/%s" (prjPath repo.Url) branch path
        use client = new HttpClient()
        let! res = client.GetAsync(url)
        if res.IsSuccessStatusCode then
            let! content = res.Content.ReadAsStreamAsync()
            let! version = parseFileContent repo content
            let res =
                match version with
                    | Some v -> Ok v
                    | _ -> Error "could not parse version string"
            return res
        else
            return Error (sprintf "got error requesting file from github %A" res.StatusCode)
    }

let getMpsVersionDefault repo = getMpsVersion repo repo.DefaultBranch