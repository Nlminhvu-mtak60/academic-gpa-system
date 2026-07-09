import re

class IntentClassifier:
    @staticmethod
    def classify(message: str) -> str:
        msg = message.lower().strip()
        
        # 1. Final Exam Prediction Indicators
        # Keywords relating to final exams, target course grades, scores required to get A/B/excellence
        final_exam_indicators = [
            r"điểm thi", r"cuối kỳ", r"cuối khoá", r"thi cuối", r"thi được", r"thi bao nhiêu",
            r"final exam", r"score.*final", r"need on.*final", r"score on.*final",
            r"get an a", r"get a b", r"đạt điểm a", r"đạt điểm b", r"đạt điểm tối đa",
            r"score.*excellence", r"score.*good", r"score.*excellent", r"thi được mấy",
            r"được điểm a", r"được điểm b", r"được a không", r"can i still get an a"
        ]
        if any(re.search(pattern, msg) for pattern in final_exam_indicators):
            return "FinalExamPrediction"
            
        # 2. Goal Planning Indicators
        # Target GPA, graduation honors/distinction, semester target requirements
        goal_indicators = [
            r"mục tiêu", r"target", r"đạt gpa", r"đạt được gpa", r"gpa bao nhiêu để",
            r"graduat.*distinction", r"tốt nghiệp loại", r"bằng giỏi", r"bằng khá", r"xuất sắc",
            r"tốt nghiệp xuất sắc", r"points do i need", r"gpa.*graduate", r"graduate with",
            r"gpa target", r"how many points", r"cần bao nhiêu điểm để", r"cần bao nhiêu tín chỉ để",
            r"đạt bằng giỏi", r"đạt bằng khá", r"đạt bằng xuất sắc", r"đạt gpa.*", r"gpa \d\.\d",
            r"gpa \d+"
        ]
        if any(re.search(pattern, msg) for pattern in goal_indicators):
            return "GoalPlanning"
            
        # 3. Personal GPA/Semester Analysis Indicators
        # Analyze current GPA, semester review, study advice
        analysis_indicators = [
            r"phân tích", r"đánh giá", r"lời khuyên", r"advice", r"học tập", r"cố vấn",
            r"tình hình", r"nhận xét", r"semester", r"học kỳ", r"gpa của tôi", r"my gpa",
            r"my semester", r"analyze my", r"study advice", r"cải thiện gpa", r"improve gpa"
        ]
        if any(re.search(pattern, msg) for pattern in analysis_indicators):
            return "PersonalAnalysis"
            
        # 4. Academic Explanations/Questions Indicators
        # General definitions of GPA, graduation requirements, calculation logic
        academic_indicators = [
            r"là gì", r"what is", r"how is", r"cách tính", r"tính gpa", r"tính điểm",
            r"how.*calculated", r"definition", r"định nghĩa", r"thang điểm", r"grade scale",
            r"credit", r"tín chỉ", r"gpa là gì", r"tính điểm trung bình"
        ]
        if any(re.search(pattern, msg) for pattern in academic_indicators):
            return "AcademicQuestion"
            
        # 5. General Chat / Greeting Indicators
        # Hello, how are you, who are you, greetings
        chat_indicators = [
            r"hello", r"hi", r"chào", r"xin chào", r"khoẻ không", r"bạn là ai",
            r"who are you", r"how are you", r"gặp bạn", r"nice to meet", r"chào bạn"
        ]
        if any(re.search(pattern, msg) for pattern in chat_indicators) or len(msg.split()) <= 2:
            return "GeneralChat"
            
        # Default fallback
        return "GeneralChat"
