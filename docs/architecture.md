# Architecture

## Layers

### `MouseTranslator.App`

Responsibilities:

- WPF application startup
- Overlay window rendering
- Tray icon and context menu
- Global hotkey registration
- Dependency composition

Key types:

- `ApplicationController`
- `CompositionRoot`
- `OverlayPresenter`
- `OverlayWindow`
- `GlobalHotkeyManager`
- `TrayManager`

### `MouseTranslator.Core`

Responsibilities:

- Selection workflow orchestration
- Text validation
- Duplicate suppression
- Translation cache
- Overlay placement math
- Cross-layer interfaces

Key types:

- `SelectionCoordinator`
- `TextValidationService`
- `TranslationCache`
- `OverlayPlacementCalculator`
- `IMouseSelectionMonitor`
- `ITextExtractor`
- `ITranslationService`
- `IOverlayPresenter`

### `MouseTranslator.Infrastructure`

Responsibilities:

- Win32 mouse hook
- Foreground process inspection
- SendInput keyboard simulation
- UI Automation extraction
- Clipboard fallback extraction
- Settings storage
- Translation providers

Key types:

- `Win32MouseHook`
- `SendInputService`
- `ForegroundWindowInfoProvider`
- `UIAutomationTextExtractor`
- `ClipboardTextExtractor`
- `CompositeTextExtractor`
- `JsonSettingsStore`
- `MockTranslationService`
- `OpenAICompatibleTranslationService`

## End-to-end flow

1. `Win32MouseHook` detects a drag-selection gesture.
2. `SelectionCoordinator` waits for the selection to stabilize.
3. `CompositeTextExtractor` tries UI Automation first.
4. If UI Automation fails, it falls back to simulated `Ctrl+C` and clipboard extraction.
5. `TextValidationService` rejects empty, too short, too long, or sensitive-looking text.
6. `SelectionCoordinator` checks the in-memory `TranslationCache`.
7. If needed, it calls the selected translation provider.
8. `OverlayPresenter` positions and shows `OverlayWindow` near the cursor.
9. Tray and hotkeys can pause the pipeline or hide the overlay.

## Runtime decisions

- The app defaults to the `Mock` provider, so the MVP works without network access.
- Real translation is enabled by settings only.
- Selected text is not persisted to disk.
- The cache is in-memory only and disappears when the app exits.
