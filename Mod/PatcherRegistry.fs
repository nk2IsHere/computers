namespace Computers.Mod

open System
open Computers.Cascade
open Computers.Game
open Computers.Utils
open Microsoft.Xna.Framework
open StardewModdingAPI

type PatcherResult =
    | PatchDataBigCraftablesInformation of BigCraftable list
    | PatchDataCraftingRecipes of CraftingRecipe list
    | PatchTileSheetsCraftables of unit

type Patcher = ContentState -> IAssetData -> RailwayResult<PatcherResult, string>

module Patcher =
    let Registry: RegistryContext -> Registry<string, Patcher> =
        Registry.Initialize [
            (
                "Data/BigCraftablesInformation",
                fun context state asset ->
                    let bigCraftableGameData = asset.AsDictionary<int, string>().Data
                    let latestBigCraftableId = bigCraftableGameData.Keys |> Seq.max
                    
                    state.bigCraftableStorage
                     |> List.indexed
                     |> List.map (fun (i, bigCraftable) ->
                            do context.monitor.Log $"Add BigCraftable {bigCraftable.Name}"
                            
                            let bigCraftableId = latestBigCraftableId + i + 1
                            
                            let bigCraftableValue = (
                                bigCraftable
                                |> BigCraftable.ToPackable
                                |> Packable.Pack (Map [])
                            )
                            
                            Railway.tryR (
                                fun () -> do bigCraftableGameData.Add(
                                    bigCraftableId,
                                    bigCraftableValue
                                )
                            )
                            |> Railway.mapR (
                                fun () -> {
                                    bigCraftable
                                    with GameId = Some bigCraftableId
                                }
                            )
                     )
                     |> Railway.sequenceR
                     |> Railway.mapR PatchDataBigCraftablesInformation
            )
            (
                "Data/CraftingRecipes",
                fun context state asset ->
                    let craftingRecipeGameData = asset.AsDictionary<string, string>().Data
                    
                    state.craftingRecipeStorage
                    |> List.map(fun craftingRecipe ->
                        do context.monitor.Log $"Add CraftingRecipe {craftingRecipe.Name}"
                        
                        let matchingBigCraftable =
                            Railway.failIfNone
                            <| $"BigCraftable by name {craftingRecipe.Name} is not found in storage."
                            <| (
                                state.bigCraftableStorage
                                |> List.tryFind (fun bigCraftable -> bigCraftable.Name = craftingRecipe.Name)
                            )
                        
                        let matchingBigCraftableId =
                            matchingBigCraftable
                            |> Railway.bindR (
                                 fun bigCraftable ->
                                     Railway.failIfNone
                                     <| "Data/BigCraftablesInformation must be loaded before Data/CraftingRecipes"
                                     <| bigCraftable.GameId
                            )
                            
                        
                        let craftingRecipeId = craftingRecipe.Name
                        
                        let craftingRecipeValue =
                            matchingBigCraftableId
                            |> Railway.mapR (
                                fun bigCraftableId ->
                                    craftingRecipe
                                    |> CraftingRecipe.ToPackable
                                    |> Packable.Pack (Map [
                                        (
                                            "ComputerCraftingRecipeOutput",
                                            CompositePackableValue(
                                                [IntPackableValue bigCraftableId; IntPackableValue 1],
                                                StringPackableValue " "
                                            )
                                        )
                                    ])
                            )
                        
                        craftingRecipeValue
                        |> Railway.mapR (
                            fun craftingRecipeValue ->
                                do craftingRecipeGameData.Add(
                                    craftingRecipeId,
                                    craftingRecipeValue
                                )
                        )
                        |> Railway.mapR (fun _ -> craftingRecipe)
                    )
                    |> Railway.sequenceR
                    |> Railway.mapR PatchDataCraftingRecipes
            )
            (
                "TileSheets/Craftables",
                fun context state asset ->
                    let craftableTileSheetGameData = asset.AsImage()
                    do craftableTileSheetGameData.ExtendImage(craftableTileSheetGameData.Data.Width, 4096) |> ignore
                    
                    state.bigCraftableStorage
                    |> List.map(fun bigCraftable ->
                        do context.monitor.Log $"Add Texture for BigCraftable {bigCraftable.Name}"
                        
                        let bigCraftableId =
                            Railway.failIfNone
                            <| "Data/BigCraftablesInformation must be loaded before Data/CraftingRecipes"
                            <| bigCraftable.GameId
                        
                        let bigCraftableTexture =
                            Railway.failIfNone
                            <| "Should not happen, UpdateBigCraftableTexturesDataAction must update textures of all BigCraftables"
                            <| bigCraftable.Texture
                        
                        Railway.combineR bigCraftableId bigCraftableTexture
                        |> Railway.bindR (
                            fun (bigCraftableId, bigCraftableTexture) ->
                                Railway.tryR (
                                    fun () ->
                                        do craftableTileSheetGameData.PatchImage(
                                             bigCraftableTexture,
                                             Nullable(),
                                             Rectangle(bigCraftableId % 8 * 16, bigCraftableId / 8 * 32, 16, 32),
                                             PatchMode.Replace
                                        )
                                )
                        )
                    )
                    |> Railway.sequenceR
                    |> Railway.mapR (fun _ -> PatchTileSheetsCraftables ())
            )
        ]
