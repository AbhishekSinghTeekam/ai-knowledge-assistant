import { useState } from 'react'

interface UploadPanelProps {
  isUploading: boolean
  onUpload: (file: File) => Promise<void>
}

export default function UploadPanel({ isUploading, onUpload }: UploadPanelProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [error, setError] = useState<string | null>(null)

  const handleUpload = async () => {
    setError(null)

    if (!selectedFile) {
      setError('Please choose a file first.')
      return
    }

    const allowedTypes = [
      'application/pdf',
      'text/plain',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    ]

    if (!allowedTypes.includes(selectedFile.type)) {
      setError('Only PDF, TXT, and DOCX files are supported.')
      return
    }

    await onUpload(selectedFile)
    setSelectedFile(null)
  }

  return (
    <section className="rounded-3xl border border-stone-900/10 bg-white/75 p-5 shadow-[0_18px_60px_rgba(0,0,0,0.08)] backdrop-blur sm:p-6">
      <h3 className="text-2xl text-stone-900">Upload</h3>
      <p className="mt-2 text-sm text-stone-700">
        Ingest PDF, TXT, or DOCX files. The backend will extract text, chunk it, and index vectors.
      </p>

      <div className="mt-5 space-y-3">
        <input
          type="file"
          accept=".pdf,.txt,.docx"
          onChange={(event) => {
            const file = event.target.files?.[0] ?? null
            setSelectedFile(file)
          }}
          className="block w-full rounded-xl border border-stone-300 bg-white px-3 py-2 text-sm text-stone-800 file:mr-4 file:rounded-lg file:border-0 file:bg-stone-900 file:px-3 file:py-2 file:text-xs file:font-semibold file:text-amber-50"
        />

        {selectedFile && (
          <p className="text-xs text-stone-600">
            Selected: <span className="font-semibold">{selectedFile.name}</span>
          </p>
        )}

        {error && (
          <p className="rounded-xl border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">
            {error}
          </p>
        )}

        <button
          type="button"
          onClick={() => void handleUpload()}
          disabled={isUploading}
          className="rounded-xl bg-stone-900 px-4 py-2.5 text-sm font-semibold text-amber-50 transition hover:bg-stone-800 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isUploading ? 'Uploading...' : 'Upload document'}
        </button>
      </div>
    </section>
  )
}
