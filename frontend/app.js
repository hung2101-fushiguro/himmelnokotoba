// Backend URL — đổi URL này khi deploy production (Phase 6)
const API_URL = "http://localhost:5191/api/chat";

// Lịch sử chat giữ trong bộ nhớ tạm thời (sẽ thay bằng localStorage ở Phase 4).
// Mỗi request gửi TOÀN BỘ lịch sử này lên — đúng nguyên tắc Stateless Backend
// (docs/02-ARCHITECTURE.md mục 2).
let history = [];

async function sendMessage(text) {
  const userMessage = { role: "user", content: text };
  history.push(userMessage);

  try {
    const response = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ messages: history })
    });

    const data = await response.json();

    if (data.success) {
      history.push({ role: "assistant", content: data.content });
      console.log("AI trả lời:", data.content);
      console.log("Provider dùng:", data.providerUsed);
    } else {
      console.log("Lỗi:", data.error);
    }
  } catch (error) {
    console.log("Lỗi gọi API:", error);
  }
}
