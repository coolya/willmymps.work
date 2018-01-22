
module WillMyMps.Work.Backend.Views.Common

open Giraffe
open GiraffeViewEngine


let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ encodedText "Will my MPS work?" ]
            link [ attr "rel"  "stylesheet"
                   attr "type" "text/css"
                   attr "href" "/main.css" ]
        ]
        body [] (( script [attr "src" "bundle.js"] [] ):: content)
    ]
