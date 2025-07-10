# Contributing to FluentUIScaffold

Thank you for your interest in contributing to FluentUIScaffold!

## Developer Quick Start

1. **Clone the repository:**
   ```sh
   git clone https://github.com/fluent-ui-scaffold/fluent-ui-scaffold.git
   cd fluent-ui-scaffold
   ```

2. **Install .NET SDK:**
   - .NET 6.0, 7.0, 8.0, or 9.0 (multi-target supported)
   - [Download .NET SDK](https://dotnet.microsoft.com/download)

3. **Restore dependencies:**
   ```sh
   dotnet restore
   ```

4. **Build the solution:**
   ```sh
   dotnet build --configuration Release
   ```

5. **Run tests:**
   ```sh
   dotnet test --configuration Release
   ```

6. **Run code format check:**
   ```sh
   dotnet format --verify-no-changes
   ```

## Contribution Guidelines

- **Branching:**
  - Create a feature branch from `main` for your work.
  - Use descriptive branch names (e.g., `feature/fluent-api`, `bugfix/element-wait`).

- **Commits:**
  - Write clear, concise commit messages.
  - Group related changes into a single commit when possible.

- **Pull Requests:**
  - Ensure all builds and tests pass before submitting a PR.
  - Reference the relevant story or issue in your PR description.
  - Add or update documentation as needed.
  - Request a review from a maintainer.

- **Coding Style:**
  - Follow the `.editorconfig` and code style enforced by the repository.
  - Run `dotnet format` before pushing changes.

- **CI/CD:**
  - All PRs are validated by GitHub Actions (build, test, code quality, security).

## Need Help?
- Check the [docs/roadmap/README.md](docs/roadmap/README.md) for project structure and roadmap.
- Open an issue or discussion for questions or suggestions.

Happy coding! 