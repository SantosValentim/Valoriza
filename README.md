# Valoriza

**Plataforma digital de gestão de Diversidade, Equidade e Inclusão (DEI) organizacional**

**Autor:** Oliver Valentim Carvalho Santos — **RA 2632071**  
**Instituição:** Universidade Paulista (UNIP)  
**Trabalho:** Projeto Integrador Multidisciplinar (PIM)

Repositório: [https://github.com/SantosValentim/Valoriza.git](https://github.com/SantosValentim/Valoriza.git)

---

## Sumário

1. [Sobre o projeto](#sobre-o-projeto)
2. [Tecnologias](#tecnologias)
3. [Estrutura do repositório](#estrutura-do-repositório)
4. [Papéis de usuário](#papéis-de-usuário)
5. [Instalação das ferramentas](#instalação-das-ferramentas)
   - [SQL Server Developer ou Express](#1-sql-server-developer-ou-express)
   - [SQL Server Management Studio (SSMS)](#2-sql-server-management-studio-ssms)
   - [Visual Studio 2022](#3-visual-studio-2022)
   - [Visual Studio Code (alternativa)](#4-visual-studio-code-alternativa)
   - [Flutter SDK](#5-flutter-sdk)
   - [Android Studio e emulador](#6-android-studio-e-emulador)
6. [Configurar o projeto](#configurar-o-projeto)
7. [Tutorial para rodar o projeto](#tutorial-para-rodar-o-projeto)
8. [Funcionalidades](#funcionalidades)
9. [Segurança](#segurança)
10. [Migrações EF Core](#migrações-ef-core)
11. [Problemas comuns](#problemas-comuns)
12. [Scripts rápidos](#scripts-rápidos)
13. [Autor](#autor)

---

## Sobre o projeto

A **Valoriza** é uma plataforma digital voltada à gestão de **Diversidade, Equidade e Inclusão (DEI)** nas organizações. O sistema apoia a construção de uma cultura inclusiva e antirracista por meio de:

- Trilhas de treinamento DEI (texto, vídeo e quiz)
- Canal de denúncias com protocolo e acompanhamento
- Indicadores de diversidade e dashboard gerencial
- Programas de mentoria
- Comunicação inclusiva (checklists para gestores DEI)
- Painel administrativo web e aplicativo mobile
- Controle de assinaturas e churn (AdminValoriza)
- Relatórios gerenciais em CSV
- Auditoria de operações críticas relacionadas a denúncias

O projeto foi desenvolvido como **Projeto Integrador Multidisciplinar (PIM)** na UNIP.

---

## Tecnologias

| Camada       | Tecnologia            | Observação                          |
|--------------|-----------------------|-------------------------------------|
| API          | ASP.NET Core          | .NET 10, JWT, Swagger               |
| Painel Web   | ASP.NET Core MVC      | Identity (cookie), proteção CSRF    |
| Mobile       | Flutter / Dart        | Denúncias offline-first             |
| Banco        | SQL Server            | EF Core Code-First                  |
| Autenticação | ASP.NET Core Identity | Hash PBKDF2 + JWT (API)             |

---

## Estrutura do repositório

```
Valoriza/
├── backend/
│   ├── Valoriza.API/          # API REST + Identity + EF Core + Swagger
│   └── Valoriza.Web/          # Painel administrativo MVC
├── mobile/
│   └── valoriza_app/          # Aplicativo Flutter
├── Valoriza.sln
├── .gitignore
└── README.md
```

---

## Papéis de usuário

| Papel           | Acesso principal                                                        |
|-----------------|-------------------------------------------------------------------------|
| AdminValoriza   | Dashboard, empresas, churn, usuários e módulos da própria Valoriza      |
| AdminEmpresa    | Dashboard, treinamentos, denúncias, indicadores e usuários da empresa   |
| GestorDEI       | Dashboard, indicadores, comunicação inclusiva e denúncias               |
| Colaborador     | Treinamentos, mentorias, denúncias próprias e perfil                    |

No **aplicativo mobile** não há telas de usuários, empresas nem churn.

---

## Instalação das ferramentas

> Ambiente de referência: **Windows 10/11**.  
> Ordem sugerida: banco → SSMS → IDE (.NET) → Android Studio → Flutter.

---

### 1. SQL Server Developer ou Express

Edição **gratuita** para desenvolvimento (Developer com recursos equivalentes ao Enterprise; Express com limites de hardware).

#### Download

1. Acesse: https://www.microsoft.com/sql-server/sql-server-downloads  
2. Em **Developer** ou **Express**, clique em **Download now**.  
3. Ou busque “SQL Server 2022 Developer/Express” / “SQL Server 2025 Developer/Express” no site da Microsoft.

#### Instalação

1. Execute o instalador e escolha **Instalação básica** ou **Personalizada**.  
2. Em tipo de instalação, selecione **Developer** ou **Express**.  
3. Aceite os termos e continue.  
4. Anote o caminho de instalação e a instância (geralmente `MSSQLSERVER` = padrão, ou o nome que você definir).  
5. Em autenticação, pode usar:
   - **Modo Autenticação do Windows** (recomendado no dev local), ou  
   - **Modo misto** (Windows + usuário `sa` com senha forte).  
6. Conclua a instalação e reinicie o PC se o instalador pedir.

#### Conferir se o serviço está rodando

1. Tecla `Win + R` → `services.msc` → Enter.  
2. Procure **SQL Server (MSSQLSERVER)** ou **SQL Server (NOME_DA_INSTANCIA)**.  
3. Status deve estar **Em execução**. Se não, botão direito → **Iniciar**.

#### Connection string típica

```text
Server=localhost;Database=ValorizaDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Instância nomeada:

```text
Server=localhost\NOMEDAINSTANCIA;Database=ValorizaDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

---

### 2. SQL Server Management Studio (SSMS)

Ferramenta gráfica para conectar ao SQL Server, criar bancos, rodar scripts e inspecionar tabelas.

#### Download

1. Acesse: https://learn.microsoft.com/ssms/install/install  
2. Clique em **Download SQL Server Management Studio (SSMS)**.  
3. Baixe a versão mais recente para Windows.

#### Instalação

1. Execute o arquivo `.exe` baixado.  
2. Aceite os termos de licença.  
3. Mantenha o caminho de instalação padrão (recomendado).  
4. Clique em **Instalar** e aguarde a conclusão.  
5. Ao finalizar, marque **Iniciar SSMS** (opcional) e feche o instalador.

#### Conferir se está funcionando

1. Abra **SQL Server Management Studio** no menu Iniciar.  
2. Na tela **Conectar ao Servidor**:
   - **Tipo de servidor:** Mecanismo de Banco de Dados  
   - **Nome do servidor:** `localhost` ou `localhost\NOMEDAINSTANCIA` ou `.`  
   - **Autenticação:** Autenticação do Windows (se instalou o SQL assim)  
3. Clique em **Conectar**.  
4. No Object Explorer, expanda **Databases**. Se conectar sem erro, está ok.

#### Criar o banco manualmente (opcional)

O Entity Framework pode criar o banco com `dotnet ef database update`. Se quiser criar antes:

1. Botão direito em **Databases** → **New Database**.  
2. Nome: `ValorizaDb` → **OK**.

Ou em **New Query**:

```sql
CREATE DATABASE ValorizaDb;
```

---

### 3. Visual Studio 2022

IDE completa para desenvolver e depurar a API e o painel Web (.NET).

#### Download

1. Acesse: https://visualstudio.microsoft.com/downloads/  
2. Em **Community 2022** (gratuita), clique em **Download gratuito**.  
3. Execute o instalador (`VisualStudioSetup.exe`).

#### Instalação

1. No instalador, aguarde o carregamento das cargas de trabalho.  
2. Marque a workload:
   - **Desenvolvimento para ASP.NET e web**  
3. Em **Componentes individuais** (opcional, mas útil):
   - SDK do .NET correspondente ao projeto (** .NET 10** quando disponível)  
   - Ferramentas de dados do SQL Server  
4. Clique em **Instalar** e aguarde (pode demorar).  
5. Ao terminar, clique em **Iniciar** / abra o Visual Studio.  
6. Faça login com conta Microsoft (opcional) e escolha o tema.

#### Conferir se está funcionando

1. Abra o Visual Studio.  
2. **Arquivo → Abrir → Projeto/Solução** → selecione `Valoriza.sln`.  
3. No terminal integrado ou no Prompt de Comando:

```bash
dotnet --version
```

Deve exibir a versão do SDK (ex.: 10.x).

#### Abrir e rodar o projeto no Visual Studio

1. Abra `Valoriza.sln`.  
2. No Gerenciador de Soluções, botão direito em **Valoriza.API** → **Definir como projeto de inicialização**.  
3. Pressione **F5** (com depuração) ou **Ctrl+F5** (sem depuração).  
4. Para o Web, defina **Valoriza.Web** como inicialização da mesma forma.

---

### 4. Visual Studio Code (alternativa)

Editor leve para backend e Flutter. Pode ser usado no lugar do Visual Studio.

#### Download

1. Acesse: https://code.visualstudio.com/  
2. Clique em **Download for Windows**.  
3. Execute o instalador.

#### Instalação

1. Aceite o contrato.  
2. Marque as opções:
   - **Add to PATH**  
   - **Add “Open with Code” action** (opcional, recomendado)  
3. Conclua a instalação e abra o VS Code.

#### Extensões

No VS Code, abra **Extensions** (`Ctrl+Shift+X`) e instale:

1. **C# Dev Kit** ou **C#** (Microsoft)  
2. **Flutter** (Dart Code)  
3. **Dart** (Dart Code)  
4. **SQL Server (mssql)** (opcional)

#### Conferir se está funcionando

1. **File → Open Folder** → pasta `Valoriza`.  
2. Abra o terminal integrado (`Ctrl+'`):

```bash
dotnet --version
flutter --version
```

---

### 5. Flutter SDK

SDK para desenvolver e executar o aplicativo mobile.

#### Download

1. Acesse: https://docs.flutter.dev/get-started/install/windows  
2. Baixe o **Flutter SDK** (ZIP do canal stable).  
3. Não use caminho com espaços; exemplo recomendado: `C:\src\flutter`.

#### Instalação

1. Extraia o ZIP para `C:\src\flutter` (ou outro caminho **sem espaços**).  
2. Adicione ao **PATH**:
   1. Tecla `Win` → pesquise **variáveis de ambiente** → **Editar as variáveis de ambiente do sistema**.  
   2. **Variáveis de Ambiente** → em **Path** (usuário ou sistema) → **Editar** → **Novo**.  
   3. Informe: `C:\src\flutter\bin`  
   4. Confirme com **OK** em todas as janelas.  
3. Feche e abra novamente o PowerShell ou o Prompt de Comando.

#### Conferir se está funcionando

```bash
flutter --version
flutter doctor
```

O `flutter doctor` lista o que falta (Android toolchain, licenças, etc.).

#### Licenças Android (depois de instalar o Android Studio)

```bash
flutter doctor --android-licenses
```

Digite `y` para aceitar cada licença.

---

### 6. Android Studio e emulador

IDE Android + emulador para rodar o app Flutter.

#### Download

1. Acesse: https://developer.android.com/studio  
2. Clique em **Download Android Studio**.  
3. Aceite os termos e baixe o instalador para Windows.

#### Instalação

1. Execute o instalador.  
2. Escolha **Standard** (instalação padrão).  
3. Aceite os componentes: Android SDK, Android Virtual Device, etc.  
4. Conclua e abra o Android Studio.  
5. No assistente inicial, deixe baixar os componentes do SDK.

#### Configurar SDK

1. No Android Studio: **More Actions** → **SDK Manager** (ou **Settings → Languages & Frameworks → Android SDK**).  
2. Aba **SDK Platforms**:
   - Marque pelo menos uma plataforma (ex.: **Android 14.0** / API 34).  
3. Aba **SDK Tools**:
   - Android SDK Build-Tools  
   - Android SDK Platform-Tools  
   - Android Emulator  
4. Clique em **Apply** / **OK** e aguarde o download.

#### PATH do `adb` (recomendado)

1. Caminho típico:  
   `C:\Users\SEU_USUARIO\AppData\Local\Android\Sdk\platform-tools`  
2. Adicione essa pasta ao **Path** do Windows (mesmo processo do Flutter).  
3. Teste em um terminal novo:

```bash
adb version
```

#### Criar um emulador (AVD)

1. No Android Studio, abra **Device Manager** (ícone de dispositivo).  
2. **Create Device**.  
3. Escolha um modelo (ex.: **Pixel 6**) → **Next**.  
4. Selecione uma **System Image** (ex.: API 34) → **Download** se necessário → **Next**.  
5. **Finish**.  
6. Clique em **Play** para iniciar o emulador.

#### Conferir com o Flutter

```bash
flutter doctor
flutter devices
```

Deve aparecer o emulador (ex.: `sdk gphone64` / `emulator-5554`) quando estiver ligado.

#### Uso com a API Valoriza (emulador)

Com a API em `http://0.0.0.0:5000`:

```bash
adb reverse tcp:5000 tcp:5000
```

No app (`lib/services/api_service.dart`):

```dart
static const String baseUrl = 'http://127.0.0.1:5000/api';
```

---

## Configurar o projeto

### 1. Obter o código

```bash
git clone https://github.com/SantosValentim/Valoriza.git
cd Valoriza
```

Ou abra a pasta do projeto já existente no disco.

### 2. Connection string e JWT

Edite:

- `backend/Valoriza.API/appsettings.json`
- `backend/Valoriza.Web/appsettings.json`

Exemplo (`Valoriza.API`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ValorizaDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-com-pelo-menos-32-caracteres!",
    "Issuer": "Valoriza.API",
    "Audience": "Valoriza.Client"
  },
  "SeedAdmin": {
    "Email": "admin@valoriza.com",
    "Senha": "Senha@123",
    "Nome": "Nome Completo"
  }
}
```

Use a **mesma** connection string no Web.

### 3. Ferramenta EF Core

```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
```

---

## Tutorial para rodar o projeto

Execute na ordem abaixo.

### Passo 1 — SQL Server em execução

1. `Win + R` → `services.msc`  
2. Confirme que **SQL Server** está **Em execução**.  
3. (Opcional) Conecte pelo SSMS em `localhost`.

### Passo 2 — Criar / atualizar o banco

```bash
cd backend/Valoriza.API
dotnet restore
dotnet ef database update
```

Se houver *pending model changes*:

```bash
dotnet ef migrations add SyncPendingModel
dotnet ef database update
```

### Passo 3 — Subir a API

```bash
cd backend/Valoriza.API
dotnet run --urls "http://0.0.0.0:5000"
```

- Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)  
- Deixe este terminal **aberto**

Na primeira execução, o seed cria:

- Papéis: `AdminValoriza`, `AdminEmpresa`, `GestorDEI`, `Colaborador`
- Empresa **Valoriza**
- Usuário administrador (`SeedAdmin` no `appsettings.json`)

**Login de teste (exemplo do seed):**

```text
E-mail: admin@valoriza.com
Senha:  Senha@123
```

### Passo 4 — Subir o painel Web

Em **outro terminal**:

```bash
cd backend/Valoriza.Web
dotnet restore
dotnet run
```

Abra a URL indicada no terminal (ex.: [http://localhost:5200](http://localhost:5200)).

> API e Web **não** devem usar a mesma porta. API em `5000`; Web em outra (ex.: `5200`).

### Passo 5 — Configurar o mobile

```bash
cd mobile/valoriza_app
flutter pub get
```

Em `lib/services/api_service.dart`:

```dart
// Com adb reverse (recomendado no emulador):
static const String baseUrl = 'http://127.0.0.1:5000/api';
```

No `android/app/src/main/AndroidManifest.xml`, dentro de `<application>`:

```xml
android:usesCleartextTraffic="true"
```

### Passo 6 — Emulador + porta da API

1. Inicie o emulador no Android Studio.  
2. Com a API rodando:

```bash
adb reverse tcp:5000 tcp:5000
```

### Passo 7 — Executar o app

```bash
cd mobile/valoriza_app
flutter devices
flutter run
```

Faça login com o mesmo usuário do seed/API.

### Ordem do dia a dia

| Ordem | Ação |
|-------|------|
| 1 | SQL Server em execução |
| 2 | API: `dotnet run --urls "http://0.0.0.0:5000"` |
| 3 | Conferir Swagger |
| 4 | Web: `dotnet run` (outra porta) |
| 5 | Emulador + `adb reverse tcp:5000 tcp:5000` |
| 6 | `flutter run` |

---

## Funcionalidades

### API REST

- Autenticação JWT e papéis  
- Trilhas, conteúdos, progresso e quiz  
- Denúncias (criar, listar, atualizar status)  
- Indicadores, mentorias e usuários  
- DTOs com validação e respostas padronizadas  
- Auditoria em criar/alterar status de denúncia  

### Painel Web

- Login Identity (cookie)  
- Dashboard com métricas DEI  
- Gestão de trilhas e conteúdos (Admin/Gestor)  
- Canal de denúncias com isolamento por empresa  
- Indicadores, mentorias, usuários e empresas  
- Churn e valor de assinatura por plano  
- Relatórios gerenciais (CSV)  
- Comunicação inclusiva (somente Gestor DEI)  
- Perfil e alteração de senha pelo próprio usuário  

### Mobile

- Login JWT  
- Dashboard (Admin/Gestor)  
- Treinamentos: abrir trilha, conteúdos, quiz e vídeo  
- Denúncias com fila **offline-first** e sincronização  
- Indicadores, mentorias e perfil  
- Sem telas de usuários, empresas ou churn  

---

## Segurança

| Controle               | Implementação                                          |
|------------------------|--------------------------------------------------------|
| HTTPS / HSTS           | API e Web (`UseHttpsRedirection`, `UseHsts`)           |
| CSRF                   | Web: `AutoValidateAntiforgeryToken` + tokens nos forms |
| Hash de senha          | Identity (PBKDF2)                                      |
| JWT                    | API e mobile                                           |
| Validação de entrada   | DataAnnotations + filtro global                        |
| Isolamento multiempresa| Filtros por `EmpresaId`                                |
| Auditoria              | Logs de denúncia (criar / status)                     |

---

## Migrações EF Core

Sempre a partir da pasta da API:

```bash
cd backend/Valoriza.API

# Nova migração
dotnet ef migrations add NomeDescritivo

# Aplicar no banco
dotnet ef database update

# Listar migrações
dotnet ef migrations list
```

---

## Problemas comuns

| Problema | Solução |
|----------|---------|
| SQL Server não conecta | Serviço “SQL Server” em execução; teste no SSMS |
| Tabelas não existem / `AspNetRoles` inválido | `dotnet ef database update` na API |
| Pending model changes | `dotnet ef migrations add SyncPendingModel` e `database update` |
| Porta em uso (API e Web) | API em 5000; Web em outra porta |
| Mobile: Network unreachable | API em `0.0.0.0:5000`, `http://` (não https), `adb reverse`, `usesCleartextTraffic` |
| `url_launcher` não encontrado | Incluir no `pubspec.yaml` e `flutter pub get` |
| Flutter: arquivo de tela não encontrado | Nome do arquivo = nome no `import` |
| `flutter doctor` com X no Android | Instalar SDK Platform e Platform-Tools no Android Studio |

---

## Scripts rápidos

```bash
# API
cd backend/Valoriza.API
dotnet restore
dotnet ef database update
dotnet run --urls "http://0.0.0.0:5000"

# Web
cd backend/Valoriza.Web
dotnet restore
dotnet run

# Mobile
adb reverse tcp:5000 tcp:5000
cd mobile/valoriza_app
flutter pub get
flutter run
```

---

## Licença e uso

Projeto acadêmico desenvolvido para o **PIM** da **UNIP**.  
Antes de qualquer ambiente real: altere senhas do seed, chave JWT e connection strings.

---

## Autor

**Oliver Valentim Carvalho Santos**  
RA **2632071**  
Universidade Paulista (UNIP)  
Projeto Integrador Multidisciplinar (PIM)  

Repositório: [https://github.com/SantosValentim/Valoriza.git](https://github.com/SantosValentim/Valoriza.git)
```
