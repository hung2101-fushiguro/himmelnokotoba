// Backend URL — đổi URL này khi deploy production (Phase 6)
const API_URL = "http://localhost:5191/api/chat";

// Key lưu lịch sử chat trong localStorage (docs/01-PRD.md FR-6)
const STORAGE_KEY = "himmel-chat-history";

// Lịch sử chat giữ trong bộ nhớ (khởi tạo từ localStorage — Render lịch sử
// khi load lại trang sẽ làm ở bước sau). Mỗi request gửi TOÀN BỘ lịch sử này
// lên — đúng nguyên tắc Stateless Backend (docs/02-ARCHITECTURE.md mục 2).
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
