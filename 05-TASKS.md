# 05 — Task Checklist (theo Phase)

> Quy tắc: thực hiện tuần tự từng Phase, hoàn thành Task nào đánh dấu `[x]`
> Task đó trước khi qua Task tiếp theo. Không nhảy cóc sang Phase sau khi
> Phase trước chưa xong và chưa test được.

## Phase 0 — Khởi tạo project

- [x] Tạo repo Git mới, cấu trúc thư mục `/backend` và `/frontend` theo
      `02-ARCHITECTURE.md`
- [x] Khởi tạo ASP.NET Core Minimal API project (`dotnet new web`) trong
      `/backend`
- [x] Tạo file `.env.example` liệt kê các biến môi trường cần thiết (không
      chứa giá trị thật)
- [x] Tạo file `.gitignore` chuẩn cho .NET (đảm bảo `.env`, `bin/`, `obj/`
      không bị commit)

## Phase 1 — Backend: Fallback Chain hoạt động được

- [x] Cài package `Microsoft.Extensions.Http.Polly`
- [x] Tạo `Models/ChatRequest.cs`, `Models/ChatResponse.cs`,
      `Models/MessageDto.cs` theo schema ở `03-API-SPEC.md`
- [x] Tạo `Services/IAiProviderService.cs` (interface chung cho mọi provider)
- [x] Implement `Services/GroqProviderService.cs` — gọi Groq API, hỗ trợ đổi
      model qua config
- [x] Implement `Services/GeminiProviderService.cs` — gọi Gemini API
- [x] Tạo `Prompts/ModelConfig.cs` — nơi duy nhất chứa 4 model ID theo Fallback Chain
- [x] Implement `Services/FallbackChainService.cs` — điều phối gọi lần lượt
      theo đúng thứ tự ở `03-API-SPEC.md` mục 2
- [x] Đặt System Prompt cố định trong `Prompts/SystemPrompts.cs` (copy nguyên
      văn từ `03-API-SPEC.md` mục 3)
- [x] Khai báo route `POST /api/chat` trong `Program.cs`
- [x] **Test bằng tay** (Postman/curl): gửi request mẫu `"uchi ni"`, xác nhận
      nhận được response đúng schema

## Phase 2 — Backend: Xử lý lỗi & CORS

- [x] Cấu hình CORS chỉ cho phép `ALLOWED_ORIGIN`
- [x] Xử lý trường hợp tất cả provider đều fail → trả về đúng
      "Response Body (lỗi)" ở `03-API-SPEC.md`
- [x] Thêm logging (console log đủ dùng ở giai đoạn này) ghi lại provider nào
      được dùng cho mỗi request — để debug Fallback Chain

## Phase 3 — Frontend: Giao diện cơ bản

- [x] Đọc kỹ `04-DESIGN-SYSTEM.md`, khai báo CSS variables trong `style.css`
- [x] Dựng layout chat cơ bản trong `index.html` (khung chat, ô input, nút gửi)
- [x] Import Google Fonts (Nunito + Noto Sans JP)
- [x] Style bong bóng chat user/AI theo Himmel palette

## Phase 4 — Frontend: Kết nối Backend + localStorage

- [x] Viết hàm `sendMessage()` trong `app.js` gọi `POST /api/chat`
- [x] Viết hàm `loadHistory()` / `saveHistory()` đọc-ghi `localStorage`
- [x] Render lịch sử chat khi load lại trang
- [ ] Thêm nút "Xóa lịch sử" (Clear History) — xóa `localStorage` + xóa UI
- [ ] Hiển thị loading indicator khi đang chờ AI trả lời (vì có thể mất
      2-8 giây do Fallback Chain)

## Phase 5 — Polish & Test end-to-end

- [ ] Test cả 5 loại input theo `01-PRD.md`: Kanji, Hiragana/Katakana, Romaji,
      Hán Việt, câu hoàn chỉnh (dịch câu)
- [ ] Test trường hợp cố tình gây lỗi (vd tắt tạm API key Groq) để xác nhận
      Fallback Chain rớt xuống Gemini đúng như thiết kế
- [ ] Test responsive trên mobile (viewport nhỏ)
- [ ] Review lại toàn bộ text hiển thị — đảm bảo 100% tiếng Việt, không sót
      text tiếng Anh debug

## Phase 6 — Deploy

- [ ] Deploy Backend lên Render.com, khai báo Environment Variables trên
      Dashboard (không commit `.env`)
- [ ] Deploy Frontend lên Vercel/Netlify, trỏ `app.js` gọi đúng URL backend
      production
- [ ] Set up UptimeRobot ping Backend định kỳ (tránh Render free tier ngủ)
- [ ] Test lại toàn bộ luồng trên URL production thật (không phải localhost)

---

**Ghi chú cho AI Agent:** nếu trong lúc code phát sinh quyết định kỹ thuật
chưa được mô tả rõ trong 4 file docs trước, hãy chọn phương án **đơn giản
nhất, ít phụ thuộc nhất**, và ghi chú lại trong code comment — không tự ý
thêm Database, thêm thư viện nặng, hoặc đổi kiến trúc đã chốt.
