namespace GameStates
{
    public enum GameState : byte
    {
        INACTIVE,

        ACTIVE
    };

    public enum InstanceState : byte
    {
        WORLD,

        APARTMENT,

        POI
    }

    public enum MovementState : byte
    {
        NORTH,
        SOUTH,
        EAST,
        WEST,
        UP,
        DOWN,

        STOPPED,

        PORT
    }

    public enum VehicleState : byte
    {
        UNMOUNTED,

        CAR,
        HOVERCAR,

        HELICOPTER
    };
}
