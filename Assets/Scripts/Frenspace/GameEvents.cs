namespace GameEvents
{
  public enum GameEvent : byte
  {
    PLAYER,

    MOVE,

    SOCIAL
  };

  public enum PlayerEvent : byte
  {
    LIST,

    JOINED,
    LEFT,

    UPDATE
  }

  public enum MoveEvent : byte
  {
    NORTH,
    SOUTH,
    EAST,
    WEST,

    STOPPED,

    PORT
  }

  public enum SocialEvent : byte
  {
    SAY,

    EMOTE
  }
}
