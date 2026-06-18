import { useMemo, useState } from 'react'
import ReactMarkdown from 'react-markdown'
import type { ChatMessage } from '../hooks/useChat'
import ChatSkeleton from './ChatSkeleton'

interface ChatPanelProps {
  messages: ChatMessage[]
  isLoading: boolean
  isSending: boolean
  error: string | null
  onSend: (question: string) => Promise<void>
}

function formatTimestamp(isoDate: string): string {
  const parsed = new Date(isoDate)
  if (Number.isNaN(parsed.getTime())) {
    return ''
  }

  return parsed.toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
  })
}

export default function ChatPanel({
  messages,
  isLoading,
  isSending,
  error,
  onSend,
}: ChatPanelProps) {
  const [question, setQuestion] = useState('')

  const emptyState = useMemo(() => !messages.length && !isLoading, [messages.length, isLoading])

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmed = question.trim()
    if (!trimmed || isSending) {
      return
    }

    setQuestion('')
    await onSend(trimmed)
  }

  return (
    <section className="flex h-[72vh] min-h-[560px] flex-col rounded-3xl border border-stone-900/10 bg-white/75 shadow-[0_18px_60px_rgba(0,0,0,0.08)] backdrop-blur">
      <header className="flex items-center justify-between border-b border-stone-200/80 px-4 py-3 sm:px-6">
        <h2 className="text-2xl text-stone-900">Chat</h2>
        <p className="text-xs uppercase tracking-[0.12em] text-stone-500">SSE streaming</p>
      </header>

      <div className="flex-1 overflow-y-auto px-4 py-4 sm:px-6">
        {isLoading ? (
          <ChatSkeleton />
        ) : (
          <div className="space-y-4">
            {emptyState && (
              <div className="rounded-2xl border border-dashed border-stone-300 bg-amber-50/70 p-5 text-stone-700">
                Ask a question about your uploaded documents to start a conversation.
              </div>
            )}

            {messages.map((message) => {
              const isUser = message.role === 'user'

              return (
                <article
                  key={message.id}
                  className={`flex ${isUser ? 'justify-end' : 'justify-start'}`}
                >
                  <div
                    className={`max-w-[85%] rounded-2xl px-4 py-3 shadow-sm sm:max-w-[78%] ${
                      isUser
                        ? 'bg-stone-900 text-amber-50'
                        : 'border border-teal-200 bg-teal-50 text-stone-900'
                    }`}
                  >
                    <div
                      className={`prose prose-sm max-w-none ${
                        isUser ? 'prose-invert prose-p:text-amber-50' : 'prose-p:text-stone-800'
                      }`}
                    >
                      <ReactMarkdown>{message.content || (isSending ? '...' : '')}</ReactMarkdown>
                    </div>

                    <p
                      className={`mt-2 text-[11px] ${
                        isUser ? 'text-amber-100/80' : 'text-stone-500'
                      }`}
                    >
                      {formatTimestamp(message.createdAt)}
                    </p>
                  </div>
                </article>
              )
            })}
          </div>
        )}
      </div>

      {error && (
        <div className="mx-4 mb-3 rounded-xl border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700 sm:mx-6">
          {error}
        </div>
      )}

      <form
        onSubmit={handleSubmit}
        className="flex gap-2 border-t border-stone-200/80 px-4 py-3 sm:px-6"
      >
        <input
          type="text"
          value={question}
          onChange={(event) => setQuestion(event.target.value)}
          placeholder="Ask about architecture, auth, RAG pipeline..."
          className="flex-1 rounded-xl border border-stone-300 bg-white px-4 py-3 text-sm text-stone-900 outline-none ring-amber-300 transition focus:border-amber-500 focus:ring"
        />

        <button
          type="submit"
          disabled={isSending}
          className="rounded-xl bg-teal-800 px-4 py-3 text-sm font-semibold text-amber-50 transition hover:bg-teal-700 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isSending ? 'Sending...' : 'Send'}
        </button>
      </form>
    </section>
  )
}
