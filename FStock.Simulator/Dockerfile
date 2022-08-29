FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FStock.Simulator/FStock.Simulator.fsproj", "FStock.Simulator/"]
RUN dotnet restore "FStock.Simulator/FStock.Simulator.fsproj"
COPY . .
WORKDIR "/src/FStock.Simulator"
RUN dotnet build "FStock.Simulator.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FStock.Simulator.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FStock.Simulator.dll"]
