package licenta.facultate.alex.backendlicenta.dto.api;

import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;

public final class LessonProgressDtos {
    private LessonProgressDtos() {
    }

    public record CompleteLessonRequest(
            @JsonProperty("LessonId")
            @NotBlank
            String lessonId,

            @JsonProperty("Completed")
            boolean completed,

            @JsonProperty("CorrectAnswersCount")
            @Min(0)
            int correctAnswersCount,

            @JsonProperty("QuestionCount")
            @Min(1)
            @Max(100)
            int questionCount,

            @JsonProperty("AwardedExperience")
            int awardedExperience,

            @JsonProperty("AwardedCoins")
            int awardedCoins,

            @JsonProperty("ElapsedSeconds")
            @Min(0)
            @Max(86400)
            int elapsedSeconds
    ) {
    }
}
