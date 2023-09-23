#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.Endorphina/Tgs.GameRecord.Endorphina.csproj", "Tgs.GameRecord.Endorphina/"]
COPY ["Tgs.GameRecord.Endorphina.Core/Tgs.GameRecord.Endorphina.Core.csproj", "Tgs.GameRecord.Endorphina.Core/"]
RUN dotnet restore "Tgs.GameRecord.Endorphina/Tgs.GameRecord.Endorphina.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.Endorphina"
RUN dotnet build "Tgs.GameRecord.Endorphina.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameRecord.Endorphina.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.Endorphina.dll"]