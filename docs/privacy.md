# Privacy

## Default behavior

- The app only reacts to text the user actively selects.
- Selected source text is not saved to disk by default.
- Translation results are not saved to disk by default.
- The app does not write raw selected text or translated text to logs.
- Password fields are skipped by the UI Automation path.

## Clipboard fallback

When UI Automation cannot read the selection, the app can simulate `Ctrl+C` and read the clipboard temporarily.

Protection measures:

- Existing clipboard data is captured before fallback extraction.
- The app attempts to restore the previous clipboard content immediately after reading.
- Clipboard content is not logged.

Important limitation:

- Clipboard restoration is best effort. Complex formats cannot be guaranteed to restore perfectly in every app.

## Online translation

If `Translation.Provider` is set to `OpenAICompatible`, the selected text is sent to the configured translation API.

This means:

- selected text leaves the local machine
- privacy then depends on the configured provider
- API keys must be provided through an environment variable, not committed to the repository

## Blacklist

The settings file contains an `AppBlacklist` list. If the foreground process name matches a blacklisted executable name, the selection pipeline is skipped.

## Data storage

Persistent local data is limited to:

- the JSON settings file under `%AppData%\MouseSelectTranslator\settings.json`

The in-memory translation cache is not persisted.
