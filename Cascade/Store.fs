namespace Computers.Cascade
 
type Reducer<'s, 'a> = 's -> 'a -> 's

type Dispatcher<'a> = 'a -> 'a

type IStore<'s, 'a> =
    abstract member CurrentState: 's
    abstract member Dispatch: Dispatcher<'a>

type Middleware<'s, 'a> = IStore<'s, 'a> -> Dispatcher<'a> -> Dispatcher<'a>

type Store<'s, 'a>(reducers: List<Reducer<'s, 'a>>, middlewares: List<Middleware<'s, 'a>>, initialState: 's) =
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
            this.dispatcher

module Store =
    let store<'s, 'a> (reducers: List<Reducer<'s, 'a>>) (middlewares: List<Middleware<'s, 'a>>) (initialState: 's): Store<'s, 'a> =
        new Store<'s, 'a>(reducers, middlewares, initialState)
