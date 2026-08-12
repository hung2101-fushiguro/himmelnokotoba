# 01 — Product Requirements Document (PRD)

## 1. Bối cảnh (Background)

Người dùng đang học tiếng Nhật (trình độ JLPT N3) và muốn 1 công cụ tra cứu +
chat nhanh, không cần đăng nhập, không cần cài đặt Database.

## 2. Đối tượng người dùng (User Persona)

- Người Việt tự học tiếng Nhật, đã có nền tảng N3.
- Quen gõ tiếng Nhật bằng nhiều dạng khác nhau: Romaji, Hiragana, Katakana, Kanji,
  hoặc thậm chí gõ Hán Việt (vd: "gia" thay vì gõ 家).
- Cần câu trả lời **nhanh, chính xác**, tin tưởng được (không muốn bị AI "bịa"
  âm đọc sai).

## 3. Các tính năng chính (Functional Requirements)

### FR-1: Universal Input Detection (Nhận diện đầu vào tự động)

- Người dùng gõ **bất kỳ chuỗi nào** vào 1 ô input duy nhất (không cần chọn
  "tôi đang tra Kanji" hay "tôi đang tra ngữ pháp").
- Hệ thống tự phân loại input thuộc 1 trong các nhóm sau (Equivalence Partitioning):
  1. Kanji đơn hoặc cụm Kanji (Unicode range `\u4E00-\u9FFF`)
  2. Hiragana (`\u3040-\u309F`)
  3. Katakana (`\u30A0-\u30FF`)
  4. Romaji (chữ Latin, có thể là từ vựng — vd "uchi") hoặc cấu trúc ngữ pháp
     (vd "uchi ni", "ba...hodo")
  5. Hán Việt (chữ Latin có dấu tiếng Việt hoặc không dấu, vd "gia", "quốc")
  6. Câu hoàn chỉnh (nhiều từ, có thể lẫn Kanji+Hiragana) → được coi là yêu cầu
     **dịch câu** (FR-3), không phải tra từ đơn.

> Lưu ý: bước phân loại này chỉ mang tính **gợi ý hướng xử lý** (routing hint),
> quyết định cuối cùng vẫn để AI Provider phân tích ngữ nghĩa, vì ranh giới giữa
> "1 từ vựng" và "1 cấu trúc ngữ pháp" không phải lúc nào cũng rõ ràng bằng Regex.

### FR-2: Kanji / Từ vựng Lookup

**Input mẫu:** `家`, `うち`, `ウチ`, `uchi`, `gia`

**Output bắt buộc gồm:**
- Chữ Kanji (nếu có)
- Âm Hán Việt
- Âm On (音読み) — ghi bằng Katakana
- Âm Kun (訓読み) — ghi bằng Hiragana
- Nghĩa tiếng Việt
- Ít nhất 2 từ vựng ví dụ có chứa Kanji/từ đó
- Ít nhất 1 câu ví dụ (kèm nghĩa tiếng Việt + Romaji)

### FR-3: Grammar Lookup (Tra ngữ pháp)

**Input mẫu:** `uchi ni`, `ba...hodo`, `～てしまう`

**Output bắt buộc gồm:**
- Tên cấu trúc ngữ pháp (dạng gốc tiếng Nhật + Romaji)
- Ý nghĩa / chức năng ngữ pháp
- Công thức (formula), vd: `V-る + うちに`
- Cấp độ JLPT ước lượng (N5/N4/N3/N2/N1) — ghi rõ nếu không chắc chắn
- Ít nhất 2 câu ví dụ (tiếng Nhật + Romaji + nghĩa tiếng Việt)
- Cấu trúc/ngữ pháp dễ nhầm lẫn liên quan (nếu có)

### FR-4: Dịch câu (Sentence Translation)

**Input mẫu:** 1 câu tiếng Nhật hoàn chỉnh, hoặc 1 câu tiếng Việt.

**Output bắt buộc gồm:**
- Bản dịch
- Phiên âm Romaji (nếu input là tiếng Nhật)
- Phân tích ngắn cấu trúc câu (chủ ngữ / động từ / trợ từ chính) — tùy chọn,
  hiển thị nếu người dùng bật chế độ "Giải thích chi tiết"

### FR-5: Kiểm tra lỗi ngữ pháp (Grammar Error Check)

**Input mẫu:** 1 câu tiếng Nhật do người dùng tự viết, có thể sai.

**Output bắt buộc gồm:**
- Câu đã sửa đúng
- Danh sách lỗi tìm được, mỗi lỗi gồm: vị trí lỗi, giải thích tại sao sai,
  cách sửa
- Nếu câu đã đúng hoàn toàn → xác nhận rõ ràng, không tự "chế thêm" lỗi giả

### FR-6: Lưu lịch sử hội thoại (Chat History)

- Lịch sử chat được lưu tại `localStorage` của trình duyệt.
- Khi refresh trang, lịch sử vẫn còn.
- Có nút "Xóa lịch sử" (Clear History) để người dùng chủ động xóa.
- **Không** đồng bộ lịch sử giữa các thiết bị (giới hạn đã biết — Known
  Limitation, xem `02-ARCHITECTURE.md`).

## 4. Yêu cầu phi chức năng (Non-Functional Requirements)

| Mã | Yêu cầu |
|---|---|
| NFR-1 | Không dùng Database server (Postgres/Mongo/MySQL...) |
| NFR-2 | Toàn bộ hạ tầng phải deploy được ở gói Free của nhà cung cấp |
| NFR-3 | Ưu tiên độ chính xác hơn tốc độ — chấp nhận chờ AI trả lời 2-5 giây |
| NFR-4 | Giao diện responsive, dùng tốt trên mobile (người dùng học mọi lúc mọi nơi) |
| NFR-5 | Giảm thiểu hallucination — xem chiến lược Grounding ở `02-ARCHITECTURE.md` |

## 5. Ngoài phạm vi (Out of Scope — Phase 1)

- Đăng nhập / tài khoản người dùng
- Đồng bộ dữ liệu đa thiết bị
- Chấm điểm phát âm (Speech-to-Text)
- Flashcard / SRS (Spaced Repetition System) — đây là tính năng của project
  **SatouNihongo** riêng, không lặp lại ở đây
