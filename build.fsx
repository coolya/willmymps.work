// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open System
open System.IO

let dotnetcliVersion = "2.1.2"
let mutable dotnetExePath = "dotnet"
let buildDir = "./bin/"


let run' timeout cmd args dir =
    if execProcess (fun info ->
        info.FileName <- cmd
        if not (String.IsNullOrWhiteSpace dir) then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) timeout |> not then
        failwithf "Error while running '%s' with args: %s" cmd args

let run = run' System.TimeSpan.MaxValue

let runDotnet workingDir args =
    let result =
        ExecProcess (fun info ->
            info.FileName <- dotnetExePath
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args


let runDocker' workingDir action args =
    let result =
        ExecProcess(fun info ->
            info.FileName <- "docker"
            info.WorkingDirectory <- workingDir
            info.Arguments <- sprintf "%s %s" action args
        ) TimeSpan.MaxValue
    if result <> 0 then failwithf "docker %s %s failed" action args

let runDocker = runDocker' "."

let platformTool tool winTool =
    let tool = if isUnix then tool else winTool
    tool
    |> ProcessHelper.tryFindFileOnPath
    |> function Some t -> t | _ -> failwithf "%s not found" tool


let npmTool = platformTool "npm" "npm.cmd"

let branch = environVarOrNone "CI_COMMIT_REF_NAME" |> function
    | Some s -> s
    | _ -> "master"

let version =
    match branch with
    | "master" -> environVarOrNone "CI_COMMIT_SHA" |> Option.defaultValue "dev"
    | _ -> "dev"


let cleanProject path =
    let bin = Path.Combine(path, "bin")
    let obj = Path.Combine(path, "obj")
    CleanDirs [bin; obj]



Target "Clean" (fun _ ->
    cleanProject "./src/frontend"
    cleanProject "./src/backend"
)

Target "InstallDotNetCore" (fun _ ->
    dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "NpmInstall" (fun _ ->
    run npmTool "install" "./"
)

Target "Install" (fun _ ->
    runDotnet "./src/frontend" "restore"
    runDotnet "./src/backend" "restore"
)

Target "BuildFrontend" (fun _ ->
    runDotnet "./src/frontend" "fable npm-run build -p"
)

Target "BuildBackend" (fun _ ->
    runDotnet "./src/backend" "build"
)


let ipAddress = "localhost"
let port = 5000

Target "run" (fun _ ->
    runDotnet "./src/frontend" "restore"
    runDotnet "./src/backend" "restore"

    let unitTestsWatch = async {
        let result =
            ExecProcess (fun info ->
                info.FileName <- dotnetExePath
                info.WorkingDirectory <- "./src/backend"
                info.Arguments <- sprintf "watch run") TimeSpan.MaxValue

        if result <> 0 then failwith "Website shut down." }

    let fablewatch = async { runDotnet "./src/frontend" "watch fable npm-run build -p" }
    let openBrowser = async {
        System.Threading.Thread.Sleep(15000)
        Diagnostics.Process.Start("http://"+ ipAddress + sprintf ":%d" port) |> ignore }

    Async.Parallel [| unitTestsWatch; fablewatch; openBrowser |]
    |> Async.RunSynchronously
    |> ignore
)


Target "Build" DoNothing
Target "Rebuild" DoNothing


"InstallDotNetCore"
  ==> "NpmInstall"
  ==> "Install"
  ==> "BuildFrontend"
  ==> "Build"
  ==> "Run"

"Clean"
  ==> "Rebuild"
"Build"
  ==> "Rebuild"

"BuildBackend"
  ==> "Build"



RunTargetOrDefault "Build"