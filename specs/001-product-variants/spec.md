# Đặc tả tính năng: Quản lý sản phẩm có biến thể

<!-- Language: Vietnamese — all prose, headings, and descriptions in Vietnamese.
     Code, file paths, identifiers, and code comments remain in English. -->

**Nhánh tính năng**: `001-product-variants`
**Ngày tạo**: 2026-03-27
**Trạng thái**: Bản nháp
**Đầu vào**: Mô tả người dùng: "xây dựng chức năng quản lý sản phẩm có biến thể. thông tin sản phẩm bao gồm tên, sku, slug, description, danh mục"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Tạo sản phẩm mới có biến thể (Priority: P1)

Người quản trị viên tạo một sản phẩm mới với đầy đủ thông tin cơ bản (tên, SKU, slug, mô tả, danh mục) và định nghĩa các biến thể của sản phẩm (ví dụ: màu sắc, kích thước). Mỗi biến thể có SKU riêng và có thể có giá, tồn kho riêng biệt.

**Why this priority**: Đây là luồng cốt lõi — nếu không tạo được sản phẩm có biến thể thì toàn bộ chức năng không có giá trị. Là điều kiện tiên quyết cho tất cả các user story khác.

**Independent Test**: Có thể kiểm thử độc lập bằng cách tạo một sản phẩm "Áo thun" với biến thể Màu (Đỏ, Xanh) × Kích thước (S, M, L) và xác nhận sản phẩm + biến thể được lưu thành công.

**Acceptance Scenarios**:

1. **Given** quản trị viên đang ở trang tạo sản phẩm, **When** nhập đầy đủ tên, SKU, slug, mô tả, chọn ít nhất một danh mục, khai báo thuộc tính biến thể và nhập SKU thủ công cho từng biến thể được sinh ra, **Then** hệ thống lưu sản phẩm thành công và hiển thị thông báo xác nhận.
2. **Given** quản trị viên tạo sản phẩm, **When** nhập SKU trùng với sản phẩm hoặc biến thể đã tồn tại, **Then** hệ thống từ chối và hiển thị thông báo lỗi rõ ràng.
3. **Given** quản trị viên tạo sản phẩm, **When** để trống trường bắt buộc (tên) hoặc không chọn danh mục nào, **Then** hệ thống hiển thị lỗi xác thực trước khi lưu.
4. **Given** quản trị viên tạo sản phẩm, **When** nhập slug trùng với sản phẩm đã tồn tại, **Then** hệ thống từ chối và gợi ý slug thay thế.

---

### User Story 2 - Xem danh sách và tìm kiếm sản phẩm (Priority: P2)

Quản trị viên xem danh sách tất cả sản phẩm, có thể lọc theo danh mục, tìm kiếm theo tên/SKU, và xem số lượng biến thể của từng sản phẩm.

**Why this priority**: Quản trị viên cần tra cứu và kiểm soát danh mục sản phẩm hàng ngày. Đây là điểm vào chính để quản lý sản phẩm.

**Independent Test**: Có thể kiểm thử bằng cách xem danh sách sản phẩm đã tạo, tìm theo từ khóa và lọc theo danh mục, xác nhận kết quả hiển thị đúng.

**Acceptance Scenarios**:

1. **Given** có sản phẩm trong hệ thống, **When** quản trị viên truy cập trang danh sách, **Then** hiển thị danh sách sản phẩm với thông tin tên, SKU, danh mục, số lượng biến thể, trạng thái và nút toggle trạng thái nhanh.
2. **Given** danh sách sản phẩm, **When** quản trị viên tìm kiếm theo tên hoặc SKU, **Then** chỉ hiển thị sản phẩm khớp với từ khóa.
3. **Given** danh sách sản phẩm, **When** quản trị viên lọc theo danh mục, **Then** chỉ hiển thị sản phẩm thuộc danh mục đó.

---

### User Story 3 - Chỉnh sửa thông tin sản phẩm và biến thể (Priority: P2)

