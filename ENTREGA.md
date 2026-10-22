# GLAB S10 Hotel y reservas

## Comando de scaffold utilizado

```powershell
dotnet ef dbcontext scaffold "Server=.\SQLEXPRESS;Database=HotelDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context HotelContext --no-onconfiguring --force
```

## Observaciones

- La base HotelDB se creó con 4 tipos de habitación, 9 habitaciones, 7 huéspedes y 10 reservas de prueba.
- El modelo se generó con Database First y se regeneró después de agregar `Observaciones` a `Reserva`.
- Las consultas y operaciones de guardado están concentradas en los repositorios y se ejecutan de forma asíncrona.
- Las habitaciones se dan de baja de forma lógica mediante `Activo = 0`.
- La disponibilidad excluye reservas que se cruzan con el rango y no considera las canceladas.

## Conclusiones

La aplicación cumple el flujo de gestión de habitaciones y reservas solicitado usando ASP.NET Core MVC y Entity Framework Core Database First. La separación mediante repositorios mantiene los controladores enfocados en las acciones HTTP y las validaciones del formulario.

## Capturas sugeridas

1. Habitaciones con filtro por tipo.
2. Disponibilidad con un rango de fechas.
3. Formulario de nueva reserva con Observaciones.
4. Listado de reservas y botón de cancelación.
