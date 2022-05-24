namespace Computers.Utils

module Map =
    let inline toMap dictionary =
        dictionary
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq