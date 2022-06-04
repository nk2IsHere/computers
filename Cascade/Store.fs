namespace Computers.Cascade
 
type Reducer<'s, 'a> = 's -> 'a -> 's

type Dispatcher<'s, 'a> = 'a -> 's

type Middleware<'s, 'a> = 's -> Dispatcher<'s, 'a> -> Dispatcher<'s, 'a>

type Store<'s, 'a> =
    {
        reducers: Reducer<'s, 'a> list
        middlewares: Middleware<'s, 'a> list
        state: 's
    }
    with
        member private this.reducer: Reducer<'s, 'a> =
            fun state action ->
                this.reducers
                |> List.fold (fun state reducer -> reducer state action) state
                
        member private this.rootDispatcher: Dispatcher<'s, 'a> =
            fun action ->
                this.reducer this.state action
            
        member private this.combinedDispatcher: Dispatcher<'s, 'a> =
            (this.middlewares, this.rootDispatcher)
            ||> List.foldBack (fun middleware -> middleware this.state)
                
        member this.Dispatch =
            this.combinedDispatcher

module Store =
    let logger<'s, 'a> (tag: string) (outputSink: string -> unit): Middleware<'s, 'a> =
        fun oldState next ->
            fun action ->
                let newState = next action
                do outputSink $"[{tag}/{action}] {oldState} -> {newState}"
                newState
