# 📦 fib — Code Bundler CLI

A command-line tool built with **C# / .NET** and **System.CommandLine** that packs the source code files of a project into a single, readable bundle file. Handy for code reviews, submitting assignments, or sharing a whole project as one file.

---

## ✨ Features

- **Bundle by language**: include only the languages you choose, or everything with `all`
- **Smart filtering**: skips build and tooling folders such as `bin`, `obj`, `debug`, `release`, `node_modules`, `.git`, `.vs` and `publish`
- **Sorting**: order files alphabetically by name, or grouped by language
- **Source notes**: optionally write the relative path of each file above its code
- **Clean output**: optionally remove empty lines
- **Author header**: optionally write the author's name at the top of the bundle
- **Response files**: an interactive `create-rsp` command builds a ready-to-run command file for you
- **Input validation**: clear error messages for unsupported languages, missing options, invalid paths and more
- **Short aliases** for every option

---

## 🚀 Installation

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (9.0 or later)

### 1. Clone the repository
```bash
git clone <your-repository-url>
cd <repository-folder>/fib
```

### 2. Publish the executable
```bash
dotnet publish -c Release -o publish
```

### 3. Add it to your PATH (Windows)
1. Search Windows for **"Environment Variables"** and open *Edit environment variables for your account*.
2. Select **Path** → **Edit** → **New**.
3. Paste the full path of the `publish` folder created in step 2.
4. Click **OK** and open a **new** terminal.

Verify the installation:
```bash
fib --help
```

You can now run `fib` from any folder on your computer.

---

## 📖 Usage

### `bundle` — pack code files into a single file

Run the command from the root folder of the project you want to bundle:

```bash
fib bundle --language <languages> --output <file> [options]
```

| Option | Alias | Description | Required |
|---|---|---|---|
| `--language` | `-l` | One or more languages to include, separated by spaces, or `all` | ✅ |
| `--output` | `-o` | Output file name, or a full path | ✅ |
| `--note` | `-n` | Write each file's relative path as a comment above its code | |
| `--sort` | `-s` | `name` (default) or `language` | |
| `--remove-empty-lines` | `-rel` | Remove empty lines from the code | |
| `--author` | `-a` | Write the author's name at the top of the bundle | |

#### Examples

Bundle all C# files:
```bash
fib bundle -l cs -o bundle.txt
```

Bundle C# and JavaScript files, sorted by language, with source notes and an author:
```bash
fib bundle -l cs js -o bundle.txt -s language -n -a "Leah"
```

Bundle every supported file, without empty lines, to a full path:
```bash
fib bundle -l all -o "C:\My Projects\full-bundle.txt" -rel
```

> 💡 If the output path contains spaces, wrap it in quotes.

---

### `create-rsp` — build a response file interactively

Long commands are tiring to type. `create-rsp` asks you a question for each option, validates the answers, and saves a complete `bundle` command to a `.rsp` file.

```bash
fib create-rsp
```

Example session:
```
Languages (separated by spaces, or 'all'): cs js
Output file name or full path: bundle.txt
Add source path as a note? (y/n): y
Sort by 'name' or 'language' (Enter for name): language
Remove empty lines? (y/n): y
Author name (Enter to skip): Leah
Response file name (Enter for 'bundle'):
Created bundle.rsp
```

Run the saved command any time:
```bash
fib @bundle.rsp
```

> ⚠️ **PowerShell users:** `@` has a special meaning in PowerShell, so wrap the argument in single quotes:
> ```powershell
> fib '@bundle.rsp'
> ```

---

## 🗂 Supported Languages

| Value | Extension |
|---|---|
| `cs` | `.cs` |
| `java` | `.java` |
| `python` | `.py` |
| `js` | `.js` |
| `html` | `.html` |
| `css` | `.css` |
| `sql` | `.sql` |
| `all` | all of the above |

---

## 🛡 Validation & Error Handling

| Situation | Behavior |
|---|---|
| `--language` missing | Error from the parser, with usage help |
| Unsupported language | `Error: language '<name>' is not supported.` |
| Invalid `--sort` value | Error listing the allowed values |
| `--output` missing | Message explaining how to provide an output path |
| No matching files found | `No code files found for the selected languages.` |
| Folder in output path doesn't exist, no permission, file locked | Friendly error message, no crash |
| Output file has a code extension | The bundle is never included inside itself |

If the output file already exists, it is overwritten.

---

## 📄 Example Output

Running `fib bundle -l cs -o bundle.txt -n -a "Leah"` produces something like:

```csharp
// Author: Leah

// Source: Models/User.cs
public class User
{
    public string Name { get; set; }
}

// Source: Program.cs
Console.WriteLine("Hello, World!");
```

---

## 🧰 Built With

- **C#** / **.NET**
- **[System.CommandLine](https://github.com/dotnet/command-line-api)**: command, option and alias parsing, built-in help, and response-file support
- **System.IO**: file discovery, reading and writing
- **LINQ**: filtering and sorting

---

## 👩‍💻 Author

**Leah**, Practicode project
