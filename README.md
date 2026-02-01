# RefugioHuellas 🐾

RefugioHuellas es una aplicación web ASP.NET Core MVC (.NET 9) para la gestión de adopciones de perros en un refugio.
Incluye autenticación con Identity, control de roles (Admin / Usuario), evaluación de compatibilidad entre adoptante y perro, y administración completa de los registros.

# 🔐 Autenticación y roles

Sistema de login y registro con ASP.NET Core Identity.

Roles

Admin: Gestiona perros, revisa solicitudes y aprueba adopciones.

Usuario: Visualiza perros disponibles, llena formularios de compatibilidad y envía solicitudes.

# 🐕 Gestión de perros

Crear, editar, eliminar y listar perros (solo Admin).

Atributos principales:

Nombre

Descripción

Raza

Tamaño

Nivel de energía

Entorno ideal

Foto

Estado de salud

Fecha de ingreso

Esterilización

# 📄 Solicitudes de adopción

Los usuarios pueden enviar una solicitud personalizada por perro.

Cada solicitud se evalúa mediante un formulario de compatibilidad dinámico.

# 🧠 Sistema de compatibilidad inteligente

Evaluación basada en:

Rasgos de personalidad del adoptante.

Características del perro.

Cálculo de un porcentaje de compatibilidad (0–100%) mediante un modelo de ponderaciones.

Prevención de duplicados: un usuario no puede solicitar el mismo perro más de una vez.

# 🛠 Panel administrativo avanzado

El Administrador tiene acceso a tres vistas clave:

Solicitudes de adopción

Lista completa de solicitudes con:

Puntaje de compatibilidad

Estado

Fecha

Mejores coincidencias

Muestra los tres candidatos más compatibles por perro dentro de una ventana de tiempo definida.

Mejores candidatos

Resumen el mejor candidato por cada perro

# 🔄 Flujo de adopción

El usuario inicia sesión.

Selecciona un perro disponible.

Llena el formulario de compatibilidad.

El sistema guarda la solicitud con su puntaje.

Al finalizar la ventana, el Admin aprueba al mejor candidato desde “Mejores Candidatos”.

# 🧩 Arquitectura y buenas prácticas (SOLID & Patrones de Diseño)

El proyecto aplica principios SOLID y patrones de diseño aprendidos en el taller formativo, manteniendo una arquitectura limpia, extensible y desacoplada.

# ✅ Principios SOLID aplicados
SRP – Single Responsibility Principle

Cada clase tiene una única responsabilidad:

El cálculo de compatibilidad se separa de los controladores.

La lógica de reglas de compatibilidad vive en clases independientes.

La gestión de almacenamiento de imágenes se delega a un servicio especializado (IPhotoStorage).

OCP – Open/Closed Principle

El sistema está abierto a extensión pero cerrado a modificación:

Para agregar un nuevo rasgo de compatibilidad, se crea una nueva clase sin modificar el servicio principal.

El motor de compatibilidad no necesita cambios al añadir nuevas reglas.

# 🧠 Patrones de Diseño implementados
Strategy Pattern

Cada regla de compatibilidad se implementa como una estrategia independiente.

Permite intercambiar o extender reglas sin afectar el resto del sistema.

Ejemplos:

Tipo de vivienda

Nivel de energía

Espacio disponible

Tiempo disponible

Tolerancia al ruido

Repository Pattern

Se abstrae el acceso a datos mediante repositorios.

Los controladores y servicios no dependen directamente de Entity Framework.

Facilita mantenimiento, pruebas y escalabilidad.

Estos patrones aseguran una arquitectura desacoplada, reutilizable y alineada con buenas prácticas profesionales.

# 📋 Requisitos

.NET 9 SDK

Git

PostgreSQL

# ▶️ Cómo correr el proyecto

Clonar el repositorio:

git clone https://github.com/NotGuatas/RefugioHuellas.git
cd RefugioHuellas


Restaurar dependencias:

dotnet restore


Crear la base de datos:

dotnet ef database update


Ejecutar la aplicación:

dotnet run

# 🔑 Credenciales iniciales

Admin

Email: admin@huellas.com

Password: Admin123$

# 🌐 Proyecto desplegado

https://refugiohuellas-2.onrender.com/app/login

Cuenta Admin:

Email: admin@huellas.com

Password: Admin123$
