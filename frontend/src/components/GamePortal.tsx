import { useEffect, useRef } from 'react'
import GameEngine from '../classes/GameEngine'
import Connection from '../classes/Connection'

export default function GamePortal() {
  const container = useRef<HTMLDivElement>(null)

  const engineRef = useRef<GameEngine>()
  const connRef = useRef<Connection>()

  useEffect(() => {
    connRef.current = new Connection()
    engineRef.current = new GameEngine(container.current!, connRef.current)

    return () => engineRef.current?.dispose()
  }, [])

  return <div ref={container} />
}