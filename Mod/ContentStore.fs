namespace Computers.Mod

open Computers.Utils
open Computers.Cascade
open Computers.Game
open Microsoft.Xna.Framework.Graphics
open StardewModdingAPI

type ContentState =
    {
        bigCraftableStorage: BigCraftable list
        craftingRecipeStorage: CraftingRecipe list
    }
    
type UpdateBigCraftableIdsAction = (string * int option) list

type UpdateBigCraftableTextureContentAction = string * Texture2D
    
type ContentAction =
    | UpdateBigCraftableIdsAction of UpdateBigCraftableIdsAction
    | UpdateBigCraftableTextureContentAction of UpdateBigCraftableTextureContentAction

type ContentContext =
    {
        monitor: IMonitor
    }

module private ContentInternal =
    let UpdateBigCraftableIdsActionReducer: Reducer<ContentState, UpdateBigCraftableIdsAction> =
        fun state originalContent ->
            {
                state
                with bigCraftableStorage = (
                    state.bigCraftableStorage
                    |> List.map
                       (
                           fun bigCraftable ->
                               originalContent
                               |> List.tryFind (fun (name, _) -> name = bigCraftable.Name)
                               |> Option.map (fun (_, gameId) -> { bigCraftable with GameId = gameId })
                               |> Option.defaultValue bigCraftable
                       )
                )
            }

    let UpdateBigCraftableTextureContentActionReducer = 
        fun state (name, texture) ->
            {
                state
                with bigCraftableStorage = (
                    state.bigCraftableStorage
                    |> List.replace
                       (
                           fun bigCraftable ->
                               bigCraftable.Name = name
                       )
                       (
                            fun bigCraftable ->
                                {
                                    bigCraftable
                                    with Texture = Some texture
                                }
                       )
                )
            }

module Content =
    let Store: ContentContext -> ContentState -> Store<ContentState, ContentAction> =
        fun context state ->
            {
                Reducers = [
                    fun state action ->
                        match action with
                        | UpdateBigCraftableIdsAction(originalContent) -> ContentInternal.UpdateBigCraftableIdsActionReducer state originalContent
                        | UpdateBigCraftableTextureContentAction(name, texture) -> ContentInternal.UpdateBigCraftableTextureContentActionReducer state (name, texture)
                ]
                Middlewares = [
                    Store.LoggerMiddleware "dataStore" context.monitor.Log
                ]
                State = state
            }
