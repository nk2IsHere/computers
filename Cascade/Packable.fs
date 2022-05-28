namespace Computers.Cascade

type PackableValue =
    | IntPackableValue of int
    | DecimalPackableValue of decimal
    | StringPackableValue of string
    | BoolNumberPackableValue of bool
    | BoolStringPackableValue of bool
    | PlaceholderPackableValue of string

type PlaceholderReplacementStorage = Map<string, PackableValue>
type Packable = List<PackableValue>

module Packable =
    let rec PackValue (replacementStorage: PlaceholderReplacementStorage) (packableValue: PackableValue): string =
        match packableValue with
        | BoolNumberPackableValue(value) -> if value then "1" else "0"
        | BoolStringPackableValue(value) -> if value then "true" else "false"
        | PlaceholderPackableValue(key) -> PackValue replacementStorage (replacementStorage |> Map.find key)
        | _ -> string packableValue
        
    let Pack (replacementStorage: PlaceholderReplacementStorage) (packable: Packable): string =
        packable
        |> List.map (PackValue replacementStorage)
        |> List.fold (fun acc value -> $"{acc}/{value}") ""
