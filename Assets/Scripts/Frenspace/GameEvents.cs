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
    INSTANCE,

    VEHICLE,

    MEDIA
  }

  public enum MediaEvent : byte
  {
    TV,

    MUSIC
  }

  public enum BaseMediaEvent : byte
  {
    PLAY,
    PAUSE
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



  public enum SocialEvent : byte
  {
    SAY,

    EMOTE
  }
}
