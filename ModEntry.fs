namespace Computers

open System
open System.Collections.Generic
open Computers.Cascade
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open StardewModdingAPI
open StardewModdingAPI.Events
open Computers.Types
open Computers.Utils

type DataState =
    {
        bigCraftableStorage: BigCraftable list
        craftingRecipeStorage: CraftingRecipe list
    }
    
type UpdateBigCraftableIdsDataAction = IDictionary<int, string>
type UpdateBigCraftableTextureDataAction = string * Texture2D
    
type DataAction =
    | UpdateBigCraftableIdsDataAction of UpdateBigCraftableIdsDataAction
    | UpdateBigCraftableTextureDataAction of UpdateBigCraftableTextureDataAction

type public ModEntry() =
    inherit Mod()
    
    let dataStore: Store<DataState, DataAction> ref =
        ref {
            reducers = [
                fun state action ->
                    match action with
                    | UpdateBigCraftableIdsDataAction(originalData) ->
                        let bigCraftableLatestId = (
                            originalData.Keys
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
                    | UpdateBigCraftableTextureDataAction(name, texture) ->
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
            ]
            middlewares = [
                Store.logger "dataStore" (fun log -> printf $"{log}")
            ]
            state = {
                bigCraftableStorage = [
                    {
                       GameId = None
                       Texture = None
                       Name = "Computer"
                       Price = 0
                       Edibility = -300
                       Type = "Crafting -9"
                       Description = "Programmable computer"
                       AllowOutdoorsPlacement = true
                       AllowIndoorsPlacement = true
                       Fragility = 0
                       IsLamp = false
                       DisplayName = "Computer"
                   }
                ]
                craftingRecipeStorage = [
                    {
                       Name = "Computer"
                       Recipe = Map [(380, 5)]
                       Location = CraftingLocation.HomeCraftingLocation
                       Output = CraftingOutput.PlaceholderCraftingOutput("ComputerCraftingRecipeOutput")
                       IsBigCraftable = true
                       RequiredCondition = NoneCraftingRequiredCondition
                       DisplayName = "Computer"
                   }
                ]
            }
        }
    
    member this.patcher: Map<string, IAssetData -> unit> =
        Map [
            (
                "Data/BigCraftablesInformation",
                fun asset ->
                    let bigCraftableGameData = asset.AsDictionary<int, string>().Data
                    do dataStore.Value <- {
                        dataStore.Value
                        with state = dataStore.Value.Dispatch (UpdateBigCraftableIdsDataAction(bigCraftableGameData))
                    }
                    
                    for bigCraftable in dataStore.Value.state.bigCraftableStorage do
                        do this.Monitor.Log $"Add BigCraftable {bigCraftable.Name}"
                        bigCraftableGameData.Add(
                            bigCraftable.GameId
                            |> Option.unwrap "Should not happen, UpdateBigCraftableIdsDataAction must update ids of all BigCraftables",
                            bigCraftable
                            |> BigCraftable.ToPackable
                            |> Packable.Pack (Map [])
                        )
            )
            (
                "Data/CraftingRecipes",
                fun asset ->
                    let craftingRecipeGameData = asset.AsDictionary<string, string>().Data
                    for craftingRecipe in dataStore.Value.state.craftingRecipeStorage do
                        do this.Monitor.Log $"Add CraftingRecipe {craftingRecipe.Name}"
                        let matchingBigCraftable = (
                            dataStore.Value.state.bigCraftableStorage
                            |> List.find (fun bigCraftable -> bigCraftable.Name = craftingRecipe.Name)
                        )
                        
                        craftingRecipeGameData.Add(
                            craftingRecipe.Name,
                            craftingRecipe
                            |> CraftingRecipe.ToPackable
                            |> Packable.Pack (Map [
                                (
                                    "ComputerCraftingRecipeOutput",
                                    CompositePackableValue(
                                        [
                                            IntPackableValue(
                                                matchingBigCraftable.GameId
                                                |> Option.unwrap "Data/BigCraftablesInformation must be loaded before Data/CraftingRecipes"
                                            )
                                            IntPackableValue 1
                                        ],
                                        StringPackableValue " "
                                    )
                                )
                            ])
                        )
            )
            (
                "TileSheets/Craftables",
                fun asset ->
                    let craftableTileSheetGameData = asset.AsImage()
                    do craftableTileSheetGameData.ExtendImage(craftableTileSheetGameData.Data.Width, 4096) |> ignore
                    for bigCraftable in dataStore.Value.state.bigCraftableStorage do
                        do this.Monitor.Log $"Add Texture for BigCraftable {bigCraftable.Name}"
                        let gameId = (
                            bigCraftable.GameId
                            |> Option.unwrap "Data/BigCraftablesInformation must be loaded before TileSheets/Craftables"
                        )
                        
                        craftableTileSheetGameData.PatchImage(
                             bigCraftable.Texture
                             |> Option.unwrap $"Texture of {bigCraftable.Name} must be loaded before TileSheets/Craftables",
                             Nullable(),
                             Rectangle(gameId % 8 * 16, gameId / 8 * 32, 16, 32),
                             PatchMode.Replace
                        )
                        
            )
        ]
    
    override this.Entry (helper: IModHelper) =
        do dataStore.Value <- {
            dataStore.Value
            with state = dataStore.Value
                .Dispatch (UpdateBigCraftableTextureDataAction(
                    "Computer",
                    helper
                        .ModContent
                        .Load<Texture2D>("assets/computer.png")
               ))
        }
        
        do helper
            .Events.Content.AssetRequested
            .AddHandler (
                fun (_: _) (args: AssetRequestedEventArgs) ->
                    this.patcher
                    |> Map.tryPick (
                        fun name patcher ->
                            if args.Name.IsEquivalentTo(name) then Some patcher
                            else None
                        )
                    |> Option.iter (
                        fun patcher ->
                            args.Edit(fun asset -> patcher asset)
                    )
            )
        
        // Enforce loading Data/BigCraftablesInformation before TileSheets/Craftables
        do helper
            .GameContent
            .Load<Dictionary<int, string>>("Data/BigCraftablesInformation")
            |> ignore
