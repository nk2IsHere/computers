namespace Computers

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open StardewValley
open StardewValley.Menus

type public ComputerWindow(width: int, height: int) =
    inherit IClickableMenu(
        Game1.viewport.Width / 2 - (width + IClickableMenu.borderWidth * 2) / 2,
        Game1.viewport.Height / 2 - (height + IClickableMenu.borderWidth * 2) / 2 - Game1.tileSize,
        width + IClickableMenu.borderWidth * 2,
        height + IClickableMenu.borderWidth * 2 + Game1.tileSize
    )
    
    override this.gameWindowSizeChanged(oldBounds: Rectangle, newBounds: Rectangle) =
        do base.gameWindowSizeChanged(oldBounds, newBounds)
        do this.xPositionOnScreen <- Game1.viewport.Width / 2 - (width + IClickableMenu.borderWidth * 2) / 2
        do this.yPositionOnScreen <- Game1.viewport.Height / 2 - (height + IClickableMenu.borderWidth * 2) / 2 - Game1.tileSize
    
    override this.draw(batch: SpriteBatch) =
        do Game1.drawDialogueBox(
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            false,
            true
        )

        do this.drawMouse(batch)
    
//    override this.update(time: GameTime) =
//        