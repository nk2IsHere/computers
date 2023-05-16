namespace Computers.Utils

module List =
    let replace f sub xs = 
        let rec finish acc = function
            | [] -> acc
            | x::xs -> finish (x::acc) xs
            
        let rec search acc = function
            | [] -> None
            | x::xs -> 
                if f x then Some(finish ((sub x)::xs) acc)
                else search (x::acc) xs
        
        (search [] xs)
        |> Option.defaultValue xs
