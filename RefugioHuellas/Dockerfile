# ============================
# 1) Etapa de build
# ============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos el .csproj desde la carpeta del proyecto
COPY ["RefugioHuellas/RefugioHuellas.csproj", "RefugioHuellas/"]

# Nos movemos a la carpeta del proyecto
WORKDIR /src/RefugioHuellas

# Restaurar paquetes
RUN dotnet restore "RefugioHuellas.csproj"

# Volvemos a /src y copiamos TODO el código del repo
WORKDIR /src
COPY . .

# De nuevo a la carpeta del proyecto para publicar
WORKDIR /src/RefugioHuellas
RUN dotnet publish "RefugioHuellas.csproj" -c Release -o /app/publish

# ============================
# 2) Etapa de runtime
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiar lo publicado
COPY --from=build /app/publish .

# Puerto interno donde escuchará la app
EXPOSE 8080

# Hacer que ASP.NET escuche en 0.0.0.0:8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Nombre del dll de tu proyecto (igual que el .csproj)
ENTRYPOINT ["dotnet", "RefugioHuellas.dll"]
