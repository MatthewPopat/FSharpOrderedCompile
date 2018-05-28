module GraphOperationsTests

open Xunit
open GraphOperations

[<Fact>]
let ``Puts dependee before dependor`` () =
    let result = TopologicalSort [1; 2] [(1, 2)]
    Assert.Equal(Some [2; 1], result)

[<Fact>]
let ``Puts nested dependencies in order`` () =
    let result = TopologicalSort [1; 2; 3] [(1, 2); (2, 3)]
    Assert.Equal(Some [3; 2; 1], result)

[<Fact>]
let ``Allows dependency on grandchild`` () =
    let result = TopologicalSort [1; 2; 3] [(1, 2); (2, 3); (1, 3)]
    Assert.Equal(Some [3; 2; 1], result)

[<Fact>]
let ``Returns None if there are mutual dependencies`` () =
    let result = TopologicalSort [1; 2] [(1, 2); (2, 1)]
    Assert.Equal(None, result)

[<Fact>]
let ``Returns None if there is a dependency three cycle`` () =
    let result = TopologicalSort [1; 2; 3] [(1, 2); (2, 3); (3, 1)]
    Assert.Equal(None, result)

[<Fact>]
let ``Includes orphanned nodes`` () =
    let result = TopologicalSort [1; 2; 3] [(1, 2)]
    Assert.True(result = Some [3; 2; 1] || result = Some [2; 1; 3])

[<Fact>]
let ``If two nodes depend on the same node puts that node first`` () =
    let result = TopologicalSort [1; 2; 3] [(1, 3); (2, 3)]
    Assert.True(result = Some [3; 2; 1] || result = Some [3; 1; 2])

[<Fact>]
let ``If a nodes depends on two nodes puts that node last`` () =
    let result = TopologicalSort [1; 2; 3] [(1, 2); (1, 3)]
    Assert.True(result = Some [3; 2; 1] || result = Some [2; 3; 1])