namespace Computers.Utils

module Option =
    let unwrap<'a> (errorMessage: string) (value: 'a option): 'a =
        match value with
        | Some value -> value
        | None -> failwith errorMessage
