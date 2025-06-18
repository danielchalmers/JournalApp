namespace JournalApp;

public static class WorksheetData
{
    private static IReadOnlyList<Worksheet> _collection;

    public static IReadOnlyList<Worksheet> AllWorksheets => _collection ??= GetAllWorksheets().ToList();

    private static IEnumerable<Worksheet> GetAllWorksheets()
    {
        // All are sourced from https://www.nimh.nih.gov/health/publications/brochures-and-fact-sheets-in-english.
        // The information is in the public domain and may be reused or copied without permission. However, you may not reuse or copy the images.

        yield return new()
        {
            Title = "My Mental Health: Do I Need Help?",
            Description = "Information about how to assess your mental health and determine if you need help. It provides examples of mild and severe symptoms, as well as self-care activities and options for professional help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/my-mental-health-do-i-need-help",
            Category = "",
        };
        yield return new()
        {
            Title = "Generalized Anxiety Disorder: When Worry Gets Out of Control",
            Description = "Information about generalized anxiety disorder including common signs and symptoms, treatment options, and how to find help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/generalized-anxiety-disorder-gad",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "I’m So Stressed Out! Fact Sheet",
            Description = "Intended for teens and young adults and presents information about stress, anxiety, and ways to cope when feeling overwhelmed.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/so-stressed-out-fact-sheet",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "I’m So Stressed Out! Infographic",
            Description = "Infographic about stress, anxiety, and ways to cope when feeling overwhelmed. It was developed for use on social media to highlight the \"I’m So Stressed Out!\" fact sheet.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/so-stressed-out-infographic",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Panic Disorder: When Fear Overwhelms",
            Description = "Information about panic disorder, including common signs and symptoms, treatment options, and how to find help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/panic-disorder-when-fear-overwhelms",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Social Anxiety Disorder: More Than Just Shyness",
            Description = "Information about social anxiety disorder, including common signs and symptoms, treatment options, and how to find help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/social-anxiety-disorder-more-than-just-shyness",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Stand Up to Stress!",
            Description = "Coloring and activity book for children ages 8-12 teaches kids about stress and anxiety and offers tips for coping in a healthy way.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/stand-up-to-stress",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "Stress Catcher",
            Description = "\"Fortune teller\" craft for children that offers coping strategies to help manage stress and other difficult emotions. Instructions on how to create and use the Stress Catcher are provided.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/stress-catcher",
            Category = "Anxiety & Stress",
        };
        yield return new()
        {
            Title = "ADHD in Adults: 4 Things to Know",
            Description = "Information about attention-deficit/hyperactivity disorder (ADHD) in adults including symptoms, diagnosis, treatment options, and resources to find help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/adhd-what-you-need-to-know",
            Category = "ADHD",
        };
        yield return new()
        {
            Title = "Attention-Deficit/Hyperactivity Disorder: What You Need to Know",
            Description = "Information about symptoms, diagnosis, treatment, and resources for children, teens, and adults with attention-deficit/hyperactivity disorder (ADHD).",
            SourceUri = "https://www.nimh.nih.gov/health/publications/attention-deficit-hyperactivity-disorder-what-you-need-to-know",
            Category = "ADHD",
        };
        yield return new()
        {
            Title = "Autism Spectrum Disorder",
            Description = "Describes autism spectrum disorder (ASD): signs and symptoms; causes and risk factors; diagnosis in children, teens, and adults; and treatments.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/autism-spectrum-disorder",
            Category = "Autism",
        };
        yield return new()
        {
            Title = "Bipolar Disorder",
            Description = "Information on bipolar disorder including symptoms, causes, diagnosis, treatment options, and resources to find help for yourself or others.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/bipolar-disorder",
            Category = "Bipolar",
        };
        yield return new()
        {
            Title = "Bipolar Disorder in Children and Teens",
            Description = "Information about bipolar disorder in children and teens including causes, signs and symptoms, diagnosis, treatment options, and how to help a child or teen who has bipolar disorder.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/bipolar-disorder-in-children-and-teens",
            Category = "Bipolar",
        };
        yield return new()
        {
            Title = "Bipolar Disorder in Teens and Young Adults: Know the Signs",
            Description = "This infographic presents common signs and symptoms of bipolar disorder in teens and young adults.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/bipolar-disorder-in-teens-and-young-adults-know-the-signs",
            Category = "Bipolar",
        };
        yield return new()
        {
            Title = "Borderline Personality Disorder",
            Description = "Information about borderline personality disorder, including signs and symptoms, diagnosis, and treatments for borderline personality disorder.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/borderline-personality-disorder",
            Category = "BPD",
        };
        yield return new()
        {
            Title = "Understanding the Link Between Chronic Disease and Depression",
            Description = "Information about the link between depression and chronic disease, including symptoms of depression and resources to find help for yourself or someone else.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/chronic-illness-mental-health",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Depression",
            Description = "Information about depression including the different types of depression, signs and symptoms, how it is diagnosed, treatment options, and how to find help for yourself or a loved one.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/depression",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Depression in Women: 4 Things to Know",
            Description = "Information about depression in women including signs and symptoms, types of depression unique to women, treatment options, and how to find help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/depression-in-women",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Perinatal Depression",
            Description = "Information about perinatal depression, including how it differs from the \"baby blues,\" causes, signs and symptoms, treatment options, and how you or a loved one can get help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/perinatal-depression",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Seasonal Affective Disorder",
            Description = "Information about seasonal affective disorder (SAD), a type of depression. It includes a description of SAD, signs and symptoms, how SAD is diagnosed, causes, and treatment options.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/seasonal-affective-disorder",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Seasonal Affective Disorder (SAD): More Than the Winter Blues",
            Description = "Infographic about how to recognize the symptoms of seasonal affective disorder (SAD) and what to do to get help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/seasonal-affective-disorder-sad-more-than-the-winter-blues",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Teen Depression: More Than Just Moodiness",
            Description = "Intended for teens and young adults and presents information about how to recognize the symptoms of depression and how to get help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/teen-depression",
            Category = "Depression",
        };
        yield return new()
        {
            Title = "Eating Disorders: What You Need to Know",
            Description = "Information about eating disorders including who is at risk, common types of eating disorders and the symptoms of each, treatment options, and resources to find help for yourself or someone else.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/eating-disorders",
            Category = "Eating Disorders",
        };
        yield return new()
        {
            Title = "Let’s Talk About Eating Disorders",
            Description = "Facts that can help shape conversations around eating disorders.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/lets-talk-about-eating-disorders",
            Category = "Eating Disorders",
        };
        yield return new()
        {
            Title = "Obsessive-Compulsive Disorder: When Unwanted Thoughts or Repetitive Behaviors Take Over",
            Description = "Information on obsessive-compulsive disorder (OCD) including signs and symptoms, causes, and treatment options such as psychotherapy and medication.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/obsessive-compulsive-disorder-when-unwanted-thoughts-or-repetitive-behaviors-take-over",
            Category = "OCD",
        };
        yield return new()
        {
            Title = "PANS and PANDAS: Questions and Answers",
            Description = "Information about the causes, signs and symptoms, diagnosis, and treatment of Pediatric Autoimmune Neuropsychiatric Disorders Associated with Streptococcal Infections (PANDAS).",
            SourceUri = "https://www.nimh.nih.gov/health/publications/pandas",
            Category = "OCD",
        };
        yield return new()
        {
            Title = "Understanding Psychosis",
            Description = "Information on psychosis including causes, signs and symptoms, treatment, and resources for help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/understanding-psychosis",
            Category = "Psychosis",
        };
        yield return new()
        {
            Title = "Schizophrenia",
            Description = "Information about schizophrenia including symptoms, causes, treatment options, and how to find help for yourself or a loved one.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/schizophrenia",
            Category = "Schizophrenia",
        };
        yield return new()
        {
            Title = "5 Action Steps to Help Someone Having Thoughts of Suicide",
            Description = "Infographic presents five steps for helping someone in emotional pain: Ask, Keep Them Safe, Be There, Help Them Connect, and Stay Connected.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/5-action-steps-to-help-someone-having-thoughts-of-suicide",
            Category = "Suicide",
        };
        yield return new()
        {
            Title = "Frequently Asked Questions About Suicide",
            Description = "Information about suicide including risk factors, symptoms and warning signs, treatment options and therapies, how to find help for yourself or others, and research about suicide and suicide prevention.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/suicide-faq",
            Category = "Suicide",
        };
        yield return new()
        {
            Title = "Warning Signs of Suicide",
            Description = "This infographic presents behaviors and feelings that may be warning signs that someone is thinking about suicide.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/warning-signs-of-suicide",
            Category = "Suicide",
        };
        yield return new()
        {
            Title = "Helping Children and Adolescents Cope With Traumatic Events",
            Description = "Information on how children and adolescents respond to traumatic events, and what family, friends, and trusted adults can do to help.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/helping-children-and-adolescents-cope-with-disasters-and-other-traumatic-events",
            Category = "Traumatic Events",
        };
        yield return new()
        {
            Title = "Post-Traumatic Stress Disorder",
            Description = "Information about post-traumatic stress disorder (PTSD) including what it is, who develops PTSD, symptoms, treatment options, and how to find help for yourself or someone else who may have PTSD.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/post-traumatic-stress-disorder-ptsd",
            Category = "Traumatic Events",
        };
        yield return new()
        {
            Title = "Disruptive Mood Dysregulation Disorder: The Basics",
            Description = "Learn about disruptive mood dysregulation disorder including what it is, signs and symptoms, how it's diagnosed, treatments, and tips for parents or caregivers.",
            SourceUri = "https://www.nimh.nih.gov/health/publications/disruptive-mood-dysregulation-disorder",
            Category = "Other",
        };
    }
}
