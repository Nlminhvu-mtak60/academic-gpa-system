import os
from typing import List, Dict, Any
from fastapi import FastAPI, Header, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

# Import rule-based engines
from analyzer import AcademicAnalyzer
from recommendation import RecommendationEngine
from goal_evaluator import GoalEvaluationEngine
from prediction import PredictionHelper
from intent_classifier import IntentClassifier

app = FastAPI(
    title="Academic GPA AI Advisor Service",
    description="Python FastAPI gateway for GenAI processing and academic advice.",
    version="1.0.0"
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Shared Secret Key Authentication
SHARED_API_KEY = os.getenv("AI_SERVICE_API_KEY", "SuperSecretAiAdvisorApiKey123!")

async def verify_api_key(x_api_key: str = Header(None)):
    if x_api_key != SHARED_API_KEY:
        raise HTTPException(status_code=401, detail="Unauthorized: Invalid API Key")

# Pydantic Schemas
class StatusResponse(BaseModel):
    status: str
    service: str

class GpaTrendItem(BaseModel):
    semesterName: str
    gpa: float

class CourseItem(BaseModel):
    courseCode: str
    courseName: str
    score: float

class AcademicContext(BaseModel):
    currentCumulativeGpa: float
    totalCreditsCompleted: int
    totalCreditsRequired: int
    gpaTrend: List[GpaTrendItem]
    weakCourses: List[CourseItem]

class ChatMessage(BaseModel):
    role: str
    content: str

class ChatRequest(BaseModel):
    message: str
    preferredLanguage: str = "vi"
    academicContext: AcademicContext
    chatHistory: List[ChatMessage] = []

class ChatResponse(BaseModel):
    response: str
    tokensUsed: int
    provider: str

class PredictRequest(BaseModel):
    attendanceScore: float
    continuousScore: float
    targetGrade: str

class PredictResponse(BaseModel):
    targetScoreThreshold: float
    requiredFinalExamScore: float
    feasibility: str
    advice: str

def map_grade_to_score(grade: str) -> float:
    try:
        return float(grade)
    except ValueError:
        pass
    
    mapping = {
        "A+": 9.0,
        "A": 8.5,
        "B+": 8.0,
        "B": 7.0,
        "C+": 6.5,
        "C": 5.5,
        "D+": 5.0,
        "D": 4.0,
        "F": 0.0
    }
    return mapping.get(grade.upper().strip(), 8.0)

# Endpoints
@app.get("/health", response_model=StatusResponse)
def health_check():
    return StatusResponse(status="healthy", service="AI Advisor")

@app.get("/")
def read_root():
    return {"message": "Academic GPA AI Advisor Service is running."}

@app.post("/ai/advisor/chat", response_model=ChatResponse, dependencies=[Depends(verify_api_key)])
def advisor_chat(request: ChatRequest):
    lang = request.preferredLanguage
    ctx = request.academicContext
    query = request.message
    
    # 1. Classify Intent
    intent = IntentClassifier.classify(query)
    
    # 2. Process based on classified intent
    if intent == "GeneralChat":
        if lang == "vi":
            response_text = (
                "Xin chào! Tôi là Cố vấn học tập AI của bạn. "
                "Tôi có thể giúp bạn phân tích kết quả GPA, lên kế hoạch mục tiêu học tập, "
                "hoặc dự đoán điểm thi cuối kỳ. Bạn muốn bắt đầu từ đâu?"
            )
        else:
            response_text = (
                "Hello! I am your AI Academic Advisor. "
                "I can help you analyze your GPA trend, plan academic goals, "
                "or predict required final exam scores. How can I help you today?"
            )
            
    elif intent == "AcademicQuestion":
        if lang == "vi":
            response_text = (
                "### Cách Tính GPA & Quy Định Đào Tạo\n\n"
                "1. **Công thức điểm học phần**:\n"
                "   $$\\text{Điểm môn} = \\text{Chuyên cần} \\times 0.1 + \\text{Quá trình} \\times 0.3 + \\text{Thi cuối kỳ} \\times 0.6$$\n"
                "   *Tất cả các điểm thành phần được làm tròn về bước 0.5 trước khi tính toán.*\n\n"
                "2. **Quy đổi thang điểm 4 & Chữ**:\n"
                "   - Từ 9.0 đến 10.0: **A+** (4.0)\n"
                "   - Từ 8.5 đến 8.9: **A** (3.8)\n"
                "   - Từ 8.0 đến 8.4: **B+** (3.5)\n"
                "   - Từ 7.0 đến 7.9: **B** (3.0)\n"
                "   - Từ 6.5 đến 6.9: **C+** (2.5)\n"
                "   - Từ 5.5 đến 6.4: **C** (2.0)\n"
                "   - Từ 5.0 đến 5.4: **D+** (1.5)\n"
                "   - Từ 4.0 đến 4.9: **D** (1.0)\n"
                "   - Dưới 4.0: **F** (0.0 - Trượt môn)\n\n"
                "3. **Xếp loại học lực**:\n"
                "   - **Xuất sắc**: GPA từ 9.0 trở lên\n"
                "   - **Giỏi**: GPA từ 8.0 đến cận 9.0\n"
                "   - **Khá**: GPA từ 7.0 đến cận 8.0\n"
                "   - **Trung bình khá**: GPA từ 6.5 đến cận 7.0\n"
                "   - **Trung bình**: GPA từ 5.0 đến cận 6.5\n"
                "   - **Yếu / Kém**: Dưới 5.0"
            )
        else:
            response_text = (
                "### GPA Calculation & Mappings\n\n"
                "1. **Course Score Formula**:\n"
                "   $$\\text{Course Score} = \\text{Attendance} \\times 0.1 + \\text{Continuous} \\times 0.3 + \\text{Final Exam} \\times 0.6$$\n"
                "   *Component scores are rounded to the nearest 0.5 prior to calculations.*\n\n"
                "2. **Grade Mapping & GPA Scale**:\n"
                "   - 9.0 - 10.0: **A+** (4.0)\n"
                "   - 8.5 - 8.9: **A** (3.8)\n"
                "   - 8.0 - 8.4: **B+** (3.5)\n"
                "   - 7.0 - 7.9: **B** (3.0)\n"
                "   - 6.5 - 6.9: **C+** (2.5)\n"
                "   - 5.5 - 6.4: **C** (2.0)\n"
                "   - 5.0 - 5.4: **D+** (1.5)\n"
                "   - 4.0 - 4.9: **D** (1.0)\n"
                "   - Under 4.0: **F** (0.0 - Failed)\n\n"
                "3. **Academic Classification**:\n"
                "   - **Excellent**: GPA >= 9.0\n"
                "   - **Good**: 8.0 <= GPA < 9.0\n"
                "   - **Fair**: 7.0 <= GPA < 8.0\n"
                "   - **Average Fair**: 6.5 <= GPA < 7.0\n"
                "   - **Average**: 5.0 <= GPA < 6.5\n"
                "   - **Weak / Poor**: GPA < 5.0"
            )
            
    elif intent == "GoalPlanning":
        gpa_match = [word for word in query.lower().split() if any(char.isdigit() for char in word)]
        target_val = None
        if gpa_match:
            try:
                target_val = float(gpa_match[0].replace(",", "."))
            except ValueError:
                pass
        
        if not target_val:
            target_val = 8.0 if ctx.currentCumulativeGpa > 4.0 else 3.2
            
        is_scale_4 = target_val <= 4.0
        evaluation = GoalEvaluationEngine.evaluate_goal(
            ctx.currentCumulativeGpa,
            ctx.totalCreditsCompleted,
            ctx.totalCreditsRequired,
            target_val,
            is_scale_4,
            lang
        )
        
        if lang == "vi":
            response_text = (
                f"### Phân Tích Kế Hoạch Mục Tiêu (Mục tiêu GPA: {target_val})\n\n"
                f"- **Khả thi**: {evaluation['feasibility']}\n"
                f"- **Đánh giá & Khuyên nghị**: {evaluation['advice']}\n"
                f"- **Điểm đề xuất thay thế**: {evaluation['alternativeTarget']}/10\n\n"
                f"Bạn có thể thiết lập thêm các mục tiêu học tập chính thức trong mục 'Kế hoạch học tập'."
            )
        else:
            response_text = (
                f"### GPA Goal Target Evaluation (Target: {target_val})\n\n"
                f"- **Feasibility**: {evaluation['feasibility']}\n"
                f"- **Analysis & Feedback**: {evaluation['advice']}\n"
                f"- **Alternative Recommended Target**: {evaluation['alternativeTarget']}\n\n"
                f"You can set active goals via the 'Goal Planner' page."
            )
            
    elif intent == "FinalExamPrediction":
        gpa_match = [word for word in query.lower().split() if any(char.isdigit() for char in word)]
        target_score = 8.0
        if gpa_match:
            try:
                target_score = float(gpa_match[0].replace(",", "."))
            except ValueError:
                pass
        
        result = PredictionHelper.calculate_required_final_score(8.0, 8.0, target_score, lang)
        
        if lang == "vi":
            response_text = (
                f"### Dự Đoán Điểm Thi Kết Thúc Học Phần (Mục tiêu: {target_score}/10)\n\n"
                f"- *Giả định*: Điểm chuyên cần = 8.0 và Điểm quá trình = 8.0\n"
                f"- **Khả năng đạt**: {result['feasibility']}\n"
                f"- **Khuyên nghị**: {result['advice']}\n\n"
                f"*Lưu ý: Bạn có thể nhập điểm chi tiết từng môn trong tab 'Bảng điểm' để nhận kết quả chính xác nhất.*"
            )
        else:
            response_text = (
                f"### Final Exam Score Prediction (Target Course Score: {target_score})\n\n"
                f"- *Assumption*: Attendance score = 8.0 and Continuous assessment = 8.0\n"
                f"- **Feasibility**: {result['feasibility']}\n"
                f"- **Advice**: {result['advice']}\n\n"
                f"*Note: You can perform precise calculations for individual courses on the 'Course Grade' detail views.*"
            )
            
    else:  # PersonalAnalysis / Fallback
        trend_analysis = AcademicAnalyzer.analyze_gpa_trend(
            [{"semesterName": item.semesterName, "gpa": item.gpa} for item in ctx.gpaTrend],
            lang
        )
        sw_analysis = AcademicAnalyzer.analyze_strengths_and_weaknesses(
            [{"courseCode": item.courseCode, "courseName": item.courseName, "score": item.score} for item in ctx.weakCourses],
            lang
        )
        credit_analysis = AcademicAnalyzer.analyze_credit_progress(
            ctx.totalCreditsCompleted,
            ctx.totalCreditsRequired,
            lang
        )
        learning_pattern = AcademicAnalyzer.analyze_learning_pattern(
            [{"semesterName": item.semesterName, "gpa": item.gpa} for item in ctx.gpaTrend],
            [{"courseCode": item.courseCode, "courseName": item.courseName, "score": item.score} for item in ctx.weakCourses],
            lang
        )
        recs = RecommendationEngine.get_recommendations(
            [{"courseCode": item.courseCode, "courseName": item.courseName, "score": item.score} for item in ctx.weakCourses],
            ctx.currentCumulativeGpa,
            credit_analysis["remainingCredits"],
            lang
        )
        
        if lang == "vi":
            response_text = f"Dựa trên hồ sơ học tập của bạn, sau đây là phân tích chi tiết:\n\n"
            response_text += f"### 1. Phân Tích Kết Quả GPA\n"
            response_text += f"- **Điểm tích lũy hiện tại**: **{ctx.currentCumulativeGpa}/10**\n"
            response_text += f"- **Xu hướng điểm**: {trend_analysis['message']}\n"
            response_text += f"- **Tiến độ tín chỉ**: Bạn đã hoàn thành {ctx.totalCreditsCompleted}/{ctx.totalCreditsRequired} tín chỉ ({credit_analysis['percent']}%). Trạng thái: {credit_analysis['status']}.\n\n"
            
            if sw_analysis["weaknesses"]:
                response_text += f"### 2. Môn Học Cần Cải Thiện\n"
                for w in sw_analysis["weaknesses"]:
                    response_text += f"- {w}\n"
                response_text += "\n"
                
            response_text += f"### 3. Nhận Xét & Đề Xuất Học Tập\n"
            response_text += f"- *Đánh giá*: {learning_pattern}\n"
            for r in recs:
                response_text += f"- {r}\n"
            response_text += f"\n*Lưu ý: Các phân tích và dự báo này chỉ mang tính chất tham khảo học tập.*"
        else:
            response_text = f"Based on your academic profile, here is a detailed analysis:\n\n"
            response_text += f"### 1. GPA Performance Analysis\n"
            response_text += f"- **Current Cumulative GPA**: **{ctx.currentCumulativeGpa}/10**\n"
            response_text += f"- **GPA Trend**: {trend_analysis['message']}\n"
            response_text += f"- **Credit Progress**: Completed {ctx.totalCreditsCompleted}/{ctx.totalCreditsRequired} credits ({credit_analysis['percent']}%). Status: {credit_analysis['status']}.\n\n"
            
            if sw_analysis["weaknesses"]:
                response_text += f"### 2. Areas for Improvement\n"
                for w in sw_analysis["weaknesses"]:
                    response_text += "- " + w + "\n"
                response_text += "\n"
                
            response_text += f"### 3. Study Recommendations & Priorities\n"
            response_text += f"- *Observation*: {learning_pattern}\n"
            for r in recs:
                response_text += f"- {r}\n"
            response_text += f"\n*Note: All analyses and forecasts are advisory estimates only.*"

    tokens = len(response_text) // 3 + len(query) // 3
    return ChatResponse(
        response=response_text,
        tokensUsed=max(tokens, 50),
        provider="RuleEngine-V2"
    )

@app.post("/ai/predict/final-score", response_model=PredictResponse, dependencies=[Depends(verify_api_key)])
def predict_final_score(request: PredictRequest):
    # Map target grade
    target_score = map_grade_to_score(request.targetGrade)
    
    result = PredictionHelper.calculate_required_final_score(
        request.attendanceScore,
        request.continuousScore,
        target_score
    )
    
    return PredictResponse(
        targetScoreThreshold=result["targetScoreThreshold"],
        requiredFinalExamScore=result["requiredFinalExamScore"],
        feasibility=result["feasibility"],
        advice=result["advice"]
    )
