# File and Folder Rules

LabDocs reads documentation from the `content` directory in the application's content root.

## Content discovery

- The `content` directory must exist when the application starts.
- Markdown files use the `.md` extension.
- Subfolders are searched recursively.
- Files outside the `content` directory are not served as documentation pages.
- Symbolic links and directory junctions are rejected.

## Naming convention

Every filename and folder name must use lowercase kebab-case. Each path segment may contain lowercase letters and numbers separated by single hyphens.

| Valid | Invalid |
| --- | --- |
| `release-notes.md` | `Release Notes.md` |
| `api-version-2.md` | `api_version_2.md` |
| `guides/getting-started.md` | `guides/getting--started.md` |

The `.md` extension belongs in the filesystem but is omitted from the page URL.

> A discovered file with an invalid name prevents LabDocs from building its navigation. Rename the file or folder so it follows the convention.

The content directory should only be writable by trusted administrators.
