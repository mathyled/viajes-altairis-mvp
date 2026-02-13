"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import {
  LayoutDashboard,
  Building2,
  CalendarRange,
  BookOpen,
  ChevronLeft,
  ChevronRight,
  Plane,
} from "lucide-react"
import { cn } from "@/lib/utils"
import { useState } from "react"
import { Button } from "@/components/ui/button"

const navItems = [
  { label: "Dashboard", href: "/", icon: LayoutDashboard },
  { label: "Hotels", href: "/hotels", icon: Building2 },
  { label: "Availability", href: "/availability", icon: CalendarRange },
  { label: "Reservations", href: "/reservations", icon: BookOpen },
]

export function AppSidebar() {
  const pathname = usePathname()
  const [collapsed, setCollapsed] = useState(false)

  return (
    <aside
      className={cn(
        "flex flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground transition-all duration-300",
        collapsed ? "w-16" : "w-64"
      )}
    >
      <div className={cn(
        "flex items-center border-b border-sidebar-border px-4 py-5",
        collapsed ? "justify-center" : "gap-3"
      )}>
        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
          <Plane className="h-5 w-5" />
        </div>
        {!collapsed && (
          <div className="flex flex-col">
            <span className="text-sm font-bold tracking-wide text-sidebar-primary-foreground">
              Viajes Altairis
            </span>
            <span className="text-xs text-sidebar-foreground/60">
             Backoffice 
            </span>
          </div>
        )}
      </div>

      <nav className="flex flex-1 flex-col gap-1 p-3">
        {navItems.map((item) => {
          const isActive =
            item.href === "/" ? pathname === "/" : pathname.startsWith(item.href)
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium transition-colors",
                collapsed && "justify-center px-0",
                isActive
                  ? "bg-sidebar-accent text-sidebar-accent-foreground"
                  : "text-sidebar-foreground hover:bg-sidebar-accent/50 hover:text-sidebar-accent-foreground"
              )}
            >
              <item.icon className="h-5 w-5 shrink-0" />
              {!collapsed && <span>{item.label}</span>}
            </Link>
          )
        })}
      </nav>

      <div className="border-t border-sidebar-border p-3">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => setCollapsed(!collapsed)}
          className={cn(
            "w-full text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
            collapsed && "px-0"
          )}
        >
          {collapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <>
              <ChevronLeft className="h-4 w-4" />
              <span className="ml-2">Collapse</span>
            </>
          )}
        </Button>
      </div>
    </aside>
  )
}
