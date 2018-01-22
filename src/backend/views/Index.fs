module WillMyMps.Work.Backend.Views.Index

open FSharp.Core

open System.Threading.Tasks

open Microsoft.AspNetCore.Http

open Giraffe
open GiraffeViewEngine
open Giraffe.Tasks
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions

module R = Giraffe.Fable.Compat.Helper.Props

open WillMyMps.Work.Backend.Github
open WillMyMps.Work.Backend.Model
open WillMyMps.Work.Backend.Repos
open WillMyMps.Work.Shared.Data

open Common

let repoPart repo =
    fun (x) ->
    let content =
        match x with
        | Error s ->
            R.div[] [
                R.h2 [] [R.str (sprintf "Failed to fetch MPS version: %s" s)]
            ]
        | Ok (v: MpsVersion) ->
            R.div[] [
                R.h2 [] [R.str "MPS Version"]
                R.h2 [] [R.str (sprintf "%s"(v.ToString))]
            ]
    R.div [ R.ClassName "repo" ] [
        R.h2 [][R.str repo.Name]
        R.div [][
            R.h3 [][ R.str "Branch"]
            R.h3 [] [ R.str repo.DefaultBranch ]
        ]
        content
    ]

let indexView (model : Repository list)  =
    task {
        let tasks = model |> List.map getMpsVersionDefault
        let! all = Task.WhenAll(tasks)
        return
            model |>
            List.map repoPart |>
            List.zip (all |> List.ofArray) |>
            List.map (fun (x, y) -> y x)
    }


let indexHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! content = indexView repos
                return! ctx.RenderHtmlAsync(layout content)
            }
