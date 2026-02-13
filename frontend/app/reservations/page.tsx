"use client"

import { useEffect, useState } from "react"
import { DashboardShell } from "@/components/dashboard-shell"
import { ReservationsTable } from "@/components/reservations-table"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import {
  createReservation,
  checkAvailability,
  fetchActiveHotels,
  fetchRoomTypesByHotel,
  type AvailabilityResultDto,
  type HotelDto,
  type RoomTypeDto,
} from "@/lib/api"

export default function ReservationsPage() {
  const [hotelId, setHotelId] = useState("")
  const [roomTypeId, setRoomTypeId] = useState("")
  const [guestName, setGuestName] = useState("")
  const [checkIn, setCheckIn] = useState("")
  const [checkOut, setCheckOut] = useState("")
  const [rooms, setRooms] = useState(1)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [hotels, setHotels] = useState<HotelDto[]>([])
  const [roomTypes, setRoomTypes] = useState<RoomTypeDto[]>([])
  const [checkingAvailability, setCheckingAvailability] = useState(false)
  const [availabilityResults, setAvailabilityResults] = useState<
    { roomType: RoomTypeDto; result: AvailabilityResultDto }[]
  >([])

  useEffect(() => {
    const loadHotels = async () => {
      try {
        const data = await fetchActiveHotels()
        setHotels(data)
      } catch (err) {
        console.error(err)
        setError("No se pudieron cargar los hoteles.")
      }
    }
    loadHotels()
  }, [])

  useEffect(() => {
    const hid = Number(hotelId)
    if (!hid) {
      setRoomTypes([])
      return
    }
    const loadRoomTypes = async () => {
      try {
        const data = await fetchRoomTypesByHotel(hid)
        setRoomTypes(data)
      } catch (err) {
        console.error(err)
        setError("No se pudieron cargar los tipos de habitación.")
      }
    }
    loadRoomTypes()
  }, [hotelId])

  const handleCheckAvailability = async () => {
    setAvailabilityResults([])
    setError(null)
    setSuccess(null)

    const hid = Number(hotelId)
    const roomsCount = rooms

    if (!hid || !checkIn || !checkOut || roomsCount <= 0) {
      setError("Selecciona hotel, fechas y cantidad de habitaciones para verificar disponibilidad.")
      return
    }

    if (roomTypes.length === 0) {
      setError("El hotel seleccionado no tiene tipos de habitación configurados.")
      return
    }

    try {
      setCheckingAvailability(true)
      const results: { roomType: RoomTypeDto; result: AvailabilityResultDto }[] = []

      for (const rt of roomTypes) {
        const result = await checkAvailability({
          hotelId: hid,
          roomTypeId: rt.id,
          fechaInicio: checkIn,
          fechaFin: checkOut,
          cantidadHabitaciones: roomsCount,
        })
        results.push({ roomType: rt, result })
      }

      setAvailabilityResults(results)
    } catch (err) {
      console.error(err)
      setError("No se pudo verificar la disponibilidad.")
    } finally {
      setCheckingAvailability(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await createReservation({
        hotelId: Number(hotelId),
        roomTypeId: Number(roomTypeId),
        huespedNombre: guestName,
        fechaEntrada: checkIn,
        fechaSalida: checkOut,
        cantidadHabitaciones: rooms,
      })
      setSuccess("Reserva creada correctamente.")
      // opcional: limpiar formulario
      // setHotelId(""); setRoomTypeId(""); setGuestName(""); setCheckIn(""); setCheckOut(""); setRooms(1)
    } catch (err: any) {
      console.error(err)
      setError(err.message ?? "No se pudo crear la reserva.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <DashboardShell>
      <div className="flex flex-col gap-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Reservations</h1>
          <p className="text-sm text-muted-foreground">
            Track and manage all hotel bookings
          </p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle className="text-base font-semibold">Create Reservation</CardTitle>
            <CardDescription>Create a new reservation</CardDescription>
          </CardHeader>
          <CardContent>
            <form className="grid grid-cols-1 gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
              <div>
                <label className="mb-1 block text-xs font-medium text-muted-foreground">Hotel</label>
                <select
                  className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  value={hotelId}
                  onChange={(e) => {
                    setHotelId(e.target.value)
                    setRoomTypeId("")
                    setAvailabilityResults([])
                  }}
                  required
                >
                  <option value="">Selecciona un hotel</option>
                  {hotels.map((h) => (
                    <option key={h.id} value={h.id}>
                      {h.nombre}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-muted-foreground">Room Type</label>
                <select
                  className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  value={roomTypeId}
                  onChange={(e) => setRoomTypeId(e.target.value)}
                  required
                >
                  <option value="">Selecciona un tipo de habitación</option>
                  {roomTypes.map((rt) => (
                    <option key={rt.id} value={rt.id}>
                      {rt.nombre}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-muted-foreground">Guest Name</label>
                <Input
                  value={guestName}
                  onChange={(e) => setGuestName(e.target.value)}
                  required
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-muted-foreground">Rooms</label>
                <Input
                  type="number"
                  min={1}
                  value={rooms}
                  onChange={(e) => setRooms(Number(e.target.value) || 1)}
                  required
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-muted-foreground">Check-in</label>
                <Input
                  type="date"
                  value={checkIn}
                  onChange={(e) => setCheckIn(e.target.value)}
                  required
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-muted-foreground">Check-out</label>
                <Input
                  type="date"
                  value={checkOut}
                  onChange={(e) => setCheckOut(e.target.value)}
                  required
                />
              </div>

              <div className="md:col-span-2 flex flex-col gap-3">
                <div className="flex flex-wrap items-center gap-3">
                  <Button
                    type="button"
                    variant="outline"
                    disabled={checkingAvailability || !hotelId || !checkIn || !checkOut}
                    onClick={handleCheckAvailability}
                  >
                    {checkingAvailability ? "Checking..." : "Check availability"}
                  </Button>
                  <Button type="submit" disabled={loading || !roomTypeId}>
                    {loading ? "Creating..." : "Create Reservation"}
                  </Button>
                  {error && <span className="text-sm text-destructive">{error}</span>}
                  {success && <span className="text-sm text-emerald-600">{success}</span>}
                </div>

                {availabilityResults.length > 0 && (
                  <div className="mt-2 space-y-2 rounded-md border bg-muted/40 p-3 text-xs">
                    <p className="font-medium text-muted-foreground">
                      Availability for selected dates ({checkIn} → {checkOut}) and {rooms} room(s):
                    </p>
                    <ul className="space-y-1">
                      {availabilityResults.map(({ roomType, result }) => (
                        <li
                          key={roomType.id}
                          className="flex items-center justify-between gap-2"
                        >
                          <span>
                            <span className="font-medium text-foreground">{roomType.nombre}</span>
                            <span className="ml-2 text-muted-foreground">
                              {result.available ? "Available" : "Not available"}
                            </span>
                          </span>
                          {result.available && (
                            <Button
                              type="button"
                              variant={roomTypeId === String(roomType.id) ? "default" : "outline"}
                              size="sm"
                              onClick={() => setRoomTypeId(String(roomType.id))}
                            >
                              {roomTypeId === String(roomType.id) ? "Selected" : "Select"}
                            </Button>
                          )}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            </form>
          </CardContent>
        </Card>

        <ReservationsTable />
      </div>
    </DashboardShell>
  )
}
