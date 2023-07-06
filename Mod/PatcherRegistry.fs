namespace Computers.Mod

open System
open Computers.Cascade
open Computers.Game
open Computers.Utils
open Microsoft.Xna.Framework
open StardewModdingAPI

type Patcher = ContentState -> IAssetData -> RailwayResult<BigCraftable list, string>

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
                     |> List.reduce (fun (latestBigCraftableId, bigCraftableGameData) (i, bigCraftable) ->
                            do context.monitor.Log $"Add BigCraftable {bigCraftable.Name}"
                            
                            let bigCraftableId = latestBigCraftableId + i + 1
                            
                            let bigCraftableValue = (
                                bigCraftable
                                |> BigCraftable.ToPackable
                                |> Packable.Pack (Map [])
                            )
                            
                            do bigCraftableGameData.Add(
                                bigCraftableId,
                                bigCraftableValue
                            )
                            
                            (bigCraftableId, bigCraftableGameData)
                        )
            )
            (
                "Data/CraftingRecipes",
                fun context state asset ->
                    let craftingRecipeGameData = asset.AsDictionary<string, string>().Data
                    for craftingRecipe in state.craftingRecipeStorage do
                        do context.monitor.Log $"Add CraftingRecipe {craftingRecipe.Name}"
                        
                        let matchingBigCraftable = (
                            state.bigCraftableStorage
                            |> List.find (fun bigCraftable -> bigCraftable.Name = craftingRecipe.Name)
                        )
                        
                        let matchingBigCraftableId = (
                            matchingBigCraftable.GameId
                            |> Option.unwrap "Data/BigCraftablesInformation must be loaded before Data/CraftingRecipes"
                        )
                        
                        let craftingRecipeId = craftingRecipe.Name
                        
                        let craftingRecipeValue = (
                            craftingRecipe
                            |> CraftingRecipe.ToPackable
                            |> Packable.Pack (Map [
                                (
                                    "ComputerCraftingRecipeOutput",
                                    CompositePackableValue(
                                        [IntPackableValue matchingBigCraftableId; IntPackableValue 1],
                                        StringPackableValue " "
                                    )
                                )
                            ])    
                        )
                        
                        do craftingRecipeGameData.Add(
                            craftingRecipeId,
                            craftingRecipeValue
                        )
            )
            (
                "TileSheets/Craftables",
                fun context state asset ->
                    let craftableTileSheetGameData = asset.AsImage()
                    do craftableTileSheetGameData.ExtendImage(craftableTileSheetGameData.Data.Width, 4096) |> ignore
                    
                    for bigCraftable in state.bigCraftableStorage do
                        do context.monitor.Log $"Add Texture for BigCraftable {bigCraftable.Name}"
                        
                        let bigCraftableId = (
                            bigCraftable.GameId
                            |> Option.unwrap "Data/BigCraftablesInformation must be loaded before TileSheets/Craftables"
                        )
                        
                        let bigCraftableTexture = (
                            bigCraftable.Texture
                            |> Option.unwrap "Should not happen, UpdateBigCraftableTexturesDataAction must update textures of all BigCraftables"
                        )
                        
                        do craftableTileSheetGameData.PatchImage(
                             bigCraftableTexture,
                             Nullable(),
                             Rectangle(bigCraftableId % 8 * 16, bigCraftableId / 8 * 32, 16, 32),
                             PatchMode.Replace
                        )
                        
            )
        ]
