[简体中文](README.zh.md) | [**English**](README.md)

<p align="center">
  <img src="Fast.png" width="160" alt="Fast.Admin Logo" />
</p>

<h1 align="center">Fast.Admin</h1>

<p align="center">
  A modern open-source administration system built on Fast.NET
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Vue-3.5-42B883?logo=vuedotjs" alt="Vue 3.5" />
  <img src="https://img.shields.io/badge/TypeScript-6.0-3178C6?logo=typescript" alt="TypeScript 6" />
  <img src="https://img.shields.io/badge/Vite-8.0-646CFF?logo=vite" alt="Vite 8" />
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-Apache--2.0-blue.svg" alt="Apache-2.0 License" /></a>
</p>

Fast.Admin is a separated frontend/backend open-source project for enterprise administration scenarios. It uses [Fast.NET](https://gitee.com/FastDotnet/Fast.NET) as its server-side infrastructure and provides organization and permission management, system configuration, operations monitoring, job scheduling, and common business administration capabilities. Its modular repository includes a Web console, a mobile administration console, server applications, and a separate user-facing client project.

> **Current maturity:** The core administration features of `Web.Admin`, `Api.Server`, and `App.Admin` are complete. `App.Admin` provides H5 and WeChat Mini Program interfaces; its notification page remains planned. The user-facing `App.Client` is still under development. Native app packaging and device compatibility require separate validation for each target platform.
>
> **Important disclaimer:** This project may only be used for lawful, compliant, and duly authorized purposes. It must not be used for illegal or criminal activity. Users are solely responsible for the compliance of deployment, configuration, data processing, derivative development, and operations. Read [Disclaimer and compliance requirements](#disclaimer-and-compliance-requirements) before use.

## Why Fast.Admin

- **Separated frontend and backend**: ASP.NET Core on the server and Vue 3, TypeScript, Vite, and Element Plus for the Web console.
- **Built on Fast.NET**: Reuses caching, dependency injection, JWT, logging, dynamic APIs, OpenAPI, serialization, SqlSugar, Swagger, and unified-result infrastructure.
- **Administration fundamentals**: Covers tenants, organizations, departments, employees, positions, roles, applications, menus, APIs, settings, dictionaries, and database administration.
- **Operational governance**: Includes API rate limiting, access and operation logs, exception and SQL logs, online users, system monitoring, SignalR, and Quartz administration.
- **Web and mobile administration**: Both consoles share business APIs and permissions. The mobile console includes a workbench, search and filters, detail views, forms, role authorization, and light/dark themes.
- **Engineering safeguards**: Centralized .NET dependency management plus frontend type checking, ESLint, Prettier, and production build workflows.

## Project status

| Directory                  | Purpose                       | Status                     | Notes                                                                         |
| -------------------------- | ----------------------------- | -------------------------- | ----------------------------------------------------------------------------- |
| [`Web.Admin`](Web.Admin)   | Web administration console    | ✅ Primary validated scope | Main administration UI currently available                                    |
| [`Api.Server`](Api.Server) | .NET server                   | ✅ Primary validated scope | Contains API, file, and scheduler hosts plus domain and application services  |
| [`App.Admin`](App.Admin)   | Mobile administration console | ✅ Core features complete  | uni-app with H5 and WeChat Mini Program interfaces; notification page planned |
| [`App.Client`](App.Client) | Mobile client app             | 🚧 In development          | Built with uni-app; features and platform support are incomplete              |

These labels describe the current maintenance and validation scope only. They do not guarantee coverage of every operating system, database, browser, device, or production deployment combination.

## Development Roadmap

> “Completed” means that the feature is included in the current administration scope. It does not mean that every platform, production environment, third-party integration, or edge case has been validated. Planned items have no committed release or delivery date, and their priority and scope may change with actual requirements.

### Completed

- [x] Separated frontend/backend foundation built with .NET 10, ASP.NET Core, Fast.NET, and Vue 3.
- [x] Foundational tenant, application, database-configuration, account, and multi-organization management.
- [x] Organization, department, employee, position, job-level, role, menu, and API permission management.
- [x] System settings, data dictionaries, serial-number rules, table configuration, and password-record management.
- [x] Login CAPTCHA, password recovery, account identity verification, session management, and forced sign-out.
- [x] Visit, request, operation, exception, and SQL logs, plus online-user and system monitoring.
- [x] Separate Quartz 3 scheduler service, scheduler administration UI, and local/URL job support.
- [x] Separate file service and file-record administration.
- [x] OpenAPI, Swagger, Knife4j, and API metadata administration.
- [x] Common business modules for merchants, client users, payment records, refund records, complaint handling, and message-delivery records.
- [x] A uni-app mobile administration console with H5 and WeChat Mini Program interfaces, a home page, workbench, profile, and light/dark themes.
- [x] Mobile search, filters, pagination, detail views, forms, role authorization, and application-package URL display and copying.
- [x] Mobile password recovery, password changes including WeChat authorization, and WebSocket session connections.
- [x] Device- and menu-based switching between Web and H5, preserving the destination through login and tenant selection.

### Incomplete / Planned

- [ ] **Quartz 4 upgrade**: Upgrade dependencies and validate database schema, persistence, serialization, and existing job compatibility.
- [ ] **Notifications and message center**: Complete the `App.Admin` notification page, and extend the existing SMS, email, templates, and delivery records with in-app messages, inboxes, unread state, receipts, push delivery, and channel management.
- [ ] **Online chat**: Extend the SignalR foundation with direct and group conversations, conversation lists, message persistence, read receipts, and offline messages.
- [ ] **Workflow engine**: Add visual process design, definitions and versions, process instances, tasks, approvals, countersigning, transfers, carbon copies, reminders, and audit records.
- [ ] **Client app**: Continue completing `App.Client` user-facing business capabilities and multi-platform support.
- [ ] **Single sign-on and external identity providers**: Evaluate and integrate OIDC, OAuth 2.0, WeCom, DingTalk, and similar authentication methods.
- [ ] **Code generation and low-code configuration**: Gradually add generation for models, APIs, forms, lists, and foundational business code.
- [ ] **Automated quality assurance**: Improve unit, integration, and end-to-end tests, CI, and release checks.
- [ ] **Deployment and observability**: Improve Docker, container orchestration, health checks, metrics, distributed tracing, and alert integrations.
- [ ] **Internationalization and accessibility**: Add multilingual content, time-zone and regional formatting, and accessibility support.
- [ ] **Compatibility and documentation**: Continue improving the database compatibility matrix, deployment documentation, development examples, and public demonstration environment.

## Interface Preview

### Web administration

> Click a thumbnail to view the full-size image. Some screenshots may come from production systems created through secondary development of this project and are provided only to demonstrate the interface. Business data and sensitive information must be redacted before publication, and the screens may differ from the open-source version.

<table>
  <tr>
    <td align="center"><img src="docs/images/web-admin-login.png" width="140" alt="Web.Admin login" /><br />Login</td>
    <td align="center"><img src="docs/images/web-admin-data-dashboard.png" width="140" alt="Web.Admin data dashboard" /><br />Data Dashboard</td>
    <td align="center"><img src="docs/images/web-admin-dashboard.png" width="140" alt="Web.Admin dashboard" /><br />Dashboard</td>
    <td align="center"><img src="docs/images/web-admin-system-monitor.png" width="140" alt="Web.Admin system monitor" /><br />System Monitor</td>
    <td align="center"><img src="docs/images/web-admin-data-dictionary.png" width="140" alt="Web.Admin data dictionary" /><br />Data Dictionary</td>
  </tr>
  <tr>
    <td align="center"><img src="docs/images/web-admin-table-configuration.png" width="140" alt="Web.Admin table configuration" /><br />Table Configuration</td>
    <td align="center"><img src="docs/images/web-admin-menu-management.png" width="140" alt="Web.Admin menu management" /><br />Menu Management</td>
    <td align="center"><img src="docs/images/web-admin-scheduled-jobs.png" width="140" alt="Web.Admin scheduled jobs" /><br />Scheduled Jobs</td>
    <td align="center"><img src="docs/images/web-admin-role-management.png" width="140" alt="Web.Admin role management" /><br />Role Management</td>
    <td align="center"><img src="docs/images/web-admin-department-management.png" width="140" alt="Web.Admin department management" /><br />Department Management</td>
  </tr>
</table>

### Mobile administration (WeChat Mini Program)

These `App.Admin` screenshots were captured in WeChat DevTools using the iPhone 15 Pro Max simulator (430 × 932), all in dark mode. They show login, the workbench, system monitoring, the scheduler, roles, and departments. Click an image to view it at full size.

<table>
  <tr>
    <td align="center"><a href="docs/images/app-admin-mini-login.jpg"><img src="docs/images/app-admin-mini-login.jpg" width="200" alt="WeChat Mini Program login in dark mode on iPhone 15 Pro Max" /></a><br />Login</td>
    <td align="center"><a href="docs/images/app-admin-mini-workbench.jpg"><img src="docs/images/app-admin-mini-workbench.jpg" width="200" alt="WeChat Mini Program workbench in dark mode on iPhone 15 Pro Max" /></a><br />Workbench</td>
    <td align="center"><a href="docs/images/app-admin-mini-system-monitor.jpg"><img src="docs/images/app-admin-mini-system-monitor.jpg" width="200" alt="WeChat Mini Program system monitoring in dark mode on iPhone 15 Pro Max" /></a><br />System Monitoring</td>
  </tr>
  <tr>
    <td align="center"><a href="docs/images/app-admin-mini-scheduler.jpg"><img src="docs/images/app-admin-mini-scheduler.jpg" width="200" alt="WeChat Mini Program scheduler in dark mode on iPhone 15 Pro Max" /></a><br />Scheduler</td>
    <td align="center"><a href="docs/images/app-admin-mini-roles.jpg"><img src="docs/images/app-admin-mini-roles.jpg" width="200" alt="WeChat Mini Program role management in dark mode on iPhone 15 Pro Max" /></a><br />Roles</td>
    <td align="center"><a href="docs/images/app-admin-mini-department.jpg"><img src="docs/images/app-admin-mini-department.jpg" width="200" alt="WeChat Mini Program department management in dark mode on iPhone 15 Pro Max" /></a><br />Departments</td>
  </tr>
</table>

## Core capabilities

- Tenant, account, organization, department, employee, position, job-level, and role management.
- Application, menu, API, dictionary, configuration, database, serial-number, and table configuration management.
- Login, visit, request, operation, exception, and SQL execution logs.
- Online users, system monitoring, API rate limiting, JWT authentication, and SignalR communication.
- Quartz job administration, a separate file service, and OpenAPI/Swagger documentation.
- Common business modules for merchants, client users, payments, refunds, complaints, and message delivery records.
- H5 and WeChat Mini Program administration, a mobile workbench, account security, role authorization, light/dark themes, and cross-client login redirects.

Exact features, authorization boundaries, and third-party service availability depend on the current source, configuration, and deployment environment.

## Technology and compatibility

| Area         | Current stack                                                          |
| ------------ | ---------------------------------------------------------------------- |
| Server       | .NET 10, ASP.NET Core, Fast.NET, SqlSugar, Redis, JWT, SignalR, Quartz |
| Web console  | Vue 3.5, TypeScript 6, Vite 8, Element Plus, Pinia, Axios, ECharts     |
| Mobile admin | uni-app, Vue 3.5, TypeScript 6, Vite 5, Wot UI 2, Pinia                |
| User client  | uni-app, Vue 3.4, TypeScript 6, Vite 5, Wot UI 2 (in development)      |
| Node.js      | `^24.18.0`                                                             |
| pnpm         | `^11.0.0`                                                              |
| License      | Apache-2.0                                                             |

The server currently targets `net10.0`. Before deployment, make sure the installed SDK, Node.js, pnpm, database, and Redis environment match the project configuration.

## Architecture

```mermaid
flowchart TB
    user["Administrators / business users"]

    subgraph clients["Clients"]
        web["Web.Admin · primary validated scope"]
        adminApp["App.Admin · H5 / WeChat Mini Program"]
        clientApp["App.Client · in development"]
    end

    subgraph server["Api.Server · .NET 10"]
        api["Fast.Api · API host"]
        file["Fast.File · file host"]
        scheduler["Fast.Scheduler · scheduler host"]
        services["Admin / Center / Scheduler Services"]
        foundation["Domain / Core / Shared"]
    end

    infra["Fast.NET infrastructure"]
    storage["Databases / Redis / external services"]

    user --> web
    user --> adminApp
    user -. incomplete .-> clientApp
    web --> api
    adminApp --> api
    clientApp -.-> api
    api --> services
    file --> foundation
    scheduler --> services
    services --> foundation
    foundation --> infra
    services --> storage
```

## Repository layout

```text
Fast.Admin/
├─ Api.Server/                 # .NET 10 server solution
│  ├─ src/Fast.Api/            # Main API host, default port 38081
│  ├─ src/Fast.File/           # File host, default port 38082
│  ├─ src/Fast.Scheduler/      # Scheduler host, default port 38083
│  ├─ src/*.Service/           # Application services
│  ├─ src/*.Domain/            # Domain models
│  └─ src/Core, src/Shared/    # Core and shared infrastructure
├─ Web.Admin/                  # Vue 3 Web console, default port 2001
├─ App.Admin/                  # uni-app mobile admin, H5 default port 2010
├─ App.Client/                 # uni-app mobile client app, in development
├─ Sql/                        # Database scripts
├─ README.zh.md / README.md    # Chinese and English project documentation
└─ LICENSE                     # Apache-2.0 license
```

## Quick start

### 1. Prerequisites

- .NET SDK 10.0
- Node.js 24.18 or a compatible version
- pnpm 11
- A database and Redis instance matching the server configuration

### 2. Configure the server

Review and configure these files for the target environment:

- `Api.Server/src/Core/coresettings.json`
- `Api.Server/src/Core/coresettings.Development.json`
- `Api.Server/src/Core/dbsettings.json`
- `Api.Server/src/Core/dbsettings.Development.json`

Production deployments must replace default or development database, Redis, JWT, and CORS settings. Never commit real passwords, secrets, tokens, or connection details.

### 3. Start the main API

```bash
cd Api.Server
dotnet restore Fast.Admin.sln
dotnet run --project src/Fast.Api/Fast.Api.csproj
```

The main API listens on `http://127.0.0.1:38081` by default. Start the file and scheduler hosts separately when needed:

```bash
dotnet run --project src/Fast.File/Fast.File.csproj
dotnet run --project src/Fast.Scheduler/Fast.Scheduler.csproj
```

### 4. Start the Web console

```bash
cd Web.Admin
pnpm install --frozen-lockfile
pnpm dev
```

The development server is available at `http://127.0.0.1:2001` by default and proxies `/api` to the main API. Make sure the proxy in `Web.Admin/.env.development` matches the server address.

### 5. Start the mobile administration console

```bash
cd App.Admin
pnpm install --frozen-lockfile
pnpm dev:h5
```

H5 is available at `http://127.0.0.1:2010` by default. Set `VITE_API_REQUEST_URL` in `App.Admin/.env.development` to the actual API address, using the same server as the Web console.

For WeChat Mini Program development:

```bash
cd App.Admin
pnpm dev:mp-weixin
```

Import `App.Admin/dist/dev/mp-weixin` into WeChat DevTools and confirm the project AppID, base-library version, and local API access settings before running it. `App.Client` is a separate user-facing client project and is not part of this administration-console startup flow.

## Local build

Server:

```bash
cd Api.Server
dotnet restore Fast.Admin.sln
dotnet build Fast.Admin.sln -c Release --no-restore
```

Web console:

```bash
cd Web.Admin
pnpm install --frozen-lockfile
pnpm build
```

Mobile administration console:

```bash
cd App.Admin
pnpm install --frozen-lockfile
pnpm build:h5
pnpm build:mp-weixin
```

The outputs are `App.Admin/dist/build/h5` and `App.Admin/dist/build/mp-weixin`. Use `pnpm build:app` for native app resources; package signing, packaging, and device validation must be completed for the target platform.

A successful build only confirms the current project build workflow. It does not mean that a production environment, database, cache, SMS, email, payment, or other third-party integration has been validated.

## Branches

| Branch    | Purpose          | Recommendation                                                        |
| --------- | ---------------- | --------------------------------------------------------------------- |
| `master`  | Stable branch    | Prefer this branch for forks, learning, and production evaluation     |
| `develop` | Iteration branch | Contains work in progress and must be tested independently before use |

## Documentation and collaboration

- [Nginx reverse proxy deployment template](docs/DEPLOYMENT.md)
- [Fast.NET](https://gitee.com/FastDotnet/Fast.NET)
- [Commit history](https://gitee.com/FastDotnet/Fast.Admin/commits/master)
- [Issue tracker](https://gitee.com/FastDotnet/Fast.Admin/issues)
- [Pull requests](https://gitee.com/FastDotnet/Fast.Admin/pulls)

Before submitting changes, complete at least the relevant build, type check, or tests for the affected project, and keep public Chinese and English documentation synchronized. Issues and pull requests are welcome.

## Disclaimer and compliance requirements

> **Do not use this project for any activity that violates the laws of the People's Republic of China, the laws applicable where it is used, or the lawful rights of any third party.**

1. This project is provided only for lawful learning, research, internal administration, and duly authorized business scenarios. It must not be used for cyberattacks, unauthorized access, fraud, gambling, money laundering, privacy violations, unlawful collection or trading of data, evasion of regulation, distribution of illegal content, or any other unlawful or criminal purpose.
2. Users must independently ensure that their use, deployment, derivative development, data collection and processing, content operations, API calls, and external services have all required rights, authorization, qualifications, licenses, and security controls. Users are solely responsible for the resulting legal obligations and liabilities.
3. When processing personal information, important data, accounts and permissions, payments, SMS, email, files, logs, or third-party platforms, users must comply with applicable data protection, cybersecurity, consumer protection, and industry requirements, including appropriate notice, consent, minimization, encryption, auditing, backup, and access control.
4. The software is provided on an "AS IS" basis, without promises that it is defect-free, uninterrupted, completely secure, or suitable for a particular purpose. To the extent permitted by applicable law, the authors and contributors are not liable for loss, disputes, or obligations resulting from use, inability to use, misconfiguration, derivative development, data breaches, data loss, business interruption, third-party service failures, or unlawful use.
5. Example configuration, initialization data, scripts, and third-party integrations in this repository are for development reference only. Before production use, conduct an independent security review, compliance assessment, load test, backup and recovery validation, and any necessary professional review.
6. This notice is not legal advice and does not replace applicable law, regulatory requirements, or professional legal counsel. Use, copying, modification, and distribution are also subject to the [Apache License 2.0](LICENSE) and relevant third-party licenses.

By downloading, using, or creating a derivative work from this project, the user acknowledges the risks and responsibilities above. If you do not agree, stop using the project and remove all related copies.

## License

Fast.Admin is open source under the [Apache License 2.0](LICENSE). Use, modification, and distribution must comply with the license, applicable third-party licenses, and applicable law.

## Maintainer

Created and maintained by **XiaoFang (1.8K 仔)**. Fast.Admin continues to bring together practical ideas and infrastructure to offer the .NET open-source ecosystem a modern administration-system option.
