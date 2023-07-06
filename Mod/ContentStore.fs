namespace Computers.Mod

open System.Collections.Generic
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
    
type UpdateBigCraftableIdsContentAction = IDictionary<int, string>

type UpdateBigCraftableTextureContentAction = string * Texture2D
    
type ContentAction =
    | UpdateBigCraftableIdsContentAction of UpdateBigCraftableIdsContentAction
    | UpdateBigCraftableTextureContentAction of UpdateBigCraftableTextureContentAction

type ContentContext =
    {
        state: ContentState
        monitor: IMonitor
    }

module private ContentInternal =
    let UpdateBigCraftableIdsContentActionReducer: Reducer<ContentState, UpdateBigCraftableIdsContentAction> =
        fun state originalContent ->
            let bigCraftableLatestId = (
                originalContent.Keys
                |> seq
                |> Seq.max
                |> (+) 1
            )
            
            {
                state
                with bigCraftableStorage = (
                    state.bigCraftableStorage
                    |> List.indexed
                    |> List.map (
                        fun (index, bigCraftable) ->
                            {
                                bigCraftable
                                with GameId = Some (bigCraftableLatestId + index)
                            }
                    )
                )
            }

    let UpdateBigCraftableTextureContentAction = 
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
    let Store: ContentContext -> Store<ContentState, ContentAction> =
        fun context ->
            {
                Reducers = [
                    fun state action ->
                        match action with
                        | UpdateBigCraftableIdsContentAction(originalContent) -> ContentInternal.UpdateBigCraftableIdsContentActionReducer state originalContent
                        | UpdateBigCraftableTextureContentAction(name, texture) -> ContentInternal.UpdateBigCraftableTextureContentAction state (name, texture)
                ]
                Middlewares = [
                    Store.LoggerMiddleware "dataStore" context.monitor.Log
                ]
                State = context.state
            }
