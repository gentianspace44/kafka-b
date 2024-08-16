FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PendingBatchVoucherProcessingService/PendingBatchVoucherProcessingService.csproj", "PendingBatchVoucherProcessingService/"]
COPY ["VPS.Domain/VPS.Domain.csproj", "VPS.Domain/"]
COPY ["VPS.Helpers/VPS.Helpers.csproj", "VPS.Helpers/"]
COPY ["VPS.Infrastructure/VPS.Infrastructure.csproj", "VPS.Infrastructure/"]
COPY ["VPS.Services/VPS.Services.csproj", "VPS.Services/"]
COPY ["VPS.API/VPS.API.csproj", "VPS.API/"]
RUN dotnet restore "PendingBatchVoucherProcessingService/PendingBatchVoucherProcessingService.csproj"
COPY . .
WORKDIR "/src/PendingBatchVoucherProcessingService"
RUN dotnet build "PendingBatchVoucherProcessingService.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PendingBatchVoucherProcessingService.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1.1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
ENTRYPOINT ["dotnet", "PendingBatchVoucherProcessingService.dll"]
