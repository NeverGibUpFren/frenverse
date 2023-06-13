export type Packet = string

export function pack(e: number[], payload?: string): Packet {
  return e.map(n => String(n)).reduce((p, c, i) => p + (i === 0 ? '' : ':') + c, '') + (payload ? ('|' + payload) : '') 
}

export function unpack<T = string>(pkt: Packet, formatter?: (i: string) => T) {
  const [events, payload, never] = pkt.split('|')
  return [events.split(':').map(e => Number(e)), formatter ? formatter(payload) : (payload as T), never] as const
}