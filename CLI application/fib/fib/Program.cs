
using System.CommandLine;


var bundelOutputOption = new Option<FileInfo>("--output", "-o") { Description = "File path and name" };
var bundelLanguOption = new Option<string[]>("--language", "-l") { Required = true, Description = "choosing code language ", AllowMultipleArgumentsPerToken = true };
var bundelNoteOption = new Option<bool>("--note", "-n") { Description = "write as a note the source of the code" };
var bundelSortOption = new Option<string>("--sort", "-s")
{
    Description = "sort the files: 'name' (alphabetic, default) or 'language'",
    DefaultValueFactory = result => "name"
};
bundelSortOption.AcceptOnlyFromAmong("name", "language");
var bundelRemoveEmptyLinesOption = new Option<bool>("--remove-empty-lines", "-rel") { Description = "delete empty lines from the files" };
var bundelAuthorOption = new Option<string>("--author", "-a") { Description = "write the author" };


var bundler = new Command("bundler", "Bundle code files to a single file");



bundler.Options.Add(bundelLanguOption);
bundler.Options.Add(bundelOutputOption);
bundler.Options.Add(bundelNoteOption);
bundler.Options.Add(bundelSortOption);
bundler.Options.Add(bundelRemoveEmptyLinesOption);
bundler.Options.Add(bundelAuthorOption);


string[] excludedFolders = { "bin", "obj", "debug", "release", "node_modules", ".git", ".vs" };
var languageExtensions = new Dictionary<string, string>
{
    { "cs", ".cs" },
    { "java", ".java" },
    { "python", ".py" },
    { "js", ".js" },
    { "html", ".html" },
    { "css", ".css" },
    { "sql", ".sql" }
};



bundler.SetAction((parseResult) =>
{



    string[] language = parseResult.GetValue(bundelLanguOption);
    FileInfo? output = parseResult.GetValue(bundelOutputOption);
    //StreamWriter writer = new StreamWriter(output.FullName);
    bool note = parseResult.GetValue(bundelNoteOption);
    string? sort = parseResult.GetValue(bundelSortOption);
    bool removeEmptyLines = parseResult.GetValue(bundelRemoveEmptyLinesOption);
    string? author = parseResult.GetValue(bundelAuthorOption);



    if (output == null)
    {
        Console.WriteLine(@"you must enter -o 'YOUR NEWFILE NAME AND PAHT FOR EXAMPLE: C:\folder\bundle.txt");
        return;
    }





    string currentDir = Directory.GetCurrentDirectory();

    string[] allFiles = Directory.GetFiles(currentDir, "*", SearchOption.AllDirectories);

    var filteredFiles = allFiles.Where(file =>
    !file.Split(Path.DirectorySeparatorChar)
         .Any(part => excludedFolders.Contains(part.ToLower()))).ToList();

    List<string> extensions;

    if (language.Contains("all"))
    {
        extensions = languageExtensions.Values.ToList();
    }
    else
    {
        extensions = new List<string>();
        foreach (string lang in language)
        {
            if (!languageExtensions.ContainsKey(lang.ToLower()))
            {
                Console.WriteLine($"Error: language '{lang}' is not supported.");
                return;
            }
            extensions.Add(languageExtensions[lang.ToLower()]);
        }
    }

    var codeFiles = filteredFiles
    .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
    .ToList();

    if (codeFiles.Count == 0)
    {
        Console.WriteLine("No code files found for the selected languages.");
        return;
    }



    //הבדיקה עוברת על כל קובץ שנמצא בתיקייה, ומשאירה ברשימה רק קבצים שעומדים בשני תנאים: הסיומת שלהם שייכת לאחת השפות שהמשתמש ביקש, וגם הם לא הקובץ שאליו אנחנו עומדות לכתוב את ה-bundle.

    if (sort == "language")
    {
         codeFiles = filteredFiles
          .Where(file => extensions.Contains(Path.GetExtension(file).ToLower())
                      && !string.Equals(file, output.FullName, StringComparison.OrdinalIgnoreCase))
          .ToList();
    }
    else
    {
        codeFiles = codeFiles
            .OrderBy(file => Path.GetFileName(file))
            .ToList();
    }

    foreach (string f in codeFiles)
    {
        Console.WriteLine(f);
    }


    try
    {


        // יוצר קובץ חדש, או דורס אם הוא קיים
        using (StreamWriter writer = new StreamWriter(output.FullName))
        {
            if (!string.IsNullOrWhiteSpace(author))
            {
                writer.WriteLine($"// Author: {author}");
                writer.WriteLine();
            }


            foreach (string file in codeFiles)
            {
                if (note)
                {
                    string relativePath = Path.GetRelativePath(currentDir, file);
                    writer.WriteLine($"// Source: {relativePath}");
                }
                string[] lines = File.ReadAllLines(file);

                foreach (string line in lines)
                {
                    if (!(removeEmptyLines && line.Trim() == ""))
                        writer.WriteLine(line);
                    
                }

                writer.WriteLine(); // שורה ריקה בין קובץ לקובץ
            }
        }

        Console.WriteLine($"Bundle created successfully: {output.FullName}");
    }
    catch (IOException ex)
    {
        // למשל: הקובץ פתוח כרגע בתוכנה אחרת ולכן אי אפשר לדרוס אותו
        Console.WriteLine($"Error: could not write the file. {ex.Message}");
    }
    catch (Exception ex)
    {
        // כל שגיאה אחרת שלא צפינו
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }



});


