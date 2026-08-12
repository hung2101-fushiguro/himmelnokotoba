# 02 — Architecture (Kiến trúc hệ thống)

## 1. Sơ đồ tổng quan

```
┌─────────────────────┐        ┌──────────────────────┐        ┌─────────────────┐
│   Browser (Client)  │  HTTP  │  Backend Minimal API  │  HTTP  │   AI Provider    │
│  - HTML/CSS/JS      │ ─────► │  (ASP.NET Core, C#)   │ ─────► │  Groq / Gemini   │
│  - localStorage      │ ◄───── │  - Stateless           │ ◄───── │                  │
│    (lịch sử chat)    │        │  - Input detection     │        │                  │
└─────────────────────┘        │  - Fallback chain       │        └─────────────────┘
                                │  - Prompt builder       │
                                └──────────────────────┘
```

## 2. Nguyên tắc cốt lõi: Stateless Backend (Backend không trạng thái)

Đây là quyết định kiến trúc quan trọng nhất, cần AI Agent tuân thủ tuyệt đối:

- Backend **không lưu bất kỳ dữ liệu người dùng nào**. Mỗi request là độc lập.
- Client (trình duyệt) chịu trách nhiệm giữ lịch sử hội thoại trong
  `localStorage`, và **gửi kèm lịch sử đó** trong mỗi request tới backend
  (giống cách gọi Anthropic/OpenAI Chat API — luôn gửi kèm mảng `messages`).
- Nhờ vậy: không cần Database, không cần Redis, không lo mất session khi
  server free-tier bị "ngủ" (sleep) — vì backend vốn dĩ không giữ gì để mất.

> So sánh với project Fuwa3e: ở đó phải dùng Redis vì chatbot cần nhớ trạng thái
> đơn hàng **xuyên suốt nhiều lần** người dùng quay lại, kể cả khi tắt máy.
> Ở đây, "trí nhớ" đã có sẵn ở phía Client rồi, nên Server không cần nhớ lại.

## 3. Known Limitation (Giới hạn đã biết — ghi rõ để không ai report thành "bug")

- Nếu người dùng xóa cache trình duyệt / đổi máy khác → mất lịch sử chat.
- Không đồng bộ đa thiết bị.
- Đây là đánh đổi (trade-off) **chủ động chấp nhận** để đổi lấy: không cần
  Database, deploy đơn giản, chi phí = 0đ.

## 4. Chiến lược chống Hallucination (Grounding Strategy)

Đây là ưu tiên số 1 theo yêu cầu ban đầu: **AI không được bịa thông tin.**

### Nguyên tắc áp dụng theo từng loại tính năng:

| Tính năng | Nguồn thông tin | Cách giảm hallucination |
|---|---|---|
| Kanji/Từ vựng (FR-2) | AI Provider (Groq/Gemini) | System Prompt yêu cầu: nếu không chắc chắn 100% về âm đọc, PHẢI nói rõ "cần xác minh thêm", không được đoán |
| Ngữ pháp (FR-3) | AI Provider | Tương tự — luôn kèm ví dụ cụ thể để người dùng tự đối chiếu, không đưa quy tắc chung chung mơ hồ |
| Dịch câu (FR-4) | AI Provider | Yêu cầu dịch sát nghĩa, không thêm thông tin ngoài câu gốc |
| Sửa lỗi (FR-5) | AI Provider | Không được "bịa" thêm lỗi nếu câu đã đúng |

### Roadmap mở rộng (Phase 2, không bắt buộc ở Phase 1)

Khi có thời gian, có thể tăng độ chính xác Kanji/Từ vựng bằng cách bundle sẵn
1 file JSON tĩnh (không phải Database — chỉ là file dữ liệu đóng gói cùng code,
load vào RAM lúc khởi động, giống `appsettings.json`) chứa dữ liệu từ nguồn mở
như KANJIDIC2/JMDict, tra cứu trong file JSON đó **trước**, chỉ dùng AI để bổ
sung phần diễn giải/ví dụ. Việc này KHÔNG nằm trong Phase 1 — ghi chú lại để
làm sau.

## 5. Tech Stack chi tiết

### Backend — ASP.NET Core Minimal API (.NET 10)

- 1 project duy nhất, không cần layered architecture phức tạp (khác với
  SatouNihongo) vì đây là API rất mỏng, chỉ có 1-2 endpoint.
- Cấu trúc thư mục đề xuất:
  ```
  /backend
    Program.cs              # Entry point, khai báo route
    Services/
      IAiProviderService.cs
      GroqProviderService.cs
      GeminiProviderService.cs
      FallbackChainService.cs
    Models/
      ChatRequest.cs
      ChatResponse.cs
      MessageDto.cs
    Prompts/
      SystemPrompts.cs       # Chứa các System Prompt cố định
  ```

### Frontend — HTML/CSS/JS thuần (Vanilla)

- Không dùng React/Vue/framework nặng — vì app chỉ có 1 màn hình chat, dùng
  Vanilla JS đủ và dễ AI Agent chỉnh sửa chính xác từng dòng CSS theo
  `04-DESIGN-SYSTEM.md`.
- Cấu trúc thư mục đề xuất:
  ```
  /frontend
    index.html
    style.css
    app.js                  # Xử lý gọi API, render tin nhắn, đọc/ghi localStorage
  ```

### Giao tiếp Frontend ↔ Backend

- 1 endpoint duy nhất: `POST /api/chat`
- Xem chi tiết request/response schema tại `03-API-SPEC.md`

## 6. Deploy

| Thành phần | Nơi deploy | Ghi chú |
|---|---|---|
| Backend | Render.com (Free Web Service) | Đã quen dùng, nhớ cấu hình biến môi trường `GROQ_API_KEY`, `GEMINI_API_KEY` trong Render Dashboard, KHÔNG commit vào code |
| Frontend | Vercel / Netlify (Free Static Site) | Deploy thẳng từ GitHub repo, tự động rebuild khi push code |
| Keep-alive | UptimeRobot (đã quen dùng) | Ping backend định kỳ tránh Render free tier ngủ |
