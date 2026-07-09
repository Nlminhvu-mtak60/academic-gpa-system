from typing import Dict, Any

class PredictionHelper:
    @staticmethod
    def calculate_required_final_score(
        attendance_score: float,
        continuous_score: float,
        target_score: float,
        lang: str = "vi"
    ) -> Dict[str, Any]:
        # Course score formula: Score = Attendance*0.1 + Continuous*0.3 + FinalExam*0.6
        # Required FinalExam = (Target - Attendance*0.1 - Continuous*0.3) / 0.6
        
        required_final = (target_score - (attendance_score * 0.1) - (continuous_score * 0.3)) / 0.6
        required_final = round(required_final, 2)
        
        # Round to nearest 0.5 as per specification requirement
        # E.g. 8.2 -> 8.0, 8.3 -> 8.5
        rounded_final = round(required_final * 2) / 2
        
        if required_final > 10.0:
            feasibility = "Impossible"
            if lang == "vi":
                advice = f"Mục tiêu đạt {target_score} điểm học phần là KHÔNG KHẢ THI. Điểm thi cuối kỳ yêu cầu ({required_final}) vượt quá điểm tối đa 10.0."
            else:
                advice = f"Targeting a course score of {target_score} is IMPOSSIBLE. The required final exam score ({required_final}) exceeds the maximum of 10.0."
        elif required_final <= 0.0:
            feasibility = "Achieved"
            if lang == "vi":
                advice = f"Mục tiêu đã ĐẠT ĐƯỢC. Với điểm quá trình hiện tại, bạn chắc chắn đạt {target_score} điểm học phần mà không phụ thuộc điểm thi."
            else:
                advice = f"Target already SECURED. With your current assessment scores, you will achieve at least {target_score} regardless of your final exam score."
        else:
            feasibility = "Achievable"
            if lang == "vi":
                advice = f"Để đạt điểm học phần {target_score}, bạn cần đạt tối thiểu {rounded_final} điểm trong bài thi kết thúc học phần."
            else:
                advice = f"To achieve a course score of {target_score}, you need a minimum of {rounded_final} on the final exam."
                
        return {
            "targetScoreThreshold": target_score,
            "requiredFinalExamScore": max(rounded_final, 0.0),
            "feasibility": feasibility,
            "advice": advice
        }
