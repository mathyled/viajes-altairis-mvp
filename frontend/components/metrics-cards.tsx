"use client"

import { useEffect, useState } from "react"
import { Building2, BookOpen, Percent, TrendingUp } from "lucide-react"
import { Card, CardContent } from "@/components/ui/card"
import { fetchHotels, fetchReservations, type UiHotel, type UiReservation } from "@/lib/api"

interface Metric {
  label: string
  value: string | number
  change: string
  icon: typeof Building2
  color: string
}

export function MetricsCards() {
  const [hotels, setHotels] = useState<UiHotel[]>([])
  const [reservations, setReservations] = useState<UiReservation[]>([])

  useEffect(() => {
    const load = async () => {
      try {
        const [h, r] = await Promise.all([fetchHotels(), fetchReservations()])
        setHotels(h)
        setReservations(r)
      } catch (err) {
        console.error("Error loading metrics data", err)
      }
    }
    load()
  }, [])

  const totalHotels = hotels.length
  const activeReservations = reservations.filter((r) => r.status === "confirmed").length
  const activeHotels = hotels.filter((h) => h.status === "active")
  const avgOccupancy =
    activeHotels.length > 0
      ? Math.round(activeHotels.reduce((sum, h) => sum + h.occupancy, 0) / activeHotels.length)
      : 0
  const totalRevenue = reservations
    .filter((r) => r.status === "confirmed")
    .reduce((sum, r) => sum + r.total, 0)

  const metrics: Metric[] = [
    {
      label: "Total Hotels",
      value: totalHotels,
      change: "",
      icon: Building2,
      color: "bg-primary/10 text-primary",
    },
    {
      label: "Active Reservations",
      value: activeReservations,
      change: "",
      icon: BookOpen,
      color: "bg-chart-4/15 text-chart-4",
    },
    {
      label: "Occupancy Rate",
      value: `${avgOccupancy}%`,
      change: "",
      icon: Percent,
      color: "bg-chart-2/15 text-chart-2",
    },
    {
      label: "Revenue (Total)",
      value: `$${(totalRevenue / 1000).toFixed(1)}k`,
      change: "",
      icon: TrendingUp,
      color: "bg-chart-5/15 text-chart-5",
    },
  ]

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {metrics.map((metric) => (
        <Card key={metric.label} className="overflow-hidden">
          <CardContent className="p-5">
            <div className="flex items-start justify-between">
              <div className="flex flex-col gap-1">
                <span className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                  {metric.label}
                </span>
                <span className="text-2xl font-bold text-foreground">
                  {metric.value}
                </span>
                {metric.change && (
                  <span className="text-xs text-muted-foreground">
                    {metric.change}
                  </span>
                )}
              </div>
              <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${metric.color}`}>
                <metric.icon className="h-5 w-5" />
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
