# Dokumentacja Techniczna - Aplikacja Czatowa

## 1. Opis Projektu

### Cel
Aplikacja czatowa to system komunikacji w czasie rzeczywistym umożliwiający użytkownikom prowadzenie rozmów grupowych poprzez interfejs webowy. System składa się z dwóch głównych komponentów: serwera WebSocket obsługującego komunikację w czasie rzeczywistym oraz klienta webowego opartego na technologii Blazor.

### Ogólny Przegląd
Aplikacja wykorzystuje architekturę klient-serwer z komunikacją WebSocket, co zapewnia natychmiastową wymianę wiadomości między użytkownikami. System obsługuje rejestrację i logowanie użytkowników, przechowując dane uwierzytelniania w lokalnym pliku JSON.

### Zastosowanie
- Komunikacja grupowa w czasie rzeczywistym
- Czat dla małych i średnich zespołów
- Platforma do szybkiej wymiany informacji
- Baza do dalszego rozwoju bardziej zaawansowanych funkcji komunikacyjnych

## 2. Technologie i Zależności

### Technologie Główne
- **.NET 9.0** - Framework aplikacyjny
- **C#** - Język programowania
- **Blazor Server** - Framework dla interfejsu użytkownika
- **WebSocket** - Protokół komunikacji w czasie rzeczywistym
- **Docker** - Konteneryzacja aplikacji

### Biblioteki i Zależności

#### Serwer WebSocket
```xml
<PackageReference Include="websocketsharp.core" Version="1.0.1" />
```

#### Klient Web
```xml
<PackageReference Include="Microsoft.AspNetCore" Version="2.3.0" />
<PackageReference Include="Websocket.Client" Version="5.2.0" />
<PackageReference Include="WebSocketSharp" Version="1.0.3-rc11" />
```

### Dodatkowe Technologie
- **Bootstrap** - Framework CSS dla responsywnego interfejsu
- **JSON** - Format przechowywania danych użytkowników
- **Docker Compose** - Orkiestracja kontenerów

## 3. Kompletny Opis Kodu

### Architektura Systemu

System składa się z dwóch głównych komponentów działających w oddzielnych kontenerach Docker:

#### 3.1 Serwer WebSocket (`server/WebSocket_Server/`)

**Program.cs** - Główny punkt wejścia serwera
```csharp
// Inicjalizacja serwera WebSocket na porcie 8081
var ws = new WebSocketServer("ws://server:8081");
ws.AddWebSocketService<ChatService>("/chat");
```

**ChatService.cs** - Główna klasa obsługująca połączenia WebSocket
- `OnOpen()` - Obsługa nowych połączeń
- `OnMessage()` - Przetwarzanie wiadomości od klientów
- `OnClose()` - Obsługa rozłączeń
- `OnError()` - Obsługa błędów

Obsługiwane typy wiadomości:
- `signup` - Rejestracja nowego użytkownika
- `signin` - Logowanie użytkownika
- `message` - Wysyłanie wiadomości czatu

**Authentication.cs** - System uwierzytelniania
- Przechowywanie użytkowników w pliku `storage.json`
- Metody `SignIn()` i `SignUp()` dla autoryzacji
- Automatyczne ładowanie i zapisywanie danych

#### 3.2 Klient Web (`client/`)

**Program.cs** - Konfiguracja aplikacji Blazor
```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<User>();
builder.Services.AddSingleton<WebSocketService>();
```

**Komponenty Blazor:**

- **Home.razor** - Strona główna z opcjami logowania/rejestracji
- **Login.razor** - Formularz logowania z walidacją
- **Register.razor** - Formularz rejestracji nowych użytkowników
- **Chat.razor** - Główny interfejs czatu z obsługą WebSocket
- **MainLayout.razor** - Układ strony z nawigacją

**Usługi:**

- **WebSocketService.cs** - Obsługa połączenia WebSocket po stronie klienta
- **User.cs** - Zarządzanie stanem użytkownika
- **AuthService.cs** - Walidacja danych uwierzytelniania

### 3.3 Przepływ Danych

