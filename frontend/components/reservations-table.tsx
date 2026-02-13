"use client"

import { useEffect, useMemo, useState } from "react"
import { Search, CalendarDays } from "lucide-react"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { cancelReservation, fetchReservations, type UiReservation } from "@/lib/api"
import { format, parseISO } from "date-fns"

const statusStyles: Record<string, string> = {
  confirmed: "bg-chart-4/15 text-chart-4 border-chart-4/20",
  pending: "bg-chart-5/15 text-chart-5 border-chart-5/20",
  cancelled: "bg-destructive/15 text-destructive border-destructive/20",
}

export function ReservationsTable() {
  const [reservations, setReservations] = useState<UiReservation[]>([])
  const [search, setSearch] = useState("")
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [actionLoadingId, setActionLoadingId] = useState<number | null>(null)

  const loadReservations = async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await fetchReservations()
      setReservations(data)
    } catch (err) {
      console.error(err)
      setError("No se pudieron cargar las reservas. Intenta nuevamente.")
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadReservations()
  }, [])

  const filtered = useMemo(() => {
    return reservations.filter(
      (r) =>
        r.guestName.toLowerCase().includes(search.toLowerCase()) ||
        r.hotelName.toLowerCase().includes(search.toLowerCase()) ||
        r.id.toLowerCase().includes(search.toLowerCase())
    )
  }, [search, reservations])

  const handleCancel = async (reservation: UiReservation) => {
    if (!confirm(`¿Cancelar la reserva ${reservation.id}?`)) return
    try {
      setActionLoadingId(reservation.backendId)
      await cancelReservation(reservation.backendId)
      await loadReservations()
    } catch (err) {
      console.error(err)
      setError("No se pudo cancelar la reserva.")
    } finally {
      setActionLoadingId(null)
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle className="text-base font-semibold">All Reservations</CardTitle>
            <CardDescription>
              {loading
                ? "Loading reservations..."
                : error
                ? error
                : `${filtered.length} reservations found`}
            </CardDescription>
          </div>
          <div className="relative w-full sm:w-72">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search reservations..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9"
            />
          </div>
        </div>
      </CardHeader>
      <CardContent className="p-0">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50 hover:bg-muted/50">
              <TableHead className="pl-6">ID</TableHead>
              <TableHead>Guest</TableHead>
              <TableHead>Hotel</TableHead>
              <TableHead>Room Type</TableHead>
              <TableHead>Check-in / Check-out</TableHead>
              <TableHead className="text-right">Total</TableHead>
              <TableHead className="text-center">Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={7} className="py-12 text-center text-muted-foreground">
                  Loading reservations...
                </TableCell>
              </TableRow>
            ) : filtered.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="py-12 text-center text-muted-foreground">
                  {error ? error : "No reservations found."}
                </TableCell>
              </TableRow>
            ) : (
              filtered.map((r) => (
                <TableRow key={r.backendId}>
                  <TableCell className="pl-6 font-mono text-xs text-muted-foreground">
                    {r.id}
                  </TableCell>
                  <TableCell className="font-medium text-foreground">{r.guestName}</TableCell>
                  <TableCell className="text-muted-foreground">{r.hotelName}</TableCell>
                  <TableCell className="text-muted-foreground">{r.roomType}</TableCell>
                  <TableCell>
                    <span className="flex items-center gap-1.5 text-muted-foreground">
                      <CalendarDays className="h-3.5 w-3.5" />
                      {format(parseISO(r.checkIn), "MMM d")} &ndash; {format(parseISO(r.checkOut), "MMM d")}
                    </span>
                  </TableCell>
                  <TableCell className="text-right font-medium text-foreground">
                    ${r.total.toLocaleString()}
                  </TableCell>
                  <TableCell className="text-center">
                    <div className="flex items-center justify-center gap-2">
                      <Badge variant="outline" className={statusStyles[r.status]}>
                        {r.status}
                      </Badge>
                      <button
                        className="text-xs text-destructive underline-offset-2 hover:underline disabled:opacity-50"
                        onClick={() => handleCancel(r)}
                        disabled={actionLoadingId === r.backendId}
                      >
                        {actionLoadingId === r.backendId ? "Cancelling..." : "Cancel"}
                      </button>
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  )
}
