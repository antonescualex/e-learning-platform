package licenta.facultate.alex.backendlicenta.validation;

import licenta.facultate.alex.backendlicenta.dto.api.LessonContentDtos;
import org.junit.jupiter.api.Test;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;

class LessonValidatorTests {
    private final LessonValidator validator = new LessonValidator();

    @Test
    void validateReadTogetherAcceptsShortDistinctPassages() {
        LessonContentDtos.ReadTogetherLessonResponse response = new LessonContentDtos.ReadTogetherLessonResponse(List.of(
                new LessonContentDtos.ReadTogetherQuestionDto("The cat runs fast."),
                new LessonContentDtos.ReadTogetherQuestionDto("Birds sing. We smile.")
        ));

        List<String> errors = validator.validateReadTogether(response, 2);

        assertThat(errors).isEmpty();
    }

    @Test
    void validateReadTogetherRejectsUnsupportedPassages() {
        LessonContentDtos.ReadTogetherLessonResponse response = new LessonContentDtos.ReadTogetherLessonResponse(List.of(
                new LessonContentDtos.ReadTogetherQuestionDto("I have 2 cats."),
                new LessonContentDtos.ReadTogetherQuestionDto("Tiny."),
                new LessonContentDtos.ReadTogetherQuestionDto("We play at home."),
                new LessonContentDtos.ReadTogetherQuestionDto("we play at home")
        ));

        List<String> errors = validator.validateReadTogether(response, 4);

        assertThat(errors).contains("Question 0: PassageText must use only letters, spaces, and periods");
        assertThat(errors).contains("Question 1: PassageText must contain 4 to 10 words");
        assertThat(errors).contains("Question 3: PassageText must be unique");
    }
}
