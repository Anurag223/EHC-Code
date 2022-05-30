#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS base

COPY /EHC.ADMIN.API/certs/ehc.qa.crt /EHC.ADMIN.API/certs/CA1.crt /EHC.ADMIN.API/certs/CA2.crt /EHC.ADMIN.API/certs/Root.crt /usr/local/share/ca-certificates/
RUN cd /usr/local/share/ca-certificates && update-ca-certificates

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build
WORKDIR /src
COPY ["EHC.ADMIN.API/EHC.ADMIN.API.csproj", "EHC.ADMIN.API/"]
COPY ["EHC.ADMIN.API/NuGet.Config", "EHC.ADMIN.API/"]
COPY ["EHC.Common/EHC.Common.csproj", "EHC.Common/"]
RUN dotnet restore "EHC.ADMIN.API/EHC.ADMIN.API.csproj" --configfile "EHC.ADMIN.API/NuGet.Config"
COPY . .
WORKDIR "/src/EHC.ADMIN.API"
RUN dotnet build "EHC.ADMIN.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "EHC.ADMIN.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "EHC.ADMIN.API.dll"]