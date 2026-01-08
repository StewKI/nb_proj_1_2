FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

COPY *.slnx ./
COPY NppCore/*.csproj ./NppCore/
COPY NppApi/*.csproj ./NppApi/

RUN dotnet restore

COPY . .

WORKDIR /app/NppApi

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]
