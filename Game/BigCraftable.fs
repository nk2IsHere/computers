namespace Computers.Types

open Computers.Cascade
open Microsoft.Xna.Framework.Graphics

type BigCraftable =
    {
        GameId: int
        Texture: Texture2D
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
    }

module BigCraftable =
    let ToPackable (bigCraftable: BigCraftable): Packable =
        [
            StringPackableValue bigCraftable.Name
            IntPackableValue bigCraftable.Price
            IntPackableValue bigCraftable.Edibility
            StringPackableValue bigCraftable.Type
            StringPackableValue bigCraftable.Description
            BoolStringPackableValue bigCraftable.AllowOutdoorsPlacement
            BoolStringPackableValue bigCraftable.AllowIndoorsPlacement
            IntPackableValue bigCraftable.Fragility
            BoolNumberPackableValue bigCraftable.IsLamp
            StringPackableValue bigCraftable.DisplayName
        ]
