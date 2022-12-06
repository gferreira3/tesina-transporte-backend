FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/TransporteApi/TransporteApi.csproj", "src/TransporteApi/"]
RUN dotnet restore "src/TransporteApi/TransporteApi.csproj"
COPY . .
WORKDIR "/src/src/TransporteApi"
RUN dotnet build "TransporteApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TransporteApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransporteApi.dll"]