namespace Computers.Cascade
/// From: https://github.com/swlaschin/Railway-Oriented-Programming-Example/blob/master/src/FsRopExample/Rop.fs

/// A Result is a success or failure
/// The Success case has a success value, plus a list of messages
/// The Failure case has just a list of messages
type RailwayResult<'TSuccess, 'TMessage> =
    | Success of 'TSuccess * 'TMessage list
    | Failure of 'TMessage list

module Railway =
    /// create a Success with no messages
    let succeed: 'TSuccess -> RailwayResult<'TSuccess, 'TMessage> =
        fun x ->
            Success (x, [])

    /// create a Success with a message
    let succeedWithMessage: 'TSuccess -> 'TMessage -> RailwayResult<'TSuccess, 'TMessage> =
        fun x message ->
            Success (x, [message])

    /// create a Failure with a message
    let fail: 'TMessage -> RailwayResult<'TSuccess, 'TMessage> =
        fun message ->
            Failure [message]

    /// A function that applies either fSuccess or fFailure 
    /// depending on the case.
    let either: ('TSuccess * 'TMessage list -> 'TResult) -> ('TMessage list -> 'TResult) -> RailwayResult<'TSuccess, 'TMessage> -> 'TResult =
        fun fSuccess fFailure result ->
            match result with
            | Success (x, messages) -> fSuccess (x, messages)
            | Failure errors -> fFailure errors

    /// merge messages with a result
    let mergeMessages: 'TMessage list -> RailwayResult<'TSuccess,'TMessage> -> RailwayResult<'TSuccess,'TMessage> =
        fun messages result ->
            let fSuccess (x, messages2) = Success (x, messages @ messages2)
            let fFailure errors = Failure (errors @ messages)
            
            either fSuccess fFailure result

    /// given a function that generates a new RailwayResult
    /// apply it only if the result is on the Success branch
    /// merge any existing messages with the new result
    let bindR: ('a -> RailwayResult<'TSuccess,'TMessage>) -> RailwayResult<'a, 'TMessage> -> RailwayResult<'TSuccess, 'TMessage> =
        fun f result ->
            let fSuccess (x, messages) = f x |> mergeMessages messages
            let fFailure errors = Failure errors
            
            either fSuccess fFailure result

    /// given a function wrapped in a result
    /// and a value wrapped in a result
    /// apply the function to the value only if both are Success
    let applyR: RailwayResult<'a -> 'b,'c> -> RailwayResult<'a,'c> -> RailwayResult<'b,'c> =
        fun f result ->
            match f, result with
            | Success (f, messages1), Success (x, messages2) -> (f x, messages1 @ messages2) |> Success
            | Failure errors, Success (_, messages)
            | Success (_, messages), Failure errors ->  errors @ messages |> Failure
            | Failure errors1, Failure errors2 -> errors1 @ errors2 |> Failure 

    /// infix version of apply
    let (<*>) = applyR

    /// given a function that transforms a value
    /// apply it only if the result is on the Success branch
    let liftR: ('TSuccessA -> 'TSuccessB) -> RailwayResult<'TSuccessA,'TMessage> -> RailwayResult<'TSuccessB,'TMessage> =
        fun f result ->
            let f' =  f |> succeed
            applyR f' result 

    /// given two values wrapped in results apply a function to both
    let lift2R: ('TSuccessA -> 'TSuccessB -> 'TSuccessC) -> RailwayResult<'TSuccessA,'TMessage> -> RailwayResult<'TSuccessB,'TMessage> -> RailwayResult<'TSuccessC,'TMessage> =
        fun f result1 result2 ->
        let f' = liftR f result1
        applyR f' result2 

    /// given three values wrapped in results apply a function to all
    let lift3R: ('TSuccessA -> 'TSuccessB -> 'TSuccessC -> 'TSuccessD) -> RailwayResult<'TSuccessA,'TMessage> -> RailwayResult<'TSuccessB,'TMessage> -> RailwayResult<'TSuccessC,'TMessage> -> RailwayResult<'TSuccessD,'TMessage> =
        fun f result1 result2 result3 ->
            let f' = lift2R f result1 result2 
            applyR f' result3

    /// given four values wrapped in results apply a function to all
    let lift4R: ('TSuccessA -> 'TSuccessB -> 'TSuccessC -> 'TSuccessD -> 'TSuccessE) -> RailwayResult<'TSuccessA,'TMessage> -> RailwayResult<'TSuccessB,'TMessage> -> RailwayResult<'TSuccessC,'TMessage> -> RailwayResult<'TSuccessD,'TMessage> -> RailwayResult<'TSuccessE,'TMessage> =
        fun f result1 result2 result3 result4 ->
            let f' = lift3R f result1 result2 result3 
            applyR f' result4

    /// infix version of liftR
    let (<!>) = liftR

    /// synonym for liftR
    let mapR = liftR

    /// given an RailwayResult, call a unit function on the success branch
    /// and pass thru the result
    let successTee: ('TSuccess * 'TMessage list -> unit) -> RailwayResult<'TSuccess, 'TMessage> -> RailwayResult<'TSuccess, 'TMessage> =
        fun f result -> 
            let fSuccess (x, messages) = 
                do f (x, messages)
                Success (x, messages)
            
            let fFailure errors = Failure errors
            
            either fSuccess fFailure result

    /// given an RailwayResult, call a unit function on the failure branch
    /// and pass thru the result
    let failureTee: ('TMessage list -> unit) -> RailwayResult<'TSuccess, 'TMessage> -> RailwayResult<'TSuccess, 'TMessage> = 
        fun f result ->
            let fSuccess (x, messages) = Success (x, messages)
            
            let fFailure errors = 
                do f errors
                Failure errors
            
            either fSuccess fFailure result

    /// given an RailwayResult, map the messages to a different error type
    let mapMessagesR: ('TMessageFrom -> 'TMessageTo) -> RailwayResult<'TSuccess, 'TMessageFrom> -> RailwayResult<'TSuccess, 'TMessageTo> = 
        fun f result -> 
            match result with 
            | Success (x, messages) -> Success (x, messages |> List.map f)
            | Failure errors -> Failure (errors |> List.map f)

    /// given an RailwayResult, in the success case, return the value.
    /// In the failure case, determine the value to return by 
    /// applying a function to the errors in the failure case
    let valueOrDefault: ('TMessage list -> 'TSuccess) -> RailwayResult<'TSuccess, 'TMessage> -> 'TSuccess = 
        fun f result ->
            match result with 
            | Success (x, _) -> x
            | Failure errors -> f errors

    /// lift an option to a RailwayResult.
    /// Return Success if Some
    /// or the given message if None
    let failIfNone: 'TMessage -> 'TSuccess option -> RailwayResult<'TSuccess, 'TMessage> =
        fun message ->
            function
            | Some x -> succeed x
            | None -> fail message 

    /// given an RailwayResult option, return it
    /// or the given message if None
    let failIfNoneR: 'TMessage -> RailwayResult<'TSuccess, 'TMessage> option -> RailwayResult<'TSuccess, 'TMessage> =
        fun message ->
            function
            | Some rop -> rop
            | None -> fail message
    
    /// given a list of RailwayResults, return a RailwayResult of a list
    let sequenceR: RailwayResult<'TSuccess, 'TMessage> list -> RailwayResult<'TSuccess list, 'TMessage> =
        fun results ->
            List.foldBack
                (fun item acc ->
                    match item, acc with
                    | Success (x, messages), Success (xs, messages2) -> Success (x :: xs, messages @ messages2)
                    | Failure errors, Success (_, messages) -> Failure (errors @ messages)
                    | Success (_, messages), Failure errors -> Failure (errors @ messages)
                    | Failure errors1, Failure errors2 -> Failure (errors1 @ errors2)
                )
                results
                (Success ([], []))
    
    /// try a function and return a RailwayResult of the result
    let tryR: (unit -> 'TSuccess) -> RailwayResult<'TSuccess, string> =
        fun f ->
            try
                f () |> succeed
            with
            | ex -> fail (ex |> sprintf "%A")
    
    
    
    /// given two RailwayResults, combine them into a RailwayResult of a tuple
    let combineR: RailwayResult<'TSuccessA, 'TMessage> -> RailwayResult<'TSuccessB, 'TMessage> -> RailwayResult<'TSuccessA * 'TSuccessB, 'TMessage> =
        fun result1 result2 ->
            let f x y = (x, y)
            lift2R f result1 result2
    