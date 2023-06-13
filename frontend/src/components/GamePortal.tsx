import { useEffect, useRef, useState } from 'react'
import GameEngine from '../classes/GameEngine'
import Connection from '../classes/Connection'

export default function GamePortal() {
  const container = useRef<HTMLDivElement>(null)

  const engineRef = useRef<GameEngine>()
  const connRef = useRef<Connection>()

  const [chatOpen, setChatOpen] = useState(false)
  const [chatText, setChatText] = useState('')

  useEffect(() => {
    if (!chatOpen && chatText) {
      engineRef.current?.say({ id: engineRef.current.selfId, text: chatText }, false)
      setChatText('')
    }
  }, [chatOpen, chatText])

  useEffect(() => {
    let co = false
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Enter') {
        co = !co
        setChatOpen(co)
        return
      }
      if (co && e.key) {
        setChatText(t => t + e.key)
        e.stopImmediatePropagation()
      }
    }
    window.addEventListener('keydown', onKeyDown)

    connRef.current = new Connection()
    engineRef.current = GameEngine.setup(container.current!, connRef.current)

    return () => {
      engineRef.current?.dispose()
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [])

  return <div ref={container}>

    { chatOpen &&
      <div className='absolute white border rounded-sm text-white bg-black/50 px-1.5 bottom-2 left-2 min-h-[26px]'>
        { chatText }
      </div>
    }
  </div>
}