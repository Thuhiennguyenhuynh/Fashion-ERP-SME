# ai-service/main.py
# Server FastAPI cho FashionERP AI module.
#   - /chatbot           : Gemini thật (cần GEMINI_API_KEY)
#   - /size-recommend    : thuật toán weighted-distance thật, không cần API ngoài
#   - /forecast           : linear trend (least squares) thật, không cần API ngoài
#
# Cài đặt & chạy:
#   pip install fastapi uvicorn google-genai
#   set GEMINI_API_KEY=xxxx        (Windows PowerShell: $env:GEMINI_API_KEY="xxxx")
#   export GEMINI_API_KEY=xxxx     (Mac/Linux)
#   uvicorn main:app --reload --port 8001
#
# appsettings.json (.NET):  "AiService": { "BaseUrl": "http://localhost:8001" }

import asyncio
import json
import os
from datetime import datetime, timedelta

from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
from google import genai
from google.genai import types as genai_types

app = FastAPI(title="FashionERP AI Service")

GEMINI_MODEL = os.environ.get("GEMINI_MODEL", "gemini-2.5-flash")
_gemini_client: "genai.Client | None" = None


def _get_gemini_client() -> "genai.Client":
    """Khởi tạo client 1 lần duy nhất (lazy singleton), báo lỗi rõ ràng nếu thiếu API key
    thay vì để lỗi mơ hồ lúc gọi generate_content."""
    global _gemini_client
    if _gemini_client is None:
        api_key = os.environ.get("GEMINI_API_KEY")
        if not api_key:
            raise RuntimeError("Thiếu biến môi trường GEMINI_API_KEY")
        _gemini_client = genai.Client(api_key=api_key)
    return _gemini_client


@app.get("/health")
async def health():
    return {"status": "ok"}


CHATBOT_RESPONSE_SCHEMA = {
    "type": "OBJECT",
    "properties": {
        "reply": {
            "type": "STRING",
            "description": "Câu trả lời tư vấn bằng tiếng Việt, thân thiện, ngắn gọn (2-4 câu)",
        },
        "suggested_indexes": {
            "type": "ARRAY",
            "items": {"type": "INTEGER"},
            "description": (
                "Tối đa 3 chỉ số (index, bắt đầu từ 0) của sản phẩm trong danh sách "
                "đã cung cấp thực sự liên quan tới câu hỏi. Để mảng rỗng nếu không có "
                "sản phẩm nào phù hợp."
            ),
        },
    },
    "required": ["reply", "suggested_indexes"],
}


def _build_system_instruction(products: list, promotions: list) -> str:
    lines = [
        "Bạn là trợ lý tư vấn bán hàng của một cửa hàng thời trang.",
        "Trả lời khách bằng tiếng Việt, giọng thân thiện, ngắn gọn, tự nhiên.",
        "CHỈ được gợi ý sản phẩm có trong danh sách dưới đây, bằng cách trả về "
        "index (0-based) của nó trong mảng suggested_indexes.",
        "TUYỆT ĐỐI không bịa ra sản phẩm, giá, hay mã khuyến mãi không có trong danh sách.",
        "",
        "Danh sách sản phẩm hiện có (index: tên - danh mục - giá - tồn kho):",
    ]
    for i, p in enumerate(products):
        price = p.get("basePrice") or 0
        lines.append(
            f"  [{i}] {p.get('name')} - {p.get('categoryName') or 'N/A'} - "
            f"{price:,.0f}đ - tồn {p.get('totalStock')}"
        )
    if promotions:
        lines.append("")
        lines.append("Khuyến mãi đang áp dụng:")
        for pr in promotions:
            lines.append(f"  - {pr.get('code')}: {pr.get('name')} ({pr.get('type')} {pr.get('discountValue')})")
    return "\n".join(lines)


@app.post("/chatbot")
async def chatbot(req: Request):
    body = await req.json()
    message = (body.get("message") or "").strip()

    # Gõ đúng chữ "TEST_TIMEOUT" trong message để test case .NET timeout
    # (HttpClient.Timeout = 30s trong Program.cs -> mong đợi .NET trả lỗi 504)
    if message == "TEST_TIMEOUT":
        await asyncio.sleep(35)

    products = body.get("productContext") or []
    promotions = body.get("promotionContext") or []
    history = body.get("history") or []

    try:
        client = _get_gemini_client()

        contents = []
        for h in history[-10:]:  # tối đa 10 lượt gần nhất, khớp comment trong ChatbotRequestDto bên C#
            role = "model" if (h.get("role") or "").lower() == "assistant" else "user"
            contents.append(
                genai_types.Content(role=role, parts=[genai_types.Part.from_text(text=h.get("content") or "")])
            )
        contents.append(genai_types.Content(role="user", parts=[genai_types.Part.from_text(text=message)]))

        response = client.models.generate_content(
            model=GEMINI_MODEL,
            contents=contents,
            config={
                "system_instruction": _build_system_instruction(products, promotions),
                "response_mime_type": "application/json",
                "response_schema": CHATBOT_RESPONSE_SCHEMA,
            },
        )

        parsed = json.loads(response.text)
        reply = parsed.get("reply") or "Xin lỗi, mình chưa có câu trả lời phù hợp lúc này."
        raw_indexes = parsed.get("suggested_indexes") or []
    except Exception as exc:
        # Bất kỳ lỗi nào (thiếu key, hết quota, model lỗi...) -> trả non-2xx,
        # .NET (AIServiceClient) sẽ tự map thành AppException 503 cho người dùng cuối.
        return JSONResponse(status_code=502, content={"message": f"Lỗi gọi Gemini: {exc}"})

    # Map index Gemini trả về -> sản phẩm THẬT lấy từ productContext do C# gửi lên.
    # Không dùng id/giá do Gemini tự sinh, chỉ dùng index để chọn -> không bao giờ
    # trả về 1 GUID không tồn tại trong DB.
    suggested = []
    seen = set()
    for idx in raw_indexes:
        if isinstance(idx, int) and 0 <= idx < len(products) and idx not in seen:
            seen.add(idx)
            p = products[idx]
            suggested.append(
                {
                    "productId": p.get("id"),
                    "name": p.get("name"),
                    "basePrice": p.get("basePrice"),
                    "mainImageUrl": None,
                }
            )
        if len(suggested) >= 3:
            break

    return {"reply": reply, "suggestedProducts": suggested}


