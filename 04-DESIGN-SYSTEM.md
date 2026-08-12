# 04 — Design System (Himmel Theme)

## 1. Ý tưởng thiết kế

Himmel trong *Sousou no Frieren* mang hình ảnh vị dũng sĩ (hero) tóc vàng óng,
mắt xanh dương và khoác áo choàng trắng-xanh — một nhân vật ấm áp, đáng tin
cậy, luôn kiên nhẫn dẫn đường cho đồng đội. Rất hợp làm biểu tượng cho 1
"gia sư AI" kiên nhẫn dẫn dắt người học tiếng Nhật.

Bảng màu chủ đạo lần này lấy theo tông **thiên thanh** — gợi liên tưởng đến
bầu trời trong hành trình phiêu lưu của Himmel và đồng đội, đồng thời mang
cảm giác trong trẻo, sáng sủa, dễ chịu khi học lâu.

⚠️ Lưu ý minh bạch: đây là **màu sắc tham khảo/diễn giải lại** theo phong cách
nhân vật (không phải mã màu chính thức do studio công bố, vì anime không có
bảng màu hex chính thức public). AI Agent có thể tinh chỉnh sắc độ ±10% khi
code thực tế để đảm bảo độ tương phản (contrast) đạt chuẩn accessibility.

## 2. Bảng màu (CSS Variables)

```css
:root {
  /* Màu chính - tông thiên thanh, lấy cảm hứng từ mắt/áo choàng Himmel và bầu trời */
  --himmel-sky: #30AFFF;         /* Xanh thiên thanh đậm - màu chủ đạo, nút bấm, tiêu đề */
  --himmel-sky-light: #92EEFF;   /* Xanh thiên thanh nhạt - hover state, highlight, gradient */
  --himmel-sky-dark: #1E7FBF;    /* Xanh đậm hơn - active state, border, text nhấn mạnh */

  /* Màu phụ - vàng nhạt lấy cảm hứng từ tóc Himmel, dùng làm điểm nhấn ấm */
  --himmel-gold-accent: #F2D48A; /* Vàng nhạt - badge, icon nhỏ, điểm nhấn tương phản với nền xanh */

  /* Nền - tông sáng, trong trẻo */
  --himmel-bg: #F0FBFF;          /* Nền chính - xanh trắng rất nhạt, dịu mắt khi đọc lâu */
  --himmel-bg-card: #FFFFFF;     /* Nền thẻ/card chat */
  --himmel-bg-muted: #E1F6FF;    /* Nền phụ - vùng phân cách, sidebar */

  /* Text */
  --himmel-text: #1C2E38;        /* Text chính - xanh đen đậm, không dùng đen thuần */
  --himmel-text-muted: #6B8894;  /* Text phụ - placeholder, timestamp */

  /* Trạng thái */
  --himmel-success: #4CAF7D;     /* Xanh lá - câu đúng, xác nhận */
  --himmel-error: #E0687A;       /* Đỏ hồng - lỗi ngữ pháp, cảnh báo */

  /* Bo góc & bóng đổ */
  --himmel-radius: 12px;
  --himmel-shadow: 0 2px 12px rgba(48, 175, 255, 0.18);
}
```

## 3. Typography

- **Font chữ Latin/Việt:** `"Nunito", "Quicksand", sans-serif` — nét chữ tròn,
  mềm mại, hợp không khí ấm áp fantasy nhẹ nhàng của Frieren.
- **Font chữ Nhật (Kanji/Kana):** `"Noto Sans JP", sans-serif` — bắt buộc dùng
  font này (hoặc `Noto Serif JP` cho phần trích dẫn ví dụ) để hiển thị đúng
  nét chữ Kanji, tránh lỗi tofu (☐☐☐) khi font hệ thống không hỗ trợ.

```css
body {
  font-family: "Nunito", "Noto Sans JP", sans-serif;
}
.japanese-text {
  font-family: "Noto Sans JP", sans-serif;
  font-size: 1.15em; /* Kanji cần to hơn 1 chút để dễ đọc nét */
}
```

Nhớ import font qua Google Fonts trong `index.html`:

```html
<link href="https://fonts.googleapis.com/css2?family=Nunito:wght@400;600;700&family=Noto+Sans+JP:wght@400;500;700&display=swap" rel="stylesheet">
```

## 4. Layout gợi ý

- Bố cục dạng **chat single-column**, giống ChatGPT/Claude — bong bóng chat
  (chat bubble) của user căn phải, của AI căn trái.
- Bong bóng chat của user dùng nền gradient nhẹ từ `--himmel-sky` sang
  `--himmel-sky-light`, chữ trắng.
- Bong bóng chat của AI dùng `--himmel-bg-card` nền trắng + border nhẹ
  `--himmel-sky-light`, bo góc `--himmel-radius`.
- Nút gửi (Send button) dùng `--himmel-sky` nền, chữ trắng, hover chuyển
  `--himmel-sky-dark`.
- Có thể dùng `--himmel-gold-accent` làm điểm nhấn nhỏ (vd: icon "đã lưu",
  badge cấp độ JLPT) để tạo tương phản ấm giữa nền xanh chủ đạo — không lạm
  dụng, chỉ dùng cho chi tiết nhỏ.
- Header trên cùng có thể đặt tên app **"Himmel no Kotoba"** kèm 1 icon kiếm
  nhỏ hoặc icon sách đơn giản (SVG, không dùng ảnh nhân vật thật vì lý do bản
  quyền — xem ghi chú bên dưới).

## 5. Ghi chú về bản quyền (Copyright Note)

**Không** sử dụng ảnh/artwork thật của nhân vật Himmel hoặc bất kỳ hình ảnh
nào trích từ anime *Sousou no Frieren* trong sản phẩm (kể cả làm logo, icon,
background) — vì đây là IP có bản quyền. Dự án chỉ **lấy cảm hứng màu sắc**,
không tái sử dụng hình ảnh gốc. Nếu muốn có icon minh họa, dùng SVG tự vẽ
đơn giản (vd icon kiếm, icon sách, icon lá phong) theo phong cách chung, không
mô phỏng nhân vật cụ thể.
