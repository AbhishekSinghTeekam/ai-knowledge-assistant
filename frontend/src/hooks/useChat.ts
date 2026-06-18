import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  type Conversation,
  type ConversationMessage,
  createConversation,
  getConversationMessages,
  listConversations,
} from '../api/conversationsApi'
import { useEventSource } from './useEventSource'

export interface ChatMessage {
  id: string
  role: 'user' | 'assistant'
  content: string
  createdAt: string
}

interface UseChatResult {
  conversations: Conversation[]
  activeConversationId: string | null
  messages: ChatMessage[]
  isLoadingConversations: boolean
  isLoadingMessages: boolean
  isSending: boolean
  error: string | null
  createNewConversation: () => Promise<void>
  selectConversation: (conversationId: string) => Promise<void>
  sendMessage: (question: string, topK?: number) => Promise<void>
}

function normalizeRole(role: string): 'user' | 'assistant' {
  return role.toLowerCase().includes('assistant') ? 'assistant' : 'user'
}

function mapMessage(message: ConversationMessage): ChatMessage {
  return {
    id: message.id,
    role: normalizeRole(message.role),
    content: message.content,
    createdAt: message.createdAt,
  }
}

export function useChat(): UseChatResult {
  const queryClient = useQueryClient()
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null)
  const [localMessages, setLocalMessages] = useState<ChatMessage[] | null>(null)
  const [isSending, setIsSending] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const autoCreateAttemptedRef = useRef(false)

  const { isStreaming, startStream } = useEventSource()

  const conversationsQuery = useQuery({
    queryKey: ['conversations'],
    queryFn: listConversations,
  })

  const createConversationMutation = useMutation({
    mutationFn: () => createConversation(''),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['conversations'] })
    },
  })

  const messagesQuery = useQuery({
    queryKey: ['conversationMessages', activeConversationId],
    enabled: Boolean(activeConversationId),
    queryFn: async () => {
      const response = await getConversationMessages(activeConversationId as string)
      return response.messages.map(mapMessage)
    },
  })

  const conversations = conversationsQuery.data ?? []
  const messages = localMessages ?? messagesQuery.data ?? []

  const createNewConversation = useCallback(async () => {
    setError(null)

    try {
      const created = await createConversationMutation.mutateAsync()
      setActiveConversationId(created.id)
      setLocalMessages([])
    } catch (requestError) {
      setError((requestError as Error).message || 'Failed to create conversation.')
    }
  }, [createConversationMutation])

  const selectConversation = useCallback(async (conversationId: string) => {
    setError(null)
    setLocalMessages(null)
    setActiveConversationId(conversationId)
  }, [])

  const sendMessage = useCallback(
    async (question: string, topK = 5) => {
      if (!activeConversationId || !question.trim() || isStreaming) {
        return
      }

      setError(null)
      setIsSending(true)

      const now = new Date().toISOString()
      const userMessage: ChatMessage = {
        id: `local-user-${crypto.randomUUID()}`,
        role: 'user',
        content: question.trim(),
        createdAt: now,
      }

      const streamingId = 'local-stream-assistant'
      const assistantPlaceholder: ChatMessage = {
        id: streamingId,
        role: 'assistant',
        content: '',
        createdAt: now,
      }

      setLocalMessages((current) => [
        ...(current ?? messagesQuery.data ?? []),
        userMessage,
        assistantPlaceholder,
      ])

      const query = new URLSearchParams({
        q: question.trim(),
        topK: String(topK),
      })

      try {
        await startStream(`/api/chat/${activeConversationId}/stream?${query.toString()}`, {
          onToken: (token) => {
            setLocalMessages((current) =>
              (current ?? []).map((message) =>
                message.id === streamingId
                  ? { ...message, content: `${message.content}${token}` }
                  : message,
              ),
            )
          },
          onDone: () => {
            setLocalMessages((current) =>
              (current ?? []).map((message) =>
                message.id === streamingId
                  ? {
                      ...message,
                      id: `local-assistant-${crypto.randomUUID()}`,
                      content: message.content.trim(),
                    }
                  : message,
              ),
            )
          },
          onError: (streamError) => {
            setError(streamError)
            setLocalMessages((current) =>
              (current ?? []).filter((message) => message.id !== streamingId),
            )
          },
        })

        await queryClient.invalidateQueries({
          queryKey: ['conversationMessages', activeConversationId],
        })
        await queryClient.refetchQueries({
          queryKey: ['conversationMessages', activeConversationId],
        })
        await queryClient.invalidateQueries({ queryKey: ['conversations'] })
        setLocalMessages(null)
      } finally {
        setIsSending(false)
      }
    },
    [activeConversationId, isStreaming, messagesQuery.data, queryClient, startStream],
  )

  useEffect(() => {
    if (conversationsQuery.isError) {
      setError(
        (conversationsQuery.error as Error).message || 'Failed to initialize chat.',
      )
      return
    }

    if (!conversationsQuery.isSuccess) {
      return
    }

    if (conversations.length === 0) {
      if (!autoCreateAttemptedRef.current && !createConversationMutation.isPending) {
        autoCreateAttemptedRef.current = true
        void createNewConversation()
      }
      return
    }

    autoCreateAttemptedRef.current = false

    if (!activeConversationId) {
      setActiveConversationId(conversations[0].id)
      setLocalMessages(null)
    }
  }, [
    activeConversationId,
    conversations,
    conversationsQuery.error,
    conversationsQuery.isError,
    conversationsQuery.isSuccess,
    createConversationMutation.isPending,
    createNewConversation,
  ])

  useEffect(() => {
    if (messagesQuery.isError) {
      setError((messagesQuery.error as Error).message || 'Failed to load messages.')
    }
  }, [messagesQuery.error, messagesQuery.isError])

  return useMemo(
    () => ({
      conversations,
      activeConversationId,
      messages,
      isLoadingConversations:
        conversationsQuery.isLoading || createConversationMutation.isPending,
      isLoadingMessages: messagesQuery.isLoading,
      isSending: isSending || isStreaming,
      error,
      createNewConversation,
      selectConversation,
      sendMessage,
    }),
    [
      conversations,
      activeConversationId,
      messages,
      conversationsQuery.isLoading,
      createConversationMutation.isPending,
      messagesQuery.isLoading,
      isSending,
      isStreaming,
      error,
      createNewConversation,
      selectConversation,
      sendMessage,
    ],
  )
}
