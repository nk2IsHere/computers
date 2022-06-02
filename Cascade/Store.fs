namespace Computers.Cascade
 
type Reducer<'s, 'a> = 's -> 'a -> 's

type Dispatcher<'a> = 'a -> 'a

type IStore<'s, 'a> =
    abstract member CurrentState: 's
    abstract member Dispatch: ('a -> 's)

type Middleware<'s, 'a> = IStore<'s, 'a> -> Dispatcher<'a> -> Dispatcher<'a>

type Store<'s, 'a>(reducers: Reducer<'s, 'a> list, middlewares: Middleware<'s, 'a> list, initialState: 's) =
    let mutable state = initialState
    
    let reducer: Reducer<'s, 'a> =
        fun state action ->
            reducers
            |> List.fold (fun state reducer -> reducer state action) state
    
    member private this.rootDispatcher: Dispatcher<'a> =
        fun action ->
            state <- reducer state action
            action
            
    member private this.dispatcher: Dispatcher<'a> =
        (middlewares, this.rootDispatcher)
        ||> List.foldBack (fun middleware -> middleware this)
            
    interface IStore<'s, 'a> with
        member this.CurrentState =
            state
        
        member this.Dispatch =
            fun action ->
                this.dispatcher action |> ignore
                state
