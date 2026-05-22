# Manual Test Checklist

## Build and startup

- [ ] `dotnet build MouseSelectTranslator.slnx` succeeds
- [ ] `dotnet test MouseSelectTranslator.slnx` succeeds
- [ ] The application starts and stays alive for at least a few seconds
- [ ] The local browser bridge responds at `http://127.0.0.1:48331/health`

## Edge extension setup

- [ ] Open `edge://extensions`
- [ ] Enable `Developer mode`
- [ ] Load the unpacked `edge-extension` folder
- [ ] If local files or local PDFs are needed, enable `Allow access to file URLs`

## MVP selection flow

- [ ] Launch the app
- [ ] Open Notepad
- [ ] Drag-select an English sentence
- [ ] Release the mouse button
- [ ] Confirm an overlay appears near the cursor
- [ ] Confirm the overlay text contains Chinese translation when `Provider=DeepSeek`
- [ ] If `Provider=Mock`, confirm the overlay text contains the mock translation prefix

## Browser flow

- [ ] Open a browser page with selectable English text
- [ ] Drag-select text on the page
- [ ] Release the mouse button
- [ ] Confirm an overlay appears near the cursor
- [ ] Repeat the test inside an Edge `input` or `textarea`
- [ ] Repeat the test inside an editable rich-text area (`contenteditable`) if available
- [ ] If testing PDF in Edge, verify whether the page is handled by the extension path or by clipboard fallback
- [ ] If testing PDF in Edge, reload the unpacked extension after updates before retesting

## Input behavior

- [ ] Single click does not trigger translation
- [ ] Repeating the same selection quickly does not trigger repeated translation calls
- [ ] Pressing `Esc` hides the overlay
- [ ] Pressing `Ctrl+Shift+T` pauses translation
- [ ] Pressing `Ctrl+Shift+T` again resumes translation

## Tray behavior

- [ ] Tray icon appears after app startup
- [ ] Tray menu can pause/resume the feature
- [ ] Tray menu can exit the application

## Privacy and safety

- [ ] Clipboard content is restored after clipboard fallback extraction
- [ ] Password-box content is not translated through the UI Automation path
- [ ] No raw selected text is written to logs

## Real API flow

- [ ] Set `Translation.Provider` to `DeepSeek`
- [ ] Set `BaseUrl`, `Model`, and the API key environment variable
- [ ] Relaunch the app
- [ ] Drag-select text
- [ ] Confirm a real translation appears instead of the mock prefix

## Known manual-only checks

- [ ] Validate behavior in at least one app where UI Automation works
- [ ] Validate behavior in at least one app where clipboard fallback is required
- [ ] Validate behavior in Edge with the unpacked extension enabled
- [ ] Validate behavior on a local `file://` page after enabling `Allow access to file URLs`
- [ ] Validate behavior on at least one PDF sample in Edge
- [ ] If OCR fallback is enabled, confirm `tesseract.exe` can be discovered and `assets/ocr/tessdata/eng.traineddata` exists under the app output
- [ ] If OCR fallback is enabled, validate one scanned PDF or image-text sample that fails the normal extraction chain
- [ ] Validate behavior against one blacklisted process entry
