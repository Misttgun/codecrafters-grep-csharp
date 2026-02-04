[![progress-banner](https://backend.codecrafters.io/progress/grep/9bebf393-2e07-4832-8cc1-85c8d472cf3a)](https://app.codecrafters.io/users/codecrafters-bot?r=2qF)

This is my C# solution to the
["Build Your Own grep" Challenge](https://app.codecrafters.io/courses/grep/overview).

A minimal `grep`-like tool written in C# (.NET 9) with a small custom regex engine. It can search stdin and/or files for matches, optionally print only the matched portions, and highlight matches with ANSI color.

**Note**: Head over to
[codecrafters.io](https://codecrafters.io) to try the challenge.

## Features

### CLI behavior

- Reads from **stdin** line-by-line and prints matching results
- Can also search **one or more paths** passed as arguments
- Exit codes:
   - `0` if at least one match was found
   - `1` if no matches were found
   - `2` on invalid arguments

### Options (current)

- `-E` — required (constraint from the Codecrafters stages in this repo)
- `-o` — print **only the matched substring(s)** instead of the full line
- `-r` — recursive search:
   - if the first provided path is a directory, searches `*.txt` files under it (recursively)
- `--color=auto|always|never` — colorize matches (ANSI escape codes)
   - `auto` enables color unless output is redirected

### Regex engine (custom)

Implemented via parsing into an AST and matching with backtracking. Supported constructs include:

- Literals, concatenation, alternation: `abc`, `a|b`
- Grouping + capturing: `( ... )`
- Backreferences: `\1`, `\2`, ...
- Character classes: `[abc]`, negated classes `[^abc]`
- Wildcard: `.`
- Anchors: `^` and `$`
- Quantifiers: `*`, `+`, `?`, and `{n}`, `{n,}`, `{n,m}`
- Shorthands:
   - `\w` (ASCII word chars + `_`)
   - `\d` (digits)

## Requirements

- .NET SDK 9.0 (or compatible runtime)

## Usage Examples

> Note: this implementation expects `-E` to be present.

- Read from stdin:
   - `cat README.md | ./your_program.sh -E "grep"`
- Search in a file:
   - `./your_program.sh -E "hello" src/Program.cs`
- Print only matched portions:
   - `./your_program.sh -E -o "\d+" numbers.txt`
- Highlight matches:
   - `./your_program.sh -E --color=always "warn" app.log`
- Recursive search (directory -> `*.txt` only):
   - `./your_program.sh -E -r "pattern" ./some-folder`

## Notes and Limitations

- This is a **challenge implementation**, not a drop-in replacement for GNU/BSD/POSIX `grep`.
- “POSIX grep” refers to a specific standardized tool/behavior; this repo is better described as **`grep`-like**.
- Recursive mode currently enumerates `*.txt` files only, and only when the first provided path is a directory.
- If both stdin and paths are provided, stdin is processed first and then files are processed.

## Project Structure

- `src/Program.cs`
   - CLI parsing (`-E`, `-o`, `-r`, `--color=...`)
   - reads stdin + file(s), runs matching, prints results, sets exit code
- `src/Regex/RegexEngine.cs`
   - compiles a pattern by parsing it into an AST and exposes `MatchAt(...)`
- `src/Regex/Parser.cs`
   - `RegexParser` that parses the pattern string into the AST
- `src/Regex/Ast.cs`
   - AST node definitions (sequence, alternation, groups, literals, quantifiers, anchors, etc.)
- `src/Regex/Matcher.cs`
   - backtracking matcher that walks the AST against input and supports captures/backrefs
- `src/Regex/RuntimeState.cs`
   - match state (current input position + capture dictionary) with snapshot/restore for backtracking
- `your_program.sh`
   - local build+run wrapper used by the Codecrafters workflow

## Development

- Target framework: `net9.0`
- Language version: C# 13
- Build:
   - `dotnet build`
- Run:
   - `dotnet run -- -E "pattern" <path>`
   - or `./your_program.sh -E "pattern" <path>`

## License

MIT (or the license specified by the repository).