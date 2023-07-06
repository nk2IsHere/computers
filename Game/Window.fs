namespace Computers.Game

open Computers.Cascade
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open StardewValley
open StardewValley.Menus

type public WindowCommand<'a> = 'a -> Window -> SpriteBatch -> unit

and public WindowAction =
    | DrawDialogueBoxWindowAction of unit
    | DrawMouseWindowAction of unit
 
and public WindowEvent =
    | InitializeWindowEvent of unit

and public Window(width: int, height: int, commands: Store<WindowAction list, WindowEvent>) as self =
    inherit IClickableMenu(
        Game1.viewport.Width / 2 - (width + IClickableMenu.borderWidth * 2) / 2,
        Game1.viewport.Height / 2 - (height + IClickableMenu.borderWidth * 2) / 2 - Game1.tileSize,
        width + IClickableMenu.borderWidth * 2,
        height + IClickableMenu.borderWidth * 2 + Game1.tileSize
    )
    
    do self.commands.Value <- commands.Dispatch(InitializeWindowEvent())
    
    let ApplyDrawDialogueBox (): WindowCommand<unit> =
        fun _ window _ ->
            do Game1.drawDialogueBox(
                window.xPositionOnScreen,
                window.yPositionOnScreen,
                window.width,
                window.height,
                false,
                true
            )
                
    let ApplyDrawMouse (): WindowCommand<unit> =
        fun _ window batch ->
            do window.drawMouse(batch)
    
    member private this.commands: WindowAction list ref =
        ref commands.State
    
    override this.gameWindowSizeChanged(oldBounds: Rectangle, newBounds: Rectangle) =
        do base.gameWindowSizeChanged(oldBounds, newBounds)
        do this.xPositionOnScreen <- Game1.viewport.Width / 2 - (width + IClickableMenu.borderWidth * 2) / 2
        do this.yPositionOnScreen <- Game1.viewport.Height / 2 - (height + IClickableMenu.borderWidth * 2) / 2 - Game1.tileSize
    
    override this.draw(batch: SpriteBatch) =        
        for command in this.commands.Value do
            match command with
            | DrawDialogueBoxWindowAction action ->
                do ApplyDrawDialogueBox() action this batch
            | DrawMouseWindowAction action ->
                do ApplyDrawMouse() action this batch
