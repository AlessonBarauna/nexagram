FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NexaGram.API/NexaGram.API.csproj", "src/NexaGram.API/"]
COPY ["src/NexaGram.Application/NexaGram.Application.csproj", "src/NexaGram.Application/"]
COPY ["src/NexaGram.Domain/NexaGram.Domain.csproj", "src/NexaGram.Domain/"]
COPY ["src/NexaGram.Infrastructure/NexaGram.Infrastructure.csproj", "src/NexaGram.Infrastructure/"]
COPY ["src/NexaGram.Shared/NexaGram.Shared.csproj", "src/NexaGram.Shared/"]
RUN dotnet restore "src/NexaGram.API/NexaGram.API.csproj"
COPY . .
WORKDIR "/src/src/NexaGram.API"
RUN dotnet build "NexaGram.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NexaGram.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NexaGram.API.dll"]
