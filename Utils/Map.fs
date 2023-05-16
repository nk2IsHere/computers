namespace Computers.Utils

module Map =
    let inline toMap dictionary =
        dictionary
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq
        
    let join (q: Map<'a,'b>) (p: Map<'a,'b>) = 
        Map(Seq.concat [ (Map.toSeq p) ; (Map.toSeq q) ])
        
    let joinSeq (q: seq<'a * 'b>) (p: Map<'a,'b>) = 
        Map(Seq.concat [ (Map.toSeq p) ; q ])
