#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PowerConsumptionOptimizer/PowerConsumptionOptimizer.csproj", "PowerConsumptionOptimizer/"]
COPY ["PowerProduction/PowerProduction.csproj", "PowerProduction/"]
COPY ["TeslaControl/TeslaControl.csproj", "TeslaControl/"]
RUN dotnet restore "PowerConsumptionOptimizer/PowerConsumptionOptimizer.csproj"
COPY . .
WORKDIR "/src/PowerConsumptionOptimizer"
RUN dotnet build "PowerConsumptionOptimizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN ["apt-get", "update"]
RUN ["apt-get", "-y", "install", "vi"]
RUN dotnet publish "PowerConsumptionOptimizer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PowerConsumptionOptimizer.dll"]