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
        var m3 = G("a1030000-0000-0000-0000-000000000003");
        var m4 = G("a1040000-0000-0000-0000-000000000004");

        ctx.Modules.Add(NewModule(m1, levelId, "Module 1: Greetings & am/is/are", "Learn basic greetings and the verb 'to be' - the foundation of English.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Module 2: Present Simple Basics", "Master basic present simple for everyday routines.", 2, 2));
        ctx.Modules.Add(NewModule(m3, levelId, "Module 3: Present Continuous", "Learn to describe what's happening right now.", 3, 2));
        ctx.Modules.Add(NewModule(m4, levelId, "Module 4: Simple Questions & Negatives", "Form basic questions and negative sentences.", 4, 2));

        var l1 = G("a1b10000-0000-0000-0000-000000000001");
        var l2 = G("a1b20000-0000-0000-0000-000000000002");
        var l3 = G("a1b30000-0000-0000-0000-000000000003");
        var l4 = G("a1b40000-0000-0000-0000-000000000004");
        var l5 = G("a1b50000-0000-0000-0000-000000000005");
        var l6 = G("a1b60000-0000-0000-0000-000000000006");
        var l7 = G("a1b70000-0000-0000-0000-000000000007");
        var l8 = G("a1b80000-0000-0000-0000-000000000008");

        // Module 1: Greetings & am/is/are
        ctx.Lessons.Add(NewLesson(l1, m1, "Basic Greetings & Hello", SkillType.Listening, 1, A1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "I am / You are / He is", SkillType.Reading, 2, A1L2()));

        // Module 2: Present Simple Basics
        ctx.Lessons.Add(NewLesson(l3, m2, "I work / You work / He works", SkillType.Reading, 1, A1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Everyday Actions & Habits", SkillType.Writing, 2, A1L4()));

        // Module 3: Present Continuous
        ctx.Lessons.Add(NewLesson(l5, m3, "What are you doing right now?", SkillType.Listening, 1, A1L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "am/is/are + -ing", SkillType.Writing, 2, A1L6()));

        // Module 4: Simple Questions & Negatives
        ctx.Lessons.Add(NewLesson(l7, m4, "Do you...? / Are you...?", SkillType.Reading, 1, A1L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "I don't / He isn't / They aren't", SkillType.Writing, 2, A1L8()));

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

        AddExercises(ctx, l5, "a1e5",
            MC("Your mother's brother is your ___.", "uncle", "cousin", "father", "nephew", "family"),
            MC("A girl's female sibling is her ___.", "sister", "mother", "cousin", "aunt", "family"),
            MC("Your grandparents are your ___ parents.", "parents'", "mothers", "fathers", "grandparents", "family"),
            FB("I have two ___ and one sister. (male siblings)", "brothers", "family"));

        AddExercises(ctx, l6, "a1e6",
            MC("Which is the correct sentence?", "She is a teacher.", "She are a teacher.", "She am a teacher.", "She be a teacher.", "pronouns,to-be"),
            MC("Complete: I ___ a student.", "am", "are", "is", "be", "pronouns,to-be"),
            MC("What is the contraction of 'you are'?", "you're", "youre", "your", "yore", "pronouns,contractions"),
            FB("They ___ very happy. (use 'to be')", "are", "pronouns,to-be"));

        AddExercises(ctx, l7, "a1e7",
            MC("Which day comes after Tuesday?", "Wednesday", "Thursday", "Monday", "Friday", "time,days"),
            MC("How many days are in a week?", "seven", "five", "six", "eight", "time,days"),
            MC("Which month has only 28 days (or 29 in a leap year)?", "February", "January", "March", "April", "time,months"),
            FB("I go to school on ___. (first day of work week)", "Monday", "time,days"));

        AddExercises(ctx, l8, "a1e8",
            MC("Which is a drink?", "coffee", "bread", "carrot", "potato", "food,vocabulary"),
            MC("What is the first meal of the day?", "breakfast", "lunch", "dinner", "supper", "food,meals"),
            MC("Which is a vegetable?", "carrot", "apple", "orange", "banana", "food,vegetables"),
            FB("We have ___ and vegetables for dinner.", "rice", "food,vocabulary"));
    }

    private static void BuildA2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("a2010000-0000-0000-0000-000000000001");
        var m2 = G("a2020000-0000-0000-0000-000000000002");
        var m3 = G("a2030000-0000-0000-0000-000000000003");
        var m4 = G("a2040000-0000-0000-0000-000000000004");
        var m5 = G("a2050000-0000-0000-0000-000000000005");
        var m6 = G("a2060000-0000-0000-0000-000000000006");
        var m7 = G("a2070000-0000-0000-0000-000000000007");
        var m8 = G("a2080000-0000-0000-0000-000000000008");

        ctx.Modules.Add(NewModule(m1, levelId, "Module 1: Past Simple (Essential Grammar Units 11-12)", "Tell stories about finished events with regular and irregular verbs.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Module 2: Past Continuous (Units 13-14)", "Describe actions that were happening in the past.", 2, 2));
        ctx.Modules.Add(NewModule(m3, levelId, "Module 3: Present Perfect (Units 15-20)", "Connect past experiences to the present with have/has.", 3, 2));
        ctx.Modules.Add(NewModule(m4, levelId, "Module 4: Modals: can/could (Units 29-30)", "Express ability, permission, and possibility.", 4, 2));
        ctx.Modules.Add(NewModule(m5, levelId, "Module 5: Modals: should/must (Units 31-32)", "Give advice and express obligation.", 5, 2));
        ctx.Modules.Add(NewModule(m6, levelId, "Module 6: Comparatives & Superlatives (Units 33-36)", "Compare things and identify the best/worst.", 6, 2));
        ctx.Modules.Add(NewModule(m7, levelId, "Module 7: Future: going to (Units 25-26)", "Express plans and intentions for the future.", 7, 2));
        ctx.Modules.Add(NewModule(m8, levelId, "Module 8: Future: will (Units 27-28)", "Make predictions and decisions with 'will'.", 8, 2));

        var l1 = G("a2b10000-0000-0000-0000-000000000001");
        var l2 = G("a2b20000-0000-0000-0000-000000000002");
        var l3 = G("a2b30000-0000-0000-0000-000000000003");
        var l4 = G("a2b40000-0000-0000-0000-000000000004");
        var l5 = G("a2b50000-0000-0000-0000-000000000005");
        var l6 = G("a2b60000-0000-0000-0000-000000000006");
        var l7 = G("a2b70000-0000-0000-0000-000000000007");
        var l8 = G("a2b80000-0000-0000-0000-000000000008");
        var l9 = G("a2b90000-0000-0000-0000-000000000009");
        var l10 = G("a2b10000-0000-0000-0000-000000000010");
        var l11 = G("a2b11000-0000-0000-0000-000000000011");
        var l12 = G("a2b12000-0000-0000-0000-000000000012");
        var l13 = G("a2b13000-0000-0000-0000-000000000013");
        var l14 = G("a2b14000-0000-0000-0000-000000000014");
        var l15 = G("a2b15000-0000-0000-0000-000000000015");
        var l16 = G("a2b16000-0000-0000-0000-000000000016");

        // Module 1: Past Simple
        ctx.Lessons.Add(NewLesson(l1, m1, "Regular verbs: worked, played, studied", SkillType.Reading, 1, A2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Irregular verbs: went, saw, ate, drank", SkillType.Writing, 2, A2L2()));

        // Module 2: Past Continuous
        ctx.Lessons.Add(NewLesson(l3, m2, "I was working / She was eating", SkillType.Reading, 1, A2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Past Continuous questions and negatives", SkillType.Writing, 2, A2L4()));

        // Module 3: Present Perfect
        ctx.Lessons.Add(NewLesson(l5, m3, "I have worked / She has eaten", SkillType.Reading, 1, A2L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Have you ever? / How long have you...?", SkillType.Writing, 2, A2L6()));

        // Module 4: Modals: can/could
        ctx.Lessons.Add(NewLesson(l7, m4, "Can / Can't - ability and permission", SkillType.Reading, 1, A2L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Could - past ability and polite requests", SkillType.Writing, 2, A2L8()));

        // Module 5: Modals: should/must
        ctx.Lessons.Add(NewLesson(l9, m5, "Should / Shouldn't - advice and opinions", SkillType.Reading, 1, A2L9()));
        ctx.Lessons.Add(NewLesson(l10, m5, "Must / Mustn't - obligation and prohibition", SkillType.Writing, 2, A2L10()));

        // Module 6: Comparatives & Superlatives
        ctx.Lessons.Add(NewLesson(l11, m6, "Comparative adjectives: bigger, more beautiful", SkillType.Reading, 1, A2L11()));
        ctx.Lessons.Add(NewLesson(l12, m6, "Superlative adjectives: the biggest, the most beautiful", SkillType.Writing, 2, A2L12()));

        // Module 7: Future: going to
        ctx.Lessons.Add(NewLesson(l13, m7, "Plans and intentions: I'm going to...", SkillType.Reading, 1, A2L13()));
        ctx.Lessons.Add(NewLesson(l14, m7, "Are you going to...? Future questions", SkillType.Writing, 2, A2L14()));

        // Module 8: Future: will
        ctx.Lessons.Add(NewLesson(l15, m8, "Predictions: It will rain / I will help", SkillType.Reading, 1, A2L15()));
        ctx.Lessons.Add(NewLesson(l16, m8, "Promises and decisions: I will do it", SkillType.Writing, 2, A2L16()));

        // Module 1: Past Simple
        AddExercises(ctx, l1, "a2e1",
            MC("Yesterday I ___ to the cinema.", "went", "go", "going", "goes", "past-simple,irregular"),
            MC("She ___ a beautiful dress last week.", "wore", "wear", "wearing", "wears", "past-simple,irregular"),
            MC("We ___ an excellent film.", "saw", "see", "seeing", "sees", "past-simple,irregular"),
            FB("He ___ his keys. (lose)", "lost", "past-simple,regular"));

        AddExercises(ctx, l2, "a2e2",
            MC("___ she go to school yesterday?", "Did", "Does", "Do", "Will", "past-simple,questions"),
            MC("What ___ you do last weekend?", "did", "do", "does", "will", "past-simple,questions"),
            MC("I ___ not finish my homework.", "did", "do", "does", "will", "past-simple,negatives"),
            FB("They ___ at home yesterday. (past of be)", "were", "past-simple"));

        // Module 2: Past Continuous
        AddExercises(ctx, l3, "a2e3",
            MC("I ___ sleeping when you called.", "was", "were", "am", "be", "past-continuous"),
            MC("She ___ a book all afternoon.", "was reading", "were reading", "am reading", "reads", "past-continuous"),
            MC("They ___ football when it rained.", "were playing", "was playing", "am playing", "plays", "past-continuous"),
            FB("He ___ TV when I arrived. (watch)", "was watching", "past-continuous"));

        AddExercises(ctx, l4, "a2e4",
            MC("What ___ you ___ at 3 o'clock?", "were-doing", "was doing", "am doing", "are do", "past-continuous"),
            MC("___ you sleeping when the phone rang?", "Were", "Was", "Are", "Am", "past-continuous,questions"),
            MC("She ___ not studying when we visited.", "was", "were", "is", "am", "past-continuous,negatives"),
            FB("___ he working yesterday afternoon?", "Was", "past-continuous,questions"));

        // Module 3: Present Perfect
        AddExercises(ctx, l5, "a2e5",
            MC("I ___ lived in London for five years.", "have", "has", "am", "was", "present-perfect"),
            MC("She ___ never eaten sushi.", "has", "have", "is", "was", "present-perfect,negatives"),
            MC("They ___ finished their work.", "have", "has", "are", "were", "present-perfect"),
            FB("He ___ gone to Paris. (perfect)", "has", "present-perfect"));

        AddExercises(ctx, l6, "a2e6",
            MC("___ you ever been to Italy?", "Have", "Has", "Are", "Do", "present-perfect,questions"),
            MC("How long ___ you lived here?", "have", "has", "are", "do", "present-perfect"),
            MC("I ___ just finished my breakfast.", "have", "has", "am", "was", "present-perfect,just"),
            FB("___ she arrived yet?", "Has", "present-perfect,questions"));

        // Module 4: Modals: can/could
        AddExercises(ctx, l7, "a2e7",
            MC("I ___ speak three languages.", "can", "can't", "could", "cans", "modal-can"),
            MC("She ___ swim very well.", "can", "cans", "cannot", "can't", "modal-can"),
            MC("___ you drive a car?", "Can", "Can't", "Could", "Cans", "modal-can,questions"),
            FB("They ___ play the guitar. (ability)", "can", "modal-can"));

        AddExercises(ctx, l8, "a2e8",
            MC("When I was young, I ___ climb trees easily.", "could", "can", "cans", "couldn't", "modal-could"),
            MC("___ you ride a bike when you were five?", "Could", "Can", "Would", "Did", "modal-could,questions"),
            MC("She ___ speak French in her youth.", "could", "can", "cans", "could't", "modal-could"),
            FB("He ___ not dance well in the past. (ability)", "could", "modal-could"));

        // Module 5: Modals: should/must
        AddExercises(ctx, l9, "a2e9",
            MC("You ___ eat more vegetables.", "should", "must", "can", "could", "modal-should"),
            MC("She ___ see a doctor about her cough.", "should", "can", "could", "will", "modal-should"),
            MC("You ___ not stay up too late.", "should", "must", "can", "will", "modal-should,negatives"),
            FB("I ___ study harder for the exam. (advice)", "should", "modal-should"));

        AddExercises(ctx, l10, "a2e10",
            MC("You ___ wear a seatbelt in the car.", "must", "should", "can", "could", "modal-must"),
            MC("Students ___ not use their phones in class.", "must", "should", "can", "will", "modal-must,negatives"),
            MC("___ I wear formal clothes?", "Must", "Should", "Can", "Will", "modal-must,questions"),
            FB("You ___ arrive on time. (obligation)", "must", "modal-must"));

        // Module 6: Comparatives & Superlatives
        AddExercises(ctx, l11, "a2e11",
            MC("This book is ___ than that one.", "better", "best", "good", "well", "comparatives"),
            MC("She is ___ than her brother.", "taller", "tallest", "tall", "more tall", "comparatives"),
            MC("This car is ___ expensive ___ that one.", "more-than", "more-as", "as-as", "than-as", "comparatives"),
            FB("My house is ___ mine. (big)", "bigger", "comparatives"));

        AddExercises(ctx, l12, "a2e12",
            MC("This is ___ film I've ever seen.", "the best", "better", "best", "more best", "superlatives"),
            MC("She is ___ student in the class.", "the tallest", "taller", "tallest", "more tall", "superlatives"),
            MC("It was ___ day of my life.", "the best", "best", "better", "the better", "superlatives"),
            FB("This is ___ beach in the country. (beautiful)", "the most beautiful", "superlatives"));

        // Module 7: Future: going to
        AddExercises(ctx, l13, "a2e13",
            MC("I ___ going to visit my family next week.", "am", "is", "are", "be", "future-going-to"),
            MC("She ___ going to study medicine.", "is", "am", "are", "be", "future-going-to"),
            MC("They ___ going to buy a new house.", "are", "am", "is", "be", "future-going-to"),
            FB("We ___ going to leave tomorrow. (future)", "are", "future-going-to"));

        AddExercises(ctx, l14, "a2e14",
            MC("___ you going to the party tonight?", "Are", "Is", "Am", "Be", "future-going-to,questions"),
            MC("What ___ she going to do tomorrow?", "is", "am", "are", "be", "future-going-to,questions"),
            MC("I ___ not going to watch that film.", "am", "is", "are", "be", "future-going-to,negatives"),
            FB("___ they going to arrive soon?", "Are", "future-going-to"));

        // Module 8: Future: will
        AddExercises(ctx, l15, "a2e15",
            MC("I think it ___ rain tomorrow.", "will", "going to", "shall", "will be", "future-will,predictions"),
            MC("She ___ help you with that problem.", "will", "going to", "shall", "would", "future-will"),
            MC("They ___ not come to the meeting.", "will", "going to", "shall", "would", "future-will,negatives"),
            FB("He ___ finish his project by Friday. (promise)", "will", "future-will"));

        AddExercises(ctx, l16, "a2e16",
            MC("___ you help me with this?", "Will", "Going to", "Shall", "Would", "future-will,questions"),
            MC("I ___ do the shopping tomorrow.", "will", "going to", "shall", "would", "future-will"),
            MC("___ you come to the party?", "Will", "Going to", "Shall", "Would", "future-will,questions"),
            FB("She ___ not forget about the appointment. (promise)", "will", "future-will"));
    }

    private static void BuildB1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("b1010000-0000-0000-0000-000000000001");
        var m2 = G("b1020000-0000-0000-0000-000000000002");
        var m3 = G("b1030000-0000-0000-0000-000000000003");
        var m4 = G("b1040000-0000-0000-0000-000000000004");
        var m5 = G("b1050000-0000-0000-0000-000000000005");
        var m6 = G("b1060000-0000-0000-0000-000000000006");
        var m7 = G("b1070000-0000-0000-0000-000000000007");
        var m8 = G("b1080000-0000-0000-0000-000000000008");

        ctx.Modules.Add(NewModule(m1, levelId, "Module 1: Present Perfect Continuous (Units 1-3)", "Connect ongoing past actions to the present.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Module 2: Past Perfect & Past Perfect Continuous (Units 4-10)", "Master complex past tenses for narrative.", 2, 2));
        ctx.Modules.Add(NewModule(m3, levelId, "Module 3: Future Continuous (Units 19-21)", "Describe actions happening at a future moment.", 3, 2));
        ctx.Modules.Add(NewModule(m4, levelId, "Module 4: Future Perfect (Units 22-25)", "Express completion before a future time.", 4, 2));
        ctx.Modules.Add(NewModule(m5, levelId, "Module 5: Modals 1 - Ability & Permission (Units 26-30)", "Can, could, may, might for ability and possibility.", 5, 2));
        ctx.Modules.Add(NewModule(m6, levelId, "Module 6: Modals 2 - Obligation & Advice (Units 31-37)", "Should, must, have to, need to for necessity.", 6, 2));
        ctx.Modules.Add(NewModule(m7, levelId, "Module 7: Conditional Sentences (Units 38-41)", "If clauses and wish for hypothetical situations.", 7, 2));
        ctx.Modules.Add(NewModule(m8, levelId, "Module 8: Passive Voice & Reported Speech (Units 42-48)", "Shift focus and report others' words accurately.", 8, 2));

        var l1 = G("b1b10000-0000-0000-0000-000000000001");
        var l2 = G("b1b20000-0000-0000-0000-000000000002");
        var l3 = G("b1b30000-0000-0000-0000-000000000003");
        var l4 = G("b1b40000-0000-0000-0000-000000000004");
        var l5 = G("b1b50000-0000-0000-0000-000000000005");
        var l6 = G("b1b60000-0000-0000-0000-000000000006");
        var l7 = G("b1b70000-0000-0000-0000-000000000007");
        var l8 = G("b1b80000-0000-0000-0000-000000000008");
        var l9 = G("b1b90000-0000-0000-0000-000000000009");
        var l10 = G("b1b10000-0000-0000-0000-000000000010");
        var l11 = G("b1b11000-0000-0000-0000-000000000011");
        var l12 = G("b1b12000-0000-0000-0000-000000000012");
        var l13 = G("b1b13000-0000-0000-0000-000000000013");
        var l14 = G("b1b14000-0000-0000-0000-000000000014");
        var l15 = G("b1b15000-0000-0000-0000-000000000015");
        var l16 = G("b1b16000-0000-0000-0000-000000000016");

        // Module 1: Present Perfect Continuous
        ctx.Lessons.Add(NewLesson(l1, m1, "How long have you been doing this?", SkillType.Reading, 1, B1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Present Perfect Continuous: emphasis on duration", SkillType.Writing, 2, B1L2()));

        // Module 2: Past Perfect & Past Perfect Continuous
        ctx.Lessons.Add(NewLesson(l3, m2, "Past Perfect: had done / had not done", SkillType.Reading, 1, B1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Past Perfect Continuous: had been doing", SkillType.Writing, 2, B1L4()));

        // Module 3: Future Continuous
        ctx.Lessons.Add(NewLesson(l5, m3, "Future Continuous: will be doing", SkillType.Reading, 1, B1L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Future Continuous questions and negatives", SkillType.Writing, 2, B1L6()));

        // Module 4: Future Perfect
        ctx.Lessons.Add(NewLesson(l7, m4, "Future Perfect: will have done", SkillType.Reading, 1, B1L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Future Perfect Continuous: will have been doing", SkillType.Writing, 2, B1L8()));

        // Module 5: Modals 1 (Ability & Permission)
        ctx.Lessons.Add(NewLesson(l9, m5, "Can, Could, Be able to: ability in different times", SkillType.Reading, 1, B1L9()));
        ctx.Lessons.Add(NewLesson(l10, m5, "May, Might: permission and possibility", SkillType.Writing, 2, B1L10()));

        // Module 6: Modals 2 (Obligation & Advice)
        ctx.Lessons.Add(NewLesson(l11, m6, "Should, Ought to, Must: obligation and advice", SkillType.Reading, 1, B1L11()));
        ctx.Lessons.Add(NewLesson(l12, m6, "Have to, Must, Need to: necessity and obligation", SkillType.Writing, 2, B1L12()));

        // Module 7: Conditional Sentences
        ctx.Lessons.Add(NewLesson(l13, m7, "First & Second Conditional: real and hypothetical", SkillType.Reading, 1, B1L9()));
        ctx.Lessons.Add(NewLesson(l14, m7, "Third Conditional & Wish: unreal past situations", SkillType.Writing, 2, B1L10()));

        // Module 8: Passive Voice & Reported Speech
        ctx.Lessons.Add(NewLesson(l15, m8, "Passive Voice: transform active to passive", SkillType.Reading, 1, B1L11()));
        ctx.Lessons.Add(NewLesson(l16, m8, "Reported Speech: indirect statements and questions", SkillType.Writing, 2, B1L12()));

        // Module 1: Present Perfect Continuous
        AddExercises(ctx, l1, "b1e1",
            MC("How long ___ you ___ here?", "have-been working", "are working", "have worked", "work", "present-perfect-continuous,duration"),
            MC("She ___ her book for three hours.", "has been writing", "has written", "writes", "is write", "present-perfect-continuous"),
            MC("They ___ football all afternoon.", "have been playing", "have played", "are playing", "plays", "present-perfect-continuous"),
            FB("I ___ English for five years. (study)", "have been studying", "present-perfect-continuous"));

        AddExercises(ctx, l2, "b1e2",
            MC("How long have you ___ here?", "been working", "worked", "work", "are working", "present-perfect-continuous"),
            MC("She ___ television all evening.", "has been watching", "has watched", "watches", "is watching", "present-perfect-continuous"),
            MC("We ___ for the bus for 20 minutes.", "have been waiting", "have waited", "wait", "are waiting", "present-perfect-continuous"),
            FB("___ you ___ well lately? (sleep)", "Have been sleeping", "present-perfect-continuous"));

        // Module 2: Past Perfect & Past Perfect Continuous
        AddExercises(ctx, l3, "b1e3",
            MC("Before they arrived, I ___ the dishes.", "had finished", "finished", "have finished", "was finishing", "past-perfect"),
            MC("She ___ that book before.", "had never read", "has never read", "never read", "was reading", "past-perfect"),
            MC("By the time he called, we ___ already ___.", "had-left", "left", "have left", "was leaving", "past-perfect"),
            FB("When she arrived, he ___ for an hour. (wait)", "had been waiting", "past-perfect-continuous"));

        AddExercises(ctx, l4, "b1e4",
            MC("He ___ there for five years before he moved.", "had been living", "lived", "has lived", "was living", "past-perfect-continuous"),
            MC("By noon, she ___ on the project for three hours.", "had been working", "worked", "has worked", "was working", "past-perfect-continuous"),
            MC("After we ___ for an hour, the bus finally came.", "had been waiting", "waited", "have waited", "were waiting", "past-perfect-continuous"),
            FB("When I arrived, they ___ dinner for ten minutes. (eat)", "had been eating", "past-perfect-continuous"));

        // Module 3: Future Continuous
        AddExercises(ctx, l5, "b1e5",
            MC("This time next week, I ___ on the beach.", "will be relaxing", "will relax", "am relaxing", "relax", "future-continuous"),
            MC("At 8 PM, she ___ to the radio.", "will be listening", "will listen", "is listening", "listens", "future-continuous"),
            MC("When you call, I ___ my homework.", "will be doing", "will do", "am doing", "do", "future-continuous"),
            FB("Next year, they ___ in a new house. (live)", "will be living", "future-continuous"));

        AddExercises(ctx, l6, "b1e6",
            MC("___ you ___ the match tomorrow at 3 PM?", "Will-be watching", "Will watch", "Are watching", "Watch", "future-continuous,questions"),
            MC("What ___ you ___ at this time next week?", "will-be doing", "will do", "are doing", "do", "future-continuous,questions"),
            MC("At midnight, we ___ the New Year.", "will be celebrating", "will celebrate", "are celebrating", "celebrate", "future-continuous"),
            FB("I ___ about you. (think)", "will be thinking", "future-continuous"));

        // Module 4: Future Perfect
        AddExercises(ctx, l7, "b1e7",
            MC("By next month, I ___ this project.", "will have completed", "will complete", "have completed", "am completing", "future-perfect"),
            MC("When you arrive, I ___ dinner.", "will have cooked", "will cook", "have cooked", "am cooking", "future-perfect"),
            MC("By the end of the year, she ___ her degree.", "will have finished", "will finish", "has finished", "is finishing", "future-perfect"),
            FB("By 2030, we ___ in this house for 20 years. (live)", "will have lived", "future-perfect"));

        AddExercises(ctx, l8, "b1e8",
            MC("By next year, I ___ here for five years.", "will have been working", "will work", "have worked", "am working", "future-perfect-continuous"),
            MC("When he retires, he ___ for 40 years.", "will have been working", "will work", "has worked", "is working", "future-perfect-continuous"),
            MC("By the time you finish reading, I ___ this book.", "will have been reading", "will read", "have read", "am reading", "future-perfect-continuous"),
            FB("By summer, we ___ on this project for six months. (work)", "will have been working", "future-perfect-continuous"));

        // Module 5: Modals 1 (Ability & Permission)
        AddExercises(ctx, l9, "b1e9",
            MC("He ___ drive when he was 16.", "could", "can", "will be able to", "might", "modal-ability,past"),
            MC("She ___ speak five languages.", "can", "could", "might", "should", "modal-ability,present"),
            MC("I ___ play tennis next week (I hope to learn).", "will be able to", "can", "could", "might", "modal-ability,future"),
            FB("When I was young, I ___ climb trees easily. (ability)", "could", "modal-ability"));

        AddExercises(ctx, l10, "b1e10",
            MC("___ I ask you a question?", "May", "Can", "Must", "Should", "modal-permission"),
            MC("You ___ stay here if you want.", "may", "must", "should", "could", "modal-permission"),
            MC("___ I use your phone?", "Might", "Can", "Should", "Could", "modal-permission"),
            FB("___ we go outside to play? (permission)", "May", "modal-permission"));

        // Module 6: Modals 2 (Obligation & Advice)
        AddExercises(ctx, l11, "b1e11",
            MC("You ___ wear a seatbelt in the car.", "must", "should", "could", "might", "modal-obligation"),
            MC("Teenagers ___ study harder.", "should", "must", "could", "might", "modal-advice"),
            MC("You ___ not smoke in hospitals.", "must", "should", "could", "might", "modal-prohibition"),
            FB("You ___ see a doctor about that pain. (advice)", "should", "modal-advice"));

        AddExercises(ctx, l12, "b1e12",
            MC("I ___ go to the meeting tomorrow.", "have to", "must", "should", "could", "modal-necessity"),
            MC("You ___ bring your passport to the airport.", "must", "should", "could", "might", "modal-obligation"),
            MC("Do you ___ wear a uniform at your job?", "have to", "must", "should", "could", "modal-necessity"),
            FB("We ___ arrive before 8 AM. (obligation)", "must", "modal-obligation"));

        // Module 7: Conditional Sentences
        AddExercises(ctx, l13, "b1e13",
            MC("If it rains tomorrow, I ___ stay home.", "will", "would", "am staying", "stayed", "first-conditional"),
            MC("If she studies hard, she ___ the exam.", "will pass", "would pass", "passes", "passed", "first-conditional"),
            MC("If I had more money, I ___ travel more.", "would", "will", "am traveling", "traveled", "second-conditional"),
            FB("If he ___ harder, he would succeed. (work)", "worked", "second-conditional"));

        AddExercises(ctx, l14, "b1e14",
            MC("If I had known about the party, I ___ gone.", "would have", "will have", "have", "would", "third-conditional"),
            MC("If she ___ me earlier, everything would be different.", "had told", "told", "has told", "will tell", "third-conditional"),
            MC("I wish I ___ travel the world.", "could", "can", "will", "am", "wish,ability"),
            FB("If we ___ left earlier, we wouldn't have missed the train. (leave)", "had", "third-conditional"));

        // Module 8: Passive Voice & Reported Speech
        AddExercises(ctx, l15, "b1e15",
            MC("The letter ___ by the postman.", "was delivered", "delivered", "has delivered", "is delivering", "passive,past"),
            MC("This book ___ by Shakespeare.", "was written", "wrote", "has written", "is writing", "passive,past"),
            MC("The house ___ by the workers tomorrow.", "will be painted", "will paint", "will have painted", "is painting", "passive,future"),
            FB("The email ___ to all staff members. (send)", "was sent", "passive"));

        AddExercises(ctx, l16, "b1e16",
            MC("She said she ___ tired.", "was", "is", "were", "am", "reported-speech,tense-shift"),
            MC("He told me he ___ to the cinema.", "was going", "is going", "goes", "will go", "reported-speech,tense-shift"),
            MC("They said they ___ finished their work.", "had", "have", "has", "will have", "reported-speech,tense-shift"),
            FB("She asked what time it ___. (be)", "was", "reported-speech,questions"));
    }

    private static void BuildB2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("b2010000-0000-0000-0000-000000000001");
        var m2 = G("b2020000-0000-0000-0000-000000000002");
        var m3 = G("b2030000-0000-0000-0000-000000000003");
        var m4 = G("b2040000-0000-0000-0000-000000000004");
        var m5 = G("b2050000-0000-0000-0000-000000000005");
        var m6 = G("b2060000-0000-0000-0000-000000000006");
        var m7 = G("b2070000-0000-0000-0000-000000000007");
        var m8 = G("b2080000-0000-0000-0000-000000000008");

        ctx.Modules.Add(NewModule(m1, levelId, "Module 1: -ing and To-infinitive (Units 53-60)", "Master -ing forms and their uses in various contexts.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Module 2: To-infinitive and -ing vs To (Units 61-68)", "Understand the differences and uses of to-infinitive and -ing.", 2, 2));
        ctx.Modules.Add(NewModule(m3, levelId, "Module 3: Articles and Nouns (Units 69-81)", "Master articles (a, an, the) and noun countability.", 3, 2));
        ctx.Modules.Add(NewModule(m4, levelId, "Module 4: Pronouns (Units 82-91)", "Use personal, possessive, reflexive, and relative pronouns correctly.", 4, 2));
        ctx.Modules.Add(NewModule(m5, levelId, "Module 5: Relative Clauses & Adjectives (Units 92-112)", "Form complex sentences with relative clauses and adjectives.", 5, 2));
        ctx.Modules.Add(NewModule(m6, levelId, "Module 6: Adverbs & Conjunctions (Units 98-120)", "Master adverb placement and coordinating conjunctions.", 6, 2));
        ctx.Modules.Add(NewModule(m7, levelId, "Module 7: Prepositions Part 1 (Units 121-128)", "Understand complex prepositions and their uses.", 7, 2));
        ctx.Modules.Add(NewModule(m8, levelId, "Module 8: Prepositions Part 2 & Phrasal Verbs (Units 129-136+)", "Advanced prepositions and productive phrasal verbs.", 8, 2));

        var l1 = G("b2b10000-0000-0000-0000-000000000001");
        var l2 = G("b2b20000-0000-0000-0000-000000000002");
        var l3 = G("b2b30000-0000-0000-0000-000000000003");
        var l4 = G("b2b40000-0000-0000-0000-000000000004");
        var l5 = G("b2b50000-0000-0000-0000-000000000005");
        var l6 = G("b2b60000-0000-0000-0000-000000000006");
        var l7 = G("b2b70000-0000-0000-0000-000000000007");
        var l8 = G("b2b80000-0000-0000-0000-000000000008");
        var l9 = G("b2b90000-0000-0000-0000-000000000009");
        var l10 = G("b2b10000-0000-0000-0000-000000000010");
        var l11 = G("b2b11000-0000-0000-0000-000000000011");
        var l12 = G("b2b12000-0000-0000-0000-000000000012");
        var l13 = G("b2b13000-0000-0000-0000-000000000013");
        var l14 = G("b2b14000-0000-0000-0000-000000000014");
        var l15 = G("b2b15000-0000-0000-0000-000000000015");
        var l16 = G("b2b16000-0000-0000-0000-000000000016");

        // Module 1: -ing and To-infinitive basics
        ctx.Lessons.Add(NewLesson(l1, m1, "-ing forms and their uses", SkillType.Reading, 1, B2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "-ing after verbs and prepositions", SkillType.Writing, 2, B2L2()));

        // Module 2: To-infinitive and -ing vs To
        ctx.Lessons.Add(NewLesson(l3, m2, "To-infinitive uses and structures", SkillType.Reading, 1, B2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "-ing vs To-infinitive: choosing correctly", SkillType.Writing, 2, B2L4()));

        // Module 3: Articles and Nouns
        ctx.Lessons.Add(NewLesson(l5, m3, "Articles: a/an/the and zero article", SkillType.Reading, 1, B2L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Countable and uncountable nouns", SkillType.Writing, 2, B2L6()));

        // Module 4: Pronouns
        ctx.Lessons.Add(NewLesson(l7, m4, "Personal and possessive pronouns", SkillType.Reading, 1, B2L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Reflexive and relative pronouns", SkillType.Writing, 2, B2L8()));

        // Module 5: Relative Clauses & Adjectives
        ctx.Lessons.Add(NewLesson(l9, m5, "Defining and non-defining relative clauses", SkillType.Reading, 1, B2L9()));
        ctx.Lessons.Add(NewLesson(l10, m5, "Adjectives and adverb patterns", SkillType.Writing, 2, B2L10()));

        // Module 6: Adverbs & Conjunctions
        ctx.Lessons.Add(NewLesson(l11, m6, "Adverb placement and formation", SkillType.Reading, 1, B2L11()));
        ctx.Lessons.Add(NewLesson(l12, m6, "Coordinating and subordinating conjunctions", SkillType.Writing, 2, B2L12()));

        // Module 7: Prepositions Part 1
        ctx.Lessons.Add(NewLesson(l13, m7, "Prepositions of time, place, and movement", SkillType.Reading, 1, B2L13()));
        ctx.Lessons.Add(NewLesson(l14, m7, "Complex prepositions and prepositional phrases", SkillType.Writing, 2, B2L14()));

        // Module 8: Prepositions Part 2 & Phrasal Verbs
        ctx.Lessons.Add(NewLesson(l15, m8, "Advanced prepositions and uses", SkillType.Reading, 1, B2L15()));
        ctx.Lessons.Add(NewLesson(l16, m8, "Phrasal verbs: advanced patterns and meanings", SkillType.Writing, 2, B2L16()));

        // Module 1: -ing and To-infinitive basics
        AddExercises(ctx, l1, "b2e1",
            MC("___ is my favorite hobby.", "Swimming", "Swim", "To swim", "Swims", "-ing-forms"),
            MC("I enjoy ___ books in my free time.", "reading", "read", "to read", "reads", "-ing-after-verbs"),
            MC("She is interested in ___ abroad.", "studying", "study", "to study", "studied", "-ing-after-verbs"),
            FB("He started ___ piano at age five. (learn)", "learning", "-ing-forms"));

        AddExercises(ctx, l2, "b2e2",
            MC("I'm responsible for ___ the project.", "managing", "manage", "to manage", "managed", "-ing-after-prepositions"),
            MC("Before ___, check your work.", "leaving", "leave", "to leave", "left", "-ing-after-prepositions"),
            MC("What's the advantage of ___ early?", "leaving", "leave", "to leave", "left", "-ing-after-prepositions"),
            FB("After ___ the meeting, we had coffee. (attend)", "attending", "-ing-after-prepositions"));

        // Module 2: To-infinitive and -ing vs To
        AddExercises(ctx, l3, "b2e3",
            MC("She went to the store ___ milk.", "to buy", "buying", "buy", "bought", "to-infinitive-purpose"),
            MC("My aim is ___ a successful career.", "to build", "building", "build", "built", "to-infinitive"),
            MC("It's important ___ healthy.", "to stay", "staying", "stay", "stayed", "to-infinitive-important"),
            FB("We use ___ to go to the beach every summer. (use)", "to", "to-infinitive"));

        AddExercises(ctx, l4, "b2e4",
            MC("I prefer ___ rather than driving.", "walking", "to walk", "walk", "walked", "-ing-vs-to"),
            MC("She avoided ___ during the presentation.", "speaking", "to speak", "speak", "spoke", "-ing-vs-to"),
            MC("Would you like ___ for dinner tonight?", "to eat out", "eating out", "eat out", "ate out", "-ing-vs-to"),
            FB("I promise ___ on time. (arrive)", "to arrive", "-ing-vs-to"));

        // Module 3: Articles and Nouns
        AddExercises(ctx, l5, "b2e5",
            MC("She is ___ doctor.", "a", "an", "the", "-", "articles"),
            MC("I need ___ advice about my career.", "a", "an", "the", "-", "articles,uncountable"),
            MC("___ sun rises in the east.", "A", "An", "The", "-", "articles,definite"),
            FB("He plays ___ guitar very well. (article)", "the", "articles"));

        AddExercises(ctx, l6, "b2e6",
            MC("There are three ___ in the box.", "boxes", "box", "boxs", "box's", "countable-nouns"),
            MC("I need some ___ for the cake.", "information", "informations", "inform", "informationing", "uncountable-nouns"),
            MC("How many ___ do you have?", "children", "child", "childs", "childes", "countable-nouns"),
            FB("The ___ is delicious. (advice)", "advice", "uncountable"));

        // Module 4: Pronouns
        AddExercises(ctx, l7, "b2e7",
            MC("___ and ___ are good friends.", "Me and him", "He and I", "Him and me", "Me and he", "pronouns-personal"),
            MC("This book is ___.", "mine", "my", "me", "I'm", "pronouns-possessive"),
            MC("The children enjoyed ___.", "themselves", "themself", "themselves", "theirselves", "pronouns-reflexive"),
            FB("The woman ___ I met was kind. (relative)", "who", "pronouns-relative"));

        AddExercises(ctx, l8, "b2e8",
            MC("___ responsibility is theirs, not ___.", "Their / ours", "There / ours", "Their / hours", "There / hours", "pronouns-possessive"),
            MC("The student ___ won the prize is talented.", "who", "which", "whose", "what", "pronouns-relative"),
            MC("I hurt ___.", "myself", "me", "I", "mine", "pronouns-reflexive"),
            FB("___ is a problem. (this)", "This", "pronouns"));

        // Module 5: Relative Clauses & Adjectives
        AddExercises(ctx, l9, "b2e9",
            MC("The book ___ I bought is excellent.", "which", "who", "whose", "where", "relative-clauses"),
            MC("The woman ___ son is a doctor lives next door.", "whose", "who", "which", "where", "relative-clauses"),
            MC("I visited the city ___ I was born.", "where", "which", "who", "whose", "relative-clauses"),
            FB("The person ___ helped me was very kind. (who)", "who", "relative-clauses"));

        AddExercises(ctx, l10, "b2e10",
            MC("She is a ___ person.", "successful", "success", "successfully", "succeed", "adjectives"),
            MC("He drives ___.", "carefully", "careful", "care", "cared", "adverbs"),
            MC("The weather was ___ cold.", "extremely", "extreme", "extremes", "extremed", "adverbs-intensifiers"),
            FB("This is an ___ opportunity. (excel)", "excellent", "adjectives"));

        // Module 6: Adverbs & Conjunctions
        AddExercises(ctx, l11, "b2e11",
            MC("___ she didn't know the answer.", "Probably", "Perhaps", "Likely", "Maybe", "adverbs-modality"),
            MC("He exercises regularly ___ stay healthy.", "to", "for", "in order to", "so as to", "adverbs-purpose"),
            MC("The package was delivered ___.", "yesterday", "yesterdaying", "yesterdayed", "yesterdayed", "adverbs-time"),
            FB("She speaks English ___. (fluent)", "fluently", "adverbs"));

        AddExercises(ctx, l12, "b2e12",
            MC("He is intelligent, ___ he is lazy.", "but", "because", "although", "so", "conjunctions-coordinating"),
            MC("I will wait here ___ you return.", "until", "unless", "while", "if", "conjunctions-subordinating"),
            MC("She was sick; ___, she came to work.", "however", "because", "therefore", "so", "conjunctions-cohesion"),
            FB("We went to the beach ___ it was sunny. (because)", "because", "conjunctions"));

        // Module 7: Prepositions Part 1
        AddExercises(ctx, l13, "b2e13",
            MC("I was born ___ 1990.", "in", "on", "at", "during", "prepositions-time"),
            MC("The meeting is ___ 3 PM.", "at", "in", "on", "during", "prepositions-time"),
            MC("The book is ___ the table.", "on", "in", "at", "under", "prepositions-place"),
            FB("She ran ___ the room. (into)", "into", "prepositions-movement"));

        AddExercises(ctx, l14, "b2e14",
            MC("He is responsible ___ the project.", "for", "of", "in", "on", "prepositions-complex"),
            MC("I am interested ___ learning languages.", "in", "on", "at", "of", "prepositions-complex"),
            MC("She is afraid ___ spiders.", "of", "from", "in", "for", "prepositions-complex"),
            FB("The solution depends ___ the situation. (on)", "on", "prepositions"));

        // Module 8: Prepositions Part 2 & Phrasal Verbs
        AddExercises(ctx, l15, "b2e15",
            MC("I'm looking forward ___ the holidays.", "to", "for", "at", "in", "prepositions-advanced"),
            MC("He is capable ___ doing difficult work.", "of", "for", "in", "to", "prepositions-advanced"),
            MC("She is proud ___ her achievements.", "of", "for", "in", "about", "prepositions-advanced"),
            FB("This company specializes ___ software. (in)", "in", "prepositions"));

        AddExercises(ctx, l16, "b2e16",
            MC("I ___ the important information in the email.", "picked up", "picked on", "picked out", "picked off", "phrasal-verbs"),
            MC("We need to ___ this project as soon as possible.", "carry out", "carry on", "carry over", "carry through", "phrasal-verbs"),
            MC("The meeting ___ until next week.", "put off", "put on", "put down", "put up", "phrasal-verbs"),
            FB("I ___ with my friend after the argument. (make)", "made up", "phrasal-verbs"));
    }

    private static void BuildC1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("c1010000-0000-0000-0000-000000000001");
        var m2 = G("c1020000-0000-0000-0000-000000000002");
        var m3 = G("c1030000-0000-0000-0000-000000000003");
        var m4 = G("c1040000-0000-0000-0000-000000000004");
        var m5 = G("c1050000-0000-0000-0000-000000000005");
        var m6 = G("c1060000-0000-0000-0000-000000000006");
        var m7 = G("c1070000-0000-0000-0000-000000000007");
        var m8 = G("c1080000-0000-0000-0000-000000000008");

        ctx.Modules.Add(NewModule(m1, levelId, "Tiempos Verbales Avanzados I: Present & Past Perfect Continuous (Units 1-6)", "Master present/past perfect continuous for extended duration and antecedent actions.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Tiempos Verbales Avanzados II: Future Forms & Backshift (Units 7-12)", "Express future: continuous, perfect, be-to, and mixed tenses in context.", 2, 2));
        ctx.Modules.Add(NewModule(m3, levelId, "Modales Deductivos I: Must, Can't, Might Certainty (Units 15-20)", "Master deduction modals for certainty levels and perfect speculations.", 3, 2));
        ctx.Modules.Add(NewModule(m4, levelId, "Modales Deductivos II: Had to, Could, Semi-modals (Units 21-25)", "Obligation, deduction modals, and semi-modal verbs for advanced nuance.", 4, 2));
        ctx.Modules.Add(NewModule(m5, levelId, "Pasiva y Reportados I: Causatives & Advanced Passive (Units 26-33)", "Causative structures and passive voice with infinitives and reporting verbs.", 5, 2));
        ctx.Modules.Add(NewModule(m6, levelId, "Pasiva y Reportados II: Reporting Verbs & Patterns (Units 34-40)", "Master reporting verbs (claim, maintain, concede) and verb pattern selection.", 6, 2));
        ctx.Modules.Add(NewModule(m7, levelId, "Condicionales I: Mixed & Unless Conditionals (Units 41-45)", "Mixed conditionals (past→present), unless, as long as, and variants.", 7, 2));
        ctx.Modules.Add(NewModule(m8, levelId, "Condicionales II: Inverted & Hypothetical (Units 46-50)", "Inverted conditionals (Had I..., Should you...) and wish/regret structures.", 8, 2));

        var l1 = G("c1b10000-0000-0000-0000-000000000001");
        var l2 = G("c1b20000-0000-0000-0000-000000000002");
        var l3 = G("c1b30000-0000-0000-0000-000000000003");
        var l4 = G("c1b40000-0000-0000-0000-000000000004");
        var l5 = G("c1b50000-0000-0000-0000-000000000005");
        var l6 = G("c1b60000-0000-0000-0000-000000000006");
        var l7 = G("c1b70000-0000-0000-0000-000000000007");
        var l8 = G("c1b80000-0000-0000-0000-000000000008");
        var l9 = G("c1b90000-0000-0000-0000-000000000009");
        var l10 = G("c1b10000-0000-0000-0000-000000000010");
        var l11 = G("c1b11000-0000-0000-0000-000000000011");
        var l12 = G("c1b12000-0000-0000-0000-000000000012");
        var l13 = G("c1b13000-0000-0000-0000-000000000013");
        var l14 = G("c1b14000-0000-0000-0000-000000000014");
        var l15 = G("c1b15000-0000-0000-0000-000000000015");
        var l16 = G("c1b16000-0000-0000-0000-000000000016");

        // Module 1: Tiempos Verbales Avanzados I
        ctx.Lessons.Add(NewLesson(l1, m1, "Present Perfect Continuous: duration and relevance (Units 1-3)", SkillType.Reading, 1, C1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Past Perfect Continuous: antecedent ongoing actions (Units 4-6)", SkillType.Writing, 2, C1L2()));

        // Module 2: Tiempos Verbales Avanzados II
        ctx.Lessons.Add(NewLesson(l3, m2, "Future Continuous & Future Perfect: temporal expressions (Units 7-9)", SkillType.Reading, 1, C1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Backshift and mixed future tenses in context (Units 10-12)", SkillType.Writing, 2, C1L4()));

        // Module 3: Modales Deductivos I
        ctx.Lessons.Add(NewLesson(l5, m3, "Modal deduction: must/can't/might for certainty levels (Units 15-17)", SkillType.Reading, 1, C1L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Modal deduction: could, might, may in continuous and perfect (Units 18-20)", SkillType.Writing, 2, C1L6()));

        // Module 4: Modales Deductivos II
        ctx.Lessons.Add(NewLesson(l7, m4, "Had to (obligation/deduction) and didn't have to vs needn't have (Units 21-23)", SkillType.Reading, 1, C1L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Semi-modals: used to, be used to, get used to, would (habits) (Units 24-25)", SkillType.Writing, 2, C1L8()));

        // Module 5: Pasiva y Reportados I
        ctx.Lessons.Add(NewLesson(l9, m5, "Causative structures: have/get something done (Units 26-28)", SkillType.Reading, 1, C1L9()));
        ctx.Lessons.Add(NewLesson(l10, m5, "Advanced passive: infinitive passive, get-passive, passive with reporting verbs (Units 29-33)", SkillType.Writing, 2, C1L10()));

        // Module 6: Pasiva y Reportados II
        ctx.Lessons.Add(NewLesson(l11, m6, "Reporting verbs in depth: claim, maintain, assert, concede (Units 34-36)", SkillType.Reading, 1, C1L11()));
        ctx.Lessons.Add(NewLesson(l12, m6, "Verb patterns: to-infinitive vs -ing after reporting verbs (Units 37-40)", SkillType.Writing, 2, C1L12()));

        // Module 7: Condicionales I
        ctx.Lessons.Add(NewLesson(l13, m7, "Mixed conditionals (past condition → present result) (Units 41-43)", SkillType.Reading, 1, C1L13()));
        ctx.Lessons.Add(NewLesson(l14, m7, "Unless, as long as, provided that, in case (Units 44-45)", SkillType.Writing, 2, C1L14()));

        // Module 8: Condicionales II
        ctx.Lessons.Add(NewLesson(l15, m8, "Inverted conditionals: Had I known..., Should you need... (Units 46-48)", SkillType.Reading, 1, C1L15()));
        ctx.Lessons.Add(NewLesson(l16, m8, "Hypothetical wish/regret: wish, if only, it's time, would rather (Units 49-50)", SkillType.Writing, 2, C1L16()));

        // Module 1 Exercises: Present Perfect Continuous
        AddExercises(ctx, l1, "c1e1",
            MC("They ___ on this project for six months now.", "have been working", "have worked", "are working", "have been working on", "perfect-continuous,duration"),
            MC("The CEO ___ with the board all morning — she looks exhausted.", "has been negotiating", "has negotiated", "is negotiating", "had negotiated", "perfect-continuous,evidence"),
            MC("Your eyes are red. ___ you ___?", "Have - been crying", "Are - crying", "Do - cry", "Have - cried", "perfect-continuous,result"),
            FB("The machinery ___ since 5 AM. We need maintenance soon. (run)", "has been running", "perfect-continuous,duration"));

        // Module 1 Exercises: Past Perfect Continuous
        AddExercises(ctx, l2, "c1e2",
            MC("Before the merger was finalized, the two companies ___ for weeks.", "had been negotiating", "had negotiated", "were negotiating", "negotiated", "past-perfect-continuous,context"),
            MC("When the fire alarm went off, how long ___ the presentation ___?", "had - been going on", "was - going on", "has - been going on", "did - go on", "past-perfect-continuous,interruption"),
            MC("She felt exhausted because she ___ on the same task for hours.", "had been working", "had worked", "was working", "worked", "past-perfect-continuous,reason"),
            FB("By the time he arrived, I ___ for him for over an hour. (wait)", "had been waiting", "past-perfect-continuous,prior-action"));

        // Module 2 Exercises: Future Continuous & Perfect
        AddExercises(ctx, l3, "c1e3",
            MC("At this time next week, we ___ our final exams.", "will be taking", "take", "will have taken", "have taken", "future-continuous,specific-time"),
            MC("By the time you finish your PhD, you ___ in university life for nearly a decade.", "will have been living", "will live", "live", "have been living", "future-perfect-continuous,duration-by-time"),
            MC("Don't call me at 8 PM — I ___ dinner with clients.", "will be having", "will have had", "have", "am having", "future-continuous,unavailable"),
            FB("When you retire, you ___ for this company for 30 years. (work)", "will have been working", "future-perfect-continuous"));

        // Module 2 Exercises: Backshift & Future Forms
        AddExercises(ctx, l4, "c1e4",
            MC("The president ___ to address Parliament tomorrow about healthcare reforms.", "is to", "is going to", "will", "is about to", "be-to,scheduled"),
            MC("She said the deadline ___ next Friday without fail.", "was to be", "is to be", "would be", "will be", "backshift,be-to"),
            MC("The package is ___ to arrive by noon. It's being tracked closely.", "to be scheduled", "supposed", "expected", "about", "be-to,expectation"),
            FB("The ceremony ___ to commence at precisely 6 PM. Please arrive early. (formal, scheduled)", "is", "be-to"));

        // Module 3 Exercises: Deduction Modals (must, can't, might)
        AddExercises(ctx, l5, "c1e5",
            MC("She ___ be in the office — I just saw her there five minutes ago.", "must", "might", "could", "can't", "modal-deduction,certainty"),
            MC("He ___ have missed the train; he would have called by now if he had.", "can't", "must", "might", "could", "modal-deduction,negation"),
            MC("The letter ___ be from the government, but it could also be from the bank.", "might", "must", "can't", "should", "modal-deduction,possibility"),
            FB("They ___ have understood the instructions clearly—they're implementing it perfectly. (modal: positive certainty)", "must", "modal-deduction,certainty"));

        // Module 3 Exercises: Modal Deduction with Continuous & Perfect
        AddExercises(ctx, l6, "c1e6",
            MC("Someone ___ using the conference room—I can hear voices inside.", "must be", "might be", "can't be", "couldn't be", "modal-deduction,continuous,present"),
            MC("He ___ lost the keys in the car; they were in his pocket all along.", "can't have", "might have", "could have", "should have", "modal-perfect,negation"),
            MC("The suspect ___ been in two places at once, so one alibi must be false.", "couldn't have", "shouldn't have", "wouldn't have", "can't be", "modal-perfect,logic"),
            FB("She ___ working on this for months—it explains her exhaustion. (must + perfect continuous)", "must have been", "modal-perfect,continuous"));

        // Module 4 Exercises: Had to, Didn't have to, Needn't have
        AddExercises(ctx, l7, "c1e7",
            MC("I ___ attend the meeting, yet I arrived two hours early out of nervousness.", "didn't have to", "needn't have", "hadn't had to", "wouldn't have to", "obligation,unnecessary"),
            MC("She ___ about the deadline—the boss had already extended it by a week.", "didn't need to worry", "needn't have worried", "wouldn't have worried", "mustn't have worried", "unnecessary-past-emotion"),
            MC("They ___ pay full price; the early-bird discount applied to them.", "didn't have to", "had to", "needn't have", "shouldn't have", "obligation,exemption"),
            FB("You ___ told him the truth—he would've found out anyway eventually. (regret about unnecessary action)", "needn't have", "unnecessary-action,regret"));

        // Module 4 Exercises: Semi-modals
        AddExercises(ctx, l8, "c1e8",
            MC("He ___ smoke, but quit five years ago for his health.", "used to", "is used to", "gets used to", "uses to", "semi-modal,past-habit"),
            MC("Noise doesn't bother her anymore; she's ___ working in open-plan offices.", "used to", "is used to", "gets used to", "using to", "semi-modal,accustomed"),
            MC("The team is gradually ___ the agile development process.", "used to", "is used to", "getting used to", "uses to", "semi-modal,gradual-adaptation"),
            FB("Before moving to the city, she ___ living in quiet, rural areas. (past habit)", "used to", "semi-modal,past-habit"));

        // Module 5 Exercises: Causatives
        AddExercises(ctx, l9, "c1e9",
            MC("I will ___ the car serviced before selling it.", "have", "get", "make", "let", "causative,have-get"),
            MC("She ___ her portrait painted by a famous artist.", "had", "has", "gets", "made", "causative,past"),
            MC("We should ___ the plumbing inspected before purchasing the house.", "have it", "get it", "make it", "let it", "causative,advice"),
            FB("They had the architect ___ the entire building plan from scratch. (redesign)", "redesign", "causative,infinitive"));

        // Module 5 Exercises: Advanced Passive
        AddExercises(ctx, l10, "c1e10",
            MC("The proposal is believed ___ the best solution available.", "to be", "being", "to be being", "be", "passive,infinitive"),
            MC("The decision appears ___ by several factors beyond our control.", "to be influenced", "to influence", "being influenced", "be influenced", "passive,infinitive,seem"),
            MC("The new policy got ___ by the board despite strong opposition.", "approved", "approving", "to approve", "being approved", "get-passive"),
            FB("These procedures are worth ___ in detail before implementation. (examine)", "examining", "passive,-ing,worth"));

        // Module 6 Exercises: Reporting Verbs
        AddExercises(ctx, l11, "c1e11",
            MC("The lead researcher ___ that the breakthrough could revolutionize treatment.", "claimed", "whispered", "shouted", "stuttered", "reporting-verbs,neutral"),
            MC("The defendant ___ having any involvement in the crime whatsoever.", "denied", "suggested", "admitted", "complained", "reporting-verbs,negation"),
            MC("Political commentators ___ that this decision would have far-reaching consequences.", "contended", "asked", "whispered", "told", "reporting-verbs,argument"),
            FB("She ___ reluctantly that her colleague's approach had merit. (partially conceded)", "acknowledged", "reporting-verbs,concession"));

        // Module 6 Exercises: Verb Patterns after Reporting
        AddExercises(ctx, l12, "c1e12",
            MC("The safety officer insisted ___ all equipment before use.", "on checking", "to check", "checking", "on to check", "verb-pattern,reporting,insist"),
            MC("The minister acknowledged ___ the previous government's efforts.", "having underestimated", "to underestimate", "underestimate", "to have underestimated", "verb-pattern,reporting,acknowledge"),
            MC("The lawyer advised her ___ signing any documents without review.", "against", "against to", "to against", "not to", "verb-pattern,reporting,advise"),
            FB("The witness maintained ___ the suspect at the scene of the crime. (had seen)", "having seen", "verb-pattern,reporting,maintain"));

        // Module 7 Exercises: Mixed Conditionals
        AddExercises(ctx, l13, "c1e13",
            MC("If I ___ the job offer back then, I ___ abroad now.", "had accepted - would be living", "accepted - would be living", "had accepted - will be living", "accept - would live", "mixed-conditional,past-condition-present"),
            MC("If she ___ the warning signs, she ___ serious health problems now.", "had heeded - wouldn't have", "had heeded - wouldn't be having", "heed - won't have", "heeded - wouldn't have", "mixed-conditional,cause-effect"),
            MC("If the government ___ action then, we ___ facing this crisis today.", "had taken - wouldn't be", "took - wouldn't be", "takes - won't be", "had taken - aren't", "mixed-conditional,policy"),
            FB("If I ___ fluent in Mandarin, I ___ that job in Beijing now. (past condition → present result)", "had become - would have", "mixed-conditional,hypothetical"));

        // Module 7 Exercises: Unless, As long as, Provided that
        AddExercises(ctx, l14, "c1e14",
            MC("You won't succeed ___ you work consistently and focus.", "unless", "as long as", "provided", "even if", "conditional-variant,negation"),
            MC("The contract is valid ___ both parties sign within 30 days.", "unless", "as long as", "provided that", "if not", "conditional-variant,requirement"),
            MC("___ you meet the minimum academic standards, admission is guaranteed.", "Unless", "As long as", "Provided", "If", "conditional-variant,condition"),
            FB("The warranty covers damage ___ it results from normal wear and tear. (except, on condition)", "provided that", "conditional-variant,exclusion"));

        // Module 8 Exercises: Inverted Conditionals
        AddExercises(ctx, l15, "c1e15",
            MC("___ I realized the consequences earlier, I would never have agreed.", "Had", "Should", "Would", "If", "inverted-conditional,past"),
            MC("___ you encounter any difficulties during the procedure, contact technical support immediately.", "Should", "Had", "Would", "Might", "inverted-conditional,possibility"),
            MC("___ the witnesses not come forward, the case would have been unsolved.", "Had", "Should", "Would", "If", "inverted-conditional,causal"),
            FB("___ I in your position, I would reconsider that decision entirely. (hypothetical, were)", "Were", "inverted-conditional,present"));

        // Module 8 Exercises: Wish, If only, Hypothetical
        AddExercises(ctx, l16, "c1e16",
            MC("I wish I ___ invested in cryptocurrency years ago—I'd be wealthy now.", "had", "have", "would have", "had had", "wish,past-regret,money"),
            MC("If only she ___ her potential before wasting those years on the wrong path.", "had realized", "realizes", "would realize", "had been realizing", "wish,past-regret,missed-potential"),
            MC("It's high time the government ___ serious action on climate change.", "took", "take", "would take", "had taken", "it's-time,urgency"),
            FB("He'd rather you ___ him the opportunity to explain himself properly. (past hypothetical, accusation)", "had given", "would-rather,regret"));
    }

    private static void BuildC2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("c2010000-0000-0000-0000-000000000001");
        var m2 = G("c2020000-0000-0000-0000-000000000002");
        var m3 = G("c2030000-0000-0000-0000-000000000003");
        var m4 = G("c2040000-0000-0000-0000-000000000004");
        var m5 = G("c2050000-0000-0000-0000-000000000005");
        var m6 = G("c2060000-0000-0000-0000-000000000006");
        var m7 = G("c2070000-0000-0000-0000-000000000007");
        var m8 = G("c2080000-0000-0000-0000-000000000008");
        ctx.Modules.Add(NewModule(m1, levelId, "Cláusulas Relativas I: Non-defining (Units 51-58)", "Advanced relative clauses: which, whose, where with sophistication.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Cláusulas Relativas II: Reduced & Cleft (Units 59-65)", "Participle clauses and cleft sentences for emphasis.", 2, 2));
        ctx.Modules.Add(NewModule(m3, levelId, "Nominalización y Enfoque I: Nominalization & Fronting (Units 66-72)", "Transform verbs to nouns and move constituents for emphasis.", 3, 2));
        ctx.Modules.Add(NewModule(m4, levelId, "Nominalización y Enfoque II: Inversion & Focus (Units 73-80)", "Negative/degree inversion and focus adverbs for native precision.", 4, 2));
        ctx.Modules.Add(NewModule(m5, levelId, "Preposiciones y Phrasals I: Complex Noun Phrases (Units 81-88)", "Pre- and post-modification in noun phrases.", 5, 2));
        ctx.Modules.Add(NewModule(m6, levelId, "Preposiciones y Phrasals II: Idiomatic Prepositions (Units 89-95)", "Fixed expressions and phrasal verbs in context.", 6, 2));
        ctx.Modules.Add(NewModule(m7, levelId, "Conectores Discursivos I: Discourse Markers (Units 96-100)", "Additive, contrastive, causal, and sequential markers.", 7, 2));
        ctx.Modules.Add(NewModule(m8, levelId, "Conectores Discursivos II: Ellipsis & Substitution (Units 101-105)", "Ellipsis and substitution for native fluency and cohesion.", 8, 2));

        var l1 = G("c2b10000-0000-0000-0000-000000000001");
        var l2 = G("c2b20000-0000-0000-0000-000000000002");
        var l3 = G("c2b30000-0000-0000-0000-000000000003");
        var l4 = G("c2b40000-0000-0000-0000-000000000004");
        var l5 = G("c2b50000-0000-0000-0000-000000000005");
        var l6 = G("c2b60000-0000-0000-0000-000000000006");
        var l7 = G("c2b70000-0000-0000-0000-000000000007");
        var l8 = G("c2b80000-0000-0000-0000-000000000008");
        var l9 = G("c2b90000-0000-0000-0000-000000000009");
        var l10 = G("c2ba0000-0000-0000-0000-000000000010");
        var l11 = G("c2bb0000-0000-0000-0000-000000000011");
        var l12 = G("c2bc0000-0000-0000-0000-000000000012");
        var l13 = G("c2bd0000-0000-0000-0000-000000000013");
        var l14 = G("c2be0000-0000-0000-0000-000000000014");
        var l15 = G("c2bf0000-0000-0000-0000-000000000015");
        var l16 = G("c2c00000-0000-0000-0000-000000000016");
        ctx.Lessons.Add(NewLesson(l1, m1, "Non-defining relative clauses: which/whose/where (Units 51-54)", SkillType.Reading, 1, C2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Sentential relative clauses and relative adverbs (Units 55-58)", SkillType.Writing, 2, C2L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Reduced relatives: participle clauses (Units 59-62)", SkillType.Reading, 1, C2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Cleft sentences: it-clefts and wh-clefts (Units 63-65)", SkillType.Writing, 2, C2L4()));
        ctx.Lessons.Add(NewLesson(l5, m3, "Nominalization: from verbs/adjectives to nouns (Units 66-69)", SkillType.Reading, 1, C2L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Fronting: moving constituents for emphasis (Units 70-72)", SkillType.Writing, 2, C2L6()));
        ctx.Lessons.Add(NewLesson(l7, m4, "Negative and degree inversion (Units 73-76)", SkillType.Reading, 1, C2L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Focus adverbs: even, only, so, such, neither/nor (Units 77-80)", SkillType.Writing, 2, C2L8()));
        ctx.Lessons.Add(NewLesson(l9, m5, "Complex noun phrases: pre- and post-modification (Units 81-85)", SkillType.Reading, 1, C2L9()));
        ctx.Lessons.Add(NewLesson(l10, m5, "Idiomatic prepositions: at, by, in, on in fixed expressions (Units 86-88)", SkillType.Writing, 2, C2L10()));
        ctx.Lessons.Add(NewLesson(l11, m6, "Advanced phrasal verbs: transitive/intransitive patterns (Units 89-92)", SkillType.Reading, 1, C2L11()));
        ctx.Lessons.Add(NewLesson(l12, m6, "Collocations and idiomatic prepositional phrases (Units 93-95)", SkillType.Writing, 2, C2L12()));
        ctx.Lessons.Add(NewLesson(l13, m7, "Discourse markers: additive, contrastive, causal, sequential (Units 96-98)", SkillType.Reading, 1, C2L13()));
        ctx.Lessons.Add(NewLesson(l14, m7, "Cohesive devices: reference, substitution, conjunction (Units 99-100)", SkillType.Writing, 2, C2L14()));
        ctx.Lessons.Add(NewLesson(l15, m8, "Ellipsis: verbal, nominal, clausal omission (Units 101-102)", SkillType.Reading, 1, C2L15()));
        ctx.Lessons.Add(NewLesson(l16, m8, "Substitution: do/so/not replacing clauses (Units 103-105)", SkillType.Writing, 2, C2L16()));

        AddExercises(ctx, l1, "c2e1",
            MC("In 'The students who submitted late received penalties,' the relative clause is:", "defining (restricts which students)", "non-defining (adds extra info)", "reduced participle", "cleft", "relative-clauses,defining"),
            MC("Which shows correct punctuation for a non-defining relative?", "The professor, whose research won an award, retired last year.", "The professor whose research won an award, retired last year.", "The professor who's research won an award retired last year.", "All three are correct.", "relative-clauses,non-defining,punctuation"),
            MC("'She consulted several experts, many of whom disagreed with her thesis.' The 'whom' phrase is:", "a non-defining relative clause", "a cleft sentence", "a participle clause", "an inversion", "relative-clauses,formal"),
            FB("The prestigious laboratory ___ the groundbreaking research was conducted has since closed permanently. (in which)", "in which", "relative-clauses,preposition,formal"));

        AddExercises(ctx, l2, "c2e2",
            MC("In 'He abandoned his family fortune, which shocked the entire community,' the 'which' refers to:", "his abandonment of the fortune (entire situation)", "the word 'fortune' alone", "his family", "the community's reaction", "sentential-relative,situation"),
            MC("Which best demonstrates a sentential relative clause?", "The proposal was rejected outright, which meant months of wasted effort.", "The proposal which was rejected was poorly written.", "The rejected proposal meant wasted effort.", "None—sentential relatives are rare.", "sentential-relative,consequence"),
            MC("'The period during which the market crashed was terrifying' — 'during which' is:", "a complex relative adverbial phrase", "a simple preposition", "an inversion structure", "an elliptical construction", "relative-clauses,complex"),
            FB("The reason why he resigned was financial hardship, a situation whereby he had little choice. The 'whereby' clause introduces:", "an alternative explanation or consequence", "relative-clauses,whereby"));

        AddExercises(ctx, l3, "c2e3",
            MC("'The proposal approved by the committee' is a reduced relative. It replaces:", "'The proposal which was approved by the committee'", "'The proposal that was approved by the committee'", "Both A and B", "Neither—it's a participle phrase", "reduced-relatives,participle"),
            MC("'Having exhausted all alternatives, the government introduced emergency measures' is:", "an adverbial participle clause (reduced from a temporal relative)", "a cleft sentence", "a nominalization structure", "an inversion", "reduced-relatives,adverbial"),
            MC("Which correctly shows a reduced passive relative?", "The regulations introduced last year are controversial.", "The regulations introducing last year are controversial.", "The regulations being introduced last year are controversial.", "The regulations to introduce last year are controversial.", "reduced-relatives,passive"),
            FB("The documents ___ without proper authorization have been flagged for review. (circulate)", "circulating", "reduced-relatives,passive,present-participle"));

        AddExercises(ctx, l4, "c2e4",
            MC("In 'It is innovation that drives market success,' the cleft sentence emphasizes:", "'innovation' (the subject agent)", "'drives market success' (the consequence)", "'market success' (the outcome)", "the entire concept", "cleft-sentences,it-cleft,emphasis"),
            MC("'What the board ultimately decided was to restructure the entire division' is:", "a wh-cleft (emphasizes the decision)", "an it-cleft", "a pseudo-cleft (no it-is or what-is frame)", "an inverted conditional", "cleft-sentences,wh-cleft"),
            MC("Comparing normal vs. cleft: Normal: 'The lack of transparency caused public distrust.' Cleft: 'It was the lack of transparency that caused public distrust.' This shift:", "highlights a specific cause by isolating it", "changes the grammatical voice", "makes the sentence shorter", "creates ambiguity", "cleft-sentences,rhetorical"),
            FB("___ that determined the outcome was not luck but careful preparation and foresight. (It)", "It was", "cleft-sentences,it-cleft"));

        AddExercises(ctx, l5, "c2e5",
            MC("The nominalization in 'The government's decision to raise taxes proved unpopular' emphasizes:", "the decision (noun) rather than the action of deciding (verb)", "tax raising rather than government policy", "public opinion rather than policy", "the verb form of decide", "nominalization,focus"),
            MC("'The implementation of the new strategy required extensive reorganization.' This nominalization achieves:", "formal, abstract style suitable for business writing", "brevity and simplicity", "emphasis on the verb 'implement'", "active voice", "nominalization,register"),
            MC("Which correctly nominalizes a gerund-based concept?", "Their commitment to environmental sustainability impressed stakeholders.", "Their committing to environmental sustainability impressed stakeholders.", "They committed to environmental sustainability, which impressed stakeholders.", "The commitment by them to sustainability was impressive.", "nominalization,gerund"),
            FB("The ___ of the research findings by multiple institutions confirmed the original hypothesis. (verify)", "verification", "nominalization,passive"));

        AddExercises(ctx, l6, "c2e6",
            MC("In 'Unprecedented were the challenges faced by the organization,' fronting achieves:", "emphasis on 'unprecedented' (the degree/quality)", "grammatical inversion", "shortened sentence length", "passive voice", "fronting,degree,emphasis"),
            MC("Comparing normal vs. fronted: Normal: 'Most troubling is the lack of transparency.' Fronted: 'Most troubling about this situation is the lack of transparency.' The fronted version:", "adds additional context and nuance", "emphasizes the troubling aspect", "changes the meaning significantly", "is grammatically incorrect", "fronting,context"),
            MC("Which demonstrates fronting in academic writing?", "Particularly significant was the discovery that previously unknown species thrived in extreme depths.", "The discovery that previously unknown species thrived was particularly significant.", "They discovered that previously unknown species thrived—particularly significant.", "All carry equal emphasis.", "fronting,academic"),
            FB("___ is the gap between policy and implementation in most large organizations. (Striking)", "Striking", "fronting,emphasis,adjective"));

        AddExercises(ctx, l7, "c2e7",
            MC("'Under no circumstances should you disclose this information' shows:", "negative inversion (subject-auxiliary inverted after negative adverb)", "degree inversion", "fronting of a prepositional phrase", "a prohibition structure", "inversion,negative"),
            MC("'So compelling was the evidence that the verdict was unanimous' demonstrates:", "degree inversion with 'so' + adjective (emphasizes intensity)", "negative inversion", "pseudo-cleft", "a conditional structure", "inversion,degree,intensity"),
            MC("'Hardly had negotiations begun when the unexpected proposal emerged' combines:", "negative inversion + temporal clause", "degree inversion", "nominalization", "ellipsis", "inversion,temporal"),
            FB("'___ once has the project exceeded budget in its fifteen-year history.' (rarely, never, seldom)", "Rarely", "inversion,negative,emphasis"));

        AddExercises(ctx, l8, "c2e8",
            MC("In 'Only the CEO can make this decision,' 'only' restricts to:", "the CEO (nobody else can)", "the decision (no other actions)", "the ability (no other circumstances)", "the present time", "focus-adverbs,scope"),
            MC("Position changes meaning: 'Even critics acknowledged merit' vs. 'Critics even acknowledged merit.' The difference is:", "the first emphasizes surprising agreement; the second emphasizes completeness of agreement", "no difference—both mean the same", "the first is emphatic; the second is neutral", "only the second is grammatical", "focus-adverbs,position"),
            MC("'Just yesterday she submitted her resignation, not long before today's announcement' — 'just' indicates:", "temporal proximity and recent timing", "emphasis on the specific day", "duration of resignation", "causality with announcement", "focus-adverbs,temporal"),
            FB("'Neither solution satisfies the requirement for compatibility across all systems.' Here 'neither' functions as:", "a focus adverb restricting to none of two options", "focus-adverbs,negative"));

        AddExercises(ctx, l9, "c2e9",
            MC("In 'The comprehensive analysis of survey data from three countries conducted over two years,' post-modification includes:", "of survey data + from three countries + conducted over two years", "the comprehensive (pre-modification only)", "conducted (only the participle)", "comprehensive and analysis (adjective + noun)", "noun-phrases,post-modification"),
            MC("'The government's controversial decision on education reform affecting millions' stacks:", "pre-modification (government's, controversial) + noun + post-modification (on education reform affecting millions)", "all pre-modification", "all post-modification", "only adjective modification", "noun-phrases,complex"),
            MC("Which complex noun phrase is correctly structured?", "The three recent studies examining climate impacts from major institutions", "The three recent from major institutions studies examining climate impacts", "Studies three recent examining climate impacts from major institutions", "The three recent studies institutions from examining climate impacts", "noun-phrases,order"),
            FB("Formal and academic writing favors extensive noun phrase modification to create:", "density of information and sophistication", "noun-phrases,register,formality"));

        AddExercises(ctx, l10, "c2e10",
            MC("'Success in business depends largely ___ maintaining strong professional relationships.' Which completes correctly?", "on", "in", "for", "with", "prepositions,collocate"),
            MC("'The policy is ___ with the recommendations from international regulatory bodies.'", "in line", "in accordance", "in agreement", "in keeping", "prepositions,fixed,formal"),
            MC("'She proceeded cautiously ___ her past experiences with betrayal and disappointment.'", "in light of", "on the basis of", "by virtue of", "in case of", "prepositions,reason"),
            FB("'The regulations ___ the merger outlined specific requirements for transparency reporting.' (governing, from)", "governing", "prepositions,participle"));

        AddExercises(ctx, l11, "c2e11",
            MC("'Give in to pressure' (yield) is classified as:", "a three-word phrasal verb with inseparable preposition (to)", "a two-word separable phrasal verb", "an intransitive structure", "a literal use of give", "phrasal-verbs,three-word"),
            MC("Separable vs. inseparable: 'look up information' can become 'look information up,' but 'look after children' cannot become 'look children after.' This shows:", "some phrasal verbs are separable, others inseparable", "all phrasal verbs are separable", "word order doesn't matter", "formality determines separability", "phrasal-verbs,separable"),
            MC("Which shows advanced phrasal verb mastery?", "'The company has cut back on overseas expansion' AND 'cut expansion on overseas back' is wrong", "'Get around regulations' (circumvent) vs. 'get around' (move about) show multiple meanings", "Both demonstrate C2 competence", "Neither requires advanced knowledge", "phrasal-verbs,semantic"),
            FB("'The committee ran into unexpected legal obstacles, which required consulting external counsel.' Here 'run into' means:", "encounter or meet (a problem) inadvertently", "phrasal-verbs,encounter"));

        AddExercises(ctx, l12, "c2e12",
            MC("The researcher will ___ her findings at the international conference next month.", "present", "make a presentation", "give a presentation", "present a presentation", "collocations,research"),
            MC("'The company ___ significant losses during the recession.' Which verb is idiomatic?", "sustained", "received", "took", "suffered", "collocations,business"),
            MC("Which collocation is correct for 'draw ___' (attract attention)?", "scrutiny", "notice", "eyes", "focus", "collocations,fixed"),
            FB("'The decision to downsize operations ___ widespread resistance from the workforce.' (meet, encounter)", "encountered", "collocations,business"));

        AddExercises(ctx, l13, "c2e13",
            MC("'The initial proposal was rejected. Nevertheless, the authors persisted with revisions.' This discourse marker shows:", "concession despite setback (contrast)", "addition", "temporal sequence", "cause-effect", "discourse-markers,contrastive"),
            MC("'Three critical issues emerged: first, funding gaps; second, lack of expertise; third, political resistance. Therefore, progress stalled.' 'Therefore' signals:", "cause-effect (the three issues → stalled progress)", "temporal sequence", "concession", "addition", "discourse-markers,causal"),
            MC("'Investment in infrastructure is essential. Moreover, taxation reforms are equally vital.' 'Moreover' is a(n):", "additive discourse marker (adds emphasis to supporting point)", "contrastive marker", "causal marker", "sequential marker", "discourse-markers,additive"),
            FB("'The initial phase faced significant obstacles. Eventually, however, breakthrough innovations emerged.' Here, 'eventually' signals:", "temporal progression despite difficulties (sequential marker)", "discourse-markers,temporal"));

        AddExercises(ctx, l14, "c2e14",
            MC("'The company restructured its operations. This decision sparked considerable debate among stakeholders.' The cohesive link uses:", "demonstrative reference (This → the restructuring)", "lexical cohesion", "conjunction", "substitution", "cohesion,demonstrative"),
            MC("'Critics argue funding is insufficient. Proponents contend resources are adequate.' The second sentence uses:", "reference (ellipsis of 'argue' and 'funding') plus implicit 'that'", "only conjunction", "substitution", "explicit reference", "cohesion,ellipsis"),
            MC("'Scientists conducted three major experiments. Each produced consistent findings.' The link between sentences is:", "reference (Each = the three experiments)", "substitution", "ellipsis", "conjunction only", "cohesion,pronoun"),
            FB("'Several proposals were submitted. The strongest one advocated for immediate action.' Cohesion here depends on:", "lexical repetition (proposals, one—referring back) plus demonstrative reference", "cohesion,reference"));

        AddExercises(ctx, l15, "c2e15",
            MC("'The government raised taxes; citizens protested. The minister resigned; the parliament dissolved early.' This pattern shows:", "nominal ellipsis (entire clause structures omitted after semicolon)", "verbal ellipsis", "substitution", "reference", "ellipsis,nominal"),
            MC("In rapid, formal discourse: 'Which solutions will succeed? Some might, others might not.' The implied structure is:", "others might not [succeed]—ellipsis of the verb 'succeed'", "substitution", "reference", "no ellipsis", "ellipsis,verbal"),
            MC("Comparing: 'She is more experienced than he is' → 'She is more experienced than he.' The ellipsis omits:", "'is' (the copula verb)", "'he'", "'experienced'", "nothing—both are equally complete", "ellipsis,comparative"),
            FB("'The first proposal was rejected; the second, accepted unanimously.' This shows:", "ellipsis of 'was' (or verb phrase), creating parallelism", "ellipsis,reduction"));

        AddExercises(ctx, l16, "c2e16",
            MC("'Will you support the initiative?' 'I will.' Here, 'will' substitutes for:", "'support the initiative' (the entire verb phrase)", "'you'", "nothing—it's not substitution", "'support'", "substitution,auxiliary"),
            MC("'Has he completed the analysis?' 'I think so.' The 'so' substitutes for:", "'he has completed the analysis' (the entire clause)", "'has'", "'analysis'", "'he'", "substitution,so-substitution"),
            MC("'The proposal is controversial. Some agree; others don't.' The 'don't' substitutes for:", "'don't agree' or 'don't think it's controversial'", "'don't propose'", "nothing—it's incomplete", "only 'do'", "substitution,negation"),
            FB("'Would you reconsider if we revised the terms?' 'I might.' Here, 'might' is used as substitution to replace:", "'reconsider if you revised the terms' (the entire conditional)", "substitution,modal"));
    }

    // ── LESSON CONTENT ────────────────────────────────────────────────────────

    private static string A1L1() => C(
        "Greetings are the first words you use when meeting someone (Essential Grammar in Use Elementary, Unit 1). In English, you choose different greetings depending on the time of day and how formal the situation is. British Council and Perfect English Grammar recommend practicing greetings with natural, conversational speed. Watch the video 'Essential Grammar in Use Unit 1' for visual examples and pronunciation practice.",
        ["Use 'Good morning' before noon", "Use 'Good afternoon' from noon to 6 pm", "Use 'Good evening' after 6 pm", "'Hi' and 'Hey' are informal — use with friends only", "Reply to 'How are you?' with: 'Fine, thanks' or 'Great, and you?'"],
        [("Hello", "Hola", "Hello, my name is Ana."), ("Hi", "Hola (informal)", "Hi! How are you?"), ("Good morning", "Buenos días", "Good morning, teacher!"), ("Goodbye / Bye", "Adiós", "Goodbye! See you tomorrow."), ("How are you?", "¿Cómo estás?", "How are you? — Fine, thank you.")],
        [("Good morning! How are you?", "Greeting in the morning"), ("Fine, thank you. And you?", "Responding to 'How are you?'"), ("Nice to meet you!", "Meeting someone for the first time"), ("Goodbye! See you later.", "Saying farewell")],
        "Write a short dialogue (4–6 lines) between two people meeting for the first time. Use at least 3 different greetings. Reference the video for authentic examples.",
        "The 'h' in 'hello' and 'hi' is always pronounced. Say it with a clear breath — HEH-loh, not EH-loh. Listen to the video pronunciation module.",
        [("English Greetings for Beginners", "EnglishClass101", "ozT4MI_oHFc"), ("How to Greet People in English", "BBC Learning English", "oJx2iH0VJQA"), ("Essential Grammar in Use Unit 1 — Greetings", "YouTube: Search 'Essential Grammar in Use Unit 1'", null)]
    );

    private static string A1L2() => C(
        "Numbers 1–20 in English must be memorized (Essential Grammar in Use Elementary, Unit 2). Numbers 13–19 follow a pattern ending in '-teen'. After 20, we combine 'twenty' with units (twenty-one, twenty-two…). Watch 'Essential Grammar in Use Unit 2' for clear pronunciation of numbers and practice with counting.",
        ["1–12 are unique words: one, two, three, four, five, six, seven, eight, nine, ten, eleven, twelve", "13–19 end in '-teen': thirteen, fourteen, fifteen, sixteen, seventeen, eighteen, nineteen", "Watch out! 'thirteen' (13) vs 'thirty' (30) — stress the '-TEEN' ending", "'fifteen' comes from 'five' — spelling changes", "'twelve' is NOT 'twoten' — it is a unique word"],
        [("one / two / three", "uno / dos / tres", "I have two brothers."), ("eleven / twelve", "once / doce", "There are twelve months in a year."), ("thirteen / fifteen / nineteen", "trece / quince / diecinueve", "She is fifteen years old."), ("twenty", "veinte", "I have twenty dollars.")],
        [("I have three cats and two dogs.", "Using small numbers"), ("She is fifteen years old.", "Talking about age"), ("There are twelve students in the class.", "Counting people"), ("My phone number is five-five-three, two-one-four-zero.", "Reading phone numbers")],
        "Write 5 sentences using different numbers: your age, people in your family, your address number, phone digits, and how many hours you sleep. Use the video to check pronunciation.",
        "Stress the '-TEEN': thirTEEN, fourTEEN. Compare: THIRty, FOURty (stress on first syllable for the tens). Listen to the video multiple times.",
        [("English Numbers 1-20", "EnglishClass101", "5Fn4r0gMUFM"), ("Numbers 1-100 in English", "Learn English with Papa English", "92BdANvnwh4"), ("Essential Grammar in Use Unit 2 — Numbers", "YouTube: Search 'Essential Grammar in Use Unit 2'", null)]
    );

    private static string A1L3() => C(
        "Colors in English are adjectives (Essential Grammar in Use Elementary, Unit 3). They always go BEFORE the noun, not after it — opposite to Spanish. In English: 'a red car', never 'a car red'. Watch 'Essential Grammar in Use Unit 3' for visual examples and color vocabulary practice.",
        ["Color comes BEFORE the noun: 'a red car', 'the blue sky'", "Primary colors: red, blue, yellow", "Secondary: green (blue+yellow), orange (red+yellow), purple (red+blue)", "Neutral: black, white, grey, brown", "Light/dark: light blue, dark green"],
        [("red", "rojo", "I have a red bag."), ("blue", "azul", "The sky is blue."), ("green", "verde", "Grass is green."), ("yellow", "amarillo", "Bananas are yellow."), ("orange", "naranja", "She wears an orange shirt."), ("white / black", "blanco / negro", "Snow is white. His car is black.")],
        [("The sky is blue on a clear day.", "Describing nature"), ("I have a red umbrella.", "Color before noun"), ("She is wearing a green dress.", "Describing clothes"), ("What is your favorite color? — My favorite color is purple.", "Asking preferences")],
        "Look around you right now. Describe 6 objects using colors. Example: 'My notebook is blue. My pen is black.'",
        "'Orange' is pronounced OR-inj (two syllables). 'White' — the 'wh' sounds like a regular 'w' in modern English.",
        [("Learn Colors in English", "Learn English Kids", "U_8dO7WcHUE"), ("English Vocabulary: Colors", "Papa English", "cKGPRrFT-zU"), ("Essential Grammar in Use Unit 3 — Colors", "YouTube: Search 'Essential Grammar in Use Unit 3'", null)]
    );

    private static string A1L4() => C(
        "Knowing the names of objects in your classroom or study space helps you communicate (Essential Grammar in Use Elementary, Unit 4). We use 'a' before consonant sounds and 'an' before vowel sounds. Watch 'Essential Grammar in Use Unit 4' for classroom object vocabulary and article practice.",
        ["Use 'a' before consonant sounds: a pen, a book, a chair", "Use 'an' before vowel sounds: an eraser, an umbrella", "Plural: add -s to most nouns (pen → pens, book → books)", "Add -es after -ch, -sh, -x, -s, -o: box → boxes", "Commands: 'Open your book', 'Close the door', 'Take a pencil'"],
        [("pen", "bolígrafo", "Write with a pen."), ("pencil", "lápiz", "Use a pencil for the test."), ("book", "libro", "Open your book to page 5."), ("desk", "escritorio", "Put the book on the desk."), ("chair", "silla", "Sit on the chair."), ("eraser", "goma de borrar", "I need an eraser.")],
        [("Take out a pen and a piece of paper.", "Following classroom instructions"), ("The book is on the desk.", "Describing location"), ("There are thirty chairs in the room.", "Counting classroom objects"), ("Open your book and go to page twelve.", "Teacher instruction")],
        "Describe your study area. Write 6 sentences: what objects are there, their colors, and how many. Example: 'I have two blue pens on my desk.'",
        "In American English: 'eraser'. In British English: 'rubber'. Both are correct — just know your context.",
        [("School Objects English Vocabulary", "7 E S L", "j6LvKqGTkSg"), ("Classroom Vocabulary in English", "Learn English with EnglishClass101.com", "RPHJ81CJbMI"), ("Essential Grammar in Use Unit 4 — Articles & Objects", "YouTube: Search 'Essential Grammar in Use Unit 4'", null)]
    );

    private static string A1L5() => C(
        "Family vocabulary helps you talk about the people closest to you. Learning family words is one of the first steps in language learning, as you constantly refer to family in daily conversation. Perfect English Grammar emphasizes the importance of mastering these foundational words.",
        ["Mother (mom) — your female parent", "Father (dad) — your male parent", "Brother — male sibling", "Sister — female sibling", "Grandmother and grandfather — your parents' parents", "Son and daughter — your children"],
        [("mother / mom", "madre / mamá", "My mother is a doctor."), ("father / dad", "padre / papá", "My dad works in an office."), ("brother", "hermano", "I have two brothers."), ("sister", "hermana", "My sister is younger than me."), ("grandmother / grandma", "abuela / abuelita", "My grandmother is eighty years old."), ("grandfather / grandpa", "abuelo / abuelito", "My grandfather lives with us.")],
        [("I have a big family with two brothers and one sister.", "Talking about family size"), ("My mother is a teacher and my father is an engineer.", "Describing family occupations"), ("I live with my parents and my grandmother.", "Describing who you live with"), ("My brother is five years older than me.", "Comparing ages")],
        "Draw your family tree or write about your family. Use at least 8 sentences describing who is in your family, what they do, and how old they are.",
        "'Family' rhymes with 'jam' — FAM-i-lee (3 syllables). 'Sister' — SIS-ter (2 syllables). 'Brother' — BRUH-ther (2 syllables). Stress the first syllable in all family words.",
        [("Family Members Vocabulary", "Learn English Kids", null), ("English Family Words", "EnglishClass101", null), ("How to Talk About Your Family", "Papa English", null)]
    );

    private static string A1L6() => C(
        "Personal pronouns and the verb 'to be' are the foundation of English grammar. You cannot construct a single sentence without them. These 8 pronouns (I, you, he, she, it, we, you, they) are the most important words in English, and 'to be' is the most used verb in the language.",
        ["I, you, he, she, it, we, they — the 8 personal pronouns", "Verb 'to be' in present: am (I), are (you/we/they), is (he/she/it)", "Contraction: I'm, you're, he's, she's, it's, we're, they're", "Negative: I'm not, you're not (you aren't), he's not (he isn't)", "Question word order: 'Are you a student?' — Subject and 'to be' swap places"],
        [("I", "yo", "I am a student."), ("you", "tú", "You are my friend."), ("he / she / it", "él / ella / eso", "He is a teacher. She is happy. It is big."), ("we", "nosotros", "We are in the class."), ("they", "ellos", "They are my friends."), ("to be", "ser/estar", "I am happy. You are kind.")],
        [("I am from Spain.", "Talking about origin"), ("You are a good friend.", "Describing others"), ("She is a doctor and he is a nurse.", "Describing occupations"), ("We are in the park.", "Describing location"), ("They are very nice people.", "Describing group")],
        "Write 10 sentences about yourself and people around you using different pronouns. Example: 'I am happy. You are smart. She is my teacher.'",
        "'I am' contracts to 'I'm' (I'm, not Iam). 'You are' → 'You're'. 'He is' → 'He's'. These contractions are very common in spoken English.",
        [("English Pronouns: I, You, He, She, It", "Learn English with Papa English", null), ("The Verb 'To Be' in English", "English Speeches", null), ("Personal Pronouns and Present 'To Be'", "Cambridge English", null)]
    );

    private static string A1L7() => C(
        "Days of the week and months of the year are essential for discussing schedules, plans, and dates. Capitalizing day and month names is a key rule in English — always capitalize them. The British Council emphasizes the importance of this distinction for written accuracy.",
        ["Days of the week: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday", "Months of the year: January, February, March, April, May, June, July, August, September, October, November, December", "ALWAYS capitalize days and months — this is a key grammar rule", "Abbreviations: Mon., Tues., Wed., etc. — Saturday and Sunday shorten to Sat. and Sun.", "On + day: 'On Monday', 'On Friday' — on is the correct preposition"],
        [("Monday", "lunes", "I have English class on Monday."), ("Friday", "viernes", "We play football on Friday."), ("January", "enero", "My birthday is in January."), ("December", "diciembre", "Christmas is in December."), ("week", "semana", "There are seven days in a week."), ("month", "mes", "There are twelve months in a year.")],
        [("I work on Mondays and Wednesdays.", "Discussing weekly schedule"), ("My birthday is in July.", "Talking about birth month"), ("The meeting is on Friday at 10 o'clock.", "Planning a specific day"), ("January is cold but February is colder.", "Comparing months")],
        "Create a weekly schedule. Write where you are or what you do on each day of the week. Example: 'On Monday, I go to school. On Wednesday, I have English class.'",
        "'Tuesday' is tricky — TUE-s-day or CHOOS-day. 'Thursday' — THURS-day (sounds like 'Thursday', not 'Thurday'). Practice the difficult days: Tuesday, Wednesday, Thursday.",
        [("Days of the Week in English", "Learn English Kids", null), ("English Months and Seasons", "Papa English", null), ("Days of the Week and Months", "Cambridge English", null)]
    );

    private static string A1L8() => C(
        "Food and drinks vocabulary opens conversations about eating habits, preferences, and meals. These words are used in daily life constantly — asking what someone ate, what they like, or ordering food. The British Council highlights the social importance of food vocabulary for real-world interaction.",
        ["Meals: breakfast (morning), lunch (midday), dinner/supper (evening)", "Drinks: water, milk, juice, tea, coffee, soda", "Fruits: apple, banana, orange, strawberry, grape", "Vegetables: carrot, potato, tomato, broccoli, lettuce", "Other foods: bread, rice, meat, chicken, fish, egg"],
        [("apple", "manzana", "An apple a day keeps the doctor away."), ("bread", "pan", "I eat bread for breakfast."), ("coffee", "café", "My dad drinks coffee every morning."), ("chicken", "pollo", "We have chicken for dinner."), ("water", "agua", "Drink plenty of water."), ("rice", "arroz", "I like rice with vegetables.")],
        [("I like apples and oranges.", "Talking about food preferences"), ("She drinks coffee in the morning.", "Describing eating habits"), ("What do you have for lunch?", "Asking about meals"), ("We eat chicken and rice for dinner.", "Describing meals"), ("Do you like vegetables?", "Asking preferences")],
        "Describe your favorite meal. Write 8 sentences about what you eat, what you drink, which foods you like and dislike, and when you eat it.",
        "'Breakfast' — BREAK-fast (2 syllables). 'Dinner' — DIN-er (2 syllables). 'Vegetables' — VEJ-uh-tulz (3 syllables). Many words have stress on the first syllable.",
        [("Food Vocabulary in English", "Learn English with Papa English", null), ("English Food and Drinks", "English Speeches", null), ("Meals and Food Vocabulary", "Cambridge English", null)]
    );

    private static string A2L1() => C(
        "Past Simple describes completed actions in the past. Regular verbs add -ED to the base form. This form is the SAME for ALL subjects — I, you, he, she, we, they.",
        ["Base verb + ED: work→worked, play→played, walk→walked", "Verb ending in E: add only D: live→lived, like→liked", "Short vowel + single consonant → double it: stop→stopped, plan→planned", "Consonant + Y → change to -IED: study→studied, try→tried", "Time markers: yesterday, last week, in 1990, ago, when"],
        [("worked", "trabajé", "I worked in that office for two years."), ("studied", "estudié", "She studied all night for the exam."), ("stopped", "paré", "The car stopped suddenly."), ("lived", "viví", "They lived in Paris for five years."), ("played", "jugué", "He played football every day.")],
        [("I worked hard yesterday.", "Regular past action"), ("She studied medicine at university.", "Regular past verb"), ("They stopped at a café.", "Double consonant rule"), ("We walked to the park.", "Simple past action")],
        "Write 8 sentences about what you did yesterday or last week using at least 6 different regular past verbs.",
        "-ED has 3 pronunciations: /t/ (worked, stopped), /d/ (played, called), /ɪd/ (wanted, needed). The pronunciation depends on the final sound.",
        [("Past Simple Regular Verbs", "Papa English", null), ("Regular Past Tense", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L2() => C(
        "Many common English verbs are irregular — they do NOT follow the -ED rule. Common irregular verbs: go→went, have→had, see→saw, come→came, give→gave, eat→ate, drink→drank, write→wrote.",
        ["Common: go→went, have→had, see→saw, come→came, give→gave, say→said", "More: eat→ate, drink→drank, write→wrote, read→read, take→took, make→made", "More: know→knew, think→thought, buy→bought, meet→met, sit→sat", "Negatives: I didn't go, she didn't have (base verb!)", "Questions: Did you see? Did he come? (base verb!)"],
        [("went (go)", "fui", "I went to the cinema last night."), ("had (have)", "tuve", "She had a great time at the party."), ("saw (see)", "vi", "We saw a great film yesterday."), ("ate (eat)", "comí", "They ate pizza for dinner."), ("wrote (write)", "escribí", "He wrote a long email to her.")],
        [("I went to the beach last summer.", "go → went"), ("She had coffee and toast for breakfast.", "have → had"), ("They saw the new film yesterday.", "see → saw"), ("He wrote her a letter.", "write → wrote")],
        "Write a paragraph about a memorable day using at least 8 different irregular past verbs. You can write about a birthday, trip, or special event.",
        "'Read' looks the same but sounds different: REED (present) vs RED (past). 'Thought' is pronounced THAWT. Irregular verbs are more common in spoken English.",
        [("Irregular Past Tense Verbs", "Papa English", null), ("Common Irregular Verbs", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L3() => C(
        "Past Continuous describes actions that were happening at a specific moment in the past. Form: was/were + verb-ING. Use it for: interrupted actions, simultaneous actions, and background situations in the past.",
        ["Form: Subject + was/were + verb-ING", "I was working, you were reading, he was sleeping, we were playing", "Time markers: at 3 o'clock, when..., while..., at that moment", "Negatives: was not (wasn't) / were not (weren't) + verb-ING", "Questions: Was/Were + subject + verb-ING?"],
        [("was working", "estaba trabajando", "I was working on a project."), ("were sleeping", "estaban durmiendo", "The children were sleeping when the storm came."), ("were playing", "estaban jugando", "They were playing football at the moment."), ("was studying", "estaba estudiando", "She was studying when her friend called.")],
        [("I was working when you called.", "Interrupted action"), ("They were eating dinner when the fire alarm went off.", "Background action"), ("She was reading a book at that moment.", "Action at a specific time"), ("We were building a house last year.", "Long-term past action")],
        "Write 8 sentences about what you and others were doing at a specific time last week. Use past continuous with time markers.",
        "Past Continuous: stress the first syllable. WAS: /wɑz/ or /wəz/ (weak form). WERE: /wɜr/ or /wər/ (weak form). The ING ending is one syllable.",
        [("Past Continuous Tense", "Papa English", null), ("English Past Continuous", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L4() => C(
        "Form questions and negatives in Past Continuous. Questions: Was/Were + subject + verb-ING? Negatives: was not / were not + verb-ING. Use these to ask about what was happening or to state what was NOT happening.",
        ["Yes/No questions: Was/Were + subject + verb-ING?", "WH- questions: What/Where/When/Why + was/were + subject + verb-ING?", "Negatives: I wasn't / he wasn't / they weren't + verb-ING", "Short answers: Yes, I was. / No, she wasn't.", "Common: What was she doing? Were they playing football?"],
        [("Was he working?", "¿Estaba trabajando?", "Was he working when you arrived?"), ("weren't playing", "no estaban jugando", "They weren't playing football."), ("What were you doing?", "¿Qué estabas haciendo?", "What were you doing at noon?"), ("Was it raining?", "¿Estaba lloviendo?", "Was it raining when you left?"), ("She wasn't listening.", "Ella no estaba escuchando.", "She wasn't listening to the teacher.")],
        [("Was she sleeping? — Yes, she was.", "Yes/no question"), ("What were they doing? — They were cooking.", "Wh- question"), ("Weren't they watching TV? — No, they weren't.", "Negative question"), ("I wasn't paying attention.", "Negative statement")],
        "Write 10 past continuous questions and negatives: 5 yes/no questions and 5 negative statements.",
        "Past Continuous questions: The order is WAS/WERE + subject + verb-ING. Wrong: 'He was doing what?' Right: 'What was he doing?'",
        [("Past Continuous Questions & Negatives", "Learn English with EnglishClass101.com", null), ("Past Continuous Tense", "English Speeches", null)]
    );

    private static string A2L5() => C(
        "Present Perfect connects past actions to the present. Form: have/has + past participle. Regular verbs: have worked, has played. Irregular: have eaten, has been. Use it for: life experiences, recent actions, and situations that started in the past and continue.",
        ["Form: Subject + have/has + past participle", "I/you/we/they + have: I have worked, you have played", "He/she/it + has: He has worked, she has played", "Regular: base + -ed (worked, played, lived)", "Irregular: went, seen, eaten, been, taken, written"],
        [("have worked", "he trabajado", "I have worked there for five years."), ("has lived", "ha vivido", "She has lived in London since 1990."), ("have eaten", "he comido", "We have eaten Thai food before."), ("has been", "ha sido", "He has been to France three times."), ("have finished", "hemos terminado", "They have finished their homework.")],
        [("I have lived in this city for ten years.", "Life experience"), ("She has worked here since 2020.", "Action starting in past, continuing"), ("Have you ever visited Japan?", "Life experience question"), ("We have completed the project.", "Recent completed action")],
        "Write 10 sentences using present perfect. Include 5 sentences about your life experiences and 5 about recent actions.",
        "Present Perfect emphasizes the connection between past and present. Don't say 'I went' when the result is relevant now; say 'I have been'.",
        [("Present Perfect Tense", "Papa English", null), ("Present Perfect English Grammar", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L6() => C(
        "Present Perfect questions ask about life experiences or actions starting in the past. Form: Have/Has + subject + past participle? Common question patterns: Have you ever...? / How long have you...? / Where have you been?",
        ["Yes/No questions: Have you worked? / Has she lived?", "WH- questions: Where have you been? / What have they done? / How long have you lived?", "Ever/never: Have you ever been to Italy? / I've never seen snow.", "How long + present perfect: How long have you been here?", "Short answers: Yes, I have. / No, she hasn't."],
        [("Have you ever?", "¿Alguna vez has...?", "Have you ever travelled by train?"), ("How long have you lived?", "¿Cuánto tiempo llevas viviendo?", "How long have you lived here?"), ("Where have you been?", "¿Dónde has estado?", "Where have you been?"), ("Has she finished?", "¿Ha terminado?", "Has she finished her homework?"), ("What have they done?", "¿Qué han hecho?", "What have they done so far?")],
        [("Have you ever been to Paris? — Yes, I have.", "Life experience question"), ("How long have you worked here? — For three years.", "Duration question"), ("Where have you been? — I've been to the market.", "Location question"), ("She hasn't finished yet.", "Negative with yet")],
        "Write 12 present perfect questions: 4 'Have you ever...' questions, 4 'How long have you...' questions, and 4 WH- questions.",
        "In Present Perfect questions, the auxiliary (have/has) comes before the subject: 'Have you been?' NOT 'You have been?' Never use past tense with 'yet' in negatives.",
        [("Present Perfect Questions", "Learn English with EnglishClass101.com", null), ("Have vs Has Questions", "English Speeches", null)]
    );

    private static string A2L7() => C(
        "CAN expresses present ability, permission, and possibility. Form: can + base verb. All subjects use the SAME form — no -s. Negatives: can't (cannot). Questions: Can you...?",
        ["Present ability: I can swim, she can cook, they can speak English", "Permission: Can I go? / Yes, you can. / No, you can't.", "Possibility: It can be expensive. / Animals can be dangerous.", "No -s: 'He CAN swim' — NOT 'He cans swim'", "Negatives: I can't / she can't / they can't"],
        [("can", "puedo", "I can play guitar."), ("can't", "no puedo", "She can't drive yet."), ("Can you?", "¿Puedes?", "Can you help me please?"), ("able to", "capaz de", "I am able to understand French."), ("couldn't", "no pude", "I couldn't open the door.")],
        [("I can cook Italian food.", "Present ability"), ("She can't speak German.", "Negative ability"), ("Can you help me?", "Polite request"), ("Animals can be dangerous.", "Possibility")],
        "Write 8 sentences: 4 using 'can' for ability and 4 using 'can' for permission or possibility.",
        "CAN has two pronunciations: strong form /kæn/ and weak form /kən/. In questions, 'Can you...?' uses weak form: 'kən yə?'",
        [("Modal Verb: CAN", "Papa English", null), ("Can and Could in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L8() => C(
        "COULD has two main uses: past ability ('I could swim as a child') and polite requests ('Could you help me?'). Form: could + base verb. All subjects use the SAME form. COULD for past ability is more common than 'was able to'.",
        ["Past ability: I could swim when I was young, she could dance", "Polite requests: Could you help me? / Could I ask a question?", "Negative: couldn't (could not) for past or requests", "Questions: Could you...? / Could he...?", "Note: 'could' also expresses possibility: 'It could rain tomorrow'"],
        [("could", "podía", "I could dance well as a child."), ("couldn't", "no podía", "She couldn't speak English before."), ("Could you...?", "¿Podrías?", "Could you open the window please?"), ("was able to", "pude lograr", "He was able to finish the race."), ("might", "podría", "It could rain tomorrow.")],
        [("When I was younger, I could climb trees.", "Past ability"), ("She couldn't cook when she was 15.", "Negative past ability"), ("Could you help me with this?", "Polite request"), ("Could he swim? — Yes, he could.", "Past ability question")],
        "Write 10 sentences: 5 about past abilities using 'could', and 5 polite requests using 'Could you...?'",
        "COULD is pronounced /kʊd/. In polite requests, 'Could you..?' sounds more formal than 'Can you...?' Use COULD for past events, not present ability.",
        [("Could - Past Tense of Can", "Learn English with EnglishClass101.com", null), ("Modals of Ability in English", "English Speeches", null)]
    );

    private static string A2L9() => C(
        "SHOULD gives advice and recommendations. Form: should + base verb. All subjects use the SAME form. Negatives: shouldn't (should not). Use SHOULD for 'good ideas', 'it's wise to...', 'in my opinion you should...'",
        ["Present advice: I should study, you should rest, she should see a doctor", "All subjects same: should + base verb (no -s)", "Meaning: advice, good ideas, recommendations, opinions", "Negatives: shouldn't (should not)", "Compare: SHOULD = advice, MUST = obligation"],
        [("should", "debería", "You should drink more water."), ("shouldn't", "no debería", "She shouldn't work so hard."), ("should I?", "¿Debería?", "Should I call him?"), ("advice", "consejo", "Can you give me some advice?"), ("recommend", "recomendar", "I recommend you see that film.")],
        [("You should eat more vegetables.", "Health advice"), ("She shouldn't stay up so late.", "Negative advice"), ("I think you should apologize.", "Polite advice"), ("You should take a break.", "Recommendation")],
        "Give advice: Write 10 sentences with SHOULD/SHOULDN'T to advise a friend on health, study, work, and relationships.",
        "SHOULD /ʃʊd/ sounds similar to COULD /kʊd/ but means different things. SHOULD = good idea. MUST = necessity. Don't confuse them!",
        [("Modal: SHOULD", "Papa English", null), ("Advice with SHOULD", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L10() => C(
        "MUST expresses obligation, necessity, and prohibition. Form: must + base verb. All subjects use the SAME form. Negatives: mustn't (must not) means 'it is prohibited'. Don't confuse with 'don't have to' (not necessary).",
        ["Obligation: You must wear a seatbelt, she must arrive on time", "Prohibition: You mustn't smoke here, students mustn't use phones in class", "All subjects same: must + base verb (no -s)", "Negatives: mustn't (must not) = prohibition", "Compare: MUST = obligation, SHOULD = advice"],
        [("must", "debes", "You must wear a seatbelt."), ("mustn't", "no debes", "You mustn't tell anyone."), ("must I?", "¿Tengo que?", "Must I attend the meeting?"), ("obligation", "obligación", "It is a legal obligation."), ("prohibition", "prohibición", "Smoking is a prohibition here.")],
        [("You must arrive on time.", "Obligation"), ("Students mustn't use phones in class.", "Prohibition"), ("Must I do the homework?", "Necessity question"), ("I must study for the exam.", "Strong obligation")],
        "Write 10 sentences with MUST/MUSTN'T: 5 about obligations and 5 about prohibitions.",
        "MUST /mʌst/ is pronounced with a short 'u' sound. MUSTN'T /ˈmʌsənt/ has a schwa in the middle. MUST is stronger than SHOULD.",
        [("Modal: MUST", "Papa English", null), ("Must vs Should", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L11() => C(
        "Comparative adjectives compare two things. Form: -ER + THAN (big→bigger) or MORE + THAN (beautiful→more beautiful). Short adjectives use -ER, long adjectives use MORE.",
        ["One syllable: big→bigger, fast→faster, cold→colder", "Two syllables ending in -y: happy→happier, busy→busier", "Two or more syllables: beautiful→more beautiful, important→more important", "Irregular: good→better, bad→worse, far→farther/further", "Structure: Subject + verb + comparative + THAN + object"],
        [("bigger", "más grande", "Your house is bigger than mine."), ("happier", "más feliz", "She is happier now."), ("more beautiful", "más hermoso/a", "This city is more beautiful than that one."), ("worse", "peor", "The weather is worse today."), ("faster", "más rápido", "He is faster than his brother.")],
        [("My car is bigger than yours.", "Comparing two things"), ("January is colder than December in my country.", "Comparative in context"), ("She is more intelligent than her friend.", "Two-syllable adjective"), ("The red dress is nicer than the blue one.", "Clothing comparison")],
        "Write 10 comparative sentences comparing two people, things, or places using both -ER and MORE forms.",
        "Comparative pronunciation: 'bigger' /ˈbɪɡər/ and 'most beautiful' /ˌmoʊst ˈbjutɪfl/. Always use THAN after the comparative.",
        [("Comparative Adjectives", "Papa English", null), ("English Comparatives", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L12() => C(
        "Superlative adjectives identify the best, worst, or most extreme in a group. Form: THE + -EST (big→the biggest) or THE + MOST (beautiful→the most beautiful). Always use THE before superlatives.",
        ["One syllable: the big→the biggest, the fast→the fastest", "Two syllables ending in -y: the happy→the happiest, the busy→the busiest", "Two or more syllables: the beautiful→the most beautiful, the important→the most important", "Irregular: good→the best, bad→the worst, far→the farthest/furthest", "Always use THE: 'the best film', NOT 'best film'"],
        [("the biggest", "el más grande", "This is the biggest house in the city."), ("the happiest", "el más feliz", "She is the happiest person I know."), ("the most beautiful", "el más hermoso/a", "This is the most beautiful city in Europe."), ("the worst", "el peor", "That was the worst day of my life."), ("the fastest", "el más rápido", "He is the fastest runner in the team.")],
        [("This is the best film I've ever seen.", "Superlative with 'ever'"), ("She is the most intelligent student in the class.", "Superlative comparison"), ("It was the coldest winter we had.", "Extreme in group"), ("These are the most expensive shoes in the store.", "Superlative in context")],
        "Write 10 superlative sentences describing the best, worst, or most extreme of people, things, or places you know.",
        "Superlative pronunciation: 'biggest' /ˈbɪɡɪst/ and 'most beautiful' /ˌmoʊst ˈbjutɪfl/. Always use THE before the superlative adjective.",
        [("Superlative Adjectives", "Papa English", null), ("English Superlatives", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L13() => C(
        "Future with 'going to' expresses plans, intentions, and expectations. Form: am/is/are + going to + base verb. Use for decisions made before speaking or obvious future events.",
        ["Positive: I'm going to visit my family next week", "All forms: I am going to / you are going to / he is going to", "Time markers: tomorrow, next week, later, this weekend", "Negatives: am not / is not / are not going to", "Questions: Are you going to...? / Is she going to...?"],
        [("going to", "ir a", "I'm going to study tomorrow."), ("are going to", "van a", "They are going to buy a new house."), ("is going to", "va a", "She is going to travel next month."), ("aren't going to", "no van a", "We aren't going to the party."), ("Are you going to...?", "¿Vas a...?", "Are you going to the beach?")],
        [("I'm going to watch a film tomorrow.", "Plan"), ("She is going to study medicine at university.", "Intention"), ("We're going to have a picnic this Sunday.", "Arrangement"), ("It's going to rain later.", "Obvious future event")],
        "Write 10 sentences about your plans and intentions using 'going to': 5 positive and 5 negative.",
        "'Going to' is often pronounced 'gonna' /ˈɡɑnə/ in informal speech, but write 'going to' formally. Stress the time marker word.",
        [("Future with Going To", "Papa English", null), ("Going To English Grammar", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L14() => C(
        "Questions and negatives with 'going to'. Form: Are/Is/Am + subject + going to + base verb? Negatives: am/is/are + not going to. Use to ask about plans or state what will NOT happen.",
        ["Yes/No questions: Are you going to...? / Is she going to...?", "WH- questions: What are you going to do? / Where are they going to go?", "Negatives: I'm not going to / she isn't going to / they aren't going to", "Short answers: Yes, I am. / No, she isn't.", "Time markers help context: When? / What time? / Which day?"],
        [("Are you going to...?", "¿Vas a...?", "Are you going to the party tonight?"), ("What are you going to do?", "¿Qué vas a hacer?", "What are you going to do tomorrow?"), ("isn't going to", "no va a", "She isn't going to come."), ("Where are they going to...?", "¿Dónde van a...?", "Where are they going to meet?"), ("going to", "ir a", "I'm going to study later.")],
        [("Are you going to the cinema? — Yes, I am.", "Yes/no question"), ("What is he going to do tomorrow?", "Wh- question"), ("We aren't going to watch TV tonight.", "Negative statement"), ("Is she going to arrive on time? — No, she isn't.", "Question with negative answer")],
        "Write 12 future 'going to' questions and negatives: 6 yes/no questions and 6 negative statements.",
        "In questions with 'going to', the verb order is: AM/IS/ARE + subject + GOING TO + base verb. Wrong: 'You are going to do what?' Right: 'What are you going to do?'",
        [("Going To Questions", "Learn English with EnglishClass101.com", null), ("Future with Going To", "English Speeches", null)]
    );

    private static string A2L15() => C(
        "Future with 'will' makes predictions about the future based on what you think will happen. Form: will + base verb. Use for predictions, spontaneous decisions, and facts about the future.",
        ["Predictions: It will rain tomorrow / I think she will win", "All subjects same: I will / you will / he will / she will", "Negatives: will not (won't) + base verb", "Questions: Will you...? / Will it...?", "Contractions: I'll, you'll, he'll, she'll, it'll, we'll, they'll"],
        [("will", "verb", "I think it will rain tomorrow."), ("won't", "no + verb", "She won't come to the party."), ("Will you...?", "¿... + verb?", "Will you help me?"), ("'ll", "will (short)", "I'll see you tomorrow."), ("happen", "ocurrir", "What will happen next?")],
        [("I think it will be sunny tomorrow.", "Prediction"), ("She will finish her work soon.", "Future fact"), ("Will he arrive on time?", "Question about future"), ("I'll help you with that.", "Spontaneous offer")],
        "Write 10 sentences making predictions about the future using 'will': 5 positive and 5 negative.",
        "WILL is pronounced /wɪl/ (strong) or /əl/ (weak). Contractions: I'LL /aɪl/, WON'T /woʊnt/ (irregular). Use 'will' for predictions, 'going to' for plans.",
        [("Future with Will", "Papa English", null), ("Will Future Tense", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L16() => C(
        "Promises and spontaneous decisions with 'will'. Use 'will' when you decide to do something at the moment of speaking, or to make promises. Form: will + base verb. This shows willingness or commitment.",
        ["Promises: I will help you / She will call you tomorrow", "Spontaneous decisions: I'll buy the tickets / You'll love that restaurant", "All subjects same: will + base verb", "Negatives: won't / will not", "Common: 'I promise I will...', 'Don't worry, I will...'"],
        [("will", "prometerá", "I promise I will be on time."), ("won't", "no", "Don't worry, it won't happen again."), ("I'll", "voy a", "I'll help you with that."), ("She will", "ella", "She will take care of it."), ("promise", "prometo", "I promise I will call you.")],
        [("I will help you move house.", "Promise"), ("Don't worry, I won't tell anyone.", "Promise of secrecy"), ("I'll buy the groceries on my way home.", "Spontaneous decision"), ("She will remember to send you a message.", "Promise about someone else")],
        "Write 10 sentences: 5 making promises with 'will' and 5 making spontaneous decisions with 'I'll'.",
        "'Will' in promises emphasizes commitment. I WILL do it (commitment) vs. I'm going to do it (plan already made). WON'T /woʊnt/ is irregular contraction of 'will not'.",
        [("Promises with Will", "Learn English with EnglishClass101.com", null), ("Spontaneous Decisions - Will", "English Speeches", null)]
    );

    private static string B1L1() => C(
        "Present Perfect Continuous emphasizes duration and shows actions that started in the past and continue now. 'How long have you been doing this?' — Form: have/has + been + verb-ING. Essential for discussing ongoing activities, work, and habits that continue to the present moment.",
        ["Form: have/has + been + verb-ING", "Focus: duration and continuity of the action", "'How long have you been working here?' → emphasis on time spent", "'I have worked here 5 years' (how many jobs?) vs 'I have been working here 5 years' (how long?)", "Common: 'I've been waiting for 20 minutes', 'She's been studying all day', 'We've been living here since 2020'"],
        [("have been working", "he estado trabajando", "I have been working on this project for weeks."), ("has been studying", "ha estado estudiando", "She has been studying English for 3 years."), ("have been living", "he estado viviendo", "We have been living in Barcelona since 2020."), ("has been waiting", "ha estado esperando", "He has been waiting for an hour.")],
        [("How long have you been learning English? — For two years.", "Duration question"), ("She has been working here since she graduated.", "Ongoing work situation"), ("They have been playing football all afternoon.", "Activity in progress"), ("I have been trying to call you all day.", "Repeated or continuous attempts")],
        "Write 8 Present Perfect Continuous sentences about your life: work, studies, hobbies, or projects. Include 'how long' in at least 3 sentences.",
        "The ING ending adds syllables. Stress pattern: 'I HAVE been STUDying' — stress on 'have' and the participle. In fast speech: 'I've-bin-STUDY-ing'.",
        [("Present Perfect Continuous Tense", "Papa English", null), ("How Long Have You Been...", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L2() => C(
        "Present Perfect Continuous questions and negatives ask about duration and express what has NOT been happening. Form: have/has (not) + been + verb-ING. Use 'How long...?' questions to ask about ongoing activities and their duration.",
        ["Questions: 'How long have you been waiting?', 'What have you been doing?', 'Where have you been living?'", "Negatives: haven't been / hasn't been + verb-ING", "'Have you been feeling ill?' → emphasizes the duration of the feeling", "Time expressions: How long? / since / for / all day / recently / lately", "Short answers: 'Yes, I have been.' / 'No, I haven't been.'"],
        [("Have you been waiting?", "¿Llevas esperando?", "How long have you been waiting?"), ("hasn't been working", "no ha estado trabajando", "She hasn't been working properly."), ("What have you been doing?", "¿Qué has estado haciendo?", "What have you been doing all day?"), ("haven't been sleeping", "no hemos estado durmiendo", "I haven't been sleeping well lately.")],
        [("How long have you been learning English? — For two years.", "Duration question"), ("What have you been working on? — A new project.", "Activity question"), ("Haven't you been complaining about that?", "Emphasis on duration"), ("They haven't been studying for the exam.", "Negative statement")],
        "Write 10 questions and negatives using Present Perfect Continuous: 5 'How long...?' questions and 5 negative statements with 'haven't/hasn't been'.",
        "In questions, 'been' is usually unstressed: 'How LONG have you BIN waiting?' The stress falls on the question word and participle.",
        [("Present Perfect Continuous Questions", "Learn English with EnglishClass101.com", null), ("Questions with How Long", "English Speeches", null)]
    );

    private static string B1L3() => C(
        "Past Perfect (Pluperfect) shows which event happened FIRST when discussing two past events. Form: had + past participle. Crucial for storytelling, narrative, and clarifying sequence in complex past situations. Essential for B1+ level fluency.",
        ["Form: had + past participle (worked, gone, eaten, finished)", "Shows the earlier of two past events: 'Before they arrived, I had already finished'", "Time expressions: after, before, when, by the time, by + noun, had just", "Often abbreviated: 'I'd finished', 'She'd eaten', 'They'd left'", "Compare Past Simple: 'I finished at 5 PM' vs Past Perfect: 'I had finished before he arrived'"],
        [("had finished", "había terminado", "Before the concert started, she had finished her homework."), ("had eaten", "había comido", "By the time he arrived, we had already eaten."), ("had never seen", "nunca había visto", "I had never seen snow before that day."), ("had left", "había salido", "When they called, I had already left the house.")],
        [("She had studied French before she moved to Paris.", "Earlier event → later event"), ("By the time I woke up, everyone had left.", "Clear sequence with 'by the time'"), ("He realized he had forgotten his passport.", "Past realization about earlier action"), ("After she had finished university, she traveled the world.", "Completed past action before another")],
        "Write 8 Past Perfect sentences telling stories where one event happened before another. Use time expressions: before, after, by the time, by the time, as soon as, had just.",
        "'Had' is often contracted: 'I'd finished', 'she'd eaten'. In rapid speech: 'I-fin-isht'. The stress falls on the past participle, not 'had'.",
        [("Past Perfect Tense Explained", "Papa English", null), ("Had vs Did - Past Perfect", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L4() => C(
        "Past Perfect Continuous shows how long an action had been happening before another event in the past. Form: had + been + verb-ING. Use for: expressing duration before a past event, background actions in narratives, and explaining ongoing past situations.",
        ["Form: had + been + verb-ING", "Shows: duration before a past event", "'By the time he called, I had been working for three hours'", "'When she arrived, they had been waiting for an hour'", "Compare: 'I had worked' (completed) vs 'I had been working' (duration emphasized)"],
        [("had been working", "había estado trabajando", "He had been working there for five years before he moved."), ("had been waiting", "había estado esperando", "By noon, she had been waiting on the project for three hours."), ("had been studying", "había estado estudiando", "When I arrived, they had been studying for ten hours."), ("had been living", "había estado viviendo", "When war broke out, they had been living abroad for a decade.")],
        [("By the time he arrived, I had been waiting for an hour.", "Duration before a past event"), ("When the manager arrived, they had been working on it for days.", "Background action"), ("She had been teaching for thirty years when she retired.", "Total duration"), ("We had been traveling for weeks when we finally arrived home.", "Long journey narrative")],
        "Write 8 Past Perfect Continuous sentences about past experiences, travel, work, or projects where one action lasted for a duration before another happened.",
        "Past Perfect Continuous: had-BIN-working. 'Been' is unstressed. Stress falls on the participle: 'They HAD-bin WAIT-ing'. In rapid speech: sounds like one continuous phrase.",
        [("Past Perfect Continuous Tense", "Papa English", null), ("How Long Had They Been...", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L5() => C(
        "Future Continuous describes actions that will be happening at a specific moment in the future. Form: will + be + verb-ING. Use for: actions in progress at a future time, events that won't be completed by a future deadline, and predictions about future activities.",
        ["Form: will + be + verb-ING", "Shows: action happening at a specific future moment", "'At 3 PM tomorrow, I will be studying' — the action will be in progress at that time", "'This time next week, I will be relaxing on a beach'", "Common time markers: at + time, this time (next week/month), when + future event"],
        [("will be working", "estaré trabajando", "This time next week, I will be relaxing on the beach."), ("will be studying", "estaré estudiando", "When you call, I will be sleeping."), ("will be traveling", "estaré viajando", "Next month, we will be traveling in Europe."), ("will be living", "estaré viviendo", "In five years, I will be living in a new house.")],
        [("At 8 PM, she will be watching the news.", "Action at a specific future time"), ("When you arrive, I will be cooking dinner.", "Future action in progress"), ("This time tomorrow, we will be flying to Paris.", "Future continuous moment"), ("Next year, they will be working on a new project.", "Expected future activity")],
        "Write 10 Future Continuous sentences: 5 about what you will be doing at specific times, and 5 about what others will be doing in the future.",
        "Will be: stress on WILL and the participle. 'I will-BE-STUDying'. In fast speech: sounds like one unit. Don't pause between 'be' and the participle.",
        [("Future Continuous Tense", "Papa English", null), ("Will Be Doing - Future Continuous", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L6() => C(
        "Future Perfect Continuous shows how long an action will have been happening before a specific future moment. Form: will + have + been + verb-ING. Use for: expressing duration before a future event, life milestones, and achievements by future dates.",
        ["Form: will + have + been + verb-ING", "Shows: duration continuing up to a future time", "'By next year, I will have been working here for 10 years'", "'When you graduate, I will have been studying for 4 years'", "Emphasizes: completion AND duration"],
        [("will have been working", "habré estado trabajando", "By next year, I will have been working here for five years."), ("will have been studying", "habré estado estudiando", "When he retires, he will have been working for 40 years."), ("will have been living", "habré estado viviendo", "By 2030, we will have been living in this house for 20 years."), ("will have been learning", "habré estado aprendiendo", "By summer, I will have been learning English for 6 months.")],
        [("By the end of the year, she will have been teaching for 20 years.", "Career milestone"), ("When we meet again, I will have been living abroad for 3 years.", "Duration before future meeting"), ("By graduation, they will have been studying together for 4 years.", "Shared experience"), ("In six months, I will have been working on this project for a year.", "Project duration")],
        "Write 8 Future Perfect Continuous sentences about achievements, durations, and milestones you expect by certain future dates.",
        "Will have been: 4 stresses. But only stress WILL and the participle heavily. 'I will-HAVE-been STUDying.' In rapid speech it flows: will've-been.",
        [("Future Perfect Continuous", "Papa English", null), ("Will Have Been Doing", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L7() => C(
        "Modals express ability in different times: CAN (present), COULD (past), BE ABLE TO (all tenses). CAN and COULD are most common for ability. 'I can swim', 'She could speak five languages as a child', 'I will be able to drive next year'.",
        ["Present ability: CAN (I can swim, she can cook, they can speak English)", "Past ability: COULD (I could swim as a child, he could speak French)", "Future ability: WILL BE ABLE TO (I will be able to drive next month)", "Possibility/Permission: MAY (May I...?), MIGHT (It might rain)", "Common: 'Can't' (cannot), 'couldn't' (could not)"],
        [("can", "puedo", "I can play guitar."), ("could", "podía", "She could speak five languages as a child."), ("be able to", "ser capaz de", "I will be able to understand French by next year."), ("may/might", "puedo/podría", "It might rain tomorrow. / May I leave early?")],
        [("He can play tennis very well.", "Present ability"), ("When I was young, I could climb trees.", "Past ability"), ("She will be able to drive in two months.", "Future ability"), ("May I use the bathroom?", "Polite permission"), ("It might be cold tomorrow.", "Possibility")],
        "Write 10 sentences: 3 about present ability with 'can', 3 about past ability with 'could', 2 about future ability with 'will be able to', and 2 about possibility/permission with 'may/might'.",
        "'Can' has two pronunciations: strong /kæn/ and weak /kən/. 'Could' is /kʊd/. Practice weak forms: 'I-kən do it', 'He-kəd speak French'.",
        [("Modal Verbs: CAN & COULD", "Papa English", null), ("Can Could Ability English Grammar", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L8() => C(
        "SHOULD, MUST, HAVE TO express obligation and advice at different strengths. SHOULD = advice/recommendation. MUST/HAVE TO = obligation/necessity. 'You should study' (good idea), 'You must arrive on time' (obligation), 'I have to go now' (necessity).",
        ["SHOULD: advice, good ideas, opinions — 'You should see a doctor'", "MUST: strong obligation, prohibition — 'You must wear a seatbelt', 'You mustn't smoke'", "HAVE TO: necessity, obligation — 'I have to go to work', 'She has to study'", "NEED TO: necessity — 'You need to bring your passport'", "Strength: SHOULD (weakest) < HAVE TO < MUST (strongest)"],
        [("should", "debería", "You should exercise more."), ("must", "debes", "You must arrive on time."), ("have to", "tengo que", "I have to finish this work."), ("mustn't", "no debes", "You mustn't tell anyone."), ("need to", "necesito", "I need to buy groceries.")],
        [("You should eat more vegetables.", "Advice — SHOULD"), ("You must wear a helmet when biking.", "Strong obligation — MUST"), ("I have to work tomorrow.", "Necessity — HAVE TO"), ("You mustn't smoke in the hospital.", "Prohibition — MUSTN'T"), ("She needs to study hard for the exam.", "Necessity — NEED TO")],
        "Write 10 sentences: 3 with 'should' (advice), 3 with 'must' (obligation/prohibition), 2 with 'have to' (necessity), and 2 with 'need to' (necessity).",
        "MUST /mʌst/ vs SHOULD /ʃʊd/ — different vowel sounds. HAVE TO: stress on HAVE. NEED TO: stress on NEED. Practice the stress patterns.",
        [("Modals: SHOULD MUST HAVE TO", "Papa English", null), ("Obligation and Advice Modals", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L9() => C(
        "First Conditional describes real, possible situations in the future: IF + Present Simple, WILL + base verb. 'If it rains tomorrow, I will stay home.' Second Conditional describes unlikely/imaginary situations: IF + Past Simple, WOULD + base verb. 'If I were rich, I would travel.'",
        ["First Conditional: real/possible future situations", "IF + Present Simple + WILL + base verb: 'If you study, you will pass'", "Second Conditional: unlikely/imaginary situations", "IF + Past Simple + WOULD + base verb: 'If I had wings, I would fly'", "Compare: First = possible; Second = unlikely or imaginary"],
        [("If ... will", "Si ... va a", "If it rains, I will take an umbrella."), ("If ... would", "Si ... haría", "If I were taller, I would play basketball."), ("Unless", "A menos que", "Unless you hurry, you will miss the bus."), ("would", "haría/iría", "I would live in Italy if I could.")],
        [("If it rains tomorrow, I will stay home.", "First Conditional"), ("If I had more money, I would travel the world.", "Second Conditional"), ("She would be happier if she lived near the sea.", "Hypothetical preference"), ("If you study hard, you'll pass the exam.", "Real future possibility")],
        "Write 10 conditional sentences: 5 first conditionals about possible futures, and 5 second conditionals about imaginary or unlikely situations.",
        "First: WILL contracts: I'll, you'll, she'll. Second: WOULD contracts: I'd, you'd, she'd. Both stress the IF clause: 'IF it rains, I'll stay' — 'if' is emphasized.",
        [("First vs Second Conditional", "Papa English", null), ("If Clauses English Grammar", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L10() => C(
        "Third Conditional describes impossible past situations: IF + Past Perfect, WOULD HAVE + past participle. 'If I had known, I would have helped.' WISH expresses regrets about unreal or impossible situations. 'I wish I had studied harder.' Both show hypothetical or regretful thoughts.",
        ["Third Conditional: impossible past situations (can't change what happened)", "IF + Past Perfect + WOULD HAVE + pp: 'If I had studied, I would have passed'", "WISH + Past Perfect: 'I wish I had gone to the party' (but I didn't)", "WISH + Past Simple: 'I wish I knew the answer' (but I don't)", "These express regret, hope, or wistful thinking about unreal situations"],
        [("If I had known", "Si hubiera sabido", "If I had known about the party, I would have gone."), ("would have", "habría", "If he had studied harder, he would have passed."), ("I wish", "Ojalá/Desearía", "I wish I had listened to you."), ("I wish I could", "Ojalá pudiera", "I wish I could speak French fluently.")],
        [("If we had left earlier, we wouldn't have missed the train.", "Third Conditional"), ("I wish I had studied more for the exam.", "Regret about past"), ("If I had known her, we could have become friends.", "Impossible past"), ("I wish I were taller.", "Present wish about unchangeable fact")],
        "Write 10 sentences: 5 third conditionals expressing regrets about the past, and 5 wishes about impossible/unreal situations (past or present).",
        "Third Conditional: 'If I-ad-KNOWN, I-would-have-HELPED'. Stress the main verbs: 'known' and 'helped'. WISH: /wɪʃ/ — practice natural stress and flow.",
        [("Third Conditional Sentences", "Papa English", null), ("Wish Sentences - Present & Past", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L11() => C(
        "Passive Voice transforms active sentences: 'Someone stole my phone' → 'My phone was stolen.' Use passive when the agent is unknown, unimportant, or obvious. Present Passive: am/is/are + pp. Past Passive: was/were + pp. Essential for formal writing and news reporting.",
        ["Active: Subject + verb + object. Passive: Object + be + pp (+ by agent)", "Present passive: am/is/are + pp — 'The car is made in Germany'", "Past passive: was/were + pp — 'The letter was written yesterday'", "Agent with 'by': 'The novel was written by Austen' (important person)", "Omit agent: 'The window was broken' (agent unknown)"],
        [("is made", "está hecho", "This car is made in Germany."), ("was written", "fue escrito", "The report was written yesterday."), ("are sold", "se venden", "These products are sold online."), ("is spoken", "se habla", "English is spoken worldwide.")],
        [("English is spoken all over the world.", "Present passive"), ("The letter was written by the CEO.", "Past passive with agent"), ("My phone was stolen on the subway.", "Past passive, agent unknown"), ("These shoes are made in Italy.", "Present passive - origin")],
        "Write 10 passive sentences: 5 present passive and 5 past passive. Use topics: products, famous buildings, historical events, things that happened to you, or things you observe.",
        "Passive stress: 'The letter WAS-WRITten'. Agent with 'by' is less stressed: 'by auSTEN'. The participle carries the main stress.",
        [("Passive Voice Explained", "Papa English", null), ("Passive Voice in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L12() => C(
        "Reported Speech allows you to communicate what someone said indirectly. Rules: verb tenses shift (present → past), pronouns shift (I → he/she), and time words change (now → then). Direct: 'I am happy,' she said. Reported: She said she was happy. Essential for natural conversation about what others said.",
        ["Tense shift: Present → Past, Past → Past Perfect, will → would", "Pronouns: I → he/she, you → me/him/her, my → his/her, this → that", "Time shifts: now → then, today → that day, tomorrow → the next day, here → there", "Yes/No questions: ask + if/whether — 'He asked if I would help'", "WH questions: ask + wh-word — 'She asked where he lived'"],
        [("said that", "dijo que", "She said that she was tired."), ("asked if", "preguntó si", "He asked if we wanted to go."), ("told me", "me dijo", "She told me she would be late."), ("asked what", "preguntó qué", "He asked what I was doing.")],
        [("Direct: 'I like pizza.' → Reported: He said he liked pizza.", "Present → Past"), ("Direct: 'Where is the station?' → Reported: She asked where the station was.", "WH question"), ("Direct: 'Will you help?' → Reported: He asked if I would help.", "Yes/No question"), ("Direct: 'I have finished.' → Reported: She said she had finished.", "Present Perfect → Past Perfect")],
        "Convert 8 sentences from direct to reported speech: 3 statements, 3 yes/no questions, and 2 wh-questions. Focus on tense shifts, pronouns, and time expressions.",
        "'Said' is /sed/ (rhymes with 'bed'). 'Asked' is /æskt/. In reported speech, these verbs are often unstressed: 'She SAID she was going'.",
        [("Reported Speech - Indirect Speech", "Papa English", null), ("Direct and Indirect Speech", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L1() => C(
        "The passive voice shifts the focus from the person doing the action to the action itself. It is used when the agent is unknown, unimportant, or obvious. British Council and academic writing frequently use passive voice.",
        ["Present passive: am/is/are + past participle", "Past passive: was/were + past participle", "Agent introduced with 'by': 'The novel was written by Hemingway'", "Omit 'by' when agent is unknown or unimportant: 'The window was broken'", "Active: 'Someone stole my bag.' → Passive: 'My bag was stolen.'"],
        [("is made", "es hecho/fabricado", "This car is made in Germany."), ("was written", "fue escrito", "The report was written yesterday."), ("are sold", "se venden", "These products are sold online."), ("is spoken", "se habla", "English is spoken worldwide.")],
        [("English is spoken all over the world.", "Present passive — general fact"), ("The letter was written by the CEO.", "Past passive — agent specified"), ("My phone was stolen on the subway.", "Past passive — unknown agent"), ("These shoes are made in Italy.", "Present passive — origin")],
        "Write 6 passive sentences: 3 in present passive and 3 in past passive. Write about products you use, famous buildings, historical events, or things that happened to you.",
        "In passive sentences, the past participle carries main stress: 'The letter WAS WRITten'. Practice the rhythm: was/were are unstressed, the participle is stressed.",
        [("Passive Voice Explained", "Papa English", null), ("Passive Voice in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L2() => C(
        "Modals combine with passive voice to express obligation, recommendation, possibility, or permission without specifying who performs the action. This is essential for formal communication and professional writing.",
        ["Structure: modal + BE + past participle (same for ALL subjects)", "Must be done, should be submitted, can be found, might be cancelled", "Compare active: 'You must submit it' → Passive: 'It must be submitted'", "Perfect modal passive: should have been done, could have been avoided", "Very common in formal writing, rules, instructions, and announcements"],
        [("must be + pp", "debe ser", "The form must be signed."), ("should be + pp", "debería ser", "Errors should be corrected."), ("can be + pp", "puede ser", "The meeting can be rescheduled."), ("might be + pp", "podría ser", "The event might be cancelled.")],
        [("All homework must be submitted by Friday.", "Obligation — must be"), ("Mistakes should be corrected before submission.", "Recommendation — should be"), ("The report can be found on the website.", "Possibility — can be"), ("The flight might be delayed due to weather.", "Uncertainty — might be")],
        "Write 5 passive sentences with modals. Write rules or guidelines for a real situation: workplace policy, school rules, or environmental regulations.",
        "'Should be' sounds like 'SHUD-bee'. 'Must be' sounds like 'MUST-bee'. The passive 'be' is unstressed — focus on the modal and participle.",
        [("Passive Voice with Modals", "Papa English", null), ("Modal Verbs with Passive Voice", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L3() => C(
        "Reported speech is used to report what someone said without quoting their exact words. Verb tenses 'shift back' (backshift), and pronouns and time expressions change. Essential for narrative writing and formal communication.",
        ["Present Simple → Past Simple: 'I work' → he said he worked", "Present Continuous → Past Continuous: 'I'm working' → she said she was working", "Will → Would: 'I will call' → she said she would call", "Can → Could: 'I can help' → he said he could help", "Pronouns change: I → he/she, we → they, my → his/her"],
        [("said (that)", "dijo (que)", "She said that she was tired."), ("told me (that)", "me dijo (que)", "He told me that he knew the answer."), ("explained that", "explicó que", "She explained that she was busy."), ("mentioned that", "mencionó que", "He mentioned that he was leaving soon.")],
        [("Direct: 'I love English.' → She said she loved English.", "Present → Past"), ("Direct: 'We are leaving.' → They said they were leaving.", "Continuous → Past Continuous"), ("Direct: 'I will call you.' → He said he would call me.", "Will → Would"), ("Direct: 'I can swim.' → She said she could swim.", "Can → Could")],
        "Think of a recent conversation. Report 5 things that were said using 'said that', 'told me that', 'mentioned that', or 'explained that'.",
        "'Said' is pronounced SED (rhymes with 'bed') — not 'sayd'. 'Told' is TOHLD. These are the most common reporting verbs — correct pronunciation matters.",
        [("Reported Speech", "Papa English", null), ("Reported Speech in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L4() => C(
        "Reported questions do NOT use question word order (no inversion). They use statement order (subject + verb). Yes/No questions use 'if' or 'whether'; Wh- questions keep their question word. Critical for advanced communication.",
        ["Wh- questions: keep question word, use statement order", "'Where do you live?' → He asked me where I lived.", "Yes/No questions: use 'if' or 'whether'", "'Are you tired?' → She asked if/whether I was tired.", "No question marks! No inverted word order! Tense backshift still applies"],
        [("asked where/when/why/how", "preguntó dónde/cuándo/por qué/cómo", "She asked where I worked."), ("asked if / whether", "preguntó si", "He asked if I was ready."), ("wanted to know", "quería saber", "She wanted to know what time it was.")],
        [("'Where do you work?' → He asked me where I worked.", "Wh- question reported"), ("'Are you coming?' → She asked if I was coming.", "Yes/No → 'if'"), ("'Do you speak Spanish?' → She wanted to know whether I spoke Spanish.", "Using 'whether'")],
        "Write 5 reported questions. Imagine someone asked you these things at a job interview or on a first date. Use different reporting phrases.",
        "'Whether' is pronounced WETH-er — exactly like 'weather'. They are homophones. In writing: 'whether' for reported questions, 'weather' for climate.",
        [("Reported Questions", "Papa English", null), ("Reported Questions in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L5() => C(
        "'Wish' and 'if only' express regret about situations in the present or past, or aspirations for the future. These structures use past tenses for present/past wishes (not reflecting actual past) and 'would' for future. Essential for expressing unreal situations.",
        ["Present wish (unreal now): wish + Past Simple. 'I wish I lived in Paris' (but I don't)", "Past wish (unreal yesterday): wish + Past Perfect. 'I wish I had studied harder' (but I didn't)", "Future wish: wish + would. 'I wish you would help me' or 'I wish I would win the lottery'", "'If only' = 'wish' but more emphatic: 'If only I had known!'", "Common mistake: 'I wish I would learn' — use Present Simple 'I wish I learned' for present wishes"],
        [("I wish I could", "Ojalá pudiera", "I wish I could speak fluent Spanish."), ("I wish I had", "Ojalá hubiera", "I wish I had taken that job opportunity."), ("If only I knew", "Si al menos supiera", "If only I knew the answer!"), ("I wish you would", "Desearía que", "I wish you would be more honest with me.")],
        [("I wish I were taller.", "Present unreal situation"), ("If only I had listened to my parents.", "Regret about past"), ("I wish I could go back in time.", "Impossible aspiration"), ("She wishes he would apologize.", "Wanting someone else to do something")],
        "Write 7 sentences using 'wish' and 'if only': 2 expressing present regrets, 2 about past regrets, 2 about future wishes, and 1 about someone else's action.",
        "'I wish I were' (subjunctive) is formal; 'I wish I was' is common in spoken English. Both are acceptable, but 'were' sounds more educated. In reporting: 'I wish I had known' — stress 'HAD' and 'KNOWN'.",
        [("Wish and If Only English Grammar", "Papa English", null), ("Expressing Regrets with Wish", "Learn English with EnglishClass101.com", null), ("If Only and Wish Structures", "Cambridge English", null)]
    );

    private static string B2L6() => C(
        "Cleft sentences reorganize information for emphasis by splitting a sentence into two parts. They focus on WHO did something ('It was X who...') or WHAT happened ('It was X that...'). Common in formal writing, journalism, and persuasive speech.",
        ["It + be + X + that/who + clause: 'It was Sarah who won the prize'", "'What' cleft: 'What I love about you is your honesty'", "The cleft structure emphasizes the part between 'be' and the relative pronoun", "Pseudo-cleft: 'What you said was untrue' = 'Your statement was untrue'", "Contrastive cleft for heavy emphasis: 'It wasn't the money that mattered; it was the respect.'"],
        [("It was... who", "Fue... quien", "It was the teacher who explained the concept clearly."), ("It was... that", "Fue... que", "It was last year that I moved here."), ("What... is/was", "Lo que... es/era", "What impressed me was her dedication."), ("It's not... but...", "No es... sino...", "It's not the destination that matters; it's the journey.")],
        [("It was the internet that changed everything.", "Emphasis on cause"), ("What really matters is your health.", "Pseudo-cleft to emphasize value"), ("It was at the conference where I met him.", "Emphasis on location"), ("It is discipline, not talent, that leads to success.", "Contrastive emphasis")],
        "Rewrite 6 sentences using cleft structures to emphasize different parts: 2 'it was... who' sentences, 2 'what... is' sentences, and 2 contrasting clefts.",
        "The emphasis comes AFTER 'be': 'It WAS Sarah who won' or 'It was SARAH who won' — both are correct depending on what you want to emphasize. Stress shifts the focus.",
        [("Cleft Sentences in English", "Papa English", null), ("It Cleft Sentences", "Learn English with EnglishClass101.com", null), ("Emphasis with Cleft Structures", "Cambridge English", null)]
    );

    private static string B2L7() => C(
        "Phrasal verbs (verb + particle combinations) are central to natural English. Advanced phrasal verbs convey complex meanings concisely. Understanding the particle (up, down, in, out, on, off, etc.) helps predict meaning. Essential for fluent, native-like English.",
        ["Separable: 'pick up' — 'pick the keys up' or 'pick up the keys'", "Inseparable: 'look into' — NEVER 'look the matter into'", "Transitive (need object): 'carry out' vs Intransitive (no object): 'break down'", "Common advanced: put up with, come across, run into, see through, bring about, cut down on", "Context matters: 'put up' = construct/tolerate/increase price — meaning depends on context"],
        [("put up with", "aguantar", "I can't put up with this behavior anymore."), ("come across", "encontrarse con", "I came across an interesting article yesterday."), ("run into", "encontrarse con", "I ran into an old friend at the supermarket."), ("see through", "ver la verdad", "She finally saw through his lies."), ("bring about", "causar/provocar", "The new policy brought about significant changes.")],
        [("I had to carry out the project despite obstacles.", "Perform/execute"), ("The situation calls for cutting down on expenses.", "Reduce consumption"), ("He looked into the matter thoroughly.", "Investigate"), ("The scandal came out last week.", "Be revealed"), ("We need to face up to our mistakes.", "Acknowledge and accept")],
        "Write 8 sentences using different advanced phrasal verbs. Create a narrative about overcoming challenges or a business scenario.",
        "Stress in phrasal verbs: the PARTICLE typically gets the stress: 'PUT up WITH', 'COME a-CROSS', 'RUN IN-to'. This distinguishes them from regular verbs with prepositions.",
        [("Advanced Phrasal Verbs", "Papa English", null), ("Phrasal Verbs in English", "Learn English with EnglishClass101.com", null), ("Phrasal Verbs for Upper Intermediate", "English Speeches", null)]
    );

    private static string B2L8() => C(
        "Quantifiers and articles at B2 level require nuanced understanding. Use 'some' vs 'any', 'much' vs 'many' vs 'a lot of', and articles with generics. Mastering these structures prevents common intermediate errors and enables precise expression.",
        ["Countable: many, a few, several, a large number of | Uncountable: much, a little, a great deal of", "'Some' in positive statements, 'any' in negatives/questions — but 'some' in offers: 'Would you like some tea?'", "'The' with specific things, no article with generics: 'Dogs are loyal' vs 'The dogs in the park were friendly'", "'A/an' with professions/singular: 'She is a teacher' | Zero article with plurals: 'They are teachers'", "Context shapes meaning: 'all the time' (specific) vs 'all time' (general, rare)"],
        [("a few / a little", "unos pocos / un poco", "A few people attended the event. I have a little experience."), ("several / a number of", "varios / varios de", "Several options are available."), ("each / every", "cada / cada", "Each student received a certificate. Every day is important."), ("the / no article", "el/la/los/las / -", "The president announced new policies. Presidents face tough decisions.")],
        [("There are many opportunities in this field.", "Many with countable plural"), ("I have much respect for her work.", "Much with uncountable"), ("Some students prefer morning classes.", "Some in positive statement"), ("I don't want any sugar in my coffee.", "Any in negative"), ("The information provided was accurate.", "The with specific information")],
        "Write 8 sentences using quantifiers and articles correctly. Write about a topic of interest: travel, education, hobbies, or workplace.",
        "Quantifiers are often weak/unstressed: 'a few STUDENTS', 'much RESPect'. The noun after gets the stress. Articles are completely unstressed: 'ə BOOK', 'ðə BOOK'.",
        [("Quantifiers and Determiners", "Learn English with Papa English", null), ("Articles A An The in English", "Cambridge English", null), ("Countable and Uncountable Nouns", "English Speeches", null)]
    );

    private static string B2L9() => C(
        "Relative clauses add information about a noun, making sentences more sophisticated. Defining clauses (no commas) provide essential info; non-defining clauses (with commas) provide extra info. Mastering both creates complex, fluent English.",
        ["Defining (restrictive): no commas, essential info — 'The person who helped me was kind'", "Non-defining (non-restrictive): commas, extra info — 'John, who is my friend, helped me'", "Relative pronouns: WHO (people), THAT/WHICH (things), WHOSE (possession), WHERE (place), WHEN (time)", "In defining clauses, 'that' is often preferred to 'which': 'The book that I bought...'", "Prepositions at the end (stranded): 'The person I spoke to' NOT 'to whom I spoke'"],
        [("who", "quien", "The teacher who explained this is excellent."), ("which", "que / cual", "The report which arrived today is important."), ("whose", "cuyo/a", "The student whose project won was talented."), ("where", "donde", "The café where we met is closed."), ("when", "cuando", "The year when I graduated was 2020.")],
        [("I know someone who speaks five languages.", "Defining with who"), ("Sarah, who is my colleague, gave the presentation.", "Non-defining with who"), ("The book that I read was fascinating.", "Defining with that"), ("The restaurant, which is very popular, is fully booked.", "Non-defining with which"), ("The reason why they left is unclear.", "Relative adverb: why")],
        "Write 10 sentences: 5 with defining relative clauses and 5 with non-defining relative clauses. Mix different relative pronouns.",
        "In defining clauses, 'that' is stressed if it's important: 'THE PERSON THAT helped me...' In non-defining clauses with commas, the relative pronoun is lighter.",
        [("Defining vs Non-defining Clauses", "Papa English", null), ("Relative Clauses in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L10() => C(
        "Adjectives describe nouns; adverbs describe verbs, adjectives, or other adverbs. Many adverbs are formed by adding -ly to adjectives, but some are irregular. Correct placement and formation are essential for fluent English.",
        ["Most adverbs: adjective + -ly (quick → quickly, happy → happily)", "Irregular: good → well, fast → fast, hard → hard, late → late", "Adverbs of manner: slowly, carefully, beautifully, 'She spoke gently'", "Adverbs of frequency: always, usually, often, sometimes, rarely, never", "Placement: end of clause (usually), before verb (frequency adverbs), or before adjective (adverbs of degree)"],
        [("quickly", "rápidamente", "She walked quickly to the station."), ("well", "bien", "He plays the piano well."), ("carefully", "cuidadosamente", "Handle the package carefully."), ("frequently", "frecuentemente", "They visit frequently."), ("extremely", "extremadamente", "The weather was extremely cold.")],
        [("She drives carefully.", "Adverb of manner at end"), ("I usually arrive early.", "Frequency adverb before verb"), ("This cake is extremely delicious.", "Adverb of degree before adjective"), ("He works hard.", "Adjective/adverb homonyms")],
        "Write 10 sentences using different types of adverbs: 3 of manner, 3 of frequency, 2 of degree, and 2 irregular adverbs.",
        "Adverb stress: -ly adverbs are usually unstressed: 'She walked QUICK-ly' — the root syllable gets main stress. Some adverbs have irregular stress: 'FREquent-ly' (stress 1st).",
        [("Adverbs of Manner", "Papa English", null), ("Adverb Placement in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L11() => C(
        "Conjunctions connect clauses and sentences. Coordinating conjunctions (and, but, or) join equal clauses. Subordinating conjunctions (because, if, when) join a dependent clause to an independent clause. Mastering them creates smooth, logical writing.",
        ["Coordinating: and, but, or, nor, so, yet (FANBOYS)", "Subordinating: because, if, when, while, although, unless, before, after, so that, in case", "Dependent clause needs main clause: 'WHEN YOU ARRIVE, call me.' ← dependent + main", "Use commas wisely: after introductory dependent clause, before 'but' (in complex ideas), around non-defining information", "Conjunctive adverbs (however, therefore, moreover) connect ideas between sentences"],
        [("although", "aunque", "Although it rained, we continued the journey."), ("unless", "a menos que", "Unless you study, you won't pass."), ("so that", "para que / de modo que", "Speak clearly so that everyone understands."), ("in case", "en caso de que", "I'll bring a jacket in case it gets cold."), ("moreover", "además / por otra parte", "The project is delayed. Moreover, costs have increased.")],
        [("She is smart, but she doesn't work hard.", "Coordinating: contrast"), ("Because it was raining, we stayed inside.", "Subordinating: reason"), ("I will wait here unless you tell me to leave.", "Subordinating: condition"), ("He studied hard; therefore, he passed.", "Conjunctive adverb: result")],
        "Write 8 sentences: 3 with coordinating conjunctions, 3 with subordinating conjunctions, and 2 with conjunctive adverbs.",
        "Conjunctions often carry little stress — they're 'glue' words. 'I'll go AND you'll stay' — 'and' is light. But 'although' in an introductory clause gets slight emphasis.",
        [("Conjunctions in English Grammar", "Papa English", null), ("Subordinating and Coordinating Conjunctions", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L12() => C(
        "Prepositions express relationships of place, time, direction, and manner. Some prepositions combine with specific verbs (phrasal verbs), while others have multiple meanings depending on context. Understanding prepositions is essential for natural English.",
        ["Place: in (the box), on (the table), at (home), under (the bridge), between (two people)", "Time: in (2025), on (Monday), at (3 PM), during (the meeting), before (Friday)", "Direction: to, from, towards, into, out of, across, through, along", "Complex: apart from, instead of, because of, with regard to, in addition to", "Some prepositions have multiple meanings: 'at the station' (place), 'at noon' (time), 'at my best' (condition)"],
        [("in the morning", "por la mañana", "I prefer to exercise in the morning."), ("on time", "a tiempo", "The train arrived on time."), ("at the corner", "en la esquina", "Meet me at the corner."), ("by next week", "para la próxima semana", "The report will be finished by next week."), ("throughout", "durante todo", "The project continued throughout the year.")],
        [("She was sitting at the cafe.", "Place: at"), ("We'll meet on Friday at 3 PM.", "Time: on + at"), ("The cat jumped onto the table.", "Direction: onto"), ("Instead of complaining, take action.", "Alternative: instead of"), ("Apart from the cost, it's a great idea.", "Exclusion: apart from")],
        "Write 8 sentences using different prepositions of place, time, and direction. Then write 2 sentences using complex prepositions.",
        "Prepositions are usually unstressed: 'I'm IN the house' — 'in' is light. But in poetic or emphatic speech, you can stress them: 'It's IN the house, not ON it.'",
        [("Prepositions of Time, Place, Movement", "Papa English", null), ("English Prepositions", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L13() => C(
        "Advanced prepositions and multi-word prepositions (in spite of, on account of, by means of) add sophistication to writing. These structures are common in academic and formal English. Mastering them marks B2/C1 level proficiency.",
        ["With nouns/pronouns: despite/in spite of (+ noun), according to (+ person), owing to (+ reason)", "With gerunds (-ing): in addition to, in order to, as well as, instead of, without", "Time: during, throughout, pending, prior to, subsequent to, following", "Expressions: in view of, in light of, given that, seeing that, considering that", "Formal writing: as to, in regard to, on the subject of, with respect to, in the matter of"],
        [("in spite of", "a pesar de", "In spite of the rain, the event continued."), ("according to", "según", "According to the report, sales increased."), ("in view of", "en vista de / considerando", "In view of the circumstances, the decision was justified."), ("in order to", "para / a fin de", "I studied hard in order to pass."), ("owing to", "debido a / por", "Owing to the delay, we arrived late.")],
        [("Despite his youth, he was very mature.", "Despite + noun"), ("In addition to English, she speaks French.", "Multi-word preposition with noun"), ("According to recent statistics, unemployment is falling.", "According to + authority"), ("In view of the evidence, we must reconsider.", "Formal preposition in writing"), ("She succeeded without any help.", "Preposition with -ing form")],
        "Write 10 sentences using advanced prepositions, particularly multi-word prepositions. Use contexts: academic writing, formal communication, news reports.",
        "Multi-word prepositions are typically unstressed: 'in SPITE of the rain' — the stress falls on the noun, not the preposition phrase.",
        [("Advanced Prepositions in English", "Papa English", null), ("Multi-word Prepositions", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L14() => C(
        "Phrasal verbs combine a verb with an adverb or preposition particle, creating meanings often impossible to guess from the individual words. Advanced phrasal verbs are essential for natural, native-like English across all registers.",
        ["Separable: look up (information), pick up, hand in, put off, carry out", "Inseparable: look after, come across, run into, look into, put up with", "Transitive: 'I carried out the project' (needs object). Intransitive: 'The car broke down' (no object)", "Particles change meaning: 'put up' (construct/tolerate), 'put on' (wear/perform), 'put off' (postpone/switch off)", "Three-word phrasal verbs: look forward to, run out of, cut down on, look up to, deal with"],
        [("come across", "encontrarse con", "I came across an old friend at the supermarket."), ("put up with", "aguantar / tolerar", "I can't put up with this behavior."), ("look forward to", "esperar con entusiasmo", "We look forward to hearing from you."), ("run out of", "quedarse sin", "We've run out of coffee."), ("carry out", "realizar / llevar a cabo", "The team carried out the project successfully.")],
        [("I had to call off the meeting.", "Separate: call + off"), ("She's looking after her sister.", "Inseparable: look after"), ("I'm looking forward to the vacation.", "Three-word phrasal verb"), ("We came across some interesting research.", "Inseparable: come across"), ("He ran into his ex-girlfriend at the store.", "Inseparable: run into")],
        "Write 10 sentences using a variety of phrasal verbs: 3 separable, 3 inseparable, 2 transitive, and 2 three-word phrasal verbs. Create a coherent narrative about work or study.",
        "Stress in phrasal verbs: the PARTICLE gets the stress, not the verb: 'CARRY out' (not 'carry OUT'). This distinguishes them from regular verb + preposition.",
        [("Phrasal Verbs List B2", "Papa English", null), ("Advanced Phrasal Verbs", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L15() => C(
        "Collocations are word combinations that sound natural to native speakers. Understanding common collocations prevents errors like 'do a mistake' (correct: 'make a mistake'). Mastering collocations marks the difference between non-native and native-like English.",
        ["Verb collocations: make a decision, take a risk, do homework, have a conversation, reach a conclusion", "Adjective collocations: deeply committed, highly unlikely, firmly believe, utterly exhausted, strongly opposed", "Noun collocations: a dull ache, a bitter end, a blind rage, a cold shoulder, a wild goose chase", "Adverb + adjective: widely known, commonly accepted, deeply sorry, fully qualified, partly responsible", "Collocation errors: NOT 'do a decision' (make), NOT 'take homework' (do), NOT 'have a risk' (take)"],
        [("make a decision", "tomar una decisión", "She made a difficult decision to leave her job."), ("take a risk", "arriesgarse / asumir un riesgo", "Don't take unnecessary risks."), ("strongly oppose", "oponerserotundamente", "I strongly oppose this proposal."), ("widely known", "ampliamente conocido", "The author is widely known for his novels."), ("bitter end", "fin amargo / hasta el final", "They fought to the bitter end.")],
        [("We reached a compromise after long discussions.", "Collocation: reach + compromise"), ("She is deeply committed to her work.", "Collocation: deeply + committed"), ("Making a hasty decision often leads to regret.", "Collocation: hasty + decision"), ("The policy faced strong opposition from the public.", "Collocation: strong + opposition"), ("I have a burning desire to travel the world.", "Collocation: burning + desire")],
        "Write a paragraph (8-10 sentences) using at least 8 different collocations. Choose a topic: a career goal, a personal achievement, or a difficult life decision.",
        "Collocations flow naturally as units: 'make-a-decision' sounds like one phrase. 'Do a decision' sounds wrong because it breaks the natural rhythm native speakers expect.",
        [("Common English Collocations", "Papa English", null), ("Verb + Noun Collocations", "Learn English with EnglishClass101.com", null)]
    );

    private static string B2L16() => C(
        "Register and style involve choosing appropriate vocabulary, grammar, and tone for your audience and context. Formal English differs from casual English in verb choice, vocabulary, sentence complexity, and contraction use. Mastering register is key to sophisticated communication.",
        ["Formal: 'I would appreciate your assistance' vs Informal: 'Can you help me?'", "Avoid contractions in formal writing: cannot (not can't), will not (not won't), I have (not I've)", "Formal vocabulary: commence (start), obtain (get), demonstrate (show), require (need), endeavor (try)", "Formal sentences: complex with subordination. Informal: shorter, simpler, more direct.", "Tone: formal (impersonal, objective), neutral (balanced), informal (personal, friendly, casual)"],
        [("commence", "comenzar / iniciar", "The meeting will commence at 9 AM."), ("obtain", "obtener / conseguir", "You can obtain further information from the website."), ("endeavor", "esforzarse / intentar", "We endeavor to provide excellent service."), ("hereby", "por este medio / por la presente", "I hereby declare this session open."), ("furthermore", "además / por otra parte", "Furthermore, the cost is prohibitive.")],
        [("Formal: 'I would like to express my gratitude.' / Informal: 'Thanks so much.'", "Register contrast"), ("This matter requires our urgent attention.", "Formal register in business"), ("The project aims to facilitate community development.", "Formal + nominalization"), ("Can you send me the files? — Yeah, sure.", "Informal register")],
        "Rewrite these informal sentences in formal register: 1) Hey, I want to ask about the job. 2) We can't do it because we don't have enough money. 3) The results show that our plan is good. 4) You gotta be careful with this stuff. 5) Basically, everyone thinks this is not a good idea.",
        "In formal speech, your pace should be measured and deliberate. Avoid 'um', 'uh', 'like', 'basically', 'you know'. Maintain full, clear vowel pronunciation.",
        [("Formal vs Informal English", "Papa English", null), ("Register in English Communication", "Cambridge English", null)]
    );

    private static string C1L1() => C(
        "Inversion places the auxiliary verb BEFORE the subject after certain negative or limiting adverbials. It creates strong emphasis and is a key feature of formal, literary, and sophisticated English. Perfect English Grammar emphasizes this advanced technique.",
        ["Trigger words: Never, Rarely, Seldom, Hardly, Barely, Scarcely, No sooner, Not only, Not until, Only when, Little", "Structure: Negative adverb + AUXILIARY + subject + main verb", "'Never I have seen' is WRONG → 'Never have I seen' is CORRECT", "'Not only did she win, but she also broke the record.'", "Common in formal speeches, literary texts, journalism, and advanced writing"],
        [("Never have I...", "Nunca he...", "Never have I witnessed such courage."), ("Rarely does...", "Raramente...", "Rarely does he make a mistake."), ("Hardly had... when...", "Apenas había... cuando...", "Hardly had she arrived when it started raining."), ("Not only... but also...", "No solo... sino también...", "Not only did he lie, but he also stole.")],
        [("Never have I seen such dedication.", "Never + inversion"), ("Not only did she pass, but she got the highest score.", "Not only + inversion"), ("Hardly had I sat down when the phone rang.", "Hardly + had + inversion"), ("Rarely does the committee agree on anything.", "Rarely + does + inversion")],
        "Rewrite these 5 sentences using inversion for emphasis: 1) I have never worked so hard. 2) She rarely makes mistakes. 3) I had barely fallen asleep when the alarm went off. 4) He not only failed the test but also lost his scholarship. 5) You will only understand when you experience it yourself.",
        "Inversion creates a dramatic, authoritative tone in speech. Drop your pitch at the end rather than rising. 'Never HAVE I seen...' — stress the auxiliary.",
        [("Inversion for Emphasis", "Papa English", null), ("Advanced Inversion in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L2() => C(
        "Mixed conditionals combine different time frames. The most common type uses a past condition (3rd conditional) with a present result (2nd conditional), showing how a past event still affects the present. Essential for C1 proficiency.",
        ["Type A — Past condition, present result: IF + Past Perfect → WOULD + base verb", "'If I had studied medicine (past), I would be a doctor now (present).'", "Type B — Present condition, past result: IF + Past Simple → WOULD HAVE + past participle", "'If she were more organized (now), she would have finished on time (then).'", "These mark C1/C2 level proficiency — essential for advanced communication"],
        [("If I had + pp ... would + base", "Si hubiera... sería/estaría", "If I had moved there, I would speak the language now."), ("If I were ... would have + pp", "Si fuera... habría", "If I were braver, I would have spoken up.")],
        [("If I had taken that job, I would be living in New York now.", "Past condition → present result"), ("If she were more patient, she would have handled it better.", "Present trait → past result"), ("He would be a millionaire now if he hadn't sold those shares.", "Inverted order")],
        "Write 4 mixed conditional sentences about your own life. Think about: a past choice and how it affects your present; or a character trait you have and how it affected a past situation.",
        "'Would have' in fast speech sounds like 'would've' (WOOD-uv) or 'woulda'. 'Should have' → 'should've' (SHUD-uv). Essential for listening comprehension.",
        [("Mixed Conditionals", "Papa English", null), ("Mixed Conditional Sentences", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L3() => C(
        "Academic and formal language uses precise vocabulary to convey complex ideas with nuance. At C1 level, you replace informal words with formal equivalents and use nominalizations for a sophisticated style. British Council and academic institutions emphasize this skill.",
        ["Prefer formal: commence (start), obtain (get), demonstrate (show), require (need), endeavour (try)", "Nominalizations: 'analyze' → 'analysis', 'develop' → 'development', 'argue' → 'argument'", "Avoid contractions in formal writing: cannot (not can't), do not (not don't)", "Qualify claims with hedging: 'it appears that', 'evidence suggests', 'it is worth noting'", "Cohesion: furthermore, nevertheless, consequently, notwithstanding"],
        [("ubiquitous", "ubicuo / presente en todas partes", "Smartphones are now ubiquitous."), ("exacerbate", "agravar / empeorar", "The delay exacerbated the problem."), ("ameliorate", "mejorar / aliviar", "The policy aims to ameliorate inequality."), ("laconic", "lacónico / conciso", "His laconic reply revealed little."), ("verbose", "verboso / palabrero", "The report is verbose and needs editing.")],
        [("The study demonstrates a significant correlation between diet and health.", "Formal verb 'demonstrates'"), ("The situation was further exacerbated by the lack of resources.", "Formal adjective + nominalization"), ("The analysis reveals several key findings.", "Nominalization: analyze → analysis"), ("It is worth noting that these results may not be generalizable.", "Hedging + formal vocabulary")],
        "Rewrite these informal sentences in formal style: 1) The problem got worse because of the weather. 2) We need to look at this more carefully. 3) The results show that our idea was right. 4) They're trying to make things better for poor people. 5) Nobody knows why this happens.",
        "In formal spoken English (presentations, interviews), avoid 'gonna', 'wanna', 'kinda'. Say 'going to', 'want to', 'kind of'. Formal speech is clearer and more deliberate.",
        [("Formal Academic English", "Papa English", null), ("Academic Vocabulary", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L4() => C(
        "Collocations are word partnerships that native speakers use naturally. Idioms are fixed expressions with meanings different from their literal words. Mastering both makes your English genuinely fluent and natural-sounding.",
        ["Strong verb collocations: MAKE a decision, DO homework, TAKE a risk, GIVE a presentation, REACH a conclusion", "Adjective collocations: deeply committed, highly unlikely, firmly believe, utterly exhausted", "Idioms: 'a blessing in disguise' = something good that seemed bad at first", "Idioms: 'burn bridges' = destroy relationships permanently, 'hit a deadlock' = reach a total impasse", "Collocations cannot be changed: 'make a decision' — NOT 'do a decision'"],
        [("photographic memory", "memoria fotográfica", "She has a photographic memory — recalls every detail."), ("beside yourself", "fuera de sí (con emoción)", "He was beside himself with excitement."), ("a blessing in disguise", "un mal que por bien viene", "Losing that job was a blessing in disguise."), ("hit a deadlock", "llegar a un punto muerto", "Negotiations hit a deadlock."), ("burn bridges", "quemar puentes", "Don't burn bridges — you may need them later.")],
        [("She gave an outstanding presentation that left everyone speechless.", "Collocation: give a presentation"), ("He was beside himself with joy when he passed the exam.", "Idiom: beside yourself"), ("Losing the contract turned out to be a blessing in disguise.", "Idiom in context"), ("The committee reached a deadlock and adjourned the meeting.", "Collocation + formal vocabulary")],
        "Write a short paragraph (6–8 sentences) about a challenging situation. Use at least 3 collocations and 2 idioms from this lesson. The situation can be real or invented.",
        "Idioms are spoken as fixed chunks — 'blessing in disguise' flows as one unit: BLESS-ing-in-dis-GIZE. Do not pause between words. The rhythm is key.",
        [("English Collocations and Idioms", "Papa English", null), ("Common Idioms in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L5() => C(
        "Nominalization transforms verbs and adjectives into nouns, making language more formal and academic. 'He analyzed the data' becomes 'His analysis of the data...'. This technique is essential for sophisticated writing in English.",
        ["Verb → Noun: analyze/analysis, develop/development, argue/argument, review/review, judge/judgment", "Adjective → Noun: able/ability, possible/possibility, difficult/difficulty, significant/significance", "Benefits: saves words, adds formality, creates logical flow", "Overuse nominalization creates dense, hard-to-read prose — balance is key", "Common pattern: possessive + nominalization: 'His analysis showed...' (more sophisticated than 'He analyzed')"],
        [("analysis", "análisis", "The analysis revealed unexpected findings."), ("development", "desarrollo / evolución", "The development of the technology took years."), ("judgment", "juicio / criterio", "In my judgment, the proposal is sound."), ("implementation", "implementación / ejecución", "Implementation of the plan begins next week."), ("achievement", "logro / realización", "The achievement of our goals requires cooperation.")],
        [("Rather than: 'We discovered that the system was inefficient.' / Better: 'Our discovery revealed systemic inefficiency.'", "Nominalization for sophistication"), ("The development of new strategies led to improved outcomes.", "Development as nominalization"), ("His analysis of the problem demonstrated considerable insight.", "Possessive + nominalization"), ("The implementation of the policy faced significant resistance.", "Nominalization in formal context")],
        "Rewrite these sentences using nominalization: 1) The researchers examined the data carefully. 2) We developed three solutions. 3) The company transformed its approach. 4) The government approved the bill. 5) She contributed significantly to the project.",
        "Nominalizations shift stress: 'development' is DEV-elop-ment (stress 1st syllable). 'Discovery' is dis-KUV-er-ee. The stress often differs from the verb form.",
        [("Nominalization in Academic Writing", "Papa English", null), ("Transform Verbs to Nouns", "Learn English with EnglishClass101.com", null), ("Advanced Nominalizations", "Cambridge English", null)]
    );

    private static string C1L6() => C(
        "Discourse markers (furthermore, nevertheless, meanwhile, notwithstanding) signal logical relationships between ideas. They guide the reader through your argument. Cohesion devices create smooth, professional English. Essential for C1 writing proficiency.",
        ["Addition/Agreement: furthermore, moreover, additionally, in addition to, as well as", "Contrast/Disagreement: nevertheless, notwithstanding, however, conversely, by contrast", "Cause/Effect: consequently, as a result, due to, as a consequence, resulting in", "Time/Order: meanwhile, subsequently, eventually, henceforth, in due course", "Emphasis/Conclusion: notably, in particular, in fact, in conclusion, to summarize"],
        [("nevertheless", "sin embargo / no obstante", "The evidence is inconclusive; nevertheless, findings suggest a pattern."), ("notwithstanding", "a pesar de / sin embargo", "Notwithstanding these challenges, progress continues."), ("resultantly", "como resultado / en consecuencia", "The delay exacerbated problems; resultantly, costs increased."), ("furthermore", "además / por otra parte", "The project is delayed. Furthermore, the budget is exhausted."), ("conversely", "por el contrario / inversamente", "Some argue for expansion; conversely, others seek consolidation.")],
        [("The data are inconclusive; nevertheless, the trend is evident.", "Discourse marker: nevertheless"), ("Due to the recession, sales declined. Consequently, staff were reduced.", "Cause-effect link"), ("The evidence supports this view. Moreover, recent findings confirm it.", "Addition marker"), ("In the first case, investment was high. Conversely, returns were minimal.", "Contrast between cases")],
        "Write a 10-12 sentence paragraph on a topic of your choice. Use at least 6 different discourse markers to link your ideas logically. Aim for sophisticated connections.",
        "Discourse markers are typically unstressed: 'nev-ER-the-less' — 'nonetheless' flows as a bridge between ideas. 'Consequently' is CON-se-quent-ly — the emphasis is on the thought-connection, not the word.",
        [("Discourse Markers and Connectors", "Papa English", null), ("Advanced Linking Words and Phrases", "Cambridge English", null), ("Cohesion in Academic Writing", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L7() => C(
        "Ellipsis (omitting words understood from context) and substitution (using pronouns or 'do') create natural, concise English. Native speakers use these constantly to avoid repetition. Essential for fluent, authentic English.",
        ["Ellipsis: 'John went to Paris; Sarah to Rome' (ellipsis of 'went')", "Substitution: 'Do you like coffee?' 'Yes, I do.' (do = like coffee)", "'So' and 'not' substitutes: 'Is she coming?' 'I think so.' / 'I hope not.'", "Comparative ellipsis: 'His score was higher than hers (was).'", "Stranding prepositions: 'Which conference did you go to?' (not 'to which')"],
        [("ellipsis", "elipsis / omisión", "He went to London; she to Berlin. (ellipsis of 'went')"), ("substitution", "sustitución", "Do you want tea? I do. (do replaces 'want tea')"), ("do-substitution", "sustitución con 'do'", "You don't play chess like he does."), ("so-substitution", "sustitución con 'so'", "Is that true? I think so.")],
        [("'I like chocolate.' 'So do I.' (substitution)", "Do-substitution in agreement"), ("He visited three countries; she four.", "Ellipsis in comparative"), ("'Which office are you in?' / 'It's the one John is in.' (stranding)", "Preposition stranding"), ("Some students passed; others did not.", "Substitution with 'did'")],
        "Rewrite these sentences using ellipsis or substitution where appropriate: 1) John bought a car; Mary bought a car. 2) Do you want to join us? I want to join you. 3) She's more experienced than he is experienced. 4) Which research did you rely on? 5) Some agree with the policy; some don't agree.",
        "Ellipsis creates rhythm in speech: 'John went to Paris; Sarah to Rome.' The second clause is faster and lighter because words are omitted. This mirrors natural pauses.",
        [("Ellipsis in English Grammar", "Papa English", null), ("Substitution and Ellipsis", "Cambridge English", null), ("Advanced Pronoun Substitution", "English Speeches", null)]
    );

    private static string C1L8() => C(
        "The subjunctive mood expresses wishes, recommendations, and unreal situations. In formal English, it appears in 'I suggest that he BE present' (not 'is'). Modal structures (could, might, may) express possibility, permission, and hypothetical situations with precision.",
        ["Subjunctive: 'I suggest that she be informed.' 'It is crucial that he arrive on time.' (NOT 'arrives')", "After verbs: recommend, insist, demand, propose, suggest", "After expressions: It is important that..., It is essential that..., It is vital that...", "Modal precision: can (present ability), could (past ability or possibility), might (possibility), may (formal permission)", "Perfect modals: should have, could have, might have express regret, criticism, speculation"],
        [("subjunctive mood", "modo subjuntivo", "The subjunctive mood is used after 'insist that'."), ("recommend that", "recomendar que", "I recommend that she be informed."), ("essential that", "esencial que", "It is essential that standards be maintained."), ("could have", "habría podido / podría haber", "She could have succeeded if she had tried."), ("should have", "debería haber / habría debido", "He should have known better.")],
        [("The company insists that all employees be trained thoroughly.", "Subjunctive in business context"), ("It is essential that these standards be maintained.", "Formal requirement"), ("He should have known better.", "Perfect modal: criticism"), ("She might have taken a different approach.", "Perfect modal: speculation"), ("I recommend that this policy be reconsidered.", "Subjunctive in formal proposal")],
        "Write 8 sentences: 4 using subjunctive (insist, recommend, it is essential that, it is vital that) and 4 using perfect modals (should have, could have, might have, may have) to express regret, criticism, or speculation.",
        "The subjunctive is often not pronounced differently from the indicative, but formal speech emphasizes the 'be': 'I insist that she BE present' — the 'be' sounds deliberate.",
        [("Subjunctive Mood in English", "Papa English", null), ("Perfect Modal Verbs", "Cambridge English", null), ("Advanced Modal Structures", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L9() => C(
        "Reported speech (or indirect speech) conveys what someone said without quoting directly. At C1, you master reporting structures beyond simple 'said' — including tell, ask, wonder, admit, acknowledge, insist — and handle tense shifts and reported questions precisely.",
        ["Reporting verbs: say, tell (person), ask, wonder, inquire, admit, acknowledge, deny, insist", "Tense shifts: direct 'I'm working' → reported 'She said (that) she was working'", "Reported questions: direct 'Where are you going?' → reported 'He asked where I was going'", "Reported requests/commands: direct 'Clean your room!' → reported 'She told him to clean his room'", "Backshift in time: past reported 'She said she was tired' (was working then) vs. present 'She says she is tired' (is still tired)"],
        [("admitted", "admitió", "He admitted (that) he had made a mistake."), ("denied", "negó", "She denied having seen the document."), ("acknowledge", "reconocer", "The committee acknowledged the report's findings."), ("alleged", "alegó", "The defendant alleged that he was elsewhere."), ("insisted", "insistió", "He insisted that he be allowed to speak.")],
        [("She said that she would attend the conference next month.", "Tense shift: present → conditional"), ("He asked me whether I had finished the report.", "Reported question with backshift"), ("The witness admitted that she had seen the suspect near the building.", "Reported admission with perfect"), ("The doctor told me to reduce my stress levels.", "Reported instruction"), ("They insisted that the proposal be reconsidered.", "Reported insistence with subjunctive")],
        "Convert 8 direct speech sentences into reported speech, including at least one reported question, one reported command, one reported negative statement, and one using a formal reporting verb (acknowledge, admitted, alleged, denied).",
        "In formal speech, 'he said' flows naturally, but in academic contexts, use varied reporting verbs: 'he argued', 'she proposed', 'they contended'. Reporting verbs signal your stance toward the information.",
        [("Reported Speech in English", "Papa English", null), ("Reporting Verbs and Advanced Structures", "Cambridge English", null), ("Indirect Speech Transformation", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L10() => C(
        "Advanced reported speech includes tense shifts (backshift), reported subjunctive, and reporting embedded clauses. Understanding backshift is crucial: 'He said he was tired' (at that moment) versus 'He said he is tired' (still tired now).",
        ["Tense backshift: present perfect → past perfect, future → conditional, present continuous → past continuous", "No backshift if reporting present situation: 'He says he is tired' (still tired)", "Reported subjunctive: 'I recommend that he be informed' → 'She recommended that he be informed'", "Reported conditionals: direct 'If I were you, I would...' → reported 'He suggested that if I were him, I would...'", "Embedded questions and clauses: 'She wondered why he hadn't called' (no question mark)"],
        [("backshift", "cambio de tiempo", "The tense backshift occurred when reporting past speech."), ("recommend that", "recomendar que", "I recommend that the proposal be accepted."), ("embedded", "incrustada / integrada", "The embedded clause provides crucial context."), ("contend", "sostener / argumentar", "The author contends that reform is necessary."), ("asserted", "afirmó rotundamente", "He asserted that the evidence was conclusive.")],
        [("She said she had been waiting for hours.", "Backshift: continuous to perfect continuous"), ("He insists that students be given more time to complete the exam.", "Reported subjunctive"), ("The report suggested that if the trend continued, demand would triple.", "Reported conditional"), ("She wondered whether anyone would volunteer.", "Embedded question"), ("They asserted that the findings contradicted previous research.", "Formal reporting verb")],
        "Write a dialogue (6-8 exchanges) between two people discussing a disagreement. Then convert it into reported speech, using backshift, at least one reported subjunctive, and at least 5 different reporting verbs.",
        "When reporting, your intonation subtly expresses your stance: skeptical 'She claimed...' vs. neutral 'She said...' vs. endorsing 'She correctly noted...'",
        [("Advanced Reported Speech", "Cambridge English", null), ("Reported Subjunctive in Academic Writing", "Academic English", null), ("Tense Backshift Rules", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L11() => C(
        "Participle clauses (reduced relative clauses) make writing more concise and sophisticated. 'The student who was working on the project' becomes 'The student working on the project' — more elegant and more natural in academic and professional contexts.",
        ["Participle clauses replace relative clauses: 'The woman standing there' = 'The woman who is standing there'", "Active -ing clauses: 'The children playing in the park' (= who are playing)", "Passive past participle clauses: 'The book published last year' (= which was published)", "Adverbial participle clauses: 'Completing the task, he felt relieved' (= After completing, he felt relieved)", "Perfect participles: 'Having finished the report, she submitted it' (= After having finished)"],
        [("participle clause", "cláusula de participio", "The participle clause modifies the noun."), ("reducing", "reduciendo / simplificando", "Reducing the clause made the sentence more concise."), ("pending", "pendiente", "All matters pending approval will be addressed next week."), ("pertaining to", "relativo a / que concierne", "The documents pertaining to this case are confidential."), ("resembling", "parecido a", "Rocks resembling meteorites were discovered.")],
        [("The student working on the project completed it on time.", "Participle clause replacing relative clause"), ("Having completed the analysis, she presented her findings.", "Perfect participle for temporal relationship"), ("The documents pertaining to the merger are confidential.", "Participle clause with preposition"), ("Pending final approval, the proposal will proceed.", "Adverbial participle clause"), ("Books published in the last decade show this trend.", "Past participle clause (passive)")],
        "Convert 10 sentences containing 'who/which is' or 'who/which was' into reduced participle clauses. Also write 3 sentences using perfect participles (having completed, having been informed, etc.).",
        "Participle clauses add sophistication: 'The woman standing near the door arrived yesterday' is more elegant than 'The woman who was standing near the door arrived yesterday.' Use them in formal writing.",
        [("Participle Clauses Explained", "Cambridge English", null), ("Reducing Relative Clauses", "Papa English", null), ("Advanced Sentence Structure", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L12() => C(
        "Advanced relative clause structures include whose + noun (possession), preposition + which (formal), and adverbial relative clauses (why, when, where, how). These structures add sophistication to complex, academic writing.",
        ["Whose + noun: 'The student whose essay was accepted...' (possession)", "Preposition + relative pronoun: 'The institution to which she applied...' (formal)", "Adverbial relative clauses: when, where, why, how, the reason why, the place where", "Postmodifying clauses: 'The person in charge' becomes 'The person in charge of the project'", "Non-defining vs defining: 'My brother, who is a doctor' (non-defining) vs 'My brother who is a doctor' (defining — I have multiple brothers)"],
        [("whose", "cuyo/a/os/as", "The student whose project won the award..."), ("in which", "en el cual / donde", "The laboratory in which the research was conducted..."), ("during which", "durante el cual", "The period during which the project was developing..."), ("whereby", "mediante / por lo cual", "A system whereby each person gets a vote..."), ("notwithstanding", "a pesar de / sin embargo", "The difficulties notwithstanding, they succeeded.")],
        [("The researchers to whom the grant was awarded began their work immediately.", "Preposition + which"), ("The reason why she left is still unclear.", "Adverbial relative clause with why"), ("The technique whereby rust is removed is highly effective.", "Defining relative with whereby"), ("The period during which the company expanded rapidly was very profitable.", "Adverbial relative clause"), ("The documents, whose copies were distributed, contained sensitive information.", "Possessive relative with non-defining clause")],
        "Write 10 complex sentences using: 4 with whose + noun, 3 with preposition + relative pronoun (to which, in which, for which, etc.), and 3 with adverbial relative clauses (when, where, why, during which).",
        "Non-defining relative clauses (set off by commas) need natural breath pauses in speech. Defining relative clauses flow without pauses. This rhythm helps you distinguish them when speaking.",
        [("Advanced Relative Clauses", "Cambridge English", null), ("Whose vs Of Which in English", "Papa English", null), ("Formal Relative Structures", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L13() => C(
        "Gradable adjectives (hot, cold, good, bad) admit degrees (very hot, quite hot). Ungradable adjectives (perfect, complete, impossible, dead) do not — you cannot say 'very complete'. At C1, you distinguish these for precision and avoid elementary mistakes.",
        ["Gradable adjectives: hot, cold, big, small, good, bad, interesting, difficult, expensive (accept degree modification)", "Ungradable adjectives: perfect, complete, entire, absolute, dead, alive, unique, impossible, essential (cannot be 'very')", "Intensifiers for gradable: very, quite, rather, fairly, pretty, extremely, incredibly, remarkably", "Intensifiers for ungradable: absolutely, completely, utterly, totally (modify the entire concept)", "Comparative/superlative with gradable only: 'more interesting' but NOT 'more perfect'"],
        [("ungradable", "no gradable / sin grados", "The word 'perfect' is ungradable."), ("intensifier", "intensificador", "Use 'very' as an intensifier with gradable adjectives."), ("absolute", "absoluto", "We need absolute certainty before proceeding."), ("entire", "entero", "The entire project was compromised."), ("unique", "único", "This approach is completely unique (not 'very unique').")],
        [("The situation is absolutely impossible.", "Ungradable: intensifier is 'absolutely'"), ("The presentation was very effective and quite persuasive.", "Gradable: multiple intensifiers"), ("This is a unique opportunity, not 'a very unique opportunity'.", "Ungradable corrected"), ("The data are entirely complete and ready for analysis.", "Ungradable: 'entirely' modifies the concept"), ("The solution is extremely simple but absolutely essential.", "Mix of gradable and ungradable")],
        "Identify which adjectives are gradable and which are ungradable in 15 given sentences. Then rewrite sentences containing incorrect intensifier use (e.g., 'very perfect', 'rather complete') with the correct intensifier choice.",
        "In speech, gradable intensifiers can be slightly reduced: 'vry good' (very good). Ungradable intensifiers remain fully pronounced: 'AB-so-LUTE-ly impossible' — the word carries weight.",
        [("Gradable vs Ungradable Adjectives", "Cambridge English", null), ("Intensifiers in English", "Papa English", null), ("Adjective Gradability", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L14() => C(
        "Adverb placement affects meaning and elegance. Adverbs can appear sentence-initial (formally), mid-position (typically), or end-position (naturally). Comment adverbs (frankly, undoubtedly, fortunately) signal the speaker's attitude and add sophistication.",
        ["Adverbial positions: sentence-initial (However, she succeeded.), mid-position (She has clearly misunderstood.), end-position (She worked diligently.)", "Frequency adverbs: position matters. 'I always enjoy it' vs 'Do you always enjoy it?' (after subject/auxiliary)", "Comment adverbs: frankly, undoubtedly, understandably, fortunately, regrettably, arguably, inevitably", "Focus adverbs: only, even, just, still, already — position changes meaning: 'Only she knows' vs 'She only knows'", "Manner adverbs typically end-position; frequency mid-position; degree/comment adverbs flexible"],
        [("comment adverb", "adverbio de comentario", "Undoubtedly, the proposal has merit."), ("arguably", "se podría argumentar", "Arguably, this is the best solution available."), ("regrettably", "lamentablemente", "Regrettably, the deadline was not met."), ("inevitably", "inevitablemente", "Inevitably, costs will increase."), ("inextricably", "indisolublemente", "The two issues are inextricably linked.")],
        [("Frankly, I don't agree with this approach.", "Comment adverb: sentence-initial"), ("She has undoubtedly made the right decision.", "Comment adverb: mid-position"), ("Regrettably, the project was cancelled.", "Comment adverb expressing speaker's attitude"), ("Only the CEO can authorize this expenditure.", "Focus adverb: change meaning with position"), ("The two concepts are inextricably linked.", "Manner adverb: end-position")],
        "Write 12 sentences: 4 using comment adverbs (frankly, undoubtedly, regrettably, fortunately), 4 with focus adverbs (only, even, just, still) showing position effects, and 4 with frequency/manner adverbs in correct positions.",
        "Comment adverbs signal your tone: 'Frankly, I disagree' (emphatic honesty), 'Arguably, the claim is weak' (academic distance). In speech, they often carry falling intonation.",
        [("Adverb Position and Word Order", "Cambridge English", null), ("Comment Adverbs in English", "Papa English", null), ("Adverb Placement for Advanced Learners", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L15() => C(
        "Complex prepositions (in front of, because of, in addition to, in accordance with, by virtue of) are more precise than simple prepositions. Mastering them elevates your formal writing and academic expression.",
        ["Complex prepositions: in front of, because of, by means of, in addition to, in spite of, by virtue of, in accordance with, with regard to, in the light of", "Distinction: 'in front of' (physical) vs 'ahead of' (competitive); 'because of' (cause) vs 'due to' (consequence)", "Idiomatic usage: 'owing to' (passive acknowledgment), 'on the basis of' (evaluation), 'in the course of' (during)", "Prepositional nuance: 'despite' (concessive) vs 'because of' (causal); choosing the right preposition changes logical meaning", "Register: 'because of' is neutral; 'owing to' is formal; 'due to' is less formal for written English"],
        [("by virtue of", "en virtud de", "She was appointed by virtue of her expertise."), ("in the light of", "a la luz de", "In the light of new evidence, the decision was reconsidered."), ("in accordance with", "de acuerdo con", "The process was followed in accordance with regulations."), ("notwithstanding", "no obstante / a pesar de", "The difficulties notwithstanding, progress was made."), ("pursuant to", "de conformidad con", "Pursuant to your request, we have provided the information.")],
        [("In accordance with the company policy, all employees must submit their reports.", "Complex preposition in formal context"), ("By virtue of his experience, he was chosen to lead the project.", "Formal appointment language"), ("In the light of recent developments, the committee has reconsidered its position.", "Preposition introducing revised information"), ("Notwithstanding the challenges, the team achieved all objectives.", "Concessive complex preposition"), ("Pursuant to the contract, payment is due within 30 days.", "Formal business language")],
        "Write 8 sentences incorporating: 2 with 'in accordance/accordance with', 2 with 'because of/due to' (show the difference), 2 with 'by virtue of/in virtue of', and 2 with other complex prepositions (notwithstanding, pursuant to, in the light of, in addition to).",
        "Complex prepositions carry formal weight in speech. Pronounce them clearly: 'in LIGHT of' (not 'inlightof'), 'PUR-suant TO' (not rushed). Clarity marks professionalism.",
        [("Complex Prepositions in English", "Cambridge English", null), ("Prepositional Phrases and Nuance", "Papa English", null), ("Advanced Prepositions for Formal Writing", "Learn English with EnglishClass101.com", null)]
    );

    private static string C1L16() => C(
        "Inversion (placing the verb before the subject) and cleft sentences (using 'it is...that' or 'what...is') add emphasis and sophistication. These structures signal native proficiency in formal, academic, and literary English.",
        ["Inversion with negative adverbs: 'Never have I seen such...' (formal emphasis)", "Inversion with 'so': 'So impressive were the results that...' (literary effect)", "Cleft sentences with 'it': 'It is the methodology that distinguishes this study.' (emphasis on methodology)", "Cleft sentences with 'what': 'What matters most is implementation.' (emphasis on what matters)", "Pseudo-cleft: 'The key issue is implementation' vs true cleft 'It is implementation that is the key issue'"],
        [("inversion", "inversión", "Inverted sentence structure for emphasis."), ("cleft", "dividida (estructura)", "A cleft sentence emphasizes part of the clause."), ("pseudo-cleft", "pseudo-escisión", "This is a pseudo-cleft construction."), ("striking", "notable / sorprendente", "Striking results emerged from the study."), ("paramount", "supremo / primordial", "Attention to detail is paramount.")],
        [("Never have I encountered such compelling evidence.", "Inversion with negative for emphasis"), ("What they discovered was a fundamental flaw in the theory.", "Cleft with 'what'"), ("It is the budget constraints that limit our options.", "Cleft with 'it'"), ("So thorough was the investigation that no stone was left unturned.", "Inversion with 'so'"), ("Paramount to success is clear communication.", "Inverted copular structure")],
        "Write 10 sentences using inversion and cleft structures: 4 with negative inversion ('Never...', 'Seldom...'), 3 with 'it is...that' clefts, 2 with 'what...is' clefts, and 1 with pseudo-cleft. Then explain which elements are emphasized.",
        "In speech, inverted structures carry dramatic intonation. 'NEVER have I SEEN such...' — the never is stressed, then a falling pitch. Cleft sentences sound formal and deliberate: 'It IS the BUDGET that matters.'",
        [("Inversion for Emphasis in English", "Cambridge English", null), ("Cleft Sentences Explained", "Papa English", null), ("Advanced Emphasis Structures", "Learn English with EnglishClass101.com", null)]
    );

    private static string C2L1() => C(
        "Non-defining relative clauses (set off by commas) provide extra information about a noun without restricting its meaning. They often use which, whose, and where, and are more common in formal, academic writing. Understanding their structure is essential for C2 sophistication.",
        ["Non-defining: 'The report, which was published last year, became a bestseller.' (commas — the report is already identified)", "Defining: 'The report that was published last year became a bestseller.' (no commas — identifies which report)", "Whose in non-defining: 'The author, whose books are famous, gave a lecture.' (possession, non-restrictive)", "Where in non-defining: 'The office, where I worked for years, is now closed.' (location, non-restrictive)", "Formal prepositions: 'The laboratory in which the research was conducted is prestigious.' (more formal than 'that')"],
        [("sentential relative", "relativa sentencial / de sentencia", "He lost the game, which upset him greatly. (which = entire clause)"), ("restrictive", "restrictiva / que limita", "The students who submitted late received lower scores."), ("non-restrictive", "no restrictiva / que añade información", "The students, who all worked hard, passed the exam."), ("whereby", "mediante / por lo cual", "A system whereby everyone gets a fair chance."), ("during which", "durante el cual", "The period during which the crisis escalated.")],
        [("The book, which won three awards, is now out of print.", "Non-defining relative with which"), ("The laboratory in which this research was conducted is famous.", "Formal preposition + relative"), ("She studied medicine, which surprised no one.", "Sentential relative clause"), ("The period during which he served was transformative.", "Adverbial relative with during which")],
        "Write 8 sentences: 4 with non-defining relative clauses (with commas) and 4 with formal prepositions + relative pronouns (to which, in which, for which, during which).",
        "Non-defining relative clauses require natural pauses (commas) in speech. 'My brother, who is a doctor' flows with pauses; without commas, it sounds defining: 'My brother who is a doctor' (implying you have multiple brothers).",
        [("Non-defining Relative Clauses", "Cambridge English", null), ("Formal Relative Clause Structures", "Papa English", null), ("Advanced Relative Clauses in Academic English", "English Speeches", null)]
    );

    private static string C2L2() => C(
        "Sentential relative clauses use which to refer to an entire previous clause rather than a specific noun. Relative adverbs (where, when, why, how) add sophistication. These structures are hallmarks of academic and formal writing.",
        ["Sentential which: 'He forgot his keys, which caused him to miss the meeting.' (which = the entire situation)", "'He admitted the error, which surprised everyone.' (which = his admission, not just the error)", "Relative adverbs: why (reason), when (time), where (place), how (manner)", "The reason why, the place where, the time when — these combine relative adverbs with antecedents", "Common in academic and literary writing; creates sophisticated causal and temporal connections"],
        [("sentential", "sentencial / de la oración entera", "The sentential relative clause adds commentary."), ("relative adverb", "adverbio relativo", "She explained the reason why she left."), ("whereby", "mediante el cual / por lo cual", "A process whereby everyone benefits equally."), ("wherein", "en el cual / dentro del cual", "The document wherein all terms are detailed is binding."), ("whence", "de donde / de ahí", "The place whence he came is forgotten.")],
        [("He announced his resignation, which shocked the team.", "Sentential relative clause"), ("The reason why she refused is still unclear.", "Relative adverb + antecedent"), ("The period during which we negotiated was tense.", "Sophisticated temporal relative"), ("A system whereby fairness is ensured appeals to everyone.", "Formal relative structure")],
        "Write 6 sentences: 3 with sentential which clauses and 3 with relative adverbs (why, where, when, how) + antecedents. Show causal and temporal relationships.",
        "'Sentential' is sen-TEN-shal (stress 2nd). 'Whereby' is where-BY (stress 2nd). 'Wherein' is where-IN (stress 2nd). These formal structures are pronounced with clear stress for emphasis and formality.",
        [("Sentential Relative Clauses", "Cambridge English", null), ("Relative Adverbs and Their Use", "Papa English", null), ("Advanced Relative Structures", "English Speeches", null)]
    );

    private static string C2L3() => C(
        "Reduced relative clauses — participle clauses and adjectival phrases — replace full relative clauses with more concise, elegant structures. 'The student working diligently' replaces 'The student who is working diligently.' This is essential for sophisticated, professional writing.",
        ["Active participles: 'The woman standing near the door' = 'The woman who is standing'", "Passive participles: 'The document submitted yesterday' = 'The document which was submitted yesterday'", "Perfect participles: 'Having finished the report, she left.' = 'After having finished, she left.'", "Adjectival phrases: 'The person in charge' (replaces 'the person who is in charge')", "Creates sophisticated flow and avoids repetitive 'who/which' structures"],
        [("participle clause", "cláusula de participio / estructura de participio", "The participle clause modifies the noun efficiently."), ("reduced relative", "relativa reducida", "Reducing clauses creates elegant, formal writing."), ("pending", "pendiente / esperando", "All matters pending approval will be addressed."), ("pertaining to", "relativo a / que concierne", "Documents pertaining to the merger are confidential."), ("resembling", "parecido a / que se asemeja a", "Phenomena resembling this pattern have been documented.")],
        [("The student working on the project completed it early.", "Active participle clause"), ("Having received the promotion, she felt relieved.", "Perfect participle clause"), ("Matters pertaining to security are classified.", "Participle + preposition"), ("The documents submitted contain sensitive information.", "Passive participle clause"), ("The period during which... = The period during which the crisis escalated was difficult.", "Formal relative with adverbial participle")],
        "Convert 10 sentences containing full relative clauses ('who is', 'which was', etc.) into reduced participle clauses. Write 3 additional sentences using perfect participles (having completed, having been informed).",
        "Participle clauses flow smoothly: 'The woman standing at the entrance' reads as one unit. 'Pending' is PEN-ding (stress 1st). 'Pertaining' is per-TANE-ing (stress 2nd). Reduced clauses should sound concise, not clipped.",
        [("Participle Clauses and Reduction", "Cambridge English", null), ("Reduced Relative Clauses in Academic Writing", "Papa English", null), ("Elegant Sentence Structure with Participles", "English Speeches", null)]
    );

    private static string C2L4() => C(
        "Cleft sentences emphasize specific elements using 'it is...that' (it-clefts) or 'what...is' (wh-clefts). They are powerful tools for shifting emphasis and creating sophisticated English. 'It is methodology that distinguishes this study' emphasizes 'methodology'.",
        ["It-clefts: 'It is the budget that limits us.' (emphasizes 'the budget')", "Wh-clefts: 'What matters most is implementation.' (emphasizes 'implementation')", "Pseudo-clefts: 'The key to success is clarity.' (no 'it is' or 'what is')", "Clefts allow you to rearrange normal word order for rhetorical effect", "Common in formal writing, speeches, and when arguing a point"],
        [("cleft sentence", "oración escindida / de énfasis", "Cleft sentences shift emphasis for effect."), ("it-cleft", "oración escindida con 'it'", "It was her dedication that won the award."), ("wh-cleft", "oración escindida con 'what'", "What impressed us was his honesty."), ("pseudo-cleft", "pseudo-escisión", "The real issue is communication."), ("emphasize", "enfatizar / poner énfasis", "Use clefts to emphasize key points.")],
        [("It is the data that supports our conclusion.", "It-cleft emphasizing 'data'"), ("What they need is better communication.", "Wh-cleft emphasizing 'communication'"), ("The problem is not lack of funding but poor management.", "Pseudo-cleft structure"), ("It was her persistence that led to success.", "Emphatic it-cleft")],
        "Write 12 sentences: 4 with it-clefts, 4 with wh-clefts, and 4 with pseudo-clefts. Vary what you emphasize to show rhetorical control.",
        "'Cleft' is KLEFT (1 syllable, stress 1st). 'It-cleft' emphasizes a word with slight stress increase: 'It IS the BUDGET that matters.' 'Wh-cleft' also shifts stress: 'WHAT matters is CLARITY.' These create rising-falling intonation for emphasis.",
        [("Cleft Sentences for Emphasis", "Cambridge English", null), ("It-Clefts and Wh-Clefts Explained", "Papa English", null), ("Rhetorical Emphasis in Advanced English", "English Speeches", null)]
    );

    private static string C2L5() => C(
        "Nominalization transforms action (verbs/adjectives) into things (nouns) for formal, sophisticated style. 'The government decided to reform education' becomes 'The government's reform of education.' Nominalizations dominate academic and professional writing.",
        ["Verb → Noun: develop→development, analyze→analysis, argue→argument, judge→judgment", "Adjective → Noun: able→ability, possible→possibility, significant→significance", "Benefits: conciseness, formality, logical flow, academic register", "Disadvantage: overuse creates dense, hard-to-read prose — balance is essential", "Signal pattern: possessive + nominalization = sophisticated style"],
        [("nominalization", "nominalización", "Nominalization makes writing more formal."), ("transformation", "transformación", "The transformation of the system took years."), ("analysis", "análisis", "The analysis revealed unexpected patterns."), ("implementation", "implementación / ejecución", "Implementation of the policy will begin next month."), ("achievement", "logro / realización", "The achievement of our goals requires commitment.")],
        [("Rather than: 'We analyzed the data.' / Better: 'Our analysis of the data.'", "Nominalization for formality"), ("The development of these strategies led to success.", "Nominalization in formal context"), ("His ability to lead inspired confidence.", "Possessive + nominalization"), ("The implementation of reforms faced resistance.", "Nominalization in business writing")],
        "Rewrite these sentences using nominalization: 1) The team analyzed the results. 2) Companies must innovate to survive. 3) The government approved the policy. 4) Students demonstrated remarkable improvement. 5) She contributed significantly to the project.",
        "'Nominalization' is nom-i-nal-i-ZA-shun (stress 4th). 'Development' is de-VEL-up-ment (stress 2nd). Nominalizations are pronounced clearly to signal formality and abstraction.",
        [("Nominalization in Academic Writing", "Cambridge English", null), ("Transform Verbs to Nouns", "Papa English", null), ("Formal Style Through Nominalization", "English Speeches", null)]
    );

    private static string C2L6() => C(
        "Fronting — moving sentence elements to the front for emphasis or stylistic effect — is a hallmark of sophisticated writing. 'Remarkable were the results.' 'Particularly troubling is the trend.' Fronting shifts attention and creates rhetorical impact.",
        ["Fronting an adjective: 'Remarkable was his dedication.' (vs. 'His dedication was remarkable.')", "Fronting an adverbial: 'Only then did we understand.' (creates narrative tension)", "Fronting an object: 'This issue, we must address.' (conversational emphasis)", "Fronting a prepositional phrase: 'In the corner sat an old chair.' (narrative/literary effect)", "More common in formal, literary, and journalistic writing; creates variety and emphasis"],
        [("fronting", "anteposición / inversión", "Fronting emphasizes specific elements."), ("inversion", "inversión", "Inversion follows fronting of certain elements."), ("narrative", "narrativo / del relato", "Fronting creates narrative tension and flow."), ("literary", "literario", "Literary writing uses fronting for stylistic effect."), ("constituent", "constituyente / elemento sintáctico", "Fronting moves a constituent to sentence-initial position.")],
        [("Remarkable was her composure throughout the crisis.", "Fronted adjective"), ("Only after years of effort did she succeed.", "Fronted adverbial"), ("In the distance loomed the mountains.", "Fronted prepositional phrase"), ("Particularly concerning is the lack of transparency.", "Fronted adjectival phrase")],
        "Write 10 sentences using fronting: 3 with fronted adjectives, 3 with fronted adverbials, 2 with fronted prepositional phrases, and 2 with fronted objects or complements.",
        "'Fronting' is FRONT-ing (stress 1st). When you front elements, they receive primary stress: 'REMARKABLE was her COMPOSURE.' This stress pattern signals emphasis and draws listener attention.",
        [("Fronting for Emphasis and Style", "Cambridge English", null), ("Inversion and Fronting in English", "Papa English", null), ("Literary Use of Fronting", "English Speeches", null)]
    );

    private static string C2L7() => C(
        "Negative inversion (with Never, Hardly, Seldom) and degree inversion (with So, Such) create dramatic emphasis. 'Never have I seen such courage.' 'So striking were the results that...' These are hallmarks of formal, literary, and sophisticated English.",
        ["Negative inversion: 'Never have I witnessed...' (auxiliary before subject after negative adverb)", "'Hardly had she arrived when...' (temporal + negative inversion)", "'Seldom does such talent emerge.' (regular inversion)", "Degree inversion: 'So impressive were the findings...' (So + adjective + inversion)", "'Such remarkable progress have they made...' (Such + adjective + noun + inversion)", "Creates emphasis and is common in formal speeches, academic writing, and literature"],
        [("inversion", "inversión", "Inversion places the verb before the subject."), ("negative adverb", "adverbio negativo", "Negative adverbs trigger inversion."), ("degree", "grado / medida", "Degree inversion emphasizes extent or intensity."), ("trigger", "desencadenar / provocar", "Certain adverbs trigger inversion."), ("consequential", "como consecuencia / resultante", "So damaging were the consequences that...")],
        [("Never have I encountered such evidence.", "Negative inversion with Never"), ("Hardly had the meeting begun when objections arose.", "Negative inversion with Hardly"), ("So thorough was the investigation that no stone was left unturned.", "Degree inversion with So"), ("Such significant advances have rarely been achieved.", "Degree inversion with Such")],
        "Write 12 sentences: 4 with negative inversion (Never, Hardly, Seldom, Rarely), 4 with So inversion, and 4 with Such inversion. Ensure grammatical accuracy and natural emphasis.",
        "Negative inversion is dramatic: 'NEV-er HAVE I seen...' — stress the never, then the auxiliary. Degree inversion: 'SO STRIK-ing WERE the RESULTS...' — falling intonation emphasizes the degree and creates formality.",
        [("Inversion in English: Negative and Degree", "Cambridge English", null), ("Emphasis Through Inversion", "Papa English", null), ("Literary Use of Inversion", "English Speeches", null)]
    );

    private static string C2L8() => C(
        "Focus adverbs (even, only, just, still, already, so) change meaning based on position. 'Only she knows' vs. 'She only knows' — different emphasis. These subtleties mark native proficiency and are essential for nuanced communication.",
        ["Position of 'only': 'Only she knows' (no one else knows) vs. 'She only knows' (she merely knows, doesn't understand) vs. 'She knows only him' (she knows only that person)", "'Even' adds emphasis: 'Even he passed' (surprisingly), 'He even passed' (contrary to expectations)", "'Just' as focus: 'I just arrived' (moments ago) vs. 'I just want peace' (all I want)", "'Still' and 'already' show state changes: 'I'm still waiting' vs. 'I've already left'", "Shift in position creates shift in meaning — this is subtle but crucial for C2"],
        [("focus adverb", "adverbio de enfoque / de alcance", "Focus adverbs restrict or emphasize meaning."), ("scope", "alcance / dominio", "The scope of 'only' determines meaning."), ("contrastive", "contrastivo / de contraste", "Even creates contrastive stress."), ("temporal", "temporal / de tiempo", "Just marks temporal proximity."), ("already", "ya / ya", "Already marks completed action or change of state.")],
        [("Only she attended the meeting. (No one else)", "Only with narrow scope"), ("She only attended the meeting. (She merely attended, didn't participate)", "Only with wider scope"), ("Even he understood the concept. (Surprising)", "Even for contrast"), ("I've already finished. (Change of state before now)", "Already marking completion")],
        "Write 15 sentences showing how position of focus adverbs (only, even, just, still, already) changes meaning. Include at least 2 sentences for each adverb, showing different positions.",
        "Focus adverbs carry subtle stress: 'ONLY she knows' (no one else) vs. 'she ONLY knows' (merely knows). The stress position signals the meaning. In rapid speech, focus adverbs can be reduced but should remain clear.",
        [("Position of Focus Adverbs", "Cambridge English", null), ("Only and Its Meanings", "Papa English", null), ("Scope Ambiguity in Focus Adverbs", "English Speeches", null)]
    );

    private static string C2L9() => C(
        "Complex noun phrases use extensive pre- and post-modification to build sophisticated, information-dense expressions. 'The government decision on education policy' vs. 'The government's education policy decision'— word order and modification create different formality and emphasis.",
        ["Pre-modification: adjectives and nouns before the head: 'the tall administrative building', 'government education policy'", "Post-modification: prepositional phrases, relative clauses, participles after the head: 'the analysis of the data by experts'", "Stacking modifiers: 'The comprehensive analysis of survey data from three countries conducted over two years'", "Register shift: informal 'the big problem we discussed' vs. formal 'the significant problem pertaining to implementation'", "Complex NPs are hallmarks of academic, legal, and formal writing"],
        [("noun phrase", "sintagma nominal", "Complex noun phrases convey dense information."), ("pre-modification", "premodificación", "Pre-modification adds specificity."), ("post-modification", "postmodificación", "Post-modification elaborates meaning."), ("constituent", "constituyente / componente", "Each constituent contributes meaning."), ("embedding", "incrustación", "Embedding creates hierarchical meaning.")],
        [("The policy on education reform implemented by the government", "Complex post-modification"), ("The government's comprehensive education reform initiative", "Complex pre-modification"), ("The three-year study of climate patterns across five continents published last year", "Stacked pre- and post-modification"), ("Matters concerning administrative procedure and implementation responsibility", "Formal complex NP")],
        "Identify and analyze 5 complex noun phrases from academic or formal texts. Map their pre- and post-modification. Then write 8 sentences using complex NPs with at least 3 levels of modification each.",
        "Complex noun phrases should flow naturally: 'the comprehensive analysis OF the data BY experts FROM three countries.' Slight pauses help the listener parse meaning without losing naturalness.",
        [("Complex Noun Phrases in Academic Writing", "Cambridge English", null), ("Modification and Noun Phrase Structure", "Papa English", null), ("Information Density in English", "English Speeches", null)]
    );

    private static string C2L10() => C(
        "Idiomatic prepositions appear in fixed expressions where meaning cannot be derived from individual words. 'At stake' (not 'in stake'), 'by means of' (not 'with means'), 'in accordance with' (not 'in agreement with'). These fixed collocations are essential for native-like fluency.",
        ["'At stake' (risk), 'at hand' (present/available), 'at random' (without plan)", "'By means of' (using), 'by virtue of' (due to), 'by dint of' (through effort)", "'In light of' (considering), 'in accordance with' (following), 'in the course of' (during)", "'On the basis of' (evaluating by), 'on account of' (because), 'on behalf of' (representing)", "Mistakes here are very noticeable to native speakers — these must be learned as fixed expressions"],
        [("at stake", "en juego / en riesgo", "The company's reputation is at stake."), ("by virtue of", "en virtud de / por", "She was appointed by virtue of her expertise."), ("in light of", "a la luz de", "In light of new evidence, we reconsidered."), ("on account of", "debido a / por", "The meeting was postponed on account of weather."), ("in accordance with", "de acuerdo con", "Procedures were followed in accordance with policy.")],
        [("The success of the project is at stake.", "Idiomatic at stake"), ("By means of careful planning, they avoided disaster.", "Idiomatic by means of"), ("In accordance with regulations, we must submit reports.", "Formal idiomatic in accordance with"), ("On the basis of available evidence, we conclude...", "Academic idiomatic on the basis of")],
        "Write 12 sentences using idiomatic fixed prepositions: 'at stake', 'at hand', 'by means of', 'by virtue of', 'in light of', 'in accordance with', 'on the basis of', 'on account of', 'on behalf of', plus 3 others from your knowledge.",
        "Idiomatic prepositions should flow smoothly as fixed chunks: 'in LIGHT of' (not in-LITE of), 'by VIRT-yoo' (not by-VIRT-yoo). These are pronounced as units, not word-by-word.",
        [("Idiomatic Prepositions and Fixed Expressions", "Cambridge English", null), ("Prepositional Collocations in English", "Papa English", null), ("Advanced Preposition Usage", "English Speeches", null)]
    );

    private static string C2L11() => C(
        "Advanced phrasal verbs show complex patterns: transitive vs. intransitive, separable vs. inseparable, and multi-word particles. 'Turn down' (transitive) vs. 'run into' (transitive), vs. 'look out' (intransitive). Understanding these patterns is essential for advanced fluency.",
        ["Transitive separable: 'turn down the offer' or 'turn the offer down'", "Transitive inseparable: 'run into trouble' (NOT 'run trouble into')", "Intransitive: 'look out!' (no object possible)", "Three-word phrasal verbs: 'look forward to', 'get away with', 'catch up with' (with = inseparable preposition)", "Usage errors are very noticeable — mastery shows sophistication"],
        [("transitive", "transitivo", "This phrasal verb is transitive."), ("inseparable", "inseparable", "Some phrasal verbs are inseparable."), ("particle", "partícula", "The particle changes the verb's meaning."), ("multi-word", "de múltiples palabras", "Multi-word phrasal verbs are complex."), ("collocation", "colocación", "Phrasal verbs form fixed collocations.")],
        [("She turned down his proposal. / She turned his proposal down.", "Transitive separable"), ("He ran into an old friend yesterday.", "Transitive inseparable"), ("They ran up against unexpected difficulties.", "Three-word phrasal verb"), ("I'm looking forward to our meeting.", "Three-word with preposition")],
        "Write sentences using: 5 transitive separable phrasal verbs (show both orders), 5 transitive inseparable, 3 intransitive, and 3 multi-word phrasal verbs. Verify each pattern is correct.",
        "Phrasal verbs form tight units: 'turn DOWN the offer' — stress the particle. Multi-word: 'look FORWARD TO' — stress forward and to. Position changes meaning, but pronunciation rhythm stays consistent.",
        [("Phrasal Verbs: Patterns and Positions", "Cambridge English", null), ("Advanced Phrasal Verb Structures", "Papa English", null), ("Mastering Phrasal Verbs at C2", "English Speeches", null)]
    );

    private static string C2L12() => C(
        "Collocations and idiomatic prepositional phrases are word partnerships that native speakers use naturally. 'Strong coffee' (not 'powerful coffee'), 'reach an agreement' (not 'arrive at an agreement'), 'come to a conclusion' (not 'come to a decision' when referring to deduction). These cannot be learned rule-by-rule.",
        ["Strong collocations: 'make a decision', 'reach a conclusion', 'come to an agreement', 'raise an issue'", "Weak collocations: 'good/bad luck' (but NOT 'good/bad accident')", "Idiomatic: 'give sb a hand' (help), 'get sb's drift' (understand), 'bend over backwards' (work very hard)", "Register matters: 'kick the bucket' (informal) vs. 'pass away' (formal) = same meaning, different collocations", "Collocation errors mark non-natives immediately — recognition and production are both crucial"],
        [("collocation", "colocación", "These words form a strong collocation."), ("strong collocation", "colocación fuerte / frecuente", "Strong collocations are less flexible."), ("weak collocation", "colocación débil", "Weak collocations allow more variation."), ("give sb a hand", "echar una mano", "Can you give me a hand?"), ("get sb's drift", "captar lo que alguien quiere decir", "I get your drift.")],
        [("They reached an agreement after long negotiations.", "Standard collocation"), ("She made a remarkable discovery.", "Collocation: made + discovery"), ("He bent over backwards to help us.", "Idiomatic collocation"), ("The committee came to an unfortunate conclusion.", "Collocation: come to + conclusion")],
        "Find 15 strong collocations in English texts. For each, explain why the specific verb/adjective is used rather than alternatives. Then write 8 sentences using 8 different strong collocations naturally.",
        "Collocations should sound automatic, not constructed: 'make a DECISION' flows as one unit. 'Strong collocations' are pronounced without hesitation — they're automatic for native speakers.",
        [("English Collocations and Idioms", "Cambridge English", null), ("Verb-Noun Collocations", "Papa English", null), ("Idiomatic Expressions and Collocations", "English Speeches", null)]
    );

    private static string C2L13() => C(
        "Discourse markers organize extended text and signal logical relationships. Additive markers (furthermore, moreover), contrastive (nevertheless, however), causal (consequently, therefore), sequential (subsequently, eventually). These guide readers through complex arguments and mark sophisticated writing.",
        ["Additive: furthermore, moreover, in addition, additionally, as well as", "Contrastive: nevertheless, however, conversely, by contrast, on the other hand, yet", "Causal: consequently, therefore, as a result, due to, because of, for this reason", "Sequential: subsequently, eventually, next, later, meanwhile, in due course", "More formal markers appear sentence-initially or mid-sentence to guide argument flow"],
        [("furthermore", "además / por otra parte", "Furthermore, the evidence suggests..."), ("nevertheless", "sin embargo / no obstante", "The data are inconclusive; nevertheless, patterns emerge."), ("consequently", "en consecuencia / por lo tanto", "The delay was problematic; consequently, costs increased."), ("subsequently", "posteriormente / después", "He joined in 2010; subsequently, he led the department."), ("conversely", "por el contrario / inversamente", "Some support expansion; conversely, others favor consolidation.")],
        [("The study was limited in scope. Furthermore, the sample size was small.", "Additive marker"), ("The evidence is mixed. Nevertheless, trends favor this conclusion.", "Contrastive marker"), ("The project was delayed. Consequently, the budget exceeded projections.", "Causal marker"), ("The initial phase was challenging. Subsequently, progress accelerated.", "Sequential marker")],
        "Write a 15-20 sentence paragraph on a topic of your choice using at least 8 different discourse markers to organize ideas logically. Label each marker and its function (additive, contrastive, causal, sequential).",
        "Discourse markers are typically unstressed bridges: 'Furthermore' — the rhythm emphasizes what follows, not the marker itself. 'Consequently' flows as the bridge between two ideas.",
        [("Discourse Markers and Cohesion", "Cambridge English", null), ("Linking Words for Advanced Writers", "Papa English", null), ("Organization Through Discourse Markers", "English Speeches", null)]
    );

    private static string C2L14() => C(
        "Cohesive devices — reference, substitution, conjunction, and ellipsis — tie sentences and ideas together. Understanding how to create and follow cohesion is crucial for reading and writing coherent, sophisticated texts. Cohesion is the glue that makes writing flow.",
        ["Reference: pronouns refer back to nouns, maintaining continuity ('She is a doctor. She specializes...')", "Substitution: pronouns or verbs replace earlier words ('Do you like coffee? I do.')", "Conjunction: linking words show relationships (but, and, or, because, although)", "Ellipsis: omitting understood words for brevity ('John went to Paris; Sarah to Rome')", "Thematic progression: moving from given (known) to new information creates flow"],
        [("cohesion", "cohesión", "Strong cohesion guides readers through text."), ("reference", "referencia / anáfora", "The pronoun 'it' refers back to the noun."), ("substitution", "sustitución", "Do substitution with 'do' or 'so'."), ("conjunction", "conjunción / nexo", "Conjunctions link ideas logically."), ("thematic", "temático", "Thematic progression shows text structure.")],
        [("She arrived early. She was nervous.", "Reference: 'she' maintains cohesion"), ("'Will you attend?' 'Yes, I will.'", "Substitution: 'will' replaces 'attend'"), ("He studied hard. However, he failed the exam.", "Conjunction showing contrast"), ("Some students passed; others did not.", "Ellipsis: 'passed' understood in second clause")],
        "Analyze a 2-3 paragraph academic text. Map all cohesive devices (reference, substitution, ellipsis, conjunction). Show how thematic progression (given→new) creates flow. Then write a 10-sentence paragraph demonstrating strong cohesion through all four types of devices.",
        "Cohesive devices create rhythm: reference pronouns are usually unstressed ('She, she, she' flows naturally). Cohesion should be invisible — readers follow the argument without noticing the devices.",
        [("Cohesion in English Texts", "Cambridge English", null), ("Creating Coherence and Cohesion", "Papa English", null), ("Discourse Cohesion Strategies", "English Speeches", null)]
    );

    private static string C2L15() => C(
        "Ellipsis — omitting words understood from context — creates natural, concise English. 'He went to Paris; she to Rome' (ellipsis of 'went'). 'Better late than never' (ellipsis of 'it is'). Ellipsis makes writing and speech more fluent and less repetitive.",
        ["Verbal ellipsis: omitting the verb or auxiliary ('John passed; so did Mary')", "Nominal ellipsis: omitting the noun ('I prefer red ones over blue ones' → 'I prefer red over blue')", "Clausal ellipsis: omitting entire clauses when understood ('He's coming tomorrow — I think so')", "Stranding: leaving a preposition at the end when the object is omitted ('What do you think about?' vs. 'What office are you in?')", "Creates rhythm and avoids awkward repetition"],
        [("ellipsis", "elipsis / omisión", "Ellipsis makes sentences concise."), ("verbal", "verbal / del verbo", "Verbal ellipsis omits the verb."), ("nominal", "nominal / del sustantivo", "Nominal ellipsis omits the noun."), ("stranding", "preposición al final", "Preposition stranding is natural in informal speech."), ("implied", "implícito / sobreentendido", "Understanding ellipsis requires recognizing implied words.")],
        [("John bought a car; Mary, a truck.", "Nominal ellipsis of 'bought a'"), ("'Do you want to come?' 'I'd like to.' (ellipsis of verb + object)", "Verbal ellipsis"), ("'She's more experienced than he is.' → 'She's more experienced than he.' (ellipsis of 'is')", "Ellipsis in comparison"), ("'Which conference did you attend?' (stranding of preposition)", "Preposition stranding")],
        "Rewrite 12 sentences using ellipsis: 4 with verbal ellipsis, 4 with nominal ellipsis, 2 with comparative ellipsis, and 2 with stranded prepositions. Ensure they sound natural.",
        "Ellipsis creates rhythm and fluency: 'John went to Paris; Sarah to Rome.' The second clause is faster because words are omitted — this mirrors natural speech patterns.",
        [("Ellipsis in English Grammar", "Cambridge English", null), ("Omission and Ellipsis", "Papa English", null), ("Creating Fluency Through Ellipsis", "English Speeches", null)]
    );

    private static string C2L16() => C(
        "Substitution — using pronouns or auxiliary verbs to replace clauses — allows native speakers to speak elegantly without repetition. 'Do you like coffee?' 'Yes, I do.' / 'Is she coming?' 'I think so.' / 'Some agree; others don't.' This is fluency and sophistication.",
        ["Do-substitution: 'She likes coffee; I do too.' (do = like coffee)", "So/not-substitution: 'Is he coming?' 'I hope so.' / 'Will they succeed?' 'I hope not.'", "Substitution with auxiliaries: 'She's finished. I haven't.' (have = finished)", "Substitution makes writing and speech more natural and less repetitive", "Errors in substitution mark non-native speakers — mastery shows fluency"],
        [("substitution", "sustitución", "Substitution replaces words to avoid repetition."), ("do-substitution", "sustitución con 'do'", "Do substitutes for the main verb."), ("so-substitution", "sustitución con 'so'", "So replaces an affirmative clause."), ("not-substitution", "sustitución con 'not'", "Not replaces a negative clause."), ("auxiliary", "verbo auxiliar", "Auxiliaries can substitute for main verbs.")],
        [("'Do you like this?' 'Yes, I do.' (do = like this)", "Do-substitution"), ("'Is he aware of the issue?' 'I think so.' (so = he is aware)", "So-substitution"), ("'Will she attend?' 'I hope not.' (not = she won't attend)", "Not-substitution"), ("'They finished early. We haven't.' (have = finished)", "Auxiliary substitution")],
        "Write 20 short dialogues: 4 using do-substitution (various tenses), 4 using so-substitution, 4 using not-substitution, 4 using auxiliary substitution, and 4 mixing multiple types. Make them sound natural.",
        "Substitution is automatic in native speech: 'Yes, I DO' (slight stress on do), 'I HOPE so' (so is usually unstressed). Substitution flows naturally — it should sound unthinking and fluent.",
        [("Substitution and Ellipsis in English", "Cambridge English", null), ("Do-Support and Substitution", "Papa English", null), ("Native-Like Use of Substitution", "English Speeches", null)]
    );

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string C(
        string explanation,
        string[] keyPoints,
        (string w, string t, string ex)[]? vocab = null,
        (string s, string n)[]? examples = null,
        string? writingPrompt = null,
        string? pronunciationTip = null,
        (string title, string channel, string? videoId)[]? videos = null) =>
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
            var exId = Guid.NewGuid();
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
                        Id = Guid.NewGuid(),
                        ExerciseId = exId, Text = shuffled[j],
                        IsCorrect = shuffled[j] == correct,
                        Explanation = shuffled[j] == correct ? "Correct!" : null
                    });
            }
        }
    }
}
