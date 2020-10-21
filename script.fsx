#r "nuget: Deedle"

let aFloat = 1.10
let asObject = (aFloat :> obj)

asObject.GetType()

open Deedle

let seriesA = Series.ofValues [ 1; 2; 3; 4; 5; 6]
let seriesB = Series.ofValues [ 10; 12; 14]

let frame = Frame.ofColumns([ "A", seriesA; "B", seriesB ])

let doAsFrameOp (f : IFrame) =
    { new IFrameOperation<_> with
        member x.Invoke(f) =
            let columnTypes = f.ColumnTypes
            columnTypes }
    |> f.Apply

frame
|> doAsFrameOp

let (|SeriesValues|_|) (value : obj) = 
    let iser = value.GetType().GetInterface("ISeries`1")
    if iser <> null then 
        let keys = 
            value.GetType().GetProperty("Keys").GetValue(value) :?> System.Collections.IEnumerable
        let vector = value.GetType().GetProperty("Vector").GetValue(value) :?> IVector
        Some(Seq.zip (Seq.cast<obj> keys) vector.ObjectSequence)
    else None

open System

/// Super opinionated because I know how the pattern looks like
/// Don't trust me on this
let peekAtSeriesTypes (s: ((obj * OptionalValue<obj>) seq)) =
    if Seq.isEmpty s then
        None
    else
        let (k, v) = Seq.head s
        (k.GetType(), (v.ValueOrDefault.GetType()))
        |> Some

match seriesA with
| SeriesValues s ->
    let (k, v) = Seq.head s
    printfn "Key type: %A Value type: %A" (k.GetType()) (v.GetType())

    let blub = v.ValueOrDefault
    printfn "Value type more detailed %A" (v.ValueOrDefault.GetType())
| _ -> printfn "womp womp"

let castToWhateverSeries (s: Series<_, _>) =
    s :> ISeries<_>

let seriesC =
    [1..10]
    |> Series.ofValues
    |> castToWhateverSeries

let seriesD =
    ["1"; "2"; "3"]
    |> Series.ofValues
    |> castToWhateverSeries

let someFrame =
    [ "C", seriesC; "D", seriesD ]
    |> Frame.ofColumns

let blub = ResizeArray<obj>()
blub.Add(10)
blub.Add("bommel")
blub.Add([10; 12; 15])

let flights = Frame.ReadCsv("/home/gregor/source/repos/FSharpForDataScience/datasets/nycflights13/flights.csv")
let originSeries = flights.GetColumn<string>("carrier")