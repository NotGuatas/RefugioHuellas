# 1) Build React
FROM node:20-alpine AS reactbuild
WORKDIR /web
COPY refugiohuellas-react/package*.json ./
RUN npm ci
COPY refugiohuellas-react ./
RUN npm run build

# 2) Build .NET
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY RefugioHuellas/RefugioHuellas.csproj RefugioHuellas/
WORKDIR /src/RefugioHuellas
RUN dotnet restore

WORKDIR /src
COPY . .
WORKDIR /src/RefugioHuellas
RUN dotnet publish -c Release -o /app/publish

# Copiar React build a wwwroot/app dentro del publish
COPY --from=reactbuild /web/dist /app/publish/wwwroot/app

# 3) Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "RefugioHuellas.dll"]
