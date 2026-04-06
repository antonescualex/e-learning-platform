package licenta.facultate.alex.backendlicenta.dto.api;

import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotEmpty;
import jakarta.validation.constraints.Pattern;

import java.util.List;

public final class LessonContentDtos {
    private LessonContentDtos() {
    }

    public record LessonRequest(
            @JsonProperty("LessonId") @NotBlank String lessonId,
            @JsonProperty("QuestionCount") @Min(1) @Max(10) int questionCount
    ) {
    }

    public record ShapesLessonRequest(
            @JsonProperty("LessonId") @NotBlank String lessonId,
            @JsonProperty("QuestionCount") @Min(1) @Max(10) int questionCount,
            @JsonProperty("AllowedShapeIds") @NotEmpty List<@Pattern(regexp = "^[A-Za-z0-9_-]{1,40}$") String> allowedShapeIds
    ) {
    }

    public record TextChoiceLessonResponse(@JsonProperty("Questions") List<TextChoiceQuestionDto> questions) {
    }

    public record ShapesLessonResponse(@JsonProperty("Questions") List<ShapeQuestionDto> questions) {
    }

    public record ClockLessonResponse(@JsonProperty("Questions") List<ClockQuestionDto> questions) {
    }

    public record SyllableDivisionLessonResponse(@JsonProperty("Questions") List<SyllableDivisionQuestionDto> questions) {
    }

    public record WriteCorrectlyLessonResponse(@JsonProperty("Questions") List<WriteCorrectlyQuestionDto> questions) {
    }

    public record ReadTogetherLessonResponse(@JsonProperty("Questions") List<ReadTogetherQuestionDto> questions) {
    }

    public record TextChoiceQuestionDto(
            @JsonProperty("QuestionText") String questionText,
            @JsonProperty("Answers") List<String> answers,
            @JsonProperty("CorrectAnswerIndex") int correctAnswerIndex
    ) {
    }

    public record ShapeQuestionDto(
            @JsonProperty("ShapeId") String shapeId,
            @JsonProperty("Answers") List<String> answers,
            @JsonProperty("CorrectAnswerIndex") int correctAnswerIndex
    ) {
    }

    public record ClockQuestionDto(
            @JsonProperty("Hour") int hour,
            @JsonProperty("Minute") int minute,
            @JsonProperty("Answers") List<String> answers,
            @JsonProperty("CorrectAnswerIndex") int correctAnswerIndex
    ) {
    }

    public record SyllableDivisionQuestionDto(
            @JsonProperty("PromptText") String promptText,
            @JsonProperty("ExpectedAnswer") String expectedAnswer
    ) {
    }

    public record WriteCorrectlyQuestionDto(
            @JsonProperty("SentenceText") String sentenceText,
            @JsonProperty("ExpectedAnswer") String expectedAnswer
    ) {
    }

    public record ReadTogetherQuestionDto(
            @JsonProperty("PassageText") String passageText
    ) {
    }

    public record ApiErrorResponse(
            @JsonProperty("Code") String code,
            @JsonProperty("Message") String message,
            @JsonProperty("Details") List<String> details,
            @JsonProperty("CorrelationId") String correlationId
    ) {
    }
}
