using EA.Domain.Entities;
using EA.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EA.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try { await context.Database.MigrateAsync(); }
        catch (Exception ex) { logger.LogError(ex, "Migration failed"); return; }

        if (await context.Levels.AnyAsync())
        {
            logger.LogInformation("Updating lesson content with latest videos...");
            BackfillContent(context);
            await context.SaveChangesAsync();
            logger.LogInformation("Content update complete.");
            return;
        }

        logger.LogInformation("Seeding database...");

        var lvlA1 = NewLevel("a1000000-0000-0000-0000-000000000001", "A1", "Beginner", 1);
        var lvlA2 = NewLevel("a2000000-0000-0000-0000-000000000002", "A2", "Elementary", 2, "Complete A1");
        var lvlB1 = NewLevel("b1000000-0000-0000-0000-000000000003", "B1", "Intermediate", 3, "Complete A2");
        var lvlB2 = NewLevel("b2000000-0000-0000-0000-000000000004", "B2", "Upper Intermediate", 4, "Complete B1");
        var lvlC1 = NewLevel("c1000000-0000-0000-0000-000000000005", "C1", "Advanced", 5, "Complete B2");
        var lvlC2 = NewLevel("c2000000-0000-0000-0000-000000000006", "C2", "Proficient", 6, "Complete C1");
        context.Levels.AddRange(lvlA1, lvlA2, lvlB1, lvlB2, lvlC1, lvlC2);

        BuildA1Modules(context, lvlA1.Id);
        BuildA2Modules(context, lvlA2.Id);
        BuildB1Modules(context, lvlB1.Id);
        BuildB2Modules(context, lvlB2.Id);
        BuildC1Modules(context, lvlC1.Id);
        BuildC2Modules(context, lvlC2.Id);

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

    // ── BACKFILL ──────────────────────────────────────────────────────────────

    private static void BackfillContent(ApplicationDbContext ctx)
    {
        var map = ContentMap();
        foreach (var lesson in ctx.Lessons.Where(l => l.ContentJson == null).ToList())
            if (map.TryGetValue(lesson.Id, out var json))
                lesson.ContentJson = json;
    }

    private static Dictionary<Guid, string> ContentMap() => new()
    {
        [G("a1b10000-0000-0000-0000-000000000001")] = A1L1(),
        [G("a1b20000-0000-0000-0000-000000000002")] = A1L2(),
        [G("a1b30000-0000-0000-0000-000000000003")] = A1L3(),
        [G("a1b40000-0000-0000-0000-000000000004")] = A1L4(),
        [G("a2b10000-0000-0000-0000-000000000001")] = A2L1(),
        [G("a2b20000-0000-0000-0000-000000000002")] = A2L2(),
        [G("a2b30000-0000-0000-0000-000000000003")] = A2L3(),
        [G("a2b40000-0000-0000-0000-000000000004")] = A2L4(),
        [G("b1b10000-0000-0000-0000-000000000001")] = B1L1(),
        [G("b1b20000-0000-0000-0000-000000000002")] = B1L2(),
        [G("b1b30000-0000-0000-0000-000000000003")] = B1L3(),
        [G("b1b40000-0000-0000-0000-000000000004")] = B1L4(),
        [G("b2b10000-0000-0000-0000-000000000001")] = B2L1(),
        [G("b2b20000-0000-0000-0000-000000000002")] = B2L2(),
        [G("b2b30000-0000-0000-0000-000000000003")] = B2L3(),
        [G("b2b40000-0000-0000-0000-000000000004")] = B2L4(),
        [G("c1b10000-0000-0000-0000-000000000001")] = C1L1(),
        [G("c1b20000-0000-0000-0000-000000000002")] = C1L2(),
        [G("c1b30000-0000-0000-0000-000000000003")] = C1L3(),
        [G("c1b40000-0000-0000-0000-000000000004")] = C1L4(),
        [G("c2b10000-0000-0000-0000-000000000001")] = C2L1(),
        [G("c2b20000-0000-0000-0000-000000000002")] = C2L2(),
        [G("c2b30000-0000-0000-0000-000000000003")] = C2L3(),
        [G("c2b40000-0000-0000-0000-000000000004")] = C2L4(),
    };

    // ── BUILD MODULES ─────────────────────────────────────────────────────────

    private static void BuildA1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("a1010000-0000-0000-0000-000000000001");
        var m2 = G("a1020000-0000-0000-0000-000000000002");
        ctx.Modules.Add(NewModule(m1, levelId, "Greetings & Introductions", "Learn how to say hello and introduce yourself.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Colors & Everyday Objects", "Learn basic vocabulary for daily life.", 2, 2));

        var l1 = G("a1b10000-0000-0000-0000-000000000001");
        var l2 = G("a1b20000-0000-0000-0000-000000000002");
        var l3 = G("a1b30000-0000-0000-0000-000000000003");
        var l4 = G("a1b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l1, m1, "Basic Greetings", SkillType.Listening, 1, A1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Numbers 1–20", SkillType.Reading, 2, A1L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Colors", SkillType.Reading, 1, A1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Classroom Objects", SkillType.Reading, 2, A1L4()));

        AddExercises(ctx, l1, "a1e1",
            MC("The correct response to 'How are you?' is:", "Fine, thank you.", "I am hungry.", "Yesterday.", "She is tall.", "greetings,responses"),
            MC("Which phrase is used as a greeting?", "Good morning!", "See you later.", "Thank you.", "Sorry.", "greetings"),
            MC("How do you say goodbye formally?", "Farewell!", "Hello!", "Please.", "Never mind.", "greetings,farewells"),
            FB("Nice to ___ you.", "meet", "greetings"));

        AddExercises(ctx, l2, "a1e2",
            MC("How do you write 5 in English?", "five", "fife", "fife", "fiive", "numbers"),
            MC("Which number comes after eleven?", "twelve", "ten", "thirteen", "twenty", "numbers"),
            MC("Which is an odd number?", "seven", "four", "eight", "twelve", "numbers"),
            FB("I have ___ cats. (number: 2)", "two", "numbers"));

        AddExercises(ctx, l3, "a1e3",
            MC("What color is the sky on a clear day?", "blue", "red", "yellow", "green", "colors,vocabulary"),
            MC("An apple is usually...", "red", "blue", "purple", "orange", "colors,vocabulary"),
            MC("Which is NOT a color?", "walk", "pink", "brown", "grey", "colors,vocabulary"),
            FB("Grass is ___.", "green", "colors"));

        AddExercises(ctx, l4, "a1e4",
            MC("You use this to write on paper.", "pen", "desk", "window", "floor", "vocabulary,objects"),
            MC("You sit on a ___.", "chair", "pencil", "book", "door", "vocabulary,objects"),
            MC("Which is a classroom object?", "book", "cloud", "river", "mountain", "vocabulary,objects"),
            FB("Open your ___. (the thing you study from)", "book", "vocabulary"));
    }

    private static void BuildA2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("a2010000-0000-0000-0000-000000000001");
        var m2 = G("a2020000-0000-0000-0000-000000000002");
        ctx.Modules.Add(NewModule(m1, levelId, "Present Simple", "Master everyday actions and habits.", 1, 3));
        ctx.Modules.Add(NewModule(m2, levelId, "Past Simple", "Talk about finished events in the past.", 2, 3));

        var l1 = G("a2b10000-0000-0000-0000-000000000001");
        var l2 = G("a2b20000-0000-0000-0000-000000000002");
        var l3 = G("a2b30000-0000-0000-0000-000000000003");
        var l4 = G("a2b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l1, m1, "Third Person Singular", SkillType.Writing, 1, A2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Negatives & Questions", SkillType.Writing, 2, A2L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Regular Past Verbs", SkillType.Writing, 1, A2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Irregular Past Verbs", SkillType.Writing, 2, A2L4()));

        AddExercises(ctx, l1, "a2e1",
            MC("She ___ to school every day.", "goes", "go", "going", "goed", "present-simple,third-person"),
            MC("He ___ coffee every morning.", "drinks", "drink", "drinked", "drinking", "present-simple,third-person"),
            MC("The train ___ at 9 o'clock.", "arrives", "arrive", "arrived", "arriving", "present-simple,third-person"),
            FB("My brother ___ in London. (live)", "lives", "present-simple"));

        AddExercises(ctx, l2, "a2e2",
            MC("___ you like pizza?", "Do", "Does", "Is", "Are", "present-simple,questions"),
            MC("She ___ not speak French.", "does", "do", "is", "has", "present-simple,negatives"),
            MC("He doesn't ___ early.", "wake up", "wakes up", "woke up", "waking up", "present-simple,negatives"),
            FB("___ she work here? (auxiliary)", "Does", "present-simple,questions"));

        AddExercises(ctx, l3, "a2e3",
            MC("Yesterday I ___ to the park.", "walked", "walk", "walking", "walks", "past-simple,regular"),
            MC("She ___ the dishes after dinner.", "washed", "wash", "washing", "washes", "past-simple,regular"),
            MC("We ___ our homework on time.", "finished", "finish", "finishing", "finishes", "past-simple,regular"),
            FB("He ___ for two hours. (work)", "worked", "past-simple"));

        AddExercises(ctx, l4, "a2e4",
            MC("I ___ a great film last week.", "saw", "see", "seen", "sees", "past-simple,irregular"),
            MC("She ___ to Paris last summer.", "went", "go", "gone", "goes", "past-simple,irregular"),
            MC("We ___ dinner together.", "had", "have", "has", "having", "past-simple,irregular"),
            FB("I ___ up early this morning. (get)", "got", "past-simple,irregular"));
    }

    private static void BuildB1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("b1010000-0000-0000-0000-000000000001");
        var m2 = G("b1020000-0000-0000-0000-000000000002");
        ctx.Modules.Add(NewModule(m1, levelId, "Present Perfect", "Connect past experiences to the present.", 1, 4));
        ctx.Modules.Add(NewModule(m2, levelId, "Conditional Sentences", "Explore real and hypothetical situations.", 2, 4));

        var l1 = G("b1b10000-0000-0000-0000-000000000001");
        var l2 = G("b1b20000-0000-0000-0000-000000000002");
        var l3 = G("b1b30000-0000-0000-0000-000000000003");
        var l4 = G("b1b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l1, m1, "Have/Has + Past Participle", SkillType.Writing, 1, B1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "For vs Since", SkillType.Writing, 2, B1L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "First Conditional", SkillType.Writing, 1, B1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Second Conditional", SkillType.Writing, 2, B1L4()));

        AddExercises(ctx, l1, "b1e1",
            MC("I ___ here for three years.", "have lived", "am living", "live", "lived", "present-perfect,duration"),
            MC("She ___ her homework already.", "has finished", "finished", "is finishing", "finishes", "present-perfect"),
            MC("Have you ever ___ sushi?", "eaten", "eat", "ate", "eating", "present-perfect,experience"),
            FB("He ___ three books this week. (read)", "has read", "present-perfect"));

        AddExercises(ctx, l2, "b1e2",
            MC("I have worked here ___ 2020.", "since", "for", "from", "during", "present-perfect,since-for"),
            MC("She has been ill ___ three days.", "for", "since", "during", "from", "present-perfect,since-for"),
            MC("We haven't spoken ___ a long time.", "for", "since", "from", "at", "present-perfect,since-for"),
            FB("He has known her ___ they were children. (since/for)", "since", "present-perfect,since-for"));

        AddExercises(ctx, l3, "b1e3",
            MC("If it rains, I ___ an umbrella.", "will take", "would take", "take", "took", "first-conditional"),
            MC("She ___ if she studies hard.", "will pass", "would pass", "passes", "passed", "first-conditional"),
            MC("If you hurry, you ___ the bus.", "will catch", "would catch", "catch", "caught", "first-conditional"),
            FB("If you ___ early, you'll get a good seat. (arrive)", "arrive", "first-conditional"));

        AddExercises(ctx, l4, "b1e4",
            MC("If I had more money, I ___ a new car.", "would buy", "will buy", "buy", "bought", "second-conditional"),
            MC("She would travel more if she ___ more time.", "had", "has", "would have", "having", "second-conditional"),
            MC("If I were you, I ___ accept the offer.", "would", "will", "should", "could", "second-conditional"),
            FB("If he ___ harder, he would succeed. (work)", "worked", "second-conditional"));
    }

    private static void BuildB2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("b2010000-0000-0000-0000-000000000001");
        var m2 = G("b2020000-0000-0000-0000-000000000002");
        ctx.Modules.Add(NewModule(m1, levelId, "Passive Voice", "Shift focus from agent to action.", 1, 5));
        ctx.Modules.Add(NewModule(m2, levelId, "Reported Speech", "Report what others said accurately.", 2, 5));

        var l1 = G("b2b10000-0000-0000-0000-000000000001");
        var l2 = G("b2b20000-0000-0000-0000-000000000002");
        var l3 = G("b2b30000-0000-0000-0000-000000000003");
        var l4 = G("b2b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l1, m1, "Present & Past Passive", SkillType.Writing, 1, B2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Passive with Modals", SkillType.Writing, 2, B2L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Reporting Statements", SkillType.Writing, 1, B2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Reporting Questions", SkillType.Writing, 2, B2L4()));

        AddExercises(ctx, l1, "b2e1",
            MC("The letter ___ yesterday.", "was written", "wrote", "is written", "has written", "passive,past"),
            MC("This bridge ___ every year.", "is inspected", "inspects", "was inspecting", "has inspected", "passive,present"),
            MC("The report must be ___ by Friday.", "submitted", "submit", "submitting", "submits", "passive,modals"),
            FB("The homework ___ by all students. (must + do, passive)", "must be done", "passive"));

        AddExercises(ctx, l2, "b2e2",
            MC("All complaints ___ in writing.", "should be made", "should make", "must make", "are making", "passive,modals"),
            MC("The results ___ before Thursday.", "must be analyzed", "must analyze", "analyzed", "are analyzed", "passive,modals"),
            MC("The package ___ on time.", "was delivered", "delivered", "has deliver", "is deliver", "passive,past"),
            FB("The decision ___ by the board. (make, past passive)", "was made", "passive"));

        AddExercises(ctx, l3, "b2e3",
            MC("She said she ___ tired.", "was", "is", "were", "had", "reported-speech,statements"),
            MC("He told me he ___ the answer.", "knew", "knows", "know", "had know", "reported-speech,statements"),
            MC("They said they ___ going to the party.", "were", "are", "will be", "had", "reported-speech,statements"),
            FB("She said she ___ the film. (like, report)", "liked", "reported-speech"));

        AddExercises(ctx, l4, "b2e4",
            MC("He asked me where I ___.", "lived", "live", "am living", "had live", "reported-speech,questions"),
            MC("She wanted to know if I ___ Spanish.", "spoke", "speak", "am speaking", "had spoke", "reported-speech,questions"),
            MC("They asked what time it ___.", "was", "is", "were", "has been", "reported-speech,questions"),
            FB("He asked me if I ___ help. (need, report)", "needed", "reported-speech"));
    }

    private static void BuildC1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("c1010000-0000-0000-0000-000000000001");
        var m2 = G("c1020000-0000-0000-0000-000000000002");
        ctx.Modules.Add(NewModule(m1, levelId, "Advanced Grammar", "Master inversion, emphasis, and mixed conditionals.", 1, 6));
        ctx.Modules.Add(NewModule(m2, levelId, "Advanced Vocabulary", "Expand academic and idiomatic language.", 2, 6));

        var l1 = G("c1b10000-0000-0000-0000-000000000001");
        var l2 = G("c1b20000-0000-0000-0000-000000000002");
        var l3 = G("c1b30000-0000-0000-0000-000000000003");
        var l4 = G("c1b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l1, m1, "Inversion for Emphasis", SkillType.Writing, 1, C1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Mixed Conditionals", SkillType.Writing, 2, C1L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Academic & Formal Language", SkillType.Reading, 1, C1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Collocations & Idioms", SkillType.Reading, 2, C1L4()));

        AddExercises(ctx, l1, "c1e1",
            MC("Not only ___ he win the race, but he broke the record.", "did", "had", "was", "has", "inversion,advanced-grammar"),
            MC("Hardly ___ he arrived when problems started.", "had", "did", "was", "were", "inversion,advanced-grammar"),
            MC("Never ___ I seen such a beautiful place.", "have", "had", "did", "was", "inversion,advanced-grammar"),
            FB("Rarely ___ we have such good weather in November. (do/did)", "do", "inversion"));

        AddExercises(ctx, l2, "c1e2",
            MC("If I ___ taken that job, I would be rich now.", "had", "have", "would have", "was", "mixed-conditional"),
            MC("She would have more friends if she ___ friendlier.", "were", "was", "would be", "is", "mixed-conditional"),
            MC("He wouldn't be struggling now if he ___ worked harder then.", "had", "has", "would have", "did", "mixed-conditional"),
            FB("If I ___ born in Italy, I would speak Italian now. (be, third conditional)", "had been", "mixed-conditional"));

        AddExercises(ctx, l3, "c1e3",
            MC("A synonym of 'ubiquitous' is:", "omnipresent", "rare", "unique", "occasional", "vocabulary,academic"),
            MC("Which word means 'to make something worse'?", "exacerbate", "ameliorate", "mitigate", "alleviate", "vocabulary,academic"),
            MC("'Laconic' describes someone who is:", "brief in speech", "very talkative", "highly emotional", "deeply philosophical", "vocabulary,academic"),
            FB("The politician's speech was ___ — it went on for three hours. (wordy)", "verbose", "vocabulary"));

        AddExercises(ctx, l4, "c1e4",
            MC("She has a ___ memory — she never forgets anything.", "photographic", "selective", "short-term", "poor", "collocations,idioms"),
            MC("He was ___ with joy when he heard the news.", "beside himself", "above himself", "behind himself", "beyond himself", "idioms"),
            MC("'A blessing in disguise' means:", "something good that seemed bad", "a curse in hiding", "unexpected danger", "a literal blessing", "idioms"),
            FB("The negotiations hit a ___; no progress was made. (impasse word)", "deadlock", "vocabulary,idioms"));
    }

    private static void BuildC2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("c2010000-0000-0000-0000-000000000001");
        var m2 = G("c2020000-0000-0000-0000-000000000002");
        ctx.Modules.Add(NewModule(m1, levelId, "Nuanced Expression", "Achieve near-native register and style.", 1, 8));
        ctx.Modules.Add(NewModule(m2, levelId, "Academic Writing", "Master hedging, argumentation, and critical analysis.", 2, 8));

        var l1 = G("c2b10000-0000-0000-0000-000000000001");
        var l2 = G("c2b20000-0000-0000-0000-000000000002");
        var l3 = G("c2b30000-0000-0000-0000-000000000003");
        var l4 = G("c2b40000-0000-0000-0000-000000000004");
        ctx.Lessons.Add(NewLesson(l1, m1, "Register & Formal Style", SkillType.Writing, 1, C2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Idiomatic Mastery", SkillType.Reading, 2, C2L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Hedging Language", SkillType.Writing, 1, C2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Critical Analysis", SkillType.Reading, 2, C2L4()));

        AddExercises(ctx, l1, "c2e1",
            MC("Which is the most formal equivalent of 'ask for'?", "solicit", "demand", "request", "inquire", "register,formal,vocabulary"),
            MC("The word 'ameliorate' means:", "to improve", "to worsen", "to ignore", "to delay", "vocabulary,formal"),
            MC("Which sentence uses the subjunctive correctly?", "I suggest that he be present.", "I suggest that he is present.", "I suggest that he will be present.", "I suggest that he was present.", "subjunctive,advanced-grammar"),
            FB("The treaty was ___ with great ceremony. (formally ratified)", "ratified", "vocabulary,formal"));

        AddExercises(ctx, l2, "c2e2",
            MC("'Pyrrhic victory' means:", "a win at too great a cost", "an easy victory", "a false victory", "an unexpected win", "idioms,vocabulary"),
            MC("Which phrase means 'from the very beginning'?", "from scratch", "from the top", "from ground zero", "from the start", "idioms"),
            MC("'Penultimate' means:", "second to last", "the very last", "the very first", "second from the beginning", "vocabulary"),
            FB("His argument was so ___ that no one could refute it. (well-constructed, logical)", "cogent", "vocabulary,academic"));

        AddExercises(ctx, l3, "c2e3",
            MC("Which is an example of hedging language?", "It could be argued that...", "It is absolutely certain that...", "It is obvious that...", "Clearly, ...", "hedging,academic-writing"),
            MC("'Ostensibly' means:", "apparently, though not actually", "certainly", "obviously", "extremely", "vocabulary,academic"),
            MC("Which is NOT a hedging expression?", "It is absolutely certain that...", "It seems that...", "It could be suggested...", "Evidence appears to indicate...", "hedging,academic-writing"),
            FB("The results ___ suggest a correlation between the variables. (appear)", "appear to", "hedging"));

        AddExercises(ctx, l4, "c2e4",
            MC("A 'non sequitur' is:", "a conclusion that doesn't follow the premise", "a logical argument", "a strong counterpoint", "a rhetorical question", "critical-analysis,vocabulary"),
            MC("'Equivocal' describes a statement that is:", "ambiguous", "crystal clear", "misleading intentionally", "factually incorrect", "vocabulary,critical-analysis"),
            MC("Which word means 'talking around the point'?", "circumlocution", "elaboration", "amplification", "clarification", "vocabulary,critical-analysis"),
            FB("The data is ___ — it supports more than one interpretation. (ambiguous, formal)", "equivocal", "vocabulary"));
    }

    // ── LESSON CONTENT ────────────────────────────────────────────────────────

    private static string A1L1() => C(
        "Greetings are the first words you use when meeting someone. In English, you choose different greetings depending on the time of day and how formal the situation is. British Council and Perfect English Grammar recommend practicing greetings with natural, conversational speed.",
        ["Use 'Good morning' before noon", "Use 'Good afternoon' from noon to 6 pm", "Use 'Good evening' after 6 pm", "'Hi' and 'Hey' are informal — use with friends only", "Reply to 'How are you?' with: 'Fine, thanks' or 'Great, and you?'"],
        [("Hello", "Hola", "Hello, my name is Ana."), ("Hi", "Hola (informal)", "Hi! How are you?"), ("Good morning", "Buenos días", "Good morning, teacher!"), ("Goodbye / Bye", "Adiós", "Goodbye! See you tomorrow."), ("How are you?", "¿Cómo estás?", "How are you? — Fine, thank you.")],
        [("Good morning! How are you?", "Greeting in the morning"), ("Fine, thank you. And you?", "Responding to 'How are you?'"), ("Nice to meet you!", "Meeting someone for the first time"), ("Goodbye! See you later.", "Saying farewell")],
        "Write a short dialogue (4–6 lines) between two people meeting for the first time. Use at least 3 different greetings.",
        "The 'h' in 'hello' and 'hi' is always pronounced. Say it with a clear breath — HEH-loh, not EH-loh.",
        [("English Greetings for Beginners", "EnglishClass101", "ozT4MI_oHFc"), ("How to Greet People in English", "BBC Learning English", "oJx2iH0VJQA")]
    );

    private static string A1L2() => C(
        "Numbers 1–20 in English must be memorized. Numbers 13–19 follow a pattern ending in '-teen'. After 20, we combine 'twenty' with units (twenty-one, twenty-two…). Resources like Perfect English Grammar provide detailed breakdowns of this crucial foundation.",
        ["1–12 are unique words: one, two, three, four, five, six, seven, eight, nine, ten, eleven, twelve", "13–19 end in '-teen': thirteen, fourteen, fifteen, sixteen, seventeen, eighteen, nineteen", "Watch out! 'thirteen' (13) vs 'thirty' (30) — stress the '-TEEN' ending", "'fifteen' comes from 'five' — spelling changes", "'twelve' is NOT 'twoten' — it is a unique word"],
        [("one / two / three", "uno / dos / tres", "I have two brothers."), ("eleven / twelve", "once / doce", "There are twelve months in a year."), ("thirteen / fifteen / nineteen", "trece / quince / diecinueve", "She is fifteen years old."), ("twenty", "veinte", "I have twenty dollars.")],
        [("I have three cats and two dogs.", "Using small numbers"), ("She is fifteen years old.", "Talking about age"), ("There are twelve students in the class.", "Counting people"), ("My phone number is five-five-three, two-one-four-zero.", "Reading phone numbers")],
        "Write 5 sentences using different numbers: your age, people in your family, your address number, phone digits, and how many hours you sleep.",
        "Stress the '-TEEN': thirTEEN, fourTEEN. Compare: THIRty, FOURty (stress on first syllable for the tens).",
        [("English Numbers 1-20", "EnglishClass101", "5Fn4r0gMUFM"), ("Numbers 1-100 in English", "Learn English with Papa English", "92BdANvnwh4")]
    );

    private static string A1L3() => C(
        "Colors in English are adjectives. They always go BEFORE the noun, not after it — opposite to Spanish. In English: 'a red car', never 'a car red'. Perfect English Grammar provides detailed explanations on color usage.",
        ["Color comes BEFORE the noun: 'a red car', 'the blue sky'", "Primary colors: red, blue, yellow", "Secondary: green (blue+yellow), orange (red+yellow), purple (red+blue)", "Neutral: black, white, grey, brown", "Light/dark: light blue, dark green"],
        [("red", "rojo", "I have a red bag."), ("blue", "azul", "The sky is blue."), ("green", "verde", "Grass is green."), ("yellow", "amarillo", "Bananas are yellow."), ("orange", "naranja", "She wears an orange shirt."), ("white / black", "blanco / negro", "Snow is white. His car is black.")],
        [("The sky is blue on a clear day.", "Describing nature"), ("I have a red umbrella.", "Color before noun"), ("She is wearing a green dress.", "Describing clothes"), ("What is your favorite color? — My favorite color is purple.", "Asking preferences")],
        "Look around you right now. Describe 6 objects using colors. Example: 'My notebook is blue. My pen is black.'",
        "'Orange' is pronounced OR-inj (two syllables). 'White' — the 'wh' sounds like a regular 'w' in modern English.",
        [("Learn Colors in English", "Learn English Kids", "U_8dO7WcHUE"), ("English Vocabulary: Colors", "Papa English", "cKGPRrFT-zU")]
    );

    private static string A1L4() => C(
        "Knowing the names of objects in your classroom or study space helps you communicate and follow instructions in English. We use 'a' before consonant sounds and 'an' before vowel sounds. The British Council emphasizes practical vocabulary for real-world communication.",
        ["Use 'a' before consonant sounds: a pen, a book, a chair", "Use 'an' before vowel sounds: an eraser, an umbrella", "Plural: add -s to most nouns (pen → pens, book → books)", "Add -es after -ch, -sh, -x, -s, -o: box → boxes", "Commands: 'Open your book', 'Close the door', 'Take a pencil'"],
        [("pen", "bolígrafo", "Write with a pen."), ("pencil", "lápiz", "Use a pencil for the test."), ("book", "libro", "Open your book to page 5."), ("desk", "escritorio", "Put the book on the desk."), ("chair", "silla", "Sit on the chair."), ("eraser", "goma de borrar", "I need an eraser.")],
        [("Take out a pen and a piece of paper.", "Following classroom instructions"), ("The book is on the desk.", "Describing location"), ("There are thirty chairs in the room.", "Counting classroom objects"), ("Open your book and go to page twelve.", "Teacher instruction")],
        "Describe your study area. Write 6 sentences: what objects are there, their colors, and how many. Example: 'I have two blue pens on my desk.'",
        "In American English: 'eraser'. In British English: 'rubber'. Both are correct — just know your context.",
        [("School Objects English Vocabulary", "7 E S L", "j6LvKqGTkSg"), ("Classroom Vocabulary in English", "Learn English with EnglishClass101.com", "RPHJ81CJbMI")]
    );

    private static string A2L1() => C(
        "In the Present Simple tense, verbs change their form ONLY for the third person singular (he, she, it). This is one of the most important rules in English grammar, covered extensively by Perfect English Grammar.",
        ["I/you/we/they + base verb: work, play, live", "He/she/it + verb + S: works, plays, lives", "Add -ES after -ch, -sh, -x, -o, -ss: watches, goes, fixes", "Consonant + Y → drop Y and add -IES: study → studies, carry → carries", "Irregular: have → has, be → is, do → does"],
        [("works", "trabaja", "She works at a hospital."), ("studies", "estudia", "He studies every evening."), ("watches", "mira/ve", "She watches TV after dinner."), ("goes", "va", "He goes to the gym on Mondays."), ("has", "tiene", "She has two children.")],
        [("She plays tennis every Saturday.", "Regular verb with -s"), ("He watches TV after dinner.", "Verb ending in -ch adds -es"), ("My sister studies medicine at university.", "Consonant+y → -ies"), ("The train arrives at platform 3.", "Third person in context")],
        "Write 6 sentences about what a family member or friend does in their daily routine. Use he/she with correct third person singular forms.",
        "The final -s/-es has 3 sounds: /s/ (works, eats), /z/ (plays, lives), /ɪz/ (watches, teaches — after -sh, -ch, -x, -s, -z).",
        [("Present Simple - Third Person Singular -s/-es", "Learn English with Papa English", "m3FfULhEcvE"), ("Present Simple Tense", "English Speeches", "v8_3WvX4Q6k")]
    );

    private static string A2L2() => C(
        "To make negatives and questions in Present Simple, we need a helper verb: DO for I/you/we/they and DOES for he/she/it. The main verb ALWAYS stays in its base form after don't/doesn't/do/does.",
        ["Negative: Subject + don't/doesn't + base verb", "He/she/it uses DOESN'T — not 'don't'", "Question: Do/Does + subject + base verb?", "After doesn't/does: main verb has NO -s (she doesn't work, does she work?)", "Short answers: Yes, she does. / No, she doesn't."],
        [("don't (do not)", "no + verbo", "I don't like coffee."), ("doesn't (does not)", "no + verbo (él/ella)", "She doesn't eat meat."), ("Do you...?", "¿Tú...?", "Do you speak Spanish?"), ("Does he/she...?", "¿Él/ella...?", "Does she work here?")],
        [("I don't like spicy food.", "Negative — I/you/we/they"), ("She doesn't speak French.", "Negative — he/she/it"), ("Do you have a car?", "Yes/No question"), ("Does he live nearby? — Yes, he does.", "Question with short answer")],
        "Write 3 negative sentences and 3 questions about your daily routine or lifestyle. Include at least one he/she example.",
        "'Doesn't' is pronounced DUZ-ent. In spoken English, contractions (don't, doesn't) are far more common than the full forms (do not, does not)."
    );

    private static string A2L3() => C(
        "Regular verbs in the Past Simple are formed by adding -ED to the base form. This form is the SAME for ALL subjects — I, you, he, she, we, they. Only ONE form to learn!",
        ["Base verb + ED: walk → walked, play → played, watch → watched", "Verb ending in -E: add only -D: live → lived, like → liked", "Short vowel + single consonant → double it: stop → stopped, plan → planned", "Consonant + Y → change to -IED: study → studied, try → tried", "Negative: didn't + base verb | Question: Did + subject + base verb?"],
        [("walked", "caminó/caminé", "I walked to school."), ("studied", "estudió/estudié", "She studied all night."), ("stopped", "paró/paré", "The car stopped suddenly."), ("lived", "vivió/viví", "They lived in Paris for a year.")],
        [("I walked to the park yesterday.", "Regular past — base + ed"), ("She studied for the exam last night.", "Y → ied"), ("They stopped at a café for coffee.", "Double consonant"), ("He didn't call me. / Did you finish?", "Negative and question forms")],
        "Write a short paragraph (5–7 sentences) about what you did yesterday or last weekend. Use at least 5 different regular past verbs.",
        "-ED has 3 sounds: /t/ after voiceless consonants (worked, stopped), /d/ after voiced sounds (played, called), /ɪd/ after t/d (wanted, needed)."
    );

    private static string A2L4() => C(
        "Many of the most common English verbs are irregular — they do NOT follow the -ED rule. Their past forms must be memorized. Negatives and questions still use 'did' + base verb.",
        ["Common: go→went, have→had, see→saw, come→came, give→gave", "More: eat→ate, drink→drank, write→wrote, read→read, say→said", "More: take→took, make→made, know→knew, think→thought, buy→bought", "Negatives: I didn't go, she didn't have (base verb!)", "Questions: Did you see? Did he come? (base verb!)"],
        [("went (go)", "fue/fui", "I went to the cinema last night."), ("had (have)", "tuvo/tuve", "She had a great time."), ("saw (see)", "vio/vi", "We saw a great film."), ("ate (eat)", "comió/comí", "They ate pizza for dinner."), ("wrote (write)", "escribió/escribí", "He wrote a long email.")],
        [("I went to the beach last summer.", "go → went"), ("She had coffee and toast for breakfast.", "have → had"), ("They saw the new film yesterday.", "see → saw"), ("He wrote her a letter but she didn't reply.", "write → wrote + negative")],
        "Write a paragraph about a memorable day or event. Use at least 6 different irregular past verbs. You can write about a birthday, trip, special meal, or a memorable moment.",
        "'Read' looks the same in present and past but sounds different: REED (present) vs RED (past). 'Thought' is pronounced THAWT — the 'ough' is silent."
    );

    private static string B1L1() => C(
        "The Present Perfect connects the past to NOW. We use it for experiences in your life, recent events, or past actions that still affect the present. Formed with: have/has + past participle. Perfect English Grammar and EngVid provide excellent explanations of this fundamental tense.",
        ["Form: have/has + past participle (worked, gone, eaten, seen...)", "USE 1 — Life experiences (ever/never): 'Have you ever been to Japan?'", "USE 2 — Recent actions (just/already/yet): 'She has just arrived'", "USE 3 — Unfinished situations: 'I have lived here for 5 years' (still true NOW)", "DO NOT use with specific finished time: say 'I saw him yesterday' — NOT 'I have seen him yesterday'"],
        [("ever", "alguna vez", "Have you ever eaten sushi?"), ("never", "nunca", "I have never seen snow."), ("just", "acaba de", "She has just called me."), ("already", "ya", "He has already finished."), ("yet", "todavía/aún", "Have you eaten yet? — Not yet.")],
        [("I have never eaten sushi.", "Life experience with 'never'"), ("Have you ever been to London?", "Life experience question"), ("She has just finished her homework.", "Recent action with 'just'"), ("They haven't arrived yet.", "Expected action with 'yet'")],
        "Write 6 Present Perfect sentences: 2 with 'ever/never', 2 with 'just/already', and 2 with 'yet'.",
        "In spoken English, 'have' contracts: I've, you've, she's, he's, we've, they've. 'I have never' → 'I've never'. These contractions are essential for natural speech.",
        [("Present Perfect Tense Explained", "Learn English with EnglishClass101.com", "dj-iEPfFqlI"), ("Present Perfect Grammar Lesson", "English Addict with Mr. Duncan", "fHYrBa-jaPo")]
    );

    private static string B1L2() => C(
        "'For' and 'since' both describe duration, but differently. FOR tells us the period of time; SINCE tells us the starting point. Both are used with the Present Perfect.",
        ["FOR + a period of time: for two hours, for a week, for six months, for ten years", "SINCE + a starting point: since Monday, since 2015, since I was a child", "Quick test: 'how long?' → FOR. 'Starting from when?' → SINCE", "Common error: 'I live here since 5 years' — wrong! Use Present Perfect: 'I have lived here for 5 years'"],
        [("for two hours", "durante dos horas", "I have been waiting for two hours."), ("for a long time", "desde hace mucho tiempo", "We haven't talked for a long time."), ("since Monday", "desde el lunes", "She has been ill since Monday."), ("since last year", "desde el año pasado", "He has worked here since last year.")],
        [("I have worked here for three years.", "FOR + duration"), ("She has been a teacher since 2018.", "SINCE + starting point"), ("He hasn't eaten since this morning.", "SINCE + time of day"), ("We have known each other for a very long time.", "FOR in a longer statement")],
        "Write 6 Present Perfect sentences: 3 with 'for' and 3 with 'since'. Talk about your hobbies, home, work, friendships, or habits.",
        "'Since' is pronounced SINTS (one syllable). 'For' in fast speech often sounds like 'fer' — that is normal and native."
    );

    private static string B1L3() => C(
        "The First Conditional describes real, possible situations in the future and their likely results. If the condition happens, the result will follow.",
        ["Structure: IF + Present Simple, WILL + base verb", "The IF clause can come first or second: 'If I study, I will pass' = 'I will pass if I study'", "Use a comma only when the IF clause comes first", "Can also use: might, may, can, could instead of will", "Compare: First = possible future; Second = unlikely/imaginary"],
        [("If ... will", "Si ... va a / irá", "If it rains, I will take an umbrella."), ("Unless", "A menos que / Si no", "Unless you hurry, you will miss the bus."), ("might / may", "podría / puede que", "If you study, you might pass.")],
        [("If it rains tomorrow, I will stay home.", "Basic first conditional"), ("You'll miss the bus if you don't leave now.", "IF clause at the end"), ("If she calls, tell her I'm busy.", "First conditional as instruction"), ("If you eat too much sugar, you might feel ill.", "Using 'might'")],
        "Write 5 first conditional sentences about your week ahead. Think about: the weather, your studies, your plans. Use 'will', 'might', and 'might not'.",
        "'Will' contracts in spoken English: I'll, you'll, she'll, he'll, we'll, they'll. 'I will go' → 'I'll go'. Practice until contractions feel natural."
    );

    private static string B1L4() => C(
        "The Second Conditional describes unreal, hypothetical, or unlikely situations. It imagines 'What if...?' scenarios that are not currently true.",
        ["Structure: IF + Past Simple, WOULD + base verb", "'If I were' is preferred to 'If I was' (formal writing)", "Compare: First = 'If I win the lottery' (possible) vs Second = 'If I won' (unlikely dream)", "Use for: advice ('If I were you, I would...'), imaginary situations", "'Would' contracts: I'd, you'd, she'd, he'd, we'd, they'd"],
        [("If I were", "Si yo fuera", "If I were taller, I'd play basketball."), ("If I had", "Si tuviera", "If I had more time, I would travel."), ("would", "haría/iría (condicional)", "I would live in Italy if I could."), ("If I were you", "Yo que tú / En tu lugar", "If I were you, I would apologize.")],
        [("If I had more money, I would travel the world.", "Imaginary situation"), ("She would be happier if she lived near the sea.", "Hypothetical preference"), ("If I were you, I would accept the job offer.", "Giving advice"), ("What would you do if you lost your phone?", "Second conditional question")],
        "Write 5 second conditional sentences: 2 imaginary 'what if' scenarios, 1 piece of advice using 'If I were you...', and 2 hypothetical preferences.",
        "'Would' is pronounced WOOD — the 'l' is silent. In fast speech it sounds just like 'wood'. Contractions: I'd, you'd, she'd — the 'd' is barely audible."
    );

    private static string B2L1() => C(
        "The passive voice shifts the focus from the person doing the action to the action itself. It is used when the agent is unknown, unimportant, or obvious.",
        ["Present passive: am/is/are + past participle", "Past passive: was/were + past participle", "Agent introduced with 'by': 'The novel was written by Hemingway'", "Omit 'by' when agent is unknown or unimportant: 'The window was broken'", "Active: 'Someone stole my bag.' → Passive: 'My bag was stolen.'"],
        [("is made", "es hecho/fabricado", "This car is made in Germany."), ("was written", "fue escrito", "The report was written yesterday."), ("are sold", "se venden", "These products are sold online."), ("is spoken", "se habla", "English is spoken worldwide.")],
        [("English is spoken all over the world.", "Present passive — general fact"), ("The letter was written by the CEO.", "Past passive — agent specified"), ("My phone was stolen on the subway.", "Past passive — unknown agent"), ("These shoes are made in Italy.", "Present passive — origin")],
        "Write 6 passive sentences: 3 in present passive and 3 in past passive. Write about products you use, famous buildings, historical events, or things that happened to you.",
        "In passive sentences, the past participle carries main stress: 'The letter WAS WRITten'. Practice the rhythm: was/were are unstressed, the participle is stressed."
    );

    private static string B2L2() => C(
        "Modals combine with passive voice to express obligation, recommendation, possibility, or permission without specifying who performs the action.",
        ["Structure: modal + BE + past participle (same for ALL subjects)", "Must be done, should be submitted, can be found, might be cancelled", "Compare active: 'You must submit it' → Passive: 'It must be submitted'", "Perfect modal passive: should have been done, could have been avoided", "Very common in formal writing, rules, instructions, and announcements"],
        [("must be + pp", "debe ser", "The form must be signed."), ("should be + pp", "debería ser", "Errors should be corrected."), ("can be + pp", "puede ser", "The meeting can be rescheduled."), ("might be + pp", "podría ser", "The event might be cancelled.")],
        [("All homework must be submitted by Friday.", "Obligation — must be"), ("Mistakes should be corrected before submission.", "Recommendation — should be"), ("The report can be found on the website.", "Possibility — can be"), ("The flight might be delayed due to weather.", "Uncertainty — might be")],
        "Write 5 passive sentences with modals. Write rules or guidelines for a real situation: workplace policy, school rules, or environmental regulations.",
        "'Should be' sounds like 'SHUD-bee'. 'Must be' sounds like 'MUST-bee'. The passive 'be' is unstressed — focus on the modal and participle."
    );

    private static string B2L3() => C(
        "Reported speech is used to report what someone said without quoting their exact words. Verb tenses 'shift back' (backshift), and pronouns and time expressions change.",
        ["Present Simple → Past Simple: 'I work' → he said he worked", "Present Continuous → Past Continuous: 'I'm working' → she said she was working", "Will → Would: 'I will call' → she said she would call", "Can → Could: 'I can help' → he said he could help", "Pronouns change: I → he/she, we → they, my → his/her"],
        [("said (that)", "dijo (que)", "She said that she was tired."), ("told me (that)", "me dijo (que)", "He told me that he knew the answer."), ("explained that", "explicó que", "She explained that she was busy."), ("mentioned that", "mencionó que", "He mentioned that he was leaving soon.")],
        [("Direct: 'I love English.' → She said she loved English.", "Present → Past"), ("Direct: 'We are leaving.' → They said they were leaving.", "Continuous → Past Continuous"), ("Direct: 'I will call you.' → He said he would call me.", "Will → Would"), ("Direct: 'I can swim.' → She said she could swim.", "Can → Could")],
        "Think of a recent conversation. Report 5 things that were said using 'said that', 'told me that', 'mentioned that', or 'explained that'.",
        "'Said' is pronounced SED (rhymes with 'bed') — not 'sayd'. 'Told' is TOHLD. These are the most common reporting verbs — correct pronunciation matters."
    );

    private static string B2L4() => C(
        "Reported questions do NOT use question word order (no inversion). They use statement order (subject + verb). Yes/No questions use 'if' or 'whether'; Wh- questions keep their question word.",
        ["Wh- questions: keep question word, use statement order", "'Where do you live?' → He asked me where I lived.", "Yes/No questions: use 'if' or 'whether'", "'Are you tired?' → She asked if/whether I was tired.", "No question marks! No inverted word order! Tense backshift still applies"],
        [("asked where/when/why/how", "preguntó dónde/cuándo/por qué/cómo", "She asked where I worked."), ("asked if / whether", "preguntó si", "He asked if I was ready."), ("wanted to know", "quería saber", "She wanted to know what time it was.")],
        [("'Where do you work?' → He asked me where I worked.", "Wh- question reported"), ("'Are you coming?' → She asked if I was coming.", "Yes/No → 'if'"), ("'Do you speak Spanish?' → She wanted to know whether I spoke Spanish.", "Using 'whether'")],
        "Write 5 reported questions. Imagine someone asked you these things at a job interview or on a first date. Use different reporting phrases.",
        "'Whether' is pronounced WETH-er — exactly like 'weather'. They are homophones. In writing: 'whether' for reported questions, 'weather' for climate."
    );

    private static string C1L1() => C(
        "Inversion places the auxiliary verb BEFORE the subject after certain negative or limiting adverbials. It creates strong emphasis and is a key feature of formal, literary, and sophisticated English.",
        ["Trigger words: Never, Rarely, Seldom, Hardly, Barely, Scarcely, No sooner, Not only, Not until, Only when, Little", "Structure: Negative adverb + AUXILIARY + subject + main verb", "'Never I have seen' is WRONG → 'Never have I seen' is CORRECT", "'Not only did she win, but she also broke the record.'", "Common in formal speeches, literary texts, journalism, and advanced writing"],
        [("Never have I...", "Nunca he...", "Never have I witnessed such courage."), ("Rarely does...", "Raramente...", "Rarely does he make a mistake."), ("Hardly had... when...", "Apenas había... cuando...", "Hardly had she arrived when it started raining."), ("Not only... but also...", "No solo... sino también...", "Not only did he lie, but he also stole.")],
        [("Never have I seen such dedication.", "Never + inversion"), ("Not only did she pass, but she got the highest score.", "Not only + inversion"), ("Hardly had I sat down when the phone rang.", "Hardly + had + inversion"), ("Rarely does the committee agree on anything.", "Rarely + does + inversion")],
        "Rewrite these 5 sentences using inversion for emphasis: 1) I have never worked so hard. 2) She rarely makes mistakes. 3) I had barely fallen asleep when the alarm went off. 4) He not only failed the test but also lost his scholarship. 5) You will only understand when you experience it yourself.",
        "Inversion creates a dramatic, authoritative tone in speech. Drop your pitch at the end rather than rising. 'Never HAVE I seen...' — stress the auxiliary."
    );

    private static string C1L2() => C(
        "Mixed conditionals combine different time frames. The most common type uses a past condition (3rd conditional) with a present result (2nd conditional), showing how a past event still affects the present.",
        ["Type A — Past condition, present result: IF + Past Perfect → WOULD + base verb", "'If I had studied medicine (past), I would be a doctor now (present).'", "Type B — Present condition, past result: IF + Past Simple → WOULD HAVE + past participle", "'If she were more organized (now), she would have finished on time (then).'", "These mark C1/C2 level proficiency — essential for advanced communication"],
        [("If I had + pp ... would + base", "Si hubiera... sería/estaría", "If I had moved there, I would speak the language now."), ("If I were ... would have + pp", "Si fuera... habría", "If I were braver, I would have spoken up.")],
        [("If I had taken that job, I would be living in New York now.", "Past condition → present result"), ("If she were more patient, she would have handled it better.", "Present trait → past result"), ("He would be a millionaire now if he hadn't sold those shares.", "Inverted order")],
        "Write 4 mixed conditional sentences about your own life. Think about: a past choice and how it affects your present; or a character trait you have and how it affected a past situation.",
        "'Would have' in fast speech sounds like 'would've' (WOOD-uv) or 'woulda'. 'Should have' → 'should've' (SHUD-uv). Essential for listening comprehension."
    );

    private static string C1L3() => C(
        "Academic and formal language uses precise vocabulary to convey complex ideas with nuance. At C1 level, you replace informal words with formal equivalents and use nominalizations for a sophisticated style.",
        ["Prefer formal: commence (start), obtain (get), demonstrate (show), require (need), endeavour (try)", "Nominalizations: 'analyze' → 'analysis', 'develop' → 'development', 'argue' → 'argument'", "Avoid contractions in formal writing: cannot (not can't), do not (not don't)", "Qualify claims with hedging: 'it appears that', 'evidence suggests', 'it is worth noting'", "Cohesion: furthermore, nevertheless, consequently, notwithstanding"],
        [("ubiquitous", "ubicuo / presente en todas partes", "Smartphones are now ubiquitous."), ("exacerbate", "agravar / empeorar", "The delay exacerbated the problem."), ("ameliorate", "mejorar / aliviar", "The policy aims to ameliorate inequality."), ("laconic", "lacónico / conciso", "His laconic reply revealed little."), ("verbose", "verboso / palabrero", "The report is verbose and needs editing.")],
        [("The study demonstrates a significant correlation between diet and health.", "Formal verb 'demonstrates'"), ("The situation was further exacerbated by the lack of resources.", "Formal adjective + nominalization"), ("The analysis reveals several key findings.", "Nominalization: analyze → analysis"), ("It is worth noting that these results may not be generalizable.", "Hedging + formal vocabulary")],
        "Rewrite these informal sentences in formal style: 1) The problem got worse because of the weather. 2) We need to look at this more carefully. 3) The results show that our idea was right. 4) They're trying to make things better for poor people. 5) Nobody knows why this happens.",
        "In formal spoken English (presentations, interviews), avoid 'gonna', 'wanna', 'kinda'. Say 'going to', 'want to', 'kind of'. Formal speech is clearer and more deliberate."
    );

    private static string C1L4() => C(
        "Collocations are word partnerships that native speakers use naturally. Idioms are fixed expressions with meanings different from their literal words. Mastering both makes your English genuinely fluent.",
        ["Strong verb collocations: MAKE a decision, DO homework, TAKE a risk, GIVE a presentation, REACH a conclusion", "Adjective collocations: deeply committed, highly unlikely, firmly believe, utterly exhausted", "Idioms: 'a blessing in disguise' = something good that seemed bad at first", "Idioms: 'burn bridges' = destroy relationships permanently, 'hit a deadlock' = reach a total impasse", "Collocations cannot be changed: 'make a decision' — NOT 'do a decision'"],
        [("photographic memory", "memoria fotográfica", "She has a photographic memory — recalls every detail."), ("beside yourself", "fuera de sí (con emoción)", "He was beside himself with excitement."), ("a blessing in disguise", "un mal que por bien viene", "Losing that job was a blessing in disguise."), ("hit a deadlock", "llegar a un punto muerto", "Negotiations hit a deadlock."), ("burn bridges", "quemar puentes", "Don't burn bridges — you may need them later.")],
        [("She gave an outstanding presentation that left everyone speechless.", "Collocation: give a presentation"), ("He was beside himself with joy when he passed the exam.", "Idiom: beside yourself"), ("Losing the contract turned out to be a blessing in disguise.", "Idiom in context"), ("The committee reached a deadlock and adjourned the meeting.", "Collocation + formal vocabulary")],
        "Write a short paragraph (6–8 sentences) about a challenging situation. Use at least 3 collocations and 2 idioms from this lesson. The situation can be real or invented.",
        "Idioms are spoken as fixed chunks — 'blessing in disguise' flows as one unit: BLESS-ing-in-dis-GIZE. Do not pause between words. The rhythm is key."
    );

    private static string C2L1() => C(
        "Register is the level of formality in language. C2 speakers switch effortlessly between formal, neutral, and informal registers. Understanding register means knowing when something sounds too formal, too casual, or just right.",
        ["Formal: 'I would like to enquire about...' / Informal: 'I want to ask about...'", "Subjunctive in formal writing: 'I suggest that he be present' (not 'is')", "Avoid filler words in formal contexts: 'basically', 'you know', 'like', 'sort of'", "Use hedging for academic precision: 'It appears that...', 'Evidence suggests...'", "Nominalizations elevate register: 'develop' → 'development', 'solve' → 'solution'"],
        [("solicit", "solicitar / pedir formalmente", "I would like to solicit your expert opinion."), ("ratify", "ratificar", "The treaty was ratified by all members."), ("promulgate", "promulgar", "The new law was promulgated last year."), ("ameliorate", "mejorar / aliviar", "Measures were taken to ameliorate the situation."), ("commence", "comenzar / iniciar", "The ceremony will commence at 9 AM.")],
        [("I would like to enquire about the position advertised on your website.", "Formal opening for a letter"), ("I suggest that the committee be given more time to deliberate.", "Subjunctive mood"), ("The board ratified the proposal with immediate effect.", "Formal vocabulary in a business context")],
        "Rewrite this casual email in formal register: 'Hey, I wanted to ask about your job. I think I'd be really good at it and I want to know more stuff about what it involves. Can you send me some info? Thanks.'",
        "In formal spoken English, your speech should be clear, measured, and deliberate. Maintain full vowels — do not reduce them as in casual speech."
    );

    private static string C2L2() => C(
        "At C2 level, you use complex idioms, cultural references, and nuanced vocabulary naturally. These expressions add texture and authenticity — not by translating from your language, but by thinking in English.",
        ["'Pyrrhic victory': a win so costly it resembles defeat — from King Pyrrhus of Epirus", "'Penultimate': second to last — NOT 'second ultimate'!", "'Cogent': clear, logical, convincing — a cogent argument, cogent reasoning", "'Circumlocution': using many words to avoid saying something directly", "'Equivocal': ambiguous, capable of multiple interpretations"],
        [("pyrrhic victory", "victoria pírrica", "Their market dominance was a pyrrhic victory."), ("penultimate", "penúltimo", "The penultimate episode was the best."), ("cogent", "convincente / sólido", "She presented a cogent case for reform."), ("circumlocution", "circunloquio / rodeos", "Stop the circumlocution — just say what you mean."), ("equivocal", "equívoco / ambiguo", "His equivocal answer raised more questions.")],
        [("The company's Pyrrhic victory left it with no resources to compete further.", "Pyrrhic victory in business"), ("In the penultimate chapter, the mystery is finally unraveled.", "Penultimate as literary term"), ("Her argument was so cogent that no one could find a flaw.", "Cogent in academic context"), ("His equivocal response only deepened suspicions.", "Equivocal in political context")],
        "Write a paragraph (7–9 sentences) using all five vocabulary items from this lesson in natural, connected prose. You can write about a fictional character, a historical event, or a scenario from your professional life.",
        "'Penultimate' is pen-UL-ti-mate (5 syllables, stress 2nd). 'Equivocal' is eh-KWIV-oh-kal (4 syllables, stress 2nd). 'Cogent' is KOH-jent. 'Circumlocution' is sir-kum-loh-KYOO-shun."
    );

    private static string C2L3() => C(
        "Hedging language makes claims less absolute, signalling academic caution and intellectual honesty. It is indispensable in research writing, academic essays, and any context where absolute certainty is impossible.",
        ["Hedging verbs: suggest, indicate, appear, seem, tend, imply, point to", "Modal hedges: may, might, could, would (possibility), should (recommendation)", "Adverb hedges: possibly, probably, apparently, seemingly, generally, typically", "Phrase hedges: 'It could be argued that...', 'Evidence suggests that...', 'It appears that...'", "Over-hedging weakens writing — use hedges purposefully, not constantly"],
        [("It could be argued that", "Se podría argumentar que", "It could be argued that inequality drives conflict."), ("Evidence suggests", "La evidencia sugiere", "Evidence suggests a causal link."), ("It appears / It seems", "Parece que", "It appears that demand is declining."), ("tend to", "tiende a", "Students tend to perform better with feedback."), ("ostensibly", "aparentemente / en apariencia", "Ostensibly, the policy aims to reduce poverty.")],
        [("The data suggest a possible correlation between sleep and productivity.", "Hedging with 'suggest'"), ("It could be argued that economic inequality exacerbates social tensions.", "Phrase hedge"), ("The results appear to indicate a significant improvement.", "Hedge with 'appear'"), ("Ostensibly, the measure was introduced for public safety reasons.", "Adverb hedge")],
        "Rewrite these definitive sentences with appropriate hedging: 1) Climate change causes wars. 2) Social media makes teenagers depressed. 3) Early education determines success in life. 4) Exercise cures anxiety. 5) Technology will replace all human workers.",
        "'Ostensibly' is os-TEN-si-blee (5 syllables). 'Apparently' is a-PAR-ent-lee (4 syllables). In speech, pause slightly before a hedge — it signals deliberate, careful thought."
    );

    private static string C2L4() => C(
        "Critical analysis means examining arguments carefully: identifying strengths, exposing logical flaws, and evaluating evidence. At C2, you both construct and deconstruct sophisticated arguments.",
        ["Non sequitur: conclusion does not logically follow from the premises", "Ad hominem: attacking the speaker's character instead of their argument", "Straw man: misrepresenting an opponent's argument to make it easier to attack", "False dichotomy: presenting only two options when others exist", "Equivocation: using an ambiguous term with different meanings in the same argument"],
        [("non sequitur", "conclusión que no se sigue", "That is a non sequitur — it does not follow."), ("ad hominem", "ataque personal", "That ad hominem adds nothing to the debate."), ("cogent", "sólido / convincente", "The paper makes a cogent, well-evidenced argument."), ("equivocal", "equívoco / ambiguo", "The data is equivocal on this point."), ("circumlocution", "circunloquio / rodeos", "His circumlocution masked a lack of substance.")],
        [("The argument is cogent and supported by robust empirical evidence.", "Positive critical evaluation"), ("This is a classic non sequitur — growth does not automatically reduce poverty.", "Identifying a logical flaw"), ("The author resorts to ad hominem attacks rather than engaging with the argument.", "Identifying ad hominem"), ("The data remain equivocal, pointing in different directions depending on methodology.", "Acknowledging uncertainty")],
        "Find a news article or opinion piece. Write a critical analysis paragraph (8–10 sentences) identifying at least 2 argumentative strengths and 2 logical weaknesses. Use vocabulary from this lesson.",
        "'Non sequitur' is Latin: non-SEK-wi-ter. 'Ad hominem' is ad-HOM-in-em. Academic Latin phrases are pronounced the English way, not classical Latin. Both 3 syllables, stress on 2nd."
    );

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string C(
        string explanation,
        string[] keyPoints,
        (string w, string t, string ex)[]? vocab = null,
        (string s, string n)[]? examples = null,
        string? writingPrompt = null,
        string? pronunciationTip = null,
        (string title, string channel, string videoId)[]? videos = null) =>
        JsonSerializer.Serialize(new
        {
            explanation,
            keyPoints,
            vocabulary = vocab?.Select(v => new { word = v.w, translation = v.t, example = v.ex }).ToArray(),
            examples = examples?.Select(e => new { sentence = e.s, note = e.n }).ToArray(),
            writingPrompt,
            pronunciationTip,
            videos = videos?.Select(v => new { title = v.title, channel = v.channel, videoId = v.videoId }).ToArray()
        });

    private static Level NewLevel(string id, string code, string name, int order, string? unlock = null) =>
        new() { Id = G(id), Code = code, Name = name, Order = order, UnlockRequirement = unlock };

    private static Module NewModule(Guid id, Guid levelId, string title, string desc, int order, int hours) =>
        new() { Id = id, LevelId = levelId, Title = title, Description = desc, Order = order, EstimatedHours = hours };

    private static Lesson NewLesson(Guid id, Guid moduleId, string title, SkillType skill, int order, string? content = null) =>
        new() { Id = id, ModuleId = moduleId, Title = title, SkillType = skill, Order = order, ContentJson = content };

    private static Assessment NewAssessment(string id, Guid levelId, string title, int passScore, int minutes) =>
        new() { Id = G(id), ScopeType = AssessmentScopeType.Level, ScopeId = levelId, Title = title, PassScore = passScore, TimeLimitMinutes = minutes, CEFRAligned = true };

    private static Guid G(string s) => Guid.Parse(s);

    private static (ExerciseType, string, string, string[], string) MC(
        string prompt, string correct, string o2, string o3, string o4, string tags) =>
        (ExerciseType.MultipleChoice, prompt, correct, [o2, o3, o4], tags);

    private static (ExerciseType, string, string, string[], string) FB(
        string prompt, string correct, string tags) =>
        (ExerciseType.FillBlank, prompt, correct, [], tags);

    private static void AddExercises(
        ApplicationDbContext ctx, Guid lessonId, string idPrefix,
        params (ExerciseType Type, string Prompt, string Correct, string[] Others, string Tags)[] exercises)
    {
        for (int i = 0; i < exercises.Length; i++)
        {
            var (type, prompt, correct, others, tags) = exercises[i];
            var exId = G($"{idPrefix}{i + 1:00}00-0000-0000-0000-000000000001");
            ctx.Exercises.Add(new Exercise
            {
                Id = exId, LessonId = lessonId, Type = type, Prompt = prompt,
                CorrectAnswer = correct, Difficulty = 1, Tags = tags,
                Source = ExerciseSource.Manual, IsValidated = true
            });

            if (type == ExerciseType.MultipleChoice)
            {
                var all = new[] { correct }.Concat(others).ToArray();
                var shuffled = all.Skip(i % all.Length).Concat(all.Take(i % all.Length)).ToArray();
                for (int j = 0; j < shuffled.Length; j++)
                    ctx.ExerciseOptions.Add(new ExerciseOption
                    {
                        Id = G($"{idPrefix}{i + 1:00}{j + 1:0}0-0000-0000-0000-000000000001"),
                        ExerciseId = exId, Text = shuffled[j],
                        IsCorrect = shuffled[j] == correct,
                        Explanation = shuffled[j] == correct ? "Correct!" : null
                    });
            }
        }
    }
}