1. **Rejestracja:**
   - Użytkownik wypełnia formularz rejestracji
   - Dane wysyłane przez WebSocket typu `signup`
   - Serwer sprawdza unikalność nazwy użytkownika
   - Zwracana odpowiedź z wynikiem operacji

2. **Logowanie:**
   - Użytkownik podaje dane logowania
   - Nawiązywane połączenie WebSocket
   - Wysyłana wiadomość typu `signin`
   - Po pozytywnej autoryzacji użytkownik dodawany do aktywnych połączeń

3. **Wysyłanie wiadomości:**
   - Wiadomość wysyłana przez WebSocket typu `message`
   - Serwer rozgłasza wiadomość do wszystkich aktywnych połączeń
   - Klienci otrzymują wiadomość i aktualizują interfejs

## 4. Instrukcja Instalacji

### Wymagania Systemowe
- Docker Engine 20.10+
- Docker Compose 2.0+
- 2GB RAM
- 1GB miejsca na dysku

### Instalacja za pomocą Docker Compose

1. **Klonowanie repozytorium:**
```bash
git clone <repository-url>
cd chat_client
```

2. **Budowanie i uruchomienie kontenerów:**
```bash
docker-compose up --build
```

3. **Weryfikacja instalacji:**
- Serwer WebSocket: `http://localhost:8081`
- Aplikacja kliencka: `http://localhost:80`

### Instalacja Lokalna (Development)

#### Serwer:
```bash
cd server/WebSocket_Server
dotnet restore
dotnet run
```

#### Klient:
```bash
cd client
dotnet restore
dotnet run
```

## 5. Konfiguracja

### Docker Compose Configuration
```yaml
# docker-compose.yml
services:
  server:
    build: ./server/WebSocket_Server
    ports:
      - "8081:8081"
    networks:
      - chatapp-network

  client:
    build: ./client
    ports:
      - "80:80"
    networks:
      - chatapp-network
    depends_on:
      - server
```

### Zmienne Środowiskowe

#### Serwer WebSocket
- `ASPNETCORE_ENVIRONMENT` - Środowisko aplikacji (Development/Production)
- Port serwera: `8081` (hardcoded w kodzie)

#### Klient Web
- `ASPNETCORE_ENVIRONMENT` - Środowisko aplikacji
- `ASPNETCORE_URLS` - Adresy URL aplikacji

### Pliki Konfiguracyjne

**client/appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**server/storage.json:** (automatycznie generowany)
```json
[
  {
    "name": "username",
    "password": "password"
  }
]
```

## 6. Użytkowanie

### Uruchomienie Aplikacji

1. **Użycie Docker Compose (Zalecane):**
```bash
docker-compose up -d
```

2. **Dostęp do aplikacji:**
   - Otwórz przeglądarkę internetową
   - Przejdź do `http://localhost:80`

### Proces Użytkowania

1. **Pierwsze uruchomienie:**
   - Kliknij "Zarejestruj się"
   - Wprowadź nazwę użytkownika i hasło (min. 3 znaki)
   - Potwierdź rejestrację

2. **Logowanie:**
   - Wprowadź dane logowania
   - Zostaniesz przekierowany do czatu

3. **Korzystanie z czatu:**
   - Wpisz wiadomość w polu tekstowym
   - Naciśnij Enter lub przycisk "Wyślij"
   - Wiadomości są widoczne dla wszystkich użytkowników online

4. **Wylogowanie:**
   - Kliknij przycisk "Wyloguj" w menu bocznym

### Funkcjonalności

- **Rejestracja nowych użytkowników**
- **Logowanie/wylogowanie**
- **Wysyłanie wiadomości w czasie rzeczywistym**
- **Powiadomienia systemowe** (dołączenie/opuszczenie czatu)
- **Responsywny interfejs** (desktop/mobile)
- **Walidacja formularzy**

## 7. API / Interfejsy

### WebSocket API

**Endpoint:** `ws://server:8081/chat`

#### Typy Wiadomości (Client → Server)

