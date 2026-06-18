import apiClient from './apiClient'

export interface DocumentItem {
  id: string
  fileName: string
  contentType: string
  fileSizeBytes: number
  status: string
  uploadedAt: string
  processedAt: string | null
  chunkCount: number
}

interface DocumentListResponse {
  documents: DocumentItem[]
}

export interface IngestDocumentResponse {
  documentId: string
  fileName: string
  status: string
  uploadedAt: string
}

export async function listDocuments(): Promise<DocumentItem[]> {
  const response = await apiClient.get<DocumentListResponse>('/api/documents')
  return response.data.documents
}

export async function uploadDocument(file: File): Promise<IngestDocumentResponse> {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post<IngestDocumentResponse>(
    '/api/documents/ingest',
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    },
  )

  return response.data
}

export async function deleteDocument(documentId: string): Promise<void> {
  await apiClient.delete(`/api/documents/${documentId}`)
}
