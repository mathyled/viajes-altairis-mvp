"use client"

import { DashboardShell } from "@/components/dashboard-shell"
import { MetricsCards } from "@/components/metrics-cards"
import { RecentActivity } from "@/components/recent-activity"

export default function DashboardPage() {
  return (
    <DashboardShell>
      <div className="flex flex-col gap-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Dashboard</h1>
          <p className="text-sm text-muted-foreground">
            Overview of your hotel portfolio performance
          </p>
        </div>
        <MetricsCards />
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <RecentActivity />
        </div>
      </div>
    </DashboardShell>
  )
}
