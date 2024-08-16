#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y --no-install-recommends curl telnet netcat tzdata && \
    ln -fs /usr/share/zoneinfo/Africa/Johannesburg /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    apt-get clean

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["VPS.Provider.OTT/VPS.Provider.OTT.csproj", "VPS.Provider.OTT/"]
COPY ["VPS.Domain/VPS.Domain.csproj", "VPS.Domain/"]
COPY ["VPS.Helpers/VPS.Helpers.csproj", "VPS.Helpers/"]
COPY ["VPS.Infrastructure/VPS.Infrastructure.csproj", "VPS.Infrastructure/"]
COPY ["VPS.Services/VPS.Services.csproj", "VPS.Services/"]
RUN dotnet restore "VPS.Provider.OTT/VPS.Provider.OTT.csproj"
COPY . .
WORKDIR "/src/VPS.Provider.OTT"
RUN dotnet build "VPS.Provider.OTT.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VPS.Provider.OTT.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1.1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
ENTRYPOINT ["dotnet", "VPS.Provider.OTT.dll"]