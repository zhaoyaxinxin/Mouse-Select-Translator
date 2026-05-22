const endpoint = "http://127.0.0.1:48331/selection";
const selectionMessageType = "mouse-select-translator.selection";

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (!message || message.type !== selectionMessageType || !message.payload) {
    return false;
  }

  void fetch(endpoint, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(message.payload)
  })
    .then(() => sendResponse({ ok: true }))
    .catch((error) => {
      console.debug("Mouse Select Translator bridge request failed.", error, sender?.url);
      sendResponse({ ok: false });
    });

  return true;
});
