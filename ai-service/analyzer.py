from typing import List, Dict, Any

class AcademicAnalyzer:
    @staticmethod
    def analyze_gpa_trend(gpa_trend: List[Dict[str, Any]], lang: str = "vi") -> Dict[str, Any]:
        if not gpa_trend:
            return {
                "trend": "Stable",
                "message": "Chưa có dữ liệu lịch sử GPA." if lang == "vi" else "No historical GPA data available."
            }
        
        # Sort trend if needed, but assume chronological order
        gpas = [item.get("gpa", 0.0) for item in gpa_trend]
        
        if len(gpas) < 2:
            return {
                "trend": "Stable",
                "message": "Cần thêm dữ liệu học kỳ để phân tích xu hướng." if lang == "vi" else "Need more semester data to analyze trends."
            }
        
        diff = gpas[-1] - gpas[0]
        if diff > 0.3:
            trend = "Improving"
            message = "GPA của bạn đang có xu hướng cải thiện rõ rệt qua các học kỳ." if lang == "vi" else "Your GPA shows a clear upward trend across semesters."
        elif diff < -0.3:
            trend = "Declining"
            message = "GPA của bạn đang có xu hướng giảm sút. Hãy xem xét lại phương pháp học tập." if lang == "vi" else "Your GPA shows a downward trend. Consider adjusting your study methods."
        else:
            trend = "Stable"
            message = "GPA của bạn tương đối ổn định qua các học kỳ." if lang == "vi" else "Your GPA remains relatively stable across semesters."
            
        return {"trend": trend, "message": message}

    @staticmethod
    def analyze_strengths_and_weaknesses(weak_courses: List[Dict[str, Any]], lang: str = "vi") -> Dict[str, Any]:
        strengths = []
        weaknesses = []
        
        for course in weak_courses:
            score = course.get("score", 0.0)
            code = course.get("courseCode", "")
            name = course.get("courseName", "")
            disp_name = f"{code} - {name}" if code else name
            
            # Since these are loaded as "weakCourses" context, but some might actually have okay scores,
            # we classify them. Generally if they are in this list they need improvement.
            if score >= 8.0:
                strengths.append(disp_name)
            else:
                weaknesses.append(f"{disp_name} ({score}/10)")
                
        return {
            "strengths": strengths,
            "weaknesses": weaknesses
        }

    @staticmethod
    def analyze_credit_progress(completed: int, required: int, lang: str = "vi") -> Dict[str, Any]:
        required = max(required, 1)
        percent = min(round((completed / required) * 100, 2), 100.0)
        remaining = max(required - completed, 0)
        
        if percent >= 90:
            status = "Sắp tốt nghiệp" if lang == "vi" else "Near Graduation"
        elif percent >= 50:
            status = "Đã hoàn thành hơn một nửa chương trình" if lang == "vi" else "Halfway through program"
        else:
            status = "Giai đoạn đầu chương trình" if lang == "vi" else "Early stage of program"
            
        return {
            "percent": percent,
            "remainingCredits": remaining,
            "status": status
        }

    @staticmethod
    def analyze_learning_pattern(gpa_trend: List[Dict[str, Any]], weak_courses: List[Dict[str, Any]], lang: str = "vi") -> str:
        # Generate a general rule-based learning pattern comment
        if len(weak_courses) > 2:
            return (
                "Bạn đang gặp khó khăn ở một số học phần chuyên ngành. Hãy phân bố thời gian tự học nhiều hơn." 
                if lang == "vi" else 
                "You are facing difficulties in multiple courses. Try allocating more self-study hours."
            )
        elif len(gpa_trend) >= 2 and gpa_trend[-1].get("gpa", 0.0) > 8.0:
            return (
                "Bạn có khả năng tự tiếp thu rất tốt và duy trì phong độ cao ở các học kỳ gần đây."
                if lang == "vi" else
                "You have excellent self-learning capabilities and have maintained high performance in recent semesters."
            )
        else:
            return (
                "Nhịp độ học tập của bạn ở mức trung bình. Tập trung ôn tập trước các kỳ thi sẽ giúp đạt kết quả tốt hơn."
                if lang == "vi" else
                "Your learning pace is moderate. Concentrating on prep before exams will yield better scores."
            )
