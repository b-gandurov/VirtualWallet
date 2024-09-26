# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VirtualWallet.WEB/VirtualWallet.WEB.csproj", "VirtualWallet.WEB/"]
COPY ["VirtualWallet.BUSINESS/VirtualWallet.BUSINESS.csproj", "VirtualWallet.BUSINESS/"]
COPY ["VirtualWallet.DATA/VirtualWallet.DATA.csproj", "VirtualWallet.DATA/"]
RUN dotnet restore "./VirtualWallet.WEB/VirtualWallet.WEB.csproj"
COPY . .
WORKDIR "/src/VirtualWallet.WEB"
RUN dotnet build "./VirtualWallet.WEB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./VirtualWallet.WEB.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualWallet.WEB.dll"]
