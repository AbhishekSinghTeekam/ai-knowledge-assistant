import { useCallback, useRef, useState } from 'react'
import { getAuthToken } from '../auth/tokenStore'

interface StreamHandlers {
  onToken: (token: string) => void
  onDone: () => void
  onError: (errorMessage: string) => void
}

interface UseEventSourceResult {
  isStreaming: boolean
  streamError: string | null
  startStream: (pathWithQuery: string, handlers: StreamHandlers) => Promise<void>
  stopStream: () => void
}

export function useEventSource(): UseEventSourceResult {
  const controllerRef = useRef<AbortController | null>(null)
  const [isStreaming, setIsStreaming] = useState(false)
  const [streamError, setStreamError] = useState<string | null>(null)

  const stopStream = useCallback(() => {
    controllerRef.current?.abort()
    controllerRef.current = null
    setIsStreaming(false)
  }, [])

  const startStream = useCallback(
    async (pathWithQuery: string, handlers: StreamHandlers) => {
      stopStream()
      setStreamError(null)
      setIsStreaming(true)

      const controller = new AbortController()
      controllerRef.current = controller

      try {
        const token = getAuthToken()
        const baseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5087'
        const response = await fetch(`${baseUrl}${pathWithQuery}`, {
          method: 'GET',
          headers: {
            Accept: 'text/event-stream',
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
          },
          signal: controller.signal,
        })

        if (!response.ok) {
          const errorText = await response.text()
          const message = errorText || `SSE request failed with status ${response.status}`
          setStreamError(message)
          handlers.onError(message)
          return
        }

        if (!response.body) {
          const message = 'SSE response body is empty.'
          setStreamError(message)
          handlers.onError(message)
          return
        }

        const decoder = new TextDecoder()
        const reader = response.body.getReader()
        let buffer = ''

        while (true) {
          const { value, done } = await reader.read()
          if (done) {
            break
          }

          buffer += decoder.decode(value, { stream: true })
          const events = buffer.split('\n\n')
          buffer = events.pop() ?? ''

          for (const eventChunk of events) {
            const dataLine = eventChunk
              .split('\n')
              .find((line) => line.startsWith('data:'))

            if (!dataLine) {
              continue
            }

            const payload = dataLine.slice('data:'.length).trim()

            if (payload === '[DONE]') {
              handlers.onDone()
              stopStream()
              return
            }

            try {
              const parsed = JSON.parse(payload) as { token?: string }
              if (parsed.token) {
                handlers.onToken(parsed.token)
              }
            } catch {
              // Ignore malformed chunks and keep stream alive.
            }
          }
        }

        handlers.onDone()
      } catch (error) {
        if ((error as Error).name !== 'AbortError') {
          const message = (error as Error).message || 'SSE stream failed.'
          setStreamError(message)
          handlers.onError(message)
        }
      } finally {
        setIsStreaming(false)
      }
    },
    [stopStream],
  )

  return {
    isStreaming,
    streamError,
    startStream,
    stopStream,
  }
}
