# Content Refresh System - Testing Guide

## ✅ Automated Tests Passed

All unit and integration tests have been verified:
- ✅ Compilation: 0 errors, 0 warnings
- ✅ Hangfire integration: In-memory storage working
- ✅ Service registration: DI configured correctly
- ✅ Daily job scheduled: 2:00 AM UTC
- ✅ Exercise generator: All features implemented

---

## 🧪 Manual Testing (Local)

### Step 1: Start the API

```bash
cd src/EA.API
dotnet run
```

Expected output:
```
[HH:MM:SS INF] Daily content refresh job scheduled for 2:00 AM UTC
[HH:MM:SS INF] Starting Hangfire Server using job storage: 'In-Memory Storage'
[HH:MM:SS INF] Server [...] successfully announced
[HH:MM:SS INF] All the dispatchers started
```

### Step 2: Verify Database Connection

The migration `AddContentVersioning` will auto-run on startup via `DataSeeder.SeedAsync()`.

Check that the `ContentVersions` table was created:

```sql
-- In SQL Server Management Studio
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'ContentVersions';
```

Expected result:
```
ContentVersions
```

### Step 3: Test Exercise Generation (Manual)

To manually trigger the content refresh job in a test environment:

1. Add this endpoint temporarily to `LessonsController.cs`:

```csharp
[HttpPost("test-refresh")]
[Authorize]
public async Task<IActionResult> TestRefresh([FromServices] ContentRefreshService service)
{
    await service.RefreshDailyContentAsync();
    return Ok(new { message = "Content refresh job executed" });
}
```

2. Call it:
```bash
curl -X POST http://localhost:5225/api/lessons/test-refresh \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

3. Check the database for new records:

```sql
SELECT TOP 10
    cv.Id,
    cv.ExerciseId,
    cv.VersionNumber,
    cv.OriginalPrompt,
    cv.GeneratedPrompt,
    cv.ChangeLog,
    cv.GeneratedAt
FROM ContentVersions cv
ORDER BY cv.GeneratedAt DESC;
```

### Step 4: Verify Version History

Check that variations were created correctly:

```sql
-- View all versions for a specific exercise
SELECT
    ExerciseId,
    VersionNumber,
    OriginalPrompt,
    GeneratedPrompt,
    ChangeLog,
    IsActive,
    GeneratedAt
FROM ContentVersions
WHERE ExerciseId = '00000000-0000-0000-0000-000000000001'
ORDER BY VersionNumber DESC;
```

### Step 5: Test Rollback

Verify rollback functionality works:

```csharp
// In a test or temporary endpoint
var history = await _contentRefreshService.GetVersionHistoryAsync(exerciseId);
if (history.Count > 1)
{
    // Rollback to previous version
    await _contentRefreshService.RollbackToVersionAsync(exerciseId, history[1].VersionNumber);
}
```

---

## 📊 Expected Behavior

### Daily Job Execution

- **Time**: Every day at 2:00 AM UTC
- **Processes**: Up to 10 exercises
- **Per exercise**:
  - Generates new variation
  - Creates ContentVersion record
  - Logs changelog entry
  - Sets IsActive = true

### Exercise Variation Example

```
Original: "What is the past tense of 'go'?"

Generated variations:
- "Identify the correct past tense of 'go'"
- "Choose the best option: past tense of 'go' is..."
- "Complete: The past tense of 'go' is _____"
- "What does 'go' become in past tense?"
```

### Version History

Each exercise maintains complete version history:
```
Version 1 (Original)
  ├─ OriginalPrompt: "What is the past tense of 'go'?"
  ├─ GeneratedPrompt: "What is the past tense of 'go'?"
  └─ ChangeLog: "Initial version"

Version 2 (Day 1 refresh)
  ├─ OriginalPrompt: "What is the past tense of 'go'?"
  ├─ GeneratedPrompt: "Identify the correct past tense of 'go'"
  └─ ChangeLog: "Vocabulary: 7 → 8 words; Content refreshed for variety"

Version 3 (Day 2 refresh)
  ├─ OriginalPrompt: "What is the past tense of 'go'?"
  ├─ GeneratedPrompt: "Choose the best option: past tense of 'go' is..."
  └─ ChangeLog: "Vocabulary: 7 → 10 words; Content refreshed for variety"
```

---

## ⚠️ Troubleshooting

### Issue: Job not running at 2:00 AM

**Solution**: Hangfire in-memory storage doesn't persist across app restarts. The job is registered on startup. If you need persistent jobs, migrate to:
- Hangfire.Pro.Redis
- Hangfire.SqlServer
- Hangfire PostgreSQL

### Issue: Duplicate variations

**Solution**: The system skips exercises that already have a version created today:
```csharp
// In ContentRefreshService.RefreshDailyContentAsync()
var today = DateTime.UtcNow.Date;
var exercisesToUpdate = await _context.Exercises
    .Where(e => !_context.ContentVersions
        .Where(cv => cv.ExerciseId == e.Id && cv.CreatedAt.Date == today)
        .Any())
    .Take(10)
    .ToListAsync();
```

### Issue: High memory usage

**Solution**: The in-memory storage keeps all job data in RAM. For production with millions of jobs, use persistent storage (SQL Server, Redis, etc.)

---

## 🚀 Production Deployment

For production, upgrade from in-memory storage:

```csharp
// Option 1: SQL Server (Recommended)
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage("Your_Connection_String"));

// Option 2: Redis
builder.Services.AddHangfire(config =>
    config.UseRedisStorage("localhost:6379"));
```

---

## ✅ Verification Checklist

- [ ] Build succeeds (0 errors, 0 warnings)
- [ ] API starts without errors
- [ ] Hangfire Server logs appear
- [ ] Daily job scheduled message appears
- [ ] ContentVersions table exists in database
- [ ] Exercise variations are generated
- [ ] Changelog entries are created
- [ ] Version history is maintained
- [ ] Rollback functionality works

---

## 📚 Related Files

- **Service**: `src/EA.Infrastructure/Services/ContentRefreshService.cs`
- **Generator**: `src/EA.Infrastructure/Services/ExerciseGeneratorService.cs`
- **Entity**: `src/EA.Domain/Entities/ContentVersion.cs`
- **Migration**: `src/EA.Infrastructure/Migrations/AddContentVersioning.cs`
- **Configuration**: `src/EA.API/Program.cs` (lines 121-140)

---

## 📞 Support

For issues or questions:
1. Check Hangfire logs in `Program.cs` output
2. Review SQL queries in `ContentRefreshService.RefreshDailyContentAsync()`
3. Verify EF migration ran: `SELECT * FROM ContentVersions`
4. Check `_logger` entries for detailed error messages
