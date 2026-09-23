package licenta.facultate.alex.backendlicenta.service;

import licenta.facultate.alex.backendlicenta.dto.api.LessonContentDtos;
import licenta.facultate.alex.backendlicenta.exception.ApiException;
import licenta.facultate.alex.backendlicenta.model.LessonId;
import licenta.facultate.alex.backendlicenta.model.LessonRequestContext;
import licenta.facultate.alex.backendlicenta.model.LessonType;
import licenta.facultate.alex.backendlicenta.validation.LessonValidator;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.http.HttpStatus;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
class LessonContentServiceTests {

    @Mock
    private OpenAiResponsesClient openAiResponsesClient;

    @Mock
    private LessonValidator validator;

    @Test
    void generateReadTogetherRoutesReadTogetherLessonsThroughOpenAi() {
        LessonContentService service = new LessonContentService(openAiResponsesClient, validator);
        LessonContentDtos.ReadTogetherLessonResponse response = new LessonContentDtos.ReadTogetherLessonResponse(List.of(
                new LessonContentDtos.ReadTogetherQuestionDto("The cat runs fast.")
        ));
        when(openAiResponsesClient.generate(any(), eq(LessonContentDtos.ReadTogetherLessonResponse.class), any())).thenReturn(response);

        LessonContentDtos.ReadTogetherLessonResponse actual = service.generateReadTogether(
                new LessonContentDtos.LessonRequest("EnglishReadTogether", 1)
        );

        ArgumentCaptor<LessonRequestContext> contextCaptor = ArgumentCaptor.forClass(LessonRequestContext.class);
        verify(openAiResponsesClient).generate(contextCaptor.capture(), eq(LessonContentDtos.ReadTogetherLessonResponse.class), any());

        assertThat(actual).isSameAs(response);
        assertThat(contextCaptor.getValue().lessonId()).isEqualTo(LessonId.ENGLISH_READ_TOGETHER);
        assertThat(contextCaptor.getValue().lessonType()).isEqualTo(LessonType.READ_TOGETHER);
    }

    @Test
    void generateReadTogetherRejectsLessonIdsFromOtherEndpoints() {
        LessonContentService service = new LessonContentService(openAiResponsesClient, validator);

        assertThatThrownBy(() -> service.generateReadTogether(new LessonContentDtos.LessonRequest("EnglishWriteCorrectly", 1)))
                .isInstanceOf(ApiException.class)
                .satisfies(ex -> {
                    ApiException apiException = (ApiException) ex;
                    assertThat(apiException.status()).isEqualTo(HttpStatus.BAD_REQUEST);
                    assertThat(apiException.code()).isEqualTo("LESSON_ENDPOINT_MISMATCH");
                });
    }
}
