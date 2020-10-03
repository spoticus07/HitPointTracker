FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["HitPointTracker_API.csproj", "./"]
RUN dotnet restore "HitPointTracker_API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "HitPointTracker_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HitPointTracker_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HitPointTracker_API.dll"]
