#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["VPS.RedemptionWidget/VPS.RedemptionWidget.csproj", "VPS.RedemptionWidget/"]
RUN dotnet restore "VPS.RedemptionWidget/VPS.RedemptionWidget.csproj"
COPY . .
WORKDIR "/src/VPS.RedemptionWidget"
RUN dotnet build "VPS.RedemptionWidget.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VPS.RedemptionWidget.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1.1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
ENTRYPOINT ["dotnet", "VPS.RedemptionWidget.dll"]