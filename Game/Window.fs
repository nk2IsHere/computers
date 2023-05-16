namespace Computers.Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open StardewValley
open StardewValley.Menus

type public WindowInitialContext =
    {
        width: int
        height: int
    }

and public WindowContext =
    {
        window: Window
        width: int
        height: int
        xPositionOnScreen: int
        yPositionOnScreen: int
    }

and public WindowCommand = WindowContext -> SpriteBatch -> WindowContext
 
and public Window(context: WindowInitialContext, commands: WindowCommand list) =
    inherit IClickableMenu(
        Game1.viewport.Width / 2 - (context.width + IClickableMenu.borderWidth * 2) / 2,
        Game1.viewport.Height / 2 - (context.height + IClickableMenu.borderWidth * 2) / 2 - Game1.tileSize,
        context.width + IClickableMenu.borderWidth * 2,
        context.height + IClickableMenu.borderWidth * 2 + Game1.tileSize
    )
    
    member private this.context: WindowContext ref =
        ref {
            window = this
            width = context.width
            height = context.height
            xPositionOnScreen = this.xPositionOnScreen
            yPositionOnScreen = this.yPositionOnScreen
        }
    
    override this.gameWindowSizeChanged(oldBounds: Rectangle, newBounds: Rectangle) =
        do base.gameWindowSizeChanged(oldBounds, newBounds)
        do this.xPositionOnScreen <- Game1.viewport.Width / 2 - (context.width + IClickableMenu.borderWidth * 2) / 2
        do this.yPositionOnScreen <- Game1.viewport.Height / 2 - (context.height + IClickableMenu.borderWidth * 2) / 2 - Game1.tileSize
    
    override this.draw(batch: SpriteBatch) =        
        this.context.Value <- commands |> List.fold (fun state command -> command state batch) this.context.Value
       
module Window =
    let DrawDialogueBoxCommand (): WindowCommand =
        fun context _ ->
            do Game1.drawDialogueBox(
                context.xPositionOnScreen,
                context.yPositionOnScreen,
                context.width,
                context.height,
                false,
                true
            )
            
            context
    
    let DrawMouseCommand (): WindowCommand =
        fun context batch ->
            do context.window.drawMouse(batch)
            context