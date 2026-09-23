package licenta.facultate.alex.backendlicenta.model;

import java.util.List;

public record LessonRequestContext(LessonId lessonId, int questionCount, List<String> allowedShapeIds) {
    public LessonType lessonType() {
        return lessonId.type();
    }

    public String clientLessonId() {
        return lessonId.clientValue();
    }
}