var rootCommand = new RootCommand("Root command for Bundler CLI");

rootCommand.Subcommands.Add(bundler);













var createRsp = new Command("create-rsp", "Create a response file with a ready bundler command");

createRsp.SetAction(parseResult =>
{
    // language - חובה, וכל שפה חייבת להיות נתמכת
    string languageAnswer = "";
    while (true)
    {
        Console.Write("Languages (separated by spaces, or 'all'): ");
        languageAnswer = Console.ReadLine()?.Trim() ?? "";

        if (languageAnswer == "")
        {
            Console.WriteLine("Language is required.");
            continue;
        }

        var invalid = languageAnswer.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.ToLower() != "all" && !languageExtensions.ContainsKey(l.ToLower()))
            .ToList();

        if (invalid.Count > 0)
        {
            Console.WriteLine($"Not supported: {string.Join(", ", invalid)}");
            continue;
        }
        break;
    }

    // output - חובה
    string outputAnswer = "";
    while (outputAnswer == "")
    {
        Console.Write("Output file name or full path: ");
        outputAnswer = Console.ReadLine()?.Trim() ?? "";
        if (outputAnswer == "")
            Console.WriteLine("Output is required.");
    }

    // note - כן/לא
    Console.Write("Add source path as a note? (y/n): ");
    bool noteAnswer = Console.ReadLine()?.Trim().ToLower() == "y";

    // sort - name או language, ריק = ברירת מחדל
    string sortAnswer = "";
    while (true)
    {
        Console.Write("Sort by 'name' or 'language' (Enter for name): ");
        sortAnswer = Console.ReadLine()?.Trim().ToLower() ?? "";
        if (sortAnswer == "" || sortAnswer == "name" || sortAnswer == "language")
            break;
        Console.WriteLine("Please enter 'name' or 'language'.");
    }

    // remove-empty-lines - כן/לא
    Console.Write("Remove empty lines? (y/n): ");
    bool removeAnswer = Console.ReadLine()?.Trim().ToLower() == "y";

    // author - לא חובה
    Console.Write("Author name (Enter to skip): ");
    string authorAnswer = Console.ReadLine()?.Trim() ?? "";

    // בניית הפקודה
    string command = $"bundler -l {languageAnswer} -o \"{outputAnswer}\"";
    if (noteAnswer) command += " -n";
    if (sortAnswer != "") command += $" -s {sortAnswer}";
    if (removeAnswer) command += " -rel";
    if (authorAnswer != "") command += $" -a \"{authorAnswer}\"";

    // שם קובץ ה-rsp
    Console.Write("Response file name (Enter for 'bundle'): ");
    string rspName = Console.ReadLine()?.Trim() ?? "";
    if (rspName == "") rspName = "bundle";
    if (!rspName.EndsWith(".rsp")) rspName += ".rsp";

    // שמירה
    try
    {
        File.WriteAllText(rspName, command);
        Console.WriteLine($"Created {rspName}");
        Console.WriteLine($"Run it with: fib @{rspName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not create the file: {ex.Message}");
    }
});

rootCommand.Subcommands.Add(createRsp);



rootCommand.Parse(args).Invoke();