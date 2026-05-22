# Local OCR Assets

Place Tesseract language data files here.

Minimum file required for the current implementation:

- `eng.traineddata`

The app looks for OCR assets in:

- `assets/ocr/tessdata` under the application output directory
- `MOUSE_TRANSLATOR_TESSDATA_PATH` if you want to override the location

The OCR adapter also needs a Tesseract executable. It resolves in this order:

1. `MOUSE_TRANSLATOR_TESSERACT_PATH`
2. `assets/ocr/tesseract/tesseract.exe` under the application output directory
3. `tesseract.exe` on `PATH`
4. `C:\Program Files\Tesseract-OCR\tesseract.exe`
5. `C:\Program Files (x86)\Tesseract-OCR\tesseract.exe`
