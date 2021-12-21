## Svelte for Visual Studio

This extension is a language server client for [Visual Studio 2019](https://marketplace.visualstudio.com/items?itemName=lyu-jason.svelte-vs) and [Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=lyu-jason.svelte-vs-2022). Power by [Svelte Language Server](https://github.com/sveltejs/language-tools/)
with [language server protocol](https://microsoft.github.io/language-server-protocol/). 

Viusal Studio 2022 fixes a lot of bug related to LSP. Thus it's recommanded to use the VS2022 version.  

### Configuration

Create a `VSWorkspaceSettings.json` in the `.vs` folder in the folder you want the config to apply to.
Complete list see [Svelte Language Server](https://github.com/sveltejs/language-tools/tree/master/packages/language-server#list-of-settings)
Note that not all feature is supported in Visual Studio.

```json
// /.vs/VSWorkspaceSettings.json
{
    "svelte.plugin.css.hover.enable": true,
    "typescript.preferences.quoteStyle": "single"
}
```

### Known feature doesn't work on Visual Studio but on VSCode

See [#6](https://github.com/jasonlyu123/SvelteVisualStudio/issues/6)