Quản trị viên cập nhật thông tin sản phẩm đã tồn tại: chỉnh sửa tên, mô tả, danh mục, slug; thêm biến thể mới, chỉnh sửa hoặc xóa biến thể hiện có.

**Why this priority**: Thông tin sản phẩm thay đổi theo thời gian (cập nhật giá, thêm màu mới, v.v.). Đây là nhu cầu vận hành thường xuyên.

**Independent Test**: Có thể kiểm thử bằng cách chỉnh sửa tên và thêm biến thể mới cho sản phẩm đã tạo, xác nhận thay đổi được lưu đúng.

**Acceptance Scenarios**:

1. **Given** sản phẩm đã tồn tại, **When** quản trị viên cập nhật tên và mô tả, **Then** hệ thống lưu thay đổi và hiển thị thông tin mới.
2. **Given** sản phẩm đã có biến thể, **When** quản trị viên thêm biến thể mới với SKU hợp lệ, **Then** biến thể mới được lưu và hiển thị trong danh sách biến thể.
3. **Given** sản phẩm đã có biến thể, **When** quản trị viên xóa một biến thể, **Then** biến thể bị xóa và không còn xuất hiện trong danh sách.
4. **Given** sản phẩm đang chỉnh sửa, **When** quản trị viên thay đổi sang SKU trùng, **Then** hệ thống từ chối và báo lỗi.

---

### User Story 4 - Xóa sản phẩm (Priority: P3)

Quản trị viên xóa sản phẩm không còn kinh doanh. Hệ thống cần xác nhận trước khi xóa để tránh thao tác nhầm.

**Why this priority**: Xóa sản phẩm là thao tác không thường xuyên nhưng cần thiết để duy trì danh mục sạch sẽ. Ưu tiên thấp hơn vì ít dùng hơn.

**Independent Test**: Có thể kiểm thử bằng cách xóa một sản phẩm và xác nhận nó không còn xuất hiện trong danh sách.

**Acceptance Scenarios**:

1. **Given** sản phẩm đang tồn tại, **When** quản trị viên chọn xóa và xác nhận, **Then** sản phẩm và tất cả biến thể liên quan bị đánh dấu đã xóa (soft delete) và không còn hiển thị trong danh sách.
2. **Given** quản trị viên chọn xóa sản phẩm, **When** chưa xác nhận, **Then** hệ thống hiển thị hộp thoại xác nhận với thông tin sản phẩm sắp bị xóa.

---

### Edge Cases

- Khi admin cố xóa danh mục, hệ thống chặn thao tác và hiển thị thông báo lỗi kèm số lượng sản phẩm đang được gán vào danh mục đó (kể cả sản phẩm có nhiều danh mục khác).
- Sản phẩm có thể tồn tại mà không có biến thể nào (sản phẩm đơn giản) — đây là trường hợp hợp lệ.
- Khi slug tự động sinh từ tên bị trùng, hệ thống từ chối lưu và gợi ý slug thay thế cho admin chỉnh sửa thủ công (không tự thêm suffix).
- Khi admin thêm một giá trị thuộc tính mới (ví dụ: thêm màu "Vàng" vào sản phẩm đã có biến thể), hệ thống sinh thêm biến thể mới cho giá trị đó và để admin xem xét/vô hiệu hóa nếu cần.

## Clarifications

### Session 2026-03-27

