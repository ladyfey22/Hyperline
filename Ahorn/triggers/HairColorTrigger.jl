module HyperlineHairColorTrigger

using ..Ahorn, Maple

@mapdef Trigger "Hyperline/HairColorTrigger" HairChangeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, resetOnLeave::Bool=false, preset::String="")

const placements = Ahorn.PlacementDict(
    "Hair Change Trigger (Hyperline)" => Ahorn.EntityPlacement(
        HairChangeTrigger,
        "rectangle"
    )
)

end