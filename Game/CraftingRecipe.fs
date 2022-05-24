namespace Computers.Types

open Computers.Cascade

type CraftingLocation =
    | HomeCraftingLocation
    | FieldCraftingLocation

type CraftingSkill =
    | NoneCraftingSkill
    | ForagingCraftingSkill
    | FarmingCraftingSkill
    | FishingCraftingSkill
    | MiningCraftingSkill
    | CombatCraftingSkill

type CraftingOutput =
    | PlaceholderCraftingOutput of string
    | ValueCraftingOutput of int * int

type CraftingRecipe =
    {
        Recipe: Map<int, int>
        Location: CraftingLocation
        Output: CraftingOutput
        IsBigCraftable: bool
        RequiredSkill: Option<CraftingSkill * int>
        DisplayName: string
    }

module CraftingRecipe =
    let ToPackableCraftingLocation (craftingLocation: CraftingLocation): PackableValue =
        match craftingLocation with
        | HomeCraftingLocation -> StringPackableValue "Home"
        | FieldCraftingLocation -> StringPackableValue "Field"
        
    let ToPackableOutput (output: CraftingOutput): PackableValue =
        match output with
        | ValueCraftingOutput(outputGameId, outputCount) -> StringPackableValue $"{outputGameId} {outputCount}"
        | PlaceholderCraftingOutput(key) -> PlaceholderPackableValue key
    
    let ToPackableRequiredSkill (requiredSkill: Option<CraftingSkill * int>): PackableValue =
        match requiredSkill with
        | Some(skillType, skillLevel) -> StringPackableValue $"{skillType} {skillLevel}"
        | None -> StringPackableValue "null"
    
    let ToPackable (craftingRecipe: CraftingRecipe): Packable =
        [
            StringPackableValue (craftingRecipe.Recipe
                |> Map.fold (fun acc k v -> acc @ [$"{k} {v}"]) []
                |> String.concat " ")
            ToPackableCraftingLocation craftingRecipe.Location
            ToPackableOutput craftingRecipe.Output
            BoolStringPackableValue craftingRecipe.IsBigCraftable
            ToPackableRequiredSkill craftingRecipe.RequiredSkill
            StringPackableValue craftingRecipe.DisplayName
        ]
