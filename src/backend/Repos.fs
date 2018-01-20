module WillMyMps.Work.Backend.Repos

open System

open Model

let repos =  [
    {
         Name = "mebddr.core"
         Url = Uri("https://github.com/mbeddr/mbeddr.core")
         Branches = All
         DefaultBranch = "master"
         DependencyManager = gradle "build.gradle" "ext.mpsBuild"
     }
]