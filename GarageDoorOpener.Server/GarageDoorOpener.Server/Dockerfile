#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Can't build arm7 with the below line.
#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["GarageDoorOpener.Server/GarageDoorOpener.Server.csproj", "GarageDoorOpener.Server/"]
RUN dotnet restore "GarageDoorOpener.Server/GarageDoorOpener.Server.csproj"
COPY . .
WORKDIR "/src/GarageDoorOpener.Server"
RUN dotnet build "GarageDoorOpener.Server.csproj" -c Release -o /app/build

FROM build AS publish
ARG password
RUN dotnet publish "GarageDoorOpener.Server.csproj" -c Release -o /app/publish
RUN dotnet dev-certs https -ep /.aspnet/https/aspnetapp.pfx -p $password

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /.aspnet/https/aspnetapp.pfx .
ENTRYPOINT ["dotnet", "GarageDoorOpener.Server.dll"]