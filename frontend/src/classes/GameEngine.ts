import { Application, Sprite } from 'pixi.js'

import Connection from './Connection'

import apuSrc from '../assets/apu.webp'

import GameEvent from '../../../shared/GameEvent'
import Movement from '../../../shared/Movement'
import Player from '../../../shared/Player'
import { Packet, unpack } from '../../../shared/Packet'

export default class GameEngine {
  app: Application

  private player: Sprite

  private sprites = {
    apu: () => Sprite.from(apuSrc)
  }

  constructor(private container: HTMLDivElement, private conn: Connection) {
    this.app = new Application({ background: '#127252' })
    this.fitToParent()
    container.parentElement?.append(this.app.view as unknown as HTMLCanvasElement)

    this.player = this.app.stage.addChild(this.sprites.apu())

    this.conn.$message.subscribe(this.onGameEvent.bind(this))

    this.onKeyDown = this.onKeyDown.bind(this)
    this.onKeyUp = this.onKeyUp.bind(this)
    window.addEventListener('keydown', this.onKeyDown)
    window.addEventListener('keyup', this.onKeyUp)

    let elapsed = 0.0
    this.app.ticker.add((delta) => {
      elapsed += delta
      this.onTick(delta, elapsed)
    })
  }

  private getParentDim() {
    return [this.container.parentElement!.clientHeight, this.container.parentElement!.clientWidth]
  }

  fitToParent() {
    const [h, w] = this.getParentDim()
    this.app.renderer.resize(w, h)
  }

  private currentMovement = Movement.STOPPED
  private speed = 4

  onTick(delta: number, elapsed: number) {
    const moveAmt = (this.speed * delta)
    // self
    switch (this.currentMovement) {
      case Movement.NORTH: this.player.y -= moveAmt; break
      case Movement.SOUTH: this.player.y += moveAmt; break
      case Movement.EAST: this.player.x += moveAmt; break
      case Movement.WEST: this.player.x -= moveAmt; break
      default: break
    }

    // other players
    for (const id in this.players) {
      const p = this.players[id]
      switch (p.movement) {
        case Movement.NORTH: p.sprite.y -= moveAmt; break
        case Movement.SOUTH: p.sprite.y += moveAmt; break
        case Movement.EAST: p.sprite.x += moveAmt; break
        case Movement.WEST: p.sprite.x -= moveAmt; break
        default: break
      }
    }
  }

  onGameEvent(message: string) {
    const [e, payload] = unpack(message as Packet)
    switch(e[0]) {
      case GameEvent.PLAYER:
        switch (e[1]) {
          case Player.JOINED: this.addPlayer(payload as string); break
        }
        break
      case GameEvent.MOVE:
        this.players[payload as string].movement = e[1] as Movement
        break
    }
  }

  public players: { [key: string]: { id: string, movement: Movement, sprite: Sprite } } = {}
  addPlayer(id: string) {
    const s = this.app.stage.addChild(this.sprites.apu())
    this.players[id] = {
      id,
      movement: Movement.STOPPED,
      sprite: s
    }
  }

  private moveKeyMap: { [key: string]: Movement } = {
    w: Movement.NORTH,
    s: Movement.SOUTH,
    d: Movement.EAST,
    a: Movement.WEST
  }

  private pressedKeys: { [key: string]: boolean }  = { w: false, s: false, d: false, a: false }

  onKeyDown(e: KeyboardEvent, force = false) {
    if (!this.pressedKeys[e.key] || force) {
      this.conn.send([GameEvent.MOVE, this.moveKeyMap[e.key]])
      this.currentMovement = this.moveKeyMap[e.key]
      this.pressedKeys[e.key] = true
    }
  }

  onKeyUp(e: KeyboardEvent) {
    if (this.pressedKeys[e.key]) {
      this.pressedKeys[e.key] = false
      const otherKeyPress = Object.keys(this.pressedKeys).map(k => [k, this.pressedKeys[k]])
        .find(a => a[1])?.[0]

      if (otherKeyPress) {
        this.onKeyDown({ key: otherKeyPress } as unknown as KeyboardEvent, true) // simulate other keypress
      } else {
        this.conn.send([GameEvent.MOVE, Movement.STOPPED])
        this.currentMovement = Movement.STOPPED
      }
    }
  }

  dispose() {
    this.conn.dispose()
    window.removeEventListener('keydown', this.onKeyDown)
    window.removeEventListener('keyup', this.onKeyUp)
    this.app.destroy(true)
  }

}