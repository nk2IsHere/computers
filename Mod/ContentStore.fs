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
    
type UpdateBigCraftablesContentAction = BigCraftable list

type UpdateBigCraftableTextureContentAction = string * Texture2D
    
type ContentAction =
    | UpdateBigCraftablesContentAction of UpdateBigCraftablesContentAction
    | UpdateBigCraftableTextureContentAction of UpdateBigCraftableTextureContentAction

type ContentContext =
    {
        state: ContentState
        monitor: IMonitor
    }

module private ContentInternal =
    let UpdateBigCraftablesContentActionReducer: Reducer<ContentState, UpdateBigCraftablesContentAction> =
        fun state originalContent ->
            {
                state
                with bigCraftableStorage = originalContent
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
                        | UpdateBigCraftablesContentAction(originalContent) -> ContentInternal.UpdateBigCraftablesContentActionReducer state originalContent
                        | UpdateBigCraftableTextureContentAction(name, texture) -> ContentInternal.UpdateBigCraftableTextureContentAction state (name, texture)
                ]
                Middlewares = [
                    Store.LoggerMiddleware "dataStore" context.monitor.Log
                ]
                State = context.state
            }
