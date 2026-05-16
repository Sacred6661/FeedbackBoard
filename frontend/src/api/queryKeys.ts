export const queryKeys = {
  feedbacks: {
    all: ["feedbacks"] as const,
    byId: (id: string) => ["feedbacks", id] as const,
    byCategory: (categoryId: number) =>
      ["feedbacks", "category", categoryId] as const,
    byStatus: (status: number) => ["feedbacks", "status", status] as const,
    top: (count: number) => ["feedbacks", "top", count] as const,
  },
  metadata: {
    statuses: ["metadata", "statuses"] as const,
    categories: ["metadata", "categories"] as const,
  },
  health: ["health"] as const,
};
