# Claude Code - Base Rules & Project Guidelines

**Project:** HouseLedger
**Created:** October 19, 2025
**Last Updated:** October 19, 2025

---

## 📋 Base Rules

### Documentation Management

**Rule 1: Single Development Plan**
- All development planning MUST be consolidated in ONE file: `development-plan.md`
- The file MUST have a summary table at the top showing:
  - Phase/Task name
  - Status (Not Started / In Progress / Completed)
  - Start Date
  - Completion Date
  - Notes

**Rule 2: Completed Implementation Archive**
- All completed implementation details MUST be moved to: `development-plan-done.md`
- This keeps the main plan file focused on current/future work
- Archive includes:
  - What was built
  - Key decisions made
  - Code samples (if relevant)
  - Problems encountered and solutions

**Rule 3: Delete Redundant Documentation**
- Once consolidated into `development-plan.md` and `development-plan-done.md`:
  - DELETE old plan files to avoid confusion
  - Keep only: architecture docs, mindset docs, and the two plan files

---

## 🖥️ Deployment Target: QNAP TS-231P NAS

### Hardware Specifications

**CPU:**
- Model: AnnapurnaLabs Alpine AL-212
- Architecture: ARM Cortex-A15 (32-bit ARM)
- Cores: Dual-core
- Clock Speed: 1.7 GHz

**Memory:**
- RAM: 1GB DDR3
- Flash: 512 MB (dual boot OS protection)

**Storage:**
- 2-bay NAS (user-provided drives)

**Network:**
- Dual Gigabit Ethernet ports
- 3x USB 3.2 Gen 1 ports

**Performance:**
- Read: Up to 224 MB/s
- Write: Up to 176 MB/s
- Hardware AES 256-bit encryption: 179+ MB/s

### Software Capabilities

**Operating System:**
- QNAP QTS (Linux-based)

**Container Support:**
- ✅ **Docker support via Container Station**
- ✅ **LXC (Linux Containers) support**
- ✅ Container import/export
- ✅ Permission settings
- ✅ Docker Hub Registry access

**Virtualization:**
- Container Station integrates LXC and Docker
- Can run multiple isolated Linux systems

### .NET Deployment Implications

#### Docker Image Requirements
**Critical:** Must use ARM32-compatible Docker images

```dockerfile
# ❌ WRONG - x64/AMD64 images won't work
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# ✅ CORRECT - ARM32 Linux images
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-arm32v7
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy-arm32v7
```

#### Build Considerations

**Option 1: Build on NAS (Slow)**
- 1GB RAM and ARM32 will make builds slow
- Not recommended for development

**Option 2: Cross-compile (Recommended)**
- Build on development machine (x64)
- Publish for `linux-arm` runtime
- Deploy compiled artifacts to NAS

```bash
# Publish for ARM32 Linux
dotnet publish -c Release -r linux-arm --self-contained false
```

**Option 3: Multi-arch Docker Build**
- Use Docker buildx for multi-architecture images
- Build on x64 machine, push ARM32 image to registry
- Pull on NAS

```bash
docker buildx build --platform linux/arm/v7 -t housledger:latest .
```

#### Memory Constraints

**1GB RAM Considerations:**
- Keep container memory limits reasonable (256-512MB per service)
- Monitor memory usage carefully
- Consider running as monolith rather than multiple microservices
- Use lightweight base images (Alpine when possible)

#### Database

**SQLite is PERFECT for this deployment:**
- ✅ No separate database server needed (saves memory)
- ✅ File-based (works great on NAS)
- ✅ Low memory footprint
- ✅ No network overhead
- ✅ ARM32 compatible

**Storage Path:**
- Store database files on NAS storage (e.g., `/share/HouseLedger/data/`)
- Use Docker volume mounts for persistence

### Deployment Strategy

#### Recommended Approach: Docker Compose (Monolith)

**Why Monolith:**
- Limited RAM (1GB) makes microservices impractical
- Single process easier to manage
- Lower memory overhead
- Simpler deployment

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  housledger:
    image: housledger:arm32v7
    container_name: housledger
    restart: unless-stopped
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/data/housledger.db
    volumes:
      - /share/HouseLedger/data:/data
      - /share/HouseLedger/logs:/app/logs
      - /share/HouseLedger/bills:/app/bills
    mem_limit: 512m
    memswap_limit: 512m
