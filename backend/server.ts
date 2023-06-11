import { ServerWebSocket } from 'bun'
import { randomUUID } from 'crypto'

import GameEvent from '../shared/GameEvent'
import Player from '../shared/Player'

import { Packet, pack, unpack } from '../shared/Packet'

type WSUnique = ServerWebSocket & { id?: string }

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
      wss.publish('broadcast', pack([GameEvent.PLAYER, Player.JOINED], ws.id))
      ws.subscribe('broadcast')
    },
    message(ws: WSUnique, message) {
      console.log(ws.id, message)
      const [e, payload] = unpack(message as Packet)
      switch (e[0]) {
        case GameEvent.MOVE: {
          ws.publish('broadcast', message + '|' + ws.id)
          break
        }
        default: {
          ws.publish('broadcast', message)
          break
        }
      }

    }
  }
})

console.log('Server listening: ' + wss.hostname + ':' + wss.port)