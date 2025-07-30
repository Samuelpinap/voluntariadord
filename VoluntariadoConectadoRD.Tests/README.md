# Pruebas Unitarias - Postulaciones de Voluntariado

Este proyecto contiene las pruebas unitarias para las funcionalidades de postulaciones del sistema de voluntariado.

## Funcionalidades Probadas

### 1. Endpoint GET para Visualizar Postulaciones del Usuario

**Endpoint:** `GET /api/voluntariado/my-applications`

**Descripción:** Permite a los usuarios voluntarios y administradores ver todas sus postulaciones a oportunidades de voluntariado.

**Pruebas Implementadas:**
- ✅ Retorna las postulaciones del usuario cuando el ID es válido
- ✅ Retorna lista vacía cuando el usuario no tiene postulaciones
- ✅ Retorna BadRequest cuando el ID de usuario es inválido
- ✅ Retorna InternalServerError cuando el servicio lanza una excepción

### 2. Servicio de Postulaciones

**Clase:** `OpportunityService`

**Métodos Probados:**

#### `GetUserApplicationsAsync(int userId)`
- ✅ Retorna las postulaciones del usuario especificado
- ✅ Retorna lista vacía para usuarios sin postulaciones
- ✅ Incluye información completa de la oportunidad y usuario

#### `ApplyToOpportunityAsync(int opportunityId, int userId, ApplyToOpportunityDto? applyDto)`
- ✅ Crea una nueva postulación exitosamente
- ✅ Retorna false cuando la oportunidad no existe
- ✅ Retorna false cuando el usuario ya aplicó anteriormente
- ✅ Retorna false cuando la oportunidad no está activa
- ✅ Incrementa el contador de voluntarios inscritos

#### `UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status, int organizationId, string? notes)`
- ✅ Actualiza el estado de la postulación exitosamente
- ✅ Retorna false cuando la aplicación no existe
- ✅ Retorna false cuando la organización no es dueña de la oportunidad

## Estructura de Pruebas

### ApplicationControllerTests.cs
Pruebas unitarias para el controlador `VoluntariadoController`:
- Usa Moq para simular el servicio `IOpportunityService`
- Prueba todos los escenarios de respuesta del endpoint
- Verifica mensajes de error y éxito

### ApplicationServiceTests.cs
Pruebas unitarias para el servicio `OpportunityService`:
- Usa Entity Framework In-Memory para la base de datos
- Incluye datos de prueba completos (usuarios, organizaciones, oportunidades, postulaciones)
- Prueba la lógica de negocio del servicio

## Datos de Prueba

### Usuarios
- **Usuario 1:** Juan Pérez (juan.perez@test.com) - Voluntario
- **Usuario 2:** María García (maria.garcia@test.com) - Voluntario

### Organizaciones
- **Fundación Test:** Organización de prueba

### Oportunidades
- **Ayuda en Hospital:** Oportunidad en Santo Domingo, área de salud
- **Limpieza de Playa:** Oportunidad en Boca Chica, área de medio ambiente

### Postulaciones
- Usuario 1 → Oportunidad 1 (Pendiente)
- Usuario 1 → Oportunidad 2 (Aceptada)
- Usuario 2 → Oportunidad 1 (Pendiente)

## Ejecutar Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con más detalles
dotnet test --verbosity normal

# Ejecutar pruebas específicas
dotnet test --filter "ApplicationControllerTests"
dotnet test --filter "ApplicationServiceTests"
```

## Cobertura de Pruebas

Las pruebas cubren:
- ✅ Casos exitosos
- ✅ Casos de error
- ✅ Validaciones de entrada
- ✅ Manejo de excepciones
- ✅ Lógica de negocio
- ✅ Integración con base de datos

## Tecnologías Utilizadas

- **xUnit:** Framework de pruebas
- **Moq:** Framework de mocking
- **Entity Framework In-Memory:** Base de datos en memoria para pruebas
- **Microsoft.AspNetCore.Mvc.Testing:** Pruebas de controladores

## Notas Importantes

1. **Base de Datos In-Memory:** Cada prueba usa una base de datos en memoria única para evitar interferencias
2. **Datos de Prueba:** Se crean datos de prueba completos en cada ejecución
3. **Limpieza:** Se implementa `IDisposable` para limpiar recursos después de cada prueba
4. **Aislamiento:** Las pruebas son independientes entre sí 