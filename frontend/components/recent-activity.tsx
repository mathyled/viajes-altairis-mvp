"use client"

import { useEffect, useState } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { fetchReservations, type UiReservation } from "@/lib/api"

const statusStyles: Record<string, string> = {
  confirmed: "bg-chart-4/15 text-chart-4 border-chart-4/20",
  pending: "bg-chart-5/15 text-chart-5 border-chart-5/20",
  cancelled: "bg-destructive/15 text-destructive border-destructive/20",
}

export function RecentActivity() {
  const [recent, setRecent] = useState<UiReservation[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const data = await fetchReservations()
        // ordenar por fecha de creación descendente si disponible
        const sorted = [...data].sort((a, b) => (a.checkIn < b.checkIn ? 1 : -1))
        setRecent(sorted.slice(0, 5))
      } catch (err) {
        console.error(err)
        setError("No se pudo cargar la actividad reciente.")
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base font-semibold">Recent Reservations</CardTitle>
        <CardDescription>Latest booking activity across your portfolio</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="flex flex-col gap-3">
          {loading && (
            <span className="text-sm text-muted-foreground">Loading reservations...</span>
          )}
          {error && !loading && (
            <span className="text-sm text-destructive">{error}</span>
          )}
          {!loading && !error && recent.length === 0 && (
            <span className="text-sm text-muted-foreground">No recent reservations.</span>
          )}
          {!loading && !error && recent.map((reservation) => (
            <div
              key={reservation.backendId}
              className="flex items-center justify-between rounded-lg border bg-background p-3"
            >
              <div className="flex flex-col gap-0.5">
                <span className="text-sm font-medium text-foreground">
                  {reservation.guestName}
                </span>
                <span className="text-xs text-muted-foreground">
                  {reservation.hotelName} &middot; {reservation.roomType}
                </span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-sm font-semibold text-foreground">
                  ${reservation.total.toLocaleString()}
                </span>
                <Badge className={statusStyles[reservation.status]} variant="outline">
                  {reservation.status}
                </Badge>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
