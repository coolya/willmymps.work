module WillMyMps.Work.Backend.Views.Index

open FSharp.Core

open System.Threading.Tasks

open Microsoft.AspNetCore.Http

open Giraffe
open GiraffeViewEngine
open Giraffe.Tasks
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions

module R = Giraffe.Fable.Compat.Helper

open Giraffe.Fable.Compat.Helper.Props

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
    R.div [ ClassName "repo" ] [
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
        let repoPart =
            model |>
            List.map repoPart |>
            List.zip (all |> List.ofArray) |>
            List.map (fun (x, y) -> y x)

        let header =
            R.div [ClassName "header"] [
                R.span[ClassName "mps-logo"][]
                R.p[ClassName "claim"] [
                    R.str "Will my "
                    R.strong[] [R.str "MPS"]
                    R.str " work?"
                ]
                R.p [][
                    R.str "This website shows you open source repositories and which MPS version they are using. Click one of the repositories for more infos e.g. which version is used on other branches."
                ]
            ]
        let about =
            R.div [ClassName "about"] [
                R.p [][
                    R.str "willmymps.work is build as a fun project. It tried to figure out which MPS a open source project is using. At the moment it only support a pretty simple approach of parsing a gradle file."
                    R.str "In the future I would like to build more robust ways to figure out the MPS version. One idea is to parse the model files and check their dependencies to certain MPS languages."
                    R.str "If you would like to add your project head over to the "
                    R.a[Href "https://github.com/coolya/willmymps.work"] [R.str "repository"]
                    R.str " and sent a pull requst with your repository information."
                ]
            ]
        return header :: repoPart @ [about]

    }


let indexHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! content = indexView repos
                return! ctx.RenderHtmlAsync(layout content)
            }
