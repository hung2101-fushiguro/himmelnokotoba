# Himmel no Kotoba (ヒンメルの言葉) 🗡️📖

> Chatbot hỗ trợ học tiếng Nhật — tra Kanji/Hiragana/Katakana/Hán Việt, tra ngữ pháp,
> dịch câu, và tìm lỗi sai — lấy cảm hứng màu sắc từ nhân vật **Himmel** trong
> *Sousou no Frieren*.

## 🎯 Mục tiêu project

Xây dựng một web app single-page, KHÔNG cần Database server, deploy free, nơi
người dùng gõ **bất kỳ** dạng chữ Nhật nào (Kanji, Hiragana, Katakana, Romaji,
Hán Việt) hoặc một cụm ngữ pháp (vd: "uchi ni") và nhận lại thông tin chi tiết.

## 📚 Đọc tài liệu theo thứ tự này

AI Agent (OpenCode) và người review PHẢI đọc đúng thứ tự sau trước khi viết code,
vì mỗi file phụ thuộc vào quyết định của file trước:

1. **[docs/01-PRD.md](docs/01-PRD.md)** — Yêu cầu chức năng (Functional Requirements),
   user story, 4 tính năng chính.
2. **[docs/02-ARCHITECTURE.md](docs/02-ARCHITECTURE.md)** — Kiến trúc hệ thống,
   tech stack, lý do KHÔNG dùng Database, cách "nhớ" lịch sử chat.
3. **[docs/03-API-SPEC.md](docs/03-API-SPEC.md)** — Hợp đồng API (API contract),
   AI Provider Fallback Chain (Groq → Gemini), Prompt Template.
4. **[docs/04-DESIGN-SYSTEM.md](docs/04-DESIGN-SYSTEM.md)** — Bảng màu Himmel,
   Typography, CSS variables.
5. **[docs/05-TASKS.md](docs/05-TASKS.md)** — Checklist các Task theo Phase,
   AI Agent thực hiện tuần tự từng task, KHÔNG nhảy cóc.

## 🛠️ Tech Stack tóm tắt

| Layer | Công nghệ | Lý do |
|---|---|---|
| Backend | ASP.NET Core Minimal API (.NET 10, C#) | Bạn đang học C#/.NET → vừa build vừa luyện |
| Frontend | HTML + CSS thuần + JavaScript (Vanilla) | Không cần build tool, style tự do theo Himmel palette |
| Lưu lịch sử chat | `localStorage` (trình duyệt) | Không cần Database, đơn giản, miễn phí |
| AI Provider | Groq (chính) → Gemini (dự phòng) | Free tier tốt nhất, xem chi tiết ở 03-API-SPEC.md |
| Deploy Backend | Render.com Free Tier | Bạn đã quen dùng ở Fuwa3e/AniSeason |
| Deploy Frontend | Vercel / Netlify / Render Static Site | Free, deploy 1-click từ GitHub |

## ⚠️ Nguyên tắc bắt buộc cho AI Agent khi code

- **KHÔNG** tự ý thêm Database (PostgreSQL, MongoDB, SQLite...) — kiến trúc này
  cố tình không cần Database.
- **KHÔNG** hard-code API key trong code — luôn đọc từ biến môi trường
  (Environment Variables).
- Mọi lời gọi tới AI Provider phải đi qua **Fallback Chain** mô tả trong
  `03-API-SPEC.md`, không được gọi thẳng 1 provider duy nhất.
- Toàn bộ text hiển thị ra UI dùng tiếng Việt (đây là app dành cho người Việt học
  tiếng Nhật).
