"use client"

import { useEffect, useState } from "react"
import { Search, ChevronLeft, ChevronRight, Star, MapPin } from "lucide-react"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
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
import { fetchPagedHotels, type UiHotel } from "@/lib/api"

const statusStyles: Record<string, string> = {
  active: "bg-chart-4/15 text-chart-4 border-chart-4/20",
  inactive: "bg-muted text-muted-foreground border-border",
  maintenance: "bg-chart-5/15 text-chart-5 border-chart-5/20",
}

const ITEMS_PER_PAGE = 10

export function HotelsTable() {
  const [hotels, setHotels] = useState<UiHotel[]>([])
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize] = useState(ITEMS_PER_PAGE)
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const result = await fetchPagedHotels(page, pageSize, search)
        if (active) {
          setHotels(result.items)
          setTotalCount(result.totalCount)
        }
      } catch (err) {
        console.error(err)
        if (active) {
          setError("No se pudieron cargar los hoteles. Intenta nuevamente.")
        }
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }
    load()

    return () => {
      active = false
    }
  }, [page, pageSize, search])

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  return (
    <Card>
      <CardHeader>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <CardTitle className="text-base font-semibold">Hotel Directory</CardTitle>
            <CardDescription>
              {loading
                ? "Loading hotels..."
                : error
                ? error
                : `${totalCount} hotels in your portfolio`}
            </CardDescription>
          </div>
          <div className="relative w-full sm:w-72">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search hotels..."
              value={search}
              onChange={(e) => {
                setSearch(e.target.value)
                setPage(1)
              }}
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
              <TableHead>Hotel Name</TableHead>
              <TableHead>Location</TableHead>
              <TableHead className="text-center">Rooms</TableHead>
              <TableHead className="text-center">Occupancy</TableHead>
              <TableHead className="text-center">Rating</TableHead>
              <TableHead className="text-center">Status</TableHead>
            </TableRow>
          </TableHeader>
              <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={7} className="py-12 text-center text-muted-foreground">
                  Loading hotels...
                </TableCell>
              </TableRow>
            ) : hotels.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="py-12 text-center text-muted-foreground">
                  {error ? error : "No hotels found matching your search."}
                </TableCell>
              </TableRow>
            ) : (
              hotels.map((hotel) => (
                <TableRow key={hotel.id}>
                  <TableCell className="pl-6 font-mono text-xs text-muted-foreground">
                    {hotel.id}
                  </TableCell>
                  <TableCell className="font-medium text-foreground">
                    {hotel.name}
                  </TableCell>
                  <TableCell>
                    <span className="flex items-center gap-1 text-muted-foreground">
                      <MapPin className="h-3.5 w-3.5" />
                      {hotel.location}
                    </span>
                  </TableCell>
                  <TableCell className="text-center">{hotel.rooms}</TableCell>
                  <TableCell className="text-center">
                    {hotel.status === "active" ? (
                      <span className="font-medium">{hotel.occupancy}%</span>
                    ) : (
                      <span className="text-muted-foreground">--</span>
                    )}
                  </TableCell>
                  <TableCell className="text-center">
                    <span className="inline-flex items-center gap-1">
                      <Star className="h-3.5 w-3.5 fill-chart-5 text-chart-5" />
                      {hotel.rating}
                    </span>
                  </TableCell>
                  <TableCell className="text-center">
                    <Badge variant="outline" className={statusStyles[hotel.status]}>
                      {hotel.status}
                    </Badge>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>

        {totalPages > 1 && (
          <div className="flex items-center justify-between border-t px-6 py-3">
            <span className="text-xs text-muted-foreground">
              Page {page} of {totalPages}
            </span>
            <div className="flex items-center gap-1">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                <ChevronLeft className="h-4 w-4" />
                <span className="sr-only">Previous</span>
              </Button>
              {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                <Button
                  key={p}
                  variant={p === page ? "default" : "outline"}
                  size="sm"
                  className="h-8 w-8 p-0"
                  onClick={() => setPage(p)}
                >
                  {p}
                </Button>
              ))}
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
              >
                <ChevronRight className="h-4 w-4" />
                <span className="sr-only">Next</span>
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
