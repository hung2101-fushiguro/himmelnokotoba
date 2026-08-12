# 03 — API Specification

## 1. Endpoint chính

### `POST /api/chat`

**Request Body:**

```json
{
  "messages": [
    { "role": "user", "content": "uchi ni" },
    { "role": "assistant", "content": "..." },
    { "role": "user", "content": "còn ba...hodo thì sao?" }
  ]
}
```

- `messages`: toàn bộ lịch sử hội thoại, Client gửi kèm mỗi lần gọi (xem
  `02-ARCHITECTURE.md` mục "Stateless Backend").
- Tin nhắn cuối cùng trong mảng (`role: "user"`) là câu hỏi hiện tại cần trả lời.

**Response Body (thành công):**

```json
{
  "success": true,
  "type": "grammar",
  "content": "... nội dung trả lời dạng Markdown ...",
  "providerUsed": "groq"
}
```

- `type`: 1 trong `"kanji" | "vocabulary" | "grammar" | "translation" | "error_check" | "general"`
  — do AI Provider tự xác định và trả về, giúp Frontend hiển thị đúng layout
  (vd: dạng thẻ Kanji card, hoặc dạng bảng ngữ pháp).
- `content`: nội dung trả lời, định dạng **Markdown** để Frontend render đẹp
  (bold, bullet list, code block cho công thức ngữ pháp).
- `providerUsed`: giúp debug — biết request vừa rồi được provider nào trả lời
  (hữu ích khi test Fallback Chain).

**Response Body (lỗi — tất cả provider đều fail):**

```json
{
  "success": false,
  "error": "Hệ thống đang quá tải, vui lòng thử lại sau ít phút."
}
```

## 2. AI Provider Fallback Chain

Thứ tự ưu tiên gọi provider — **bắt buộc implement đúng thứ tự này**, có
retry + timeout hợp lý ở mỗi bước trước khi rớt xuống provider tiếp theo:

```
1️⃣ Groq — model: "qwen/qwen3.6-27b"       (mạnh nhất cho suy luận, nhưng Preview
                                              nên có thể bị Groq rút bất kỳ lúc nào)
        ↓ lỗi (429 / 5xx / timeout)
2️⃣ Groq — model: "openai/gpt-oss-120b"    (Production, ổn định, fallback nội bộ
                                              cùng provider trước khi đổi hẳn provider khác)
        ↓ lỗi
3️⃣ Gemini — model: "gemini-2.5-flash"
        ↓ lỗi
4️⃣ Gemini — model: "gemini-2.5-flash-lite" (nhẹ nhất, RPD cao nhất, cứu cánh cuối)
        ↓ lỗi
5️⃣ Trả lỗi thân thiện cho Client (xem Response Body lỗi ở trên)
```

**Lưu ý quan trọng cho AI Agent khi code:**

- Model ID thay đổi theo thời gian (nhà cung cấp deprecate model liên tục).
  Đặt các model ID này trong **1 file config/constant duy nhất**
  (`Prompts/ModelConfig.cs` hoặc biến môi trường), KHÔNG hardcode rải rác
  nhiều chỗ trong code — để sau này đổi model chỉ cần sửa 1 nơi.
- Dùng thư viện **Polly** (`Microsoft.Extensions.Http.Polly`) để implement
  Retry Policy + Fallback Policy, không tự viết vòng lặp try-catch thủ công.
- Timeout đề xuất mỗi provider: 8 giây trước khi coi là fail và rớt xuống
  bước tiếp theo.

## 3. System Prompt (Prompt Template)

Đây là phần quyết định chất lượng trả lời — đặt cố định trong
`Prompts/SystemPrompts.cs`, không để AI Agent tự do diễn giải lại mỗi lần code.

```
Bạn là một gia sư tiếng Nhật chuyên nghiệp, hỗ trợ người Việt học tiếng Nhật
trình độ JLPT N3. Nhiệm vụ của bạn:

1. Xác định người dùng đang hỏi về: Kanji/từ vựng, ngữ pháp, dịch câu, hay
   kiểm tra lỗi ngữ pháp — dựa vào nội dung tin nhắn mới nhất.
2. Trả lời NGẮN GỌN, CÓ CẤU TRÚC, định dạng Markdown.
3. QUY TẮC BẮT BUỘC — CHỐNG BỊA THÔNG TIN:
   - Nếu không chắc chắn 100% về âm đọc (On/Kun), Hán Việt, hoặc cấp độ JLPT
     của 1 cấu trúc ngữ pháp, PHẢI ghi rõ "⚠️ Cần xác minh thêm" thay vì đoán.
   - KHÔNG được tự chế thêm từ vựng/câu ví dụ không có thật.
   - Khi sửa lỗi ngữ pháp: nếu câu người dùng viết đã đúng, XÁC NHẬN RÕ là
     đúng, KHÔNG được cố tìm ra lỗi giả để có gì đó trả lời.
4. Luôn trả lời bằng tiếng Việt (trừ phần tiếng Nhật/Romaji cần giữ nguyên).
5. Trả về kèm 1 dòng đầu tiên duy nhất theo định dạng:
   TYPE: <kanji|vocabulary|grammar|translation|error_check|general>
   (dòng này để hệ thống parse, không hiển thị cho người dùng)
```

## 4. Biến môi trường cần thiết (Environment Variables)

| Tên biến | Mô tả |
|---|---|
| `GROQ_API_KEY` | API key lấy từ console.groq.com |
| `GEMINI_API_KEY` | API key lấy từ Google AI Studio |
| `ALLOWED_ORIGIN` | Domain của Frontend, dùng để cấu hình CORS (vd: `https://himmel-no-kotoba.vercel.app`) |

## 5. CORS

Backend phải bật CORS chỉ cho phép đúng domain Frontend gọi vào (đọc từ
`ALLOWED_ORIGIN`), không mở `AllowAnyOrigin()` cho môi trường production.
