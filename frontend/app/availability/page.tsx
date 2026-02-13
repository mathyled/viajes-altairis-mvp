"use client"

import { DashboardShell } from "@/components/dashboard-shell"
import { AvailabilityMatrix } from "@/components/availability-matrix"

export default function AvailabilityPage() {
  return (
    <DashboardShell>
      <div className="flex flex-col gap-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Availability Calendar</h1>
          <p className="text-sm text-muted-foreground">
            Monitor room availability across all room types
          </p>
        </div>
        <AvailabilityMatrix />
      </div>
    </DashboardShell>
  )
}