- Q: Khi admin khai báo thuộc tính biến thể, hệ thống sinh tổ hợp theo cách nào? → A: Tự động sinh toàn bộ tổ hợp (cartesian product), admin có thể xóa hoặc vô hiệu hóa từng biến thể không mong muốn sau đó.
- Q: Khi admin xóa sản phẩm, hệ thống thực hiện hard delete hay soft delete? → A: Soft delete — đánh dấu đã xóa, ẩn khỏi danh sách quản lý, vẫn giữ dữ liệu để bảo toàn tham chiếu lịch sử.
- Q: SKU biến thể được xác định như thế nào? → A: Admin nhập thủ công SKU cho từng biến thể — hệ thống không tự sinh SKU.
- Q: Khi admin xóa danh mục đang được gán cho sản phẩm, hệ thống xử lý thế nào? → A: Chặn xóa — hiển thị thông báo lỗi kèm số lượng sản phẩm đang dùng danh mục đó.
- Q: Admin quản lý trạng thái hoạt động/không hoạt động của sản phẩm như thế nào? → A: Admin có thể toggle trạng thái trực tiếp từ danh sách sản phẩm hoặc từ trang chỉnh sửa.
- Clarification (user): Một sản phẩm có thể được gắn nhiều danh mục (quan hệ nhiều-nhiều). Danh mục là phẳng, không phân cấp cha con.
- Q: Khi nào hệ thống chặn xóa danh mục? → A: Luôn chặn nếu có bất kỳ sản phẩm nào đang thuộc danh mục đó, dù sản phẩm có danh mục khác hay không.
- Q: VariantAttribute (tên thuộc tính biến thể) được quản lý per-product hay global? → A: Per-product — mỗi sản phẩm tự nhập tên thuộc tính tự do, không chia sẻ với sản phẩm khác, không có bảng attributes toàn hệ thống.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hệ thống PHẢI cho phép tạo sản phẩm với các trường: tên (bắt buộc), SKU (bắt buộc, duy nhất), slug (bắt buộc, duy nhất), mô tả (không bắt buộc), danh mục (bắt buộc, chọn một hoặc nhiều).
- **FR-002**: Hệ thống PHẢI tự động sinh slug từ tên sản phẩm nếu người dùng không nhập, và đảm bảo slug là duy nhất trong toàn hệ thống.
- **FR-003**: Hệ thống PHẢI cho phép admin nhập tự do tên thuộc tính biến thể (ví dụ: "Màu sắc", "Kích thước") và các giá trị tương ứng khi tạo/chỉnh sửa sản phẩm. Thuộc tính thuộc về từng sản phẩm riêng lẻ, không được chia sẻ hay chuẩn hóa toàn hệ thống.
- **FR-004**: Hệ thống PHẢI tự động tạo tập hợp biến thể (variants) từ tích Descartes các giá trị thuộc tính (ví dụ: 2 màu × 3 size = 6 biến thể). Sau khi tổ hợp được sinh ra, admin PHẢI nhập thủ công SKU riêng cho từng biến thể (bắt buộc, duy nhất toàn hệ thống) trước khi có thể lưu sản phẩm. Admin có thể xóa hoặc vô hiệu hóa từng biến thể không mong muốn.
- **FR-005**: Hệ thống PHẢI xác thực SKU là duy nhất — không được trùng giữa SKU sản phẩm và SKU biến thể, và không trùng với bất kỳ sản phẩm/biến thể nào khác.
- **FR-006**: Hệ thống PHẢI hiển thị danh sách sản phẩm với phân trang, cho phép tìm kiếm theo tên/SKU và lọc theo danh mục.
- **FR-007**: Người dùng PHẢI có thể chỉnh sửa thông tin sản phẩm và thêm/xóa/sửa biến thể sau khi tạo.
- **FR-011**: Admin PHẢI có thể thay đổi trạng thái sản phẩm (hoạt động/không hoạt động) trực tiếp từ danh sách sản phẩm (toggle nhanh) hoặc từ trang chỉnh sửa sản phẩm. Trạng thái mặc định khi tạo mới là hoạt động.
- **FR-008**: Hệ thống PHẢI yêu cầu xác nhận trước khi xóa sản phẩm. Xóa là soft delete — sản phẩm và biến thể liên quan bị đánh dấu đã xóa và ẩn khỏi danh sách quản lý, nhưng dữ liệu được giữ lại để bảo toàn tham chiếu lịch sử.
- **FR-009**: Hệ thống PHẢI hiển thị thông báo lỗi rõ ràng khi xác thực thất bại (SKU trùng, slug trùng, thiếu trường bắt buộc).
- **FR-010**: Sản phẩm PHẢI thuộc ít nhất một danh mục và có thể thuộc nhiều danh mục cùng lúc (quan hệ nhiều-nhiều). Danh mục là phẳng, không phân cấp. Danh mục phải được quản lý độc lập (tạo/đọc danh sách danh mục để gán). Hệ thống PHẢI chặn xóa danh mục nếu có bất kỳ sản phẩm nào đang thuộc danh mục đó — bất kể sản phẩm có thuộc danh mục khác hay không — và hiển thị thông báo lỗi kèm số lượng sản phẩm liên quan.

