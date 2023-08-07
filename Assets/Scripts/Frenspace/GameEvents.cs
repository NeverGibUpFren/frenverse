namespace GameEvents
{
  public enum GameEvent : byte
  {
    PLAYER,

    MOVE,

    ACTION,

    SOCIAL
  };

  public enum ActionEvent : byte
  {
    VEHICLE
  }

  public enum VehicleEvent : byte
  {
    SPAWN,
    DESPAWN,

    MOUNT,
    DISMOUNT
  }

  public enum PlayerEvent : byte
  {
    LIST,

    JOINED,
    LEFT,

    UPDATE,

    REQUEST,
    ASSIGN
  }

  public enum MoveEvent : byte
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

  public enum SocialEvent : byte
  {
    SAY,

    EMOTE
  }
}
