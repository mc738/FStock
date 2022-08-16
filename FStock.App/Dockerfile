FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FStock.App/FStock.App.fsproj", "FStock.App/"]
RUN dotnet restore "FStock.App/FStock.App.fsproj"
COPY . .
WORKDIR "/src/FStock.App"
RUN dotnet build "FStock.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FStock.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FStock.App.dll"]
