# Notas de setup del entorno de desarrollo — facturador-by-rc

## Docker Desktop (Windows) — instalación en unidad D:

Docker Desktop es prerequisito para el PostgreSQL local del proyecto
(via `docker-compose.yml`). **No es necesario instalar PostgreSQL en Windows**:
el contenedor es la base de datos local.

### Instalación del programa en D: (~2-3 GB)

El instalador gráfico no permite elegir ruta; hacerlo por PowerShell (como
administrador), desde la carpeta del instalador descargado:

```powershell
Start-Process -Wait "Docker Desktop Installer.exe" -ArgumentList "install","--installation-dir=D:\Docker"
```

Alternativa simple: instalar por defecto en C: y mover solo los datos (paso
siguiente), que es donde va el consumo real de disco.

### Mover los datos a D: (imágenes/contenedores/volúmenes — decenas de GB)

Después de instalar:

1. Docker Desktop → **Settings → Resources → Advanced**
2. **Disk image location** → `D:\Docker\wsl-data`
3. Aplicar — Docker migra el disco virtual WSL2 automáticamente.

(Por defecto queda en `C:\Users\<usuario>\AppData\Local\Docker\wsl`.)

### Requisitos y verificación

- Requiere WSL2 (el instalador suele configurarlo; si no: `wsl --install` + reinicio)
- Tras instalar: cerrar sesión/reiniciar, abrir Docker Desktop una vez, y verificar:

```bash
docker --version
docker compose version
```

### Uso en este proyecto

```bash
docker compose up -d      # levanta PostgreSQL local en localhost:5433
docker compose down       # lo apaga (los datos persisten en el volumen)
```

Puerto 5433 (no 5432) porque el 5432 ya lo ocupa otro proyecto (`sad_ganadero`)
corriendo en la misma máquina.

Connection string de desarrollo por variable de entorno `DATABASE_URL`
(fallback en `appsettings.Development.json` → `ConnectionStrings:Default`).
Para inspeccionar la base: pgAdmin o DBeaver conectados a `localhost:5433`.

## Claude Code en Windows — nota de terminal

En Git Bash (mintty) el comando `claude` falla con
`Error: Input must be provided either through stdin or as a prompt argument`
porque mintty no provee TTY a ejecutables nativos → Claude Code cae en modo
`--print`.

**Soluciones:**

- Usar PowerShell / Windows Terminal (funciona directo) ✅ (opción en uso)
- En Git Bash: `winpty claude.cmd`
- Recomendado a futuro: perfil de Git Bash dentro de Windows Terminal
  (ConPTY, sin winpty): Settings → Add new profile →
  `C:\Program Files\Git\bin\bash.exe -i -l`

## Estado del entorno (2026-07-08)

- [x] Solución .NET 8 compilando, 5/5 tests en verde
- [x] Repo público: https://github.com/rcsoliz/facturador-by-rc
- [x] CLAUDE.md con restricción NIT/mocks commiteada
- [x] Docker Desktop instalado y verificado
- [x] Persistencia EF Core + Npgsql: DbContext, configuraciones, migración inicial,
      correlativo atómico por punto de venta, 8/8 tests en verde (incluye
      integración real contra Postgres vía Testcontainers)
