namespace Computers

open System.Collections.Generic
open Computers.Cascade
open Computers.Game
open Microsoft.Xna.Framework.Graphics
open StardewModdingAPI
open StardewModdingAPI.Events
open Computers.Utils
open Computers.Mod

type public ModEntry() =
    inherit Mod()
    
    member this.contentStore: Store<ContentState, ContentAction> ref =
        ref (Content.Store {
            monitor = this.Monitor
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
                       Recipe = [
                           ValueCraftingRecipeEntry (380, 5)
                       ]
                       Location = CraftingLocation.HomeCraftingLocation
                       Output = CraftingOutput.PlaceholderCraftingOutput("ComputerCraftingRecipeOutput")
                       IsBigCraftable = true
                       RequiredCondition = NoneCraftingRequiredCondition
                       DisplayName = "Computer"
                   }
                ]
            }
        })
    
    member this.patcher = Patcher.Registry { monitor = this.Monitor }
        
    member this.interactor = Interaction.Registry { monitor = this.Monitor }
        
    
    override this.Entry (helper: IModHelper) =
        do this.contentStore.Value <- {
            this.contentStore.Value
            with State = this.contentStore.Value
                .Dispatch (UpdateBigCraftableTextureContentAction(
                    "Computer",
                    helper
                        .ModContent
                        .Load<Texture2D>("assets/computer.png")
               ))
        }
        
        do helper
            .Events
            .Content
            .AssetRequested
            .AddHandler (
                fun (_: _) (args: AssetRequestedEventArgs) ->
                    this.patcher
                    |> Registry.Pick (fun name -> args.Name.IsEquivalentTo(name))
                    |> Option.iter (
                        fun patcher ->
                            args.Edit (
                                fun data ->
                                    match (patcher this.contentStore.Value.State data) with
                                    | Failure(messages) -> failwith (String.Join "\n" messages)
                                    | Success(result, messages) ->
                                        match result with
                                        | PatchDataBigCraftablesInformation(bigCraftablesInformation) ->
                                            do this.Monitor.Log (String.Join "\n" (messages @ ["Big craftables patched"]))
                                            do this.contentStore.Value <- {
                                                this.contentStore.Value
                                                with State = this.contentStore.Value
                                                    .Dispatch (UpdateBigCraftablesContentAction(bigCraftablesInformation))
                                            }
                                            
                                        | PatchDataCraftingRecipes(_) ->
                                            do this.Monitor.Log (String.Join "\n" (messages @ ["Crafting recipes patched"]))
                                        
                                        | PatchTileSheetsCraftables() ->
                                            do this.Monitor.Log (String.Join "\n" (messages @ ["Tile sheet patched"]))
                            )
                    )
            )
            
        do helper
            .Events
            .Input
            .ButtonPressed
            .AddHandler (
                fun (_: _) (args: ButtonPressedEventArgs) ->
                    let shouldHandleInteraction = (
                        Context.IsPlayerFree
                            && (args.Button.IsActionButton() || Constants.TargetPlatform = GamePlatform.Android)
                            && StardewValley.Game1.currentLocation.objects.ContainsKey(args.Cursor.Tile)
                            && args.Cursor.Tile.Equals(args.Cursor.GrabTile)
                    )
                    
                    do if shouldHandleInteraction then (
                        let currentSelectedObject = StardewValley.Game1.currentLocation.objects[args.Cursor.Tile]
                        this.interactor
                        |> Registry.Find currentSelectedObject.Name
                        |> Option.iter (
                            fun object ->
                                object currentSelectedObject   
                        )
                    )
            )
        
        // Enforce loading Data/BigCraftablesInformation before TileSheets/Craftables
        do helper
            .GameContent
            .Load<Dictionary<int, string>>("Data/BigCraftablesInformation")
            |> ignore
        