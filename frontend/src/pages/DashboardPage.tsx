import { useNavigate } from 'react-router-dom'
import ChatPanel from '../components/ChatPanel'
import DocumentList from '../components/DocumentList'
import UploadPanel from '../components/UploadPanel'
import { useChat } from '../hooks/useChat'
import { useAuth } from '../hooks/useAuth'
import { useDocuments } from '../hooks/useDocuments'

export default function DashboardPage() {
  const navigate = useNavigate()
  const { session, signOut } = useAuth()
  const {
    conversations,
    activeConversationId,
    messages,
    isLoadingConversations,
    isLoadingMessages,
    isSending,
    error: chatError,
    createNewConversation,
    selectConversation,
    sendMessage,
  } = useChat()
  const {
    documents,
    isLoading: isLoadingDocuments,
    isUploading,
    error: documentsError,
    refreshDocuments,
    upload,
    remove,
  } = useDocuments()

  const onSignOut = () => {
    signOut()
    navigate('/login', { replace: true })
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-[1400px] px-4 py-8 md:px-8">
      <header className="mb-6 rounded-3xl border border-stone-900/10 bg-white/75 p-5 shadow-[0_18px_60px_rgba(0,0,0,0.08)] backdrop-blur sm:p-6">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.15em] text-teal-800">
              AI Knowledge Assistant
            </p>
            <h1 className="mt-2 text-3xl text-stone-900 sm:text-4xl">
              Welcome back, {session?.name ?? 'User'}
            </h1>
            <p className="mt-2 text-sm text-stone-700">{session?.email}</p>
          </div>

          <button
            type="button"
            onClick={onSignOut}
            className="rounded-xl bg-stone-900 px-4 py-2.5 text-sm font-semibold text-amber-50 transition hover:bg-stone-800"
          >
            Sign out
          </button>
        </div>
      </header>

      <section className="grid gap-6 lg:grid-cols-[340px_1fr]">
        <aside className="space-y-6">
          <section className="rounded-3xl border border-stone-900/10 bg-white/75 p-5 shadow-[0_18px_60px_rgba(0,0,0,0.08)] backdrop-blur sm:p-6">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-2xl text-stone-900">Conversations</h2>
              <button
                type="button"
                onClick={() => void createNewConversation()}
                className="rounded-lg border border-stone-300 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-stone-700 transition hover:bg-stone-100"
              >
                New
              </button>
            </div>

            {isLoadingConversations ? (
              <div className="space-y-3">
                {Array.from({ length: 3 }).map((_, index) => (
                  <div key={index} className="h-12 animate-pulse rounded-xl bg-stone-200/80" />
                ))}
              </div>
            ) : (
              <ul className="space-y-2">
                {conversations.map((conversation) => (
                  <li key={conversation.id}>
                    <button
                      type="button"
                      onClick={() => void selectConversation(conversation.id)}
                      className={`w-full rounded-xl border px-3 py-2 text-left transition ${
                        conversation.id === activeConversationId
                          ? 'border-teal-700 bg-teal-50 text-teal-900'
                          : 'border-stone-300 bg-white text-stone-800 hover:bg-stone-50'
                      }`}
                    >
                      <p className="truncate text-sm font-semibold">
                        {conversation.title || 'Untitled conversation'}
                      </p>
                      <p className="mt-1 text-xs opacity-80">
                        {conversation.messageCount} messages
                      </p>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <UploadPanel
            isUploading={isUploading}
            onUpload={async (file) => {
              await upload(file)
              await refreshDocuments()
            }}
          />

          <DocumentList
            documents={documents}
            isLoading={isLoadingDocuments}
            error={documentsError}
            onRefresh={refreshDocuments}
            onDelete={remove}
          />
        </aside>

        <ChatPanel
          messages={messages}
          isLoading={isLoadingMessages}
          isSending={isSending}
          error={chatError}
          onSend={async (question) => {
            await sendMessage(question, 5)
          }}
        />
      </section>
    </main>
  )
}
