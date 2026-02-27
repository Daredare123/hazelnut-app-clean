FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Ensure required PostgreSQL libraries are present for GSSAPI
RUN apt-get update && apt-get install -y libgssapi-krb5-2

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "HazelnutVeb.dll"]