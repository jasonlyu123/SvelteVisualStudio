## (WIP) Svelte for Visual Studio

This extension is a language server client for Visual Studio 2019. Power by [Svelte Language Server](https://github.com/sveltejs/language-tools/) 
with [language server protocol](https://microsoft.github.io/language-server-protocol/)

### Configuration

Create a `VSWorkspaceSettings.json` in the `.vs` folder in the folder you want the config to apply to.
Complete list see [Svelte for VSCode](https://github.com/sveltejs/language-tools/tree/master/packages/svelte-vscode#settings)
Note that not all feature is supported in Visual Studio.

```json
// /.vs/VSWorkspaceSettings.json
{
    "svelte.plugin.css.hover.enable": true,
    "typescript.preferences.quoteStyle": "single"
}
```

### Known feature doesn't work on Visual Studio but on VSCode

| Feature                                        |             Reason              |
| ---------------------------------------------- | :-----------------------------: |
| Semantic Tokens                                | VS implemention is non standard |
| Linked Editing                                 | VS implemention is non standard |
| Document Color and Color Picker                |      VS doesn't support it      |
| Smart Select (Selection Range)                 |      VS doesn't support it      |
| Completion Snippet                             |      VS doesn't support it      |
| Document Highlight (Highlight All Occurrences) |             Unknown             |
| Comment Hotkey in JS/CSS                       |             Unknown             |
| Smart Indentation                              |             Unknown             |
| Update Import                                  |         Not implemented         |
| Reflect js/ts update without file save         |         Not implemented         |
