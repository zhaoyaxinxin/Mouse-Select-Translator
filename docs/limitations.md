# Limitations

## Platform limits

- Windows only
- WPF desktop application only
- No macOS support
- No Linux support

## Text extraction limits

- Some apps do not expose `TextPattern` through UI Automation.
- `ValuePattern` fallback can recover textbox content, but it may represent the control value rather than the exact user sub-selection.
- Some apps intercept or block `Ctrl+C`, so clipboard fallback may fail.
- Some PDF readers expose selection inconsistently.
- Some Electron apps only work through the clipboard fallback path.
- Elevated applications may require the translator app to run with matching privileges.

## Edge extension limits

- Browser webpage, textbox, and `contenteditable` extraction now has a dedicated Edge extension path.
- Local `file://` pages and local PDF files require enabling `Allow access to file URLs` for the unpacked extension.
- The extension now also tries `about:blank` and origin-fallback frame injection plus PDF frame selection sampling, but this still depends on how the built-in PDF viewer is hosted.
- Some built-in browser PDF viewers may still block direct extension injection, so those cases can still fall back to the clipboard path.
- If the local loopback bridge on `http://127.0.0.1:48331/` cannot start, browser-extension extraction is unavailable but the desktop extraction chain still works.

## Clipboard limits

- Clipboard restoration is best effort.
- Rich clipboard formats may not always restore exactly as they were.

## Overlay limits

- The overlay is auto-hidden by timer and by explicit `Esc`, but it does not yet track mouse-leave with pixel-perfect behavior.
- Multi-monitor placement depends on the monitor returned by the cursor anchor point at show time.

## Product-scope limits

- OCR fallback is implemented as a local Tesseract adapter, but `tesseract.exe` and `eng.traineddata` are still external runtime prerequisites
- OCR is still last-resort only and does not provide a standalone OCR UI or batch-document workflow
- No history
- No vocabulary notebook
- No cloud sync
- No account system
- No cross-platform packaging

## Validation status

Implemented and automatically verified:

- solution restore
- solution build
- unit tests
- application launch without immediate crash
- local browser bridge health endpoint responds after app startup

Still requiring manual verification:

- drag-selection in Notepad
- drag-selection in a browser
- Edge textbox selection with the unpacked extension
- browser PDF behavior with the unpacked extension and clipboard fallback
- OCR behavior on scanned PDFs and image text after local Tesseract assets are installed
- real UI Automation success/failure behavior per target app
- clipboard restoration quality across apps
