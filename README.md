# Viajes Altairis — MVP Backoffice

## Descripción

MVP de backoffice para la gestión hotelera B2B de Viajes Altairis. Sistema integral para administrar hoteles, inventario de habitaciones, disponibilidad y reservas en tiempo real.

---

## Instrucciones de Ejecución

### Prerrequisitos

- **Docker Desktop** instalado y en ejecución ([descargar](https://www.docker.com/products/docker-desktop/))

### Ejecución

Ejecuta el siguiente comando en la raíz del proyecto:

```bash
docker compose up --build
```

Este comando levanta:

1. **SQL Server** (puerto 1433) — Base de datos  
2. **API Backend** (puerto 5000) — .NET 8 Web API  
3. **Frontend** (puerto 3000) — Next.js  

La primera ejecución puede tardar unos minutos mientras se construyen las imágenes y se inicializa la base de datos.

### URLs de Acceso

| Aplicación  | URL                          |
|-------------|------------------------------|
| **Frontend** | http://localhost:3000        |
| **Swagger API** | http://localhost:5000       |

### Credenciales

**No hay sistema de login** en este MVP. El acceso al backoffice es directo. La base de datos se inicializa automáticamente con datos de prueba (10 hoteles, tipos de habitación, inventario y reservas de ejemplo).

---

## Stack Tecnológico

### Frontend

- **Next.js** (App Router) — Framework React con renderizado híbrido
- **Tailwind CSS** — Estilos utility-first
- **Shadcn UI** — Componentes accesibles basados en Radix UI
- **date-fns** — Manejo de fechas

### Backend

- **.NET 8 Web API** — API REST
- **Entity Framework Core** — ORM y migraciones
- **SQL Server** — Base de datos relacional
- **Swagger/OpenAPI** — Documentación interactiva

### Infraestructura

- **Docker Compose** — Orquestación de contenedores
- **SQL Server 2022** — Contenedor oficial Microsoft

---

## Decisiones de Arquitectura

### Backend: Clean Architecture

El backend está organizado en capas siguiendo **Clean Architecture**:

| Capa | Descripción |
|------|-------------|
| **Domain** | Entidades puras (`Hotel`, `Reservation`, `Inventory`, `RoomType`) e interfaces de repositorios |
| **Application** | Servicios de negocio (`HotelService`, `ReservationService`, `InventoryService`), DTOs y lógica de aplicación |
| **Infrastructure** | Implementación de repositorios, `DbContext` y acceso a datos |
| **API** | Controladores REST, configuración de CORS, Swagger y pipeline HTTP |

### Patrón Repository

Cada entidad de agregado dispone de un **repositorio específico** (`IHotelRepository`, `IReservationRepository`, `IInventoryRepository`) que abstrae el acceso a datos. Se utiliza también un `IRepository<T>` genérico para entidades CRUD básicas, manteniendo la separación entre dominio e infraestructura.

### Auto-Migration y Seeding

Al iniciar la API se ejecutan de forma automática:

1. **Migraciones** — `context.Database.Migrate()` aplica el esquema actual a la base de datos.
2. **Seeding** — `DbInitializer.SeedAsync()` inserta datos de prueba si la base está vacía (10 hoteles españoles, tipos de habitación, inventario para 30 días y reservas de ejemplo).

Todo el proceso ocurre en el arranque, sin pasos manuales adicionales.

### Frontend: Componentes Servidor y Cliente

- **Server Components** — Páginas y layouts se renderizan en el servidor cuando es posible.
- **Client Components** — Componentes interactivos (`"use client"`) para tablas, formularios, matriz de disponibilidad y navegación, optimizando la carga inicial y la interactividad.

---

## Estructura del Proyecto

```
viajes-altairis-mvp/
├── docker-compose.yml          # Orquestación: db, backend, frontend
├── backend/
│   ├── Altairis.API/           # Punto de entrada, Program.cs, Controllers
│   ├── Altairis.Application/   # Servicios, DTOs
│   ├── Altairis.Domain/        # Entidades, Interfaces
│   ├── Altairis.Infrastructure/# DbContext, Repositories, DbInitializer
│   └── Dockerfile
├── frontend/
│   ├── app/                    # App Router: page.tsx, layout.tsx, rutas
│   │   ├── page.tsx            # Dashboard
│   │   ├── hotels/             # Gestión de hoteles
│   │   ├── availability/       # Matriz de disponibilidad
│   │   └── reservations/       # Reservas
│   ├── components/             # Componentes reutilizables
│   │   ├── app-sidebar.tsx     # Navegación lateral
│   │   ├── dashboard-shell.tsx # Layout del backoffice
│   │   ├── availability-matrix.tsx
│   │   ├── hotels-table.tsx
│   │   ├── reservations-table.tsx
│   │   └── ui/                 # Shadcn UI
│   ├── lib/                    # api.ts (fetch, DTOs), utils.ts
│   └── Dockerfile
└── README.md
```

---

## API Principal

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/hotels` | Lista de hoteles |
| GET | `/api/hotels/paged` | Hoteles paginados con búsqueda |
| POST | `/api/hotels` | Crear hotel |
| GET | `/api/roomtypes/hotel/{id}` | Tipos de habitación por hotel |
| GET | `/api/inventory/hotel/{h}/roomtype/{rt}` | Inventario por rango de fechas |
| POST | `/api/inventory/bulk` | Crear inventario masivo |
| POST | `/api/inventory/check-availability` | Verificar disponibilidad |
| GET | `/api/reservations` | Lista de reservas |
| POST | `/api/reservations` | Crear reserva |
| PATCH | `/api/reservations/{id}/status` | Actualizar estado |
| POST | `/api/reservations/{id}/cancel` | Cancelar reserva |

---

*Documentación técnica — Viajes Altairis MVP*
