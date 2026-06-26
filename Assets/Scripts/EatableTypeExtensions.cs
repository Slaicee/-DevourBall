public static class EatableTypeExtensions
{
    public static Eatable.EatableType TagToType(string tag)
    {
        return tag switch
        {
            "Human" => Eatable.EatableType.Human,
            "Animal" => Eatable.EatableType.Animal,
            "Plant" => Eatable.EatableType.Plant,
            "Vehicle" => Eatable.EatableType.Vehicle,
            "Building" => Eatable.EatableType.Building,
            "Street" => Eatable.EatableType.Street,
            _ => Eatable.EatableType.Other,
        };
    }

    public static string ToTag(Eatable.EatableType type)
    {
        return type switch
        {
            Eatable.EatableType.Human => "Human",
            Eatable.EatableType.Animal => "Animal",
            Eatable.EatableType.Plant => "Plant",
            Eatable.EatableType.Vehicle => "Vehicle",
            Eatable.EatableType.Building => "Building",
            Eatable.EatableType.Street => "Street",
            _ => "Other",
        };
    }
}
