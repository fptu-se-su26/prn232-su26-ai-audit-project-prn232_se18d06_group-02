import EmptyState from '@/components/ui/EmptyState'

interface ChatEmptyStateProps {
  title: string
  description?: string
}

export default function ChatEmptyState({ title, description }: ChatEmptyStateProps) {
  return <EmptyState icon="chat" title={title} description={description} />
}
