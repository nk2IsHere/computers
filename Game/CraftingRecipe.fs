namespace Computers.Game

open Computers.Cascade

type CraftingLocation =
    | HomeCraftingLocation
    | FieldCraftingLocation

type CraftingSkill =
    | ForagingCraftingSkill
    | FarmingCraftingSkill
    | FishingCraftingSkill
    | MiningCraftingSkill
    | CombatCraftingSkill

module CraftingSkill =
    let StringValue (craftingSkill: CraftingSkill): string =
        match craftingSkill with
        | ForagingCraftingSkill -> "Foraging"
        | FarmingCraftingSkill -> "Farming"
        | FishingCraftingSkill -> "Fishing"
        | MiningCraftingSkill -> "Mining"
        | CombatCraftingSkill -> "Combat"

type CraftingRequiredCondition =
    | FriendshipCraftingRequiredCondition of (string * int)
    | LevelCraftingRequiredCondition of int
    | SkillCraftingRequiredCondition of (CraftingSkill * int)
    | NoneCraftingRequiredCondition

type CraftingOutput =
    | PlaceholderCraftingOutput of string
    | ValueCraftingOutput of int * int

type CraftingRecipeEntry =
    | PlaceholderCraftingRecipeEntry of string * int
    | ValueCraftingRecipeEntry of int * int

type CraftingRecipe =
    {
        Name: string
        Recipe: CraftingRecipeEntry list
        Location: CraftingLocation
        Output: CraftingOutput
        IsBigCraftable: bool
        RequiredCondition: CraftingRequiredCondition
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
    
    let ToPackableRequiredCondition (requiredCondition: CraftingRequiredCondition): PackableValue =
        match requiredCondition with
        | FriendshipCraftingRequiredCondition(npcName, heartsLevel) -> StringPackableValue $"f {npcName} {heartsLevel}"
        | LevelCraftingRequiredCondition(level) -> StringPackableValue $"l {level}"
        | SkillCraftingRequiredCondition(craftingSkill, level) -> StringPackableValue $"s {craftingSkill |> CraftingSkill.StringValue} {level}"
        | NoneCraftingRequiredCondition -> StringPackableValue "none"
        
    let ToPackableRecipeEntry (recipeEntry: CraftingRecipeEntry): PackableValue =
        match recipeEntry with
        | PlaceholderCraftingRecipeEntry(key, count) -> CompositePackableValue ([PlaceholderPackableValue key; IntPackableValue count], StringPackableValue " ")
        | ValueCraftingRecipeEntry(gameId, count) -> StringPackableValue $"{gameId} {count}"
    
    let ToPackable (craftingRecipe: CraftingRecipe): Packable =
        [
            CompositePackableValue (
                craftingRecipe.Recipe |> List.map ToPackableRecipeEntry,
                StringPackableValue " "
            )
            ToPackableCraftingLocation craftingRecipe.Location
            ToPackableOutput craftingRecipe.Output
            BoolStringPackableValue craftingRecipe.IsBigCraftable
            ToPackableRequiredCondition craftingRecipe.RequiredCondition
            StringPackableValue craftingRecipe.DisplayName
        ]
