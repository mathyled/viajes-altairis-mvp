export const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000"

// Backend DTOs
export interface HotelDto {
  id: number
  nombre: string
  direccion: string
  categoria: number
  estado: boolean
}

export interface CreateHotelDto {
  nombre: string
  direccion: string
  categoria: number
  estado: boolean
}

export interface ReservationDto {
  id: number
  hotelId: number
  roomTypeId: number
  huespedNombre: string
  fechaEntrada: string
  fechaSalida: string
  estado: string
  montoTotal: number
  fechaCreacion: string
  hotelNombre?: string
  roomTypeName?: string
  noches: number
}

export interface PagedResultDto<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

export interface CreateReservationDto {
  hotelId: number
  roomTypeId: number
  huespedNombre: string
  fechaEntrada: string
  fechaSalida: string
  cantidadHabitaciones: number
}

export interface UpdateReservationStatusDto {
  estado: string
}

export interface ReservationResultDto {
  success: boolean
  message: string
  reservation?: ReservationDto
}

// UI models
export interface UiHotel {
  id: string
  name: string
  location: string
  rooms: number
  status: "active" | "inactive" | "maintenance"
  occupancy: number
  rating: number
  lastUpdated: string
}

export type ReservationStatus = "confirmed" | "pending" | "cancelled" | string
export type HotelStatus = "active" | "inactive" | "maintenance" | string

export interface UiReservation {
  id: string
  backendId: number
  guestName: string
  hotelName: string
  checkIn: string
  checkOut: string
  status: ReservationStatus
  roomType: string
  total: number
}

export interface InventoryDto {
  id: number
  hotelId: number
  roomTypeId: number
  fecha: string
  cantidadTotal: number
  cantidadReservada: number
  cantidadDisponible: number
  hotelNombre?: string
  roomTypeName?: string
}

export interface RoomTypeDto {
  id: number
  nombre: string
  precioBase: number
  hotelId: number
  hotelNombre?: string
}

export interface CreateRoomTypeDto {
  nombre: string
  precioBase: number
  hotelId: number
}

export interface BulkCreateInventoryDto {
  hotelId: number
  roomTypeId: number
  fechaInicio: string
  fechaFin: string
  cantidadTotal: number
}

export interface CheckAvailabilityDto {
  hotelId: number
  roomTypeId: number
  fechaInicio: string
  fechaFin: string
  cantidadHabitaciones: number
}

export interface AvailabilityResultDto {
  available: boolean
  message: string
  inventoryDetails?: InventoryDto[]
}

function mapEstadoToStatus(estado: string): ReservationStatus {
  const e = estado.toLowerCase()
  if (e.includes("confirm")) return "confirmed"
  if (e.includes("pend")) return "pending"
  if (e.includes("cancel")) return "cancelled"
  return estado
}

function mapReservation(dto: ReservationDto): UiReservation {
  return {
    id: `R${dto.id.toString().padStart(3, "0")}`,
    backendId: dto.id,
    guestName: dto.huespedNombre,
    hotelName: dto.hotelNombre ?? `Hotel ${dto.hotelId}`,
    checkIn: dto.fechaEntrada,
    checkOut: dto.fechaSalida,
    status: mapEstadoToStatus(dto.estado),
    roomType: dto.roomTypeName ?? `RoomType ${dto.roomTypeId}`,
    total: dto.montoTotal,
  }
}

export async function fetchHotels(): Promise<UiHotel[]> {
  const res = await fetch(`${API_BASE_URL}/api/hotels`, {
    next: { revalidate: 0 },
  })

  if (!res.ok) {
    throw new Error(`Error al obtener hoteles: ${res.status}`)
  }

  const data: HotelDto[] = await res.json()

  // Mapear DTO del backend al modelo de UI actual
  return data.map((h) => ({
    id: `H${h.id.toString().padStart(3, "0")}`,
    name: h.nombre,
    location: h.direccion,
    rooms: 0,
    status: h.estado ? "active" : "inactive",
    occupancy: 0,
    rating: h.categoria,
    lastUpdated: new Date().toISOString().slice(0, 10),
  }))
}

export async function fetchPagedHotels(
  pageNumber: number,
  pageSize: number,
  searchTerm?: string
): Promise<{ items: UiHotel[]; totalCount: number; pageNumber: number; pageSize: number }> {
  const params = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(pageSize),
  })
  if (searchTerm && searchTerm.trim().length > 0) {
    params.set("searchTerm", searchTerm.trim())
  }

  const res = await fetch(`${API_BASE_URL}/api/hotels/paged?${params.toString()}`, {
    next: { revalidate: 0 },
  })

  if (!res.ok) {
    throw new Error(`Error al obtener hoteles paginados: ${res.status}`)
  }

  const raw = await res.json()

  const result: PagedResultDto<HotelDto> = {
    items: raw.items ?? raw.Items ?? [],
    totalCount: raw.totalCount ?? raw.TotalCount ?? 0,
    pageNumber: raw.pageNumber ?? raw.PageNumber ?? pageNumber,
    pageSize: raw.pageSize ?? raw.PageSize ?? pageSize,
  }

  const items: UiHotel[] = result.items.map((h) => ({
    id: `H${h.id.toString().padStart(3, "0")}`,
    name: h.nombre,
    location: h.direccion,
    rooms: 0,
    status: h.estado ? "active" : "inactive",
    occupancy: 0,
    rating: h.categoria,
    lastUpdated: new Date().toISOString().slice(0, 10),
  }))

  return {
    items,
    totalCount: result.totalCount,
    pageNumber: result.pageNumber,
    pageSize: result.pageSize,
  }
}

