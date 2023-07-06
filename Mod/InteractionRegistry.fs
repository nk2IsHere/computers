namespace Computers.Mod

open Computers.Cascade
open Computers.Game

type Interaction = StardewValley.Object -> unit

module Interaction =
    let Registry: RegistryContext -> Registry<string, Interaction> =
        Registry.Initialize [
            (
                "Computer",
                fun context object ->
                    do context.monitor.Log "Computer clicked"
                    // do StardewValley.Game1.activeClickableMenu <- Window(
                    //     {
                    //         width = 600
                    //         height = 400
                    //     },
                    //     [
                    //         Window.DrawDialogueBoxCommand()
                    //         Window.DrawMouseCommand()
                    //     ]
                    // )
            )
        ]
    