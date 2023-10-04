namespace JournalApp;

public static class Worksheets
{
    private static IReadOnlyList<Worksheet> _collection;

    public static IReadOnlyList<Worksheet> Collection => _collection ??= GetAllWorksheets().ToList();

    private static IEnumerable<Worksheet> GetAllWorksheets()
    {
        // All are sourced from https://www.nimh.nih.gov/health/publications/brochures-and-fact-sheets-in-english.
        // The information is in the public domain and may be reused or copied without permission. However, you may not reuse or copy the images. 

        yield return new()
        {
            Title = "My Mental Health: Do I Need Help?",
            Description = "Information about how to assess your mental health and determine if you need help. It provides examples of mild and severe symptoms, as well as self-care activities and options for professional help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/health/publications/my-mental-health-do-i-need-help/my-mental-health-do-i-need-help.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/my-mental-health-do-i-need-help",
            Category = "",
        };
        yield return new()
        {
            Title = "Generalized Anxiety Disorder: When Worry Gets Out of Control",
            Description = "Information about generalized anxiety disorder including common signs and symptoms, treatment options, and how to find help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/generalized-anxiety-disorder-gad/generalized_anxiety_disorder.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/generalized-anxiety-disorder-gad",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "I'm So Stressed Out! Fact Sheet",
            Description = "Intended for teens and young adults and presents information about stress, anxiety, and ways to cope when feeling overwhelmed.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/so-stressed-out-fact-sheet/Im-So-Stressed-Out.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/so-stressed-out-fact-sheet",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "I'm So Stressed Out! Infographic",
            Description = "Infographic about stress, anxiety, and ways to cope when feeling overwhelmed. It was developed for use on social media to highlight the \"I'm So Stressed Out\" fact sheet.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/so-stressed-out-infographic/so-stressed-out-infographic.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/so-stressed-out-infographic",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Panic Disorder: When Fear Overwhelms",
            Description = "Information about panic disorder, including common signs and symptoms, treatment options, and how to find help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/panic-disorder-when-fear-overwhelms/panic-disorder-when-fear-overwhelms.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/panic-disorder-when-fear-overwhelms",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Social Anxiety Disorder: More Than Just Shyness",
            Description = "Information about social anxiety disorder, including common signs and symptoms, treatment options, and how to find help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/social-anxiety-disorder-more-than-just-shyness/social-anxiety-disorder-more-than-just-shyness.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/social-anxiety-disorder-more-than-just-shyness",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Stand Up to Stress!",
            Description = "Coloring and activity book for children ages 8-12 teaches kids about stress and anxiety and offers tips for coping in a healthy way.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/stand-up-to-stress/stand-up-to-stress.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/stand-up-to-stress",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Stress Catcher",
            Description = "\"Fortune teller\" craft for children that offers coping strategies to help manage stress and other difficult emotions. Instructions on how to create and use the Stress Catcher are provided.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/stress-catcher/stress-catcher.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/stress-catcher",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Attention-Deficit/Hyperactivity Disorder in Adults: What You Need to Know",
            Description = "Information about attention-deficit/hyperactivity disorder (ADHD) in adults including symptoms, how it's diagnosed, causes, treatments, and how to find help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/adhd-in-adults.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/adhd-what-you-need-to-know",
            Category = "ADHD",
        };
        yield return new()
        {
            Title = "Attention-Deficit/Hyperactivity Disorder in Children and Teens: What You Need to Know",
            Description = "Information about attention-deficit/hyperactivity disorder (ADHD) in children and teens including symptoms, how it is diagnosed, causes, treatment options, and helpful resources.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/adhd-what-you-need-to-know/adhd-in-children-and-teens-what-you-need-to-know.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/attention-deficit-hyperactivity-disorder-in-children-and-teens-what-you-need-to-know",
            Category = "ADHD",
        };
        yield return new()
        {
            Title = "Autism Spectrum Disorder",
            Description = "Information about autism spectrum disorder (ASD) including signs and symptoms, causes and risk factors, diagnosis in young children, older children, teens, and adults, and treatments.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/autism-spectrum-disorder/22-MH-8084-Autism-Spectrum-Disorder.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/autism-spectrum-disorder",
            Category = "Autism",
        };
        yield return new()
        {
            Title = "Bipolar Disorder",
            Description = "Information on bipolar disorder including symptoms, causes, diagnosis, treatment options, and resources to find help for yourself or others.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/health/publications/bipolar-disorder/bipolar-disorder_0.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/bipolar-disorder",
            Category = "Bipolar",
        };
        yield return new()
        {
            Title = "Bipolar Disorder in Children and Teens",
            Description = "Information about bipolar disorder in children and teens including causes, signs and symptoms, diagnosis, treatment options, and how to help a child or teen who has bipolar disorder.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/bipolar-disorder-in-children-and-teens/bipolar-disorder-in-children-and-teens.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/bipolar-disorder-in-children-and-teens",
            Category = "Bipolar",
        };
        yield return new()
        {
            Title = "Bipolar Disorder in Teens and Young Adults: Know the Signs",
            Description = "This infographic presents common signs and symptoms of bipolar disorder in teens and young adults.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/bipolar-disorder-in-children-and-teens/bipolar-disorder-in-teens-and-young-adults.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/bipolar-disorder-in-teens-and-young-adults-know-the-signs",
            Category = "Bipolar",
        };
        yield return new()
        {
            Title = "Borderline Personality Disorder",
            Description = "Information on borderline personality disorder, including signs and symptoms, causes, diagnosis, co-occurring illnesses, treatment options, and resources to find help for yourself or others.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/borderline-personality-disorder/borderline-personality-disorder.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/borderline-personality-disorder",
            Category = "BPD",
        };
        yield return new()
        {
            Title = "Chronic Illness and Mental Health: Recognizing and Treating Depression",
            Description = "Information about depression and mental health for people living with chronic illnesses, including children and adolescents. It discusses symptoms, risk factors, treatment options, and presents resources to find help for yourself or someone else.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/health/publications/chronic-illness-mental-health/recognizing-and-treating-depression.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/chronic-illness-mental-health",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Depression",
            Description = "Information about depression including the different types of depression, signs and symptoms, how it is diagnosed, treatment options, and how to find help for yourself or a loved one.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/depression/21-mh-8079-depression_0.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/depression",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Depression in Women: 5 Things You Should Know",
            Description = "Information about depression in women including signs and symptoms, types of depression unique to women, treatment options, and how to find help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/depression-in-women/depression-in-women-5-things-you-should-know.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/depression-in-women",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Perinatal Depression",
            Description = "Information about perinatal depression, including how it differs from the \"baby blues,\" causes, signs and symptoms, treatment options, and how you or a loved one can get help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/perinatal-depression/perinatal-depression.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/perinatal-depression",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Seasonal Affective Disorder",
            Description = "Information about seasonal affective disorder (SAD), a type of depression. It includes a description of SAD, signs and symptoms, how SAD is diagnosed, causes, and treatment options.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/seasonal-affective-disorder/seasonal-affective-disorder.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/seasonal-affective-disorder",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Seasonal Affective Disorder (SAD): More Than the Winter Blues",
            Description = "Infographic about how to recognize the symptoms of seasonal affective disorder (SAD) and what to do to get help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/seasonal-affective-disorder/seasonal_affective_disorder_more_than_the_winter_blues.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/seasonal-affective-disorder-sad-more-than-the-winter-blues",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Teen Depression: More Than Just Moodiness",
            Description = "Intended for teens and young adults and presents information about how to recognize the symptoms of depression and how to get help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/teen-depression/Teen_Depression_More_Than_Just_Moodiness_2022.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/teen-depression",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Eating Disorders: About More Than Food",
            Description = "Information about eating disorders including who is at risk, common types of eating disorders and the symptoms of each, treatment options, and resources to find help for yourself or someone else.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/eating-disorders/eating-disorders-about-more-than-food.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/eating-disorders",
            Category = "Eating Disorders",
        };
        yield return new()
        {
            Title = "Let's Talk About Eating Disorders",
            Description = "Facts that can help shape conversations around eating disorders.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/lets-talk-about-eating-disorders/lets-talk-about-eating-disorders_0.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/lets-talk-about-eating-disorders",
            Category = "Eating Disorders",
        };
        yield return new()
        {
            Title = "Obsessive-Compulsive Disorder: When Unwanted Thoughts or Repetitive Behaviors Take Over",
            Description = "Information on obsessive-compulsive disorder (OCD) including signs and symptoms, causes, and treatment options such as psychotherapy and medication.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/obsessive-compulsive-disorder-when-unwanted-thoughts-take-over/obsessive-compulsive-disorder-508.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/obsessive-compulsive-disorder-when-unwanted-thoughts-or-repetitive-behaviors-take-over",
            Category = "OCD",
        };
        yield return new()
        {
            Title = "PANDAS—Questions and Answers",
            Description = "Information about the causes, signs and symptoms, diagnosis, and treatment of Pediatric Autoimmune Neuropsychiatric Disorders Associated with Streptococcal Infections (PANDAS).",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/pandas/pandas-qa.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/pandas",
            Category = "OCD",
        };
        yield return new()
        {
            Title = "Understanding Psychosis",
            Description = "Information on psychosis including causes, signs and symptoms, treatment, and resources for help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/understanding-psychosis/23-MH-8110-Understanding-Psychosis.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/understanding-psychosis",
            Category = "Psychosis",
        };
        yield return new()
        {
            Title = "Schizophrenia",
            Description = "Information about schizophrenia including symptoms, causes, treatment options, and how to find help for yourself or a loved one.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/schizophrenia/schizophrenia.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/schizophrenia",
            Category = "Schizophrenia",
        };
        yield return new()
        {
            Title = "5 Action Steps for Helping Someone in Emotional Pain",
            Description = "Infographic presents five steps for helping someone in emotional pain in order to prevent suicide. Steps include: Ask, Keep Them Safe, Be There, Help Them Connect, and Stay Connected.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/5-action-steps-for-helping-someone-in-emotional-pain/5-action-steps.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/5-action-steps-for-helping-someone-in-emotional-pain",
            Category = "Suicide",
        };
        yield return new()
        {
            Title = "Frequently Asked Questions About Suicide",
            Description = "Information about suicide including risk factors, symptoms and warning signs, treatment options and therapies, how to find help for yourself or others, and research about suicide and suicide prevention.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/suicide-faq/suicide-faq_0.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/suicide-faq",
            Category = "Suicide",
        };
        yield return new()
        {
            Title = "Warning Signs of Suicide",
            Description = "This infographic presents behaviors and feelings that may be warning signs that someone is thinking about suicide.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/warning-signs-of-suicide/Warning_Signs_of_Suicide.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/warning-signs-of-suicide",
            Category = "Suicide",
        };
        yield return new()
        {
            Title = "Helping Children and Adolescents Cope With Traumatic Events",
            Description = "Information on how children and adolescents respond to traumatic events, and what family, friends, and trusted adults can do to help.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/helping-children-and-adolescents-cope-with-disasters-and-other-traumatic-events/helping-children-and-adolescents-cope-with-traumatic-events.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/helping-children-and-adolescents-cope-with-disasters-and-other-traumatic-events",
            Category = "Traumatic Events",
        };
        yield return new()
        {
            Title = "Post-Traumatic Stress Disorder",
            Description = "Information about post-traumatic stress disorder (PTSD) including what it is, who develops PTSD, symptoms, treatment options, and how to find help for yourself or someone else who may have PTSD.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/post-traumatic-stress-disorder-ptsd/post-traumatic-stress-disorder_1.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/post-traumatic-stress-disorder-ptsd",
            Category = "Traumatic Events",
        };
        yield return new()
        {
            Title = "Disruptive Mood Dysregulation Disorder: The Basics",
            Description = "Learn about disruptive mood dysregulation disorder including what it is, signs and symptoms, how it's diagnosed, treatments, and tips for parents or caregivers.",
            Uri = "https://www.nimh.nih.gov/sites/default/files/documents/health/publications/disruptive-mood-dysregulation-disorder/disruptive-mood-dysregulation-disorder-the-basics.pdf",
            SourceUri = "https://www.nimh.nih.gov/health/publications/disruptive-mood-dysregulation-disorder",
            Category = "Other",
        };
    }
}
