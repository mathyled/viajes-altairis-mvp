"use client"

import { useEffect, useMemo, useState } from "react"
import { format, parseISO, addDays } from "date-fns"
import { ChevronLeft, ChevronRight } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  bulkCreateInventory,
  fetchActiveHotels,
  fetchInventoryRange,
  fetchRoomTypesByHotel,
  type HotelDto,
  type RoomTypeDto,
} from "@/lib/api"
import { cn } from "@/lib/utils"

type RoomAvailabilityRow = {
  roomTypeId: number
  roomType: string
  total: number
  availability: {
    date: string
    available: number
  }[]
}

export function AvailabilityMatrix() {
  const [startOffset, setStartOffset] = useState(0)
  const visibleDays = 7

  const [hotels, setHotels] = useState<HotelDto[]>([])
  const [selectedHotelId, setSelectedHotelId] = useState<number | null>(null)
  const [roomTypes, setRoomTypes] = useState<RoomTypeDto[]>([])
   const [selectedRoomTypeId, setSelectedRoomTypeId] = useState<number | null>(null)
  const [roomAvailability, setRoomAvailability] = useState<RoomAvailabilityRow[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [refreshKey, setRefreshKey] = useState(0)

  const [fromDate, setFromDate] = useState("")
  const [toDate, setToDate] = useState("")
  const [totalStock, setTotalStock] = useState(0)
  const [saving, setSaving] = useState(false)
  const [saveMessage, setSaveMessage] = useState<string | null>(null)

  const today = useMemo(() => new Date(), [])

  const allDates = useMemo(() => {
    // Mostramos 14 días de inventario a partir de hoy
    return Array.from({ length: 14 }, (_, i) =>
      addDays(today, i).toISOString().slice(0, 10)
    )
  }, [today])

  const dates = useMemo(() => {
    return allDates.slice(startOffset, startOffset + visibleDays)
  }, [allDates, startOffset])

  const canGoBack = startOffset > 0
  const canGoForward = startOffset + visibleDays < allDates.length

  useEffect(() => {
    const loadHotels = async () => {
      try {
        setLoading(true)
        setError(null)
        const data = await fetchActiveHotels()
        setHotels(data)
        if (data.length > 0 && selectedHotelId == null) {
          setSelectedHotelId(data[0].id)
        }
      } catch (err) {
        console.error(err)
        setError("No se pudieron cargar los hoteles activos.")
      } finally {
        setLoading(false)
      }
    }
    loadHotels()
  }, [selectedHotelId])

  useEffect(() => {
    const loadAvailability = async () => {
      if (!selectedHotelId) return
      try {
        setLoading(true)
        setError(null)

        const fetchedRoomTypes = await fetchRoomTypesByHotel(selectedHotelId)
        setRoomTypes(fetchedRoomTypes)
        if (!selectedRoomTypeId && fetchedRoomTypes.length > 0) {
          setSelectedRoomTypeId(fetchedRoomTypes[0].id)
        }

        const from = allDates[0]
        const to = allDates[allDates.length - 1]

        const rows: RoomAvailabilityRow[] = []

        for (const rt of fetchedRoomTypes) {
          const inventory = await fetchInventoryRange(selectedHotelId, rt.id, from, to)

          // Mapear todas las fechas del rango, aunque no haya inventario (disponible 0)
          const availability = allDates.map((date) => {
            const inv = inventory.find((i) => i.fecha.slice(0, 10) === date)
            return {
              date,
              available: inv ? inv.cantidadDisponible : 0,
            }
          })

          const total = inventory.length > 0 ? inventory[0].cantidadTotal : 0

          rows.push({
            roomTypeId: rt.id,
            roomType: rt.nombre,
            total,
            availability,
          })
        }

        setRoomAvailability(rows)
      } catch (err) {
        console.error(err)
        setError("No se pudo cargar el inventario de habitaciones.")
      } finally {
        setLoading(false)
      }
    }

    loadAvailability()
  }, [selectedHotelId, allDates, selectedRoomTypeId, refreshKey])

  function getCellColor(available: number, total: number) {
    const ratio = available / total
    if (ratio === 0) return "bg-destructive/15 text-destructive"
    if (ratio <= 0.25) return "bg-destructive/10 text-destructive"
    if (ratio <= 0.5) return "bg-chart-5/15 text-chart-5"
    return "bg-chart-4/10 text-chart-4"
  }

  function getCellBorder(available: number, total: number) {
    const ratio = available / total
    if (ratio === 0) return "border-destructive/30"
    if (ratio <= 0.25) return "border-destructive/20"
    if (ratio <= 0.5) return "border-chart-5/30"
    return "border-chart-4/30"
  }

  const handleBulkCreate = async (e: any) => {
    e.preventDefault()
    setSaveMessage(null)
    setError(null)

    if (!selectedHotelId || !selectedRoomTypeId) {
      setError("Select a hotel and a room type.")
      return
    }
    if (!fromDate || !toDate) {
      setError("Select a date range.")
      return
    }
    if (fromDate >= toDate) {
      setError("The start date must be before the end date.")
      return
    }
    if (totalStock <= 0) {
      setError("The total stock must be greater than 0.")
      return
    }

    try {
      setSaving(true)
      await bulkCreateInventory({
        hotelId: selectedHotelId,
        roomTypeId: selectedRoomTypeId,
        fechaInicio: fromDate,
        fechaFin: toDate,
        cantidadTotal: totalStock,
      })
      setSaveMessage("Availability created/updated successfully for the specified range.")
      setRefreshKey((k) => k + 1)
    } catch (err: any) {
      console.error(err)
      setError(err.message ?? "Could not update availability.")
    } finally {
      setSaving(false)
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle className="text-base font-semibold">Room Availability</CardTitle>
            <CardDescription>
              14-day availability overview by room type (live data)
            </CardDescription>
          </div>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
            <span className="text-xs font-medium text-muted-foreground">Hotel</span>
            <select
              className="h-9 rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              value={selectedHotelId ?? ""}
              onChange={(e) => {
                const value = e.target.value
                setSelectedHotelId(value ? Number(value) : null)
              }}
            >
              {hotels.length === 0 && <option value="">No hotels</option>}
              {hotels.map((h) => (
                <option key={h.id} value={h.id}>
                  {h.nombre}
                </option>
              ))}
            </select>
          </div>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setStartOffset((o) => Math.max(0, o - visibleDays))}
              disabled={!canGoBack}
            >
              <ChevronLeft className="h-4 w-4" />
              <span className="sr-only">Previous week</span>
            </Button>
            <span className="min-w-[140px] text-center text-sm font-medium text-foreground">
              {dates[0] && format(parseISO(dates[0]), "MMM d")} &ndash;{" "}
              {dates[dates.length - 1] &&
                format(parseISO(dates[dates.length - 1]), "MMM d, yyyy")}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() =>
                setStartOffset((o) =>
                  Math.min(
                    roomAvailability[0].availability.length - visibleDays,
                    o + visibleDays
                  )
                )
              }
              disabled={!canGoForward}
            >
              <ChevronRight className="h-4 w-4" />
              <span className="sr-only">Next week</span>
            </Button>
          </div>
        </div>
        <form
          className="mt-4 grid gap-3 sm:grid-cols-5"
          onSubmit={handleBulkCreate}
        >
          <div className="space-y-1.5 sm:col-span-1">
            <Label htmlFor="roomType">Room Type</Label>
            <select
              id="roomType"
              className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              value={selectedRoomTypeId ?? ""}
              onChange={(e) => {
                const value = e.target.value
                setSelectedRoomTypeId(value ? Number(value) : null)
              }}
            >
              {roomTypes.length === 0 && <option value="">No room types</option>}
              {roomTypes.map((rt) => (
                <option key={rt.id} value={rt.id}>
                  {rt.nombre}
                </option>
              ))}
            </select>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="fromDate">From</Label>
            <Input
              id="fromDate"
              type="date"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              required
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="toDate">To</Label>
            <Input
              id="toDate"
              type="date"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              required
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="totalStock">Total rooms</Label>
            <Input
              id="totalStock"
              type="number"
              min={1}
              value={totalStock}
              onChange={(e) => setTotalStock(Number(e.target.value) || 0)}
              required
            />
          </div>
          <div className="flex items-end">
            <Button type="submit" disabled={saving}>
              {saving ? "Saving..." : "Save availability"}
            </Button>
          </div>
        </form>

        <div className="flex flex-wrap items-center gap-4 pt-4">
          <div className="flex items-center gap-1.5">
            <div className="h-3 w-3 rounded-sm border border-chart-4/30 bg-chart-4/15" />
            <span className="text-xs text-muted-foreground">Available</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="h-3 w-3 rounded-sm border border-chart-5/30 bg-chart-5/15" />
            <span className="text-xs text-muted-foreground">Low</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="h-3 w-3 rounded-sm border border-destructive/30 bg-destructive/15" />
            <span className="text-xs text-muted-foreground">Full / Critical</span>
          </div>
        </div>
      </CardHeader>
      <CardContent className="overflow-x-auto p-0 pb-4">
        {error && (
          <div className="px-4 pb-4 text-sm text-destructive">
            {error}
          </div>
        )}
        {saveMessage && !error && (
          <div className="px-4 pb-4 text-sm text-emerald-600">
            {saveMessage}
          </div>
        )}
        {loading && roomAvailability.length === 0 && (
          <div className="px-4 pb-4 text-sm text-muted-foreground">
            Loading availability...
          </div>
        )}
        <table className="w-full min-w-[640px] border-collapse">
          <thead>
            <tr className="border-y bg-muted/50">
              <th className="sticky left-0 z-10 bg-muted/50 px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                Room Type
              </th>
              {dates.map((date) => {
                const d = parseISO(date)
                const isWeekend = d.getDay() === 0 || d.getDay() === 6
                return (
                  <th
                    key={date}
                    className={cn(
                      "px-2 py-3 text-center text-xs font-medium",
                      isWeekend ? "text-primary" : "text-muted-foreground"
                    )}
                  >
                    <div className="flex flex-col items-center gap-0.5">
                      <span className="uppercase">{format(d, "EEE")}</span>
                      <span className="text-sm font-semibold text-foreground">{format(d, "d")}</span>
                    </div>
                  </th>
                )
              })}
            </tr>
          </thead>
          <tbody>
            {roomAvailability.map((room) => (
              <tr key={room.roomType} className="border-b last:border-b-0">
                <td className="sticky left-0 z-10 bg-card px-4 py-3">
                  <div className="flex flex-col">
                    <span className="text-sm font-medium text-foreground">
                      {room.roomType}
                    </span>
                    <span className="text-xs text-muted-foreground">
                      {room.total} total
                    </span>
                  </div>
                </td>
                {dates.map((date) => {
                  const dayIndex = allDates.indexOf(date)
                  const dayData = room.availability[dayIndex]
                  const available = dayData?.available ?? 0
                  const colorClass = getCellColor(available, room.total)
                  const borderClass = getCellBorder(available, room.total)
                  return (
                    <td key={date} className="px-1.5 py-2">
                      <div
                        className={cn(
                          "flex flex-col items-center justify-center rounded-md border px-2 py-2 transition-transform hover:scale-105",
                          colorClass,
                          borderClass
                        )}
                      >
                        <span className="text-sm font-bold">
                          {available}
                        </span>
                        <span className="text-[10px] opacity-70">
                          {"/ "}{room.total}
                        </span>
                      </div>
                    </td>
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </CardContent>
    </Card>
  )
}
