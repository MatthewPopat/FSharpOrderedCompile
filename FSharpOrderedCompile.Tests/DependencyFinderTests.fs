module DependencyFinderTests

open Xunit
open DependencyFinder

[<Fact>]
let ``Allows reference to top level module`` () =
    let dependor = """
module Dependor
open Dependee"""
    let dependee = """
module Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows reference to top level namespaced module`` () =
    let dependor = """
module Dependor
open Test.Dependee"""
    let dependee = """
module Test.Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows reference directly to module if namespace has been opened`` () =
    let dependor = """
module Dependor
open Test
open Dependee"""
    let dependee = """
module Test.Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Ignores namespaces opened in another module`` () =
    let dependor = """
module Dependor
module inner1 =
    open Test
module inner2 =
    open Dependee"""
    let dependee = """
module Test.Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.False(result)

[<Fact>]
let ``Allows fully qualified reference after openning namespace`` () =
    let dependor = """
module Dependor
open Test
open Test.Dependee"""
    let dependee = """
module Test.Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Ignores reference if wrong sequence of namspaces is opened`` () =
    let dependor = """
module Dependor
open Test.Inner
open Inner.Dependee"""
    let dependee = """
module Test.Inner.Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.False(result)

[<Fact>]
let ``Ignores reference if it matches a substring`` () =
    let dependor = """
module Dependor
open OtherDependee"""
    let dependee = """
module Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.False(result)

[<Fact>]
let ``Ignores reference if it is to a type in another namespace`` () =
    let dependor = """
module Dependor
open Other.Dependee"""
    let dependee = """
module Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.False(result)

[<Fact>]
let ``Allows reference to member of module`` () =
    let dependor = """
module Dependor
let y = Dependee.x + 1"""
    let dependee = """
module Dependee
let x = 1"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Theory>]
[<InlineData("public")>]
[<InlineData("private")>]
[<InlineData("internal")>]
let ``Allows reference to module with accessibility modifier`` modifier =
    let dependor = """
module Dependor
open Dependee"""
    let dependee = (sprintf "module %s Dependee" modifier)
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)
    
[<Fact>]
let ``Allows comment before module delaration`` () =
    let dependor = """
module Dependor
open Dependee"""
    let dependee = """
(* comment *) module Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows comment after module delaration`` () =
    let dependor = """
module Dependor
open Dependee"""
    let dependee = """
module Dependee // comment"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows reference to module inside namespace declaration`` () =
    let dependor = """
module Dependor
open Test.Dependee"""
    let dependee = """
namespace Test
module Dependee =
    let x = 1"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows reference to module inside nested namespace declaration`` () =
    let dependor = """
module Dependor
open Test.Inner.Dependee"""
    let dependee = """
namespace Test.Inner
module Dependee =
    let x = 1"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows reference to module inside second namespace declaration`` () =
    let dependor = """
module Dependor
open Second.Dependee2"""
    let dependee = """
namespace Test
module Dependee1 =
    let x = 1
namespace Second
module Dependee2 =
    let x = 2"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows reference within namespace`` () =
    let dependor = """
namespace Dependor
open Dependee"""
    let dependee = """
module Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Allows reference within namespace after openning containing namespace`` () =
    let dependor = """
namespace Dependor
open Test
open Dependee"""
    let dependee = """
module Test.Dependee"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)

[<Fact>]
let ``Ingores reference when containing namespace was opened in another namespace`` () =
    let dependor = """
namespace Dependor
open Test
namespace Second
open Dependee"""
    let dependee = """
namespace Test
module Dependee =
    let x = 1"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.False(result)

[<Fact>]
let ``Allows references to modules from the same namespace`` () =
    let dependor = """
namespace Test
open Dependee"""
    let dependee = """
namespace Test
module Dependee =
    let x = 1"""
    let result = HasDependency dependor dependee "Dependee"
    Assert.True(result)
