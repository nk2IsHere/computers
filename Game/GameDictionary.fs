namespace Computers.Types

open System.Collections.Generic
open StardewModdingAPI
open Computers.Utils.Map

type UnloadedGameDictionary =
    {
        path: string
    }
    
type LoadedGameDictionary<'K, 'V when 'K: comparison> =
    {
        path: string
        cache: Map<'K, 'V>
    }

type GameDictionary<'K, 'V when 'K: comparison> =
    | UnloadedGameDictionary of UnloadedGameDictionary
    | LoadedGameDictionary of LoadedGameDictionary<'K, 'V>

module GameDictionary =
    let Load<'K, 'V when 'K: comparison> (helper: IModHelper) (gameDictionary: GameDictionary<'K, 'V>) =
        match gameDictionary with
        | UnloadedGameDictionary({ path = path }) -> toMap (helper.GameContent.Load<Dictionary<'K, 'V>>(path))
        | LoadedGameDictionary({ path = _; cache = cache }) -> cache
