import type { DocumentItem } from '../api/documentsApi'

interface DocumentListProps {
  documents: DocumentItem[]
  isLoading: boolean
  error: string | null
  onRefresh: () => Promise<void>
  onDelete: (documentId: string) => Promise<void>
}

function toHumanSize(bytes: number): string {
  if (bytes <= 0) {
    return '0 B'
  }

  const units = ['B', 'KB', 'MB', 'GB']
  const index = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1)
  const value = bytes / 1024 ** index

  return `${value.toFixed(index === 0 ? 0 : 1)} ${units[index]}`
}

export default function DocumentList({
  documents,
  isLoading,
  error,
  onRefresh,
  onDelete,
}: DocumentListProps) {
  return (
    <section className="rounded-3xl border border-stone-900/10 bg-white/75 p-5 shadow-[0_18px_60px_rgba(0,0,0,0.08)] backdrop-blur sm:p-6">
      <header className="mb-4 flex items-center justify-between">
        <h3 className="text-2xl text-stone-900">Documents</h3>
        <button
          type="button"
          onClick={() => void onRefresh()}
          className="rounded-lg border border-stone-300 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-stone-700 transition hover:bg-stone-100"
        >
          Refresh
        </button>
      </header>

      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 3 }).map((_, index) => (
            <div key={index} className="h-16 animate-pulse rounded-xl bg-stone-200/80" />
          ))}
        </div>
      ) : documents.length === 0 ? (
        <p className="rounded-xl border border-dashed border-stone-300 bg-amber-50/70 p-4 text-sm text-stone-700">
          No documents ingested yet. Upload a PDF, TXT, or DOCX to begin.
        </p>
      ) : (
        <ul className="space-y-3">
          {documents.map((document) => (
            <li
              key={document.id}
              className="rounded-xl border border-stone-200 bg-white/80 p-3"
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="font-medium text-stone-900">{document.fileName}</p>
                  <p className="mt-1 text-xs text-stone-600">
                    {toHumanSize(document.fileSizeBytes)} | {document.contentType}
                  </p>
                  <p className="mt-1 text-xs text-stone-600">
                    Status: <span className="font-semibold">{document.status}</span>
                  </p>
                </div>

                <button
                  type="button"
                  onClick={() => void onDelete(document.id)}
                  className="rounded-lg border border-red-300 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-red-700 transition hover:bg-red-50"
                >
                  Delete
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {error && (
        <p className="mt-4 rounded-xl border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">
          {error}
        </p>
      )}
    </section>
  )
}
