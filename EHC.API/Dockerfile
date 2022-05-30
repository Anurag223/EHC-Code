FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS base

COPY /EHC.API/certs/ehc.qa.crt /EHC.API/certs/CA1.crt /EHC.API/certs/CA2.crt /EHC.API/certs/Root.crt /usr/local/share/ca-certificates/
RUN cd /usr/local/share/ca-certificates && update-ca-certificates

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build
WORKDIR /src
COPY ["EHC.API/EHC.API.csproj", "EHC.API/"]
COPY ["EHC.API/NuGet.Config", "EHC.API/"]
COPY ["EHC.Common/EHC.Common.csproj", "EHC.Common/"]
RUN dotnet restore "EHC.API/EHC.API.csproj" --configfile "EHC.API/NuGet.Config"
COPY . .
WORKDIR "/src/EHC.API"
RUN dotnet build "EHC.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "EHC.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Tlm.EHC.API.dll"]