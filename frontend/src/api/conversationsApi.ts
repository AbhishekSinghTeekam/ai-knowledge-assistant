import apiClient from './apiClient'

export interface Conversation {
  id: string
  title: string
  createdAt: string
  updatedAt: string
  messageCount: number
}

export interface ConversationListResponse {
  conversations: Conversation[]
}

export interface ConversationMessage {
  id: string
  role: string
  content: string
  createdAt: string
}

export interface ConversationMessagesResponse {
  conversationId: string
  title: string
  messages: ConversationMessage[]
}

export interface CreateConversationResponse {
  id: string
  title: string
  createdAt: string
}

export async function listConversations(): Promise<Conversation[]> {
  const response = await apiClient.get<ConversationListResponse>('/api/conversations')
  return response.data.conversations
}

export async function createConversation(title = ''): Promise<CreateConversationResponse> {
  const response = await apiClient.post<CreateConversationResponse>('/api/conversations', {
    title,
  })

  return response.data
}

export async function getConversationMessages(
  conversationId: string,
): Promise<ConversationMessagesResponse> {
  const response = await apiClient.get<ConversationMessagesResponse>(
    `/api/conversations/${conversationId}/messages`,
  )

  return response.data
}
