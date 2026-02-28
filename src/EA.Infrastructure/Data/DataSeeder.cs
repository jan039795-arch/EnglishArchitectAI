using EA.Domain.Entities;
using EA.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EA.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration failed");
            return;
        }

        if (await context.Levels.AnyAsync()) return;

        logger.LogInformation("Seeding database...");

        // ── LEVELS ────────────────────────────────────────────────────────────

        var lvlA1 = NewLevel("a1000000-0000-0000-0000-000000000001", "A1", "Beginner", 1);
        var lvlA2 = NewLevel("a2000000-0000-0000-0000-000000000002", "A2", "Elementary", 2, "Complete A1");
        var lvlB1 = NewLevel("b1000000-0000-0000-0000-000000000003", "B1", "Intermediate", 3, "Complete A2");
        var lvlB2 = NewLevel("b2000000-0000-0000-0000-000000000004", "B2", "Upper Intermediate", 4, "Complete B1");
        var lvlC1 = NewLevel("c1000000-0000-0000-0000-000000000005", "C1", "Advanced", 5, "Complete B2");
        var lvlC2 = NewLevel("c2000000-0000-0000-0000-000000000006", "C2", "Proficient", 6, "Complete C1");

        context.Levels.AddRange(lvlA1, lvlA2, lvlB1, lvlB2, lvlC1, lvlC2);

        // ── A1 ───────────────────────────────────────────────────────────────

        BuildA1Modules(context, lvlA1.Id);
        BuildA2Modules(context, lvlA2.Id);
        BuildB1Modules(context, lvlB1.Id);
        BuildB2Modules(context, lvlB2.Id);
        BuildC1Modules(context, lvlC1.Id);
        BuildC2Modules(context, lvlC2.Id);

        // ── ASSESSMENTS (one per level) ───────────────────────────────────────

        context.Assessments.AddRange(
            NewAssessment("a5100000-0000-0000-0000-000000000001", lvlA1.Id, "A1 Final Exam", 70, 15),
            NewAssessment("a5200000-0000-0000-0000-000000000002", lvlA2.Id, "A2 Final Exam", 70, 20),
            NewAssessment("a5300000-0000-0000-0000-000000000003", lvlB1.Id, "B1 Final Exam", 75, 25),
            NewAssessment("a5400000-0000-0000-0000-000000000004", lvlB2.Id, "B2 Final Exam", 75, 30),
            NewAssessment("a5500000-0000-0000-0000-000000000005", lvlC1.Id, "C1 Final Exam", 80, 35),
            NewAssessment("a5600000-0000-0000-0000-000000000006", lvlC2.Id, "C2 Final Exam", 80, 40));

        await context.SaveChangesAsync();
        logger.LogInformation("Seeding complete.");
    }

    // ── A1 ────────────────────────────────────────────────────────────────────

    private static void BuildA1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1Id = G("a1010000-0000-0000-0000-000000000001");
        var m2Id = G("a1020000-0000-0000-0000-000000000002");

        ctx.Modules.Add(NewModule(m1Id, levelId, "Greetings & Introductions", "Learn how to say hello and introduce yourself.", 1, 2));
        ctx.Modules.Add(NewModule(m2Id, levelId, "Colors & Everyday Objects", "Learn basic vocabulary for daily life.", 2, 2));

        // Module 1 lessons
        var l1Id = G("a1b10000-0000-0000-0000-000000000001");
        var l2Id = G("a1b20000-0000-0000-0000-000000000002");
        ctx.Lessons.Add(NewLesson(l1Id, m1Id, "Basic Greetings", SkillType.Listening, 1));
        ctx.Lessons.Add(NewLesson(l2Id, m1Id, "Numbers 1–20", SkillType.Reading, 2));

        AddExercises(ctx, l1Id, "a1e1",
            MC("The correct response to 'How are you?' is:", "Fine, thank you.", "I am hungry.", "Yesterday.", "She is tall.",
                "greetings,responses"),
            MC("Which phrase is used as a greeting?", "Good morning!", "See you later.", "Thank you.", "Sorry.",
                "greetings"),
            MC("How do you say goodbye formally?", "Farewell!", "Hello!", "Please.", "Never mind.",
                "greetings,farewells"),
            FB("Nice to ___ you.", "meet", "greetings"));

        AddExercises(ctx, l2Id, "a1e2",
            MC("How do you write 5 in English?", "five", "fife", "fife", "fiive",
                "numbers"),
            MC("Which number comes after eleven?", "twelve", "ten", "thirteen", "twenty",
                "numbers"),
            MC("Which is an odd number?", "seven", "four", "eight", "twelve",
                "numbers"),
            FB("I have ___ cats. (number: 2)", "two", "numbers"));

        // Module 2 lessons
        var l3Id = G("a1b30000-0000-0000-0000-000000000003");
        var l4Id = G("a1b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l3Id, m2Id, "Colors", SkillType.Reading, 1));
        ctx.Lessons.Add(NewLesson(l4Id, m2Id, "Classroom Objects", SkillType.Reading, 2));

        AddExercises(ctx, l3Id, "a1e3",
            MC("What color is the sky on a clear day?", "blue", "red", "yellow", "green",
                "colors,vocabulary"),
            MC("An apple is usually...", "red", "blue", "purple", "orange",
                "colors,vocabulary"),
            MC("Which is NOT a color?", "walk", "pink", "brown", "grey",
                "colors,vocabulary"),
            FB("Grass is ___.", "green", "colors"));

        AddExercises(ctx, l4Id, "a1e4",
            MC("You use this to write on paper.", "pen", "desk", "window", "floor",
                "vocabulary,objects"),
            MC("You sit on a ___.", "chair", "pencil", "book", "door",
                "vocabulary,objects"),
            MC("Which is a classroom object?", "book", "cloud", "river", "mountain",
                "vocabulary,objects"),
            FB("Open your ___. (the thing you study from)", "book", "vocabulary"));
    }

    // ── A2 ────────────────────────────────────────────────────────────────────

    private static void BuildA2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1Id = G("a2010000-0000-0000-0000-000000000001");
        var m2Id = G("a2020000-0000-0000-0000-000000000002");

        ctx.Modules.Add(NewModule(m1Id, levelId, "Present Simple", "Master everyday actions and habits.", 1, 3));
        ctx.Modules.Add(NewModule(m2Id, levelId, "Past Simple", "Talk about finished events in the past.", 2, 3));

        var l1Id = G("a2b10000-0000-0000-0000-000000000001");
        var l2Id = G("a2b20000-0000-0000-0000-000000000002");
        ctx.Lessons.Add(NewLesson(l1Id, m1Id, "Third Person Singular", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l2Id, m1Id, "Negatives & Questions", SkillType.Writing, 2));

        AddExercises(ctx, l1Id, "a2e1",
            MC("She ___ to school every day.", "goes", "go", "going", "goed",
                "present-simple,third-person"),
            MC("He ___ coffee every morning.", "drinks", "drink", "drinked", "drinking",
                "present-simple,third-person"),
            MC("The train ___ at 9 o'clock.", "arrives", "arrive", "arrived", "arriving",
                "present-simple,third-person"),
            FB("My brother ___ in London. (live)", "lives", "present-simple"));

        AddExercises(ctx, l2Id, "a2e2",
            MC("___ you like pizza?", "Do", "Does", "Is", "Are",
                "present-simple,questions"),
            MC("She ___ not speak French.", "does", "do", "is", "has",
                "present-simple,negatives"),
            MC("He doesn't ___ early.", "wake up", "wakes up", "woke up", "waking up",
                "present-simple,negatives"),
            FB("___ she work here? (auxiliary)", "Does", "present-simple,questions"));

        var l3Id = G("a2b30000-0000-0000-0000-000000000003");
        var l4Id = G("a2b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l3Id, m2Id, "Regular Past Verbs", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l4Id, m2Id, "Irregular Past Verbs", SkillType.Writing, 2));

        AddExercises(ctx, l3Id, "a2e3",
            MC("Yesterday I ___ to the park.", "walked", "walk", "walking", "walks",
                "past-simple,regular"),
            MC("She ___ the dishes after dinner.", "washed", "wash", "washing", "washes",
                "past-simple,regular"),
            MC("We ___ our homework on time.", "finished", "finish", "finishing", "finishes",
                "past-simple,regular"),
            FB("He ___ for two hours. (work)", "worked", "past-simple"));

        AddExercises(ctx, l4Id, "a2e4",
            MC("I ___ a great film last week.", "saw", "see", "seen", "sees",
                "past-simple,irregular"),
            MC("She ___ to Paris last summer.", "went", "go", "gone", "goes",
                "past-simple,irregular"),
            MC("We ___ dinner together.", "had", "have", "has", "having",
                "past-simple,irregular"),
            FB("I ___ up early this morning. (get)", "got", "past-simple,irregular"));
    }

    // ── B1 ────────────────────────────────────────────────────────────────────

    private static void BuildB1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1Id = G("b1010000-0000-0000-0000-000000000001");
        var m2Id = G("b1020000-0000-0000-0000-000000000002");

        ctx.Modules.Add(NewModule(m1Id, levelId, "Present Perfect", "Connect past experiences to the present.", 1, 4));
        ctx.Modules.Add(NewModule(m2Id, levelId, "Conditional Sentences", "Explore real and hypothetical situations.", 2, 4));

        var l1Id = G("b1b10000-0000-0000-0000-000000000001");
        var l2Id = G("b1b20000-0000-0000-0000-000000000002");
        ctx.Lessons.Add(NewLesson(l1Id, m1Id, "Have/Has + Past Participle", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l2Id, m1Id, "For vs Since", SkillType.Writing, 2));

        AddExercises(ctx, l1Id, "b1e1",
            MC("I ___ here for three years.", "have lived", "am living", "live", "lived",
                "present-perfect,duration"),
            MC("She ___ her homework already.", "has finished", "finished", "is finishing", "finishes",
                "present-perfect"),
            MC("Have you ever ___ sushi?", "eaten", "eat", "ate", "eating",
                "present-perfect,experience"),
            FB("He ___ three books this week. (read)", "has read", "present-perfect"));

        AddExercises(ctx, l2Id, "b1e2",
            MC("I have worked here ___ 2020.", "since", "for", "from", "during",
                "present-perfect,since-for"),
            MC("She has been ill ___ three days.", "for", "since", "during", "from",
                "present-perfect,since-for"),
            MC("We haven't spoken ___ a long time.", "for", "since", "from", "at",
                "present-perfect,since-for"),
            FB("He has known her ___ they were children. (since/for)", "since", "present-perfect,since-for"));

        var l3Id = G("b1b30000-0000-0000-0000-000000000003");
        var l4Id = G("b1b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l3Id, m2Id, "First Conditional", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l4Id, m2Id, "Second Conditional", SkillType.Writing, 2));

        AddExercises(ctx, l3Id, "b1e3",
            MC("If it rains, I ___ an umbrella.", "will take", "would take", "take", "took",
                "first-conditional"),
            MC("She ___ if she studies hard.", "will pass", "would pass", "passes", "passed",
                "first-conditional"),
            MC("If you hurry, you ___ the bus.", "will catch", "would catch", "catch", "caught",
                "first-conditional"),
            FB("If you ___ early, you'll get a good seat. (arrive)", "arrive", "first-conditional"));

        AddExercises(ctx, l4Id, "b1e4",
            MC("If I had more money, I ___ a new car.", "would buy", "will buy", "buy", "bought",
                "second-conditional"),
            MC("She would travel more if she ___ more time.", "had", "has", "would have", "having",
                "second-conditional"),
            MC("If I were you, I ___ accept the offer.", "would", "will", "should", "could",
                "second-conditional"),
            FB("If he ___ harder, he would succeed. (work)", "worked", "second-conditional"));
    }

    // ── B2 ────────────────────────────────────────────────────────────────────

    private static void BuildB2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1Id = G("b2010000-0000-0000-0000-000000000001");
        var m2Id = G("b2020000-0000-0000-0000-000000000002");

        ctx.Modules.Add(NewModule(m1Id, levelId, "Passive Voice", "Shift focus from agent to action.", 1, 5));
        ctx.Modules.Add(NewModule(m2Id, levelId, "Reported Speech", "Report what others said accurately.", 2, 5));

        var l1Id = G("b2b10000-0000-0000-0000-000000000001");
        var l2Id = G("b2b20000-0000-0000-0000-000000000002");
        ctx.Lessons.Add(NewLesson(l1Id, m1Id, "Present & Past Passive", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l2Id, m1Id, "Passive with Modals", SkillType.Writing, 2));

        AddExercises(ctx, l1Id, "b2e1",
            MC("The letter ___ yesterday.", "was written", "wrote", "is written", "has written",
                "passive,past"),
            MC("This bridge ___ every year.", "is inspected", "inspects", "was inspecting", "has inspected",
                "passive,present"),
            MC("The report must be ___ by Friday.", "submitted", "submit", "submitting", "submits",
                "passive,modals"),
            FB("The homework ___ by all students. (must + do, passive)", "must be done", "passive"));

        AddExercises(ctx, l2Id, "b2e2",
            MC("All complaints ___ in writing.", "should be made", "should make", "must make", "are making",
                "passive,modals"),
            MC("The results ___ before Thursday.", "must be analyzed", "must analyze", "analyzed", "are analyzed",
                "passive,modals"),
            MC("The package ___ on time.", "was delivered", "delivered", "has deliver", "is deliver",
                "passive,past"),
            FB("The decision ___ by the board. (make, past passive)", "was made", "passive"));

        var l3Id = G("b2b30000-0000-0000-0000-000000000003");
        var l4Id = G("b2b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l3Id, m2Id, "Reporting Statements", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l4Id, m2Id, "Reporting Questions", SkillType.Writing, 2));

        AddExercises(ctx, l3Id, "b2e3",
            MC("She said she ___ tired.", "was", "is", "were", "had",
                "reported-speech,statements"),
            MC("He told me he ___ the answer.", "knew", "knows", "know", "had know",
                "reported-speech,statements"),
            MC("They said they ___ going to the party.", "were", "are", "will be", "had",
                "reported-speech,statements"),
            FB("She said she ___ the film. (like, report)", "liked", "reported-speech"));

        AddExercises(ctx, l4Id, "b2e4",
            MC("He asked me where I ___.", "lived", "live", "am living", "had live",
                "reported-speech,questions"),
            MC("She wanted to know if I ___ Spanish.", "spoke", "speak", "am speaking", "had spoke",
                "reported-speech,questions"),
            MC("They asked what time it ___.", "was", "is", "were", "has been",
                "reported-speech,questions"),
            FB("He asked me if I ___ help. (need, report)", "needed", "reported-speech"));
    }

    // ── C1 ────────────────────────────────────────────────────────────────────

    private static void BuildC1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1Id = G("c1010000-0000-0000-0000-000000000001");
        var m2Id = G("c1020000-0000-0000-0000-000000000002");

        ctx.Modules.Add(NewModule(m1Id, levelId, "Advanced Grammar", "Master inversion, emphasis, and mixed conditionals.", 1, 6));
        ctx.Modules.Add(NewModule(m2Id, levelId, "Advanced Vocabulary", "Expand academic and idiomatic language.", 2, 6));

        var l1Id = G("c1b10000-0000-0000-0000-000000000001");
        var l2Id = G("c1b20000-0000-0000-0000-000000000002");
        ctx.Lessons.Add(NewLesson(l1Id, m1Id, "Inversion for Emphasis", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l2Id, m1Id, "Mixed Conditionals", SkillType.Writing, 2));

        AddExercises(ctx, l1Id, "c1e1",
            MC("Not only ___ he win the race, but he broke the record.", "did", "had", "was", "has",
                "inversion,advanced-grammar"),
            MC("Hardly ___ he arrived when problems started.", "had", "did", "was", "were",
                "inversion,advanced-grammar"),
            MC("Never ___ I seen such a beautiful place.", "have", "had", "did", "was",
                "inversion,advanced-grammar"),
            FB("Rarely ___ we have such good weather in November. (do/did)", "do", "inversion"));

        AddExercises(ctx, l2Id, "c1e2",
            MC("If I ___ taken that job, I would be rich now.", "had", "have", "would have", "was",
                "mixed-conditional"),
            MC("She would have more friends if she ___ friendlier.", "were", "was", "would be", "is",
                "mixed-conditional"),
            MC("He wouldn't be struggling now if he ___ worked harder then.", "had", "has", "would have", "did",
                "mixed-conditional"),
            FB("If I ___ born in Italy, I would speak Italian now. (be, third conditional)", "had been", "mixed-conditional"));

        var l3Id = G("c1b30000-0000-0000-0000-000000000003");
        var l4Id = G("c1b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l3Id, m2Id, "Academic & Formal Language", SkillType.Reading, 1));
        ctx.Lessons.Add(NewLesson(l4Id, m2Id, "Collocations & Idioms", SkillType.Reading, 2));

        AddExercises(ctx, l3Id, "c1e3",
            MC("A synonym of 'ubiquitous' is:", "omnipresent", "rare", "unique", "occasional",
                "vocabulary,academic"),
            MC("Which word means 'to make something worse'?", "exacerbate", "ameliorate", "mitigate", "alleviate",
                "vocabulary,academic"),
            MC("'Laconic' describes someone who is:", "brief in speech", "very talkative", "highly emotional", "deeply philosophical",
                "vocabulary,academic"),
            FB("The politician's speech was ___ — it went on for three hours. (wordy)", "verbose", "vocabulary"));

        AddExercises(ctx, l4Id, "c1e4",
            MC("She has a ___ memory — she never forgets anything.", "photographic", "selective", "short-term", "poor",
                "collocations,idioms"),
            MC("He was ___ with joy when he heard the news.", "beside himself", "above himself", "behind himself", "beyond himself",
                "idioms"),
            MC("'A blessing in disguise' means:", "something good that seemed bad", "a curse in hiding", "unexpected danger", "a literal blessing",
                "idioms"),
            FB("The negotiations hit a ___; no progress was made. (impasse word)", "deadlock", "vocabulary,idioms"));
    }

    // ── C2 ────────────────────────────────────────────────────────────────────

    private static void BuildC2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1Id = G("c2010000-0000-0000-0000-000000000001");
        var m2Id = G("c2020000-0000-0000-0000-000000000002");

        ctx.Modules.Add(NewModule(m1Id, levelId, "Nuanced Expression", "Achieve near-native register and style.", 1, 8));
        ctx.Modules.Add(NewModule(m2Id, levelId, "Academic Writing", "Master hedging, argumentation, and critical analysis.", 2, 8));

        var l1Id = G("c2b10000-0000-0000-0000-000000000001");
        var l2Id = G("c2b20000-0000-0000-0000-000000000002");
        ctx.Lessons.Add(NewLesson(l1Id, m1Id, "Register & Formal Style", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l2Id, m1Id, "Idiomatic Mastery", SkillType.Reading, 2));

        AddExercises(ctx, l1Id, "c2e1",
            MC("Which is the most formal equivalent of 'ask for'?", "solicit", "demand", "request", "inquire",
                "register,formal,vocabulary"),
            MC("The word 'ameliorate' means:", "to improve", "to worsen", "to ignore", "to delay",
                "vocabulary,formal"),
            MC("Which sentence uses the subjunctive correctly?", "I suggest that he be present.", "I suggest that he is present.", "I suggest that he will be present.", "I suggest that he was present.",
                "subjunctive,advanced-grammar"),
            FB("The treaty was ___ with great ceremony. (formally ratified)", "ratified", "vocabulary,formal"));

        AddExercises(ctx, l2Id, "c2e2",
            MC("'Pyrrhic victory' means:", "a win at too great a cost", "an easy victory", "a false victory", "an unexpected win",
                "idioms,vocabulary"),
            MC("Which phrase means 'from the very beginning'?", "from scratch", "from the top", "from ground zero", "from the start",
                "idioms"),
            MC("'Penultimate' means:", "second to last", "the very last", "the very first", "second from the beginning",
                "vocabulary"),
            FB("His argument was so ___ that no one could refute it. (well-constructed, logical)", "cogent", "vocabulary,academic"));

        var l3Id = G("c2b30000-0000-0000-0000-000000000003");
        var l4Id = G("c2b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l3Id, m2Id, "Hedging Language", SkillType.Writing, 1));
        ctx.Lessons.Add(NewLesson(l4Id, m2Id, "Critical Analysis", SkillType.Reading, 2));

        AddExercises(ctx, l3Id, "c2e3",
            MC("Which is an example of hedging language?", "It could be argued that...", "It is absolutely certain that...", "It is obvious that...", "Clearly, ...",
                "hedging,academic-writing"),
            MC("'Ostensibly' means:", "apparently, though not actually", "certainly", "obviously", "extremely",
                "vocabulary,academic"),
            MC("Which is NOT a hedging expression?", "It is absolutely certain that...", "It seems that...", "It could be suggested...", "Evidence appears to indicate...",
                "hedging,academic-writing"),
            FB("The results ___ suggest a correlation between the variables. (appear)", "appear to", "hedging"));

        AddExercises(ctx, l4Id, "c2e4",
            MC("A 'non sequitur' is:", "a conclusion that doesn't follow the premise", "a logical argument", "a strong counterpoint", "a rhetorical question",
                "critical-analysis,vocabulary"),
            MC("'Equivocal' describes a statement that is:", "ambiguous", "crystal clear", "misleading intentionally", "factually incorrect",
                "vocabulary,critical-analysis"),
            MC("Which word means 'talking around the point'?", "circumlocution", "elaboration", "amplification", "clarification",
                "vocabulary,critical-analysis"),
            FB("The data is ___ — it supports more than one interpretation. (ambiguous, formal)", "equivocal", "vocabulary"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Level NewLevel(string id, string code, string name, int order, string? unlock = null) =>
        new() { Id = G(id), Code = code, Name = name, Order = order, UnlockRequirement = unlock };

    private static Module NewModule(Guid id, Guid levelId, string title, string desc, int order, int hours) =>
        new() { Id = id, LevelId = levelId, Title = title, Description = desc, Order = order, EstimatedHours = hours };

    private static Lesson NewLesson(Guid id, Guid moduleId, string title, SkillType skill, int order) =>
        new() { Id = id, ModuleId = moduleId, Title = title, SkillType = skill, Order = order };

    private static Assessment NewAssessment(string id, Guid levelId, string title, int passScore, int minutes) =>
        new() { Id = G(id), ScopeType = AssessmentScopeType.Level, ScopeId = levelId, Title = title, PassScore = passScore, TimeLimitMinutes = minutes, CEFRAligned = true };

    private static Guid G(string s) => Guid.Parse(s);

    /// <summary>Multiple-choice exercise with 4 options. First option is always correct.</summary>
    private static (ExerciseType Type, string Prompt, string Correct, string[] Others, string Tags) MC(
        string prompt, string correct, string o2, string o3, string o4, string tags) =>
        (ExerciseType.MultipleChoice, prompt, correct, [o2, o3, o4], tags);

    /// <summary>Fill-in-the-blank exercise (no options).</summary>
    private static (ExerciseType Type, string Prompt, string Correct, string[] Others, string Tags) FB(
        string prompt, string correct, string tags) =>
        (ExerciseType.FillBlank, prompt, correct, [], tags);

    private static void AddExercises(
        ApplicationDbContext ctx,
        Guid lessonId,
        string idPrefix,
        params (ExerciseType Type, string Prompt, string Correct, string[] Others, string Tags)[] exercises)
    {
        for (int i = 0; i < exercises.Length; i++)
        {
            var (type, prompt, correct, others, tags) = exercises[i];
            var exId = G($"{idPrefix}{i + 1:00}00-0000-0000-0000-000000000001");

            var exercise = new Exercise
            {
                Id = exId,
                LessonId = lessonId,
                Type = type,
                Prompt = prompt,
                CorrectAnswer = correct,
                Difficulty = 1,
                Tags = tags,
                Source = ExerciseSource.Manual,
                IsValidated = true
            };
            ctx.Exercises.Add(exercise);

            if (type == ExerciseType.MultipleChoice)
            {
                var allOptions = new[] { correct }.Concat(others).ToArray();
                // Shuffle deterministically by rotating based on exercise index
                var shuffled = allOptions.Skip(i % allOptions.Length)
                                         .Concat(allOptions.Take(i % allOptions.Length))
                                         .ToArray();

                for (int j = 0; j < shuffled.Length; j++)
                {
                    ctx.ExerciseOptions.Add(new ExerciseOption
                    {
                        Id = G($"{idPrefix}{i + 1:00}{j + 1:0}0-0000-0000-0000-000000000001"),
                        ExerciseId = exId,
                        Text = shuffled[j],
                        IsCorrect = shuffled[j] == correct,
                        Explanation = shuffled[j] == correct ? "Correct!" : null
                    });
                }
            }
        }
    }
}
