namespace Computers

open System
open System.Collections.Generic 
open Computers.Cascade
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open StardewModdingAPI
open StardewModdingAPI.Events
open Computers.Types

type public ModEntry() =
    inherit Mod()
    
    member this.generatePatcher (bigCraftableStorage: BigCraftable list, craftingRecipeStorage: CraftingRecipe list): Map<string, IAssetData -> unit> =
        Map [
            (
                "Data/BigCraftablesInformation",
                fun asset ->
                    let bigCraftableGameData = asset.AsDictionary<int, string>().Data
                    for bigCraftable in bigCraftableStorage do
                        do this.Monitor.Log $"Add BigCraftable {bigCraftable.GameId}"
                        bigCraftableGameData.Add(
                            bigCraftable.GameId,
                            bigCraftable
                            |> BigCraftable.ToPackable
                            |> Packable.Pack (Map [])
                        )
            )
            (
                "Data/CraftingRecipes",
                fun asset ->
                    let craftingRecipeGameData = asset.AsDictionary<string, string>().Data
                    for craftingRecipe in craftingRecipeStorage do
                        do this.Monitor.Log $"Add CraftingRecipe {craftingRecipe.Name}"
                        craftingRecipeGameData.Add(
                            craftingRecipe.Name,
                            craftingRecipe
                            |> CraftingRecipe.ToPackable
                            |> Packable.Pack (Map [])
                        )
            )
            (
                "TileSheets/Craftables",
                fun asset ->
                    let craftableTileSheetGameData = asset.AsImage()
                    do craftableTileSheetGameData.ExtendImage(craftableTileSheetGameData.Data.Width, 4096) |> ignore
                    for bigCraftable in bigCraftableStorage do
                        do this.Monitor.Log $"Add Texture for BigCraftable {bigCraftable.GameId}"
                        craftableTileSheetGameData.PatchImage(
                             bigCraftable.Texture,
                             Nullable(),
                             Rectangle(bigCraftable.GameId % 8 * 16, bigCraftable.GameId / 8 * 32, 16, 32),
                             PatchMode.Replace
                        )
                        
            )
        ]
    
    override this.Entry (helper: IModHelper) =
        let computerBigCraftableApplicableId = 281 //TODO: make any possible way to guess the id before BigCraftables is loaded
        
        do this.Monitor.Log $"Computers will receive id of {computerBigCraftableApplicableId}"
        
        let computerBigCraftableTexture = (
            helper
                .ModContent
                .Load<Texture2D>("assets/computer.png")
        )
        
        let patcher = this.generatePatcher (
            [
               {
                   GameId = computerBigCraftableApplicableId
                   Texture = computerBigCraftableTexture
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
            ],
            [
               {
                   Name = "Computer"
                   Recipe = Map [(380, 5)]
                   Location = CraftingLocation.HomeCraftingLocation
                   Output = CraftingOutput.ValueCraftingOutput(computerBigCraftableApplicableId, 1)
                   IsBigCraftable = true
                   RequiredCondition = NoneCraftingRequiredCondition
                   DisplayName = "Computer"
               }
            ]
        )
        
        helper
            .Events.Content.AssetRequested
            .AddHandler (
                fun (_: _) (args: AssetRequestedEventArgs) ->
                    patcher
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
        