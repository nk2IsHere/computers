namespace Computers.Cascade
 
type Reducer<'s, 'a> = 's -> 'a -> 's

type Dispatcher<'s, 'a> = 'a -> 's

type Middleware<'s, 'a> = 's -> Dispatcher<'s, 'a> -> Dispatcher<'s, 'a>

type Store<'s, 'a> =
    {
        Reducers: Reducer<'s, 'a> list
        Middlewares: Middleware<'s, 'a> list
        State: 's
    }
    with
        member private this.reducer: Reducer<'s, 'a> =
            fun state action ->
                this.Reducers
                |> List.fold (fun state reducer -> reducer state action) state
                
        member private this.rootDispatcher: Dispatcher<'s, 'a> =
            fun action ->
                this.reducer this.State action
            
        member private this.combinedDispatcher: Dispatcher<'s, 'a> =
            (this.Middlewares, this.rootDispatcher)
            ||> List.foldBack (fun middleware -> middleware this.State)
        
        member this.Dispatch =
            this.combinedDispatcher


module Store =
    let LoggerMiddleware<'s, 'a> (tag: string) (outputSink: string -> unit): Middleware<'s, 'a> =
        fun state next action ->
            let newState = next action
            do outputSink $"[{tag}/{action}] {state} -> {newState}"
            newState
    
    let Chain<'s, 't, 'a> (intermediateStore: Store<'s, 'a>) (targetStore: Store<'t, 's>): Store<'t, 'a> =
        {
            Reducers = [
                fun _ action ->
                    let intermediateState = intermediateStore.Dispatch action
                    targetStore.Dispatch intermediateState
            ]
            Middlewares = []
            State = targetStore.State
        }
    
    let Compose<'s, 't, 'a> (leftStore: Store<'s, 'a>) (rightStore: Store<'t, 'a>): Store<'s * 't, 'a> =
        {
            Reducers = [
                fun _ action ->
                    let leftState = leftStore.Dispatch action
                    let rightState = rightStore.Dispatch action
                    (leftState, rightState)
            ]
            Middlewares = []
            State = (leftStore.State, rightStore.State)
        }
    
    let Combine<'s, 'a> (combiner: 's -> 's -> 's) (stores: Store<'s, 'a> list): Store<'s, 'a> =
        {
            Reducers = (
                stores
                |> List.collect (fun store -> store.Reducers)
            )
            Middlewares = (
                stores
                |> List.collect (fun store -> store.Middlewares)
            )
            State = (
                stores
                |> List.map (fun store -> store.State)
                |> List.reduce combiner
            )
        }
