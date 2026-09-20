# Valoriza

**Plataforma Digital de Gestão de Diversidade, Equidade e Inclusão Organizacional**

Autor: Oliver Valentim Carvalho Santos — RA 2632071  
UNIP — PIM VII

---

## Tecnologias

| Camada | Tecnologia | Versão |
|--------|------------|--------|
| API | ASP.NET Core | .NET 10 (LTS) |
| Painel Web | ASP.NET Core MVC | .NET 10 |
| Mobile | Flutter | 3.47+ / Dart 3.13+ |
| Banco | SQL Server 2025 | Developer / Express |
| Auth | JWT + Identity | 10.0 |

---

## Estrutura

```
Valoriza/
├── backend/
│   ├── Valoriza.API/     → API REST + Swagger
│   └── Valoriza.Web/     → Painel administrativo MVC
├── mobile/
│   └── valoriza_app/     → App Flutter
├── Valoriza.sln
└── README.md
```

---

## Como executar

### 1. Banco de dados
Ajuste a connection string em `appsettings.json` se necessário:
```
Server=localhost;Database=ValorizaDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### 2. API
```bash
cd backend/Valoriza.API
dotnet restore
dotnet ef database update
dotnet run
```
Acesse: `https://localhost:PORTA/swagger`

### 3. Painel Web
```bash
cd backend/Valoriza.Web
dotnet restore
dotnet run
```
Acesse: `https://localhost:PORTA`

### 4. Mobile
```bash
cd mobile/valoriza_app
flutter pub get
flutter run
```

---

## Funcionalidades

- Login JWT com papéis (AdminValoriza, AdminEmpresa, GestorDEI, Colaborador)
- Trilhas de treinamento DEI
- Canal de denúncias confidencial
- Indicadores de diversidade
- Mentorias
- Gestão de usuários
- Painel administrativo web
- App mobile multiplataforma
