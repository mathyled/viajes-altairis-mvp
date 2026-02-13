"use client"

import { useState } from "react"
import { DashboardShell } from "@/components/dashboard-shell"
import { HotelsTable } from "@/components/hotels-table"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { createHotel, createRoomType } from "@/lib/api"

export default function HotelsPage() {
  const [nombre, setNombre] = useState("")
  const [direccion, setDireccion] = useState("")
  const [categoria, setCategoria] = useState(4)
  const [estado, setEstado] = useState(true)
  const [roomTypes, setRoomTypes] = useState(
    Array.from({ length: 4 }, () => ({ name: "", price: 0 }))
  )
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const handleSubmit = async (e: any) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    setSuccess(null)

    try {
      const validRoomTypes = roomTypes.filter(
        (rt) => rt.name.trim().length > 0 && rt.price > 0
      )

      if (validRoomTypes.length === 0) {
        throw new Error(
          "Debes especificar al menos un Room Type válido (nombre y precio base > 0)."
        )
      }

      const hotel = await createHotel({
        nombre,
        direccion,
        categoria: Number(categoria),
        estado,
      })

      for (const rt of validRoomTypes) {
        await createRoomType({
          nombre: rt.name,
          precioBase: Number(rt.price),
          hotelId: hotel.id,
        })
      }

      setSuccess("Hotel y Room Types creados correctamente.")
      setNombre("")
      setDireccion("")
      setCategoria(4)
      setEstado(true)
      setRoomTypes(Array.from({ length: 4 }, () => ({ name: "", price: 0 })))

      if (typeof window !== "undefined") {
        window.location.reload()
      }
    } catch (err: any) {
      console.error(err)
      setError(err.message ?? "No se pudo crear el hotel.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <DashboardShell>
      <div className="flex flex-col gap-6">
        <div className="flex items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-foreground">Hotel Management</h1>
            <p className="text-sm text-muted-foreground">
              Manage and monitor all hotels in your portfolio
            </p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle className="text-base font-semibold">New Hotel</CardTitle>
            <CardDescription>Create a new hotel and add it to your portfolio.</CardDescription>
          </CardHeader>
          <CardContent>
            <form className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
              <div className="space-y-1.5">
                <Label htmlFor="nombre">Name</Label>
                <Input
                  id="nombre"
                  value={nombre}
                  onChange={(e: any) => setNombre(e.target.value)}
                  placeholder="Altairis Downtown Hotel"
                  required
                />
              </div>
              <div className="space-y-1.5 md:col-span-1">
                <Label htmlFor="direccion">Address</Label>
                <Input
                  id="direccion"
                  value={direccion}
                  onChange={(e: any) => setDireccion(e.target.value)}
                  placeholder="123 Main Street, Madrid"
                  required
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="categoria">Category (stars)</Label>
                <Input
                  id="categoria"
                  type="number"
                  min={1}
                  max={5}
                  value={categoria}
                  onChange={(e: any) => setCategoria(Number(e.target.value) || 1)}
                  required
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="estado">Status</Label>
                <select
                  id="estado"
                  className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  value={estado ? "active" : "inactive"}
                  onChange={(e: any) => setEstado(e.target.value === "active")}
                >
                  <option value="active">Active</option>
                  <option value="inactive">Inactive</option>
                </select>
              </div>

              {roomTypes.map((rt, index) => (
                <div key={index} className="space-y-1.5 md:col-span-1">
                  <Label htmlFor={`roomTypeName-${index}`}>
                    Room Type {index + 1}
                  </Label>
                  <Input
                    id={`roomTypeName-${index}`}
                    value={rt.name}
                    onChange={(e: any) => {
                      const value = e.target.value
                      setRoomTypes((prev) =>
                        prev.map((item, i) =>
                          i === index ? { ...item, name: value } : item
                        )
                      )
                    }}
                    placeholder="Standard Room"
                  />
                  <Label htmlFor={`roomTypePrice-${index}`} className="mt-2 block text-xs">
                    Base Price
                  </Label>
                  <Input
                    id={`roomTypePrice-${index}`}
                    type="number"
                    min={0}
                    step="0.01"
                    value={rt.price}
                    onChange={(e: any) => {
                      const value = Number(e.target.value) || 0
                      setRoomTypes((prev) =>
                        prev.map((item, i) =>
                          i === index ? { ...item, price: value } : item
                        )
                      )
                    }}
                  />
                </div>
              ))}

              <div className="md:col-span-2 flex items-center gap-3">
                <Button type="submit" disabled={loading}>
                  {loading ? "Creating..." : "Create Hotel"}
                </Button>
                {error && <span className="text-sm text-destructive">{error}</span>}
                {success && <span className="text-sm text-emerald-600">{success}</span>}
              </div>
            </form>
          </CardContent>
        </Card>

        <HotelsTable />
      </div>
    </DashboardShell>
  )
}
