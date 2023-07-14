namespace JournalApp;

public class DataPointCategory
{
    [Key]
    public Guid Guid { get; set; }

    public string Group { get; set; }

    public string Name { get; set; }

    public int Index { get; set; }

    public bool Enabled { get; set; } = true;

    public DataType Type { get; set; }

    public decimal? MedicationDose { get; set; }

    public string MedicationUnit { get; set; }

    public bool MedicationEveryDay { get; set; }

    public virtual HashSet<DataPoint> DataPoints { get; set; } = new();

    public override string ToString() => $"{string.Join("|", Group, Name)} #{Index}";
}

public class DataPoint
{
    [Key]
    public Guid Guid { get; set; }

    public virtual Day Day { get; set; }

    public virtual DataPointCategory Category { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DataType DataType { get; set; }

    public string Mood { get; set; }
    public decimal? SleepHours { get; set; }
    public int ScaleIndex { get; set; }
    public bool? Bool { get; set; }
    public double? Number { get; set; }
    public string Text { get; set; }
    public decimal? Dose { get; set; }
    public string Unit { get; set; }

    public override string ToString() => $"{DataType} ({Category})";

    public static IReadOnlyList<string> Moods { get; } = new[] { "🤔", "😁", "😀", "🙂", "😐", "🙁", "😢", "😭", };

    public static IReadOnlyCollection<string> Medications { get; } = new[]
    {
        "Atorvastatin",
        "Levothyroxine",
        "Metformin",
        "Lisinopril",
        "Amlodipine",
        "Metoprolol",
        "Albuterol",
        "Omeprazole",
        "Losartan",
        "Gabapentin",
        "Hydrochlorothiazide",
        "Sertraline",
        "Simvastatin",
        "Montelukast",
        "Escitalopram",
        "Acetaminophen",
        "Rosuvastatin",
        "Bupropion",
        "Furosemide",
        "Pantoprazole",
        "Trazodone",
        "Dextroamphetamine",
        "Fluticasone",
        "Tamsulosin",
        "Fluoxetine",
        "Carvedilol",
        "Duloxetine",
        "Meloxicam",
        "Clopidogrel",
        "Prednisone",
        "Citalopram",
        "Insulin",
        "Potassium",
        "Pravastatin",
        "Tramadol",
        "Aspirin",
        "Alprazolam",
        "Ibuprofen",
        "Cyclobenzaprine",
        "Amoxicillin",
        "Methylphenidate",
        "Allopurinol",
        "Venlafaxine",
        "Clonazepam",
        "Ethinyl",
        "Ergocalciferol",
        "Zolpidem",
        "Apixaban",
        "Glipizide",
        "Hydrochlorothiazide",
        "Spironolactone",
        "Cetirizine",
        "Atenolol",
        "Oxycodone",
        "Buspirone",
        "Fluticasone",
        "Topiramate",
        "Warfarin",
        "Estradiol",
        "Cholecalciferol",
        "Budesonide",
        "Lamotrigine",
        "Ethinyl",
        "Quetiapine",
        "Lorazepam",
        "Famotidine",
        "Folic",
        "Azithromycin",
        "Acetaminophen",
        "Hydroxyzine",
        "Insulin",
        "Diclofenac",
        "Loratadine",
        "Sitagliptin",
        "Clonidine",
        "Diltiazem",
        "Latanoprost",
        "Pregabalin",
        "Doxycycline",
        "Insulin",
        "Amitriptyline",
        "Paroxetine",
        "Ondansetron",
        "Tizanidine",
        "Lisdexamfetamine",
        "Rivaroxaban",
        "Glimepiride",
        "Propranolol",
        "Aripiprazole",
        "Finasteride",
        "Naproxen",
        "Levetiracetam",
        "Hydrochlorothiazide",
        "Alendronate",
        "Fenofibrate",
        "Dulaglutide",
        "Oxybutynin",
        "Celecoxib",
        "Lovastatin",
        "Ezetimibe",
        "Cephalexin",
        "Empagliflozin",
        "Hydralazine",
        "Mirtazapine",
        "Cyanocobalamin",
        "Triamcinolone",
        "Amoxicillin",
        "Baclofen",
        "Valproate",
        "Tiotropium",
        "Sumatriptan",
        "Donepezil",
        "Methotrexate",
        "Isosorbide",
        "Fluticasone",
        "Ferrous",
        "Thyroid",
        "Acetaminophen",
        "Valacyclovir",
        "Desogestrel",
        "Sulfamethoxazole",
        "Esomeprazole",
        "Valsartan",
        "Insulin",
        "Clindamycin",
        "Hydroxychloroquine",
        "Methocarbamol",
        "Diazepam",
        "Semaglutide",
        "Dexmethylphenidate",
        "Hydrochlorothiazide",
        "Ciprofloxacin",
        "Chlorthalidone",
        "Rizatriptan",
        "Nifedipine",
        "Insulin",
        "Norethindrone",
        "Risperidone",
        "Olmesartan",
        "Morphine",
        "Benazepril",
        "Meclizine",
        "Timolol",
        "Oxcarbazepine",
        "Drospirenone",
        "Liraglutide",
        "Dicyclomine",
        "Irbesartan",
        "Hydrocortisone",
        "Albuterol",
        "Verapamil",
        "Memantine",
        "Prednisolone",
        "Metformin",
        "Nortriptyline",
        "Ropinirole",
        "Benzonatate",
        "Progesterone",
        "Ethinyl",
        "Mirabegron",
        "Methylprednisolone",
        "Acyclovir",
        "Docusate",
        "Olanzapine",
        "Nitroglycerin",
        "Bimatoprost",
        "Nitrofurantoin",
        "Pioglitazone",
        "Amlodipine",
        "Ketoconazole",
        "Clobetasol",
        "Testosterone",
        "Azelastine",
        "Fluconazole",
        "Brimonidine",
        "Desvenlafaxine",
        "Ranitidine",
        "Oseltamivir",
        "Levocetirizine",
        "Anastrozole",
        "Phentermine",
        "Sucralfate",
        "Sildenafil",
        "Mesalamine",
        "Carbamazepine",
        "Buprenorphine",
        "Acetaminophen",
        "Flecainide",
        "Gemfibrozil",
        "Prazosin",
        "Lansoprazole",
        "Diphenhydramine",
        "Pramipexole",
        "Ethinyl",
        "Dorzolamide",
        "Ramipril",
        "Lithium",
        "Amiodarone",
        "Omega",
        "Glyburide",
        "Acetaminophen",
        "Magnesium",
        "Mupirocin",
        "Calcium",
        "Adalimumab",
        "Methimazole",
        "Budesonide",
        "Promethazine",
        "Doxazosin",
        "Labetalol",
        "Terazosin",
        "Cyclosporine",
        "Torsemide",
        "Medroxyprogesterone",
        "Calcium",
        "Dorzolamide",
        "Dapagliflozin",
        "Liothyronine",
        "Sacubitril",
        "Beclomethasone",
        "Insulin",
        "Metronidazole",
        "Temazepam",
        "Fluticasone",
        "Erythromycin",
        "Polyethylene",
        "Nystatin",
        "Cefdinir",
        "Benztropine",
        "Tretinoin",
        "Mometasone",
        "Eszopiclone",
        "Betamethasone",
        "Erenumab",
        "Hydrochlorothiazide",
        "Minocycline",
        "Digoxin",
        "Empagliflozin",
        "Nebivolol",
        "Levofloxacin",
        "Colchicine",
        "Ofloxacin",
        "Vortioxetine",
        "Linaclotide",
        "Umeclidinium",
        "Insulin",
        "Ticagrelor",
        "Telmisartan",
        "Ketorolac",
        "Hydromorphone",
        "Epinephrine",
        "Doxepin",
        "Quinapril",
        "Umeclidinium",
        "Fexofenadine",
        "Brimonidine",
        "Letrozole",
        "Ranolazine",
        "Lurasidone",
        "Phenytoin",
        "Tadalafil",
        "Pancrelipase",
        "Dexlansoprazole",
        "Isotretinoin",
        "Sodium",
        "Solifenacin",
        "Bisoprolol",
        "Olopatadine",
        "Primidone",
        "Bumetanide",
        "Tolterodine",
        "Dexamethasone",
        "Chlorhexidine",
        "Sodium",
        "Varenicline",
        "Zonisamide",
        "Calcitriol",
        "Emtricitabine",
        "Terbinafine",
        "Fluocinonide",
        "Hydrochlorothiazide",
        "Ziprasidone",
        "Estrogens",
        "Sulfasalazine",
        "Icosapent",
        "Dexamethasone",
        "Atomoxetine",
        "Formoterol",
        "Ketotifen",
        "Bisoprolol",
        "Sennosides",
        "Raloxifene",
        "Linagliptin",
        "Canagliflozin",
        "Alogliptin",
        "Sotalol",
        "Potassium",
        "Melatonin",
        "Isosorbide",
        "Guanfacine",
    };
}

public enum DataType
{
    [Description("Empty")]
    None,

    [Description("Mood emoji")]
    Mood,

    [Description("Sleep duration")]
    Sleep,

    [Description("1-5 scale")]
    Scale,

    [Description("Yes or no")]
    Bool,

    [Description("Decimal number")]
    Number,

    [Description("Short text")]
    Text,

    [Description("Long note")]
    Note,

    [Description("Medication")]
    Medication,
}