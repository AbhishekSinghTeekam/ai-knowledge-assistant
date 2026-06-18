export default function ChatSkeleton() {
  return (
    <div className="space-y-4">
      <div className="h-4 w-32 animate-pulse rounded bg-stone-200" />

      <div className="ml-auto max-w-[78%] space-y-2 rounded-2xl bg-stone-200/80 p-4">
        <div className="h-3 animate-pulse rounded bg-stone-300" />
        <div className="h-3 w-3/4 animate-pulse rounded bg-stone-300" />
      </div>

      <div className="max-w-[80%] space-y-2 rounded-2xl bg-teal-100/80 p-4">
        <div className="h-3 animate-pulse rounded bg-teal-200" />
        <div className="h-3 w-5/6 animate-pulse rounded bg-teal-200" />
        <div className="h-3 w-2/3 animate-pulse rounded bg-teal-200" />
      </div>
    </div>
  )
}
