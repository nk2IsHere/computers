namespace Computers.Utils

module String =
    let Join (separator: string) (values: string list) =
        values |> List.reduce (fun acc value -> acc + separator + value)
