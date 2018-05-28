namespace IntegrationTest

 module Say =  let hello name =
                 printfn "Hello %s" name
               module Say2 =
                   let hello name =
                       printfn "Hello %s" name

namespace SecondNamespace

module Say3 =
     module Say4 =
        let hello name =
            printfn "Hello %s" name