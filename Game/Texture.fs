namespace Computers.Types

open Microsoft.Xna.Framework.Graphics
open StardewModdingAPI

type UnloadedTexture =
    {
        name: string
    }

type LoadedTexture =
    {
        name: string
        cache: Texture2D
    }

type Texture =
    | UnloadedTexture of UnloadedTexture
    | LoadedTexture of LoadedTexture
    
module Texture =
    let Load (helper: IModHelper) (texture: Texture) =
        match texture with
        | UnloadedTexture({ name = name }) -> helper.ModContent.Load<Texture2D>($"assets/{name}.png")
        | LoadedTexture({ name = _; cache = cache }) -> cache
