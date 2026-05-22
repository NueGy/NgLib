[🏠 Back to documentation](/README.md)
# Development Rules and Project Contributions


Conventions and best practices for development and project contributions. This documentation is intended for all contributors, human or AI, to ensure code consistency and quality.

**IMPORTANT**: Never propose code without first consulting this documentation entirely!


## Development 


### .NET Code Conventions

- **Naming**: PascalCase for classes/methods, camelCase for local variables
- **Files**: one file per class, file name = class name
- **Namespace**: must match the folder structure
- **Slogan**: Keep it simple, do only what is requested.


### Mandatory Rules for AI

- Do not use icons in text and documentation.
- Do not create mocks or supplementary documentation if not requested.
- Wait for validation before modifying code or creating new files.
- This library is used in production — be careful, any change may introduce regressions.
- Before coding, always propose different solutions and verify best practices online.
- When building/compiling, limit output to errors only: `dotnet build xxx.csproj -v quiet`
- Reduce code size: `if` without braces when possible, static functions to avoid duplication.
- Keep it simple, do only what is requested.
 

### Standard Project Structure

```
dev/         .NET source code (solutions, projects)
docs/        Documentation (wiki_*.md)
publish/     Publication artifacts (gitignore)
tech/        PowerShell tooling scripts
tmp/         Temporary files (gitignore)
```

### Tests

Always create unit tests in `dev/Nglib.PublicTests` to validate changes and document functionality.
- Tests must be clear, concise and cover only relevant use cases.
- Do not multiply test methods; prefer grouping scenarios for simplicity and readability.
- Group scenarios with section comments `// === Feature Tests ===` and include descriptive assertion messages.
- ClassName: xxxxxTests, MethodNames: xxxxxxTest.


## Plans
### Fix Bugs and Issues

When fixing a bug, you must first analyze the root cause and understand the issue thoroughly. Check if existing unit tests can reproduce the bug; if not, create a minimal test case that demonstrates the problem. Once the analysis is complete, provide a detailed report including the cause, proposed solution, and impact assessment. Wait for explicit validation before modifying any code or implementing the fix.
