package licenta.facultate.alex.backendlicenta.controller;

import licenta.facultate.alex.backendlicenta.dto.api.LessonContentDtos;
import licenta.facultate.alex.backendlicenta.exception.GlobalExceptionHandler;
import licenta.facultate.alex.backendlicenta.service.LessonContentService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import java.util.List;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

class LessonContentControllerTests {

    private final LessonContentService lessonContentService = mock(LessonContentService.class);
    private MockMvc mockMvc;

    @BeforeEach
    void setUp() {
        mockMvc = MockMvcBuilders.standaloneSetup(new LessonContentController(lessonContentService))
                .setControllerAdvice(new GlobalExceptionHandler())
                .build();
    }

    @Test
    void readTogetherEndpointReturnsReadTogetherQuestions() throws Exception {
        LessonContentDtos.ReadTogetherLessonResponse response = new LessonContentDtos.ReadTogetherLessonResponse(List.of(
                new LessonContentDtos.ReadTogetherQuestionDto("The cat runs fast."),
                new LessonContentDtos.ReadTogetherQuestionDto("We play at home.")
        ));
        when(lessonContentService.generateReadTogether(any())).thenReturn(response);

        mockMvc.perform(post("/api/lesson-content/read-together")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("""
                                {
                                  "LessonId": "EnglishReadTogether",
                                  "QuestionCount": 2
                                }
                                """))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.Questions[0].PassageText").value("The cat runs fast."))
                .andExpect(jsonPath("$.Questions[1].PassageText").value("We play at home."));
    }
}
