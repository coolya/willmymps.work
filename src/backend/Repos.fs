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
     };
    {
         Name = "iets3.opensource"
         Url = Uri("https://github.com/IETS3/iets3.opensource")
         Branches = All
         DefaultBranch = "master"
         DependencyManager = gradle "build.gradle" "ext.mpsVersion"
     }
]
