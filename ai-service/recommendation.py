from typing import List, Dict, Any

class RecommendationEngine:
    @staticmethod
    def get_recommendations(
        weak_courses: List[Dict[str, Any]], 
        current_gpa: float,
        remaining_credits: int,
        lang: str = "vi"
    ) -> List[str]:
        recommendations = []

        # 1. Subjects that need improvement
        if weak_courses:
            top_weak = weak_courses[0]
            name = top_weak.get("courseName", "")
            code = top_weak.get("courseCode", "")
            disp = f"{code} - {name}" if code else name
            
            if lang == "vi":
                recommendations.append(f"Tập trung cải thiện điểm số cho các môn học đang yếu, đặc biệt là môn '{disp}'.")
            else:
                recommendations.append(f"Focus on improving scores in weaker subjects, especially '{disp}'.")
        
        # 2. Suggested study priorities
        if current_gpa < 7.0:
            if lang == "vi":
                recommendations.append("Tăng cường thời gian tự học và chủ động tham gia các nhóm học tập để củng cố kiến thức nền tảng.")
                recommendations.append("Xem xét đăng ký học cải thiện các môn học có điểm số dưới D+ hoặc F để kéo lại GPA.")
            else:
                recommendations.append("Increase self-study hours and join study groups to reinforce fundamental knowledge.")
                recommendations.append("Consider retaking courses with grades lower than D+ or F to boost your GPA.")
        else:
            if lang == "vi":
                recommendations.append("Duy trì nhịp độ học tập hiện tại. Hãy nghiên cứu thêm tài liệu chuyên ngành để đạt kết quả xuất sắc (A/A+).")
            else:
                recommendations.append("Maintain your current study pace. Explore extra academic materials to target top grades (A/A+).")

        # 3. Future semester planning
        if remaining_credits > 60:
            if lang == "vi":
                recommendations.append("Lên kế hoạch đăng ký môn học cân bằng giữa các môn chuyên ngành khó và các môn đại cương/tự chọn nhẹ nhàng hơn.")
            else:
                recommendations.append("Balance your future semesters by combining difficult core major requirements with lighter electives.")
        else:
            if lang == "vi":
                recommendations.append("Bạn đang ở giai đoạn cuối. Hãy chú trọng hoàn thành tốt khóa luận tốt nghiệp hoặc các học phần thực tập.")
            else:
                recommendations.append("You are in the final stages. Prioritize performing well in your graduation thesis or internships.")
                
        return recommendations
