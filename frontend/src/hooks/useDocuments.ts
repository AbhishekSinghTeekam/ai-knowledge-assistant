import { useCallback } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  deleteDocument,
  type DocumentItem,
  listDocuments,
  uploadDocument,
} from '../api/documentsApi'

interface UseDocumentsResult {
  documents: DocumentItem[]
  isLoading: boolean
  isUploading: boolean
  error: string | null
  refreshDocuments: () => Promise<void>
  upload: (file: File) => Promise<void>
  remove: (documentId: string) => Promise<void>
}

export function useDocuments(): UseDocumentsResult {
  const queryClient = useQueryClient()

  const documentsQuery = useQuery({
    queryKey: ['documents'],
    queryFn: listDocuments,
  })

  const uploadMutation = useMutation({
    mutationFn: uploadDocument,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['documents'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: deleteDocument,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['documents'] })
    },
  })

  const refreshDocuments = useCallback(async () => {
    await documentsQuery.refetch()
  }, [documentsQuery])

  const upload = useCallback(
    async (file: File) => {
      await uploadMutation.mutateAsync(file)
    },
    [uploadMutation],
  )

  const remove = useCallback(
    async (documentId: string) => {
      await deleteMutation.mutateAsync(documentId)
    },
    [deleteMutation],
  )

  const errorMessage =
    (uploadMutation.error as Error | null)?.message ??
    (deleteMutation.error as Error | null)?.message ??
    (documentsQuery.error as Error | null)?.message ??
    null

  return {
    documents: documentsQuery.data ?? [],
    isLoading: documentsQuery.isLoading,
    isUploading: uploadMutation.isPending,
    error: errorMessage,
    refreshDocuments,
    upload,
    remove,
  }
}
