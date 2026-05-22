(() => {
  const selectionMessageType = "mouse-select-translator.selection";
  const dispatchDelayMs = 60;
  const recentCopyMaxAgeMs = 1500;
  let pendingTimer = null;
  let recentCopiedText = "";
  let recentCopiedAt = 0;

  const isTextInput = (element) => {
    if (!(element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement)) {
      return false;
    }

    return typeof element.selectionStart === "number" && typeof element.selectionEnd === "number";
  };

  const getInputSelectionText = (element) => {
    if (!isTextInput(element)) {
      return "";
    }

    const start = element.selectionStart ?? 0;
    const end = element.selectionEnd ?? 0;
    if (end <= start) {
      return "";
    }

    return element.value.slice(start, end).trim();
  };

  const getDomSelectionText = () => {
    const selection = window.getSelection();
    return selection ? selection.toString().trim() : "";
  };

  const getSelectionTextFromRoot = (root) => {
    try {
      const selection = root.getSelection?.();
      const text = selection ? selection.toString().trim() : "";
      if (text) {
        return text;
      }
    } catch {
    }

    const activeElement = root.activeElement;
    if (isTextInput(activeElement)) {
      const text = getInputSelectionText(activeElement);
      if (text) {
        return text;
      }
    }

    const editableElement = activeElement instanceof HTMLElement && activeElement.isContentEditable
      ? activeElement
      : null;
    if (editableElement) {
      try {
        const selection = editableElement.ownerDocument?.defaultView?.getSelection?.();
        const text = selection ? selection.toString().trim() : "";
        if (text) {
          return text;
        }
      } catch {
      }
    }

    const descendants = root.querySelectorAll ? Array.from(root.querySelectorAll("*")) : [];
    for (const element of descendants) {
      if (!element.shadowRoot) {
        continue;
      }

      const text = getSelectionTextFromRoot(element.shadowRoot);
      if (text) {
        return text;
      }
    }

    return "";
  };

  const getDocumentSelectionText = (doc) => {
    try {
      const selection = doc.defaultView?.getSelection?.();
      return selection ? selection.toString().trim() : "";
    } catch {
      return "";
    }
  };

  const getFrameSelectionText = () => {
    const frames = Array.from(document.querySelectorAll("iframe, frame"));
    for (const frame of frames) {
      try {
        const frameDocument = frame.contentDocument;
        if (!frameDocument) {
          continue;
        }

        const text = getSelectionTextFromRoot(frameDocument);
        if (text) {
          return text;
        }
      } catch {
      }
    }

    return "";
  };

  const captureCopiedText = (event) => {
    const copiedText = event.clipboardData?.getData("text/plain")?.trim() ?? "";
    if (!copiedText) {
      return;
    }

    recentCopiedText = copiedText;
    recentCopiedAt = Date.now();
  };

  const getRecentCopiedText = () => {
    if (!recentCopiedText) {
      return "";
    }

    if (Date.now() - recentCopiedAt > recentCopyMaxAgeMs) {
      recentCopiedText = "";
      recentCopiedAt = 0;
      return "";
    }

    return recentCopiedText;
  };

  const isPdfLikeDocument = () => {
    const href = window.location.href.toLowerCase();
    return document.contentType === "application/pdf"
      || href.endsWith(".pdf")
      || document.querySelector("embed[type='application/pdf'], iframe[src$='.pdf']") !== null;
  };

  const buildPayload = () => {
    const activeElement = document.activeElement;
    const isPdf = isPdfLikeDocument();
    let text = "";
    let source = "Edge.Extension";

    if (isPdf) {
      text = getRecentCopiedText();
      source = "Edge.Extension.Pdf.Copy";
    }

    if (!text && isTextInput(activeElement)) {
      text = getInputSelectionText(activeElement);
      source = "Edge.Extension.Input";
    }

    if (!text && activeElement instanceof HTMLElement && activeElement.isContentEditable) {
      text = getDomSelectionText();
      source = "Edge.Extension.ContentEditable";
    }

    if (!text) {
      text = getDomSelectionText();
      source = isPdf ? "Edge.Extension.Pdf" : "Edge.Extension";
    }

    if (!text && isPdf) {
      text = getSelectionTextFromRoot(document);
      source = "Edge.Extension.Pdf";
    }

    if (!text && isPdf) {
      text = getFrameSelectionText();
      source = "Edge.Extension.Pdf";
    }

    if (!text) {
      return null;
    }

    return {
      text,
      source,
      url: window.location.href,
      title: document.title || null,
      isPdf,
      capturedAtUtc: new Date().toISOString()
    };
  };

  const dispatchSelection = () => {
    pendingTimer = null;
    const payload = buildPayload();
    if (!payload) {
      return;
    }

    chrome.runtime.sendMessage({
      type: selectionMessageType,
      payload
    });
  };

  const scheduleDispatch = () => {
    if (pendingTimer !== null) {
      window.clearTimeout(pendingTimer);
    }

    pendingTimer = window.setTimeout(dispatchSelection, dispatchDelayMs);
  };

  document.addEventListener("mouseup", scheduleDispatch, true);
  document.addEventListener("selectionchange", scheduleDispatch, true);
  document.addEventListener("copy", (event) => {
    captureCopiedText(event);
    scheduleDispatch();
  }, true);
  document.addEventListener("keyup", (event) => {
    if (event.key.startsWith("Arrow") || event.key === "Shift" || (event.ctrlKey && event.key.toLowerCase() === "a")) {
      scheduleDispatch();
    }
  }, true);
})();
