namespace EA.Application.Features.Lessons.SearchLessons;

public record SearchLessonDto(Guid LessonId, string LessonTitle, string ModuleTitle, string LevelCode, Guid ModuleId);
