// Backend URL — đổi URL này khi deploy production (Phase 6)
const API_URL = "http://localhost:5191/api/chat";

// Key lưu lịch sử chat trong localStorage (docs/01-PRD.md FR-6)
const STORAGE_KEY = "himmel-chat-history";

// Lịch sử chat giữ trong bộ nhớ, mỗi request gửi TOÀN BỘ lịch sử này lên —
// đúng nguyên tắc Stateless Backend (docs/02-ARCHITECTURE.md mục 2).
let history = [];

function saveHistory(messages) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(messages));
}

function loadHistory() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed : [];
  } catch (error) {
    console.log("Lỗi đọc lịch sử từ localStorage:", error);
    return [];
  }
}

// ── Render (Phase 3 style bubble + Markdown tối giản) ──

function escapeHtml(text) {
  return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

// Parse Markdown tối giản tự viết, hỗ trợ: code block, heading, bullet list,
// **bold**. Nhận input đã escape HTML để tránh XSS.
function renderMarkdown(text) {
  const lines = escapeHtml(text).split("\n");
  const out = [];
  let inCode = false;
  let inList = false;
  const codeBuf = [];

  const closeList = () => {
    if (inList) {
      out.push("</ul>");
      inList = false;
    }
  };

  for (const line of lines) {
    if (line.trim().startsWith("```")) {
      if (inCode) {
        out.push(`<pre><code>${codeBuf.join("\n")}</code></pre>`);
        codeBuf.length = 0;
        inCode = false;
      } else {
        inCode = true;
      }
      continue;
    }

    if (inCode) {
      codeBuf.push(line);
      continue;
    }

    const headingMatch = line.match(/^(#{1,3})\s+(.*)$/);
    if (headingMatch) {
      closeList();
      const level = headingMatch[1].length;
      out.push(`<h${level}>${headingMatch[2]}</h${level}>`);
      continue;
    }

    const bulletMatch = line.match(/^[-*]\s+(.*)$/);
    if (bulletMatch) {
      if (!inList) {
        out.push("<ul>");
        inList = true;
      }
      out.push(`<li>${bulletMatch[1]}</li>`);
      continue;
    }

    if (!line.trim()) {
      closeList();
      continue;
    }

    closeList();
    const bold = line.replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>");
    out.push(`<p>${bold}</p>`);
  }

  if (inCode) {
    out.push(`<pre><code>${codeBuf.join("\n")}</code></pre>`);
  }
  closeList();

  return out.join("\n");
}

function appendMessage(role, content) {
  const chatMessages = document.getElementById("chatMessages");
  const bubble = document.createElement("div");
  bubble.className = role === "user" ? "chat-bubble-user" : "chat-bubble-ai";
  bubble.innerHTML = role === "user" ? escapeHtml(content) : renderMarkdown(content);
  chatMessages.appendChild(bubble);

  const container = document.querySelector(".chat-container");
  if (container) {
    container.scrollTop = container.scrollHeight;
  }
}

// Render toàn bộ lịch sử cũ khi load lại trang (docs/01-PRD.md FR-6)
function renderHistory() {
  if (history.length === 0) return;
  const chatMessages = document.getElementById("chatMessages");
  chatMessages.innerHTML = "";
  for (const msg of history) {
    appendMessage(msg.role === "assistant" ? "ai" : "user", msg.content);
  }
}

// Xóa lịch sử: xóa localStorage + xóa toàn bộ UI (docs/01-PRD.md FR-6)
function clearHistory() {
  if (!confirm("Xóa toàn bộ lịch sử chat?")) return;

  history = [];
  localStorage.removeItem(STORAGE_KEY);

  const chatMessages = document.getElementById("chatMessages");
  chatMessages.innerHTML = `
    <div class="welcome-message">
      <p>Xin chào! Mình là gia sư tiếng Nhật. Hãy nhập câu hỏi bất kỳ bằng Kanji, Hiragana, Katakana, Romaji hoặc tiếng Việt nhé.</p>
    </div>`;
}

document.addEventListener("DOMContentLoaded", () => {
  history = loadHistory();
  renderHistory();

  document.getElementById("clearBtn").addEventListener("click", clearHistory);
});

async function sendMessage(text) {
  const userMessage = { role: "user", content: text };
  history.push(userMessage);
  saveHistory(history);

  try {
    const response = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ messages: history })
    });

    const data = await response.json();

    if (data.success) {
      history.push({ role: "assistant", content: data.content });
      saveHistory(history);
      console.log("AI trả lời:", data.content);
      console.log("Provider dùng:", data.providerUsed);
    } else {
      console.log("Lỗi:", data.error);
    }
  } catch (error) {
    console.log("Lỗi gọi API:", error);
  }
}