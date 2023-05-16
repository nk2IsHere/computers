namespace Computers.Cascade

type PackableValue =
    | IntPackableValue of int
    | DecimalPackableValue of decimal
    | StringPackableValue of string
    | BoolNumberPackableValue of bool
    | BoolStringPackableValue of bool
    | PlaceholderPackableValue of string
    | CompositePackableValue of PackableValue list * PackableValue

type PlaceholderReplacementStorage = Map<string, PackableValue>

module PackableValue =
    let rec Pack (replacementStorage: PlaceholderReplacementStorage) (packableValue: PackableValue): string =
        match packableValue with
        | BoolNumberPackableValue(value) ->
            if value then "1"
            else "0"
        | BoolStringPackableValue(value) ->
            if value then "true"
            else "false"
        | PlaceholderPackableValue(key) ->
            replacementStorage
            |> Map.find key
            |> Pack replacementStorage 
        | CompositePackableValue(packableValues, splitter) ->
            packableValues
            |> List.map (Pack replacementStorage)
            |> String.concat (
                splitter
                |> Pack replacementStorage
            )
        | IntPackableValue(value) ->
            string value
        | DecimalPackableValue(value) ->
            string value
        | StringPackableValue(value) ->
            string value

type Packable = PackableValue list

module Packable =
    let Pack (replacementStorage: PlaceholderReplacementStorage) (packable: Packable): string =
        CompositePackableValue(packable, StringPackableValue "/")
        |> PackableValue.Pack replacementStorage
