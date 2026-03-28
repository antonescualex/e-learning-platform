package licenta.facultate.alex.backendlicenta.service;

import licenta.facultate.alex.backendlicenta.dto.api.LessonContentDtos;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import licenta.facultate.alex.backendlicenta.model.LessonId;
import licenta.facultate.alex.backendlicenta.model.LessonRequestContext;
import licenta.facultate.alex.backendlicenta.model.LessonType;
import licenta.facultate.alex.backendlicenta.validation.LessonValidator;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Set;

@Service
public class LessonContentService {
    private final OpenAiResponsesClient openAiResponsesClient;
    private final LessonValidator validator;

    public LessonContentService(OpenAiResponsesClient openAiResponsesClient, LessonValidator validator) {
        this.openAiResponsesClient = openAiResponsesClient;
        this.validator = validator;
    }

    public LessonContentDtos.TextChoiceLessonResponse generateTextChoice(LessonContentDtos.LessonRequest request) {
        LessonRequestContext context = contextFor(request.lessonId(), request.questionCount(), List.of(), LessonType.TEXT_CHOICE);
        return openAiResponsesClient.generate(
                context,
                LessonContentDtos.TextChoiceLessonResponse.class,
                response -> validator.validateTextChoice(response, context.questionCount())
        );
    }

    public LessonContentDtos.ShapesLessonResponse generateShapes(LessonContentDtos.ShapesLessonRequest request) {
        LessonRequestContext context = contextFor(request.lessonId(), request.questionCount(), request.allowedShapeIds(), LessonType.SHAPES);
        Set<String> allowed = Set.copyOf(context.allowedShapeIds());
        return openAiResponsesClient.generate(
                context,
                LessonContentDtos.ShapesLessonResponse.class,
                response -> validator.validateShapes(response, context.questionCount(), allowed)
        );
    }

    public LessonContentDtos.ClockLessonResponse generateClock(LessonContentDtos.LessonRequest request) {
        LessonRequestContext context = contextFor(request.lessonId(), request.questionCount(), List.of(), LessonType.CLOCK);
        return openAiResponsesClient.generate(
                context,
                LessonContentDtos.ClockLessonResponse.class,
                response -> validator.validateClock(response, context.questionCount())
        );
    }

    public LessonContentDtos.SyllableDivisionLessonResponse generateSyllableDivision(LessonContentDtos.LessonRequest request) {
        LessonRequestContext context = contextFor(request.lessonId(), request.questionCount(), List.of(), LessonType.SYLLABLE_DIVISION);
        return openAiResponsesClient.generate(
                context,
                LessonContentDtos.SyllableDivisionLessonResponse.class,
                response -> validator.validateSyllableDivision(response, context.questionCount())
        );
    }

    public LessonContentDtos.WriteCorrectlyLessonResponse generateWriteCorrectly(LessonContentDtos.LessonRequest request) {
        LessonRequestContext context = contextFor(request.lessonId(), request.questionCount(), List.of(), LessonType.WRITE_CORRECTLY);
        return openAiResponsesClient.generate(
                context,
                LessonContentDtos.WriteCorrectlyLessonResponse.class,
                response -> validator.validateWriteCorrectly(response, context.questionCount())
        );
    }

    private LessonRequestContext contextFor(String lessonIdValue, int questionCount, List<String> rawShapeIds, LessonType expectedType) {
        LessonId lessonId = LessonId.fromClientValue(lessonIdValue);

        if (lessonId.type() == LessonType.READ_TOGETHER) {
            throw new ApiException(HttpStatus.NOT_IMPLEMENTED, "LESSON_NOT_IMPLEMENTED", "EnglishReadTogether is not implemented");
        }

        if (lessonId.type() != expectedType) {
            throw new ApiException(
                    HttpStatus.BAD_REQUEST,
                    "LESSON_ENDPOINT_MISMATCH",
                    "lessonId " + lessonIdValue + " does not belong to endpoint " + expectedType
            );
        }

        List<String> sanitizedShapeIds = expectedType == LessonType.SHAPES
                ? rawShapeIds.stream().map(String::trim).filter(s -> !s.isBlank()).distinct().toList()
                : List.of();

        if (expectedType == LessonType.SHAPES && sanitizedShapeIds.isEmpty()) {
            throw new ApiException(HttpStatus.BAD_REQUEST, "INVALID_ALLOWED_SHAPES", "AllowedShapeIds must contain at least one valid shape id");
        }

        return new LessonRequestContext(lessonId, questionCount, sanitizedShapeIds);
    }
}
