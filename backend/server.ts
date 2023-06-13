import { ServerWebSocket } from 'bun'
import { randomUUID } from 'crypto'

import GameEvent from '../shared/GameEvent'
import PlayerEvent, { Player } from '../shared/Player'
import { Say } from '../shared/Say'

import { Packet, pack, unpack } from '../shared/Packet'

type WSUnique = ServerWebSocket & { id?: string }

const players: { [key: string]: Player } = {}

const wss = Bun.serve({
  async fetch(req, server) {
    // http req
    // if (req.url.indexOf('') > -1) {
    //  return new Response()
    // }
    // upgrade the request to a WebSocket
    if (server.upgrade(req)) {
      return // do not return a Response
    }
    return new Response('No action specified.', { status: 500 })
  },
  websocket: {
    open(ws: WSUnique) {
      ws.id = randomUUID()
      players[ws.id] = { id: ws.id, pos: { x: 0, y: 0 } }

      wss.publish('broadcast', pack([GameEvent.PLAYER, PlayerEvent.JOINED], JSON.stringify(players[ws.id])))

      ws.send(pack([GameEvent.PLAYER, PlayerEvent.LIST], JSON.stringify({...players, self: ws.id })))
      ws.subscribe('broadcast')
      console.log(`${ws.id} joined`)
    },
    message(ws: WSUnique, message) {
      console.log(ws.id, message)
      // TODO: unpack has to throw
      const [e, payload, never] = unpack(message as Packet)
      if (never !== undefined) {
        // message is not in the correct format, close socket
        ws.close()
        return
      }

      switch (e[0]) {
        case GameEvent.MOVE: {
          ws.publish('broadcast', pack(e, ws.id))
          break
        }
        case GameEvent.SAY: {
          ws.publish('broadcast', pack(e, JSON.stringify({ id: ws.id, text: payload })))
          break
        }
        default: {
          ws.publish('broadcast', message)
          break
        }
      }
    },
    close(ws: WSUnique) {
      delete players[ws.id!]
      wss.publish('broadcast', pack([GameEvent.PLAYER, PlayerEvent.LEFT], ws.id!))
      console.log(`${ws.id} left`)
    }
  }
})

console.log('Server listening: ' + wss.hostname + ':' + wss.port)