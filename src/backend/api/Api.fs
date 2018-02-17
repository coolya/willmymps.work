module WillMyMps.Work.Backend.Api.Main

open Giraffe

open WillMyMps.Work.Backend.Api.Branches

let apiRouter: HttpHandler =
    choose [
            routef "/branches/%s" getBranches
            ]