export async function fetchActiveHotels(): Promise<HotelDto[]> {
  const res = await fetch(`${API_BASE_URL}/api/hotels/active`, {
    next: { revalidate: 0 },
  })

  if (!res.ok) {
    throw new Error(`Error al obtener hoteles activos: ${res.status}`)
  }

  const data: HotelDto[] = await res.json()
  return data
}

export async function fetchRoomTypesByHotel(hotelId: number): Promise<RoomTypeDto[]> {
  const res = await fetch(`${API_BASE_URL}/api/roomtypes/hotel/${hotelId}`, {
    next: { revalidate: 0 },
  })

  if (!res.ok) {
    throw new Error(`Error al obtener tipos de habitación del hotel ${hotelId}: ${res.status}`)
  }

  const data: RoomTypeDto[] = await res.json()
  return data
}

export async function fetchInventoryRange(
  hotelId: number,
  roomTypeId: number,
  fechaInicio: string,
  fechaFin: string
): Promise<InventoryDto[]> {
  const params = new URLSearchParams({
    fechaInicio,
    fechaFin,
  })

  const res = await fetch(
    `${API_BASE_URL}/api/inventory/hotel/${hotelId}/roomtype/${roomTypeId}?${params.toString()}`,
    {
      next: { revalidate: 0 },
    }
  )

  if (!res.ok) {
    throw new Error(
      `Error al obtener inventario del hotel ${hotelId}, tipo ${roomTypeId}: ${res.status}`
    )
  }

  const data: InventoryDto[] = await res.json()
  return data
}

export async function bulkCreateInventory(payload: BulkCreateInventoryDto): Promise<InventoryDto[]> {
  const res = await fetch(`${API_BASE_URL}/api/inventory/bulk`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  })

  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `No se pudo crear inventario masivo: ${res.status}`)
  }

  const data: InventoryDto[] = await res.json()
  return data
}

export async function checkAvailability(
  payload: CheckAvailabilityDto
): Promise<AvailabilityResultDto> {
  const res = await fetch(`${API_BASE_URL}/api/inventory/check-availability`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  })

  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `Error al verificar disponibilidad: ${res.status}`)
  }

  const data: AvailabilityResultDto = await res.json()
  return data
}

export async function fetchReservations(): Promise<UiReservation[]> {
  const res = await fetch(`${API_BASE_URL}/api/reservations`, {
    next: { revalidate: 0 },
  })

  if (!res.ok) {
    throw new Error(`Error al obtener reservas: ${res.status}`)
  }

  const data: ReservationDto[] = await res.json()
  return data.map(mapReservation)
}

export async function fetchReservationById(id: number): Promise<UiReservation> {
  const res = await fetch(`${API_BASE_URL}/api/reservations/${id}`, {
    next: { revalidate: 0 },
  })

  if (!res.ok) {
    throw new Error(`Error al obtener reserva ${id}: ${res.status}`)
  }

  const dto: ReservationDto = await res.json()
  return mapReservation(dto)
}

export async function fetchReservationsByHotel(hotelId: number): Promise<UiReservation[]> {
  const res = await fetch(`${API_BASE_URL}/api/reservations/hotel/${hotelId}`, {
    next: { revalidate: 0 },
  })

  if (!res.ok) {
    throw new Error(`Error al obtener reservas del hotel ${hotelId}: ${res.status}`)
  }

  const data: ReservationDto[] = await res.json()
  return data.map(mapReservation)
}

export async function createReservation(payload: CreateReservationDto): Promise<ReservationResultDto> {
  const res = await fetch(`${API_BASE_URL}/api/reservations`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  })

  const data = (await res.json()) as ReservationResultDto

  if (!res.ok || !data.success) {
    throw new Error(data.message || "No se pudo crear la reserva")
  }

  return data
}

export async function updateReservationStatus(id: number, estado: string): Promise<UiReservation> {
  const res = await fetch(`${API_BASE_URL}/api/reservations/${id}/status`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ estado } satisfies UpdateReservationStatusDto),
  })

  if (!res.ok) {
    throw new Error(`No se pudo actualizar la reserva ${id}: ${res.status}`)
  }

  const dto: ReservationDto = await res.json()
  return mapReservation(dto)
}

export async function cancelReservation(id: number): Promise<void> {
  const res = await fetch(`${API_BASE_URL}/api/reservations/${id}/cancel`, {
    method: "POST",
  })

  if (!res.ok) {
    throw new Error(`No se pudo cancelar la reserva ${id}: ${res.status}`)
  }
}

export async function createHotel(payload: CreateHotelDto): Promise<HotelDto> {
  const res = await fetch(`${API_BASE_URL}/api/hotels`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  })

  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `No se pudo crear el hotel: ${res.status}`)
  }

  const data: HotelDto = await res.json()
  return data
}

export async function createRoomType(payload: CreateRoomTypeDto): Promise<RoomTypeDto> {
  const res = await fetch(`${API_BASE_URL}/api/roomtypes`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  })

  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `No se pudo crear el tipo de habitación: ${res.status}`)
  }

  const data: RoomTypeDto = await res.json()
  return data
}

