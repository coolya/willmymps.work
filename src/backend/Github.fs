module WillMyMps.Work.Backend.Github

open System
open System.IO
open System.Net.Http

open Octokit

open FSharp.Core

open Giraffe

open Model
open WillMyMps.Work.Shared.Data
open WillMyMps.Work.Shared.Logging
open WillMyMps.Work.Backend.Logger.Console

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
            let! current = reader.ReadLineAsync()

            match current |> Option.ofObj |> Option.map (String.contains strToLookUp) with
            | Some true -> return Some current
            | Some false -> return! getVersionLine reader
            | _ ->
                error "can't find variable" |> logConsole
                return None
        }
    task {
        use reader = new StreamReader(content)
        let! versionStr =  getVersionLine reader
        return versionStr |> Option.bind (parseVerionLine repo)
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

let fetchBranches repo =
    task {
        match extractInfo repo.Url with
        | Some(owner, repo) ->
            let client = GitHubClient(ProductHeaderValue("willmymps.work"))
            let! branches = client.Repository.Branch.GetAll(owner, repo)
            return Some (branches |> List.ofSeq)
        | _ -> return None
    }

