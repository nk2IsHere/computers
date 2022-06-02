namespace Computers.Cascade

type PackableValue =
    | IntPackableValue of int
    | DecimalPackableValue of decimal
    | StringPackableValue of string
    | BoolNumberPackableValue of bool
    | BoolStringPackableValue of bool
    | PlaceholderPackableValue of string

type PlaceholderReplacementStorage = Map<string, PackableValue>

module PackableValue =
        let rec Pack (replacementStorage: PlaceholderReplacementStorage) (packableValue: PackableValue): string =
            match packableValue with
            | BoolNumberPackableValue(value) -> if value then "1" else "0"
            | BoolStringPackableValue(value) -> if value then "true" else "false"
            | PlaceholderPackableValue(key) -> Pack replacementStorage (replacementStorage |> Map.find key)
            | IntPackableValue(value) -> string value
            | DecimalPackableValue(value) -> string value
            | StringPackableValue(value) -> string value

type Packable = PackableValue list

module Packable =
    let Pack (replacementStorage: PlaceholderReplacementStorage) (packable: Packable): string =
        packable
        |> List.map (PackableValue.Pack replacementStorage)
        |> String.concat "/"
