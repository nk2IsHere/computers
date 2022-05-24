namespace Computers.Types

open Computers.Cascade

type BigCraftable =
    {
        GameId: int
        Name: string
        Price: int
        Edibility: int
        Type: string
        Description: string
        AllowOutdoorsPlacement: bool
        AllowIndoorsPlacement: bool
        Fragility: int
        IsLamp: bool
        DisplayName: string
        Texture: Texture
    }

module BigCraftable =
    let ToPackable (bigCraftable: BigCraftable): Packable =
        [
            IntPackableValue bigCraftable.GameId
            StringPackableValue bigCraftable.Name
            IntPackableValue bigCraftable.Price
            IntPackableValue bigCraftable.Edibility
            StringPackableValue bigCraftable.Type
            StringPackableValue bigCraftable.Description
            IntPackableValue bigCraftable.Edibility
            BoolStringPackableValue bigCraftable.AllowOutdoorsPlacement
            BoolStringPackableValue bigCraftable.AllowIndoorsPlacement
            IntPackableValue bigCraftable.Fragility
            BoolNumberPackableValue bigCraftable.IsLamp
            StringPackableValue bigCraftable.DisplayName
        ]
