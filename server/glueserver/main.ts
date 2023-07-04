import * as ytdl from 'ytdl-core'

import * as path from 'path'
import * as express from 'express'

import { randomUUID } from 'crypto'

import { PeerConnection, DataChannel, DescriptionType } from 'node-datachannel'
import * as bodyParser from 'body-parser'
import * as cookieParser from 'cookie-parser'

// Creating express app
const app = express()

// Serving static files in the public folder
app.use('/', express.static(path.join(__dirname, '/public')))

app.use(bodyParser.json())
app.use(cookieParser())

const peers = new Map<string, { con: PeerConnection, udp: DataChannel, tcp: DataChannel }>()

app.post('/sdp', (req, res) => {
  if (req.body.requestSdp) {
    const id = randomUUID()

    const con = new PeerConnection('Server', { iceServers: ['stun:stun.l.google.com:19302'] })

    con.onStateChange(e => {
      console.log('state change', id, e)
    })

    const candidates: { c: string, m: string }[] = []
    con.onLocalCandidate((candidate, mid) => {
      candidates.push({ c: candidate, m: mid })
    })

    con.onGatheringStateChange(s => {
      if (s === 'complete') {
        con.setLocalDescription(DescriptionType.Offer)

        res.cookie('session', id).send(JSON.stringify({
          desc: {
            sdp: con.localDescription()!.sdp,
            type: DescriptionType.Offer
          },
          candidates
        }))
      }
    })

    const udp = con.createDataChannel('UDP', { ordered: true, maxRetransmits: 0 })
    const tcp = con.createDataChannel('TCP')

    peers.set(id, { con, udp, tcp })

  } else {
    if (!req.cookies?.session) {
      res.status(500).send()
      return
    }

    const id = req.cookies!.session
    const peer = peers.get(id)!

    peer.con.setRemoteDescription(req.body.desc.sdp, req.body.desc.type)

    for (const candidate of req.body.candidates) {
      peer.con.addRemoteCandidate(candidate.c, candidate.m)
    }

    res.send()
  }
})

app.get('/yt', (req, res) => {
  if (!req.query?.url) {
    res.status(406).send()
    return
  }

  ytdl.getInfo(req.query!.url as string, { requestOptions: {  } })
    .then(info => {
      res.send(info.formats.filter(f => f.hasAudio && f.hasVideo)?.[0]?.url)
    })
    .catch(() => res.status(406).send())
})

const port = 80
app.listen(port, () => {
  console.log(`server listening at http://localhost:${port}`)
})