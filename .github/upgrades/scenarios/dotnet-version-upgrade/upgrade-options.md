# Upgrade Options — qbookStudio.sln

Assessment: 3 projects across net472, net48, and net8.0-windows; 10 incompatible packages, 27k+ estimated LOC impact, 22k+ binary API issues, and 2 legacy non-SDK desktop projects.

## Strategy

### Upgrade Strategy
The solution crosses the .NET Framework to modern .NET boundary in multiple projects, so tier-by-tier validation is safer than a single atomic cutover.

| Value | Description |
|-------|-------------|
| **Bottom-Up** (selected) | Upgrade leaf-level dependencies first and validate each tier before moving upward. |

## Project Structure

### Project Approach
The .NET Framework projects are legacy desktop applications rather than ASP.NET web apps, so an in-place migration is the most direct fit for this solution.

| Value | Description |
|-------|-------------|
| **In-place** (selected) | Replace the current target framework directly during the upgrade work. |
| Multi-targeting | Keep old and new target frameworks side by side temporarily where shared libraries need to serve both worlds. |

### Package Management
The solution mixes old-style projects and packages.config with a framework-boundary migration, so central package management would add avoidable churn during the active upgrade.

| Value | Description |
|-------|-------------|
| Central Package Management (CPM) | Add Directory.Packages.props and centralize package versions now. |
| **Per-Project (defer CPM to post-migration)** (selected) | Keep package versions per project during the migration and revisit CPM after the solution is stabilized. |

## Compatibility

### Unsupported Packages
The assessment found 10 incompatible packages, which is too many to force through inline replacement while also preserving bottom-up validation.

| Value | Description |
|-------|-------------|
| Resolve Inline | Research and replace each incompatible package inside the same task that encounters it. |
| **Defer Resolution** (selected) | Keep projects compiling with temporary isolation or stubs where needed, then resolve harder package replacements in follow-up work. |
| Compatibility Mode | Keep framework-era references temporarily and suppress compatibility warnings where direct API usage is not required. |

### Unsupported API Handling
The assessment reports 22,020 binary-incompatible and 5,008 source-incompatible API findings across multiple projects, so complex API migrations should be split from straightforward mechanical fixes.

| Value | Description |
|-------|-------------|
| Fix Inline | Resolve all API changes, including complex ones, within the main upgrade task. |
| **Defer Complex Changes** (selected) | Apply simple replacements inline and defer harder API migrations behind temporary compiling stubs and follow-up tasks. |

### System.Web Adapters
The assessment surfaced a small amount of System.Web usage, but not a large ASP.NET Framework web surface that would justify carrying adapter shims.

| Value | Description |
|-------|-------------|
| Use System.Web Adapters | Add adapter shims to support incremental migration of legacy System.Web semantics. |
| **Direct Migration to ASP.NET Core APIs** (selected) | Replace the small amount of System.Web usage directly without adding an adapter layer. |

## Modernization

### Logging Framework
The solution still uses log4net, which is flagged with a security vulnerability and does not align well with the modern .NET hosting and logging stack.

| Value | Description |
|-------|-------------|
| **Migrate to Microsoft.Extensions.Logging** (selected) | Replace legacy logging usage with the built-in logging abstraction used by modern .NET. |
| Keep Existing Logging Framework | Retain the current logging framework and add adapters where needed. |

### Dependency Injection
Autofac is present in the legacy projects, and there is no surfaced evidence yet that the solution depends heavily on advanced Autofac-only features.

| Value | Description |
|-------|-------------|
| **Migrate to Built-in DI Container** (selected) | Move registrations to Microsoft.Extensions.DependencyInjection and align with the default .NET hosting model. |
| Keep Existing IoC Container | Keep Autofac and integrate it into the upgraded solution. |

### Assembly Binding Redirects
The assessment found 220 binding-related issues across two legacy projects, which strongly suggests the redirects should be inventoried before bulk removal.

| Value | Description |
|-------|-------------|
| Remove Binding Redirects | Remove all binding redirects directly during project cleanup. |
| **Document and Review Before Removing** (selected) | Inventory redirects first, then remove them with context once underlying version conflicts are understood. |

### Entity Framework
EF6 6.5.x is already present and can run on modern .NET, so combining an EF Core migration with the framework upgrade would add avoidable risk.

| Value | Description |
|-------|-------------|
| **Keep EF6** (selected) | Keep EF6 during the .NET upgrade and evaluate EF Core as a later follow-on modernization step. |
| Migrate to EF Core | Upgrade the data layer to EF Core as part of the same migration. |