```

#### Performance Optimization

**For ARM32 + 1GB RAM:**
1. Use minimal Docker base images (Alpine Linux)
2. Disable unnecessary ASP.NET Core features
3. Use AOT compilation if possible (.NET 8+)
4. Monitor memory usage and adjust limits
5. Consider running without Swagger in production
6. Use efficient logging (Serilog to file, not console)

#### CI/CD Pipeline Consideration

**Build Pipeline:**
1. Developer commits code (x64 machine)
2. GitHub Actions builds for linux-arm
3. Create Docker image for ARM32
4. Push to Docker registry (Docker Hub, GitHub Registry)
5. NAS pulls image and restarts container

**Alternative (Direct Deploy):**
1. Build on dev machine: `dotnet publish -r linux-arm`
2. SCP compiled files to NAS
3. Run with existing .NET runtime on NAS

---

## 🏗️ Architecture Decisions (Based on NAS Constraints)

### ADR-007: Monolith Deployment for NAS
**Context:** QNAP TS-231P has only 1GB RAM, ARM32 CPU
**Decision:** Deploy as single monolith application, not microservices
**Rationale:**
- Microservices require separate containers (high memory overhead)
- 1GB RAM insufficient for multiple services + database containers
- Monolith uses ~256-512MB, leaving room for OS and other NAS functions
**Status:** Approved

### ADR-008: SQLite for Data Persistence
**Context:** Limited memory and ARM32 architecture
**Decision:** Use SQLite file-based database
**Rationale:**
- No separate database server (saves ~100-200MB RAM)
- Perfect for NAS storage
- ARM32 compatible
- Proven performance for single-user/small-team applications
**Status:** Approved

### ADR-009: ARM32 Docker Images
**Context:** QNAP TS-231P is ARM Cortex-A15 (32-bit)
**Decision:** Use ARM32v7 Docker base images
**Rationale:**
- x64/AMD64 images will not run
- Must use `mcr.microsoft.com/dotnet/aspnet:8.0-jammy-arm32v7`
**Status:** Approved

### ADR-010: Cross-Compilation Strategy
**Context:** Building on ARM32 with 1GB RAM is slow
**Decision:** Build on x64 development machine, publish for linux-arm target
**Rationale:**
- Faster build times
- Better developer experience
- Only deploy compiled artifacts to NAS
**Status:** Approved

---

## 📦 Deployment Checklist

Before deploying to QNAP TS-231P:

- [ ] Verify Docker image is ARM32v7 compatible
- [ ] Test image on ARM32 environment (Raspberry Pi, QEMU)
- [ ] Set memory limits (512MB max per container)
- [ ] Configure volume mounts for data persistence
- [ ] Set up log rotation (limited NAS storage)
- [ ] Test database access on NAS storage path
- [ ] Configure restart policy (unless-stopped)
- [ ] Document port mappings
- [ ] Set up backup strategy for SQLite database
- [ ] Test performance under load
- [ ] Monitor memory usage (stay under 512MB)
- [ ] Configure QNAP Container Station properly
- [ ] Set up auto-start on NAS boot

---

## 🔧 Development Workflow

### Local Development (x64 Machine)
```bash
# Standard development
dotnet run --project src/Services/HouseLedger.Services.Finance.Api

# Test with Docker (x64)
docker-compose up
```

### Build for ARM32 Deployment
```bash
# Publish for linux-arm
dotnet publish -c Release -r linux-arm --self-contained false -o ./publish/arm32

# Build Docker image for ARM32
docker buildx build --platform linux/arm/v7 -t housledger:arm32v7 .

# Or use docker-compose with buildx
docker buildx bake --set *.platform=linux/arm/v7 -f docker-compose.yml
```

### Deploy to NAS
```bash
# Option 1: Copy published files via SCP
scp -r ./publish/arm32/* admin@nas:/share/HouseLedger/app/

# Option 2: Push Docker image
docker push yourdockerhub/housledger:arm32v7

# On NAS (via SSH or Container Station):
docker pull yourdockerhub/housledger:arm32v7
docker-compose up -d
```

---

## 📊 Monitoring & Maintenance

### Check Container Stats (on NAS)
```bash
docker stats housledger
```

### Expected Resource Usage:
- **Memory:** 256-512 MB (max)
- **CPU:** 10-30% (idle), 50-80% (under load)
- **Disk I/O:** Moderate (SQLite reads/writes)

### Log Files:
- **Location:** `/share/HouseLedger/logs/`
- **Rotation:** 30 days (configured in Serilog)
- **Format:** JSON (for analysis)

### Database Backup:
```bash
# Copy SQLite database file
cp /share/HouseLedger/data/housledger.db /share/HouseLedger/backups/housledger-$(date +%Y%m%d).db
```

---

## 🎯 Key Reminders

1. **Always use ARM32 images** - x64 will not work
2. **Memory is limited** - Keep total usage under 512MB
3. **SQLite is your friend** - No separate database server needed
4. **Build on x64, deploy to ARM32** - Cross-compile for speed
5. **Monolith, not microservices** - RAM constraints require efficiency
6. **Test on ARM32 before deploying** - Use Raspberry Pi or QEMU
7. **Monitor memory usage** - NAS has only 1GB total
8. **Use Container Station** - QNAP's Docker interface

---

**References:**
- QNAP TS-231P Hardware Specs: https://www.qnap.com/en/product/ts-231p/specs/hardware
- QNAP TS-231P Software Specs: https://www.qnap.com/en/product/ts-231p/specs/software
- .NET ARM32 Support: https://github.com/dotnet/runtime/blob/main/docs/design/features/arm32-support.md
- Docker ARM32 Images: https://hub.docker.com/u/arm32v7
