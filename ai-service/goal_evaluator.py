from typing import Dict, Any

class GoalEvaluationEngine:
    @staticmethod
    def evaluate_goal(
        current_gpa: float,
        completed_credits: int,
        total_required_credits: int,
        target_gpa: float,
        is_scale_4: bool = False,
        lang: str = "vi"
    ) -> Dict[str, Any]:
        max_scale = 4.0 if is_scale_4 else 10.0
        
        # Calculate remaining credits
        remaining_credits = max(total_required_credits - completed_credits, 0)
        
        if remaining_credits == 0:
            # No credits remaining, goal is evaluated solely on current GPA
            achieved = current_gpa >= target_gpa
            return {
                "requiredRemainingGpa": 0.0,
                "feasibility": "Realistic" if achieved else "Impossible",
                "advice": (
                    "Bạn đã hoàn thành chương trình. Mục tiêu đạt được!" if achieved
                    else "Bạn đã hoàn thành chương trình nhưng chưa đạt mục tiêu."
                ) if lang == "vi" else (
                    "Program completed. Goal achieved!" if achieved
                    else "Program completed but target was not reached."
                ),
                "alternativeTarget": current_gpa
            }
            
        # Formula: R = (Target * Total - Current * Completed) / Remaining
        total_credits = completed_credits + remaining_credits
        required_gpa = ((target_gpa * total_credits) - (current_gpa * completed_credits)) / remaining_credits
        required_gpa = round(required_gpa, 2)
        
        # Max achievable GPA if scoring perfect in all remaining credits
        max_achievable = ((max_scale * remaining_credits) + (current_gpa * completed_credits)) / total_credits
        max_achievable = round(max_achievable, 2)

        if required_gpa > max_scale:
            feasibility = "Impossible"
            alternative = max_achievable
            if lang == "vi":
                advice = f"Mục tiêu này không khả thi vì điểm số yêu cầu cho phần còn lại ({required_gpa}) vượt quá thang điểm {max_scale}. GPA tối đa bạn có thể đạt được là {max_achievable}."
            else:
                advice = f"This goal is not achievable because the required remaining GPA ({required_gpa}) exceeds the scale maximum of {max_scale}. The maximum GPA you can reach is {max_achievable}."
        elif required_gpa <= 0.0:
            feasibility = "Achieved"
            alternative = target_gpa
            if lang == "vi":
                advice = "Bạn đã đạt được mục tiêu này rồi! Hãy duy trì kết quả hiện tại."
            else:
                advice = "You have already secured this goal! Keep maintaining your current performance."
        else:
            feasibility = "Realistic"
            alternative = target_gpa
            if lang == "vi":
                advice = f"Mục tiêu khả thi. Bạn cần đạt GPA trung bình là {required_gpa} cho {remaining_credits} tín chỉ còn lại."
            else:
                advice = f"This goal is achievable. You need to average {required_gpa} GPA across your remaining {remaining_credits} credits."

        return {
            "requiredRemainingGpa": max(required_gpa, 0.0),
            "feasibility": feasibility,
            "advice": advice,
            "alternativeTarget": alternative
        }
