
module WillMyMps.Work.Backend.Api.Branches

open Microsoft.AspNetCore.Http

open Giraffe

open WillMyMps.Work.Backend.Repos
open WillMyMps.Work.Backend.Github


let private notFound = setStatusCode 404 >=> json []
let getBranches repoName : HttpHandler =

    let getBranches repo =
        task {
            let! branches = fetchBranches repo
            return
                branches
                |> Option.map
                    (List.map (fun x -> x.Name) >> json)
                |> Option.defaultValue notFound
        }

    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! response =
                repos
                |> List.tryFind (fun x -> x.Name = repoName)
                |> Option.map getBranches
                |> Option.defaultValue (task { return notFound })
            return! response next ctx
        }