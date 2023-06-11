import { Subject, firstValueFrom } from 'rxjs'
import { filter, first, map, scan } from 'rxjs/operators'

import { Packet, pack, unpack } from '../../../shared/Packet'

export default class Connection {

  public $state = new Subject()
  public $message = new Subject<string>()
  public $error = new Subject()

  private socket: WebSocket

  constructor() {
    this.socket = new WebSocket('ws://localhost:3000')

    this.socket.addEventListener('open', e => this.$state.next({ event: 'open', value: e }))
    this.socket.addEventListener('close', e => this.$state.next({ event: 'close', value: e }))

    this.socket.addEventListener('message', e => this.$message.next(e.data))

    this.socket.addEventListener('error', e => this.$error.next(e))
  }

  send(e: number[], payload?: string) {
    this.socket.send(pack(e, payload))
  }

  dispose() {
    this.socket.close()
  }

}