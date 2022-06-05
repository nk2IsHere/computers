# Computers

Programmable computers for Stardew Valley

## Goals.

### Generally:

Make processing of Stardew's resources a configurable fun pipelike-like process by introducing programming.

### Specifically this mod should:

- Add computer bigcraftable (can be placed anywhere) which
    - is interactable by player
    - has screen and input console
    - supports some interpreted programming language (python? lua? - undecided)
    - has apis that allow it to interact with other storages/machines..
    - auto-discovers machines indirectly connected to it (meaning that machines may form a group by attaching to each other and computer)
    - has peripherals


- Add monitor bigcraftable (can be placed anywhere) which
    - will have its own api to display requested data
    - will be of different sizes
    - will have to be connected directly to computer to operate


- Add peripherals:
    - Wireless stations - allows communication with other computers on a limited distance
    - ??? (still undecided)

## TODOs

### Basic item support 

- [x] Add bigcraftable type
- [x] Add craftingrecipe type
- [x] Add support for functional redux-like store
- [x] Create dataStore for patcher
- [x] Add support for figuring out the next ids for mod's bigcraftables
- [x] Add dictionary (Data/BigCraftablesInformation, Data/CraftingRecipes) patching
- [x] Add tilesheet (TileSheets/Craftables) patching

### Basic POC of computer in-game

- [ ] Make computer bigcraftable interactable on use action
- [ ] Make computer interaction display centered window
- [ ] Make displayed window render custom graphics
- [ ] Add support for interpretable language in-library
- [ ] Add VMs (sandboxes) for each computer instance in world
- [ ] Add save state mechanism for computers, make them movable and attach id tag for any obtained computer
- [ ] Add basic console interpreter for each computer
- [ ] Add basic stdlib apis for computers

### Interactability with world (TODO)

- [ ] Add possibility of auto-discovering machine groups with computer as controller
- [ ] ???

## Kudos/Inspiration

- Automate mod for Stardew
- OpenComputers mod for Minecraft
- Stackoverflow responders (a lot of them)
