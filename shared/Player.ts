enum PlayerEvent {
  LIST,

  JOINED,
  LEFT,
  
  UPDATE
}

export default PlayerEvent

export interface Player {
  pos: { x: number, y: number },
  id: string
}