namespace Computers.Cascade

open StardewModdingAPI

type RegistryContext =
    {
        monitor: IMonitor
    }

type RegistryEntry<'entry> =
    RegistryContext -> 'entry

type Registry<'id, 'entry> when 'id: comparison = {
    context: RegistryContext
    registry: Map<'id, 'entry>
}

module Registry =
    let Initialize: ('id * RegistryEntry<'entry>) list -> RegistryContext -> Registry<'id, 'entry> =
        fun registry context ->
            registry
            |> List.map (fun (id, entry) -> id, entry context)
            |> fun registry -> {
                context = context
                registry = registry |> Map.ofList
            }
    
    let Find: 'id -> Registry<'id, 'entry> -> 'entry option =
        fun id registry ->
            registry.registry
            |> Map.tryFind id
    
    let Pick: ('id -> bool) -> Registry<'id, 'entry> -> 'entry option =
        fun idPicker registry ->
            registry.registry
            |> Map.tryPick (
                fun id entry ->
                    if idPicker id then Some entry
                    else None
            )
    
    let Add: 'id * RegistryEntry<'entry> -> Registry<'id, 'entry> -> Registry<'id, 'entry> =
        fun (id, entry) registry ->
            {
                context = registry.context
                registry = registry.registry |> Map.add id (entry registry.context)
            }
    