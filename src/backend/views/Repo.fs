module WillMyMps.Work.Backend.Views.Repo

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

let private notFound = setStatusCode 404 >=> html ""
let repoHandler repoName : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            return! notFound next ctx
        }