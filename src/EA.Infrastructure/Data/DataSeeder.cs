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
        ctx.Modules.Add(NewModule(m1, levelId, "Greetings & Introductions", "Learn how to say hello and introduce yourself.", 1, 2));
        ctx.Modules.Add(NewModule(m2, levelId, "Colors & Everyday Objects", "Learn basic vocabulary for daily life.", 2, 2));
        ctx.Modules.Add(NewModule(m3, levelId, "Family & Pronouns", "Describe your family and master essential pronouns and 'to be'.", 3, 2));
        ctx.Modules.Add(NewModule(m4, levelId, "Time & Food", "Learn days, months, and basic food vocabulary.", 4, 2));

        var l1 = G("a1b10000-0000-0000-0000-000000000001");
        var l2 = G("a1b20000-0000-0000-0000-000000000002");
        var l3 = G("a1b30000-0000-0000-0000-000000000003");
        var l4 = G("a1b40000-0000-0000-0000-000000000004");
        var l5 = G("a1b50000-0000-0000-0000-000000000005");
        var l6 = G("a1b60000-0000-0000-0000-000000000006");
        var l7 = G("a1b70000-0000-0000-0000-000000000007");
        var l8 = G("a1b80000-0000-0000-0000-000000000008");
        ctx.Lessons.Add(NewLesson(l1, m1, "Basic Greetings", SkillType.Listening, 1, A1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Numbers 1–20", SkillType.Reading, 2, A1L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Colors", SkillType.Reading, 1, A1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Classroom Objects", SkillType.Reading, 2, A1L4()));
        ctx.Lessons.Add(NewLesson(l5, m3, "Family Members", SkillType.Reading, 1, A1L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Pronouns & 'To Be'", SkillType.Reading, 2, A1L6()));
        ctx.Lessons.Add(NewLesson(l7, m4, "Days & Months", SkillType.Reading, 1, A1L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Food & Drinks", SkillType.Reading, 2, A1L8()));

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

        ctx.Modules.Add(NewModule(m1, levelId, "Module 1: am/is/are (Essential Grammar Unit 1)", "Master the verb 'to be' in present tense — the foundation of English.", 1, 3));
        ctx.Modules.Add(NewModule(m2, levelId, "Module 2: Present Continuous (Units 3-4)", "Learn current actions and ongoing processes with -ing forms.", 2, 3));
        ctx.Modules.Add(NewModule(m3, levelId, "Module 3: Present Simple (Units 5-6)", "Master regular routines and habitual actions.", 3, 3));
        ctx.Modules.Add(NewModule(m4, levelId, "Module 4: Have/Got & Past was/were (Units 9-10)", "Express possession and describe the past.", 4, 3));
        ctx.Modules.Add(NewModule(m5, levelId, "Module 5: Past Simple (Units 11-12)", "Tell stories about finished events with regular and irregular verbs.", 5, 3));
        ctx.Modules.Add(NewModule(m6, levelId, "Module 6: Modals: can/could (Units 29-30)", "Express ability, permission, and possibility.", 6, 3));
        ctx.Modules.Add(NewModule(m7, levelId, "Module 7: Modals: should/must (Units 31-32)", "Give advice and express obligation.", 7, 3));
        ctx.Modules.Add(NewModule(m8, levelId, "Module 8: Questions & Negatives (Units 44-48)", "Form questions and negative sentences correctly.", 8, 3));

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

        // Module 1: am/is/are
        ctx.Lessons.Add(NewLesson(l1, m1, "I am / You are / He/She is", SkillType.Reading, 1, A2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Negatives: I'm not / isn't / aren't", SkillType.Writing, 2, A2L2()));

        // Module 2: Present Continuous
        ctx.Lessons.Add(NewLesson(l3, m2, "What are you doing right now?", SkillType.Listening, 1, A2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Present Continuous: am/is/are + -ing", SkillType.Writing, 2, A2L4()));

        // Module 3: Present Simple
        ctx.Lessons.Add(NewLesson(l5, m3, "I work, you work, he/she works", SkillType.Reading, 1, A2L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Do you? / Does he? / Do they?", SkillType.Writing, 2, A2L6()));

        // Module 4: Have/Got & was/were
        ctx.Lessons.Add(NewLesson(l7, m4, "I have got / Have you got?", SkillType.Reading, 1, A2L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "I was / You were / He was", SkillType.Writing, 2, A2L8()));

        // Module 5: Past Simple
        ctx.Lessons.Add(NewLesson(l9, m5, "I worked / I went / I saw", SkillType.Reading, 1, A2L9()));
        ctx.Lessons.Add(NewLesson(l10, m5, "Did you? / What did you do?", SkillType.Writing, 2, A2L10()));

        // Module 6: can/could
        ctx.Lessons.Add(NewLesson(l11, m6, "I can... / You can't... / Can you?", SkillType.Reading, 1, A2L11()));
        ctx.Lessons.Add(NewLesson(l12, m6, "Could = past ability", SkillType.Writing, 2, A2L12()));

        // Module 7: should/must
        ctx.Lessons.Add(NewLesson(l13, m7, "You should / You must / You mustn't", SkillType.Reading, 1, A2L13()));
        ctx.Lessons.Add(NewLesson(l14, m7, "I shouldn't / Should I?", SkillType.Writing, 2, A2L14()));

        // Module 8: Questions & Negatives
        ctx.Lessons.Add(NewLesson(l15, m8, "Forming questions: word order", SkillType.Reading, 1, A2L15()));
        ctx.Lessons.Add(NewLesson(l16, m8, "Negative sentences: don't / doesn't / didn't", SkillType.Writing, 2, A2L16()));

        // Module 1: am/is/are
        AddExercises(ctx, l1, "a2e1",
            MC("I ___ a student.", "am", "are", "is", "be", "be-am-is-are"),
            MC("She ___ a teacher.", "is", "am", "are", "be", "be-am-is-are"),
            MC("You ___ happy.", "are", "am", "is", "be", "be-am-is-are"),
            FB("They ___ friends. (plural)", "are", "be-am-is-are"));

        AddExercises(ctx, l2, "a2e2",
            MC("I ___ not tired.", "am", "are", "is", "be", "negatives-be"),
            MC("She ___ not at home.", "is", "am", "are", "be", "negatives-be"),
            MC("You ___ not late.", "are", "am", "is", "be", "negatives-be"),
            FB("He ___ not a doctor. (negative)", "is", "negatives-be"));

        // Module 2: Present Continuous
        AddExercises(ctx, l3, "a2e3",
            MC("What ___ you ___? (doing right now)", "are-doing", "am doing", "is doing", "are do", "present-continuous"),
            MC("She ___ a book now.", "is reading", "are reading", "am reading", "reads", "present-continuous"),
            MC("They ___ football at the moment.", "are playing", "is playing", "am playing", "plays", "present-continuous"),
            FB("I ___ English. (study)", "am studying", "present-continuous"));

        AddExercises(ctx, l4, "a2e4",
            MC("He ___ not ___ing right now.", "is-work", "am working", "are working", "works", "present-continuous-negative"),
            MC("We ___ not sleeping.", "are", "am", "is", "be", "present-continuous-negative"),
            MC("She ___ eating lunch.", "is", "am", "are", "be", "present-continuous,negative"),
            FB("___ you watching TV? (question)", "Are", "present-continuous,questions"));

        // Module 3: Present Simple
        AddExercises(ctx, l5, "a2e5",
            MC("He ___ to school every day.", "goes", "go", "going", "went", "present-simple"),
            MC("I ___ coffee in the morning.", "drink", "drinks", "drinking", "drank", "present-simple"),
            MC("They ___ football on weekends.", "play", "plays", "playing", "played", "present-simple"),
            FB("She ___ in London. (live)", "lives", "present-simple"));

        AddExercises(ctx, l6, "a2e6",
            MC("___ he like pizza?", "Does", "Do", "Is", "Are", "present-simple,questions"),
            MC("I ___ not like spicy food.", "do", "does", "am", "have", "present-simple,negatives"),
            MC("She ___ not speak Italian.", "does", "do", "is", "has", "present-simple,negatives"),
            FB("___ you work here? (question)", "Do", "present-simple"));

        // Module 4: Have/Got & was/were
        AddExercises(ctx, l7, "a2e7",
            MC("I ___ a car.", "have got", "has got", "am got", "be got", "have-got"),
            MC("She ___ two brothers.", "has got", "have got", "am got", "are got", "have-got"),
            MC("___ you got a pen?", "Have", "Has", "Are", "Do", "have-got,questions"),
            FB("They ___ got three children. (present)", "have", "have-got"));

        AddExercises(ctx, l8, "a2e8",
            MC("I ___ at home yesterday.", "was", "were", "am", "be", "was-were"),
            MC("She ___ in the office.", "was", "were", "is", "be", "was-were"),
            MC("You ___ late for class.", "were", "was", "are", "be", "was-were"),
            FB("They ___ happy. (past)", "were", "was-were"));

        // Module 5: Past Simple
        AddExercises(ctx, l9, "a2e9",
            MC("Yesterday I ___ to the park.", "went", "go", "going", "goes", "past-simple,irregular"),
            MC("She ___ a beautiful dress.", "wore", "wear", "wearing", "wears", "past-simple,irregular"),
            MC("We ___ an excellent film.", "saw", "see", "seeing", "sees", "past-simple,irregular"),
            FB("He ___ his car. (lose)", "lost", "past-simple,regular"));

        AddExercises(ctx, l10, "a2e10",
            MC("___ she go to school yesterday?", "Did", "Does", "Do", "Will", "past-simple,questions"),
            MC("What ___ you do last weekend?", "did", "do", "does", "will", "past-simple,questions"),
            MC("___ they at home?", "Were", "Was", "Are", "Is", "past-simple,was-were"),
            FB("___ you enjoy the party? (question)", "Did", "past-simple"));

        // Module 6: can/could
        AddExercises(ctx, l11, "a2e11",
            MC("I ___ speak English.", "can", "can't", "could", "cans", "modal-can"),
            MC("She ___ swim very well.", "can", "cans", "cannot", "can't", "modal-can"),
            MC("___ you drive a car?", "Can", "Can't", "Could", "Cans", "modal-can,questions"),
            FB("They ___ not play tennis. (ability)", "can", "modal-can"));

        AddExercises(ctx, l12, "a2e12",
            MC("When I was young, I ___ climb trees easily.", "could", "can", "cans", "couldn't", "modal-could,ability"),
            MC("He ___ not swim when he was a child.", "could", "can", "cans", "could't", "modal-could"),
            MC("___ you ride a bike when you were five?", "Could", "Can", "Would", "Did", "modal-could,questions"),
            FB("She ___ speak French in her youth. (ability)", "could", "modal-could"));

        // Module 7: should/must
        AddExercises(ctx, l13, "a2e13",
            MC("You ___ eat vegetables.", "should", "must", "can", "could", "modal-should"),
            MC("She ___ see a doctor about her cough.", "should", "can", "could", "will", "modal-should"),
            MC("You ___ not stay up too late.", "should", "must", "can", "will", "modal-should,negatives"),
            FB("I ___ study harder for the exam. (advice)", "should", "modal-should"));

        AddExercises(ctx, l14, "a2e14",
            MC("You ___ wear a seatbelt in the car.", "must", "should", "can", "could", "modal-must"),
            MC("Students ___ not use their phones in class.", "must", "should", "can", "will", "modal-must,negatives"),
            MC("___ I wear formal clothes?", "Must", "Should", "Can", "Will", "modal-must,questions"),
            FB("You ___ arrive on time. (obligation)", "must", "modal-must"));

        // Module 8: Questions & Negatives
        AddExercises(ctx, l15, "a2e15",
            MC("___ does she work?", "Where", "What", "When", "How", "questions,word-order"),
            MC("___ old are you?", "How", "What", "Where", "When", "questions,word-order"),
            MC("What time ___ they arrive?", "do", "does", "are", "did", "questions,word-order"),
            FB("___ he like ice cream? (yes/no)", "Does", "questions"));

        AddExercises(ctx, l16, "a2e16",
            MC("I ___ not like coffee.", "don't", "doesn't", "didn't", "don't like", "negatives,present-simple"),
            MC("She ___ not come to the party.", "doesn't", "don't", "didn't", "isn't", "negatives,present-simple"),
            MC("They ___ not go to school yesterday.", "didn't", "don't", "doesn't", "aren't", "negatives,past-simple"),
            FB("We ___ not want to go. (negative)", "don't", "negatives"));
    }

    private static void BuildB1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("b1010000-0000-0000-0000-000000000001");
        var m2 = G("b1020000-0000-0000-0000-000000000002");
        var m3 = G("b1030000-0000-0000-0000-000000000003");
        var m4 = G("b1040000-0000-0000-0000-000000000004");
        ctx.Modules.Add(NewModule(m1, levelId, "Present Perfect", "Connect past experiences to the present.", 1, 4));
        ctx.Modules.Add(NewModule(m2, levelId, "Conditional Sentences", "Explore real and hypothetical situations.", 2, 4));
        ctx.Modules.Add(NewModule(m3, levelId, "Continuous & Perfect Tenses", "Master complex tenses for nuanced expression.", 3, 4));
        ctx.Modules.Add(NewModule(m4, levelId, "Advanced Grammar & Structures", "Relative clauses and reported speech for sophistication.", 4, 4));

        var l1 = G("b1b10000-0000-0000-0000-000000000001");
        var l2 = G("b1b20000-0000-0000-0000-000000000002");
        var l3 = G("b1b30000-0000-0000-0000-000000000003");
        var l4 = G("b1b40000-0000-0000-0000-000000000004");
        var l5 = G("b1b50000-0000-0000-0000-000000000005");
        var l6 = G("b1b60000-0000-0000-0000-000000000006");
        var l7 = G("b1b70000-0000-0000-0000-000000000007");
        var l8 = G("b1b80000-0000-0000-0000-000000000008");
        ctx.Lessons.Add(NewLesson(l1, m1, "Have/Has + Past Participle", SkillType.Writing, 1, B1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "For vs Since", SkillType.Writing, 2, B1L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "First Conditional", SkillType.Writing, 1, B1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Second Conditional", SkillType.Writing, 2, B1L4()));
        ctx.Lessons.Add(NewLesson(l5, m3, "Present Perfect Continuous", SkillType.Writing, 1, B1L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Past Perfect", SkillType.Writing, 2, B1L6()));
        ctx.Lessons.Add(NewLesson(l7, m4, "Relative Clauses", SkillType.Reading, 1, B1L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Reported Speech", SkillType.Writing, 2, B1L8()));

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

        AddExercises(ctx, l5, "b1e5",
            MC("How long ___ you ___ at this job?", "have-been working", "are working", "have worked", "are", "present-perfect-continuous"),
            MC("She ___ her book for three hours.", "has been writing", "has written", "writes", "is write", "present-perfect-continuous"),
            MC("They ___ football all afternoon.", "have been playing", "have played", "are playing", "plays", "present-perfect-continuous"),
            FB("I ___ English for 5 years. (study)", "have been studying", "present-perfect-continuous"));

        AddExercises(ctx, l6, "b1e6",
            MC("Before they arrived, I ___ the dishes.", "had finished", "finished", "have finished", "was finishing", "past-perfect"),
            MC("She ___ that book before.", "had never read", "has never read", "never read", "was reading", "past-perfect"),
            MC("By the time he called, we ___ already ___.", "had-left", "left", "have left", "was leaving", "past-perfect"),
            FB("When she arrived, he ___ for an hour. (wait)", "had been waiting", "past-perfect"));

        AddExercises(ctx, l7, "b1e7",
            MC("The woman ___ taught me is kind.", "who", "that", "which", "where", "relative-clauses,who"),
            MC("The book ___ I read was excellent.", "that", "which", "who", "where", "relative-clauses,that-which"),
            MC("The café ___ we met is closed.", "where", "who", "that", "when", "relative-clauses,where"),
            FB("I know someone ___ speaks five languages. (relative pronoun)", "who", "relative-clauses"));

        AddExercises(ctx, l8, "b1e8",
            MC("She said she ___ tired.", "was", "is", "were", "am", "reported-speech,tense-shift"),
            MC("He asked if I ___ to the party.", "was going", "am going", "go", "will go", "reported-speech,questions"),
            MC("They told me they ___ finished.", "had", "have", "has", "will have", "reported-speech,tense-shift"),
            FB("She asked what time it ___. (be)", "was", "reported-speech,questions"));
    }

    private static void BuildB2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("b2010000-0000-0000-0000-000000000001");
        var m2 = G("b2020000-0000-0000-0000-000000000002");
        var m3 = G("b2030000-0000-0000-0000-000000000003");
        var m4 = G("b2040000-0000-0000-0000-000000000004");
        ctx.Modules.Add(NewModule(m1, levelId, "Passive Voice", "Shift focus from agent to action.", 1, 5));
        ctx.Modules.Add(NewModule(m2, levelId, "Reported Speech", "Report what others said accurately.", 2, 5));
        ctx.Modules.Add(NewModule(m3, levelId, "Unreal Situations & Emphasis", "Express wishes and add emphasis with cleft sentences.", 3, 5));
        ctx.Modules.Add(NewModule(m4, levelId, "Advanced Structures & Vocabulary", "Phrasal verbs, quantifiers, and precise expression.", 4, 5));

        var l1 = G("b2b10000-0000-0000-0000-000000000001");
        var l2 = G("b2b20000-0000-0000-0000-000000000002");
        var l3 = G("b2b30000-0000-0000-0000-000000000003");
        var l4 = G("b2b40000-0000-0000-0000-000000000004");
        var l5 = G("b2b50000-0000-0000-0000-000000000005");
        var l6 = G("b2b60000-0000-0000-0000-000000000006");
        var l7 = G("b2b70000-0000-0000-0000-000000000007");
        var l8 = G("b2b80000-0000-0000-0000-000000000008");
        ctx.Lessons.Add(NewLesson(l1, m1, "Present & Past Passive", SkillType.Writing, 1, B2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Passive with Modals", SkillType.Writing, 2, B2L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Reporting Statements", SkillType.Writing, 1, B2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Reporting Questions", SkillType.Writing, 2, B2L4()));
        ctx.Lessons.Add(NewLesson(l5, m3, "Wish & If Only", SkillType.Writing, 1, B2L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Cleft Sentences", SkillType.Writing, 2, B2L6()));
        ctx.Lessons.Add(NewLesson(l7, m4, "Advanced Phrasal Verbs", SkillType.Reading, 1, B2L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Quantifiers & Articles", SkillType.Reading, 2, B2L8()));

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

        AddExercises(ctx, l5, "b2e5",
            MC("I ___ I lived in a sunny country.", "wish", "wishes", "would wish", "wished", "wish,unreal"),
            MC("If only I ___ studied harder when I had the chance.", "had", "have", "would have", "had had", "wish,past"),
            MC("She wishes he ___ spend more time with the family.", "would", "will", "can", "could", "wish,future"),
            FB("I ___ I could turn back time. (wish, present)", "wish", "wish,unreal"));

        AddExercises(ctx, l6, "b2e6",
            MC("___ John who won the competition, not Michael.", "It was", "It is", "It were", "It had", "cleft,emphasis"),
            MC("___ impressed me was his dedication and hard work.", "What", "That", "Which", "Who", "cleft,what"),
            MC("It ___ last summer that we met for the first time.", "was", "is", "were", "has been", "cleft,time"),
            FB("It ___ her honesty that I love most about her. (cleft)", "is", "cleft"));

        AddExercises(ctx, l7, "b2e7",
            MC("I can't ___ with his constant negativity.", "put up", "put on", "put off", "put out", "phrasal-verbs"),
            MC("She ___ a new strategy to improve sales.", "came up with", "came across", "came into", "came over", "phrasal-verbs"),
            MC("We need to ___ unnecessary expenses.", "cut down on", "cut off", "cut out", "cut through", "phrasal-verbs"),
            FB("I ___ an old friend at the supermarket yesterday. (phrasal verb)", "ran into", "phrasal-verbs"));

        AddExercises(ctx, l8, "b2e8",
            MC("There are ___ people who would agree with that opinion.", "many", "much", "a great deal of", "a little", "quantifiers"),
            MC("She has ___ experience in this field.", "much", "many", "several", "a few", "quantifiers"),
            MC("___ student in the class participated in the discussion.", "Each", "Every", "All", "Some", "quantifiers,articles"),
            FB("I don't want ___ sugar in my tea. (quantifier)", "any", "quantifiers"));
    }

    private static void BuildC1Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("c1010000-0000-0000-0000-000000000001");
        var m2 = G("c1020000-0000-0000-0000-000000000002");
        var m3 = G("c1030000-0000-0000-0000-000000000003");
        var m4 = G("c1040000-0000-0000-0000-000000000004");
        ctx.Modules.Add(NewModule(m1, levelId, "Advanced Grammar", "Master inversion, emphasis, and mixed conditionals.", 1, 6));
        ctx.Modules.Add(NewModule(m2, levelId, "Advanced Vocabulary", "Expand academic and idiomatic language.", 2, 6));
        ctx.Modules.Add(NewModule(m3, levelId, "Writing Sophistication", "Nominalization, discourse markers, and cohesion for advanced writing.", 3, 6));
        ctx.Modules.Add(NewModule(m4, levelId, "Advanced Structures", "Master ellipsis, subjunctive mood, and modal perfection.", 4, 6));

        var l1 = G("c1b10000-0000-0000-0000-000000000001");
        var l2 = G("c1b20000-0000-0000-0000-000000000002");
        var l3 = G("c1b30000-0000-0000-0000-000000000003");
        var l4 = G("c1b40000-0000-0000-0000-000000000004");
        var l5 = G("c1b50000-0000-0000-0000-000000000005");
        var l6 = G("c1b60000-0000-0000-0000-000000000006");
        var l7 = G("c1b70000-0000-0000-0000-000000000007");
        var l8 = G("c1b80000-0000-0000-0000-000000000008");
        ctx.Lessons.Add(NewLesson(l1, m1, "Inversion for Emphasis", SkillType.Writing, 1, C1L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Mixed Conditionals", SkillType.Writing, 2, C1L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Academic & Formal Language", SkillType.Reading, 1, C1L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Collocations & Idioms", SkillType.Reading, 2, C1L4()));
        ctx.Lessons.Add(NewLesson(l5, m3, "Nominalization", SkillType.Writing, 1, C1L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Discourse Markers & Cohesion", SkillType.Writing, 2, C1L6()));
        ctx.Lessons.Add(NewLesson(l7, m4, "Ellipsis & Substitution", SkillType.Reading, 1, C1L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Subjunctive & Modal Perfection", SkillType.Writing, 2, C1L8()));

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

        AddExercises(ctx, l5, "c1e5",
            MC("'Analyze' becomes a nominalization:", "analysis", "analyzing", "analyzed", "analytical", "nominalization,vocabulary"),
            MC("Which is a nominalization of 'develop'?", "development", "developing", "developed", "developer", "nominalization"),
            MC("The sentence 'His judgment was sound' uses nominalization from:", "judge", "judging", "judgement", "judiciary", "nominalization"),
            FB("The ___ of the project took longer than expected. (implement, nominalization)", "implementation", "nominalization"));

        AddExercises(ctx, l6, "c1e6",
            MC("Which discourse marker shows contrast?", "nevertheless", "furthermore", "consequently", "moreover", "discourse-markers"),
            MC("'Due to the delay, sales declined.' Which marker could replace 'due to'?", "Consequently", "Furthermore", "Conversely", "Additionally", "discourse-markers"),
            MC("The correct discourse marker for cause-effect is:", "consequently", "conversely", "concurrently", "conjointly", "discourse-markers"),
            FB("The evidence is unclear. ___,  the pattern is discernible. (discourse marker for contrast)", "Nevertheless", "discourse-markers"));

        AddExercises(ctx, l7, "c1e7",
            MC("'Which office is he in? The one John is in.' This demonstrates:", "ellipsis", "substitution", "repetition", "emphasis", "ellipsis"),
            MC("'Do you like coffee?' 'Yes, I do.' The 'do' is:", "substitution", "ellipsis", "repetition", "auxiliary", "substitution"),
            MC("'He passed; she did not' uses:", "substitution", "ellipsis", "inversion", "nominalization", "substitution"),
            FB("John went to London; Sarah to Paris. (___of 'went')", "ellipsis", "ellipsis-substitution"));

        AddExercises(ctx, l8, "c1e8",
            MC("'I insist that she be present.' This uses:", "subjunctive", "conditional", "subjunctive mood", "imperative", "subjunctive,advanced-grammar"),
            MC("Which sentence is correct subjunctive?", "It is vital that he arrive on time.", "It is vital that he arrives on time.", "It is vital that he will arrive.", "It is vital that he arrived.", "subjunctive"),
            MC("'She could have succeeded if she'd tried.' This perfect modal expresses:", "regret", "permission", "ability", "obligation", "perfect-modals"),
            FB("I suggest that the policy ___ reviewed. (be, subjunctive)", "be", "subjunctive"));
    }

    private static void BuildC2Modules(ApplicationDbContext ctx, Guid levelId)
    {
        var m1 = G("c2010000-0000-0000-0000-000000000001");
        var m2 = G("c2020000-0000-0000-0000-000000000002");
        var m3 = G("c2030000-0000-0000-0000-000000000003");
        var m4 = G("c2040000-0000-0000-0000-000000000004");
        var m5 = G("c2050000-0000-0000-0000-000000000005");
        var m6 = G("c2060000-0000-0000-0000-000000000006");
        ctx.Modules.Add(NewModule(m1, levelId, "Nuanced Expression", "Achieve near-native register and style.", 1, 8));
        ctx.Modules.Add(NewModule(m2, levelId, "Academic Writing", "Master hedging, argumentation, and critical analysis.", 2, 8));
        ctx.Modules.Add(NewModule(m3, levelId, "Advanced Communication", "Paraphrasing, pragmatics, and context mastery.", 3, 8));
        ctx.Modules.Add(NewModule(m4, levelId, "Native Proficiency", "Stylistic mastery and authentic native features.", 4, 8));
        ctx.Modules.Add(NewModule(m5, levelId, "Meaning & Context", "Master subtle nuances and discourse analysis.", 5, 8));
        ctx.Modules.Add(NewModule(m6, levelId, "Culture & Expression", "Navigate allusions, ambiguity, and sophistication.", 6, 8));

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
        ctx.Lessons.Add(NewLesson(l1, m1, "Register & Formal Style", SkillType.Writing, 1, C2L1()));
        ctx.Lessons.Add(NewLesson(l2, m1, "Idiomatic Mastery", SkillType.Reading, 2, C2L2()));
        ctx.Lessons.Add(NewLesson(l3, m2, "Hedging Language", SkillType.Writing, 1, C2L3()));
        ctx.Lessons.Add(NewLesson(l4, m2, "Critical Analysis", SkillType.Reading, 2, C2L4()));
        ctx.Lessons.Add(NewLesson(l5, m3, "Paraphrasing & Summarization", SkillType.Writing, 1, C2L5()));
        ctx.Lessons.Add(NewLesson(l6, m3, "Pragmatics & Implicature", SkillType.Reading, 2, C2L6()));
        ctx.Lessons.Add(NewLesson(l7, m4, "Stylistic Variation", SkillType.Writing, 1, C2L7()));
        ctx.Lessons.Add(NewLesson(l8, m4, "Native Speaker Features", SkillType.Reading, 2, C2L8()));
        ctx.Lessons.Add(NewLesson(l9, m5, "Subtle Nuance & Precision", SkillType.Writing, 1, C2L9()));
        ctx.Lessons.Add(NewLesson(l10, m5, "Discourse Analysis", SkillType.Reading, 2, C2L10()));
        ctx.Lessons.Add(NewLesson(l11, m6, "Cultural References & Allusions", SkillType.Reading, 1, C2L11()));
        ctx.Lessons.Add(NewLesson(l12, m6, "Mastery of Ambiguity", SkillType.Writing, 2, C2L12()));

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

        AddExercises(ctx, l5, "c2e5",
            MC("Paraphrasing means:", "restating using different words and structure", "simply changing a few words", "condensing to key points", "translating to another language", "paraphrasing,writing-skills"),
            MC("Which is the best summary of a long article?", "The essential findings in 1-2 sentences", "Every detail in new words", "A shorter version with all details", "The author's opinion restated", "summarization"),
            MC("'To recapitulate' means:", "to summarize the key points", "to elaborate further", "to provide more examples", "to criticize the argument", "vocabulary,summarization"),
            FB("A ___ of the chapter provides a brief overview. (summary)", "synopsis", "summarization"));

        AddExercises(ctx, l6, "c2e6",
            MC("An implicature is:", "an implied meaning beyond literal words", "a grammatical mistake", "a formal speech pattern", "a written statement", "pragmatics,vocabulary"),
            MC("'It's cold in here' might imply:", "Close the window/Please leave", "The temperature is low", "It is winter", "I like the cold", "pragmatics,implicature"),
            MC("A presupposition in 'Stop eating cake' is:", "You are eating cake", "I want you to stop", "Cake is delicious", "You should eat less", "pragmatics"),
            FB("The ___ of his comment is that she is not performing well. (implied meaning)", "implicature", "pragmatics"));

        AddExercises(ctx, l7, "c2e7",
            MC("Which sentence demonstrates stylistic variation (informal)?", "Hey, your idea rocks!", "The proposed concept presents merit.", "It is suggested that...", "The methodology is rigorous.", "style,register"),
            MC("'Eloquent' speech is:", "expressive and persuasive", "unclear and confusing", "brief and curt", "technical and jargon-heavy", "vocabulary,style"),
            MC("Which rhetorical device is demonstrated: 'He was cold, calculating, and cruel'?", "Alliteration", "Metaphor", "Irony", "Paradox", "literary-devices"),
            FB("The baroque prose was difficult to ___. (understand/interpret)", "parse", "style,vocabulary"));

        AddExercises(ctx, l8, "c2e8",
            MC("Elision in English is:", "omitting sounds in informal speech", "adding extra syllables", "changing word order", "inventing new words", "native-features,pronunciation"),
            MC("'Gonna' is an example of:", "elision (gonna = going to)", "assimilation", "hesitation", "code-switching", "native-features"),
            MC("Prosody refers to:", "intonation, stress, and rhythm in speech", "grammatical structure", "vocabulary choice", "formal register", "pronunciation,native-features"),
            FB("'___ he was totally confused, you know?' — Native discourse pattern", "Like", "native-features,discourse"));

        AddExercises(ctx, l9, "c2e9",
            MC("'Frugal' vs 'stingy' — the difference is:", "connotation (both mean money-conscious but differ in approval)", "synonyms with identical meaning", "formal vs informal register only", "American vs British spelling", "nuance,vocabulary"),
            MC("Which near-synonym carries the most negative connotation?", "stingy", "thrifty", "economical", "frugal", "connotation,vocabulary"),
            MC("'Error' differs from 'mistake' in that:", "error is more formal; mistake is more general", "error is intentional, mistake is not", "error is only written; mistake is only spoken", "they have no difference", "nuance,vocabulary"),
            FB("The ___ between 'angry' and 'furious' is that furious expresses more intense emotion. (subtle difference)", "nuance", "nuance,vocabulary"));

        AddExercises(ctx, l10, "c2e10",
            MC("Thematic progression in discourse refers to:", "how given and new information are organized to guide readers", "the main theme of a text", "repetition of the same word", "transition words between sentences", "discourse-analysis"),
            MC("Which discourse marker signals cause-effect?", "therefore", "however", "moreover", "conversely", "discourse-markers,cohesion"),
            MC("'Problem-solution' is an example of:", "rhetorical structure that organizes extended text", "a grammar rule", "a vocabulary strategy", "a pronunciation pattern", "discourse-analysis"),
            FB("___ helps readers follow arguments by linking sentences and ideas. (Text connection)", "Cohesion", "discourse-analysis"));

        AddExercises(ctx, l11, "c2e11",
            MC("'Orwellian' refers to:", "dystopian surveillance (from George Orwell's 1984)", "British English", "agricultural societies", "romantic poetry", "allusions,cultural-references"),
            MC("'Kafkaesque' describes situations that are:", "absurdly bureaucratic and nightmarish", "romantic and whimsical", "simple and straightforward", "humorous and entertaining", "allusions,cultural-references"),
            MC("Understanding allusions requires:", "cultural and literary knowledge", "only grammar skills", "only vocabulary", "memorizing all possible references", "cultural-references"),
            FB("The phrase '____ moment' alludes to a doomed, inevitable outcome. (Titanic)", "Titanic", "allusions,cultural-references"));

        AddExercises(ctx, l12, "c2e12",
            MC("Lexical ambiguity occurs when:", "a word has multiple meanings (e.g., 'bank' = financial or river)", "a sentence structure is unclear", "an author is being sarcastic", "grammar is incorrect", "ambiguity,vocabulary"),
            MC("'I saw the man with the telescope' demonstrates:", "syntactic ambiguity (unclear who has the telescope)", "lexical ambiguity", "phonetic confusion", "misused punctuation", "ambiguity"),
            MC("'That's interesting' might mean:", "genuine interest or subtle criticism (pragmatic ambiguity)", "always genuine praise", "always dismissal", "simple, single meaning", "ambiguity,pragmatics"),
            FB("A ___ is a play on words using double meaning for humorous effect. (wordplay, single word)", "pun", "ambiguity,wordplay"));
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
        "In the Present Simple tense, verbs change their form ONLY for the third person singular (he, she, it). This is one of the most important rules in English grammar, covered extensively by Perfect English Grammar.",
        ["I/you/we/they + base verb: work, play, live", "He/she/it + verb + S: works, plays, lives", "Add -ES after -ch, -sh, -x, -o, -ss: watches, goes, fixes", "Consonant + Y → drop Y and add -IES: study → studies, carry → carries", "Irregular: have → has, be → is, do → does"],
        [("works", "trabaja", "She works at a hospital."), ("studies", "estudia", "He studies every evening."), ("watches", "mira/ve", "She watches TV after dinner."), ("goes", "va", "He goes to the gym on Mondays."), ("has", "tiene", "She has two children.")],
        [("She plays tennis every Saturday.", "Regular verb with -s"), ("He watches TV after dinner.", "Verb ending in -ch adds -es"), ("My sister studies medicine at university.", "Consonant+y → -ies"), ("The train arrives at platform 3.", "Third person in context")],
        "Write 6 sentences about what a family member or friend does in their daily routine. Use he/she with correct third person singular forms.",
        "The final -s/-es has 3 sounds: /s/ (works, eats), /z/ (plays, lives), /ɪz/ (watches, teaches — after -sh, -ch, -x, -s, -z).",
        [("Present Simple - Third Person Singular -s/-es", "Learn English with Papa English", "m3FfULhEcvE"), ("Present Simple Tense", "English Speeches", "v8_3WvX4Q6k")]
    );

    private static string A2L2() => C(
        "To make negatives and questions in Present Simple, we need a helper verb: DO for I/you/we/they and DOES for he/she/it. The main verb ALWAYS stays in its base form after don't/doesn't/do/does. This is fundamental for daily English conversation.",
        ["Negative: Subject + don't/doesn't + base verb", "He/she/it uses DOESN'T — not 'don't'", "Question: Do/Does + subject + base verb?", "After doesn't/does: main verb has NO -s (she doesn't work, does she work?)", "Short answers: Yes, she does. / No, she doesn't."],
        [("don't (do not)", "no + verbo", "I don't like coffee."), ("doesn't (does not)", "no + verbo (él/ella)", "She doesn't eat meat."), ("Do you...?", "¿Tú...?", "Do you speak Spanish?"), ("Does he/she...?", "¿Él/ella...?", "Does she work here?")],
        [("I don't like spicy food.", "Negative — I/you/we/they"), ("She doesn't speak French.", "Negative — he/she/it"), ("Do you have a car?", "Yes/No question"), ("Does he live nearby? — Yes, he does.", "Question with short answer")],
        "Write 3 negative sentences and 3 questions about your daily routine or lifestyle. Include at least one he/she example.",
        "'Doesn't' is pronounced DUZ-ent. In spoken English, contractions (don't, doesn't) are far more common than the full forms (do not, does not).",
        [("Present Simple Questions and Negatives", "Papa English", null), ("Do Does Present Simple", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L3() => C(
        "Regular verbs in the Past Simple are formed by adding -ED to the base form. This form is the SAME for ALL subjects — I, you, he, she, we, they. Only ONE form to learn! Perfect English Grammar emphasizes this simple, consistent rule.",
        ["Base verb + ED: walk → walked, play → played, watch → watched", "Verb ending in -E: add only -D: live → lived, like → liked", "Short vowel + single consonant → double it: stop → stopped, plan → planned", "Consonant + Y → change to -IED: study → studied, try → tried", "Negative: didn't + base verb | Question: Did + subject + base verb?"],
        [("walked", "caminó/caminé", "I walked to school."), ("studied", "estudió/estudié", "She studied all night."), ("stopped", "paró/paré", "The car stopped suddenly."), ("lived", "vivió/viví", "They lived in Paris for a year.")],
        [("I walked to the park yesterday.", "Regular past — base + ed"), ("She studied for the exam last night.", "Y → ied"), ("They stopped at a café for coffee.", "Double consonant"), ("He didn't call me. / Did you finish?", "Negative and question forms")],
        "Write a short paragraph (5–7 sentences) about what you did yesterday or last weekend. Use at least 5 different regular past verbs.",
        "-ED has 3 sounds: /t/ after voiceless consonants (worked, stopped), /d/ after voiced sounds (played, called), /ɪd/ after t/d (wanted, needed).",
        [("Past Simple Regular Verbs", "Papa English", null), ("Regular Past Tense", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L4() => C(
        "Many of the most common English verbs are irregular — they do NOT follow the -ED rule. Their past forms must be memorized. Negatives and questions still use 'did' + base verb. These are essential for fluent English.",
        ["Common: go→went, have→had, see→saw, come→came, give→gave", "More: eat→ate, drink→drank, write→wrote, read→read, say→said", "More: take→took, make→made, know→knew, think→thought, buy→bought", "Negatives: I didn't go, she didn't have (base verb!)", "Questions: Did you see? Did he come? (base verb!)"],
        [("went (go)", "fue/fui", "I went to the cinema last night."), ("had (have)", "tuvo/tuve", "She had a great time."), ("saw (see)", "vio/vi", "We saw a great film."), ("ate (eat)", "comió/comí", "They ate pizza for dinner."), ("wrote (write)", "escribió/escribí", "He wrote a long email.")],
        [("I went to the beach last summer.", "go → went"), ("She had coffee and toast for breakfast.", "have → had"), ("They saw the new film yesterday.", "see → saw"), ("He wrote her a letter but she didn't reply.", "write → wrote + negative")],
        "Write a paragraph about a memorable day or event. Use at least 6 different irregular past verbs. You can write about a birthday, trip, special meal, or a memorable moment.",
        "'Read' looks the same in present and past but sounds different: REED (present) vs RED (past). 'Thought' is pronounced THAWT — the 'ough' is silent.",
        [("Irregular Past Tense Verbs", "Papa English", null), ("Irregular Verbs in Past Simple", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L5() => C(
        "Adjectives describe nouns and make your English more colorful and descriptive. In English, adjectives ALWAYS go BEFORE the noun (opposite to Spanish). Word order matters: if you have multiple adjectives, follow a specific order (size, color, material). The British Council emphasizes the importance of adjective placement.",
        ["Adjectives come BEFORE the noun: a beautiful house (not 'a house beautiful')", "Common adjectives: big/small, good/bad, beautiful, ugly, happy, sad, cold, hot", "Adjective order: opinion/size/color/material — a beautiful red car, a small blue house", "Adverbs of extent before adjectives: very, quite, really — 'very happy', 'quite difficult'", "Comparative adjectives: -ER or MORE: big→bigger, beautiful→more beautiful"],
        [("beautiful", "hermoso/a", "She has a beautiful garden."), ("big", "grande", "That is a big house."), ("small", "pequeño/a", "We live in a small apartment."), ("happy", "feliz", "The children are very happy."), ("cold", "frío", "The water is very cold."), ("interesting", "interesante", "This is a really interesting book.")],
        [("She has a big yellow house.", "Adjective + color"), ("He is tall and handsome.", "Multiple adjectives"), ("The food is delicious and hot.", "Describing with multiple adjectives"), ("It was a beautiful sunny day.", "Positive adjectives in context")],
        "Describe a room in your house or a person you know using at least 5 adjectives. Write 6 sentences following the correct adjective order.",
        "'Beautiful' is pronounced BYOO-ti-ful (3 syllables). 'Interesting' — IN-trus-ting or IN-ter-esting (3 syllables). Stress falls on the first syllable.",
        [("English Adjectives", "Learn English with Papa English", null), ("Word Order with Adjectives", "English Speeches", null), ("Describing People and Things", "Cambridge English", null)]
    );

    private static string A2L6() => C(
        "Comparative adjectives (-ER or MORE) compare two things, while superlative adjectives (-EST or MOST) identify the extreme in a group. These form the foundation for making descriptions more precise and comparing options. The British Council highlights their frequency in everyday English.",
        ["Comparatives: -ER (big→bigger) or MORE (beautiful→more beautiful) + THAN", "Superlatives: -EST (big→biggest) or MOST (beautiful→most beautiful)", "One syllable: big→bigger→biggest, fast→faster→fastest", "Two syllables (varies): happy→happier→happiest, modern→more modern→most modern", "Irregular: good→better→best, bad→worse→worst, far→further/farther→furthest"],
        [("bigger", "más grande", "Your house is bigger than mine."), ("happier", "más feliz", "She is happier now."), ("best", "mejor/lo mejor", "This is the best film I've ever seen."), ("worse", "peor", "The weather is worse today."), ("fastest", "más rápido", "He is the fastest runner in the team.")],
        [("My car is bigger than yours.", "Comparing two things"), ("January is colder than December in my country.", "Comparative in context"), ("This is the most expensive restaurant in the city.", "Superlative"), ("She is the best teacher at the school.", "Superlative with 'the'")],
        "Write 6 sentences comparing two people, places, or things you know. Use at least 3 comparatives and 3 superlatives.",
        "Comparative '-ER' sounds like 'ur' at the end: bigger is BIG-ur. Superlative '-EST' is pronounced as a separate syllable: biggest is BIG-ist. Stress patterns change with suffixes.",
        [("Comparative and Superlative Adjectives", "Papa English", null), ("English Comparatives and Superlatives", "Learn English Kids", null), ("Comparing Adjectives in English", "Cambridge English", null)]
    );

    private static string A2L7() => C(
        "Possessive adjectives (my, your, his, her, its, our, their) show ownership or relationship. They are different from possessive pronouns (mine, yours, his, hers, ours, theirs). These words are essential for talking about people, objects, and relationships in everyday conversation.",
        ["Possessive adjectives: my, your, his, her, its, our, their — always come BEFORE the noun", "My book, your car, his phone, her bag, our house, their children", "Possessive pronouns: mine, yours, his, hers, ours, theirs — stand ALONE", "My book vs. mine: 'This is my book' vs. 'This book is mine'", "Its (possessive) vs. it's (it is): 'The cat lost its toy' vs. 'It's very cold'"],
        [("my", "mi", "This is my phone."), ("your", "tu/su", "What is your name?"), ("his", "su (de él)", "His car is red."), ("her", "su (de ella)", "Her sister is a doctor."), ("our", "nuestro/a", "This is our house."), ("their", "su (de ellos)", "Their children are very clever.")],
        [("My mother is a teacher and her sister is a nurse.", "Multiple possessives"), ("Is this your bag or mine?", "Possessive adjective vs. pronoun"), ("Their house is bigger than ours.", "Possessive pronouns in comparison"), ("The bird lost its feathers.", "Its = possessive (not it's)")],
        "Write 8 sentences about your family and possessions using possessive adjectives and pronouns. Include at least 3 different possessive forms.",
        "'Its' (possessive) and 'it's' (it is) sound identical — THIS is the most common mistake in English. Remember: 'its' = 'of it' (possessive), 'it's' = 'it is'.",
        [("Possessive Adjectives in English", "Learn English with Papa English", null), ("Possessive Pronouns and Adjectives", "English Speeches", null), ("My, Mine, Your, Yours - Possessive Words", "Cambridge English", null)]
    );

    private static string A2L8() => C(
        "The Present Continuous describes actions happening RIGHT NOW (I am working) or temporary situations (He is living in London this year). It is formed with: am/is/are + verb-ING. This tense is crucial for talking about what you see around you and temporary situations.",
        ["Form: Subject + am/is/are + verb-ING", "I am working, you are reading, he is sleeping, we are playing", "NOW actions: 'She is talking on the phone.'", "Temporary situations: 'He is staying with us this week.'", "Double final consonant: stop→stopping, run→running, but not: open→opening, visit→visiting"],
        [("am working", "estoy trabajando", "I am working on a project."), ("is sleeping", "está durmiendo", "The baby is sleeping right now."), ("are playing", "están jugando", "The children are playing in the park."), ("is living", "está viviendo", "She is living in Barcelona this year."), ("am studying", "estoy estudiando", "I am studying English.")],
        [("Look! The birds are flying.", "Right now action"), ("She is reading a book at the moment.", "Current action"), ("They are building a new house in our street.", "Temporary situation"), ("I am learning English. — How long? — For two years.", "Duration of current action")],
        "Write 8 sentences: 4 about things happening right now in your location, and 4 about temporary situations in your life.",
        "'Present Continuous' is pronounced: PREZ-ent CON-tin-you-us. The ING ending is one syllable: 'running' is RUN-ning (2 syllables, not 3).",
        [("Present Continuous Tense", "Papa English", null), ("English Present Continuous", "Learn English with EnglishClass101.com", null), ("BE + ING Form English Grammar", "English Speeches", null)]
    );

    private static string A2L9() => C(
        "Past Simple describes completed actions in the past. Regular verbs add -ED (worked, played), while irregular verbs have unique forms (went, saw, ate). Use it for finished events, life stories, and historical facts.",
        ["Regular: base + -ED (work→worked, play→played, walk→walked)", "Irregular: unique forms (go→went, see→saw, eat→ate, have→had, be→was/were)", "Time markers: yesterday, last week, in 1990, ago, when...", "All subjects use the same form: I went, he went, they went", "Negatives: did not (didn't) + base verb"],
        [("went", "fui", "I went to the park yesterday."), ("saw", "vi", "She saw him last week."), ("ate", "comí", "They ate pizza for dinner."), ("worked", "trabajé", "He worked here for 5 years."), ("had", "tuve", "We had a great time.")],
        [("I went to London last summer.", "Simple past action"), ("She saw an interesting film.", "Completed action"), ("They worked hard yesterday.", "Regular past verb"), ("We had dinner at 7pm.", "Irregular past verb")],
        "Write 8 sentences about your last holiday or important past event using past simple.",
        "Irregular verbs are common: go/went, see/saw, eat/ate, get/got, have/had. Memorizing 20 common irregular verbs helps enormously.",
        [("Past Simple Tense", "Papa English", null), ("English Past Simple", "Learn English with EnglishClass101.com", null), ("Past Simple Grammar", "English Speeches", null)]
    );

    private static string A2L10() => C(
        "Form questions in past simple with: Did + subject + base verb? 'Did you go?' For 'be', use: Was/Were + subject? 'Were they at home?'",
        ["Yes/No questions: Did you...? / Were you...?", "WH- questions: What did you do? Where were they? When did she arrive?", "Wh-words: What, Where, When, Why, Who, How, Which", "Short answers: Yes, I did. / No, I didn't. | Yes, he was. / No, she wasn't.", "Subject + auxiliary: 'What did she say?' NOT 'What she did say?'"],
        [("Did you go?", "¿Fuiste?", "Did you go to the party?"), ("Where were you?", "¿Dónde estabas?", "Where were you yesterday?"), ("What did she do?", "¿Qué hizo ella?", "What did she do last weekend?"), ("When did they arrive?", "¿Cuándo llegaron?", "When did they arrive?"), ("Was it good?", "¿Fue bueno?", "Was it good?")],
        [("Did you enjoy the film? — Yes, I did.", "Yes/no question"), ("What did he say? — He said hello.", "Wh- question"), ("Were they happy? — No, they weren't.", "Was/were question"), ("Who did you meet? — I met my friend.", "Who question")],
        "Write 10 past simple questions: 5 yes/no questions and 5 wh- questions.",
        "In questions, the auxiliary (did/was/were) comes BEFORE the subject. Wrong: 'What you did?' Correct: 'What did you do?'",
        [("Asking Questions in Past Simple", "Learn English with EnglishClass101.com", null), ("Past Simple Questions", "English Speeches", null)]
    );

    private static string A2L11() => C(
        "CAN expresses ability: 'I can swim.' Negatives: 'I can't (cannot) swim.' Questions: 'Can you swim?' Use CAN for present ability; COULD for past ability.",
        ["Present: can (I can) / can't (I cannot) / can he?", "Past: could (I could) / couldn't (I could not) / could he?", "Meaning: ability, permission, possibility", "No -s: 'He CAN swim' — NOT 'He cans swim'", "Informal: 'Can I...?' for permission. Formal: 'May I...?'"],
        [("can", "poder (ability)", "I can play guitar."), ("can't", "no poder", "She can't drive yet."), ("could", "pude/podía (past)", "He could swim when he was young."), ("couldn't", "no pude", "I couldn't open the door."), ("Can you?", "¿Puedes?", "Can you help me?")],
        [("I can cook Italian food.", "Present ability"), ("She can't speak German.", "Negative ability"), ("Could you swim as a child?", "Past ability"), ("I couldn't find my keys.", "Negative past ability")],
        "Write 6 sentences: 3 about present abilities and 3 about past abilities using can/could.",
        "CAN is pronounced with a short stress: CAN (strong form) vs. can (weak form in 'I can go' = 'I kən go').",
        [("Modal Verb: CAN", "Papa English", null), ("Can and Could English", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L12() => C(
        "COULD for past ability: 'He could run fast when he was young.' Form: could + base verb. Negatives: couldn't. Questions: Could you...?",
        ["COULD = past ability or past permission", "Regular form for all subjects: I could, you could, he could", "Negatives: couldn't (could not)", "Questions: Could you play football?", "Note: 'could' also means possibility in present (might): 'It could rain tomorrow'"],
        [("could", "podía/pude", "I could dance well as a child."), ("couldn't", "no podía", "She couldn't speak English before."), ("Could you?", "¿Podías?", "Could you ride a bike?"), ("might", "podría (possibility)", "It could rain tomorrow."), ("was able to", "pude lograr", "He was able to finish the race.")],
        [("When I was younger, I could climb trees.", "Past ability"), ("She couldn't cook when she was 15.", "Negative past ability"), ("Could he swim? — Yes, he could.", "Question and short answer"), ("I couldn't understand the lecture.", "Negative past ability")],
        "Write about 5 things you COULD do as a child but can't do now, and 5 things you COULDN'T do but CAN do now.",
        "COULD is the past form of CAN. When talking about past events, always use COULD for ability: 'When I was 5, I could...' NOT 'I can...'",
        [("Could - Past Tense of Can", "Learn English with EnglishClass101.com", null), ("Modals of Ability", "English Speeches", null)]
    );

    private static string A2L13() => C(
        "SHOULD gives advice: 'You should see a doctor.' Negatives: 'You shouldn't smoke.' Questions: 'Should I...?' Use SHOULD for recommendations and good ideas.",
        ["Form: should + base verb", "All subjects same form: I should, you should, he should", "Meaning: advice, good ideas, recommendations", "Negatives: shouldn't (should not)", "Common: 'You should...', 'I think you should...', 'They shouldn't...'"],
        [("should", "debería", "You should drink more water."), ("shouldn't", "no debería", "She shouldn't work so hard."), ("should I?", "¿Debería?", "Should I call him?"), ("advice", "consejo", "Can you give me some advice?"), ("recommend", "recomendar", "I recommend you see that film.")],
        [("You should eat more vegetables.", "Health advice"), ("She shouldn't stay up so late.", "Negative advice"), ("Should we go home now?", "Advice question"), ("I think you should apologize.", "Polite advice")],
        "Give advice: Write 8 sentences with SHOULD/SHOULDN'T to advise a friend on health, study, work, and relationships.",
        "SHOULD is about good ideas and advice. Don't confuse with MUST (obligation). 'You should exercise' = good idea. 'You must work' = obligation.",
        [("Modal: SHOULD", "Papa English", null), ("Should vs Must", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L14() => C(
        "SHOULD in questions and negatives: 'Should I...?' / 'You shouldn't...' Use for seeking advice or suggesting against something.",
        ["Questions: Should I...? / Should he...? / Should we...?", "Negatives: shouldn't (should not)", "Responses: 'Yes, you should.' / 'No, you shouldn't.'", "Variations: 'I think you should...' / 'Maybe you should...'", "Difference from MUST: SHOULD = advice; MUST = obligation or necessity"],
        [("Should I?", "¿Debería?", "Should I wear a coat?"), ("shouldn't", "no debería", "You shouldn't be rude."), ("think", "creer", "I think you should rest."), ("maybe", "quizá", "Maybe you should call him."), ("ought to", "deberías", "You ought to try harder.")],
        [("Should I study tonight? — Yes, you should.", "Advice question"), ("You shouldn't forget your appointment.", "Negative advice"), ("Should we help them? — Yes, we should.", "Group advice question"), ("She shouldn't drive tired.", "Safety advice")],
        "Seek and give advice: Write 10 should/shouldn't questions and answers about common situations.",
        "In American English, 'I should' sounds like 'I shud' /ʃəd/. In British English, it's more clearly pronounced.",
        [("Should Questions", "Learn English with EnglishClass101.com", null), ("Giving Advice with SHOULD", "English Speeches", null)]
    );

    private static string A2L15() => C(
        "Form questions with correct word order. For YES/NO questions: Auxiliary + subject + verb? 'Do you like tea?' For WH- questions: WH- word + auxiliary + subject + verb? 'Where do you live?'",
        ["YES/NO: Do you...? / Does he...? / Did they...? / Can you...? / Will you...?", "WH- questions: Where do you...? / What did he...? / Why can't she...? / Who is...?", "With 'be': Are you...? / Is he...? / Were they...? / Will you be...?", "Inversion: Subject and auxiliary swap positions", "No auxiliary with 'be': 'Are you happy?' NOT 'Do you are happy?'"],
        [("Where do you live?", "¿Dónde vives?", "Where do you live? — In Madrid."), ("What did you do?", "¿Qué hiciste?", "What did you do yesterday?"), ("Can you help?", "¿Puedes ayudar?", "Can you help me please?"), ("Is she here?", "¿Está aquí?", "Is she here yet?"), ("Why did they leave?", "¿Por qué se fueron?", "Why did they leave early?")],
        [("Do you like pizza? — Yes, I do.", "Yes/no with DO"), ("Where is your house?", "WH- with BE"), ("What time does the train arrive?", "WH- with auxiliary"), ("Can you swim? — No, I can't.", "Question with modal")],
        "Write 15 questions: 5 yes/no questions, 5 WH- questions, and 5 modal questions.",
        "Question word order is critical. Wrong: 'Where you live?' Right: 'Where do you live?' The auxiliary/verb comes before the subject.",
        [("English Question Formation", "Papa English", null), ("Question Words in English", "Learn English with EnglishClass101.com", null)]
    );

    private static string A2L16() => C(
        "Form negatives correctly. Present simple: don't/doesn't + base verb. Past simple: didn't + base verb. 'I don't like coffee.' 'She doesn't work here.' 'They didn't come yesterday.'",
        ["Present: don't (I, you, we, they) / doesn't (he, she, it) + base verb", "Past: didn't + base verb (all subjects same form)", "With 'be': I'm not, he isn't, they aren't, I wasn't, they weren't", "Double negative WRONG: 'I don't want nothing' → use 'I don't want anything'", "Contractions: don't = do not, doesn't = does not, didn't = did not"],
        [("don't", "no", "I don't like spicy food."), ("doesn't", "no (3ª person)", "He doesn't work here."), ("didn't", "no (pasado)", "They didn't come yesterday."), ("isn't", "no es", "She isn't happy."), ("weren't", "no eran", "They weren't at home.")],
        [("I don't speak French.", "Present negative"), ("She doesn't have a car.", "Present negative - 3rd person"), ("We didn't see the film.", "Past negative"), ("He isn't a teacher.", "Negative with be"), ("They weren't late.", "Negative was/were")],
        "Write 12 negative sentences: 4 present simple negatives, 4 past simple negatives, and 4 negatives with 'be'.",
        "Don't use double negatives in English. Say 'I don't want anything' NOT 'I don't want nothing.' This is a common mistake from Spanish speakers.",
        [("Negative Sentences in English", "Learn English with EnglishClass101.com", null), ("English Negatives", "English Speeches", null)]
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
        "'For' and 'since' both describe duration, but differently. FOR tells us the period of time; SINCE tells us the starting point. Both are used with the Present Perfect. British Council provides excellent explanations of this distinction.",
        ["FOR + a period of time: for two hours, for a week, for six months, for ten years", "SINCE + a starting point: since Monday, since 2015, since I was a child", "Quick test: 'how long?' → FOR. 'Starting from when?' → SINCE", "Common error: 'I live here since 5 years' — wrong! Use Present Perfect: 'I have lived here for 5 years'"],
        [("for two hours", "durante dos horas", "I have been waiting for two hours."), ("for a long time", "desde hace mucho tiempo", "We haven't talked for a long time."), ("since Monday", "desde el lunes", "She has been ill since Monday."), ("since last year", "desde el año pasado", "He has worked here since last year.")],
        [("I have worked here for three years.", "FOR + duration"), ("She has been a teacher since 2018.", "SINCE + starting point"), ("He hasn't eaten since this morning.", "SINCE + time of day"), ("We have known each other for a very long time.", "FOR in a longer statement")],
        "Write 6 Present Perfect sentences: 3 with 'for' and 3 with 'since'. Talk about your hobbies, home, work, friendships, or habits.",
        "'Since' is pronounced SINTS (one syllable). 'For' in fast speech often sounds like 'fer' — that is normal and native.",
        [("Present Perfect For vs Since", "Papa English", null), ("For vs Since with Present Perfect", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L3() => C(
        "The First Conditional describes real, possible situations in the future and their likely results. If the condition happens, the result will follow. This is essential for discussing everyday predictions and plans.",
        ["Structure: IF + Present Simple, WILL + base verb", "The IF clause can come first or second: 'If I study, I will pass' = 'I will pass if I study'", "Use a comma only when the IF clause comes first", "Can also use: might, may, can, could instead of will", "Compare: First = possible future; Second = unlikely/imaginary"],
        [("If ... will", "Si ... va a / irá", "If it rains, I will take an umbrella."), ("Unless", "A menos que / Si no", "Unless you hurry, you will miss the bus."), ("might / may", "podría / puede que", "If you study, you might pass.")],
        [("If it rains tomorrow, I will stay home.", "Basic first conditional"), ("You'll miss the bus if you don't leave now.", "IF clause at the end"), ("If she calls, tell her I'm busy.", "First conditional as instruction"), ("If you eat too much sugar, you might feel ill.", "Using 'might'")],
        "Write 5 first conditional sentences about your week ahead. Think about: the weather, your studies, your plans. Use 'will', 'might', and 'might not'.",
        "'Will' contracts in spoken English: I'll, you'll, she'll, he'll, we'll, they'll. 'I will go' → 'I'll go'. Practice until contractions feel natural.",
        [("First Conditional Sentences", "Papa English", null), ("If Clauses - First Conditional", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L4() => C(
        "The Second Conditional describes unreal, hypothetical, or unlikely situations. It imagines 'What if...?' scenarios that are not currently true. Perfect English Grammar emphasizes the importance of mastering this tense.",
        ["Structure: IF + Past Simple, WOULD + base verb", "'If I were' is preferred to 'If I was' (formal writing)", "Compare: First = 'If I win the lottery' (possible) vs Second = 'If I won' (unlikely dream)", "Use for: advice ('If I were you, I would...'), imaginary situations", "'Would' contracts: I'd, you'd, she'd, he'd, we'd, they'd"],
        [("If I were", "Si yo fuera", "If I were taller, I'd play basketball."), ("If I had", "Si tuviera", "If I had more time, I would travel."), ("would", "haría/iría (condicional)", "I would live in Italy if I could."), ("If I were you", "Yo que tú / En tu lugar", "If I were you, I would apologize.")],
        [("If I had more money, I would travel the world.", "Imaginary situation"), ("She would be happier if she lived near the sea.", "Hypothetical preference"), ("If I were you, I would accept the job offer.", "Giving advice"), ("What would you do if you lost your phone?", "Second conditional question")],
        "Write 5 second conditional sentences: 2 imaginary 'what if' scenarios, 1 piece of advice using 'If I were you...', and 2 hypothetical preferences.",
        "'Would' is pronounced WOOD — the 'l' is silent. In fast speech it sounds just like 'wood'. Contractions: I'd, you'd, she'd — the 'd' is barely audible.",
        [("Second Conditional Sentences", "Papa English", null), ("If Clauses - Second Conditional", "Learn English with EnglishClass101.com", null)]
    );

    private static string B1L5() => C(
        "The Present Perfect Continuous emphasizes the duration of an action that started in the past and continues NOW. It answers 'How long have you been...?' We use it for activities, work, or states that are ongoing. This tense bridges past and present naturally.",
        ["Form: have/has + been + verb-ING", "'Have you been waiting long?' — Yes, I have been waiting for 20 minutes", "Duration: 'How long' → Present Perfect Continuous", "Complete action: 'How many?' → Present Perfect Simple", "Can omit 'been': 'I have been working' vs 'I have worked' (slightly different meaning)", "Common: 'I've been living here for 5 years' (still here) vs 'I've lived in 10 countries' (complete)"],
        [("have been working", "he estado trabajando", "I have been working on this project for weeks."), ("has been studying", "ha estado estudiando", "She has been studying English for 3 years."), ("have been living", "he estado viviendo", "We have been living in Barcelona since 2020."), ("has been waiting", "ha estado esperando", "He has been waiting for an hour.")],
        [("How long have you been learning English? — For two years.", "Duration question"), ("She has been working here since she graduated.", "Ongoing work situation"), ("They have been playing football all afternoon.", "Activity in progress"), ("I have been trying to call you all day.", "Repeated or continuous attempts")],
        "Write 6 Present Perfect Continuous sentences about your life: work, studies, hobbies, or projects. Include 'how long' in at least 2 sentences.",
        "The ING ending adds a syllable: 'study' (1) → 'studying' (3). 'Have been' flows together: 'have-BIN'. The stress falls on 'studying': 'I have been STUDying.'",
        [("Present Perfect Continuous Tense", "Papa English", null), ("How Long Have You Been...", "Learn English with EnglishClass101.com", null), ("Present Perfect Continuous Grammar", "English Speeches", null)]
    );

    private static string B1L6() => C(
        "The Past Perfect (Pluperfect) shows which event happened FIRST when discussing two past events. It uses had + past participle. This tense is crucial for storytelling, narrating complex past situations, and clarifying sequence in narratives.",
        ["Form: had + past participle (worked, gone, eaten, finished)", "Use: to show the earlier of two past events", "'Before they arrived, I had already finished the dinner' — finished happened first", "Time expressions: after, before, when, by the time, by + noun", "Common mistake: mixing Past Simple and Past Perfect incorrectly"],
        [("had finished", "había terminado", "Before the concert started, she had finished her homework."), ("had eaten", "había comido", "By the time he arrived, we had already eaten."), ("had never seen", "nunca había visto", "I had never seen snow before that day."), ("had left", "había salido", "When they called, I had already left the house.")],
        [("She had studied French before she moved to Paris.", "Earlier event → later event"), ("By the time I woke up, everyone had left.", "Clear sequence with 'by the time'"), ("He realized he had forgotten his passport.", "Past realization about earlier action"), ("After she had finished university, she traveled the world.", "Completed past action before another past action")],
        "Write 5 Past Perfect sentences about your life, telling stories where one event happened before another. Use time expressions like 'before', 'after', 'by the time'.",
        "'Had' is pronounced HAD (one syllable). In rapid speech, it's often just: 'I'd finished', 'she'd eaten' (with contractions). The stress falls on the past participle: 'I had FINished'.",
        [("Past Perfect Tense Explained", "Papa English", null), ("Had vs Did - Past Perfect", "Learn English with EnglishClass101.com", null), ("When to Use Past Perfect", "English Speeches", null)]
    );

    private static string B1L7() => C(
        "Relative clauses (also called adjective clauses) give extra information about a noun using relative pronouns: WHO (people), THAT/WHICH (things), WHOSE (possession), WHERE (place), WHEN (time). They make sentences more sophisticated and are essential for advanced English.",
        ["WHO: for people — 'The woman who taught me is kind'", "THAT/WHICH: for things — 'The car that I bought is blue' or 'The car, which I bought, is blue'", "WHOSE: for possession — 'The girl whose phone was stolen reported it'", "WHERE: for places — 'The café where we met is closed'", "WHEN: for time — 'The day when we arrived was sunny'", "Defining clause (no commas): 'Students who study hard pass' — ESSENTIAL info", "Non-defining clause (with commas): 'John, who is 25, is my brother' — EXTRA info"],
        [("who", "quien/que (persona)", "The doctor who helped me was excellent."), ("that/which", "que (cosa)", "The book which I read was fascinating."), ("whose", "cuyo/a/os/as", "The student whose essay won the prize is here."), ("where", "donde/en el que", "The restaurant where we ate was expensive."), ("when", "cuando/en el que", "The time when I was happiest was my childhood.")],
        [("I know someone who speaks five languages.", "Relative clause with who"), ("The house that we bought is very old.", "Relative clause with that"), ("She is the woman whose son is a doctor.", "Relative clause with whose"), ("That is the café where we first met.", "Relative clause with where")],
        "Write 7 sentences using different relative pronouns (who, that, which, whose, where, when). Mix defining and non-defining clauses.",
        "When 'that' is a relative pronoun, it's unstressed: 'The book THAT I read' — 'that' is quick and light. In 'the time when I was happy', 'when' is also unstressed.",
        [("Relative Clauses in English", "Papa English", null), ("WHO THAT WHICH WHOSE - Relative Pronouns", "Learn English Kids", null), ("Defining and Non-Defining Relative Clauses", "Cambridge English", null)]
    );

    private static string B1L8() => C(
        "Reported Speech (Indirect Speech) allows you to communicate what someone said without using their exact words. Rules change for verb tenses, pronouns, time words, and place references. This is essential for speaking naturally about what others said.",
        ["Direct: 'I like ice cream,' he said. → Reported: He said he liked ice cream.", "Tense shift: Present → Past, Past → Past Perfect, will → would", "Pronouns shift: I → he/she, you → me/him/her, my → his/her, this → that", "Time shifts: now → then, today → that day, tomorrow → the next day, here → there", "Yes/No questions: ask + if/whether; WH questions: ask what/where/why/who"],
        [("said that", "dijo que", "She said that she was tired."), ("asked if", "preguntó si", "He asked if we wanted to go."), ("told me", "me dijo", "She told me she would be late."), ("asked what", "preguntó qué", "He asked what I was doing.")],
        [("Direct: 'I am happy.' → Reported: She said she was happy.", "Present → Past"), ("Direct: 'Will you help?' → Reported: He asked if I would help.", "Yes/No question"), ("Direct: 'Where is the station?' → Reported: He asked where the station was.", "WH question"), ("Direct: 'I have finished my homework.' → Reported: She said she had finished her homework.", "Present Perfect → Past Perfect")],
        "Rewrite 6 sentences from direct to reported speech. Include 2 statements, 2 yes/no questions, and 2 WH questions. Pay attention to tense changes, pronouns, and time expressions.",
        "'Said' is pronounced SED (rhymes with 'bed'). 'Asked' is pronounced ASKT. In reported speech, these helper verbs are often unstressed: 'She SAID she was going' — 'said' is quick.",
        [("Reported Speech - Indirect Speech", "Papa English", null), ("Direct and Indirect Speech", "Learn English with EnglishClass101.com", null), ("Reported Speech Rules and Examples", "Cambridge English", null)]
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

    private static string C2L1() => C(
        "Register is the level of formality in language. C2 speakers switch effortlessly between formal, neutral, and informal registers. Understanding register means knowing when something sounds too formal, too casual, or just right. This is native-level proficiency.",
        ["Formal: 'I would like to enquire about...' / Informal: 'I want to ask about...'", "Subjunctive in formal writing: 'I suggest that he be present' (not 'is')", "Avoid filler words in formal contexts: 'basically', 'you know', 'like', 'sort of'", "Use hedging for academic precision: 'It appears that...', 'Evidence suggests...'", "Nominalizations elevate register: 'develop' → 'development', 'solve' → 'solution'"],
        [("solicit", "solicitar / pedir formalmente", "I would like to solicit your expert opinion."), ("ratify", "ratificar", "The treaty was ratified by all members."), ("promulgate", "promulgar", "The new law was promulgated last year."), ("ameliorate", "mejorar / aliviar", "Measures were taken to ameliorate the situation."), ("commence", "comenzar / iniciar", "The ceremony will commence at 9 AM.")],
        [("I would like to enquire about the position advertised on your website.", "Formal opening for a letter"), ("I suggest that the committee be given more time to deliberate.", "Subjunctive mood"), ("The board ratified the proposal with immediate effect.", "Formal vocabulary in a business context")],
        "Rewrite this casual email in formal register: 'Hey, I wanted to ask about your job. I think I'd be really good at it and I want to know more stuff about what it involves. Can you send me some info? Thanks.'",
        "In formal spoken English, your speech should be clear, measured, and deliberate. Maintain full vowels — do not reduce them as in casual speech.",
        [("Register and Formality in English", "Papa English", null), ("Formal vs Informal English", "Learn English with EnglishClass101.com", null)]
    );

    private static string C2L2() => C(
        "At C2 level, you use complex idioms, cultural references, and nuanced vocabulary naturally. These expressions add texture and authenticity — not by translating from your language, but by thinking in English.",
        ["'Pyrrhic victory': a win so costly it resembles defeat — from King Pyrrhus of Epirus", "'Penultimate': second to last — NOT 'second ultimate'!", "'Cogent': clear, logical, convincing — a cogent argument, cogent reasoning", "'Circumlocution': using many words to avoid saying something directly", "'Equivocal': ambiguous, capable of multiple interpretations"],
        [("pyrrhic victory", "victoria pírrica", "Their market dominance was a pyrrhic victory."), ("penultimate", "penúltimo", "The penultimate episode was the best."), ("cogent", "convincente / sólido", "She presented a cogent case for reform."), ("circumlocution", "circunloquio / rodeos", "Stop the circumlocution — just say what you mean."), ("equivocal", "equívoco / ambiguo", "His equivocal answer raised more questions.")],
        [("The company's Pyrrhic victory left it with no resources to compete further.", "Pyrrhic victory in business"), ("In the penultimate chapter, the mystery is finally unraveled.", "Penultimate as literary term"), ("Her argument was so cogent that no one could find a flaw.", "Cogent in academic context"), ("His equivocal response only deepened suspicions.", "Equivocal in political context")],
        "Write a paragraph (7–9 sentences) using all five vocabulary items from this lesson in natural, connected prose. You can write about a fictional character, a historical event, or a scenario from your professional life.",
        "'Penultimate' is pen-UL-ti-mate (5 syllables, stress 2nd). 'Equivocal' is eh-KWIV-oh-kal (4 syllables, stress 2nd). 'Cogent' is KOH-jent. 'Circumlocution' is sir-kum-loh-KYOO-shun.",
        [("English Idioms C2 Level", "English Speeches", null), ("Pyrrhic Victory Meaning", "English Academy", null), ("Advanced English Vocabulary", "Cambridge English", null)]
    );

    private static string C2L3() => C(
        "Hedging language makes claims less absolute, signalling academic caution and intellectual honesty. It is indispensable in research writing, academic essays, and any context where absolute certainty is impossible.",
        ["Hedging verbs: suggest, indicate, appear, seem, tend, imply, point to", "Modal hedges: may, might, could, would (possibility), should (recommendation)", "Adverb hedges: possibly, probably, apparently, seemingly, generally, typically", "Phrase hedges: 'It could be argued that...', 'Evidence suggests that...', 'It appears that...'", "Over-hedging weakens writing — use hedges purposefully, not constantly"],
        [("It could be argued that", "Se podría argumentar que", "It could be argued that inequality drives conflict."), ("Evidence suggests", "La evidencia sugiere", "Evidence suggests a causal link."), ("It appears / It seems", "Parece que", "It appears that demand is declining."), ("tend to", "tiende a", "Students tend to perform better with feedback."), ("ostensibly", "aparentemente / en apariencia", "Ostensibly, the policy aims to reduce poverty.")],
        [("The data suggest a possible correlation between sleep and productivity.", "Hedging with 'suggest'"), ("It could be argued that economic inequality exacerbates social tensions.", "Phrase hedge"), ("The results appear to indicate a significant improvement.", "Hedge with 'appear'"), ("Ostensibly, the measure was introduced for public safety reasons.", "Adverb hedge")],
        "Rewrite these definitive sentences with appropriate hedging: 1) Climate change causes wars. 2) Social media makes teenagers depressed. 3) Early education determines success in life. 4) Exercise cures anxiety. 5) Technology will replace all human workers.",
        "'Ostensibly' is os-TEN-si-blee (5 syllables). 'Apparently' is a-PAR-ent-lee (4 syllables). In speech, pause slightly before a hedge — it signals deliberate, careful thought.",
        [("Academic Writing Hedging Language", "Academic English", null), ("How to Use Modal Verbs for Hedging", "English Speeches", null), ("Cautious Language in Essays", "Cambridge English", null)]
    );

    private static string C2L4() => C(
        "Critical analysis means examining arguments carefully: identifying strengths, exposing logical flaws, and evaluating evidence. At C2, you both construct and deconstruct sophisticated arguments.",
        ["Non sequitur: conclusion does not logically follow from the premises", "Ad hominem: attacking the speaker's character instead of their argument", "Straw man: misrepresenting an opponent's argument to make it easier to attack", "False dichotomy: presenting only two options when others exist", "Equivocation: using an ambiguous term with different meanings in the same argument"],
        [("non sequitur", "conclusión que no se sigue", "That is a non sequitur — it does not follow."), ("ad hominem", "ataque personal", "That ad hominem adds nothing to the debate."), ("cogent", "sólido / convincente", "The paper makes a cogent, well-evidenced argument."), ("equivocal", "equívoco / ambiguo", "The data is equivocal on this point."), ("circumlocution", "circunloquio / rodeos", "His circumlocution masked a lack of substance.")],
        [("The argument is cogent and supported by robust empirical evidence.", "Positive critical evaluation"), ("This is a classic non sequitur — growth does not automatically reduce poverty.", "Identifying a logical flaw"), ("The author resorts to ad hominem attacks rather than engaging with the argument.", "Identifying ad hominem"), ("The data remain equivocal, pointing in different directions depending on methodology.", "Acknowledging uncertainty")],
        "Find a news article or opinion piece. Write a critical analysis paragraph (8–10 sentences) identifying at least 2 argumentative strengths and 2 logical weaknesses. Use vocabulary from this lesson.",
        "'Non sequitur' is Latin: non-SEK-wi-ter. 'Ad hominem' is ad-HOM-in-em. Academic Latin phrases are pronounced the English way, not classical Latin. Both 3 syllables, stress on 2nd.",
        [("Logical Fallacies Explained", "TED-Ed", null), ("Critical Thinking and Logical Fallacies", "Paul Graham", null), ("How to Analyze Arguments", "Cambridge English", null)]
    );

    private static string C2L5() => C(
        "Paraphrasing and summarization at C2 level requires maintaining nuance while changing structure. You reformulate complex ideas without losing meaning. Summarization condenses information while retaining key points. Essential for academic and professional communication.",
        ["Paraphrasing: restate using different words/structures but same meaning", "Avoid simply replacing words — change sentence structure completely", "Summarization: distil key points from longer text into essential information", "Signal phrases: 'To put it another way...', 'In other words...', 'Essentially...'", "Maintain accuracy — paraphrasing is NOT interpretation or commentary"],
        [("paraphrase", "parafrasear", "Let me paraphrase that for clarity."), ("distil", "destilar / extraer lo esencial", "The report distils complex data into actionable insights."), ("synopsis", "sinopsis / resumen", "A synopsis of the chapter is provided below."), ("recapitulate", "recapitular / resumir", "To recapitulate, the main findings are..."), ("consolidate", "consolidar / resumir", "The analysis consolidates evidence from multiple sources.")],
        [("Original: 'The data indicates a positive correlation.' / Paraphrase: 'Evidence points to a relationship between variables.'", "Basic paraphrasing"), ("Summarizing: 'The 300-page report concludes that climate policy must prioritize renewable energy.'", "Condensing to essential points"), ("The author's argument, stripped to essentials, is that innovation drives progress.", "Paraphrasing while summarizing"), ("To recapitulate, three key challenges emerge from this analysis.", "Summarization signal phrase")],
        "Find a complex paragraph from an academic article. Paraphrase it (3–4 sentences) maintaining all nuance, then summarize it (1–2 sentences) to its essential message.",
        "'Paraphrase' is PAR-uh-fraze (stress 1st). 'Consolidate' is con-SOL-i-date (stress 2nd). 'Recapitulate' is re-kuh-PIT-choo-late (stress 3rd). In speech, paraphrasing flows naturally — it is not word-by-word substitution.",
        [("Paraphrasing Strategies", "Academic English", null), ("Summarizing Skills for C2", "Cambridge English", null), ("Advanced Paraphrasing Techniques", "English Speeches", null)]
    );

    private static string C2L6() => C(
        "Pragmatics examines what is MEANT beyond literal words. Implicature refers to implied meaning. Native speakers understand 'Can you pass the salt?' as a request, not a question about ability. Mastering pragmatics makes your English genuinely native-like.",
        ["Implicature: implied meaning beyond literal words", "'It's cold in here' might mean 'Close the window' or 'Please leave'", "Presupposition: what is assumed to be true. 'Stop eating cake' presupposes you ARE eating cake", "Speech acts: performative verbs that do something (promise, apologize, request, warn)", "Context determines meaning — the same sentence has different meanings in different situations"],
        [("implicature", "implicatura / significado implícito", "The implicature of that comment is that she disagrees."), ("presupposition", "presuposición / asunción implícita", "The presupposition here is that he was late."), ("pragmatic", "pragmático / práctico contextualmente", "The pragmatic meaning differs from the literal."), ("felicitous", "apropiado / conveniente", "That remark is hardly felicitous in formal settings."), ("infelicitous", "inapropiado / fuera de lugar", "The comment was infelicitous — it offended the audience.")],
        [("'You could do better' → Implicature: you are not doing well enough", "Implied criticism"), ("'Have you finished?' asked at 5 PM → Presupposition: you should be done by now", "Presupposition in context"), ("'I promise to be on time' → Speech act: making a commitment", "Performative verb"), ("Literal: 'The door is open.' Pragmatic: 'You can enter.' or 'It's windy here.'", "Context-dependent pragmatics")],
        "Select 5 sentences that have implicit meaning beyond literal words. Explain the implicature and the context in which each is said.",
        "'Pragmatic' is prag-MAT-ic (3 syllables, stress 2nd). 'Implicature' is IM-pli-kuh-chur (4 syllables, stress 1st). 'Presupposition' — stress 3rd syllable: pre-sup-o-ZI-shun.",
        [("Pragmatics in English", "Academic English", null), ("Implicature and Indirect Speech Acts", "Cambridge English", null), ("Context and Meaning in English", "English Speeches", null)]
    );

    private static string C2L7() => C(
        "Stylistic variation — the ability to shift register, tone, and style based on context — distinguishes near-native from native speakers. C2 mastery involves conscious control of style for effect. Literary devices create impact; clarity requires simplicity. Style is a signature.",
        ["Register shift: formal ↔ informal based on audience", "Tone: humorous, serious, ironic, compassionate, clinical, passionate", "Sentence variety: simple, compound, complex structures for rhythm and emphasis", "Rhetorical devices: alliteration, parallelism, repetition, contrast for effect", "Clarity vs. complexity: know when to simplify and when complexity adds nuance"],
        [("eloquent", "elocuente / expresivo", "Her eloquent speech moved the audience."), ("terse", "conciso / breve", "His terse reply indicated displeasure."), ("baroque", "barroco / muy ornamentado", "The baroque prose is difficult to parse."), ("understated", "subestimado / discreto", "The understated humor appeals to sophisticated audiences."), ("prosaic", "prosaico / ordinario", "The prosaic description lacks emotional impact.")],
        [("Register shift: 'The defendant unlawfully appropriated funds.' (formal) vs. 'He stole the money.' (direct)", "Formal to direct"), ("Tone in one sentence: 'Your argument, while creative, lacks empirical support.' (constructive but firm)", "Balanced critical tone"), ("Alliteration for effect: 'The bitter, bleak, barren landscape...'", "Stylistic device"), ("Parallelism: 'It was the best of times, it was the worst of times.'", "Rhetorical device")],
        "Rewrite the same information in 3 different styles: 1) Scientific/clinical, 2) Journalistic/accessible, 3) Literary/creative. Show how style shapes meaning.",
        "Eloquent is e-KWAH-went (stress 1st). Baroque is buh-ROHK (stress 2nd). When shifting styles, your intonation and pace change — formal speech is slower and more precise.",
        [("Stylistic Variation in Writing", "Cambridge English", null), ("Register and Style in English", "Academic English", null), ("Literary Devices and Rhetoric", "English Speeches", null)]
    );

    private static string C2L8() => C(
        "Native speaker features include: elision (omitting sounds), assimilation (sound changes), prosody (rhythm/intonation), and discourse patterns. At C2, you understand how native speakers really speak — not textbook English, but authentic variation.",
        ["Elision: 'going to' → 'gonna', 'want to' → 'wanna' (in informal speech)", "Assimilation: 'handbag' → /hæm'bæg/, 'that' → /ðət/ before certain sounds", "Prosody: intonation rising for questions, stress on content words, rhythm patterns", "Discourse patterns: turn-taking, back-channeling ('mm', 'yeah'), fillers ('like', 'you know')", "Variation: native speakers code-switch, use colloquialisms, break grammar rules intentionally"],
        [("elision", "elisión / omisión", "Elision is common in rapid, informal speech."), ("assimilation", "asimilación / cambio de sonido", "Assimilation affects pronunciation in connected speech."), ("prosody", "prosodia / entonación y ritmo", "Native-like prosody requires extensive listening."), ("colloquial", "coloquial / informal", "Colloquial expressions mark native speech."), ("discourse marker", "marcador del discurso", "Discourse markers like 'you know' signal native speech patterns.")],
        [("I'm gonna go to the store. / I'm going to go to the store.", "Elision in informal speech"), ("Handbag pronounced /hæm'bæg/ due to assimilation", "Sound change in connected speech"), ("Rising intonation on 'You're coming?' signals question", "Prosodic patterns"), ("'Like, he was totally confused, you know?' — Native informal pattern", "Discourse features")],
        "Record yourself or find a native speaker interview. Transcribe 2–3 minutes and identify: elisions, assimilations, discourse markers, and prosodic patterns used.",
        "'Elision' is i-LIZH-un (stress 2nd). 'Assimilation' is uh-sim-uh-LAY-shun (stress 3rd). 'Prosody' is PRAH-suh-dee (stress 1st). Native speech flows — stress and intonation carry meaning more than individual words.",
        [("Native English Pronunciation Patterns", "Papa English", null), ("Connected Speech in English", "Cambridge English", null), ("How Native Speakers Really Talk", "English Speeches", null)]
    );

    private static string C2L9() => C(
        "Subtle nuance and precision distinguish C2 mastery. This means understanding shades of meaning, using words with exact precision, and recognizing when near-synonyms carry different implications. English has many near-synonyms that differ in register, connotation, or subtle meaning.",
        ["'Mistake' vs 'error' vs 'blunder': mistake is unintentional; error is more formal; blunder is a serious mistake", "'Angry' vs 'furious' vs 'irate': angry is general; furious is intense emotion; irate is formal/archaic", "'Small' vs 'tiny' vs 'diminutive': small is neutral; tiny emphasizes unusually small; diminutive is formal/literary", "Connotation matters: 'frugal' (positive) vs 'stingy' (negative), both mean money-conscious", "Precision is power: choosing exact words elevates writing and shows native competence"],
        [("nuance", "matiz / sutileza", "The nuance of this distinction is important."), ("subtlety", "sutileza", "Subtlety is lost in translation."), ("connotation", "connotación / matiz", "The connotation of the word is pejorative."), ("pedantic", "pedante / muy formal", "Avoiding pedantic language keeps writing accessible."), ("discernment", "discernimiento / buen juicio", "Discernment in word choice separates native from non-native writers.")],
        [("A blunder (serious mistake) vs a simple slip", "Distinguishing severity levels"), ("Frugal (approved thriftiness) vs stingy (disapproved miserliness)", "Connotation differences"), ("The nuance between 'I believe' (personal opinion) and 'I know' (certainty)", "Epistemic precision"), ("Subtle differences shape meaning: 'reluctant' vs 'unwilling'", "Near-synonym precision")],
        "Find 3 pairs of near-synonyms in a formal text. Explain why the author chose one over the other. What would change if they used the alternative?",
        "'Nuance' is NOO-ahns (stress 1st). 'Connotation' is kuh-no-TAY-shun (stress 3rd). 'Pedantic' is puh-DAN-tik (stress 2nd). Precision is pronounced PRIH-SIZH-un.",
        [("Subtle Differences in English Vocabulary", "Papa English", null), ("Near Synonyms and Precision", "Cambridge English", null), ("Connotation in Advanced English", "English Speeches", null)]
    );

    private static string C2L10() => C(
        "Discourse analysis examines how language functions beyond individual sentences — how texts are organized, how arguments develop, how writers persuade. C2 proficiency includes understanding text structure, cohesion devices, and rhetorical patterns that shape meaning at macro level.",
        ["Discourse markers organize ideas: 'however' signals contrast, 'moreover' adds ideas, 'therefore' shows cause-effect", "Thematic progression: given information (what we know) → new information (what's new) shapes flow", "Rhetorical structure: problem-solution, chronological, cause-effect, comparison organize extended texts", "Presuppositions shape discourse: what is assumed vs. explicitly stated", "Register shift within discourse: formal introduction, informal middle section, formal conclusion"],
        [("discourse analysis", "análisis del discurso", "Discourse analysis reveals how texts persuade."), ("cohesion", "cohesión / conexión", "Strong cohesion guides readers through arguments."), ("thematic", "temático / relacionado al tema", "Thematic progression makes texts coherent."), ("presupposition", "presuposición / asunción", "Presuppositions shape how readers interpret text."), ("rhetorical", "retórico / persuasivo", "The rhetorical structure strengthens the argument.")],
        [("'First, we observe the problem. Next, we examine causes. Finally, solutions emerge.' (Explicit organization)", "Discourse structure"), ("Given: 'She is a doctor.' New: 'She specializes in cardiology.'", "Thematic progression"), ("Problem (inequality exists) → Solution (policy change needed)", "Rhetorical pattern"), ("'Some argue that... However, evidence suggests...' (Discourse markers signaling contrast)", "Cohesion across sentences")],
        "Analyze a 1-2 page article. Map its discourse structure: what is given vs. new information, what rhetorical pattern organizes it (problem-solution, comparison, etc.), and what discourse markers guide readers.",
        "'Discourse' is DIS-korse (stress 1st). 'Cohesion' is ko-HEE-zhun (stress 2nd). 'Rhetorical' is ruh-TOR-i-kal (stress 2nd). Pronounce these words precisely — they are academic register markers.",
        [("Discourse Analysis in English Texts", "Academic English", null), ("How Texts are Organized", "Cambridge English", null), ("Analyzing Text Structure", "English Speeches", null)]
    );

    private static string C2L11() => C(
        "Cultural references and allusions enrich language but require cultural knowledge to fully understand. Native English includes references to literature, history, film, and shared cultural experiences. Understanding and using these marks sophisticated, culturally integrated English.",
        ["Literary allusions: 'Orwellian' (from George Orwell → dystopian), 'Kafkaesque' (from Kafka → absurd bureaucracy)", "Historical references: 'Titanic moment' (doomed inevitability), 'Shakespearean tragedy' (noble downfall)", "Film/pop culture: 'Mission Impossible' (seemingly impossible task), 'Star Wars moment' (good vs evil confrontation)", "Proverbs and idioms embed cultural wisdom: 'Don't count chickens before they hatch' (don't assume success)", "Understanding context-dependent meaning: same phrase means differently to different cultures"],
        [("Orwellian", "orwelliano / distópico", "The government's surveillance was Orwellian in scale."), ("Kafkaesque", "kafkiano / absurdo burocrático", "Navigating the bureaucracy felt Kafkaesque."), ("Shakespearean", "shakespeariano / trágico", "Their romance had a Shakespearean quality."), ("allude", "aludir / hacer referencia", "The author alludes to classical mythology."), ("metaphorical", "metafórico / figurado", "His language was highly metaphorical and cultural.")],
        [("'That's an Orwellian nightmare' — understood by those familiar with 1984", "Literary allusion"), ("'A Titanic moment was inevitable' — understood as doomed", "Historical reference"), ("Cultural allusions assume shared knowledge", "Context-dependent meaning"), ("'Don't put all your eggs in one basket' — embedded cultural wisdom", "Proverb understanding")],
        "Find 5 allusions in English literature, film, or news. Explain the cultural reference and its meaning. What would be lost if the allusion were not understood?",
        "'Orwellian' is or-WEL-ee-an (4 syllables, stress 2nd). 'Kafkaesque' is kah-ka-ESKE (3 syllables, stress 3rd). 'Allude' is uh-LOOD (2 syllables, stress 2nd). Cultural references are pronounced distinctively to signal sophistication.",
        [("English Allusions and References", "Papa English", null), ("Literary and Cultural References", "Cambridge English", null), ("Understanding Allusions in English", "English Speeches", null)]
    );

    private static string C2L12() => C(
        "Mastery of ambiguity means understanding that English embraces multiple meanings intentionally. Puns, wordplay, irony, and deliberate vagueness are tools for sophisticated communication. C2 proficiency includes producing ambiguous language when appropriate and recognizing layers of meaning.",
        ["Ambiguity can be intentional: poets use it for depth, advertisers for memorable messaging, speakers for diplomacy", "Lexical ambiguity: 'bank' (financial institution vs. riverbank), 'light' (illumination vs. weight)", "Syntactic ambiguity: 'I saw the man with the telescope' — who has the telescope?", "Pragmatic ambiguity: 'That's interesting' can be genuine interest OR subtle criticism", "Recognizing and using ambiguity marks native sophistication — not confusion, but intentional layering"],
        [("ambiguity", "ambigüedad / falta de claridad", "The ambiguity in the statement allows multiple interpretations."), ("wordplay", "juego de palabras", "The poet uses wordplay for humorous effect."), ("irony", "ironía", "Her ironic comment masked genuine concern."), ("pun", "juego de palabras / calambur", "The pun relies on double meaning."), ("equivocal", "equívoco / ambiguo", "His equivocal response left everyone confused.")],
        [("'That's a brilliant idea!' — Could be genuine praise OR sarcastic criticism (pragmatic ambiguity)", "Intentional ambiguity"), ("'Light' can mean brightness OR lack of weight (lexical ambiguity)", "Multiple meanings"), ("'I saw the boy with the binoculars' — Who has them? (syntactic ambiguity)", "Sentence structure creates ambiguity"), ("Poetry intentionally uses ambiguity to create multiple layers of meaning", "Artistic ambiguity")],
        "Write a short paragraph (4–6 sentences) that intentionally uses ambiguity at least 3 times. The meaning should be clear in context but allow for alternative interpretations. Then explain where the ambiguity lies.",
        "'Ambiguity' is am-BIG-yoo-i-tee (5 syllables, stress 2nd). 'Equivocal' is eh-KWIV-uh-kal (4 syllables, stress 2nd). 'Irony' is EYE-ruh-nee (3 syllables, stress 1st). These words are pronounced distinctly — they signal advanced discourse.",
        [("Ambiguity in English Language", "Academic English", null), ("Intentional Ambiguity in Literature", "Cambridge English", null), ("Playing with Meaning in English", "English Speeches", null)]
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