### Key Entities

- **Product (Sản phẩm)**: Đại diện cho một sản phẩm trong catalog. Thuộc tính: tên, SKU gốc, slug (định danh URL duy nhất), mô tả, trạng thái (hoạt động/không hoạt động). Quan hệ nhiều-nhiều với Category.
- **ProductVariant (Biến thể sản phẩm)**: Một phiên bản cụ thể của sản phẩm với tổ hợp thuộc tính riêng. Thuộc tính: SKU riêng, tổ hợp giá trị thuộc tính, quan hệ với sản phẩm cha.
- **VariantAttribute (Thuộc tính biến thể)**: Tên chiều biến thể (ví dụ: "Màu sắc", "Kích thước") do admin nhập tự do, gắn riêng với từng sản phẩm — không chia sẻ toàn hệ thống.
- **VariantAttributeValue (Giá trị thuộc tính)**: Giá trị cụ thể của một thuộc tính (ví dụ: "Đỏ", "M"). Kết hợp nhiều giá trị tạo thành một biến thể.
- **Category (Danh mục)**: Nhóm phân loại sản phẩm, cấu trúc phẳng (không phân cấp). Thuộc tính: tên, mô tả. Một sản phẩm có thể thuộc nhiều danh mục; một danh mục có thể chứa nhiều sản phẩm (quan hệ nhiều-nhiều).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Quản trị viên có thể hoàn tất việc tạo sản phẩm với biến thể trong vòng 3 phút.
- **SC-002**: Tìm kiếm sản phẩm trả về kết quả trong dưới 1 giây với catalog lên đến 10,000 sản phẩm.
- **SC-003**: 100% trường hợp SKU hoặc slug trùng bị từ chối với thông báo lỗi rõ ràng trước khi lưu.
- **SC-004**: Quản trị viên có thể tìm thấy sản phẩm cần quản lý trong vòng 30 giây bằng tìm kiếm hoặc lọc.
- **SC-005**: Khi sản phẩm bị xóa (soft delete), toàn bộ biến thể liên quan cũng bị đánh dấu đã xóa — không có biến thể nào vẫn hiển thị sau khi sản phẩm cha bị xóa.

## Assumptions

- Chức năng quản lý sản phẩm chỉ dành cho người dùng có vai trò quản trị viên (admin); xác thực và phân quyền đã có sẵn trong hệ thống.
- Danh mục (Category) là phẳng (không phân cấp cha con). Một sản phẩm có thể thuộc nhiều danh mục (quan hệ nhiều-nhiều).
- Biến thể sản phẩm được tạo từ tổ hợp tự động — hệ thống sinh ra tất cả tổ hợp có thể từ các thuộc tính và giá trị được khai báo.
- Sản phẩm không bắt buộc phải có biến thể (hỗ trợ sản phẩm đơn giản không có biến thể).
- Quản lý giá và tồn kho của biến thể nằm ngoài phạm vi của tính năng này (sẽ thuộc module riêng).
- Hình ảnh sản phẩm và biến thể nằm ngoài phạm vi v1.
- Giao diện người dùng là web admin dashboard; mobile không nằm trong phạm vi v1.