def _row_distance(row: dict, height: float, weight: float) -> float:
    """Khoảng cách có trọng số giữa số đo khách và 1 dòng size chart.
    = 0 nếu height & weight đều nằm trong khoảng [min, max] của size đó.
    Height được trọng số cao hơn (0.6) vì quyết định form áo/quần nhiều hơn cân nặng."""
    min_h, max_h = float(row["minHeight"]), float(row["maxHeight"])
    min_w, max_w = float(row["minWeight"]), float(row["maxWeight"])

    h_dist = 0.0 if min_h <= height <= max_h else min(abs(height - min_h), abs(height - max_h))
    w_dist = 0.0 if min_w <= weight <= max_w else min(abs(weight - min_w), abs(weight - max_w))

    return 0.6 * h_dist + 0.4 * w_dist


def _distance_to_confidence(distance: float) -> float:
    """Distance = 0 (vừa khít) -> confidence ~0.97. Distance càng lớn -> confidence giảm dần,
    không bao giờ về dưới 0.3 (vẫn là 1 gợi ý hợp lý dù không hoàn hảo)."""
    raw = 0.97 * (2.718281828 ** (-distance / 15.0))
    return round(max(0.3, raw), 2)


@app.post("/size-recommend")
async def size_recommend(req: Request):
    body = await req.json()
    charts = body.get("sizeCharts") or []
    height = float(body.get("height") or 0)
    weight = float(body.get("weight") or 0)

    if not charts:
        # .NET chỉ quan tâm status code (không phải 2xx -> ném AppException 503),
        # nên trả 422 ở đây chỉ để log debug phía Python, .NET vẫn sẽ map về lỗi chung.
        return JSONResponse(status_code=422, content={"message": "Không có sizeCharts để gợi ý"})

    scored = sorted(
        ({**row, "_distance": _row_distance(row, height, weight)} for row in charts),
        key=lambda r: r["_distance"],
    )

    best = scored[0]
    alternatives = [
        {"size": r["size"], "confidence": _distance_to_confidence(r["_distance"])}
        for r in scored[1:3]
    ]

    if best["_distance"] == 0.0:
        explanation = f"Chiều cao {height:g}cm và cân nặng {weight:g}kg của bạn nằm vừa khít trong khoảng size {best['size']}."
    else:
        explanation = f"Size {best['size']} là lựa chọn gần nhất với số đo {height:g}cm / {weight:g}kg, dù không khớp tuyệt đối 100%."

    return {
        "recommendedSize": best["size"],
        "confidence": _distance_to_confidence(best["_distance"]),
        "explanation": explanation,
        "alternatives": alternatives,
    }


def _parse_date(date_str: str) -> "datetime.date":
    # .NET serialize DateTime dạng "2026-06-21T00:00:00" (có thể có hậu tố Z) -> chỉ cần lấy 10 ký tự đầu
    return datetime.strptime(date_str[:10], "%Y-%m-%d").date()


def _forecast_demand(history: list, horizon: int) -> list:
    """Linear trend qua least squares trên (ngày thứ i, quantitySold).
    Đơn giản hơn Prophet nhưng vẫn bắt được xu hướng tăng/giảm thật từ dữ liệu,
    thay vì chỉ random như bản mock cũ."""
    if not history:
        return []

    rows = sorted(history, key=lambda h: h["date"])
    quantities = [float(h.get("quantitySold", 0)) for h in rows]
    n = len(quantities)

    x_mean = (n - 1) / 2.0
    y_mean = sum(quantities) / n
    numerator = sum((i - x_mean) * (quantities[i] - y_mean) for i in range(n))
    denominator = sum((i - x_mean) ** 2 for i in range(n)) or 1.0
    slope = numerator / denominator
    intercept = y_mean - slope * x_mean

    last_date = _parse_date(rows[-1]["date"])
    points = []
    for i in range(1, horizon + 1):
        predicted = intercept + slope * (n - 1 + i)
        predicted = max(0.0, round(predicted, 1))  # không cho dự báo âm
        points.append({
            "date": (last_date + timedelta(days=i)).isoformat(),
            "predictedQuantitySold": predicted,
        })
    return points


@app.post("/forecast")
async def forecast(req: Request):
    body = await req.json()
    horizon = body.get("horizonDays") or 30
    history = body.get("history") or []

    points = _forecast_demand(history, horizon)

    return {
        "variantId": body.get("variantId"),
        "currentStock": 0,  # C# luôn ghi đè lại bằng tồn kho thật, giá trị này không được dùng
        "forecast": points,
        # 2 field dưới đây C# cũng luôn tự tính lại (Python không biết currentStock/minStock)
        # nên trả mặc định, không có ý nghĩa thực tế:
        "willRunOutInDays": None,
        "needReorder": False,
        "note": None,
    }