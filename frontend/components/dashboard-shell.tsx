"use client"

import { AppSidebar } from "@/components/app-sidebar"
import { Bell, User } from "lucide-react"
import { Button } from "@/components/ui/button"

export function DashboardShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-screen overflow-hidden">
      <AppSidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <header className="flex h-14 items-center justify-between border-b bg-card px-6">
          <h2 className="text-sm font-medium text-muted-foreground">
            B2B Hotel Management System
          </h2>
        </header>
        <main className="flex-1 overflow-auto p-6">
          {children}
        </main>
      </div>
    </div>
  )
}