1. **Rejestracja użytkownika:**
```json
{
  "type": "signup",
  "name": "username",
  "password": "password123",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

2. **Logowanie:**
```json
{
  "type": "signin",
  "name": "username",
  "password": "password123",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

3. **Wiadomość czatu:**
```json
{
  "type": "message",
  "user": "username",
  "content": "Hello everyone!",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

#### Odpowiedzi (Server → Client)

1. **Odpowiedź autoryzacji:**
```json
{
  "Success": true,
  "Message": "Logged in!"
}
```

2. **Wiadomość czatu:**
```json
{
  "type": "chat_message",
  "user": "username",
  "content": "Hello everyone!",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

3. **Wiadomość systemowa:**
```json
{
  "type": "system_message",
  "user": "System",
  "content": "username dołączył do chatu",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

### Błędy i Kody Statusów

- **Sukces:** `Success: true`
- **Błąd autoryzacji:** `Success: false, Message: "Credentials mismatch!"`
- **Użytkownik już istnieje:** `Success: false, Message: "User already exists!"`
- **Brak danych:** `Success: false, Message: "Username and password are required!"`

## 8. Testowanie

### Testowanie Manualne

#### Test Rejestracji
1. Przejdź do `/register`
2. Wprowadź nową nazwę użytkownika i hasło
3. Sprawdź czy rejestracja się powiodła
4. Verificuj czy użytkownik został dodany do `storage.json`

#### Test Logowania
1. Użyj zarejestrowanych danych
2. Sprawdź przekierowanie do `/chat`
3. Verificuj nawiązanie połączenia WebSocket

#### Test Czatu
1. Otwórz aplikację w dwóch oknach przeglądarki
2. Zaloguj się różnymi użytkownikami
3. Wyślij wiadomości z jednego okna
4. Sprawdź czy wiadomości pojawiają się w drugim oknie

### Testowanie Automatyczne

#### Unit Tests (Przykładowy framework)
```bash
# Dodanie pakietu testowego
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
```

#### Przykład testu autoryzacji:
```csharp
[Fact]
public void SignUp_ValidCredentials_ReturnsSuccess()
{
    // Arrange
    var auth = new Authentication();
    
    // Act
    var result = auth.SignUp("testuser", "password123");
    
    // Assert
    Assert.True(result.Success);
}
```

### Testowanie Integracyjne

#### Test Docker Compose
```bash
# Testowanie budowania kontenerów
docker-compose build

# Testowanie uruchomienia
docker-compose up -d

# Sprawdzenie statusu kontenerów
docker-compose ps

# Testowanie connectivity
curl http://localhost:80
```

#### Test WebSocket Connection
```bash
# Używając wscat (WebSocket client)
npm install -g wscat
wscat -c ws://localhost:8081/chat
```

### Monitoring i Logi

#### Podgląd logów Docker:
```bash
# Logi serwera
docker-compose logs server

# Logi klienta
docker-compose logs client

# Logi w czasie rzeczywistym
docker-compose logs -f
```

#### Debugowanie problemów:
1. **Sprawdź status kontenerów:** `docker-compose ps`
2. **Sprawdź logi:** `docker-compose logs`
3. **Sprawdź sieć:** `docker network ls`
4. **Test connectivity:** `docker-compose exec client ping server`

### Problemy i Rozwiązania

#### Najczęstsze problemy:
1. **Błąd połączenia WebSocket:**
   - Sprawdź czy serwer działa na porcie 8081
   - Verificuj konfigurację sieci Docker

2. **Błąd autoryzacji:**
   - Sprawdź format pliku `storage.json`
   - Verificuj hasła (min. 3 znaki)

3. **Problemy z kontenerami:**
   - Wykonaj `docker-compose down` i `docker-compose up --build`
   - Sprawdź dostępność portów

---

## Dodatkowe Informacje

### Bezpieczeństwo
- Hasła przechowywane w postaci jawnej (do poprawy w produkcji)
- Brak szyfrowania komunikacji WebSocket
- Zalecane użycie HTTPS/WSS w środowisku produkcyjnym

### Wydajność
- Aktualnie brak limitów połączeń
- Brak persystencji wiadomości
- Zalecane dodanie bazy danych dla większej skalowalności

### Rozwój
- Możliwość dodania prywatnych rozmów
- Implementacja pokojów czatu
- Dodanie wysyłania plików
- Integracja z bazą danych
