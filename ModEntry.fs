namespace Computers

open StardewModdingAPI

type public ModEntry() =
    inherit Mod()
    
    override this.Entry (helper: IModHelper) =
        printfn "kek"