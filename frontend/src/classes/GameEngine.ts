import { Application, BitmapFont, BitmapText, Container, Graphics, Sprite } from 'pixi.js'

import Connection from './Connection'

import apuSrc from '../assets/apu.webp'

import GameEvent from '../../../shared/GameEvent'
import MovementEvent from '../../../shared/Movement'
import PlayerEvent, { Player } from '../../../shared/Player'
import SayEvent, { Say } from '../../../shared/Say'
import { Packet, unpack } from '../../../shared/Packet'

enum INDICES {
  Player,
  Text
}

BitmapFont.from('Mono', {
  fontFamily: 'monospace',
  fontSize: 16,
  fill: 'white'
}, { chars: BitmapFont.ALPHANUMERIC })

export default class GameEngine {
  app: Application

  private sprites = {
    apu: () => Sprite.from(apuSrc)
  }

  constructor() {
    this.app = new Application({ background: '#127252' })

    let elapsed = 0.0
    this.app.ticker.add((delta) => {
      elapsed += delta
      this.onTick(delta, elapsed)
    })
  }

  private container?: HTMLDivElement
  private conn?: Connection

  public static setup(container: HTMLDivElement, conn: Connection) {
    const instance = new GameEngine()
    instance.container = container
    instance.conn = conn

    instance.fitToParent()

    container.parentElement?.append(instance.app.view as unknown as HTMLCanvasElement)

    conn.$message.subscribe(instance.onGameEvent.bind(instance))

    instance.onKeyDown = instance.onKeyDown.bind(instance)
    instance.onKeyUp = instance.onKeyUp.bind(instance)
    window.addEventListener('keydown', instance.onKeyDown)
    window.addEventListener('keyup', instance.onKeyUp)

    return instance
  }

  private getParentDim() {
    return [this.container!.parentElement!.clientHeight, this.container!.parentElement!.clientWidth]
  }

  fitToParent() {
    const [h, w] = this.getParentDim()
    this.app.renderer.resize(w, h)
  }

  private speed = 4

  onTick(delta: number, elapsed: number) {
    const moveAmt = (this.speed * delta)

    for (const id in this.players) {
      const p = this.players[id]
      switch (p.movement) {
        case MovementEvent.NORTH: p.container.y -= moveAmt; break
        case MovementEvent.SOUTH: p.container.y += moveAmt; break
        case MovementEvent.EAST: p.container.x += moveAmt; break
        case MovementEvent.WEST: p.container.x -= moveAmt; break
        default: break
      }
    }
  }

  onGameEvent(message: string) {
    const [e, payload] = unpack(message as Packet)
    switch(e[0]) {
      case GameEvent.PLAYER:
        switch (e[1]) {
          case PlayerEvent.JOINED: this.addPlayer(JSON.parse(payload) as Player); break
          case PlayerEvent.LEFT: this.removePlayer(payload); break
          case PlayerEvent.LIST: {
            const players = JSON.parse(payload) as { [key: string]: Player } & { self: string }
            this.selfId = players.self
            delete (players as { self?: unknown })['self']
            for (const id in players) {
              this.addPlayer(players[id])
            }
            break
          }
        }
        break
      case GameEvent.MOVE:
        this.players[payload].movement = e[1] as MovementEvent
        break
      case GameEvent.SAY:
        switch(e[1]) {
          case SayEvent.DEFAULT: this.say(JSON.parse(payload)); break
        }
        break
    }
  }

  public selfId = ''
  public players: { [key: string]: Player & { movement: MovementEvent, container: Container } } = {}

  addPlayer(player: Player) {
    const container = new Container()

    container.addChild(this.sprites.apu())

    // const background = new Graphics()
    // background.beginFill(0xffffff)
    // background.drawRect(0, 0, 200, 100)

    // container.addChild(background)

    const txt = new BitmapText('Test', { fontName: 'Mono', align: 'center' })
    txt.position.y = -20
    txt.visible = false

    container.addChild(txt)

    this.app.stage.addChild(container)

    container.x = player.pos.x
    container.y = player.pos.y

    this.players[player.id] = {
      ...player,
      container,
      movement: MovementEvent.STOPPED
    }
  }

  removePlayer(id: string) {
    const p = this.players[id]
    this.app.stage.removeChild(p.container)
    delete this.players[id]
  }

  say(info: Say, incomming = true) {
    const p = this.players[info.id]
    const t = p.container.getChildAt(INDICES.Text) as BitmapText
    t.text = info.text
    t.dirty = true
    t.visible = true
    setTimeout(() => t.visible = false, 5000) // TODO: make this better

    !incomming && this.conn?.send([GameEvent.SAY, SayEvent.DEFAULT], JSON.stringify(info.text))
  }

  private moveKeyMap: { [key: string]: MovementEvent } = {
    w: MovementEvent.NORTH,
    s: MovementEvent.SOUTH,
    d: MovementEvent.EAST,
    a: MovementEvent.WEST
  }

  private pressedKeys: { [key: string]: boolean }  = { w: false, s: false, d: false, a: false }

  onKeyDown(e: KeyboardEvent, force = false) {
    if (force || (this.moveKeyMap[e.key] !== undefined && !this.pressedKeys[e.key])) {
      this.conn!.send([GameEvent.MOVE, this.moveKeyMap[e.key]])
      this.players[this.selfId].movement = this.moveKeyMap[e.key]
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
        this.conn?.send([GameEvent.MOVE, MovementEvent.STOPPED])
        this.players[this.selfId].movement = MovementEvent.STOPPED
      }
    }
  }

  dispose() {
    this.conn?.dispose()
    if (this.container) {
      window.removeEventListener('keydown', this.onKeyDown)
      window.removeEventListener('keyup', this.onKeyUp)
    }
    this.app.destroy(true)
  }

}