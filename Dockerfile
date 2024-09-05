#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VirtualWallet.WEB/VirtualWallet.WEB.csproj", "VirtualWallet.WEB/"]
COPY ["VirtualWallet.BUSINESS/VirtualWallet.BUSINESS.csproj", "VirtualWallet.BUSINESS/"]
COPY ["VirtualWallet.DATA/VirtualWallet.DATA.csproj", "VirtualWallet.DATA/"]
RUN dotnet restore "./VirtualWallet.WEB/./VirtualWallet.WEB.csproj"
COPY . .
WORKDIR "/src/VirtualWallet.WEB"
RUN dotnet build "./VirtualWallet.WEB.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VirtualWallet.WEB.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualWallet.WEB.dll"]